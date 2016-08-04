using DockerTools2.Shared;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debuggers;
using Microsoft.VisualStudio.ProjectSystem.DotNet;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities.DebuggerProviders;
using Microsoft.VisualStudio.ProjectSystem.VS.Debuggers;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DockerTools2
{
    [Export(typeof(IDebugProfileLaunchTargetsProvider))]
    [AppliesTo("DotNetCore")]
    [OrderPrecedence(1000)]
    public class Docker2DebugProfileLaunchTargetsProvider : IDebugProfileLaunchTargetsProvider
    {
        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider ServiceProvider { get; set; }

        [Import]
        private ConfiguredProject ConfiguredProject { get; set; }

        public void OnAfterLaunch(DebugLaunchOptions launchOptions, IDebugProfile profile)
        {
        }

        public void OnBeforeLaunch(DebugLaunchOptions launchOptions, IDebugProfile profile)
        {
        }

        public async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions, IDebugProfile profile)
        {
            const string MIDebugEngineGuid = "{EA6637C6-17DF-45B5-A183-0951C54243BC}";

            const string SettingsOptionsTemplate =
@"<PipeLaunchOptions
    xmlns=""http://schemas.microsoft.com/vstudio/MDDDebuggerOptions/2014""
    PipePath = ""{0}""
    PipeArguments = ""{1}""
    ExePath = ""{2}""
    ExeArguments = ""{3}""
    TargetArchitecture = ""{4}""
    MIMode = ""{5}"" />";

            var workspace = new Workspace(Path.GetDirectoryName(ConfiguredProject.UnconfiguredProject.FullPath));

            var launchSettings = workspace.ParseLaunchSettings();

            EnsureEmptyDirectoryExists(Path.Combine(workspace.WorkspaceDirectory, launchSettings.EmptyFolderForDockerBuild));

            await workspace.DockerComposeClient.DevelopmentUpAsync();

            string containerId = await workspace.DockerClient.GetContainerIdAsync(workspace.WorkspaceName.ToLowerInvariant());

            string configuration = ConfiguredProject.ProjectConfiguration.Dimensions["Configuration"];

            string settingsOptions = string.Format(CultureInfo.InvariantCulture,
                                                   SettingsOptionsTemplate,
                                                   "docker",
                                                   $"exec -i {containerId} {launchSettings.DebuggerDirectory}/clrdbg --interpreter=mi",
                                                   launchSettings.StartProgram,
                                                   launchSettings.StartArguments.Replace("{Configuration}", configuration).Replace("{Framework}", "netcoreapp1.0"),
                                                   "x64",
                                                   "clrdbg");

            var settings = new DebugLaunchSettings(launchOptions);
            settings.LaunchOperation = DebugLaunchOperation.CreateProcess;
            settings.Executable = launchSettings.StartProgram;
            settings.Options = settingsOptions;
            settings.SendToOutputWindow = true;
            settings.Project = ConfiguredProject.UnconfiguredProject.ToHierarchy(ServiceProvider).VsHierarchy;
            settings.CurrentDirectory = launchSettings.WorkingDirectory;
            settings.LaunchDebugEngineGuid = new Guid(MIDebugEngineGuid);

            return new List<IDebugLaunchSettings>() { settings };
        }

        public bool SupportsProfile(IDebugProfile profile)
        {
            return StringComparer.Ordinal.Equals(profile.Name, "Docker2");
        }

        private void EnsureEmptyDirectoryExists(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    if (Directory.EnumerateFileSystemEntries(directory).Any())
                    {
                        Directory.Delete(directory, recursive: true);
                    }
                }
                else
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch
            {
                // Do nothing for now.
            }
        }
    }
}
