//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public static class DockerExtensions
    {
        private static readonly string DockerPath = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramW6432%"), @"Docker\Docker\Resources\bin\docker.exe");

        public static Task<string> GetDockerContainerIdAsync(this ICommandLineClient client, string containerName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync(DockerPath, $"ps --filter \"name={containerName}\" --format {{{{.ID}}}} --last 1", cancellationToken);
        }
    }
}
