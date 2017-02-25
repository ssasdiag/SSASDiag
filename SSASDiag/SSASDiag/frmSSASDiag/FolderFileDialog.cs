using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SSASDiag
{
    public class BrowseForFolder
    {
        // Constants for sending and receiving messages in BrowseCallBackProc
        public const int WM_USER = 0x400;
        public const int BFFM_INITIALIZED = 1;
        public const int BFFM_SELCHANGED = 2;
        public const int BFFM_VALIDATEFAILEDA = 3;
        public const int BFFM_VALIDATEFAILEDW = 4;
        public const int BFFM_IUNKNOWN = 5; // provides IUnknown to client. lParam: IUnknown*
        public const int BFFM_SETSTATUSTEXTA = WM_USER + 100;
        public const int BFFM_ENABLEOK = WM_USER + 101;
        public const int BFFM_SETSELECTIONA = WM_USER + 102;
        public const int BFFM_SETSELECTIONW = WM_USER + 103;
        public const int BFFM_SETSTATUSTEXTW = WM_USER + 104;
        public const int BFFM_SETOKTEXT = WM_USER + 105; // Unicode only
        public const int BFFM_SETEXPANDED = WM_USER + 106; // Unicode only

        // Browsing for directory.
        //private uint BIF_RETURNONLYFSDIRS = 0x0001;  // For finding a folder to start document searching
        //private uint BIF_DONTGOBELOWDOMAIN = 0x0002;  // For starting the Find Computer
        //private uint BIF_STATUSTEXT = 0x0004;  // Top of the dialog has 2 lines of text for BROWSEINFO.lpszTitle and one line if
                                               // this flag is set.  Passing the message BFFM_SETSTATUSTEXTA to the hwnd can set the
                                               // rest of the text.  This is not used with BIF_USENEWUI and BROWSEINFO.lpszTitle gets
                                               // all three lines of text.
        //private uint BIF_RETURNFSANCESTORS = 0x0008;
        //private uint BIF_EDITBOX = 0x0010;   // Add an editbox to the dialog
        //private uint BIF_VALIDATE = 0x0020;   // insist on valid result (or CANCEL)

        private uint BIF_NEWDIALOGSTYLE = 0x0040;   // Use the new dialog layout with the ability to resize
                                                    // Caller needs to call OleInitialize() before using this API
        private uint BIF_USENEWUI = 0x0040 + 0x0010; //(BIF_NEWDIALOGSTYLE | BIF_EDITBOX);

        //private uint BIF_BROWSEINCLUDEURLS = 0x0080;   // Allow URLs to be displayed or entered. (Requires BIF_USENEWUI)
        //private uint BIF_UAHINT = 0x0100;   // Add a UA hint to the dialog, in place of the edit box. May not be combined with BIF_EDITBOX
        private uint BIF_NONEWFOLDERBUTTON = 0x0200;   // Do not add the "New Folder" button to the dialog.  Only applicable with BIF_NEWDIALOGSTYLE.
        //private uint BIF_NOTRANSLATETARGETS = 0x0400;  // don't traverse target as shortcut

        //private uint BIF_BROWSEFORCOMPUTER = 0x1000;  // Browsing for Computers.
        //private uint BIF_BROWSEFORPRINTER = 0x2000;// Browsing for Printers
        private uint BIF_BROWSEINCLUDEFILES = 0x4000; // Browsing for Everything
        private uint BIF_SHAREABLE = 0x8000;  // sharable resources displayed (remote shares, requires BIF_USENEWUI)

        [DllImport("shell32.dll")]
        static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

        // Note that the BROWSEINFO object's pszDisplayName only gives you the name of the folder.
        // To get the actual path, you need to parse the returned PIDL
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        // static extern uint SHGetPathFromIDList(IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] 
        //StringBuilder pszPath);
        static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

        [DllImport("user32.dll", PreserveSig = true)]
        public static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, int wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, string lParam);

        private string _initialPath;

        public delegate int BrowseCallBackProc(IntPtr hwnd, int msg, IntPtr lp, IntPtr wp);
        struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public string pszDisplayName;
            public string lpszTitle;
            public uint ulFlags;
            public BrowseCallBackProc lpfn;
            public IntPtr lParam;
            public int iImage;
        }
        public int OnBrowseEvent(IntPtr hWnd, int msg, IntPtr lp, IntPtr lpData)
        {
            switch (msg)
            {
                case BFFM_INITIALIZED: // Required to set initialPath
                    {
                        //Win32.SendMessage(new HandleRef(null, hWnd), BFFM_SETSELECTIONA, 1, lpData);
                        // Use BFFM_SETSELECTIONW if passing a Unicode string, i.e. native CLR Strings.
                        SendMessage(new HandleRef(null, hWnd), BFFM_SETSELECTIONW, 1, _initialPath);
                        break;
                    }
                case BFFM_SELCHANGED:
                    {
                        IntPtr pathPtr = Marshal.AllocHGlobal((int)(260 * Marshal.SystemDefaultCharSize));
                        if (SHGetPathFromIDList(lp, pathPtr))
                            SendMessage(new HandleRef(null, hWnd), BFFM_SETSTATUSTEXTW, 0, pathPtr);
                        Marshal.FreeHGlobal(pathPtr);
                        break;
                    }
                case BFFM_IUNKNOWN:
                    {
                        IntPtr p = IntPtr.Zero;
                        // get filter interface
                        Guid g = new Guid("{C0A651F5-B48B-11d2-B5ED-006097C686F6}");
                        if (lp != IntPtr.Zero)
                            Marshal.QueryInterface(lp, ref g, out p);
                        if (p != IntPtr.Zero)
                        {
                            Object obj = System.Runtime.InteropServices.Marshal.GetTypedObjectForIUnknown(
                            p,
                            GetFolderFilterSiteType());
                            IFolderFilterSite folderFilterSite = (IFolderFilterSite)obj;
                            FilterByExtension filter = new FilterByExtension();
                            filter.ValidExtension = _filters.ToArray();
                            folderFilterSite.SetFilter(filter);
                        }
                        break;
                    }
            }

            return 0;
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("9CC22886-DC8E-11d2-B1D0-00C04F8EEB3E")]
        public interface IFolderFilter
        {
            // Allows a client to specify which individual items should be enumerated.
            // Note: The host calls this method for each item in the folder. Return S_OK, to have the item enumerated. 
            // Return S_FALSE to prevent the item from being enumerated.
            [PreserveSig]
            Int32 ShouldShow(
                [MarshalAs(UnmanagedType.Interface)]Object psf,             // A pointer to the folder's IShellFolder interface.
                IntPtr pidlFolder,      // The folder's PIDL.
                IntPtr pidlItem);       // The item's PIDL.

            // Allows a client to specify which classes of objects in a Shell folder should be enumerated.
            [PreserveSig]
            Int32 GetEnumFlags(
                [MarshalAs(UnmanagedType.Interface)]Object psf,             // A pointer to the folder's IShellFolder interface.
                IntPtr pidlFolder,      // The folder's PIDL.
                IntPtr phwnd,           // A pointer to the host's window handle.
                out UInt32 pgrfFlags);  // One or more SHCONTF values that specify which classes of objects to enumerate.

        };

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214E6-0000-0000-C000-000000000046")]
        public interface IShellFolder
        {
            // Translates a file object's or folder's display name into an item identifier list.
            // Return value: error code, if any
            [PreserveSig]
            Int32 ParseDisplayName(
                IntPtr hwnd,                // Optional window handle
                IntPtr pbc,                 // Optional bind context that controls the parsing operation. This parameter is 
                                            // normally set to NULL. 
                [MarshalAs(UnmanagedType.LPWStr)]
            String pszDisplayName,      // Null-terminated UNICODE string with the display name.
                ref UInt32 pchEaten,        // Pointer to a ULONG value that receives the number of characters of the 
                                            // display name that was parsed.
                out IntPtr ppidl,           // Pointer to an ITEMIDLIST pointer that receives the item identifier list for 
                                            // the object.
                ref UInt32 pdwAttributes);  // Optional parameter that can be used to query for file attributes.
                                            // this can be values from the SFGAO enum

            // Allows a client to determine the contents of a folder by creating an item identifier enumeration object 
            // and returning its IEnumIDList interface.
            // Return value: error code, if any
            [PreserveSig]
            Int32 EnumObjects(
                IntPtr hwnd,                // If user input is required to perform the enumeration, this window handle 
                                            // should be used by the enumeration object as the parent window to take 
                                            // user input.
                Int32 grfFlags,             // Flags indicating which items to include in the enumeration. For a list 
                                            // of possible values, see the SHCONTF enum. 
                out IntPtr ppenumIDList);   // Address that receives a pointer to the IEnumIDList interface of the 
                                            // enumeration object created by this method. 

            // Retrieves an IShellFolder object for a subfolder.
            // Return value: error code, if any
            [PreserveSig]
            Int32 BindToObject(
                IntPtr pidl,                // Address of an ITEMIDLIST structure (PIDL) that identifies the subfolder.
                IntPtr pbc,                 // Optional address of an IBindCtx interface on a bind context object to be 
                                            // used during this operation.
                Guid riid,                  // Identifier of the interface to return. 
                out IntPtr ppv);            // Address that receives the interface pointer.

            // Requests a pointer to an object's storage interface. 
            // Return value: error code, if any
            [PreserveSig]
            Int32 BindToStorage(
                IntPtr pidl,                // Address of an ITEMIDLIST structure that identifies the subfolder relative 
                                            // to its parent folder. 
                IntPtr pbc,                 // Optional address of an IBindCtx interface on a bind context object to be 
                                            // used during this operation.
                Guid riid,                  // Interface identifier (IID) of the requested storage interface.
                out IntPtr ppv);            // Address that receives the interface pointer specified by riid.

            // Determines the relative order of two file objects or folders, given their item identifier lists.
            // Return value: If this method is successful, the CODE field of the HRESULT contains one of the following 
            // values (the code can be retrived using the helper function GetHResultCode):
            // Negative A negative return value indicates that the first item should precede the second (pidl1 < pidl2). 
            // Positive A positive return value indicates that the first item should follow the second (pidl1 > pidl2). 
            // Zero A return value of zero indicates that the two items are the same (pidl1 = pidl2). 
            [PreserveSig]
            Int32 CompareIDs(
                Int32 lParam,               // Value that specifies how the comparison should be performed. The lower 
                                            // sixteen bits of lParam define the sorting rule. The upper sixteen bits of 
                                            // lParam are used for flags that modify the sorting rule. values can be from 
                                            // the SHCIDS enum
                IntPtr pidl1,               // Pointer to the first item's ITEMIDLIST structure.
                IntPtr pidl2);              // Pointer to the second item's ITEMIDLIST structure.

            // Requests an object that can be used to obtain information from or interact with a folder object.
            // Return value: error code, if any
            [PreserveSig]
            Int32 CreateViewObject(
                IntPtr hwndOwner,           // Handle to the owner window.
                Guid riid,                  // Identifier of the requested interface. 
                out IntPtr ppv);            // Address of a pointer to the requested interface. 

            // Retrieves the attributes of one or more file objects or subfolders. 
            // Return value: error code, if any
            [PreserveSig]
            Int32 GetAttributesOf(
                UInt32 cidl,                // Number of file objects from which to retrieve attributes. 

                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]
            IntPtr[] apidl,             // Address of an array of pointers to ITEMIDLIST structures, each of which 
                                        // uniquely identifies a file object relative to the parent folder.
                ref UInt32 rgfInOut);       // Address of a single ULONG value that, on entry, contains the attributes that 
                                            // the caller is requesting. On exit, this value contains the requested 
                                            // attributes that are common to all of the specified objects. this value can
                                            // be from the SFGAO enum

            // Retrieves an OLE interface that can be used to carry out actions on the specified file objects or folders.
            // Return value: error code, if any
            [PreserveSig]
            Int32 GetUIObjectOf(
                IntPtr hwndOwner,           // Handle to the owner window that the client should specify if it displays 
                                            // a dialog box or message box.
                UInt32 cidl,                // Number of file objects or subfolders specified in the apidl parameter. 
                IntPtr[] apidl,             // Address of an array of pointers to ITEMIDLIST structures, each of which 
                                            // uniquely identifies a file object or subfolder relative to the parent folder.
                Guid riid,                  // Identifier of the COM interface object to return.
                ref UInt32 rgfReserved,     // Reserved. 
                out IntPtr ppv);            // Pointer to the requested interface.

            // Retrieves the display name for the specified file object or subfolder. 
            // Return value: error code, if any
            [PreserveSig]
            Int32 GetDisplayNameOf(
                IntPtr pidl,                // Address of an ITEMIDLIST structure (PIDL) that uniquely identifies the file 
                                            // object or subfolder relative to the parent folder. 
                UInt32 uFlags,              // Flags used to request the type of display name to return. For a list of 
                                            // possible values, see the SHGNO enum. 
                out ShellApi.STRRET pName);         // Address of a STRRET structure in which to return the display name.

            // Sets the display name of a file object or subfolder, changing the item identifier in the process.
            // Return value: error code, if any
            [PreserveSig]
            Int32 SetNameOf(
                IntPtr hwnd,                // Handle to the owner window of any dialog or message boxes that the client 
                                            // displays.
                IntPtr pidl,                // Pointer to an ITEMIDLIST structure that uniquely identifies the file object
                                            // or subfolder relative to the parent folder. 
                [MarshalAs(UnmanagedType.LPWStr)]
            String pszName,             // Pointer to a null-terminated string that specifies the new display name. 
                UInt32 uFlags,              // Flags indicating the type of name specified by the lpszName parameter. For 
                                            // a list of possible values, see the description of the SHGNO enum. 
                out IntPtr ppidlOut);       // Address of a pointer to an ITEMIDLIST structure which receives the new ITEMIDLIST. 
        }

        public class ShellApi
        {
            public delegate Int32 BrowseCallbackProc(IntPtr hwnd, UInt32 uMsg, Int32 lParam, Int32 lpData);

            // Contains parameters for the SHBrowseForFolder function and receives information about the folder selected 
            // by the user.
            [StructLayout(LayoutKind.Sequential)]
            public struct BROWSEINFO
            {
                public IntPtr hwndOwner;                // Handle to the owner window for the dialog box.

                public IntPtr pidlRoot;                 // Pointer to an item identifier list (PIDL) specifying the 
                                                        // location of the root folder from which to start browsing.

                [MarshalAs(UnmanagedType.LPStr)]        // Address of a buffer to receive the display name of the 
                public String pszDisplayName;           // folder selected by the user.

                [MarshalAs(UnmanagedType.LPStr)]        // Address of a null-terminated string that is displayed 
                public String lpszTitle;                // above the tree view control in the dialog box.

                public UInt32 ulFlags;                  // Flags specifying the options for the dialog box. 

                [MarshalAs(UnmanagedType.FunctionPtr)]  // Address of an application-defined function that the 
                public BrowseCallbackProc lpfn;                 // dialog box calls when an event occurs.

                public Int32 lParam;                    // Application-defined value that the dialog box passes to 
                                                        // the callback function

                public Int32 iImage;                    // Variable to receive the image associated with the selected folder.
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct STRRET
            {
                [FieldOffset(0)]
                public UInt32 uType;                        // One of the STRRET_* values

                [FieldOffset(4)]
                public IntPtr pOleStr;                      // must be freed by caller of GetDisplayNameOf

                [FieldOffset(4)]
                public IntPtr pStr;                         // NOT USED

                [FieldOffset(4)]
                public UInt32 uOffset;                      // Offset into SHITEMID

                [FieldOffset(4)]
                public IntPtr cStr;                         // Buffer to fill in (ANSI)
            }

            // Retrieves a pointer to the Shell's IMalloc interface.
            [DllImport("shell32.dll")]
            public static extern Int32 SHGetMalloc(
                out IntPtr hObject);    // Address of a pointer that receives the Shell's IMalloc interface pointer. 

            // Retrieves the path of a folder as an PIDL.
            [DllImport("shell32.dll")]
            public static extern Int32 SHGetFolderLocation(
                IntPtr hwndOwner,       // Handle to the owner window.
                Int32 nFolder,          // A CSIDL value that identifies the folder to be located.
                IntPtr hToken,          // Token that can be used to represent a particular user.
                UInt32 dwReserved,      // Reserved.
                out IntPtr ppidl);      // Address of a pointer to an item identifier list structure 
                                        // specifying the folder's location relative to the root of the namespace 
                                        // (the desktop). 

            // Converts an item identifier list to a file system path. 
            [DllImport("shell32.dll")]
            public static extern Int32 SHGetPathFromIDList(
                IntPtr pidl,            // Address of an item identifier list that specifies a file or directory location 
                                        // relative to the root of the namespace (the desktop). 
                StringBuilder pszPath); // Address of a buffer to receive the file system path.


            // Takes the CSIDL of a folder and returns the pathname.
            [DllImport("shell32.dll")]
            public static extern Int32 SHGetFolderPath(
                IntPtr hwndOwner,           // Handle to an owner window.
                Int32 nFolder,              // A CSIDL value that identifies the folder whose path is to be retrieved.
                IntPtr hToken,              // An access token that can be used to represent a particular user.
                UInt32 dwFlags,             // Flags to specify which path is to be returned. It is used for cases where 
                                            // the folder associated with a CSIDL may be moved or renamed by the user. 
                StringBuilder pszPath);     // Pointer to a null-terminated string which will receive the path.

            // Translates a Shell namespace object's display name into an item identifier list and returns the attributes 
            // of the object. This function is the preferred method to convert a string to a pointer to an item 
            // identifier list (PIDL). 
            [DllImport("shell32.dll")]
            public static extern Int32 SHParseDisplayName(
                [MarshalAs(UnmanagedType.LPWStr)]
            String pszName,             // Pointer to a zero-terminated wide string that contains the display name 
                                        // to parse. 
                IntPtr pbc,                 // Optional bind context that controls the parsing operation. This parameter 
                                            // is normally set to NULL.
                out IntPtr ppidl,           // Address of a pointer to a variable of type ITEMIDLIST that receives the item
                                            // identifier list for the object.
                UInt32 sfgaoIn,             // ULONG value that specifies the attributes to query.
                out UInt32 psfgaoOut);      // Pointer to a ULONG. On return, those attributes that are true for the 
                                            // object and were requested in sfgaoIn will be set. 


            // Retrieves the IShellFolder interface for the desktop folder, which is the root of the Shell's namespace. 
            [DllImport("shell32.dll")]
            public static extern Int32 SHGetDesktopFolder(
                out IntPtr ppshf);          // Address that receives an IShellFolder interface pointer for the 
                                            // desktop folder.

            // This function takes the fully-qualified pointer to an item identifier list (PIDL) of a namespace object, 
            // and returns a specified interface pointer on the parent object.
            [DllImport("shell32.dll")]
            public static extern Int32 SHBindToParent(
                IntPtr pidl,            // The item's PIDL. 
                [MarshalAs(UnmanagedType.LPStruct)]
            Guid riid,              // The REFIID of one of the interfaces exposed by the item's parent object. 
                out IntPtr ppv,         // A pointer to the interface specified by riid. You must release the object when 
                                        // you are finished. 
                ref IntPtr ppidlLast);  // The item's PIDL relative to the parent folder. This PIDL can be used with many
                                        // of the methods supported by the parent folder's interfaces. If you set ppidlLast 
                                        // to NULL, the PIDL will not be returned. 

            // Accepts a STRRET structure returned by IShellFolder::GetDisplayNameOf that contains or points to a 
            // string, and then returns that string as a BSTR.
            [DllImport("shlwapi.dll")]
            public static extern Int32 StrRetToBSTR(
                ref STRRET pstr,        // Pointer to a STRRET structure.
                IntPtr pidl,            // Pointer to an ITEMIDLIST uniquely identifying a file object or subfolder relative
                                        // to the parent folder.
                [MarshalAs(UnmanagedType.BStr)]
            out String pbstr);      // Pointer to a variable of type BSTR that contains the converted string.

            // Takes a STRRET structure returned by IShellFolder::GetDisplayNameOf, converts it to a string, and 
            // places the result in a buffer. 
            [DllImport("shlwapi.dll")]
            public static extern Int32 StrRetToBuf(
                ref STRRET pstr,        // Pointer to the STRRET structure. When the function returns, this pointer will no
                                        // longer be valid.
                IntPtr pidl,            // Pointer to the item's ITEMIDLIST structure.
                StringBuilder pszBuf,   // Buffer to hold the display name. It will be returned as a null-terminated
                                        // string. If cchBuf is too small, the name will be truncated to fit. 
                UInt32 cchBuf);         // Size of pszBuf, in characters. If cchBuf is too small, the string will be 
                                        // truncated to fit. 



            // Displays a dialog box that enables the user to select a Shell folder. 
            [DllImport("shell32.dll")]
            public static extern IntPtr SHBrowseForFolder(
                ref BROWSEINFO lbpi);   // Pointer to a BROWSEINFO structure that contains information used to display 
                                        // the dialog box. 

            public enum CSIDL
            {
                CSIDL_FLAG_CREATE = (0x8000),   // Version 5.0. Combine this CSIDL with any of the following 
                                                //CSIDLs to force the creation of the associated folder. 
                CSIDL_ADMINTOOLS = (0x0030),    // Version 5.0. The file system directory that is used to store 
                                                // administrative tools for an individual user. The Microsoft 
                                                // Management Console (MMC) will save customized consoles to 
                                                // this directory, and it will roam with the user.
                CSIDL_ALTSTARTUP = (0x001d),    // The file system directory that corresponds to the user's 
                                                // nonlocalized Startup program group.
                CSIDL_APPDATA = (0x001a),   // Version 4.71. The file system directory that serves as a 
                                            // common repository for application-specific data. A typical
                                            // path is C:\Documents and Settings\username\Application Data. 
                                            // This CSIDL is supported by the redistributable Shfolder.dll 
                                            // for systems that do not have the Microsoft® Internet 
                                            // Explorer 4.0 integrated Shell installed.
                CSIDL_BITBUCKET = (0x000a), // The virtual folder containing the objects in the user's 
                                            // Recycle Bin.
                CSIDL_CDBURN_AREA = (0x003b),   // Version 6.0. The file system directory acting as a staging
                                                // area for files waiting to be written to CD. A typical path 
                                                // is C:\Documents and Settings\username\Local Settings\
                                                // Application Data\Microsoft\CD Burning.
                CSIDL_COMMON_ADMINTOOLS = (0x002f), // Version 5.0. The file system directory containing 
                                                    // administrative tools for all users of the computer.
                CSIDL_COMMON_ALTSTARTUP = (0x001e), // The file system directory that corresponds to the 
                                                    // nonlocalized Startup program group for all users. Valid only 
                                                    // for Microsoft Windows NT® systems.
                CSIDL_COMMON_APPDATA = (0x0023), // Version 5.0. The file system directory containing application 
                                                 // data for all users. A typical path is C:\Documents and 
                                                 // Settings\All Users\Application Data.
                CSIDL_COMMON_DESKTOPDIRECTORY = (0x0019), // The file system directory that contains files and folders 
                                                          // that appear on the desktop for all users. A typical path is 
                                                          // C:\Documents and Settings\All Users\Desktop. Valid only for 
                                                          // Windows NT systems.
                CSIDL_COMMON_DOCUMENTS = (0x002e), // The file system directory that contains documents that are 
                                                   // common to all users. A typical paths is C:\Documents and 
                                                   // Settings\All Users\Documents. Valid for Windows NT systems 
                                                   // and Microsoft Windows® 95 and Windows 98 systems with 
                                                   // Shfolder.dll installed.
                CSIDL_COMMON_FAVORITES = (0x001f), // The file system directory that serves as a common repository
                                                   // for favorite items common to all users. Valid only for 
                                                   // Windows NT systems.
                CSIDL_COMMON_MUSIC = (0x0035), // Version 6.0. The file system directory that serves as a 
                                               // repository for music files common to all users. A typical 
                                               // path is C:\Documents and Settings\All Users\Documents\
                                               // My Music.
                CSIDL_COMMON_PICTURES = (0x0036), // Version 6.0. The file system directory that serves as a 
                                                  // repository for image files common to all users. A typical 
                                                  // path is C:\Documents and Settings\All Users\Documents\
                                                  // My Pictures.
                CSIDL_COMMON_PROGRAMS = (0x0017), // The file system directory that contains the directories for 
                                                  // the common program groups that appear on the Start menu for
                                                  // all users. A typical path is C:\Documents and Settings\
                                                  // All Users\Start Menu\Programs. Valid only for Windows NT 
                                                  // systems.
                CSIDL_COMMON_STARTMENU = (0x0016), // The file system directory that contains the programs and 
                                                   // folders that appear on the Start menu for all users. A 
                                                   // typical path is C:\Documents and Settings\All Users\
                                                   // Start Menu. Valid only for Windows NT systems.
                CSIDL_COMMON_STARTUP = (0x0018), // The file system directory that contains the programs that 
                                                 // appear in the Startup folder for all users. A typical path 
                                                 // is C:\Documents and Settings\All Users\Start Menu\Programs\
                                                 // Startup. Valid only for Windows NT systems.
                CSIDL_COMMON_TEMPLATES = (0x002d), // The file system directory that contains the templates that 
                                                   // are available to all users. A typical path is C:\Documents 
                                                   // and Settings\All Users\Templates. Valid only for Windows 
                                                   // NT systems.
                CSIDL_COMMON_VIDEO = (0x0037), // Version 6.0. The file system directory that serves as a 
                                               // repository for video files common to all users. A typical 
                                               // path is C:\Documents and Settings\All Users\Documents\
                                               // My Videos.
                CSIDL_CONTROLS = (0x0003), // The virtual folder containing icons for the Control Panel 
                                           // applications.
                CSIDL_COOKIES = (0x0021), // The file system directory that serves as a common repository 
                                          // for Internet cookies. A typical path is C:\Documents and 
                                          // Settings\username\Cookies.
                CSIDL_DESKTOP = (0x0000), // The virtual folder representing the Windows desktop, the root 
                                          // of the namespace.
                CSIDL_DESKTOPDIRECTORY = (0x0010), // The file system directory used to physically store file 
                                                   // objects on the desktop (not to be confused with the desktop 
                                                   // folder itself). A typical path is C:\Documents and 
                                                   // Settings\username\Desktop.
                CSIDL_DRIVES = (0x0011), // The virtual folder representing My Computer, containing 
                                         // everything on the local computer: storage devices, printers,
                                         // and Control Panel. The folder may also contain mapped 
                                         // network drives.
                CSIDL_FAVORITES = (0x0006), // The file system directory that serves as a common repository 
                                            // for the user's favorite items. A typical path is C:\Documents
                                            // and Settings\username\Favorites.
                CSIDL_FONTS = (0x0014), // A virtual folder containing fonts. A typical path is 
                                        // C:\Windows\Fonts.
                CSIDL_HISTORY = (0x0022), // The file system directory that serves as a common repository
                                          // for Internet history items.
                CSIDL_INTERNET = (0x0001), // A virtual folder representing the Internet.
                CSIDL_INTERNET_CACHE = (0x0020), // Version 4.72. The file system directory that serves as a 
                                                 // common repository for temporary Internet files. A typical 
                                                 // path is C:\Documents and Settings\username\Local Settings\
                                                 // Temporary Internet Files.
                CSIDL_LOCAL_APPDATA = (0x001c), // Version 5.0. The file system directory that serves as a data
                                                // repository for local (nonroaming) applications. A typical 
                                                // path is C:\Documents and Settings\username\Local Settings\
                                                // Application Data.
                CSIDL_MYDOCUMENTS = (0x000c), // Version 6.0. The virtual folder representing the My Documents
                                              // desktop item. This should not be confused with 
                                              // CSIDL_PERSONAL, which represents the file system folder that 
                                              // physically stores the documents.
                CSIDL_MYMUSIC = (0x000d), // The file system directory that serves as a common repository 
                                          // for music files. A typical path is C:\Documents and Settings
                                          // \User\My Documents\My Music.
                CSIDL_MYPICTURES = (0x0027), // Version 5.0. The file system directory that serves as a 
                                             // common repository for image files. A typical path is 
                                             // C:\Documents and Settings\username\My Documents\My Pictures.
                CSIDL_MYVIDEO = (0x000e), // Version 6.0. The file system directory that serves as a 
                                          // common repository for video files. A typical path is 
                                          // C:\Documents and Settings\username\My Documents\My Videos.
                CSIDL_NETHOOD = (0x0013), // A file system directory containing the link objects that may 
                                          // exist in the My Network Places virtual folder. It is not the
                                          // same as CSIDL_NETWORK, which represents the network namespace
                                          // root. A typical path is C:\Documents and Settings\username\
                                          // NetHood.
                CSIDL_NETWORK = (0x0012), // A virtual folder representing Network Neighborhood, the root
                                          // of the network namespace hierarchy.
                CSIDL_PERSONAL = (0x0005), // The file system directory used to physically store a user's
                                           // common repository of documents. A typical path is 
                                           // C:\Documents and Settings\username\My Documents. This should
                                           // be distinguished from the virtual My Documents folder in 
                                           // the namespace, identified by CSIDL_MYDOCUMENTS. 
                CSIDL_PRINTERS = (0x0004), // The virtual folder containing installed printers.
                CSIDL_PRINTHOOD = (0x001b), // The file system directory that contains the link objects that
                                            // can exist in the Printers virtual folder. A typical path is 
                                            // C:\Documents and Settings\username\PrintHood.
                CSIDL_PROFILE = (0x0028), // Version 5.0. The user's profile folder. A typical path is 
                                          // C:\Documents and Settings\username. Applications should not 
                                          // create files or folders at this level; they should put their
                                          // data under the locations referred to by CSIDL_APPDATA or
                                          // CSIDL_LOCAL_APPDATA.
                CSIDL_PROFILES = (0x003e), // Version 6.0. The file system directory containing user 
                                           // profile folders. A typical path is C:\Documents and Settings.
                CSIDL_PROGRAM_FILES = (0x0026), // Version 5.0. The Program Files folder. A typical path is 
                                                // C:\Program Files.
                CSIDL_PROGRAM_FILES_COMMON = (0x002b), // Version 5.0. A folder for components that are shared across 
                                                       // applications. A typical path is C:\Program Files\Common. 
                                                       // Valid only for Windows NT, Windows 2000, and Windows XP 
                                                       // systems. Not valid for Windows Millennium Edition 
                                                       // (Windows Me).
                CSIDL_PROGRAMS = (0x0002), // The file system directory that contains the user's program 
                                           // groups (which are themselves file system directories).
                                           // A typical path is C:\Documents and Settings\username\
                                           // Start Menu\Programs. 
                CSIDL_RECENT = (0x0008), // The file system directory that contains shortcuts to the 
                                         // user's most recently used documents. A typical path is 
                                         // C:\Documents and Settings\username\My Recent Documents. 
                                         // To create a shortcut in this folder, use SHAddToRecentDocs.
                                         // In addition to creating the shortcut, this function updates
                                         // the Shell's list of recent documents and adds the shortcut 
                                         // to the My Recent Documents submenu of the Start menu.
                CSIDL_SENDTO = (0x0009), // The file system directory that contains Send To menu items.
                                         // A typical path is C:\Documents and Settings\username\SendTo.
                CSIDL_STARTMENU = (0x000b), // The file system directory containing Start menu items. A 
                                            // typical path is C:\Documents and Settings\username\Start Menu.
                CSIDL_STARTUP = (0x0007), // The file system directory that corresponds to the user's 
                                          // Startup program group. The system starts these programs 
                                          // whenever any user logs onto Windows NT or starts Windows 95.
                                          // A typical path is C:\Documents and Settings\username\
                                          // Start Menu\Programs\Startup.
                CSIDL_SYSTEM = (0x0025), // Version 5.0. The Windows System folder. A typical path is 
                                         // C:\Windows\System32.
                CSIDL_TEMPLATES = (0x0015), // The file system directory that serves as a common repository
                                            // for document templates. A typical path is C:\Documents 
                                            // and Settings\username\Templates.
                CSIDL_WINDOWS = (0x0024), // Version 5.0. The Windows directory or SYSROOT. This 
                                          // corresponds to the %windir% or %SYSTEMROOT% environment 
                                          // variables. A typical path is C:\Windows.
            }

            public enum SHGFP_TYPE
            {
                SHGFP_TYPE_CURRENT = 0,     // current value for user, verify it exists
                SHGFP_TYPE_DEFAULT = 1      // default value, may not exist
            }

            public enum SFGAO : uint
            {
                SFGAO_CANCOPY = 0x00000001, // Objects can be copied    
                SFGAO_CANMOVE = 0x00000002, // Objects can be moved     
                SFGAO_CANLINK = 0x00000004, // Objects can be linked    
                SFGAO_STORAGE = 0x00000008,   // supports BindToObject(IID_IStorage)
                SFGAO_CANRENAME = 0x00000010,   // Objects can be renamed
                SFGAO_CANDELETE = 0x00000020,   // Objects can be deleted
                SFGAO_HASPROPSHEET = 0x00000040,   // Objects have property sheets
                SFGAO_DROPTARGET = 0x00000100,   // Objects are drop target
                SFGAO_CAPABILITYMASK = 0x00000177,  // This flag is a mask for the capability flags.
                SFGAO_ENCRYPTED = 0x00002000,   // object is encrypted (use alt color)
                SFGAO_ISSLOW = 0x00004000,   // 'slow' object
                SFGAO_GHOSTED = 0x00008000,   // ghosted icon
                SFGAO_LINK = 0x00010000,   // Shortcut (link)
                SFGAO_SHARE = 0x00020000,   // shared
                SFGAO_READONLY = 0x00040000,   // read-only
                SFGAO_HIDDEN = 0x00080000,   // hidden object
                SFGAO_DISPLAYATTRMASK = 0x000FC000, // This flag is a mask for the display attributes.
                SFGAO_FILESYSANCESTOR = 0x10000000,   // may contain children with SFGAO_FILESYSTEM
                SFGAO_FOLDER = 0x20000000,   // support BindToObject(IID_IShellFolder)
                SFGAO_FILESYSTEM = 0x40000000,   // is a win32 file system object (file/folder/root)
                SFGAO_HASSUBFOLDER = 0x80000000,   // may contain children with SFGAO_FOLDER
                SFGAO_CONTENTSMASK = 0x80000000,    // This flag is a mask for the contents attributes.
                SFGAO_VALIDATE = 0x01000000,   // invalidate cached information
                SFGAO_REMOVABLE = 0x02000000,   // is this removeable media?
                SFGAO_COMPRESSED = 0x04000000,   // Object is compressed (use alt color)
                SFGAO_BROWSABLE = 0x08000000,   // supports IShellFolder, but only implements CreateViewObject() (non-folder view)
                SFGAO_NONENUMERATED = 0x00100000,   // is a non-enumerated object
                SFGAO_NEWCONTENT = 0x00200000,   // should show bold in explorer tree
                SFGAO_CANMONIKER = 0x00400000,   // defunct
                SFGAO_HASSTORAGE = 0x00400000,   // defunct
                SFGAO_STREAM = 0x00400000,   // supports BindToObject(IID_IStream)
                SFGAO_STORAGEANCESTOR = 0x00800000,   // may contain children with SFGAO_STORAGE or SFGAO_STREAM
                SFGAO_STORAGECAPMASK = 0x70C50008    // for determining storage capabilities, ie for open/save semantics

            }

            public enum SHCONTF
            {
                SHCONTF_FOLDERS = 0x0020,   // only want folders enumerated (SFGAO_FOLDER)
                SHCONTF_NONFOLDERS = 0x0040,   // include non folders
                SHCONTF_INCLUDEHIDDEN = 0x0080,   // show items normally hidden
                SHCONTF_INIT_ON_FIRST_NEXT = 0x0100,   // allow EnumObject() to return before validating enum
                SHCONTF_NETPRINTERSRCH = 0x0200,   // hint that client is looking for printers
                SHCONTF_SHAREABLE = 0x0400,   // hint that client is looking sharable resources (remote shares)
                SHCONTF_STORAGE = 0x0800,   // include all items with accessible storage and their ancestors
            }

            public enum SHCIDS : uint
            {
                SHCIDS_ALLFIELDS = 0x80000000,  // Compare all the information contained in the ITEMIDLIST 
                                                // structure, not just the display names
                SHCIDS_CANONICALONLY = 0x10000000,  // When comparing by name, compare the system names but not the 
                                                    // display names. 
                SHCIDS_BITMASK = 0xFFFF0000,
                SHCIDS_COLUMNMASK = 0x0000FFFF
            }

            public enum SHGNO
            {
                SHGDN_NORMAL = 0x0000,      // default (display purpose)
                SHGDN_INFOLDER = 0x0001,        // displayed under a folder (relative)
                SHGDN_FOREDITING = 0x1000,      // for in-place editing
                SHGDN_FORADDRESSBAR = 0x4000,       // UI friendly parsing name (remove ugly stuff)
                SHGDN_FORPARSING = 0x8000       // parsing name for ParseDisplayName()
            }

            public enum STRRET_TYPE
            {
                STRRET_WSTR = 0x0000,           // Use STRRET.pOleStr
                STRRET_OFFSET = 0x0001,         // Use STRRET.uOffset to Ansi
                STRRET_CSTR = 0x0002            // Use STRRET.cStr
            }

            public static Int16 GetHResultCode(Int32 hr)
            {
                hr = hr & 0x0000ffff;
                return (Int16)hr;
            }

        }

        [ComVisible(true)]
        [Guid("3766C955-DA6F-4fbc-AD36-311E342EF180")]
        public class FilterByExtension : IFolderFilter
        {
            // Allows a client to specify which individual items should be enumerated.
            // Note: The host calls this method for each item in the folder. Return S_OK, to have the item enumerated. 
            // Return S_FALSE to prevent the item from being enumerated.
            public Int32 ShouldShow(
                Object psf,             // A pointer to the folder's IShellFolder interface.
                IntPtr pidlFolder,      // The folder's PIDL.
                IntPtr pidlItem)        // The item's PIDL.
            {
                // check extension, and if not ok return 1 (S_FALSE)

                // get display name of item
                IShellFolder isf = (IShellFolder)psf;

                ShellApi.STRRET ptrDisplayName;
                isf.GetDisplayNameOf(pidlItem, (uint)ShellApi.SHGNO.SHGDN_NORMAL | (uint)ShellApi.SHGNO.SHGDN_FORPARSING, out ptrDisplayName);

                String sDisplay;
                ShellApi.StrRetToBSTR(ref ptrDisplayName, (IntPtr)0, out sDisplay);

                // check if item is file or folder
                IntPtr[] aPidl = new IntPtr[1];
                aPidl[0] = pidlItem;
                uint Attrib;
                Attrib = (uint)ShellApi.SFGAO.SFGAO_FOLDER;

                int temp;
                temp = isf.GetAttributesOf(1, aPidl, ref Attrib);

                // if item is a folder, accept
                if ((Attrib & (uint)ShellApi.SFGAO.SFGAO_FOLDER) == (uint)ShellApi.SFGAO.SFGAO_FOLDER)
                    return 0;

                // if item is file, check if it has a valid extension
                for (int i = 0; i < ValidExtension.Length; i++)
                {
                    if (sDisplay.ToUpper().EndsWith("." + ValidExtension[i].ToUpper()))
                        return 0;
                }

                return 1;
            }

            // Allows a client to specify which classes of objects in a Shell folder should be enumerated.
            public Int32 GetEnumFlags(
                Object psf,             // A pointer to the folder's IShellFolder interface.
                IntPtr pidlFolder,      // The folder's PIDL.
                IntPtr phwnd,           // A pointer to the host's window handle.
                out UInt32 pgrfFlags)   // One or more SHCONTF values that specify which classes of objects to enumerate.
            {
                pgrfFlags = (uint)ShellApi.SHCONTF.SHCONTF_FOLDERS | (uint)ShellApi.SHCONTF.SHCONTF_NONFOLDERS;
                return 0;
            }

            public string[] ValidExtension;
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("C0A651F5-B48B-11d2-B5ED-006097C686F6")]
        public interface IFolderFilterSite
        {
            // Exposed by a host to allow clients to pass the host their IUnknown interface pointers.
            [PreserveSig]
            Int32 SetFilter(
                [MarshalAs(UnmanagedType.Interface)]Object punk);       // A pointer to the client's IUnknown interface. To notify the host to terminate 
                                                                        // filtering and stop calling your IFolderFilter interface, set this parameter to NULL. 
        }

        public static Type GetFolderFilterSiteType()
        {
            System.Type folderFilterSiteType = typeof(IFolderFilterSite);
            return folderFilterSiteType;
        }

        private List<string> _filters = new List<string>();
        public List<string> Filters
        {
            get { return _filters; }
            set { _filters = value;  }
        }

        public string SelectFolder(string caption, string initialPath, IntPtr parentHandle)
        {
            _initialPath = initialPath;
            StringBuilder sb = new StringBuilder(256);
            IntPtr bufferAddress = Marshal.AllocHGlobal(256); ;
            IntPtr pidl = IntPtr.Zero;
            BROWSEINFO bi = new BROWSEINFO();
            bi.hwndOwner = parentHandle;
            bi.pidlRoot = IntPtr.Zero;
            bi.lpszTitle = caption;
            bi.ulFlags = BIF_NEWDIALOGSTYLE | BIF_SHAREABLE | BIF_BROWSEINCLUDEFILES | BIF_USENEWUI | BIF_NONEWFOLDERBUTTON;
            bi.lpfn = new BrowseCallBackProc(OnBrowseEvent);
            bi.lParam = IntPtr.Zero;
            bi.iImage = 0;

            try
            {
                pidl = SHBrowseForFolder(ref bi);
                if (true != SHGetPathFromIDList(pidl, bufferAddress))
                {
                    return null;
                }
                sb.Append(Marshal.PtrToStringAuto(bufferAddress));
            }
            finally
            {
                // Caller is responsible for freeing this memory.
                Marshal.FreeCoTaskMem(pidl);
            }

            return sb.ToString();
        }
    }

}