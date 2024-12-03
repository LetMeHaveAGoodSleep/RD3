using System;

namespace Fpi.Util.Exeptions
{
    /// <summary>
    /// ApplicationException 的摘要说明。
    /// </summary>
    public class ApplicationException : FpiException
    {
        public ApplicationException()
            : base()
        {
        }

        public ApplicationException(String message)
            : base(message)
        {
        }

        public ApplicationException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}