//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace DockerTools2.Shared
{
    public class LaunchSettings
    {
        public string WorkingDirectory { get; set; }

        public string EmptyFolderForDockerBuild { get; set; }

        public string DebuggerProgram { get; set; }

        public string DebuggerArguments { get; set; }

        public string DebuggerTargetArchitecture { get; set; }

        public string DebuggerMIMode { get; set; }

        public string Program { get; set; }

        public string Arguments { get; set; }

        public static LaunchSettings FromDockerComposeDevelopmentDocument(string serviceName, DockerComposeDevelopmentDocument document)
        {
            DockerComposeService service;
            if (!document.Services.TryGetValue(serviceName, out service))
            {
                return null;
            }

            string workingDirectory = service?.WorkingDirectory;
            if (string.IsNullOrEmpty(workingDirectory))
            {
                return null;
            }

            var buildArgs = service?.Build?.Args;
            if (buildArgs == null)
            {
                return null;
            }

            string emptyFolderForDockerBuild;
            if (!buildArgs.TryGetValue("source", out emptyFolderForDockerBuild))
            {
                return null;
            }

            var labels = service?.Labels;
            if (labels == null)
            {
                return null;
            }

            string debuggerProgram;
            if (!TryGetValue(labels, "com.microsoft.visualstudio.debugger.program", out debuggerProgram))
            {
                return null;
            }

            string debuggerArguments;
            if (!TryGetValue(labels, "com.microsoft.visualstudio.debugger.arguments", out debuggerArguments))
            {
                return null;
            }

            string debuggerTargetArchitecture;
            if (!TryGetValue(labels, "com.microsoft.visualstudio.debugger.targetarchitecture", out debuggerTargetArchitecture))
            {
                return null;
            }

            string debuggerMIMode;
            if (!TryGetValue(labels, "com.microsoft.visualstudio.debugger.mimode", out debuggerMIMode))
            {
                return null;
            }

            string program;
            if (!TryGetValue(labels, "com.microsoft.visualstudio.program", out program))
            {
                return null;
            }

            string arguments;
            if (!TryGetValue(labels, "com.microsoft.visualstudio.arguments", out arguments))
            {
                return null;
            }

            return new LaunchSettings()
            {
                WorkingDirectory = workingDirectory,
                EmptyFolderForDockerBuild = emptyFolderForDockerBuild,
                DebuggerProgram = debuggerProgram,
                DebuggerArguments = debuggerArguments,
                DebuggerTargetArchitecture = debuggerTargetArchitecture,
                DebuggerMIMode = debuggerMIMode,
                Program = program,
                Arguments = arguments
            };
        }

        private static bool TryGetValue(IEnumerable<string> labels, string labelName, out string labelValue)
        {
            labelValue = null;

            foreach (string label in labels)
            {
                if (label.StartsWith(labelName + "=", StringComparison.Ordinal))
                {
                    labelValue = label.Substring(labelName.Length + 1);
                    return true;
                }
            }

            return false;
        }
    }
}
