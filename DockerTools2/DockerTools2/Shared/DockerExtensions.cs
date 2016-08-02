//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public static class DockerExtensions
    {
        private static readonly string DockerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Docker\Docker\Resources\bin\docker.exe");

        public static Task<CommandLineClientResult> DockerPsAsync(this ICommandLineClient client, string arguments, CancellationToken cancellationToken)
        {
            return client.ExecuteAsync(DockerPath, "ps " + arguments, cancellationToken);
        }
    }
}
