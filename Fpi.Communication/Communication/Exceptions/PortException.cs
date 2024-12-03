using System;

namespace Fpi.Communication.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class PortException : CommunicationException
    {
        public PortException()
            : base()
        {
        }

        public PortException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public PortException(String message)
            : base(message)
        {
        }
    }
}