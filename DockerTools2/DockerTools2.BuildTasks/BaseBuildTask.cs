using DockerTools2.Shared;
using Microsoft.Build.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BuildTask = Microsoft.Build.Utilities.Task;

namespace DockerTools2.BuildTasks
{
    public abstract class BaseBuildTask : BuildTask
    {
        protected string _workspaceDirectory;
        protected string _mode;

        public sealed override bool Execute()
        {
            if (!Directory.Exists(_workspaceDirectory))
            {
                return false;
            }

            var workspace = new Workspace(_workspaceDirectory);

            DockerDevelopmentMode mode;
            if (!DockerDevelopmentModeParser.TryParse(_mode, out mode))
            {
                return false;
            }

            try
            {
                ExecuteAsync(workspace, mode, new DockerLogger(this), CancellationToken.None).GetAwaiter().GetResult();
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
        }

        protected abstract Task ExecuteAsync(Workspace workspace, DockerDevelopmentMode mode, IDockerLogger logger, CancellationToken cancellationToken);

        private class DockerLogger : IDockerLogger
        {
            private readonly BuildTask _buildTask;

            public DockerLogger(BuildTask buildTask)
            {
                _buildTask = buildTask;
            }

            public void LogError(string error)
            {
                _buildTask.Log.LogError(error);
            }

            public void LogMessage(string message)
            {
                _buildTask.Log.LogMessage(MessageImportance.High, message);
            }

            public void LogWarning(string warning)
            {
                _buildTask.Log.LogWarning(warning);
            }
        }
    }
}
