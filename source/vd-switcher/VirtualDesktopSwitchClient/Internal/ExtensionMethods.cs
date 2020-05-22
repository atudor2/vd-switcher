using System;
using Desktopswitch;

namespace VirtualDesktopSwitchClient.Internal
{
    internal static class ExtensionMethods
    {
        public static bool UnwrapBoolResult(this BoolResult self)
        {
            return self.Result;
        }

        // TODO - Fix?
        public static string ToHexString(this IntPtr ptr) => $"0x{ptr.ToString("X")}";
    }
}
