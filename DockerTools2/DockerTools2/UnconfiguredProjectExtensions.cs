using DockerTools2.Shared;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;

namespace DockerTools2
{
    internal static class UnconfiguredProjectExtensions
    {
        public static Hierarchy ToHierarchy(this UnconfiguredProject project, IServiceProvider serviceProvider)
        {
            IVsSolution solution = (IVsSolution)serviceProvider.GetService(typeof(SVsSolution));
            IVsHierarchy hierarchy;
            if (solution != null && ErrorHandler.Succeeded(solution.GetProjectOfUniqueName(project.FullPath, out hierarchy)))
            {
                return new Hierarchy(hierarchy);
            }

            return Hierarchy.Empty;
        }

        public static Workspace ToWorkspace(this UnconfiguredProject project)
        {
            return new Workspace(Path.GetDirectoryName(project.FullPath));
        }
    }
}
