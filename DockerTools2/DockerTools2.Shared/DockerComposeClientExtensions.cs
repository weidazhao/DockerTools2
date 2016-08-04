//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public static class DockerComposeClientExtensions
    {
        public static Task<string> DevelopmentUpAsync(this IDockerComposeClient client, DockerDevelopmentMode mode, CancellationToken cancellationToken = default(CancellationToken))
        {
            string commandWithOptions = "up -d";
            if (mode == DockerDevelopmentMode.Regular)
            {
                commandWithOptions += " --no-build";
            }

            return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath} -f {client.Workspace.GetDockerComposeDevelopmentFilePath(mode)}", commandWithOptions, cancellationToken);
        }
    }
}
