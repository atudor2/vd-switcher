using System;

namespace VirtualDesktopSwitchServer
{
    public static class ExtensionMethods
    {
        public static string ToHexString(this IntPtr ptr) => $"0x{ptr.ToString("X")}";
    }
}