//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DockerTools2.Shared
{
    public class LaunchSettings
    {
        public string DebuggeeProgram { get; set; }

        public string DebuggeeArguments { get; set; }

        public string DebuggeeWorkingDirectory { get; set; }

        public string DebuggeeTerminateProgram { get; set; }

        public string DebuggerProgram { get; set; }

        public string DebuggerArguments { get; set; }

        public string DebuggerTargetArchitecture { get; set; }

        public string DebuggerMIMode { get; set; }

        public string EmptyFolderForDockerBuild { get; set; }

        public void EnsureEmptyFolderForDockerBuildExists(string rootDirectory)
        {
            if (string.IsNullOrEmpty(EmptyFolderForDockerBuild))
            {
                return;
            }

            string directory = Path.Combine(rootDirectory, EmptyFolderForDockerBuild);

            try
            {
                if (Directory.Exists(directory))
                {
                    if (Directory.EnumerateFileSystemEntries(directory).Any())
                    {
                        Directory.Delete(directory, recursive: true);
                    }
                }
                else
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch
            {
                // Do nothing for now.
            }
        }

        public static LaunchSettings FromDockerComposeDocument(string serviceName, DockerComposeDocument document)
        {
            DockerComposeService service;
            if (!document.Services.TryGetValue(serviceName, out service))
            {
                return null;
            }

            var labels = service?.Labels;
            if (labels == null)
            {
                return null;
            }

            string debuggeeProgram;
            if (!TryGetValue(labels, "com.microsoft.visualstudio.debuggee.program", out debuggeeProgram))
            {
                return null;
            }

            string debuggeeArguments;
            if (!TryGetValue(labels, "com.microsoft.visualstudio.debuggee.arguments", out debuggeeArguments))
            {
                return null;
            }

            string debuggeeWorkingDirectory;
            if (!TryGetValue(labels, "com.microsoft.visualstudio.debuggee.workingdirectory", out debuggeeWorkingDirectory))
            {
                return null;
            }

            string debuggeeTerminateProgram;
            if (!TryGetValue(labels, "com.microsoft.visualstudio.debuggee.terminateprogram", out debuggeeTerminateProgram))
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

            string emptyFolderForDockerBuild = null;

            var buildArgs = service?.Build?.Args;
            if (buildArgs != null)
            {
                buildArgs.TryGetValue("source", out emptyFolderForDockerBuild);
            }

            return new LaunchSettings()
            {
                DebuggeeProgram = debuggeeProgram,
                DebuggeeArguments = debuggeeArguments,
                DebuggeeWorkingDirectory = debuggeeWorkingDirectory,
                DebuggeeTerminateProgram = debuggeeTerminateProgram,
                DebuggerProgram = debuggerProgram,
                DebuggerArguments = debuggerArguments,
                DebuggerTargetArchitecture = debuggerTargetArchitecture,
                DebuggerMIMode = debuggerMIMode,
                EmptyFolderForDockerBuild = emptyFolderForDockerBuild
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

                    labelValue = labelValue.Replace("$$", "$");

                    return true;
                }
            }

            return false;
        }
    }
}
