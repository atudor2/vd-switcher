﻿using System;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace VirtualDesktopSwitchClient.Internal.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [Flags]
    public enum DwmWindowAttribute : uint
    {
        DWMWA_NCRENDERING_ENABLED = 1,
        DWMWA_NCRENDERING_POLICY,
        DWMWA_TRANSITIONS_FORCEDISABLED,
        DWMWA_ALLOW_NCPAINT,
        DWMWA_CAPTION_BUTTON_BOUNDS,
        DWMWA_NONCLIENT_RTL_LAYOUT,
        DWMWA_FORCE_ICONIC_REPRESENTATION,
        DWMWA_FLIP3D_POLICY,
        DWMWA_EXTENDED_FRAME_BOUNDS,
        DWMWA_HAS_ICONIC_BITMAP,
        DWMWA_DISALLOW_PEEK,
        DWMWA_EXCLUDED_FROM_PEEK,
        DWMWA_CLOAK,
        DWMWA_CLOAKED,
        DWMWA_FREEZE_REPRESENTATION,
        DWMWA_LAST
    }
    public enum GetAncestorFlags
    {
        /// <summary>
        /// Retrieves the parent window. This does not include the owner, as it does with the GetParent function.
        /// </summary>
        GetParent = 1,
        /// <summary>
        /// Retrieves the root window by walking the chain of parent windows.
        /// </summary>
        GetRoot = 2,
        /// <summary>
        /// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
        /// </summary>
        GetRootOwner = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TitleBarButtonStates
    {
        public TBBStates TitleBarState;
        public TBBStates Reserved;
        public TBBStates MinState;
        public TBBStates MaxState;
        public TBBStates HelpState;
        public TBBStates CloseState;
    }

    public enum TBBStates : uint
    {
        STATE_SYSTEM_UNAVAILABLE = 0x1,
        STATE_SYSTEM_PRESSED = 0x8,
        STATE_SYSTEM_INVISIBLE = 0x8000,
        STATE_SYSTEM_OFFSCREEN = 0x10000,
        STATE_SYSTEM_FOCUSABLE = 0x100000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TITLEBARINFO
    {
        public const int CCHILDREN_TITLEBAR = 5;
        public uint cbSize;
        public RECT rcTitleBar;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = CCHILDREN_TITLEBAR + 1)]
        public uint[] rgstate;
    }

    public enum WindowLongIndex : int
    {
        GWL_EXSTYLE = -20
    }

    public enum WindowsMessage : uint
    {
        WM_KILLFOCUS = 0x0008
    }

    public enum ExtendedWindowStyles : long
    {
        WS_EX_TOOLWINDOW = 0x00000080L
    }
}