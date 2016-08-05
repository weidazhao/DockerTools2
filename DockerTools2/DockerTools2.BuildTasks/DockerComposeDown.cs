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

        public bool RemoveImages { get; set; }

        public bool RemoveOrphans { get; set; }

        protected override Task ExecuteAsync(Workspace workspace, DockerDevelopmentMode mode, CancellationToken cancellationToken)
        {
            return workspace.DockerComposeClient.DownAsync(mode, RemoveImages, RemoveOrphans, cancellationToken);
        }
    }
}
