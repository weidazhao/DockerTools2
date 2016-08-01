//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using IOleStream = Microsoft.VisualStudio.OLE.Interop.IStream;

namespace DockerTools2
{
    internal struct Hierarchy : IEquatable<Hierarchy>
    {
        private readonly IVsHierarchy _hierarchy;

        public static readonly Hierarchy Empty = new Hierarchy();

        public bool IsEmpty
        {
            get
            {
                return (_hierarchy == null);
            }
        }

        public bool IsLoaded
        {
            get
            {
                EnvDTE.Project project = this.DteProject;
                if (project == null)
                {
                    return false;
                }

                DteProject dteProject = new DteProject(this.DteProject);
                return !dteProject.IsUnloaded;
            }
        }

        public Hierarchy(IVsHierarchy hierarchy)
        {
            _hierarchy = hierarchy;
        }

        public static Hierarchy FromDteProject(IServiceProvider serviceProvider, EnvDTE.Project project)
        {
            DteProject dteProject = new DteProject(project);
            // "Miscellaneous Files" project throws a COM Exception for project.UniqueName call
            if (!dteProject.IsEmpty && !dteProject.IsUnloaded && !string.IsNullOrEmpty(project.FullName))
            {
                return Hierarchy.FromProjectName(serviceProvider, project.UniqueName);
            }

            return Hierarchy.Empty;
        }

        public static Hierarchy FromProjectName(IServiceProvider serviceProvider, string projectFullName)
        {
            if (string.IsNullOrEmpty(projectFullName))
            {
                return Hierarchy.Empty;
            }

            IVsSolution solution = serviceProvider.GetService<IVsSolution, SVsSolution>();
            if (solution != null)
            {
                IVsHierarchy hierarchy;
                int hr = solution.GetProjectOfUniqueName(projectFullName, out hierarchy);
                if (ErrorHandler.Succeeded(hr) && hierarchy != null)
                {
                    return new Hierarchy(hierarchy);
                }
            }

            return Hierarchy.Empty;
        }

        public static Hierarchy FromProjectId(Guid projectId)
        {
            IVsSolution solution = ServiceProvider.GlobalProvider.GetService<IVsSolution, SVsSolution>();
            if (solution != null)
            {
                IVsHierarchy hierarchy;
                int hr = solution.GetProjectOfGuid(ref projectId, out hierarchy);
                if (ErrorHandler.Succeeded(hr) && hierarchy != null)
                {
                    return new Hierarchy(hierarchy);
                }
            }

            return Hierarchy.Empty;
        }

        public static Hierarchy UnloadAndReloadProject(IServiceProvider serviceProvider, Guid projectId)
        {
            IStream openDocumentsStream = null;

            try
            {
                var shellDocumentWindowMgr = serviceProvider.GetRequiredService<IVsUIShellDocumentWindowMgr, SVsUIShellDocumentWindowMgr>();
                ErrorHandler.ThrowOnFailure(NativeMethods.CreateStreamOnHGlobal(hGlobal: IntPtr.Zero, fDeleteOnRelease: true, istream: ref openDocumentsStream));
                ErrorHandler.ThrowOnFailure(shellDocumentWindowMgr.SaveDocumentWindowPositions(0, (IOleStream)openDocumentsStream));

                var solution4 = serviceProvider.GetRequiredService<IVsSolution4, SVsSolution>();
                ErrorHandler.ThrowOnFailure(solution4.UnloadProject(ref projectId, (uint)_VSProjectUnloadStatus.UNLOADSTATUS_UnloadedByUser));
                ErrorHandler.ThrowOnFailure(solution4.ReloadProject(ref projectId));

                // Go to beginning of stream
                openDocumentsStream.Seek(dlibMove: 0, dwOrigin: 0, plibNewPosition: IntPtr.Zero);
                ErrorHandler.ThrowOnFailure(shellDocumentWindowMgr.ReopenDocumentWindows((IOleStream)openDocumentsStream));
            }
            finally
            {
                if (openDocumentsStream != null)
                {
                    Marshal.ReleaseComObject(openDocumentsStream);
                }
            }

            return Hierarchy.FromProjectId(projectId);
        }

        public static IEnumerable<Hierarchy> GetHierarchiesInSolution(__VSENUMPROJFLAGS projectKinds)
        {
            IVsSolution solution = ServiceProvider.GlobalProvider.GetService<IVsSolution, SVsSolution>();
            if (solution != null)
            {
                Guid projectType = Guid.Empty;
                IEnumHierarchies hierEnum;
                if (ErrorHandler.Succeeded(solution.GetProjectEnum(
                    (uint)projectKinds,     // grfEnumFlags (the types of project to return)
                    ref projectType,        // rguidEnumOnlyThisType (only used for __VSENUMPROJFLAGS.EPF_MATCHTYPE)
                    out hierEnum)))         // ppenum
                {
                    hierEnum.Reset();
                    IVsHierarchy[] vsHierarchies = new IVsHierarchy[1];
                    uint numReturned;
                    while (ErrorHandler.Succeeded(hierEnum.Next(1, vsHierarchies, out numReturned)) && numReturned == 1)
                    {
                        var hierarchy = new Hierarchy(vsHierarchies[0]);
                        // Skip the "Miscellaneous Files" project
                        if (hierarchy.DteProject == null ||
                            !StringComparer.OrdinalIgnoreCase.Equals(hierarchy.DteProject.Kind, EnvDTE.Constants.vsProjectKindMisc))
                        {
                            yield return hierarchy;
                        }
                    }
                }
            }
        }

        public TInterface GetComInterface<TInterface>() where TInterface : class
        {
            if (IsEmpty)
            {
                return null;
            }

            IntPtr unknownPtr = IntPtr.Zero;
            IntPtr interfacePtr = IntPtr.Zero;
            try
            {
                unknownPtr = Marshal.GetIUnknownForObject(_hierarchy);

                Guid iid = typeof(TInterface).GUID;
                if (ErrorHandler.Failed(Marshal.QueryInterface(unknownPtr, ref iid, out interfacePtr)) || (interfacePtr == IntPtr.Zero))
                {
                    return null;
                }

                return Marshal.GetObjectForIUnknown(interfacePtr) as TInterface;
            }
            finally
            {
                if (unknownPtr != IntPtr.Zero)
                {
                    Marshal.Release(unknownPtr);
                }
                if (interfacePtr != IntPtr.Zero)
                {
                    Marshal.Release(interfacePtr);
                }
            }
        }

        public IVsHierarchy VsHierarchy
        {
            get
            {
                return _hierarchy;
            }
        }

        public IVsProject VsProject
        {
            get
            {
                return GetComInterface<IVsProject>();
            }
        }

        public string ProjectDirectory
        {
            get
            {
                IVsProject vsProject = this.VsProject;
                if (vsProject != null)
                {
                    string mkDoc;
                    if (ErrorHandler.Succeeded(vsProject.GetMkDocument(VSConstants.VSITEMID_ROOT, out mkDoc)))
                    {
                        return Path.GetDirectoryName(mkDoc);
                    }
                }

                return null;
            }
        }

        public Guid ProjectId
        {
            get
            {
                return Root.GetGuidProperty(__VSHPROPID.VSHPROPID_ProjectIDGuid);
            }
        }

        public IEnumerable<Hierarchy> SourceProjectReferences
        {
            get
            {
                VSLangProj.VSProject vsLangProject = this.DteProject.Object as VSLangProj.VSProject;

                if (vsLangProject == null)
                {
                    return Enumerable.Empty<Hierarchy>();
                }

                return vsLangProject.References
                    .Cast<VSLangProj.Reference>()
                    .Where(r => r.SourceProject != null)
                    .Select(r => Hierarchy.FromDteProject(ServiceProvider.GlobalProvider, r.SourceProject))
                    .ToList();
            }
        }

        public EnvDTE.Project DteProject
        {
            get
            {
                return this.Root.ExtensionObject as EnvDTE.Project;
            }
        }

        public EnvDTE.BuildDependency ProjectBuildDependencies
        {
            get
            {
                EnvDTE.Project project = this.DteProject;
                if (project != null)
                {
                    EnvDTE.BuildDependencies buildDependencies = project.DTE.Solution.SolutionBuild.BuildDependencies;
                    EnvDTE.BuildDependency projectDependencies = buildDependencies.Item(project.UniqueName);
                    return projectDependencies;
                }

                return null;
            }
        }

        public EnvDTE.SolutionContext ActiveBuildContext
        {
            get
            {
                EnvDTE.Project project = this.DteProject;
                if (project != null)
                {
                    EnvDTE.SolutionBuild solutionBuild = project.DTE.Solution.SolutionBuild;
                    EnvDTE.SolutionConfiguration configuration = solutionBuild.ActiveConfiguration;
                    if (configuration != null)
                    {
                        EnvDTE.SolutionContexts contexts = configuration.SolutionContexts;
                        if (contexts != null)
                        {
                            return contexts.Item(project.UniqueName);
                        }
                    }
                }

                return null;
            }
        }

        public HierarchyItem Root
        {
            get
            {
                return new HierarchyItem(this, VSConstants.VSITEMID_ROOT);
            }
        }

        public IEnumerable<HierarchyItem> HierarchyItems
        {
            get
            {
                return this.Root.DescendantsAndSelf();
            }
        }

        /// <summary>
        /// Try to get a HierarchyItem from its FullPath
        /// </summary>
        public bool TryParseCanonicalName(string name, out HierarchyItem hierarchyItem)
        {
            hierarchyItem = HierarchyItem.Empty;

            if (_hierarchy != null)
            {
                uint itemId;
                if (ErrorHandler.Succeeded(_hierarchy.ParseCanonicalName(name, out itemId)))
                {
                    hierarchyItem = new HierarchyItem(this, itemId);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the item in the active window.
        /// </summary>
        public HierarchyItem SelectedFrameItem
        {
            get
            {
                object itemId = null;
                object documentFrame = null;

                IVsMonitorSelection selection = ServiceProvider.GlobalProvider.GetService<IVsMonitorSelection, SVsShellMonitorSelection>();
                if (!ErrorHandler.Succeeded(selection.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_DocumentFrame, out documentFrame)))
                {
                    return HierarchyItem.Empty;
                }

                IVsWindowFrame frame = documentFrame as IVsWindowFrame;
                if (frame == null || !ErrorHandler.Succeeded(frame.GetProperty((int)__VSFPROPID.VSFPROPID_ItemID, out itemId)))
                {
                    return HierarchyItem.Empty;
                }

                return new HierarchyItem(this, HierarchyItem.GetItemId(itemId));
            }
        }

        /// <summary>
        /// Gets the appropriate full name of the Hierarchy.
        /// </summary>
        public string FullName
        {
            get
            {
                StringBuilder nameBuilder = new StringBuilder();

                EnvDTE.Project currentProject = this.DteProject;

                while (currentProject != null)
                {
                    nameBuilder.Insert(0, currentProject.Name + "/");

                    currentProject = currentProject.ParentProjectItem != null ? currentProject.ParentProjectItem.ContainingProject : null;
                }

                if (nameBuilder.Length > 0)
                {
                    // Remove the last slash.
                    nameBuilder.Remove(nameBuilder.Length - 1, 1);
                }

                return nameBuilder.ToString();
            }
        }

        #region Equality

        public bool Equals(Hierarchy other)
        {
            return (this.IsEmpty && other.IsEmpty) || ComUtilities.IsSameComObject(_hierarchy, other._hierarchy);
        }

        public override bool Equals(object obj)
        {
            if (obj is Hierarchy)
            {
                return this == (Hierarchy)obj;
            }

            return base.Equals(obj);
        }

        public static bool operator ==(Hierarchy item1, Hierarchy item2)
        {
            return item1.Equals(item2);
        }

        public static bool operator !=(Hierarchy item1, Hierarchy item2)
        {
            return !(item1 == item2);
        }

        public override int GetHashCode()
        {
            return this.ProjectId.GetHashCode();
        }

        #endregion
    }
}
