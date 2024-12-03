using System;

namespace Fpi.Xml
{
    /// <summary>
    /// XmlException ��ժҪ˵����
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