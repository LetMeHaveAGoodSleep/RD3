using System;
using Fpi.Communication.Exceptions;

namespace Fpi.Communication.Ports.SyncPorts
{
    /// <summary>
    /// CommunicationException 的摘要说明。
    /// </summary>
    public class TimeoutException : CommunicationException
    {
        public TimeoutException()
            : base()
        {
        }

        public TimeoutException(String message)
            : base(message)
        {
        }

        public TimeoutException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}