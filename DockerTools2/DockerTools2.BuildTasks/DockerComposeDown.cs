using DockerTools2.Shared;
using Microsoft.Build.Framework;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace DockerTools2.BuildTasks
{
    public class DockerComposeDown : BaseBuildTask
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

        public bool RemoveAllImages { get; set; }

        public bool RemoveOrphans { get; set; }

        protected override async Task ExecuteAsync(Workspace workspace, DockerDevelopmentMode mode, IDockerLogger logger, CancellationToken cancellationToken)
        {
            //
            // Detects if development mode has changed since last time. If yes, run docker compose down against the previous mode first.
            //
            DockerDevelopmentMode previous;
            if (workspace.TryGetDockerDevelopmentModeFromCache(out previous) && previous != mode)
            {
                await workspace.DockerComposeClient.DownAsync(previous, RemoveAllImages, RemoveOrphans, logger, cancellationToken);
            }

            await workspace.DockerComposeClient.DownAsync(mode, RemoveAllImages, RemoveOrphans, logger, cancellationToken);
        }
    }
}
