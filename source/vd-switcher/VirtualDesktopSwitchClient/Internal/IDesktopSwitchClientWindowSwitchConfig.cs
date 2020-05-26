using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualDesktopSwitchClient.Internal
{
    public interface IDesktopSwitchClientWindowSwitchConfig
    {
        bool WrapCallsInAttachThreadInput { get; }
        bool CallSetForegroundWindow { get; }
        bool CallShowWindow { get; }
        bool SetShellWindowFocusBeforeSwitch { get; }
    }
}
