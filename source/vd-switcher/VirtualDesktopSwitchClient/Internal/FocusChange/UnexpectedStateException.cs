using System;
using System.Runtime.Serialization;

namespace VirtualDesktopSwitchClient.Internal.FocusChange
{
    internal class UnexpectedStateException : Exception
    {
        public UnexpectedStateException()
        {
        }
        
        public UnexpectedStateException(string message) : base(message)
        {
        }

        public UnexpectedStateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}