//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public class DockerComposeClient : IDockerComposeClient
    {
        private readonly string _dockerComposePath;
        private readonly CommandLineClient _commandLineClient;

        public DockerComposeClient()
        {
            _dockerComposePath = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramW6432%"), @"Docker\Docker\Resources\bin\docker-compose.exe");
            _commandLineClient = new CommandLineClient();
        }

        public Task<string> ExecuteAsync(string command, string arguments, CancellationToken cancellationToken)
        {
            return _commandLineClient.ExecuteAsync(_dockerComposePath, command + " " + arguments, cancellationToken);
        }
    }
}
