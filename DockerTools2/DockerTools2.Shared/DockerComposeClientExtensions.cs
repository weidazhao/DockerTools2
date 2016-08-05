//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public static class DockerComposeClientExtensions
    {
        public static Task<string> BuildAsync(this IDockerComposeClient client,
                                              DockerDevelopmentMode mode,
                                              bool noCache,
                                              IDockerLogger logger,
                                              CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (mode)
            {
                case DockerDevelopmentMode.Fast:
                    return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath} -f {client.Workspace.GetDockerComposeDevFilePath(mode)}",
                                               "build",
                                               logger,
                                               cancellationToken);

                case DockerDevelopmentMode.Regular:
                    return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath}",
                                               $"build" + (noCache ? " --no-cache" : ""),
                                               logger,
                                               cancellationToken);

                default:
                    throw new InvalidOperationException("The mode is not supported");
            }
        }

        public static Task<string> UpAsync(this IDockerComposeClient client,
                                           DockerDevelopmentMode mode,
                                           bool noBuild,
                                           IDockerLogger logger,
                                           CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (mode)
            {
                case DockerDevelopmentMode.Fast:
                    return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath} -f {client.Workspace.GetDockerComposeDevFilePath(mode)}",
                                               "up -d",
                                               logger,
                                               cancellationToken);

                case DockerDevelopmentMode.Regular:
                    return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath} -f {client.Workspace.GetDockerComposeDevFilePath(mode)}",
                                               "up -d" + (noBuild ? " --no-build" : ""),
                                               logger,
                                               cancellationToken);

                default:
                    throw new InvalidOperationException("The mode is not supported");
            }
        }

        public static Task<string> DownAsync(this IDockerComposeClient client,
                                             DockerDevelopmentMode mode,
                                             bool removeImages,
                                             bool removeOrphans,
                                             IDockerLogger logger,
                                             CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (mode)
            {
                case DockerDevelopmentMode.Fast:
                    return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath} -f {client.Workspace.GetDockerComposeDevFilePath(mode)}",
                                               "down" + (removeImages ? " --rmi all" : "") + (removeOrphans ? " --remove-orphans" : ""),
                                               logger,
                                               cancellationToken);

                case DockerDevelopmentMode.Regular:
                    return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath}",
                                               "down" + (removeImages ? " --rmi all" : "") + (removeOrphans ? " --remove-orphans" : ""),
                                               logger,
                                               cancellationToken);

                default:
                    throw new InvalidOperationException("The mode is not supported");
            }
        }
    }
}
