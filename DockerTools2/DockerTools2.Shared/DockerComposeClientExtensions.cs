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
                                              bool noCache = false,
                                              CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (mode)
            {
                case DockerDevelopmentMode.Fast:
                    return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath} -f {client.Workspace.GetDockerComposeDevFilePath(mode)}",
                                               "build",
                                               cancellationToken);

                case DockerDevelopmentMode.Regular:
                    return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath}",
                                               $"build" + (noCache ? " --no-cache" : ""),
                                               cancellationToken);

                default:
                    throw new InvalidOperationException("The mode is not supported");
            }
        }

        public static Task<string> UpAsync(this IDockerComposeClient client,
                                           DockerDevelopmentMode mode,
                                           bool noBuild = false,
                                           CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath} -f {client.Workspace.GetDockerComposeDevFilePath(mode)}",
                                       "up -d" + (noBuild ? " --no-build" : ""),
                                       cancellationToken);
        }

        public static Task<string> DownAsync(this IDockerComposeClient client,
                                             DockerDevelopmentMode mode,
                                             bool removeImages = false,
                                             bool removeOrphans = false,
                                             CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (mode)
            {
                case DockerDevelopmentMode.Fast:
                    return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath} -f {client.Workspace.GetDockerComposeDevFilePath(mode)}",
                                               "down" + (removeImages ? " --rmi all" : "") + (removeOrphans ? " --remove-orphans" : ""),
                                               cancellationToken);

                case DockerDevelopmentMode.Regular:
                    return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath}",
                                               "down" + (removeImages ? " --rmi all" : "") + (removeOrphans ? " --remove-orphans" : ""),
                                               cancellationToken);

                default:
                    throw new InvalidOperationException("The mode is not supported");
            }
        }
    }
}
