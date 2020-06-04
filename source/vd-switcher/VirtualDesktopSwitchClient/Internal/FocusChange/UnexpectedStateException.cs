using System;
using System.Runtime.Serialization;

namespace VirtualDesktopSwitchClient.Internal.FocusChange
{
    internal class UnexpectedStateException : Exception
    {
        public UnexpectedStateException()
        {
        }

        protected UnexpectedStateException(SerializationInfo info, StreamingContext context) : base(info, context)
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