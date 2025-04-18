using System;
using Fpi.Util.Exeptions;

namespace Fpi.Communication.Exceptions
{
    /// <summary>
    ///
    /// </summary>
    public class WebException : CommunicationException
    {
        public WebException()
            : base()
        {
        }

        public WebException(String message)
            : base(message)
        {
        }

        public WebException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}