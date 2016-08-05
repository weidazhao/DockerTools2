﻿//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.IO;
using YamlDeserializer = YamlDotNet.Serialization.Deserializer;

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

        public string WorkspaceDirectory => _workspaceDirectory.FullName;

        public string ServiceName => _workspaceDirectory.Name.ToLowerInvariant();

        public string ServiceTag => $"{ServiceName}_{ServiceName}";

        public string DockerFilePath => Path.Combine(WorkspaceDirectory, "Dockerfile");

        public string DockerComposeFilePath => Path.Combine(WorkspaceDirectory, "docker-compose.yml");

        public string GetDockerComposeDevFilePath(DockerDevelopmentMode mode)
        {
            string dockerComposeDevFilePath;

            switch (mode)
            {
                case DockerDevelopmentMode.Fast:
                    dockerComposeDevFilePath = Path.Combine(WorkspaceDirectory, "docker-compose.dev.fast.yml");
                    break;
                case DockerDevelopmentMode.Regular:
                    dockerComposeDevFilePath = Path.Combine(WorkspaceDirectory, "docker-compose.dev.regular.yml");
                    break;
                default:
                    dockerComposeDevFilePath = null;
                    break;
            }

            return dockerComposeDevFilePath;
        }

        public LaunchSettings ParseLaunchSettings(DockerDevelopmentMode mode)
        {
            string dockerComposeDevFilePath = GetDockerComposeDevFilePath(mode);

            if (dockerComposeDevFilePath == null)
            {
                return null;
            }

            using (var reader = new StreamReader(dockerComposeDevFilePath))
            {
                var deserializer = new YamlDeserializer(null, null, true);
                var document = deserializer.Deserialize<DockerComposeDocument>(reader);

                return LaunchSettings.FromDockerComposeDocument(ServiceName, document);
            }
        }
    }
}
