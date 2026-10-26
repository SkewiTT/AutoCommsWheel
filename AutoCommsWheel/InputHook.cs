using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoCommsWheel
{
    public class InputHook
    {
        // The method signature corresponding to the callback used in the hook. Taken straight from Windows.
        private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        // Sets the hook
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

        // Removes the hook
        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        // Lets other things use the hook, IMPORTANT to be passed at the end of the callback otherwise other programs can't use the mouse anymore.
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        // Gets the handle / Ptr.
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        // Windows' definitions to the hook type for mouse and the events linked to what we want.
        private const int WH_MOUSE_LL = 14;        // hook type: low-level mouse
        private const int WM_MBUTTONDOWN = 0x0207; // event: middle button pressed
        private const int WM_MBUTTONUP = 0x0208;   // event: middle button released

        // Windows' definitions to the hook type for keyboard and the events linked to what we want
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104; // Alt combos and F10

        // Events that mainWindow subscribes to. Windows calls the callback when the mouse does anything and it runs these events (detected by event I think).
        public event Action? MiddleDown;
        public event Action? MiddleUp;

        // Event that mainWindow subscribes to. Windows calls the callback when a key is pressed and these events are run
        public event Action<int>? KeyPressed;

        // The pointers to the hooks for the events.
        private IntPtr _mouseHookPointer = IntPtr.Zero;
        private IntPtr _keyboardHookPointer = IntPtr.Zero;

        // Field to hold the callbacks so they arent't taken by the garbage collector.
        private LowLevelProc? _mouseProc;
        private LowLevelProc? _keyboardProc;

        public void Install()
        {
            // For if already installed because the hook pointer wouldn't be 0
            if (_mouseHookPointer == IntPtr.Zero)
            {
                // Assign the callback to the field
                _mouseProc = MouseCallback;

                // Sets the mouse hook and checks the pointer
                _mouseHookPointer = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, GetModuleHandle(null), 0);

                if (_mouseHookPointer == IntPtr.Zero)
                {
                    MessageBox.Show("Mouse hook wasn't set up correctly", "Mouse hook failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
            if (_keyboardHookPointer == IntPtr.Zero)
            {
                // Assign the callback to the field
                _keyboardProc = KeyboardCallback;

                // Sets the keyboard hook and checks the pointer
                _keyboardHookPointer = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, GetModuleHandle(null), 0);

                if (_keyboardHookPointer == IntPtr.Zero)
                {
                    MessageBox.Show("Keyboard hook wasn't set up correctly", "keyboard hook failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Removes the hook.
        public void Uninstall()
        {
            // Don't try to uninstall if it's already uninstalled.

            if (_mouseHookPointer != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHookPointer);
                _mouseHookPointer = IntPtr.Zero;
            }

            if (_keyboardHookPointer != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookPointer);
                _keyboardHookPointer = IntPtr.Zero;
            }
        }

        // The callback used in the hook when it hooks. here it invokes the events so mainWindow knows when the ones we want happen.
        private IntPtr MouseCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try 
            {
                // checks that it's a real event, negative ones must be passed on untouched.
                if (nCode >= 0)
                {
                    if ((int)wParam == WM_MBUTTONDOWN) MiddleDown?.Invoke();
                    else if ((int) wParam == WM_MBUTTONUP) MiddleUp?.Invoke();
                }
            }
            catch (Exception)
            {
                // A failing handler must never stop the event from being passed on
            }


            // Not forgetting to call the next hook for other uses.
            return CallNextHookEx(_mouseHookPointer, nCode, wParam, lParam);
        }

        private IntPtr KeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                // checks that it's a real event, negative ones must be passed on untouched. also checks it's one of the events for keyboard.
                if (nCode >= 0 && ((int)wParam == WM_KEYDOWN || (int)wParam == WM_SYSKEYDOWN))
                {
                    // lParam points to a struct whose first field is the virtual key code
                    int vkCode = Marshal.ReadInt32(lParam);
                    KeyPressed?.Invoke(vkCode);
                }
            }
            catch (Exception)
            {
                // A failing handler must never stop the event from being passed on
            }


            // Not forgetting to call the next hook for other uses.
            return CallNextHookEx(_keyboardHookPointer, nCode, wParam, lParam);
        }
    }
}
