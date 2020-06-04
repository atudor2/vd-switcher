using System;

namespace VirtualDesktopSwitchClient.Internal.FocusChange
{
    public interface IFocusChangeMethod
    {
        bool SetWindowFocusOnDesktop(IntPtr hWnd);
    }
}
