using System;
using System.Runtime.InteropServices;

namespace WindowAPI
{
    public class consts
    {
        public const int 
                        WM_USER = 0x0400,
                        WM_CLOSE = 0x0010,
                        WS_CHILD = 0x40000000,
                        WS_VISIBLE = 0x10000000,
                        WM_ACTIVATEAPP = 0x001C;
    }

    public class user32
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, uint wParam, long lParam);
        [DllImport("user32.dll")]
        public static extern bool DestroyWindow(IntPtr hWnd);

        // Constant values were found in the "windows.h" header file.

    }
}