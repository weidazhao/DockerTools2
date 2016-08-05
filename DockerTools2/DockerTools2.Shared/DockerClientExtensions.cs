//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public static class DockerClientExtensions
    {
        public static Task<string> GetContainerIdAsync(this IDockerClient client, string containerName, IDockerLogger logger, CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync(string.Empty, $"ps --filter \"name={containerName}\" --format {{{{.ID}}}} --last 1", logger, cancellationToken);
        }

        public static Task<string> ExecAsync(this IDockerClient client, string containerId, string command, IDockerLogger logger, CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync(string.Empty, $"exec -i {containerId} {command}", logger, cancellationToken);
        }
    }
}
