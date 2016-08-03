//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using DockerTools2.Shared;
using System;
using System.Threading.Tasks;

namespace DockerTools2.Tests
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            RunAsync(args).GetAwaiter().GetResult();
        }

        private static async Task RunAsync(string[] args)
        {
            string workingDirectory = args.Length == 1 ? args[0] : @"C:\Repos\DockerToolsPerf\DockerToolsPerf\src\DockerToolsPerf";

            var workspace = new Workspace(workingDirectory);

            Console.WriteLine(await workspace.DockerClient.GetContainerIdAsync("dockertoolsperf"));

            Console.WriteLine("Docker compose up started.");

            await workspace.DockerComposeClient.UpAsync();

            Console.WriteLine("Docker compose up completed.");

            Console.Read();

            Console.WriteLine("Docker compose down started.");

            await workspace.DockerComposeClient.DownAsync();

            Console.WriteLine("Docker compose down completed.");

            Console.Read();
        }
    }
}
