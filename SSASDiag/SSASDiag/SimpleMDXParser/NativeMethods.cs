using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace SimpleMDXParser
{
    internal class NativeMethods
    {
        // Fields
        internal const uint EM_CHARFROMPOS = 0xd7;
        internal const int EM_FORMATRANGE = 0x439;
        internal const uint EM_GETFIRSTVISIBLELINE = 0xce;
        internal const uint EM_LINEINDEX = 0xbb;
        internal const uint EM_LINELENGTH = 0xc1;
        internal const uint EM_POSFROMCHAR = 0xd6;
        internal const uint EM_SCROLL = 0xb5;
        private static int m_LockedWindows;
        internal const int SB_LINEDOWN = 1;
        internal const int SB_LINEUP = 0;
        internal const int SB_PAGEDOWN = 3;
        internal const int SB_PAGEUP = 2;
        internal const uint WM_KEYDOWN = 0x100;
        internal const uint WM_SETREDRAW = 11;
        internal const int WM_USER = 0x400;

        // Methods
        private NativeMethods()
        {
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool LockWindowUpdate(IntPtr hWndLock);
        internal static void NestedLockWindowUpdate(IntPtr h)
        {
            if (m_LockedWindows == 0)
            {
                LockWindowUpdate(h);
            }
            m_LockedWindows++;
        }

        internal static void NestedUnlockWindowUpdate()
        {
            m_LockedWindows--;
            if (m_LockedWindows == 0)
            {
                LockWindowUpdate(IntPtr.Zero);
            }
        }

        internal static void SendKeyDownEvent(IntPtr h, char key)
        {
            SendMessageInt(h, 0x100, (IntPtr)key, (IntPtr)1).ToInt32();
        }

        [DllImport("user32")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wp, ref FORMATRANGE lp);
        [DllImport("user32", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessageInt(IntPtr handle, uint msg, IntPtr wParam, IntPtr lParam);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32", EntryPoint = "ShowCaret")]
        internal static extern bool ShowCaretAPI(IntPtr hwnd);

        // Nested Types
        [StructLayout(LayoutKind.Sequential)]
        internal struct CHARRANGE
        {
            public int cpMin;
            public int cpMax;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FORMATRANGE
        {
            public IntPtr hdc;
            public IntPtr hdcTarget;
            public NativeMethods.RECT rc;
            public NativeMethods.RECT rcPage;
            public NativeMethods.CHARRANGE chrg;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }

 

}
