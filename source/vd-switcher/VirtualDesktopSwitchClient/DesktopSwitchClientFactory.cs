using VirtualDesktopSwitchClient.Internal;
using VirtualDesktopSwitchClient.Internal.FocusChange;

namespace VirtualDesktopSwitchClient
{
    public static class DesktopSwitchClientFactory
    {
        public static IDesktopSwitchClient CreateDesktopSwitchClient()
        {
            return new DesktopSwitchClientWindowSwitchDecorator(CreateBasicDesktopSwitchClient(), new HotKeyFocusChangeMethod());
        }
        public static IDesktopSwitchClient CreateBasicDesktopSwitchClient()
        {
            return new DesktopSwitchClient();
        }
    }
}
