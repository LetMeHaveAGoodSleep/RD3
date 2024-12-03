using System;
using Fpi.Util.Exeptions;

namespace Fpi.Communication
{
    /// <summary>
    /// CommunicationException ��ժҪ˵����
    /// </summary>
    public class CommandException : PlatformException
    {
        public CommandException()
            : base()
        {
        }

        public CommandException(String message)
            : base(message)
        {
        }

        public CommandException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}