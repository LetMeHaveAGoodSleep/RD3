using System;

namespace Fpi.Util.Exeptions
{
    /// <summary>
    /// PlatformException ��ժҪ˵����
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