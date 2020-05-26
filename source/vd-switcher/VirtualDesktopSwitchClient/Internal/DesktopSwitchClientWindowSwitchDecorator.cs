using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;
using VirtualDesktopCommon;
using VirtualDesktopSwitchClient.Internal.Native;

namespace VirtualDesktopSwitchClient.Internal
{
    internal class DesktopSwitchClientWindowSwitchDecorator : IDesktopSwitchClient
    {
        private readonly IDesktopSwitchClient _client;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDesktopSwitchClientWindowSwitchConfig _config;

        public DesktopSwitchClientWindowSwitchDecorator(IDesktopSwitchClient client, IDesktopSwitchClientWindowSwitchConfig config)
        {
            _client = client;
            _config = config;
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
                Logger.Warn(">> No window found to set a foreground");
                return;
            }

            FocusWindow(windowPtr);
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
            if (!_config.SetShellWindowFocusBeforeSwitch) return; // Nothing to do

            Logger.Info(">>> Before Desktop Switch");

            // Set the taskbar as focused window before switching
            // This should stop window flashing on switch....
            var hWnd = NativeMethods.FindWindow("Shell_TrayWnd", null);
            if (hWnd == IntPtr.Zero)
            {
                Logger.Warn($">> FindWindow(\"Shell_TrayWnd\") returned NULL - cannot focus shell window before switch");
                return;
            }

            Logger.Trace(() => $"Shell Window found: {hWnd.ToHexString()}");
            FocusWindow(hWnd);
        }

        private void FocusWindow(IntPtr hWnd)
        {
            // Depending on config, we can either try:
            // (0) Plain SetForegroundWindow()
            // (2) Plain ShowWindow()
            // (3) ShowWindow() + SetForegroundWindow()
            // Either option can be within an AttachThreadInput call

            var functions = new List<Func<IntPtr, bool>>();
            if (_config.CallShowWindow)
            {
                functions.Add(ShowWindow);
            }

            if (_config.CallSetForegroundWindow)
            {
                functions.Add(SetForegroundWindow);
            }

            if (!CallFocusChangeFunctions(hWnd, functions, _config.WrapCallsInAttachThreadInput))
            {
                Logger.Warn($">> Focus Change Request for {hWnd.ToHexString()} returned FALSE - focus was not changed to window!");
            }
            else
            { 
                Logger.Trace($">> Focus Change Request for {hWnd.ToHexString()} returned TRUE");
            }
        }

        private bool CallFocusChangeFunctions(IntPtr hWnd, IReadOnlyCollection<Func<IntPtr, bool>> functions, bool useAti)
        {
            return useAti ? NativeMethodsHelper.AttachedThreadInputAction(hWnd, () => InvokeFunctionList(functions, hWnd), true) :
                            InvokeFunctionList(functions, hWnd);
        }

        private static bool InvokeFunctionList(IEnumerable<Func<IntPtr, bool>> functions, IntPtr hWnd)
        {
            return functions.Aggregate(true, (v, f) => v && f(hWnd));
        }

        private static bool SetForegroundWindow(IntPtr hWnd)
        {
            var r = NativeMethods.SetForegroundWindow(hWnd);
            Logger.Trace($"SetForegroundWindow({hWnd.ToHexString()}) => {r}");
            return r;
        }

        private static bool ShowWindow(IntPtr hWnd)
        {
            var r = NativeMethods.ShowWindow(hWnd, ShowWindowFlags.SW_SHOW); 
            Logger.Trace($"ShowWindow({hWnd.ToHexString()}, 5) => {r}");
            return r;
        }

        private static string DumpHWndTitle(IntPtr hWnd)
        {
            return $"{NativeMethodsHelper.GetWindowText(hWnd)} ({ hWnd.ToHexString() })";
        }
    }
}
