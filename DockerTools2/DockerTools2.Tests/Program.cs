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
            string workingDirectory = args[0];

            string command = args[1];

            var workspace = new Workspace(workingDirectory);

            switch (command)
            {
                case "up":
                    Console.WriteLine("Docker compose up started.");
                    await workspace.DockerComposeClient.DevelopmentUpAsync();
                    Console.WriteLine("Docker compose up completed.");
                    break;

                case "down":
                    Console.WriteLine("Docker compose down started.");
                    await workspace.DockerComposeClient.DevelopmentDownAsync();
                    Console.WriteLine("Docker compose down completed.");
                    break;
                default:
                    break;
            }
        }
    }
}
