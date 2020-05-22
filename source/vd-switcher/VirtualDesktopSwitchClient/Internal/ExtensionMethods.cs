using Desktopswitch;

namespace VirtualDesktopSwitchClient.Internal
{
    internal static class ExtensionMethods
    {
        public static bool UnwrapBoolResult(this BoolResult self)
        {
            return self.Result;
        }
    }
}
