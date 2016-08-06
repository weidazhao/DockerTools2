//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public class CommandLineClient
    {
        public Task<string> ExecuteAsync(string command, string arguments, IDockerLogger logger, CancellationToken cancellationToken)
        {
            logger?.LogMessage($"\"{command}\" \"{arguments}\"");

            var startInfo = new ProcessStartInfo()
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };

            var process = new Process()
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            var cancellationTokenRegistration = cancellationToken.Register(() =>
            {
                lock (process)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Win32Exception)
                    {
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            });

            var outputData = new List<string>();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputData.Add(e.Data);

                    logger?.LogMessage(e.Data);
                }
            };

            var errorData = new List<string>();

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorData.Add(e.Data);

                    //
                    // Docker sends info to stderr sometimes. Need to investigate if it's by design.
                    //
                    logger?.LogMessage(e.Data);
                }
            };

            var taskCompletionSource = new TaskCompletionSource<string>();

            process.Exited += (sender, e) =>
            {
                // Even though the Exited event has occurred, there can still be pending calls to the event handlers for ErrorDataReceived and OutputDataReceived.
                // Calling WaitForExit ensures that those data streams have been closed.  Time out after 10 seconds to avoid waiting forever because there can be rare
                // occurences where it waits indefinitely for the data streams to closed even though no more output is happening.
                process.WaitForExit(10000);

                if (cancellationToken.IsCancellationRequested)
                {
                    taskCompletionSource.TrySetCanceled();
                }
                else if (process.ExitCode == 0)
                {
                    taskCompletionSource.TrySetResult(string.Join(Environment.NewLine, outputData));
                }
                else
                {
                    taskCompletionSource.TrySetException(new CommandLineClientException(string.Join(Environment.NewLine, errorData)));
                }

                process.Dispose();
                cancellationTokenRegistration.Dispose();
            };

            lock (process)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    process.Start();

                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                }
                else
                {
                    taskCompletionSource.TrySetCanceled();

                    process.Dispose();
                    cancellationTokenRegistration.Dispose();
                }
            }

            return taskCompletionSource.Task;
        }
    }
}
