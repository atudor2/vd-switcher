using System;
using Desktopswitch;

namespace VirtualDesktopCommon
{
    public static class ExtensionMethods
    {
        public static string ToHexString(this IntPtr ptr) => $"0x{ptr.ToString("X")}";
        public static bool UnwrapBoolResult(this BoolResult self) => self.Result;
    }
}
