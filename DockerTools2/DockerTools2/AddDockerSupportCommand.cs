using DockerTools2.Shared;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.ProjectSystem.DotNet;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DockerTools2
{
    internal sealed class AddDockerSupportCommand
    {
        /// <summary>
        /// Command ID for Context Menu Button.
        /// </summary>
        private const int AddDockerSupportContextMenuCommandId = 0x0010;

        /// <summary>
        /// Command ID for Project Options Button.
        /// </summary>
        private const int AddDockerSupportOptionsCommandId = 0x0011;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        private readonly Guid CommandSet = new Guid("fc3a0c11-8c3b-4bd8-9671-2eb7e37de325");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        [Import]
        private IProjectExportProvider WebProjectExportProvider { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package.</param>
        /// <exception cref="ArgumentNullException">Throws an <see cref="ArgumentNullException" /> if <paramref name="package" /> is null.</exception>
        private AddDockerSupportCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            this.package = package;

            // Invokes MEF to resolve the imports
            IComponentModel componentModel = ServiceProvider.GetService(typeof(SComponentModel)) as IComponentModel;
            Debug.Assert(componentModel != null, "Could not get the component model service");
            ICompositionService compositionService = componentModel.DefaultCompositionService;
            compositionService.SatisfyImportsOnce(this);

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            Debug.Assert(commandService != null, "Could not get the command service");

            // Context Menu
            var menuCommandID = new CommandID(CommandSet, AddDockerSupportContextMenuCommandId);
            OleMenuCommand contextMenuItem = new OleMenuCommand(AddDockerSupport_ExecuteCommand,
                                                                null,
                                                                AddDockerSupport_QueryStatus,
                                                                menuCommandID);
            commandService.AddCommand(contextMenuItem);

            // Project Options Menu
            menuCommandID = new CommandID(CommandSet, AddDockerSupportOptionsCommandId);
            OleMenuCommand projectOptionsMenuItem = new OleMenuCommand(AddDockerSupport_ExecuteCommand,
                                                                       null,
                                                                       AddDockerSupport_QueryStatus,
                                                                       menuCommandID);
            commandService.AddCommand(projectOptionsMenuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AddDockerSupportCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new AddDockerSupportCommand(package);
        }

        /// <summary>
        /// Checks the project type of the current selected project (using the IVsMonitorSelection service)
        /// Create new project items if it is a DNX type
        /// </summary>
        /// <remarks>Method is a wrapper for <c>DockerCommand.AddItemFromTemplate</c> </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddDockerSupport_ExecuteCommand(object sender, EventArgs e)
        {
            var hierarchy = GetSelectedProject();

            var workspace = new Workspace(Path.GetDirectoryName(hierarchy.DteProject.FullName));

            workspace.Scaffold("debian");

            AddDockerDebugProfiles(workspace);
        }

        /// <summary>
        /// Query Status to check the project type of the current selected project (using the IVsMonitorSelection service)
        /// Enables and makes it visible if it is DNX project type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddDockerSupport_QueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = (OleMenuCommand)sender;
            Debug.Assert(menuCommand != null, "Event sender cannot be null");

            var hierarchy = GetSelectedProject();

            if (!hierarchy.IsEmpty &&
                StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(hierarchy.DteProject.FileName), ".xproj"))
            {
                menuCommand.Visible = true;
                menuCommand.Enabled = !KnownUIContexts.SolutionBuildingContext.IsActive;
            }
            else
            {
                menuCommand.Visible = false;
                menuCommand.Enabled = false;
            }
        }

        private Hierarchy GetSelectedProject()
        {
            IVsMonitorSelection monitorSelection = ServiceProvider.GetService<IVsMonitorSelection, SVsShellMonitorSelection>();
            Debug.Assert(monitorSelection != null, "Could not find the current selected item.");

            IntPtr hierarchyPtr = IntPtr.Zero;
            uint itemId;
            IVsMultiItemSelect multiItemSelect;
            IntPtr selectionContainerPtr = IntPtr.Zero;
            IVsHierarchy project = null;

            try
            {
                Marshal.ThrowExceptionForHR(monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemId, out multiItemSelect, out selectionContainerPtr));

                // In the case of multi-select this will be null
                if (hierarchyPtr != IntPtr.Zero)
                {
                    project = (IVsHierarchy)Marshal.GetTypedObjectForIUnknown(hierarchyPtr, typeof(IVsHierarchy));
                }

            }
            finally
            {
                if (hierarchyPtr != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPtr);
                }

                if (selectionContainerPtr != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainerPtr);
                }
            }

            return new Hierarchy(project);
        }

        private void AddDockerDebugProfiles(Workspace workspace)
        {
            string launchSettingsFile = Path.Combine(workspace.WorkspaceDirectory, @"Properties\launchSettings.json");

            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            var content = JsonConvert.DeserializeObject(File.ReadAllText(launchSettingsFile), jsonSerializerSettings) as JObject;

            string dockerProfiles =
@"{
  ""profiles"": {
    ""Docker Fast"": {
    },
    ""Docker Regular"": {
    }
  }
}";

            content.Merge(JObject.Parse(dockerProfiles), new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Union });

            File.WriteAllText(launchSettingsFile, JsonConvert.SerializeObject(content, jsonSerializerSettings));
        }
    }
}
