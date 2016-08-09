//------------------------------------------------------------------------------
// <copyright file="DockerTools2Package.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using DockerTools2.LanguageService;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;

namespace DockerTools2
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad("8BB2217D-0F2D-49D1-97BC-3654ED321F3B")]
    [ProvideLanguageService(typeof(DockerLanguageService), DockerLanguageService.LanguageName, 100, DefaultToInsertSpaces = true, EnableCommenting = true, EnableLineNumbers = true, ShowCompletion = true)]
    public sealed class DockerTools2Package : Package
    {
        /// <summary>
        /// DockerTools2Package GUID string.
        /// </summary>
        public const string PackageGuidString = "3cf38592-ae02-40eb-a11e-6f19fc4cc1e9";
        public const string ActivationGuidString = "fdcdac1d-bdc9-4e79-934f-93e0de56bb2b";

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerTools2Package"/> class.
        /// </summary>
        public DockerTools2Package()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            AddDockerSupportCommand.Initialize(this);

            var serviceContainer = this as IServiceContainer;
            var langService = new DockerLanguageService(this);
            serviceContainer.AddService(typeof(DockerLanguageService), langService, true);
        }

        #endregion

        public static void ForecePackageLoad()
        {
            var shell = (IVsShell)GetGlobalService(typeof(SVsShell));
            var guid = new Guid(PackageGuidString);
            IVsPackage package;

            if (shell.IsPackageLoaded(ref guid, out package) != VSConstants.S_OK)
                ErrorHandler.Succeeded(shell.LoadPackage(ref guid, out package));
        }
    }
}
