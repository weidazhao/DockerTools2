using Microsoft.VisualStudio.ProjectSystem.Debuggers;
using Microsoft.VisualStudio.ProjectSystem.DotNet;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.VS.Debuggers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace DockerTools2
{
    [Export(typeof(IDebugProfileLaunchTargetsProvider))]
    [AppliesTo("DotNetCore")]
    [OrderPrecedence(1000)]
    public class Docker2DebugProfileLaunchTargetsProvider : IDebugProfileLaunchTargetsProvider
    {
        public void OnAfterLaunch(DebugLaunchOptions launchOptions, IDebugProfile profile)
        {
        }

        public void OnBeforeLaunch(DebugLaunchOptions launchOptions, IDebugProfile profile)
        {
        }

        public async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions, IDebugProfile profile)
        {
            //            const string DebugLaunchSettingsOptions =
            //@"<PipeLaunchOptions xmlns=""http://schemas.microsoft.com/vstudio/MDDDebuggerOptions/2014\""
            //    PipePath = ""{0}""
            //    PipeArguments = ""{1}""
            //    ExePath = ""{2}""
            //    ExeArguments = ""{3}""
            //    TargetArchitecture ""{4}""
            //    MIMode = ""{5}"">
            //  </PipeLaunchOptions>
            //";

            await Task.Yield();

            return new List<IDebugLaunchSettings>();
        }

        public bool SupportsProfile(IDebugProfile profile)
        {
            return StringComparer.Ordinal.Equals(profile.Name, "Docker2");
        }
    }
}
