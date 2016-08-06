//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
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

        public static async Task<string> RemoveDanglingImagesAsync(this IDockerClient client, IDockerLogger logger, CancellationToken cancellationToken = default(CancellationToken))
        {
            string danglingImageIds = await client.ExecuteAsync(string.Empty, $"images --filter dangling=true --format {{{{.ID}}}}", logger, cancellationToken);

            string[] danglingImageIdArray = danglingImageIds.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var danglingImageId in danglingImageIdArray)
            {
                await client.ExecuteAsync(string.Empty, $"rmi {danglingImageId}", logger, cancellationToken);
            }

            return string.Empty;
        }
    }
}
