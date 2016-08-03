//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.IO;

namespace DockerTools2.Shared
{
    public class Workspace
    {
        private readonly DirectoryInfo _workspaceDirectory;
        private readonly IDockerClient _dockerClient;
        private readonly IDockerComposeClient _dockerComposeClient;

        public Workspace(string workspaceDirectory)
        {
            _workspaceDirectory = new DirectoryInfo(workspaceDirectory);
            _dockerClient = new DockerClient(this);
            _dockerComposeClient = new DockerComposeClient(this);
        }

        public IDockerClient DockerClient => _dockerClient;

        public IDockerComposeClient DockerComposeClient => _dockerComposeClient;

        public string WorkspaceName => _workspaceDirectory.Name;

        public string WorkspaceDirectory => _workspaceDirectory.FullName;

        public string DockerFilePath => Path.Combine(WorkspaceDirectory, "Dockerfile");

        public string DockerComposeFilePath => Path.Combine(WorkspaceDirectory, "docker-compose.yml");

        public string DockerComposeDevelopmentFilePath => Path.Combine(WorkspaceDirectory, "docker-compose.dev.yml");
    }
}
