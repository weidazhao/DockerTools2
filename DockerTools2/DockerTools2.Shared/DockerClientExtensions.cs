//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public static class DockerClientExtensions
    {
        #region Basic commands

        public static Task<string> ExecAsync(this IDockerClient client, string arguments, CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync("exec", arguments, cancellationToken);
        }

        public static Task<string> ImagesAsync(this IDockerClient client, string arguments, CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync("images", arguments, cancellationToken);
        }

        public static Task<string> PsAsync(this IDockerClient client, string arguments, CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync("ps", arguments, cancellationToken);
        }

        public static Task<string> RmAsync(this IDockerClient client, string arguments, CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync("rm", arguments, cancellationToken);
        }

        public static Task<string> RmiAsync(this IDockerClient client, string arguments, CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync("rmi", arguments, cancellationToken);
        }

        public static Task<string> StopAsync(this IDockerClient client, string arguments, CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync("stop", arguments, cancellationToken);
        }

        #endregion Basic commands

        public static Task<string> GetContainerIdAsync(this IDockerClient client, string containerName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.PsAsync($"--filter \"name={containerName}\" --format {{{{.ID}}}} --last 1", cancellationToken);
        }
    }
}
