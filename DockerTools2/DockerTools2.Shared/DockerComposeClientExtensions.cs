//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public static class DockerComposeClientExtensions
    {
        #region Basic commands

        public static Task<string> BuildAsync(this IDockerComposeClient client, string arguments, CancellationToken cancellationToken)
        {
            return client.ExecuteAsync("build", arguments, cancellationToken);
        }

        public static Task<string> DownAsync(this IDockerComposeClient client, string arguments, CancellationToken cancellationToken)
        {
            return client.ExecuteAsync("down", arguments, cancellationToken);
        }

        public static Task<string> UpAsync(this IDockerComposeClient client, string arguments, CancellationToken cancellationToken)
        {
            return client.ExecuteAsync("up", arguments, cancellationToken);
        }

        #endregion Basic commands
    }
}
