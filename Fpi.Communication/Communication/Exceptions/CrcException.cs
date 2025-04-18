using System;

namespace Fpi.Communication.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class CrcException : CommunicationException
    {
        public CrcException()
            : base()
        {
        }

        public CrcException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CrcException(String message)
            : base(message)
        {
        }
    }
}