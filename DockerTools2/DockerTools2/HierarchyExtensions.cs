//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace DockerTools2
{
    internal static class HierarchyExtensions
    {
        private static readonly Guid CSharpProjectKindGuid = new Guid("fae04ec0-301f-11d3-bf4b-00c04f79efbc");

        public static bool IsCSharpProject(this Hierarchy hierarchy)
        {
            return IsProjectKind(hierarchy, CSharpProjectKindGuid);
        }

        public static string GetMSBuildProperty(this Hierarchy hierarchy,
                                                string propertyName,
                                                string configurationName = null,
                                                _PersistStorageType storageType = _PersistStorageType.PST_PROJECT_FILE)
        {
            var storage = hierarchy.GetComInterface<IVsBuildPropertyStorage>();

            string propertyValue = null;

            ErrorHandler.ThrowOnFailure(storage.GetPropertyValue(propertyName,
                                                                 configurationName,
                                                                 (uint)storageType,
                                                                 out propertyValue));

            return propertyValue;
        }

        public static void SetMSBuildProperty(this Hierarchy hierarchy,
                                              string propertyName,
                                              string propertyValue,
                                              string configurationName = null,
                                              _PersistStorageType storageType = _PersistStorageType.PST_PROJECT_FILE)
        {
            var storage = hierarchy.GetComInterface<IVsBuildPropertyStorage>();

            ErrorHandler.ThrowOnFailure(storage.SetPropertyValue(propertyName,
                                                                 configurationName,
                                                                 (uint)storageType,
                                                                 propertyValue ?? string.Empty));
        }

        public static void SetOrRemoveMSBuildProperty(this Hierarchy hierarchy,
                                                      string propertyName,
                                                      string propertyValue,
                                                      string configurationName = null,
                                                      _PersistStorageType storageType = _PersistStorageType.PST_PROJECT_FILE)
        {
            if (!string.IsNullOrEmpty(propertyValue))
            {
                hierarchy.SetMSBuildProperty(propertyName, propertyValue, configurationName, storageType);
            }
            else
            {
                var storage = hierarchy.GetComInterface<IVsBuildPropertyStorage>();

                ErrorHandler.ThrowOnFailure(storage.RemoveProperty(propertyName,
                                                                   configurationName,
                                                                   (uint)storageType));
            }
        }

        private static bool IsProjectKind(Hierarchy hierarchy, Guid targetProjectKindGuid)
        {
            Guid projectKindGuid;

            return hierarchy.DteProject != null &&
                   Guid.TryParse(hierarchy.DteProject.Kind, out projectKindGuid) &&
                   projectKindGuid == targetProjectKindGuid;
        }
    }
}
