using System;

namespace Fpi.Util.Interfaces.Initialize
{
    public class InitException : Exception
    {
        public InitException()
        {
        }

        public InitException(string msg)
            : base(msg)
        {
        }

        public InitException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}