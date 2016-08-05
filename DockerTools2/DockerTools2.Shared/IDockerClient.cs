//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public interface IDockerClient
    {
        Workspace Workspace { get; }

        Task<string> ExecuteAsync(string options, string commandWithOptions, IDockerLogger logger, CancellationToken cancellationToken);
    }
}
