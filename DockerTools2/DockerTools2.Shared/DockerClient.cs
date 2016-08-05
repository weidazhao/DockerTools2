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
        private readonly Workspace _workspace;
        private readonly string _dockerPath;
        private readonly CommandLineClient _commandLineClient;

        public DockerClient(Workspace workspace)
        {
            _workspace = workspace;
            _dockerPath = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramW6432%"), @"Docker\Docker\Resources\bin\docker.exe");
            _commandLineClient = new CommandLineClient();
        }

        public Workspace Workspace => _workspace;

        public Task<string> ExecuteAsync(string options, string commandWithOptions, IDockerLogger logger, CancellationToken cancellationToken)
        {
            return _commandLineClient.ExecuteAsync(_dockerPath, options + " " + commandWithOptions, logger, cancellationToken);
        }
    }
}
