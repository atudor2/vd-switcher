using System;
using VirtualDesktopSwitchClient.Internal.Native;

namespace VirtualDesktopSwitchClient.Internal
{
    internal class DesktopSwitchClientWindowSwitchDecorator : IDesktopSwitchClient
    {
        private readonly IDesktopSwitchClient _client;

        public DesktopSwitchClientWindowSwitchDecorator(IDesktopSwitchClient client)
        {
            _client = client;
        }
        public void Dispose()
        {
            _client.Dispose();
        }

        public bool SwitchDesktopLeft()
        {
            return WrapDesktopSwitch(() => _client.SwitchDesktopLeft());
        }

        public bool SwitchDesktopRight()
        {
            return WrapDesktopSwitch(() => _client.SwitchDesktopRight());
        }

        public bool CycleDesktopLeft()
        {
            return WrapDesktopSwitch(() => _client.CycleDesktopLeft());
        }

        public bool CycleDesktopRight()
        {
            return WrapDesktopSwitch(() => _client.CycleDesktopRight());
        }

        public bool IsWindowOnCurrentDesktop(IntPtr hWnd)
        {
            return _client.IsWindowOnCurrentDesktop(hWnd);
        }

        private T WrapDesktopSwitch<T>(Func<T> func)
        {
            BeforeDesktopSwitch();
            var r = func();
            AfterDesktopSwitch();
            return r;
        }

        private void AfterDesktopSwitch()
        {
            WriteMessage("------------- AFTER DESKTOP SWITCH -------------");

            var windowPtr = GetWindowToSetAsForeground();

            if (windowPtr == IntPtr.Zero)
            {
                WriteMessage(">> No window found to set a foreground");
                return;
            }

            SetForegroundWindow(windowPtr);
        }

        private IntPtr GetWindowToSetAsForeground()
        {
            IntPtr windowPtr = default;

            NativeMethodsHelper.EnumerateWindows(hWnd =>
            {
                // Condition order important -> the virtualdesktop call is a (potential) RPC so if we 
                // can short circuit the check it would be better
                var matchedWindow = IsValidWindow(hWnd) && _client.IsWindowOnCurrentDesktop(hWnd);
                if (!matchedWindow) return true; // Carry on search

                WriteMessage(() => DumpHWndTitle(hWnd));

                windowPtr = hWnd;
                return false;
            });

            return windowPtr;
        }

        private static bool IsValidWindow(IntPtr hWnd)
        {
            // Basic WS_VISIBLE check
            if (!NativeMethods.IsWindowVisible(hWnd)) return false;
            // Actually visible?
            if (!NativeMethodsHelper.IsActiveAndReallyVisibleTopLevelWindow(hWnd)) return false;
            // Remove notification icon programs and "Program Manager"
            if (NativeMethodsHelper.IsProgramManagerOrHiddenTrayWindow(hWnd)) return false;
            // Not a tool window
            if (NativeMethodsHelper.IsToolWindow(hWnd)) return false;
            // Not cloaked
            if (NativeMethodsHelper.IsWindowCloaked(hWnd)) return false;

            return true;
        }
        private void BeforeDesktopSwitch()
        {
            WriteMessage("------------- BEFORE DESKTOP SWITCH -------------");
            // Set the taskbar as focused window before switching
            // This should stop window flashing on switch....
            var hWnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            if (hWnd == IntPtr.Zero)
            {
                WriteMessage($">> FindWindow(\"Shell_TrayWnd\") returned NULL");
            }
            else
            {
                SetForegroundWindow(hWnd);
            }
        }

        private static void WriteMessage(Func<string> m)
        {
            WriteMessage(m());
        }

        private static void WriteMessage(string m)
        {
            // TODO REMOVE
            Console.WriteLine(m);
        }

        private static void SetForegroundWindow(IntPtr hWnd)
        {
            if (!NativeMethodsHelper.ForceForegroundWindow(hWnd))
            {
                WriteMessage($">> ForceForegroundWindow({ hWnd.ToHexString() }) returned FALSE - focus was not changed to window!");
            }
        }

        private static string DumpHWndTitle(IntPtr hWnd)
        {
            return $"{NativeMethodsHelper.GetWindowText(hWnd)} ({ hWnd.ToHexString() })";
        }
    }
}
