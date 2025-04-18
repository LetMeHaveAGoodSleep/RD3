using System;

namespace Fpi.Communication.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class CommandException : CommunicationException
    {
        public CommandException()
            : base()
        {
        }

        public CommandException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CommandException(String message)
            : base(message)
        {
        }
    }
}