using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;
using System;

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
    }
}
