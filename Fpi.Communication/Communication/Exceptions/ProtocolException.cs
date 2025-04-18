using System;

namespace Fpi.Communication.Exceptions
{
    public class ProtocolException : CommunicationException
    {
        public ProtocolException()
            : base()
        {
        }

        public ProtocolException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ProtocolException(String message)
            : base(message)
        {
        }
    }
}