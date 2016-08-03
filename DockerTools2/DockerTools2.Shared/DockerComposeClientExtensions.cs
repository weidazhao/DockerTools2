//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public static class DockerComposeClientExtensions
    {
        public static Task<string> DevelopmentDownAsync(this IDockerComposeClient client, CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath} -f {client.Workspace.DockerComposeDevelopmentFilePath}", "down", cancellationToken);
        }

        public static Task<string> DevelopmentUpAsync(this IDockerComposeClient client, CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath} -f {client.Workspace.DockerComposeDevelopmentFilePath}", "up -d", cancellationToken);
        }
    }
}
