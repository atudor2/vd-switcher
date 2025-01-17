﻿using System;
using System.Linq;
using Desktopswitch;
using NLog;
using VirtualDesktopSwitchClient;

namespace VirtualDesktopSwitch
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("Enter switch operation: ");
                    foreach (var name in Enum.GetNames(typeof(SwitchOperationType)))
                    {
                        Console.WriteLine($"* {name}");
                    }
                    return;
                }

                EnableFullLogging();

                var op = args[0];
                if (!Enum.TryParse(op, true, out SwitchOperationType operation))
                {
                    Console.WriteLine($"Invalid switch operation: {op}");
                    return;
                }

                Logger.Info($"Performing switch operation: { operation }");

                var client = DesktopSwitchClientFactory.CreateDesktopSwitchClient();

                switch (operation)
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
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }

        private static void EnableFullLogging()
        {
            foreach (var rule in LogManager.Configuration.LoggingRules.Where(r => r.Targets.Any(t => t.Name == "logconsole")))
            {
                rule.EnableLoggingForLevel(LogLevel.Trace);
            }

            LogManager.ReconfigExistingLoggers();
        }
    }
}
