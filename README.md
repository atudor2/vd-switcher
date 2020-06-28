# Windows 10 Virtual Desktop Switcher

C# application to switch VirtualDesktops via my keyboard macros.

Using AutoHotkey didn't work very well with VMs as it did not always escape the VM properly first.

Run VirtualDesktopSwitchServer.exe at startup and then:
* VirtualDesktopSwitchLeft.exe to switch to the left desktop
* VirtualDesktopSwitchRight.exe to switch to the right desktop
* VirtualDesktopCycleLeft.exe to cycle to the left desktop
* VirtualDesktopCycleRight.exe to cycle to the right desktop

VirtualDesktopStopServer.exe can be used to stop the existing server.

VirtualDesktopSwitch.exe takes a direction argument and can be run from the console and will enable full trace output