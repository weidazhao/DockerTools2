//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YamlDeserializer = YamlDotNet.Serialization.Deserializer;

namespace DockerTools2.Shared
{
    public class Workspace
    {
        private const string DockerDevelopmentModeCacheFile = @"obj\Docker\DockerDevelopmentMode";

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

        public bool TryParseDockerDevelopmentMode(string value, out DockerDevelopmentMode mode)
        {
            mode = DockerDevelopmentMode.Regular;

            if (StringComparer.Ordinal.Equals(value, "Docker Fast") || StringComparer.Ordinal.Equals(value, "Fast"))
            {
                mode = DockerDevelopmentMode.Fast;
                return true;
            }
            else if (StringComparer.Ordinal.Equals(value, "Docker Regular") || StringComparer.Ordinal.Equals(value, "Regular"))
            {
                mode = DockerDevelopmentMode.Regular;
                return true;
            }

            return false;
        }

        public bool TryGetDockerDevelopmentModeFromCache(out DockerDevelopmentMode mode)
        {
            mode = DockerDevelopmentMode.Regular;

            try
            {
                var fileInfo = new FileInfo(Path.Combine(WorkspaceDirectory, DockerDevelopmentModeCacheFile));

                if (!fileInfo.Exists)
                {
                    return false;
                }

                return TryParseDockerDevelopmentMode(File.ReadAllText(fileInfo.FullName), out mode);
            }
            catch
            {
                return false;
            }
        }

        public bool TrySetDockerDevelopmentModeToCache(DockerDevelopmentMode current)
        {
            try
            {
                var fileInfo = new FileInfo(Path.Combine(WorkspaceDirectory, DockerDevelopmentModeCacheFile));

                fileInfo.Directory.Create();

                File.WriteAllText(fileInfo.FullName, current.ToString());

                return true;
            }
            catch
            {
                return false;
            }
        }

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

        public async Task OpenApplicationUrlInBrowserAsync(Uri applicationUrl, IDockerLogger logger)
        {
            logger.LogMessage($"Launching the browser with URL {applicationUrl}");

            const int MaxRetries = 30;

            using (var httpClient = new HttpClient())
            {
                for (int retries = 0; retries < MaxRetries; retries++)
                {
                    try
                    {
                        var response = await httpClient.GetAsync(applicationUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        // Ignore exception.
                    }

                    // Wait a second before retry.
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            //
            // Launch browser even if we couldn't reach the URL.
            //
            Process.Start(applicationUrl.AbsoluteUri);
        }
    }
}
