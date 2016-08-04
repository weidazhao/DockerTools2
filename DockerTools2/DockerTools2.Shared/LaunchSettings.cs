//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Linq;

namespace DockerTools2.Shared
{
    public class LaunchSettings
    {
        public string EmptyFolderForDockerBuild { get; set; }

        public string StartProgram { get; set; }

        public string StartArguments { get; set; }

        public static LaunchSettings FromDockerComposeDevelopmentDocument(string serviceName, DockerComposeDevelopmentDocument document)
        {
            DockerComposeService service;
            if (!document.Services.TryGetValue(serviceName, out service))
            {
                return null;
            }

            var buildArgs = service?.Build?.Args;
            if (buildArgs == null)
            {
                return null;
            }

            string emptyFolder;
            if (!buildArgs.TryGetValue("source", out emptyFolder))
            {
                return null;
            }

            var labels = service?.Labels;
            if (labels == null)
            {
                return null;
            }

            string program = labels.Select(p => ExtractValueFromLabel(p, "com.microsoft.development.program"))
                                   .FirstOrDefault(p => !string.IsNullOrEmpty(p));

            if (string.IsNullOrEmpty(program))
            {
                return null;
            }

            string arguments = labels.Select(p => ExtractValueFromLabel(p, "com.microsoft.development.arguments"))
                                     .FirstOrDefault(p => !string.IsNullOrEmpty(p));

            if (string.IsNullOrEmpty(arguments))
            {
                return null;
            }

            return new LaunchSettings() { EmptyFolderForDockerBuild = emptyFolder, StartProgram = program, StartArguments = arguments };
        }

        private static string ExtractValueFromLabel(string labelWithValue, string label)
        {
            if (labelWithValue.StartsWith(label, StringComparison.Ordinal))
            {
                return labelWithValue.Substring(labelWithValue.IndexOf("=") + 1).Trim();
            }

            return null;
        }
    }
}
