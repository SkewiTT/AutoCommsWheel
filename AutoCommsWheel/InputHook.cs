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

        // Events that mainWindow subscribes to. Windows calls the callback when the mouse does anything and it runs these events (detected by event I think).
        public event Action? MiddleDown;
        public event Action? MiddleUp;

        // The pointer to the hook for the event.
        private IntPtr _hookPointer = IntPtr.Zero;

        // Field to hold the callback so it isn't taken by the garbage collector.
        private LowLevelProc? _proc;

        public void Install()
        {
            // For if already installed because the hook pointer wouldn't be 0
            if (_hookPointer != IntPtr.Zero) return;

            // Assign the callback to the field
            _proc = Callback;

            _hookPointer = SetWindowsHookEx(WH_MOUSE_LL, _proc, GetModuleHandle(null), 0);

            if (_hookPointer == IntPtr.Zero)
            {
                MessageBox.Show("Hook wasn't set up correctly", "Hook failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Removes the hook.
        public void Uninstall()
        {
            // Don't try to uninstall if it's already uninstalled.
            if (_hookPointer == IntPtr.Zero) return;

            UnhookWindowsHookEx(_hookPointer);
            _hookPointer = IntPtr.Zero;
        }

        // The callback used in the hook when it hooks. here it invokes the events so mainWindow knows when the ones we want happen.
        private IntPtr Callback(int nCode, IntPtr wParam, IntPtr lParam)
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
            return CallNextHookEx(_hookPointer, nCode, wParam, lParam);
        }
    }
}
