using System;

namespace Fpi.Communication.Exceptions
{
    public class CommandDelaySetException : ApplicationException
    {
        public CommandDelaySetException()
            : base()
        {
        }

        public CommandDelaySetException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CommandDelaySetException(String message)
            : base(message)
        {
        }
    }
}