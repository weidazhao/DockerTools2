using DockerTools2.Shared;
using Microsoft.Build.Framework;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace DockerTools2.BuildTasks
{
    public class PrepareForBuild : BaseBuildTask
    {
        [Required]
        public string WorkspaceDirectory
        {
            get { return _workspaceDirectory; }
            set { _workspaceDirectory = value; }
        }

        [Required]
        public string Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        protected override async Task ExecuteAsync(Workspace workspace, DockerDevelopmentMode mode, IDockerLogger logger, CancellationToken cancellationToken)
        {
            //
            // Detects if development mode has changed since last time. If yes, run docker compose down against the previous mode first.
            //
            DockerDevelopmentMode previous;
            if (workspace.TryGetDockerDevelopmentModeFromCache(out previous) && previous != mode)
            {
                await workspace.DockerComposeClient.DownAsync(previous, true, true, logger, cancellationToken);
            }

            //
            // Ensures that the special empty folder exists before build.
            //
            var launchSettings = workspace.ParseLaunchSettings(mode);

            launchSettings.EnsureEmptyFolderForDockerBuildExists(workspace.WorkspaceDirectory);

            //
            // Ensures that the process in the container is killed.
            //
            string containerId = await workspace.DockerClient.GetContainerIdAsync(workspace.ServiceTag, logger, cancellationToken);

            if (!string.IsNullOrEmpty(containerId))
            {
                await workspace.DockerClient.ExecAsync(containerId, launchSettings.DebuggeeKillProgram, logger, cancellationToken);
            }
        }
    }
}
