using System;

namespace Fpi.Util.Exeptions
{
    /// <summary>
    /// PlatformException 的摘要说明。
    /// </summary>
    public class PlatformException : FpiException
    {
        public PlatformException()
            : base()
        {
        }
       

        public PlatformException(String message)
            : base(message)
        {
        }

        public PlatformException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}