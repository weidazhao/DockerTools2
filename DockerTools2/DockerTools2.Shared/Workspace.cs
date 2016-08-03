//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.IO;

namespace DockerTools2.Shared
{
    public class Workspace
    {
        private readonly string _workingDirectory;
        private readonly IDockerClient _dockerClient;
        private readonly IDockerComposeClient _dockerComposeClient;

        public Workspace(string workingDirectory)
        {
            _workingDirectory = Path.GetFullPath(workingDirectory);
            _dockerClient = new DockerClient(this);
            _dockerComposeClient = new DockerComposeClient(this);
        }

        public IDockerClient DockerClient => _dockerClient;

        public IDockerComposeClient DockerComposeClient => _dockerComposeClient;

        public string DockerFilePath => Path.Combine(_workingDirectory, "Dockerfile");

        public string DockerComposeFilePath => Path.Combine(_workingDirectory, "docker-compose.yml");

        public string DockerComposeDevelopmentFilePath => Path.Combine(_workingDirectory, "docker-compose.dev.yml");
    }
}
