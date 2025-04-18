using System;

namespace Fpi.Communication.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class DeviceException : CommunicationException
    {
        public DeviceException()
            : base()
        {
        }

        public DeviceException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public DeviceException(String message)
            : base(message)
        {
        }
    }
}