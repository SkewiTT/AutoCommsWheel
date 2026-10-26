using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace AutoCommsWheel
{
    public static class ExtendedWindowStyles
    {
        // Makes the window non clickable
        public const int WS_EX_TRANSPARENT = 0x00000020;

        // Never takes focus
        public const int WS_EX_NOACTIVATE = 0x08000000;

        // Doesn't show up in alt+tab
        public const int WS_EX_TOOLWINDOW = 0x00000080;

        public const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);



        public static void ToTransparentWindow(Window window)
        {

            window.SourceInitialized +=
                delegate
                {
                    // Get this window's handle
                    IntPtr hwnd = new WindowInteropHelper(window).Handle;

                    // Change the extended window style to include WS_EX_TRANSPARENT
                    int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

                    // Here we use the bitwise operator | to check for flags
                    SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);
                };
        }
    }
}
