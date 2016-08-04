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
        private const string FastMode = "Docker Fast";
        private const string RegularMode = "Docker Regular";

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

            DockerDevelopmentMode mode;
            if (!TryParseDockerDevelopmentMode(profile.Name, out mode))
            {
                throw new InvalidOperationException("The given profile is not supported");
            }

            var workspace = new Workspace(Path.GetDirectoryName(ConfiguredProject.UnconfiguredProject.FullPath));

            var launchSettings = workspace.ParseLaunchSettings(mode);

            if (!string.IsNullOrEmpty(launchSettings.EmptyFolderForDockerBuild))
            {
                EnsureEmptyDirectoryExists(Path.Combine(workspace.WorkspaceDirectory, launchSettings.EmptyFolderForDockerBuild));
            }

            await workspace.DockerComposeClient.DevelopmentUpAsync(mode);

            string containerId = await workspace.DockerClient.GetContainerIdAsync(workspace.WorkspaceName.ToLowerInvariant());

            string configuration = ConfiguredProject.ProjectConfiguration.Dimensions["Configuration"];

            string settingsOptions = string.Format(CultureInfo.InvariantCulture,
                                                   SettingsOptionsTemplate,
                                                   "docker",
                                                   $"exec -i {containerId} {launchSettings.DebuggerProgram} {launchSettings.DebuggerArguments}",
                                                   launchSettings.DebuggeeProgram,
                                                   launchSettings.DebuggeeArguments.Replace("{Configuration}", configuration).Replace("{Framework}", "netcoreapp1.0"),
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

        public bool SupportsProfile(IDebugProfile profile)
        {
            DockerDevelopmentMode mode;
            return TryParseDockerDevelopmentMode(profile.Name, out mode);
        }

        private bool TryParseDockerDevelopmentMode(string value, out DockerDevelopmentMode mode)
        {
            mode = DockerDevelopmentMode.Regular;

            if (StringComparer.Ordinal.Equals(value, FastMode))
            {
                mode = DockerDevelopmentMode.Fast;
                return true;
            }
            else if (StringComparer.Ordinal.Equals(value, RegularMode))
            {
                mode = DockerDevelopmentMode.Regular;
                return true;
            }

            return false;
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
