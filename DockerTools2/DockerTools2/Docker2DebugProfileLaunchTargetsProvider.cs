using DockerTools2.Shared;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debuggers;
using Microsoft.VisualStudio.ProjectSystem.DotNet;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities.DebuggerProviders;
using Microsoft.VisualStudio.ProjectSystem.VS.Debuggers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
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
            var workspace = ConfiguredProject.UnconfiguredProject.ToWorkspace();

            DockerDevelopmentMode mode;
            if (!workspace.TryParseDockerDevelopmentMode(profile.Name, out mode))
            {
                throw new InvalidOperationException("The given profile is not supported");
            }

            var launchSettings = workspace.ParseLaunchSettings(mode);

            Uri applicationUrl;
            if (!Uri.TryCreate(launchSettings.DebuggeeUrl, UriKind.Absolute, out applicationUrl))
            {
                return;
            }

            var dockerLogger = VsOutputWindowPaneHelper.GetDebugOutputWindowPane(ServiceProvider, false, true).ToDockerLogger();

            workspace.OpenApplicationUrlInBrowserAsync(applicationUrl, dockerLogger).Forget();
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

            var workspace = ConfiguredProject.UnconfiguredProject.ToWorkspace();

            DockerDevelopmentMode mode;
            if (!workspace.TryParseDockerDevelopmentMode(profile.Name, out mode))
            {
                throw new InvalidOperationException("The given profile is not supported");
            }

            if (mode == DockerDevelopmentMode.Regular && (launchOptions & DebugLaunchOptions.NoDebug) != 0)
            {
                throw new InvalidOperationException("Edit & Refresh is supported in fast mode only.");
            }

            var dockerLogger = VsOutputWindowPaneHelper.GetDebugOutputWindowPane(ServiceProvider, false, true).ToDockerLogger();

            var launchSettings = workspace.ParseLaunchSettings(mode);

            //
            // Ensures debugger installed.
            //

            await workspace.EnsureClrDbgExistsAsync(dockerLogger);

            //
            // Ensures docker compose is up.
            //

            await workspace.DockerComposeClient.UpAsync(mode, dockerLogger);

            workspace.TrySetDockerDevelopmentModeToCache(mode);

            string containerId = await workspace.DockerClient.GetContainerIdAsync(workspace.ServiceTag, dockerLogger);
            if (string.IsNullOrEmpty(containerId))
            {
                throw new InvalidOperationException($"Can not find the container with the name starting with {workspace.ServiceTag}.");
            }

            //
            // Ensures no existing debuggee process running in the container.
            //

            await workspace.DockerClient.ExecAsync(containerId, launchSettings.DebuggeeKillProgram, dockerLogger);

            //
            // Launches app.
            //

            string configuration = ConfiguredProject.ProjectConfiguration.Dimensions["Configuration"];
            string debuggeeArguments = launchSettings.DebuggeeArguments.Replace("{Configuration}", configuration).Replace("{Framework}", "netcoreapp1.0");

            if (!launchOptions.HasFlag(DebugLaunchOptions.NoDebug))
            {
                string settingsOptions = string.Format(CultureInfo.InvariantCulture,
                                                       SettingsOptionsTemplate,
                                                       "docker",
                                                       $"exec -i {containerId} {launchSettings.DebuggerProgram} {launchSettings.DebuggerArguments}",
                                                       launchSettings.DebuggeeProgram,
                                                       debuggeeArguments,
                                                       launchSettings.DebuggerTargetArchitecture,
                                                       launchSettings.DebuggerMIMode);

                var settings = new DebugLaunchSettings(launchOptions);
                settings.LaunchOperation = DebugLaunchOperation.CreateProcess;
                settings.Executable = launchSettings.DebuggeeProgram;
                settings.Options = settingsOptions;
                settings.SendToOutputWindow = true;
                settings.Project = ConfiguredProject.UnconfiguredProject.ToHierarchy(ServiceProvider).VsHierarchy;
                settings.CurrentDirectory = launchSettings.DebuggeeWorkingDirectory;
                settings.LaunchDebugEngineGuid = new Guid(MIDebugEngineGuid);

                return new List<IDebugLaunchSettings>() { settings };
            }
            else
            {
                workspace.DockerClient.ExecAsync(containerId, $"{launchSettings.DebuggeeProgram} {debuggeeArguments}", dockerLogger).Forget();

                return new List<IDebugLaunchSettings>();
            }
        }

        public bool SupportsProfile(IDebugProfile profile)
        {
            var workspace = ConfiguredProject.UnconfiguredProject.ToWorkspace();

            DockerDevelopmentMode mode;
            return workspace.TryParseDockerDevelopmentMode(profile.Name, out mode);
        }
    }
}
