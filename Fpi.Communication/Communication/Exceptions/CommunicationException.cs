using System;
using Fpi.Util.Exeptions;

namespace Fpi.Communication.Exceptions
{
    public class CommunicationException : PlatformException
    {
        public CommunicationException()
            : base()
        {
        }

        public CommunicationException(String message)
            : base(message)
        {
        }

        public CommunicationException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}