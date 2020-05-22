using VirtualDesktopSwitchClient.Internal;

namespace VirtualDesktopSwitchClient
{
    public static class DesktopSwitchClientFactory
    {
        public static IDesktopSwitchClient CreateDesktopSwitchClient()
        {
            return new DesktopSwitchClientWindowSwitchDecorator(CreateBasicDesktopSwitchClient());
        }
        public static IDesktopSwitchClient CreateBasicDesktopSwitchClient()
        {
            return new DesktopSwitchClient();
        }
    }
}
