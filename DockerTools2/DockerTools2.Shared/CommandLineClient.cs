//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DockerTools2.Shared
{
    public class CommandLineClient : ICommandLineClient
    {
        public Task<CommandLineClientResult> ExecuteAsync(string command, string arguments, CancellationToken cancellationToken)
        {
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

            var outputData = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                outputData.Append(e.Data);
            };

            var errorData = new StringBuilder();

            process.ErrorDataReceived += (sender, e) =>
            {
                errorData.Append(e.Data);
            };

            var taskCompletionSource = new TaskCompletionSource<CommandLineClientResult>();

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
                    taskCompletionSource.TrySetResult(new CommandLineClientResult() { Result = outputData.ToString() });
                }
                else
                {
                    taskCompletionSource.TrySetException(new CommandLineClientException(errorData.ToString()));
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
