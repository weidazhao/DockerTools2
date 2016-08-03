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

            var dcd = workspace.ParseDockerComposeDevelopmentFile();

            string containerId = await workspace.DockerClient.GetContainerIdAsync(workspace.WorkspaceName.ToLowerInvariant());

            string settingsOptions = string.Format(CultureInfo.InvariantCulture,
                                                   SettingsOptionsTemplate,
                                                   "docker",
                                                   $"exec -i {containerId} /clrdbg/clrdbg --interpreter=mi",
                                                   "dotnet",
                                                   $"--additionalprobingpath /root/.nuget/packages bin/Debug/netcoreapp1.0/{workspace.WorkspaceName}.dll",
                                                   "x64",
                                                   "clrdbg");

            var settings = new DebugLaunchSettings(launchOptions);
            settings.LaunchOperation = DebugLaunchOperation.CreateProcess;
            settings.Executable = "dotnet";
            settings.Options = settingsOptions;
            settings.SendToOutputWindow = true;
            settings.Project = ConfiguredProject.UnconfiguredProject.ToHierarchy(ServiceProvider).VsHierarchy;
            settings.CurrentDirectory = "/app";
            settings.LaunchDebugEngineGuid = new Guid(MIDebugEngineGuid);

            return new List<IDebugLaunchSettings>() { settings };
        }

        public bool SupportsProfile(IDebugProfile profile)
        {
            return StringComparer.Ordinal.Equals(profile.Name, "Docker2");
        }
    }
}
