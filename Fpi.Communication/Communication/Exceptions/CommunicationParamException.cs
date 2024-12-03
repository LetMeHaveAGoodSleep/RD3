using System;
using Fpi.Util.Exeptions;

namespace Fpi.Communication.Exceptions
{
    public class CommunicationParamException : PlatformException
    {
        public CommunicationParamException()
            : base()
        {
        }

        public CommunicationParamException(String message)
            : base(message)
        {
        }

        public CommunicationParamException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}