using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NLog;

namespace VirtualDesktopSwitchClient.Internal.Native
{
    internal static class NativeMethodsHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static unsafe bool IsWindowCloaked(IntPtr hWnd)
        {
            // Not a cloaked window
            var isCloaked = false; // stack-alloc, no need to pin

            var hResult = NativeMethods.DwmGetWindowAttribute(hWnd, (int)DwmWindowAttribute.DWMWA_CLOAKED, &isCloaked, Marshal.SizeOf(isCloaked));

            var exception = Marshal.GetExceptionForHR(hResult);
            if (exception != null)
            {
                throw exception;
            }

            return isCloaked;
        }

        public static string GetWindowText(IntPtr hWnd)
        {
            var b = new StringBuilder(1000);
            if (NativeMethods.GetWindowText(hWnd, b, 1000) == 0)
            {
                throw new Win32Exception();
            }
            return b.ToString();
        }

        public static bool EnumerateWindows(Func<IntPtr, bool> callback)
        {
            return NativeMethods.EnumWindows((hWnd, _) => callback(hWnd), IntPtr.Zero);
        }

        public static bool IsToolWindow(IntPtr hWnd)
        {
            var windowsLong = GetWindowLong(hWnd, WindowLongIndex.GWL_EXSTYLE).ToInt64();
            if (windowsLong == 0)
            {
                throw new Win32Exception();
            }
            return (windowsLong & (long)ExtendedWindowStyles.WS_EX_TOOLWINDOW) != 0;
        }

        public static bool IsProgramManagerOrHiddenTrayWindow(IntPtr hWnd)
        {
            // Kind of magic...
            var titlebarInfo = new TITLEBARINFO();
            titlebarInfo.cbSize = (uint)Marshal.SizeOf(titlebarInfo);

            if (!NativeMethods.GetTitleBarInfo(hWnd, ref titlebarInfo))
            {
                throw new Win32Exception();
            }
            return (titlebarInfo.rgstate[0] & (uint)TBBStates.STATE_SYSTEM_INVISIBLE) != 0;
        }

        public static bool IsActiveAndReallyVisibleTopLevelWindow(IntPtr hWnd)
        {
            // From root, walk down the last active chain to the 1st visible window
            // Same handle => hWnd active and actually "visible" to the user 
            var hWndTry = NativeMethods.GetAncestor(hWnd, GetAncestorFlags.GetRootOwner);
            var hWndWalk = IntPtr.Zero;
            while (hWndTry != hWndWalk)
            {
                hWndWalk = hWndTry;
                hWndTry = NativeMethods.GetLastActivePopup(hWndWalk);
                if (NativeMethods.IsWindowVisible(hWndTry)) break;
            }

            return hWndWalk == hWnd;
        }

        public static IntPtr GetWindowLong(IntPtr hWnd, WindowLongIndex nIndex)
        {
            return IntPtr.Size == 4 ? NativeMethods.GetWindowLong32(hWnd, (int)nIndex) : NativeMethods.GetWindowLongPtr64(hWnd, (int)nIndex);
        }

        public static bool ForceForegroundWindow(IntPtr hWnd)
        {
            bool DoForegroundWindowSet()
            {
                var r = NativeMethods.SetForegroundWindow(hWnd);
                return r;
            }

            try
            {
                return AttachedThreadInputAction(hWnd, DoForegroundWindowSet);
            }
            catch (ThreadStateException)
            {
                Logger.Warn("AttachThreadInput() call failed");
                return DoForegroundWindowSet();
            }
        }

        public static T AttachedThreadInputAction<T>(IntPtr hWnd, Func<T> action)
        {
            var foreThread = NativeMethods.GetWindowThreadProcessId(hWnd, out _);
            var appThread = NativeMethods.GetCurrentThreadId();
            var threadsAttached = false;

            try
            {
                threadsAttached = foreThread == appThread || NativeMethods.AttachThreadInput(foreThread, appThread, true);

                if (threadsAttached)
                {
                    return action();
                }
                else
                {
                    throw new ThreadStateException("AttachThreadInput failed");
                }
            }
            finally
            {
                if (threadsAttached) NativeMethods.AttachThreadInput(foreThread, appThread, false);
            }
        }
    }
}
