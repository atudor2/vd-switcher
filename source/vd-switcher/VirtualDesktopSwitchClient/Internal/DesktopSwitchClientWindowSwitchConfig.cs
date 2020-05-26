using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Google.Protobuf;
using IniParser;
using IniParser.Model;

namespace VirtualDesktopSwitchClient.Internal
{
    internal class DesktopSwitchClientWindowSwitchConfig : IDesktopSwitchClientWindowSwitchConfig
    {
        public DesktopSwitchClientWindowSwitchConfig()
        {
            var assembly = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

            if (assembly is null)
            {
                throw new InvalidOperationException("Cannot determine assembly path");
            }

            var parser = new FileIniDataParser();
            var data = parser.ReadFile(Path.Combine(assembly, "VirtualDesktopSwitchClient.ini"));
            var section = data["WindowFocusBehaviour"];

            SetShellWindowFocusBeforeSwitch = AsBool(section["SetShellWindowFocusBeforeSwitch"], false);
            WrapCallsInAttachThreadInput = AsBool(section["WrapCallsInAttachThreadInput"], true);
            CallSetForegroundWindow = AsBool(section["CallSetForegroundWindow"], true);
            CallShowWindow = AsBool(section["CallShowWindow"], false);
        }

        public bool WrapCallsInAttachThreadInput { get; }
        public bool CallSetForegroundWindow { get; }
        public bool CallShowWindow { get; }
        public bool SetShellWindowFocusBeforeSwitch { get; }

        private static bool AsBool(string value, bool defaultValue)
        {
            if (!bool.TryParse(value, out var result))
            {
                result = defaultValue;
            }

            return result;
        }
    }
}
