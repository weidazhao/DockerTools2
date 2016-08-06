//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task EnsureClrDbgExistsAsync(IDockerLogger logger)
        {
            string clrDbgDirectory = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "clrdbg"));

            if (Directory.Exists(clrDbgDirectory))
            {
                return;
            }

            logger.LogMessage("Downloading Core CLR debugger...");

            Uri downloadUrl = new Uri("https://raw.githubusercontent.com/Microsoft/MIEngine/getclrdbg-release/scripts/GetClrDbg.ps1", UriKind.Absolute);
            string localPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "GetClrDbg.ps1"));

            using (var client = new HttpClient())
            {
                using (var responseStream = await client.GetStreamAsync(downloadUrl))
                {
                    using (var fileStream = new FileStream(localPath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        await responseStream.CopyToAsync(fileStream);
                    }
                }
            }

            await new CommandLineClient().ExecuteAsync(Environment.ExpandEnvironmentVariables(@"%WINDIR%\System32\WindowsPowerShell\v1.0\powershell.exe"),
                                                       $"-NonInteractive -ExecutionPolicy RemoteSigned {localPath} -Version 'VS2015U2' -RuntimeID 'debian.8-x64' -InstallPath '{clrDbgDirectory}'",
                                                       logger,
                                                       CancellationToken.None);
        }
    }
}
