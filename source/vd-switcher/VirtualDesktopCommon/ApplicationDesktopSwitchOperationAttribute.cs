using System;
using Desktopswitch;

namespace VirtualDesktopCommon
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public class ApplicationDesktopSwitchOperationAttribute : Attribute
    {
        public SwitchOperationType Operation { get; }

        public ApplicationDesktopSwitchOperationAttribute(SwitchOperationType operation)
        {
            Operation = operation;
        }
    }
}
