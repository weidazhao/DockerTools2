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
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

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

            //            const string SettingsOptionsTemplate =
            //@"<PipeLaunchOptions xmlns=""http://schemas.microsoft.com/vstudio/MDDDebuggerOptions/2014""
            //    PipePath = ""{0}""
            //    PipeArguments = ""{1}""
            //    ExePath = ""{2}""
            //    ExeArguments = ""{3}""
            //    TargetArchitecture ""{4}""
            //    MIMode = ""{5}"">
            //  </PipeLaunchOptions>
            //";


            //string settingsOptions = string.Format(CultureInfo.InvariantCulture, 
            //                                       SettingsOptionsTemplate,
            //                                       )

            var settings = new DebugLaunchSettings(launchOptions);
            settings.LaunchOperation = DebugLaunchOperation.CreateProcess;
            settings.Options = null;
            settings.SendToOutputWindow = true;
            settings.Project = ConfiguredProject.UnconfiguredProject.ToHierarchy(ServiceProvider).VsHierarchy;
            settings.LaunchDebugEngineGuid = new Guid(MIDebugEngineGuid);

            await Task.Yield();

            return new List<IDebugLaunchSettings>() { settings };
        }

        public bool SupportsProfile(IDebugProfile profile)
        {
            return StringComparer.Ordinal.Equals(profile.Name, "Docker2");
        }
    }
}
