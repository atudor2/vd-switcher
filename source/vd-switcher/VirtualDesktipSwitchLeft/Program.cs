using System;
using VirtualDesktopSwitchClient;

namespace VirtualDesktipSwitchLeft
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = DesktopSwitchClientFactory.CreateDesktopSwitchClient();
            c.CycleDesktopLeft();
        }
    }
}
