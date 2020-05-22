using System;
using NLog;
using VirtualDesktopSwitchClient;

namespace VirtualDesktopStopServer
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            try
            {
                Logger.Info("Stopping server");
                DesktopSwitchClientFactory.CreateDesktopSwitchClient().ShutdownServer();
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }
    }
}
