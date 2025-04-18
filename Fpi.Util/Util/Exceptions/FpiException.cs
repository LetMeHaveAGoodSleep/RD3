using System;

namespace Fpi.Util.Exeptions
{
    public class FpiException : Exception
    {
        public FpiException()
            : base()
        {
        }

        public FpiException(String message)
            : base(message)
        {
        }

        public FpiException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}