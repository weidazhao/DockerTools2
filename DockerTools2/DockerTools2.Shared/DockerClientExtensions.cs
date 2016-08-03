//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public static class DockerClientExtensions
    {
        public static Task<string> GetContainerIdAsync(this IDockerClient client, string containerName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync(string.Empty, $"ps --filter \"name={containerName}\" --format {{{{.ID}}}} --last 1", cancellationToken);
        }
    }
}
