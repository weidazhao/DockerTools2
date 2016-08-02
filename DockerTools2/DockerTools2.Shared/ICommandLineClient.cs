//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public interface ICommandLineClient
    {
        Task<CommandLineClientResult> ExecuteAsync(string command, string arguments, CancellationToken cancellationToken);
    }
}
