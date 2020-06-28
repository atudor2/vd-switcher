using System;
using System.Diagnostics;
using NLog;
using VirtualDesktopCommon;
using VirtualDesktopSwitchClient.Internal.FocusChange;
using VirtualDesktopSwitchClient.Internal.Native;

namespace VirtualDesktopSwitchClient.Internal
{
    internal class DesktopSwitchClientWindowSwitchDecorator : IDesktopSwitchClient
    {
        private readonly IDesktopSwitchClient _client;
        private readonly IFocusChangeMethod _focusChangeMethod;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public DesktopSwitchClientWindowSwitchDecorator(IDesktopSwitchClient client, IFocusChangeMethod focusChangeMethod)
        {
            _client = client;
            _focusChangeMethod = focusChangeMethod;
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

        public void ShutdownServer()
        {
            _client.ShutdownServer();
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
            Logger.Info(">>> After Desktop Switch");

            var windowPtr = GetWindowToSetAsForeground();

            if (windowPtr == IntPtr.Zero)
            {
                Logger.Warn(">> No window found to set a foreground, trying Shell Window");
                windowPtr = FindShellWindow();
                if (windowPtr == IntPtr.Zero)
                {
                    return; // Cannot do anything
                }
            }

            FocusWindow(windowPtr);
        }

        private IntPtr FindShellWindow()
        {
            var hWnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            if (hWnd == IntPtr.Zero)
            {
                Logger.Warn($">> FindWindow(\"Shell_TrayWnd\") returned NULL");
                return IntPtr.Zero;
            }

            Logger.Trace(() => $"Shell Window found: {hWnd.ToHexString()}");

            return hWnd;
        }

        private IntPtr GetWindowToSetAsForeground()
        {
            IntPtr windowPtr = default;

            NativeMethodsHelper.EnumerateWindows(hWnd =>
            {
                // Condition order important -> the VirtualDesktop call is a (potential) RPC so if we 
                // can short circuit the check it would be better
                var matchedWindow = IsValidWindow(hWnd) && _client.IsWindowOnCurrentDesktop(hWnd);
                if (!matchedWindow) return true; // Carry on search

                Logger.Debug(() => $"Found valid window to focus: {DumpHWndTitle(hWnd)} - { GetProcessForHWnd(hWnd) }");

                windowPtr = hWnd;
                return false;
            });

            return windowPtr;
        }

        private string GetProcessForHWnd(IntPtr hWnd)
        {
            var result = NativeMethods.GetWindowThreadProcessId(hWnd, out var pId);
            var name = result == 0 ? null : Process.GetProcessById((int)pId).MainModule?.FileName;
            return name ?? "<unknown>";
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
            // NOP
        }

        private void FocusWindow(IntPtr hWnd)
        {
            var r = _focusChangeMethod.SetWindowFocusOnDesktop(hWnd);

            if (!r)
            {
                Logger.Warn($">> Focus Change Request for {hWnd.ToHexString()} returned FALSE - focus was not changed to window!");
            }
            else
            {
                Logger.Debug($">> Focus Change Request for {hWnd.ToHexString()} returned TRUE");
            }

        }

        private static string DumpHWndTitle(IntPtr hWnd)
        {
            return $"{NativeMethodsHelper.GetWindowText(hWnd)} ({ hWnd.ToHexString() })";
        }
    }
}
