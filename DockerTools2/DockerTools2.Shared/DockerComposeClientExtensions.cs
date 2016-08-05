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
            if (mode != DockerDevelopmentMode.Regular)
            {
                throw new ArgumentException("Build is supported in the regular mode only.", nameof(mode));
            }

            return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath}",
                                       $"build" + (noCache ? " --no-cache" : ""),
                                       logger,
                                       cancellationToken);
        }

        public static Task<string> UpAsync(this IDockerComposeClient client,
                                           DockerDevelopmentMode mode,
                                           IDockerLogger logger,
                                           CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath} -f {client.Workspace.GetDockerComposeDevFilePath(mode)}",
                                       "up -d" + (mode == DockerDevelopmentMode.Regular ? " --no-build" : ""),
                                       logger,
                                       cancellationToken);
        }

        public static Task<string> DownAsync(this IDockerComposeClient client,
                                             DockerDevelopmentMode mode,
                                             bool removeAllImages,
                                             bool removeOrphans,
                                             IDockerLogger logger,
                                             CancellationToken cancellationToken = default(CancellationToken))
        {
            return client.ExecuteAsync($"-f {client.Workspace.DockerComposeFilePath} -f {client.Workspace.GetDockerComposeDevFilePath(mode)}",
                                       "down" + (removeAllImages ? " --rmi all" : "") + (removeOrphans ? " --remove-orphans" : ""),
                                       logger,
                                       cancellationToken);
        }
    }
}
