using System.Text;

namespace Fpi.Xml
{
    /// <summary>
    /// Util 的摘要说明。
    /// </summary>
    public class XmlUtil
    {
        public XmlUtil()
        {
        }

        public static string GetTab(int count)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count; i++)
                sb.Append('\t');
            return sb.ToString();
        }
    }
}