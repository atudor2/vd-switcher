using System;
using System.Reflection;
using Desktopswitch;
using VirtualDesktopCommon;
using VirtualDesktopSwitchClient;

public static class DesktopSwitchApplication
{
    public static void Main(string[] args)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var attr = assembly.GetCustomAttribute<ApplicationDesktopSwitchOperationAttribute>();

        if (attr is null)
        {
            throw new InvalidOperationException($"The Assembly {assembly.FullName} does not contain assembly-level attribute {nameof(ApplicationDesktopSwitchOperationAttribute)}");
        }

        var client = DesktopSwitchClientFactory.CreateDesktopSwitchClient();

        switch (attr.Operation)
        {
            case SwitchOperationType.Left:
                client.SwitchDesktopLeft();
                break;
            case SwitchOperationType.Right:
                client.SwitchDesktopRight();
                break;
            case SwitchOperationType.CycleLeft:
                client.CycleDesktopLeft();
                break;
            case SwitchOperationType.CycleRight:
                client.CycleDesktopRight();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
