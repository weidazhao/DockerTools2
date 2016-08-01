//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DockerTools2
{
    internal static class DteUtilities
    {
        public static Project FindProjectByName(IServiceProvider serviceProvider, string projectName)
        {
            var dte = serviceProvider.GetRequiredService<DTE, SDTE>();

            if (dte.Solution != null)
            {
                return FindProjectInCollection(dte.Solution.Projects.Cast<Project>(), p => StringComparer.OrdinalIgnoreCase.Equals(p.Name, projectName));
            }

            return null;
        }

        public static IEnumerable<Project> FindSiblingsAndSelf(IServiceProvider serviceProvider, Project project)
        {
            var dte = serviceProvider.GetRequiredService<DTE, SDTE>();

            if (project.ParentProjectItem == null)
            {
                return dte.Solution.Projects.Cast<Project>();
            }
            else
            {
                return
                    project
                        .ParentProjectItem
                        .Collection
                        .Cast<ProjectItem>()
                        .Select(i => i.SubProject)
                        .Where(s => s != null);
            }
        }

        public static Project FindProjectInCollection(IEnumerable<Project> projects, Predicate<Project> projectPredicate)
        {
            foreach (Project project in projects)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(project.Kind, EnvDTE.Constants.vsProjectKindUnmodeled))
                {
                    // skip the unloaded projects
                    continue;
                }

                if (projectPredicate(project))
                {
                    return project;
                }

                ProjectItems items = project.ProjectItems;
                if (items != null)
                {
                    Project childProject = FindProjectInCollection(
                        items
                        .Cast<ProjectItem>()
                        .Select(i => i.SubProject)
                        .Where(s => s != null),
                    projectPredicate);

                    if (childProject != null)
                    {
                        return childProject;
                    }
                }
            }

            return null;
        }

        public static IEnumerable<ProjectItem> GetAllProjectItems(Project project)
        {
            if (project != null && project.ProjectItems != null)
            {
                foreach (ProjectItem projectItem in project.ProjectItems)
                {
                    yield return projectItem;

                    foreach (var pi in GetAllProjectItems(projectItem))
                    {
                        yield return pi;
                    }
                }
            }
        }

        public static IEnumerable<ProjectItem> GetAllProjectItems(ProjectItem projectItem)
        {
            if (projectItem != null && projectItem.ProjectItems != null)
            {
                foreach (ProjectItem pi in projectItem.ProjectItems)
                {
                    yield return pi;

                    foreach (var innerItem in GetAllProjectItems(pi))
                    {
                        yield return innerItem;
                    }
                }
            }
        }

        public static bool TryGetProjectItem(ProjectItems projectItems, string path, out ProjectItem projectItem)
        {
            projectItem = null;

            var expectedItems = path.Split(Path.DirectorySeparatorChar);
            foreach (var expectedItem in expectedItems)
            {
                projectItem = projectItems.Cast<ProjectItem>().FirstOrDefault(pi => pi.Name == expectedItem);

                if (projectItem == null)
                {
                    return false;
                }

                projectItems = projectItem.ProjectItems;
            }

            return true;
        }

        public static string GetFullPath(Project project)
        {
            Debug.Assert(project != null, "Project passed to GetFullPath() is null");

            string path = null;
            Property property = project.Properties.Item("FullPath");
            if (property != null)
            {
                path = Path.GetFullPath(property.Value as string);
            }
            return path;
        }

        public static string GetFullPath(ProjectItem item)
        {
            Debug.Assert(item != null, "ProjectItem passed to GetFullPath() is null");

            string path = null;
            Property property = item.Properties.Item("FullPath");
            if (property != null)
            {
                path = Path.GetFullPath(property.Value as string);
            }
            return path;
        }

        public static bool TryGetContainingSolutionFolder(Project project, out SolutionFolder solutionFolder)
        {
            solutionFolder = null;
            if (project.ParentProjectItem != null &&
                project.ParentProjectItem.ContainingProject != null)
            {
                solutionFolder = project.ParentProjectItem.ContainingProject.Object as SolutionFolder;
            }

            return solutionFolder != null;
        }
    }
}
