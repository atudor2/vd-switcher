using System;
using System.Linq.Expressions;
using WindowsInput;
using WindowsInput.Native;
using NLog;
using VirtualDesktopCommon;
using VirtualDesktopSwitchClient.Internal.Native;

namespace VirtualDesktopSwitchClient.Internal.FocusChange
{
    internal class HotKeyFocusChangeMethod : IFocusChangeMethod
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const int HotKeyId = 0x0000baba;
        private const int HotKeyWaitTimeout = 2000;
        private const VirtualKeyCode TempHotKey = VirtualKeyCode.F22;

        public bool SetWindowFocusOnDesktop(IntPtr hWnd)
        {
            // Shamelessly borrowed from https://source.chromium.org/chromium/chromium/src/+/master:ui/base/win/foreground_helper.cc;l=37?originalUrl=https:%2F%2Fcs.chromium.org%2Fchromium%2Fsrc%2Fui%2Fbase%2Fwin%2Fforeground_helper.cc

            // As per Raymond Chen - https://devblogs.microsoft.com/oldnewthing/20090226-00/?p=19013 (Pressing a registered hotkey gives you the foreground activation love), 
            // if we register as a hotkey, Windows sets us as foreground proc and will allow a SetForegroundWindow()

            RegisterHotKey();

            try
            {
                // If the calling thread is not yet a UI thread, call PeekMessage to ensure creation of its message queue.
                NativeMethods.PeekMessage(out _, IntPtr.Zero, 0, 0, (uint)PeekMessageParams.PM_NOREMOVE);

                // SendInput() is a pain to pinvoke in C#, so use an existing lib
                new InputSimulator().Keyboard.KeyPress(TempHotKey);

                return WaitForHotKey(hWnd);
            }
            finally
            {
                UnRegisterHotKey();
            }
        }

        private void UnRegisterHotKey()
        {
            TraceExpression(() => NativeMethods.UnregisterHotKey(IntPtr.Zero, HotKeyId));
        }

        private void RegisterHotKey()
        {
            UnRegisterHotKey();
            TraceExpression(() => NativeMethods.RegisterHotKey(IntPtr.Zero, HotKeyId, 0, (uint)TempHotKey));
        }

        private static bool WaitForHotKey(IntPtr hWnd)
        {
            // Use a timer in case the hotkey never hits the message loop
            TraceExpression(() => NativeMethods.SetTimer(IntPtr.Zero, new IntPtr(HotKeyId), HotKeyWaitTimeout, null));

            try
            {
                while (NativeMethods.GetMessage(out var msg, IntPtr.Zero, 0, 0) != 0)
                {
                    NativeMethods.TranslateMessage(ref msg);
                    NativeMethods.DispatchMessage(ref msg);

                    switch (msg.msg)
                    {
                        case (uint)WindowsMessage.WM_TIMER:
                        case (uint)WindowsMessage.WM_HOTKEY:
                            return SetForegroundWindow(hWnd);
                    }
                }

                // ?? No timer or hotkey?
                throw new UnexpectedStateException("GetMessage() never received a WM_TIMER or WM_HOTKEY message before ending");
            }
            finally
            {
                TraceExpression(() => NativeMethods.KillTimer(IntPtr.Zero, new IntPtr(HotKeyId)));
            }
        }

        private static bool SetForegroundWindow(IntPtr hWnd)
        {
            var r = NativeMethods.SetForegroundWindow(hWnd);
            Logger.Trace($"SetForegroundWindow({hWnd.ToHexString()}) => {r}");
            return r;
        }

        private static T TraceExpression<T>(Expression<Func<T>> expression)
        {
            var result = expression.Compile()();
            Logger.Trace($"{expression} => {result}");
            return result;
        }
    }
}
