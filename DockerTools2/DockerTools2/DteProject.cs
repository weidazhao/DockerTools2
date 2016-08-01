//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using System;

namespace DockerTools2
{
    /// <summary>
    /// A lightweight wrapper for the EnvDTE.Project type.
    /// </summary>
    internal struct DteProject : IEquatable<DteProject>
    {
        private EnvDTE.Project _dteProject;

        public static readonly DteProject Empty = new DteProject();

        public bool IsEmpty
        {
            get
            {
                return (_dteProject == null);
            }
        }

        public DteProject(EnvDTE.Project dteProject)
        {
            _dteProject = dteProject;
        }

        public EnvDTE.Project Project
        {
            get
            {
                return _dteProject;
            }
        }

        /// <summary>
        /// Returns value indicating if project is unloaded.
        /// FileName and FullName properties throw the System.NotImplementedException for unloaded projects.
        /// </summary>
        public bool IsUnloaded
        {
            get
            {
                if (!this.IsEmpty)
                {
                    return StringComparer.OrdinalIgnoreCase.Equals(_dteProject.Kind, EnvDTE.Constants.vsProjectKindUnmodeled);
                }

                return false;
            }
        }

        public string ProjectLanguage
        {
            get
            {
                if (!this.IsEmpty)
                {
                    Guid projectKind = Guid.Parse(_dteProject.Kind);

                    //Translate from base language GUIDs
                    if (projectKind == Guid.Parse(VSLangProj.PrjKind.prjKindCSharpProject))
                    {
                        return "CSharp";
                    }
                    else if (projectKind == Guid.Parse(VSLangProj.PrjKind.prjKindVBProject))
                    {
                        return "VisualBasic";
                    }
                }
                return "Unknown";
            }
        }

        #region Equality

        public bool Equals(DteProject other)
        {
            return (this.IsEmpty && other.IsEmpty) || ComUtilities.IsSameComObject(_dteProject, other._dteProject);
        }

        public override bool Equals(object obj)
        {
            if (obj is DteProject)
            {
                return this == (DteProject)obj;
            }

            return base.Equals(obj);
        }

        public static bool operator ==(DteProject item1, DteProject item2)
        {
            return item1.Equals(item2);
        }

        public static bool operator !=(DteProject item1, DteProject item2)
        {
            return !(item1 == item2);
        }

        public override int GetHashCode()
        {
            if (!this.IsEmpty)
            {
                return _dteProject.GetHashCode();
            }
            return 0;
        }

        #endregion
    }
}
