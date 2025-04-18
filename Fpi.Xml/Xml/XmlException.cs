using System;

namespace Fpi.Xml
{
    /// <summary>
    /// XmlException 的摘要说明。
    /// </summary>
    public class XmlException : Exception
    {
        public XmlException()
            : base()
        {
        }

        public XmlException(String message)
            : base(message)
        {
        }

        public XmlException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}