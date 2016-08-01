//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Text;

namespace DockerTools2
{
    internal static class NativeMethods
    {
        public static readonly Guid SID_STopLevelBrowser = new Guid("{4C96BE40-915C-11CF-99D3-00AA004AE837}");
        public static readonly Guid IID_IDispatch = new Guid("{00020400-0000-0000-C000-000000000046}");

        public const uint SVGIO_BACKGROUND = 0x00000000;

        public const int ERROR_ELEVATION_REQUIRED = unchecked((int)0x800702E4);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

        [DllImport("shlwapi.dll", EntryPoint = "PathRelativePathToW", ExactSpelling = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PathRelativePathTo([Out] StringBuilder pszPath, [In] string pszFrom, [In] uint dwAttrFrom, [In] string pszTo, [In] uint dwAttrTo);

        ///<SecurityNote>
        ///  Critical as this code performs an UnmanagedCodeSecurity elevation.
        ///</SecurityNote>
        [SecurityCritical]
        [DllImport("ole32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int CreateStreamOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease, ref IStream istream);

        [ComImport]
        [Guid("000214E2-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellBrowser
        {
            void Reserved1();  // IOleWindow::GetWindow

            void Reserved2();  // IOleWindow::ContextSensitiveHelp

            void Reserved3();  // InsertMenusSB

            void Reserved4();  // SetMenuSB

            void Reserved5();  // RemoveMenusSB

            void Reserved6();  // SetStatusTextSB

            void Reserved7();  // EnableModelessSB

            void Reserved8();  // TranslateAcceleratorSB

            void Reserved9();  // BrowseObject

            void Reserved10(); // GetViewStateStream

            void Reserved11(); // GetControlWindow

            void Reserved12(); // SendControlMsg

            void QueryActiveShellView(out IShellView ppshv);

            void Reserved13(); // OnViewWindowActive

            void Reserved14(); // SetToolbarItems
        }

        [ComImport]
        [Guid("000214E3-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellView
        {
            void Reserved1();  // IOleWindow::GetWindow

            void Reserved2();  // IOleWindow::ContextSensitiveHelp

            void Reserved3();  // TranslateAccelerator

            void Reserved4();  // EnableModeless

            void Reserved5();  // UIActivate

            void Reserved6();  // Refresh

            void Reserved7();  // CreateViewWindow

            void Reserved8();  // DestroyViewWindow

            void Reserved9();  // GetCurrentInfo

            void Reserved10(); // AddPropertySheetPages

            void Reserved11(); // SaveViewState

            void Reserved12(); // SelectItem

            [return: MarshalAs(UnmanagedType.IDispatch)]
            object GetItemObject(uint item, ref Guid riid);
        }
    }
}
