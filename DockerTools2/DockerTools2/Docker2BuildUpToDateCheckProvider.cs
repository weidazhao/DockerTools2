using DockerTools2.Shared;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2
{
    [Export(typeof(IBuildUpToDateCheckProvider))]
    [AppliesTo("DotNetCore")]
    public class Docker2BuildUpToDateCheckProvider : IBuildUpToDateCheckProvider
    {
        private const string ActiveDebugProfile = "ActiveDebugProfile";

        [Import]
        private ConfiguredProject ConfiguredProject { get; set; }

        public async Task<bool> IsUpToDateAsync(BuildAction buildAction, TextWriter logger, CancellationToken cancellationToken = default(CancellationToken))
        {
            var workspace = ConfiguredProject.UnconfiguredProject.ToWorkspace();

            string activeDebugProfile = await ConfiguredProject.Services.ProjectPropertiesProvider.GetCommonProperties().GetEvaluatedPropertyValueAsync(ActiveDebugProfile);

            DockerDevelopmentMode mode;
            if (!workspace.TryParseDockerDevelopmentMode(activeDebugProfile, out mode))
            {
                throw new InvalidOperationException("The given profile is not supported");
            }

            switch (buildAction)
            {
                case BuildAction.Build:
                case BuildAction.Compile:
                    DockerDevelopmentMode previous;
                    return workspace.TryGetDockerDevelopmentModeFromCache(out previous) && previous == mode;

                case BuildAction.Clean:
                case BuildAction.Rebuild:
                    return false;

                default:
                    return true;
            }
        }

        public async Task<bool> IsUpToDateCheckEnabledAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var workspace = ConfiguredProject.UnconfiguredProject.ToWorkspace();

            string activeDebugProfile = await ConfiguredProject.Services.ProjectPropertiesProvider.GetCommonProperties().GetEvaluatedPropertyValueAsync(ActiveDebugProfile);

            DockerDevelopmentMode mode;
            return workspace.TryParseDockerDevelopmentMode(activeDebugProfile, out mode);
        }
    }
}
