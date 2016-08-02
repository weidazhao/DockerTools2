//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public class DockerClient : IDockerClient
    {
        private readonly string _dockerPath;
        private readonly CommandLineClient _commandLineClient;

        public DockerClient()
        {
            _dockerPath = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramW6432%"), @"Docker\Docker\Resources\bin\docker.exe");
            _commandLineClient = new CommandLineClient();
        }

        public Task<string> ExecuteAsync(string command, string arguments, CancellationToken cancellationToken)
        {
            return _commandLineClient.ExecuteAsync(_dockerPath, command + " " + arguments, cancellationToken);
        }
    }
}
