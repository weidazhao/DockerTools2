//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using DockerTools2.Shared;

namespace DockerTools2.Tests
{
    public static class Program
    {
        public static readonly ICommandLineClient client = new CommandLineClient();

        public static void Main(string[] args)
        {
            string result = client.GetDockerContainerIdAsync("dockertoolsperf").Result;
        }
    }
}
