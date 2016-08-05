using DockerTools2.Shared;
using Microsoft.Build.Framework;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace DockerTools2.BuildTasks
{
    public class DockerComposeBuild : BaseBuildTask
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

        public bool NoCache { get; set; }

        protected override Task ExecuteAsync(Workspace workspace, DockerDevelopmentMode mode, IDockerLogger logger, CancellationToken cancellationToken)
        {
            return workspace.DockerComposeClient.BuildAsync(mode, NoCache, logger, cancellationToken);
        }
    }
}
