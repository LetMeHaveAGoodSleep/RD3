using System;

namespace Fpi.Communication.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class DataFormatException : CommunicationException
    {
        public DataFormatException()
            : base()
        {
        }

        public DataFormatException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public DataFormatException(String message)
            : base(message)
        {
        }
    }

}