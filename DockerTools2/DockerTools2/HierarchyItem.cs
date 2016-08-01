//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace DockerTools2
{
    internal struct HierarchyItem : IEquatable<HierarchyItem>
    {
        private readonly Hierarchy _hierarchy;
        private readonly uint _itemId;

        public static readonly HierarchyItem Empty = new HierarchyItem();

        public bool IsEmpty
        {
            get { return _itemId == VSConstants.VSITEMID_NIL || _hierarchy.IsEmpty; }
        }

        public HierarchyItem(Hierarchy hierarchy, uint itemId)
        {
            _hierarchy = hierarchy;
            _itemId = itemId;
        }

        public HierarchyItem(IVsHierarchy hierarchy, uint itemId)
        {
            _hierarchy = new Hierarchy(hierarchy);
            _itemId = itemId;
        }

        public static HierarchyItem FromFullPath(IVsHierarchy hierarchy, string fullPath)
        {
            Hierarchy hier = new Hierarchy(hierarchy);
            return HierarchyItem.FromFullPath(hier, fullPath);
        }

        public static HierarchyItem FromFullPath(Hierarchy hierarchy, string fullPath)
        {
            if (!hierarchy.IsEmpty)
            {
                HierarchyItem item;
                if (hierarchy.TryParseCanonicalName(fullPath, out item))
                {
                    return item;
                }
            }

            return HierarchyItem.Empty;
        }

        public static HierarchyItem FromDocData(IVsHierarchy hierarchy, IntPtr punkDocData)
        {
            string filePath = GetFilePath(punkDocData);

            if (!string.IsNullOrEmpty(filePath))
            {
                return HierarchyItem.FromFullPath(hierarchy, filePath);
            }

            return HierarchyItem.Empty;
        }

        public static HierarchyItem FromReference(VSLangProj.Reference reference)
        {
            IVsBrowseObject browseObject = reference as IVsBrowseObject;
            if (browseObject != null)
            {
                uint itemId = 0;
                IVsHierarchy hierarchy = null;
                ErrorHandler.ThrowOnFailure(browseObject.GetProjectItem(out hierarchy, out itemId));
                return new HierarchyItem(hierarchy, itemId);
            }

            return HierarchyItem.Empty;
        }

        public VSLangProj.prjBuildAction? BuildAction
        {
            get
            {
                EnvDTE.ProjectItem projectItem = this.DteProjectItem;
                if (projectItem == null)
                {
                    return null;
                }

                EnvDTE.Property property = projectItem.Properties.Item("BuildAction");
                if (property == null)
                {
                    return null;
                }

                VSLangProj.prjBuildAction? buildAction;
                try
                {
                    buildAction = (VSLangProj.prjBuildAction)property.Value;
                }
                catch (InvalidCastException)
                {
                    return null;
                }

                return buildAction;
            }
        }

        public void SetBuildAction(VSLangProj.prjBuildAction buildAction)
        {
            EnvDTE.ProjectItem projectItem = this.DteProjectItem;
            if (projectItem != null)
            {
                EnvDTE.Property property = projectItem.Properties.Item("BuildAction");
                if (property != null)
                {
                    property.Value = buildAction;
                    return;
                }
            }

            Debug.Fail("Unable to set the BuildAction property");
        }

        public string CanonicalName
        {
            get
            {
                string name = null;
                if (!IsEmpty)
                {
                    ErrorHandler.ThrowOnFailure(_hierarchy.VsHierarchy.GetCanonicalName(_itemId, out name));
                }
                return name;
            }
        }

        public object ExtensionObject
        {
            get { return GetProperty(__VSHPROPID.VSHPROPID_ExtObject); }
        }

        public HierarchyItem FirstChild
        {
            get { return GetItem(GetProperty(__VSHPROPID.VSHPROPID_FirstChild)); }
        }

        public HierarchyItem FirstVisibleChild
        {
            get { return GetItem(GetProperty(__VSHPROPID.VSHPROPID_FirstVisibleChild)); }
        }

        public string FullPath
        {
            get
            {
                if (IsRoot)
                {
                    EnvDTE.Project project = this.DteProject;
                    if (project != null)
                    {
                        return DteUtilities.GetFullPath(project);
                    }
                }
                else
                {
                    EnvDTE.ProjectItem projectItem = this.DteProjectItem;
                    if (projectItem != null)
                    {
                        return DteUtilities.GetFullPath(projectItem);
                    }
                }

                return string.Empty;
            }
        }

        public Hierarchy Hierarchy
        {
            get
            {
                if (IsEmpty)
                {
                    return Hierarchy.Empty;
                }

                return _hierarchy;
            }
        }

        public IVsHierarchy VsHierarchy
        {
            get
            {
                if (IsEmpty)
                {
                    return null;
                }

                return _hierarchy.VsHierarchy;
            }
        }

        public int IconIndex
        {
            get
            {
                object value = GetProperty(__VSHPROPID.VSHPROPID_IconIndex);
                if (value != null)
                {
                    return Convert.ToInt32(value, CultureInfo.InvariantCulture);
                }
                return -1;
            }
        }

        public bool IsAncestorOf(HierarchyItem hierarchyItem)
        {
            while (!hierarchyItem.IsEmpty)
            {
                if (hierarchyItem.Parent == this)
                {
                    return true;
                }
                hierarchyItem = hierarchyItem.Parent;
            }
            return false;
        }

        public bool IsExpandable
        {
            get
            {
                return Convert.ToBoolean(GetProperty(__VSHPROPID.VSHPROPID_Expandable), CultureInfo.InvariantCulture);
            }
        }

        public bool IsExpanded
        {
            get
            {
                IVsUIHierarchy uiHierarchy = GetComInterface<IVsUIHierarchy>();

                if (uiHierarchy == null)
                {
                    return false;
                }

                uint stateMask = (uint)__VSHIERARCHYITEMSTATE.HIS_Expanded;
                uint state;
                ErrorHandler.ThrowOnFailure(SolutionExplorerWindow.GetItemState(uiHierarchy, this.ItemId, stateMask, out state));

                return ((__VSHIERARCHYITEMSTATE)state == __VSHIERARCHYITEMSTATE.HIS_Expanded);
            }
        }

        public void SetIsExpanded(bool value)
        {
            if (this.IsExpandable)
            {
                IVsUIHierarchy uiHierarchy = GetComInterface<IVsUIHierarchy>();

                if (uiHierarchy != null)
                {

                    EXPANDFLAGS action = (value) ? EXPANDFLAGS.EXPF_ExpandFolder : EXPANDFLAGS.EXPF_CollapseFolder;

                    ErrorHandler.ThrowOnFailure(SolutionExplorerWindow.ExpandItem(uiHierarchy, this.ItemId, action));
                }
            }
        }

        public bool IsFile
        {
            get
            {
                Guid typeGuid = GetGuidProperty(__VSHPROPID.VSHPROPID_TypeGuid);
                return typeGuid == VSConstants.GUID_ItemType_PhysicalFile;
            }
        }

        public bool IsFolder
        {
            get
            {
                Guid typeGuid = GetGuidProperty(__VSHPROPID.VSHPROPID_TypeGuid);
                return typeGuid == VSConstants.GUID_ItemType_PhysicalFolder;
            }
        }

        public bool IsItemInProject
        {
            get
            {
                return !Convert.ToBoolean(GetProperty(__VSHPROPID.VSHPROPID_IsNonMemberItem), CultureInfo.InvariantCulture);
            }
        }

        public bool IsRoot
        {
            get { return !IsEmpty && (_itemId == VSConstants.VSITEMID_ROOT); }
        }

        public bool IsVirtualFolder
        {
            get
            {
                Guid typeGuid = GetGuidProperty(__VSHPROPID.VSHPROPID_TypeGuid);
                return typeGuid == VSConstants.GUID_ItemType_VirtualFolder;
            }
        }

        public bool IsReferencesSpecialFolder
        {
            get
            {
                return IsOfSubtype("Reference");

            }
        }

        public bool IsPropertiesSpecialFolder
        {
            get
            {
                return IsOfSubtype("AppDesigner");
            }
        }

        public bool IsSystemSpecialFolder
        {
            get
            {
                return IsOfSubtype("AppDesigner", "WebReference", "WebReferences", "Reference");
            }
        }

        public uint ItemId
        {
            get
            {
                if (IsEmpty)
                {
                    return VSConstants.VSITEMID_NIL;
                }
                return _itemId;
            }
        }

        public string Name
        {
            get { return GetProperty(__VSHPROPID.VSHPROPID_Name) as string; }
        }

        public HierarchyItem NextSibling
        {
            get { return GetItem(GetProperty(__VSHPROPID.VSHPROPID_NextSibling)); }
        }

        public HierarchyItem NextVisibleSibling
        {
            get { return GetItem(GetProperty(__VSHPROPID.VSHPROPID_NextVisibleSibling)); }
        }

        public HierarchyItem Parent
        {
            get { return GetItem(GetProperty(__VSHPROPID.VSHPROPID_Parent)); }
        }

        public IVsProject VSProject
        {
            get { return this.Hierarchy.VsProject; }
        }

        public EnvDTE.Project DteProject
        {
            get { return GetRoot().ExtensionObject as EnvDTE.Project; }
        }

        public EnvDTE.ProjectItem DteProjectItem
        {
            get { return ExtensionObject as EnvDTE.ProjectItem; }
        }

        public IVsUserContext UserContext
        {
            get { return GetProperty(__VSHPROPID.VSHPROPID_UserContext) as IVsUserContext; }
        }

        public IVsUIHierarchyWindow SolutionExplorerWindow
        {
            get { return VsShellUtilities.GetUIHierarchyWindow(ServiceProvider.GlobalProvider, new Guid(EnvDTE.Constants.vsWindowKindSolutionExplorer)); }
        }

        public IEnumerable<HierarchyItem> ChildItems()
        {
            HierarchyItem item = FirstChild;
            if (!item.IsEmpty)
            {
                yield return item;

                while (true)
                {
                    item = item.NextSibling;
                    if (!item.IsEmpty)
                    {
                        yield return item;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public IEnumerable<HierarchyItem> Ancestors()
        {
            return AncestorsAndSelf().Skip(1);
        }

        public IEnumerable<HierarchyItem> AncestorsAndSelf()
        {
            HierarchyItem item = this;

            while (!item.IsEmpty)
            {
                yield return item;
                item = item.Parent;
            }
        }

        public IEnumerable<HierarchyItem> Descendants()
        {
            return DescendantsAndSelf().Skip(1);
        }

        public IEnumerable<HierarchyItem> DescendantsAndSelf()
        {
            yield return this;

            HierarchyItem item = this;
            bool ascending = false;
            while (!item.IsEmpty)
            {
                HierarchyItem firstChild;
                HierarchyItem nextSibling;

                // We do depth first walk: get the child nodes first
                if (!ascending && !(firstChild = item.FirstChild).IsEmpty)
                {
                    yield return firstChild;
                    item = firstChild;
                    continue;
                }

                // Stop when return to the root,
                // or if there are no child nodes on the first iteration
                if (item == this)
                {
                    yield break;
                }

                // If there are no children, begin getting siblings
                if (!(nextSibling = item.NextSibling).IsEmpty)
                {
                    yield return nextSibling;
                    item = nextSibling;
                    ascending = false;
                    continue;
                }

                // If there are no siblings, go back up to the parent
                item = item.Parent;
                ascending = true;
            }

            Debug.Fail("DescendantsAndSelf encountered empty node");
        }

        public string GetAttribute(string name)
        {
            if (!IsEmpty)
            {
                IVsBuildPropertyStorage propertyStorage = GetComInterface<IVsBuildPropertyStorage>();
                if (propertyStorage != null)
                {
                    string value;
                    int hr = propertyStorage.GetItemAttribute(_itemId, name, out value);
                    if (ErrorHandler.Succeeded(hr))
                    {
                        return value;
                    }
                }
            }
            return null;
        }

        public static Guid GetProjectId(IVsHierarchy hierarchy)
        {
            HierarchyItem item = new HierarchyItem(hierarchy, VSConstants.VSITEMID_ROOT);
            return item.GetGuidProperty(__VSHPROPID.VSHPROPID_ProjectIDGuid);
        }

        public static HierarchyItem GetSelectedFrameItem(IVsHierarchy hierarchy)
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
            return new HierarchyItem(hierarchy, GetItemId(itemId));
        }

        public HierarchyItem GetRoot()
        {
            return new HierarchyItem(_hierarchy, VSConstants.VSITEMID_ROOT);
        }

        public static HierarchyItem GetRoot(IVsHierarchy hierarchy)
        {
            return new HierarchyItem(hierarchy, VSConstants.VSITEMID_ROOT);
        }

        public void SetAttribute(string name, string value)
        {
            if (!IsEmpty)
            {
                IVsBuildPropertyStorage propertyStorage = GetComInterface<IVsBuildPropertyStorage>();
                if (propertyStorage != null)
                {
                    ErrorHandler.ThrowOnFailure(propertyStorage.SetItemAttribute(_itemId, name, value));
                }
            }
        }

        public void OpenDocument()
        {
            EnvDTE.ProjectItem projectItem = this.DteProjectItem;
            if (projectItem != null)
            {
                projectItem.Open(EnvDTE.Constants.vsViewKindPrimary);
                projectItem.Document.Activate();
            }
        }

        public static bool TryParseCanonicalName(IVsHierarchy hierarchy, string name, out HierarchyItem hierarchyItem)
        {
            uint itemId;
            if (ErrorHandler.Succeeded(hierarchy.ParseCanonicalName(name, out itemId)))
            {
                hierarchyItem = new HierarchyItem(hierarchy, itemId);
                return true;
            }
            hierarchyItem = HierarchyItem.Empty;
            return false;
        }

        public IEnumerable<HierarchyItem> VisibleChildItems()
        {
            HierarchyItem item = FirstVisibleChild;
            if (!item.IsEmpty)
            {
                yield return item;

                while (true)
                {
                    item = item.NextVisibleSibling;
                    if (!item.IsEmpty)
                    {
                        yield return item;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Get the specified property from the __VSHPROPID enumeration for this item
        /// </summary>
        public object GetProperty(__VSHPROPID propId)
        {
            if (!IsEmpty)
            {
                object value = null;
                int hr = _hierarchy.VsHierarchy.GetProperty(_itemId, (int)propId, out value);
                if (!ErrorHandler.Failed(hr))
                {
                    return value;
                }
            }
            return null;
        }

        public Guid GetGuidProperty(__VSHPROPID propId)
        {
            if (!IsEmpty)
            {
                Guid guid;
                try
                {
                    int hr = _hierarchy.VsHierarchy.GetGuidProperty(_itemId, (int)propId, out guid);
                    if (ErrorHandler.Succeeded(hr))
                    {
                        return guid;
                    }
                }
                catch (Exception)
                {
                    // Ignore exceptions
                }
            }
            return Guid.Empty;
        }

        public override string ToString()
        {
            return this.ItemId.ToString(CultureInfo.CurrentCulture) + " : '" + this.Name + "'";
        }

        #region Equality

        public override bool Equals(object obj)
        {
            if (obj is HierarchyItem)
            {
                HierarchyItem item = (HierarchyItem)obj;
                return this == item;
            }
            return base.Equals(obj);
        }

        public bool Equals(HierarchyItem other)
        {
            return (this.IsEmpty && other.IsEmpty)
                || (this._itemId == other._itemId && this._hierarchy == other._hierarchy);
        }

        public static bool operator ==(HierarchyItem item1, HierarchyItem item2)
        {
            return item1.Equals(item2);
        }

        public static bool operator !=(HierarchyItem item1, HierarchyItem item2)
        {
            return !(item1 == item2);
        }

        public override int GetHashCode()
        {
            if (this.IsEmpty)
            {
                return 0;
            }

            return _hierarchy.GetHashCode() ^ _itemId.GetHashCode();
        }

        #endregion

        #region Private methods

        private bool IsOfSubtype(params string[] subTypes)
        {
            if (this.IsFolder || this.IsVirtualFolder)
            {
                var propertyValues = GetPropertyValues(UserContext, "SubType");

                // Any value of SubType property matches any value in the argument array
                return propertyValues.Any(i => subTypes.Any(j => StringComparer.OrdinalIgnoreCase.Equals(i, j)));
            }

            return false;
        }

        private HierarchyItem GetItem(object value)
        {
            return new HierarchyItem(_hierarchy, GetItemId(value));
        }

        public static uint GetItemId(object value)
        {
            if (value == null)
            {
                return VSConstants.VSITEMID_NIL;
            }

            if (value is int)
            {
                return (uint)(int)value;
            }

            if (value is uint)
            {
                return (uint)value;
            }

            if (value is short)
            {
                return (uint)(short)value;
            }

            if (value is ushort)
            {
                return (uint)(ushort)value;
            }

            if (value is long)
            {
                return (uint)(long)value;
            }

            return VSConstants.VSITEMID_NIL;
        }

        private TInterface GetComInterface<TInterface>()
            where TInterface : class
        {
            if (IsEmpty)
            {
                return null;
            }

            return _hierarchy.GetComInterface<TInterface>();
        }

        private static string GetFilePath(IntPtr punkDocData)
        {
            IPersistFileFormat vsPersist = Marshal.GetObjectForIUnknown(punkDocData) as IPersistFileFormat;
            if (vsPersist != null)
            {
                string newMkDocument;
                uint formatIndex;
                if (ErrorHandler.Succeeded(vsPersist.GetCurFile(out newMkDocument, out formatIndex)))
                {
                    return newMkDocument;
                }
            }
            return null;
        }

        private static IEnumerable<string> GetPropertyValues(IVsUserContext userContext, string propertyName)
        {
            if (userContext != null)
            {
                int count;
                if (userContext.CountAttributes(propertyName, 0, out count) == VSConstants.S_OK)
                {
                    for (int i = 0; i < count; i++)
                    {
                        string name;
                        string value;
                        if (userContext.GetAttribute(i, propertyName, 1, out name, out value) == VSConstants.S_OK)
                        {
                            yield return value;
                        }
                    }
                }
            }
        }

        #endregion Private methods
    }
}
