//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using DockerTools2.Shared;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace DockerTools2
{
    internal static class VsOutputWindowPaneHelper
    {
        private static readonly object CreateVsOutputWindowPaneLockObject = new object();

        public static IVsOutputWindowPane GetVsOutputWindowPane(IServiceProvider serviceProvider, Guid paneGuid, bool clearPane, bool activatePane = true)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var dte = serviceProvider.GetRequiredService<DTE2, SDTE>();
            var vsOutputWindow = serviceProvider.GetRequiredService<IVsOutputWindow, SVsOutputWindow>();

            //
            // Activates output window.
            //
            dte.ToolWindows.OutputWindow.Parent.Activate();

            //
            // Gets the VsOutputWindowPane specified by the GUID.
            //
            IVsOutputWindowPane vsOutputWindowPane = null;
            int hr = vsOutputWindow.GetPane(ref paneGuid, out vsOutputWindowPane);

            //
            // Activates the pane and clears it if requested.
            //
            if (ErrorHandler.Succeeded(hr) && vsOutputWindowPane != null)
            {
                if (activatePane)
                {
                    vsOutputWindowPane.Activate();
                }

                if (clearPane)
                {
                    vsOutputWindowPane.Clear();
                }
            }

            return vsOutputWindowPane;
        }

        public static IVsOutputWindowPane GetOrCreateVsOutputWindowPane(IServiceProvider serviceProvider, Guid paneGuid, string paneName, bool clearPane, bool activatePane = true)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (paneName == null)
            {
                throw new ArgumentNullException(nameof(paneName));
            }

            var dte = serviceProvider.GetRequiredService<DTE2, SDTE>();
            var vsOutputWindow = serviceProvider.GetRequiredService<IVsOutputWindow, SVsOutputWindow>();

            //
            // Activates output window.
            //
            dte.ToolWindows.OutputWindow.Parent.Activate();

            //
            // Gets the VsOutputWindowPane specified by the GUID.
            //
            IVsOutputWindowPane vsOutputWindowPane = null;
            int hr = vsOutputWindow.GetPane(ref paneGuid, out vsOutputWindowPane);

            //
            // Creates a new pane with the specified GUID and name if it doesn't exist yet.
            //
            if (!ErrorHandler.Succeeded(hr) || vsOutputWindowPane == null)
            {
                lock (CreateVsOutputWindowPaneLockObject)
                {
                    hr = vsOutputWindow.GetPane(ref paneGuid, out vsOutputWindowPane);

                    if (!ErrorHandler.Succeeded(hr) || vsOutputWindowPane == null)
                    {
                        vsOutputWindow.CreatePane(paneGuid, paneName, fInitVisible: 1, fClearWithSolution: 0);

                        hr = vsOutputWindow.GetPane(ref paneGuid, out vsOutputWindowPane);
                    }
                }
            }

            //
            // Activates the pane and clears it if requested.
            //
            if (ErrorHandler.Succeeded(hr) && vsOutputWindowPane != null)
            {
                if (activatePane)
                {
                    vsOutputWindowPane.Activate();
                }

                if (clearPane)
                {
                    vsOutputWindowPane.Clear();
                }
            }

            return vsOutputWindowPane;
        }

        public static IVsOutputWindowPane GetDebugOutputWindowPane(IServiceProvider serviceProvider, bool clearPane, bool activatePane = true)
        {
            return GetVsOutputWindowPane(serviceProvider, VSConstants.OutputWindowPaneGuid.DebugPane_guid, clearPane, activatePane);
        }

        public static IVsOutputWindowPane GetBuildOutputWindowPane(IServiceProvider serviceProvider, bool clearPane, bool activatePane = true)
        {
            return GetVsOutputWindowPane(serviceProvider, VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, clearPane, activatePane);
        }

        public static IDockerLogger ToDockerLogger(this IVsOutputWindowPane pane)
        {
            return new DockerLogger(pane);
        }

        private class DockerLogger : IDockerLogger
        {
            private readonly IVsOutputWindowPane _pane;

            public DockerLogger(IVsOutputWindowPane pane)
            {
                _pane = pane;
            }

            public void LogError(string error)
            {
                _pane.OutputStringThreadSafe(error + Environment.NewLine);
            }

            public void LogMessage(string message)
            {
                _pane.OutputStringThreadSafe(message + Environment.NewLine);
            }

            public void LogWarning(string warning)
            {
                _pane.OutputStringThreadSafe(warning + Environment.NewLine);
            }
        }
    }
}
