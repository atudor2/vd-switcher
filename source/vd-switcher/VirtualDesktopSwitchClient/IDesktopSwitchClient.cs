using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualDesktopSwitchClient
{
    public interface IDesktopSwitchClient : IDisposable
    {
        bool SwitchDesktopLeft();

        bool SwitchDesktopRight();

        bool CycleDesktopLeft();

        bool CycleDesktopRight();

        bool IsWindowOnCurrentDesktop(IntPtr hWnd);
    }
}
