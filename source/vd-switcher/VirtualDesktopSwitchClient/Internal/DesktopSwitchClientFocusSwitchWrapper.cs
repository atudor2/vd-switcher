using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualDesktopSwitchClient.Internal
{
    internal class DesktopSwitchClientFocusSwitchDecorator : IDesktopSwitchClient
    {
        private readonly IDesktopSwitchClient _client;

        public DesktopSwitchClientFocusSwitchDecorator(IDesktopSwitchClient client)
        {
            _client = client;
        }
        public void Dispose()
        {
            _client.Dispose();
        }

        public bool SwitchDesktopLeft()
        {
            return _client.SwitchDesktopLeft();
        }

        public bool SwitchDesktopRight()
        {
            return _client.SwitchDesktopRight();
        }

        public bool CycleDesktopLeft()
        {
            return _client.CycleDesktopLeft();
        }

        public bool CycleDesktopRight()
        {
            return _client.CycleDesktopRight();
        }

        public bool IsWindowOnCurrentDesktop(IntPtr hWnd)
        {
            return _client.IsWindowOnCurrentDesktop(hWnd);
        }
    }
}
