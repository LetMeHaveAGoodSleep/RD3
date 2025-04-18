using System;
using System.IO;
using System.Xml;
using System.Collections;
using Fpi.Util.Security;
using System.Text;


namespace Fpi.Xml
{
    /// <summary>
    /// VarConfig 的摘要说明。
    /// 变量xml配置文件的解析器
    /// xml文件的格式必须如下，并且保存在config目录下
    /// 
    /// <var>    
    ///		<name1>value1</name1>
    ///		<name2>value2</name2>
    ///		...
    ///	</var>
    ///	
    /// </summary>
    public class VarConfig
    {
        private static readonly string filename = ConstConfig.XmlPath + "Var.xml";
        static XmlNode appNode = null;
        static XmlDocument xmlDoc = null;
        static IXmlEncrypt xmlEncrypt = AesCryptHelper.GetInstance();
        static VarConfig()
        {
            LoadXml();
        }

        static void LoadXml()
        {
            TextWriter textWriter = null;
            try
            {
                if (!File.Exists(filename))
                {
                    string topNodeName = "Var";
                    textWriter = File.CreateText(filename);
                    string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<"
                                 + topNodeName + ">\r\n</" + topNodeName + ">";
                    textWriter.Write(xml);
                    textWriter.Close();
                    textWriter = null;
                }

                xmlDoc = new XmlDocument();
                xmlDoc.Load(filename);
                //appNode = xmlDoc.LastChild;

                XmlNode xmlNode = xmlDoc.LastChild;
                if (xmlNode.Name == xmlEncrypt.NodeNameplate)    //文件被加密
                {
                    string decrptXml = xmlEncrypt.Decrypt(xmlNode.InnerText.Trim());
                    xmlDoc.LoadXml(decrptXml);
                    appNode = xmlDoc.LastChild;
                }
                else
                {
                    appNode = xmlNode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(Fpi.Properties.Resources.InitFail,filename,ex.Message));
            }
            finally
            {
                if (textWriter != null)
                    textWriter.Close();
            }
        }

        public static void ReLoad()
        {
            LoadXml();
        }

        public static Hashtable GetKeyValueTable()
        {
            Hashtable table = new Hashtable();
            foreach (XmlNode node in appNode.ChildNodes)
            {
                table[node.Name] = node.InnerText;
            }
            return table;
        }

        public static string GetValue(string name)
        {
            XmlNode node = GetXmlNode(name);
            if (node == null)
                return null;
            return node.InnerText;
        }

        static XmlNode GetXmlNode(string name)
        {
            if (name == null)
                return null;
            foreach (XmlNode xn in appNode.ChildNodes) //遍历所有子节点 
            {
                if (xn.Name.ToLower().Equals(name.ToLower()))
                    return xn;
            }
            return null;
        }


        public static void SaveValue(string name, string value)
        {
            SetValue(name, value);
            Save();
        }

        public static void SetValue(string name, string value)
        {
            XmlNode node = GetXmlNode(name);
            if (node == null)
            {
                XmlNode newNode = xmlDoc.CreateElement(name);
                newNode.InnerText = value;
                appNode.AppendChild(newNode);
            }
            else
            {
                node.InnerText = value;
            }
        }

        private static object syncObj = new object();
        public static void Save()
        {
            lock (syncObj)
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder sbXmlText = new StringBuilder();

                sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n");
                sb.Append("<").Append(xmlEncrypt.NodeNameplate).Append(">").Append("\r\n");

                sbXmlText.Append("<").Append(appNode.Name).Append(">").Append("\r\n");
                for (int i = 0; i < appNode.ChildNodes.Count; i++)
                {
                    XmlNode xmlNode = appNode.ChildNodes[i];
                    sbXmlText.Append("  "+ xmlNode.OuterXml).Append("\r\n");
                }
                sbXmlText.Append("</").Append(appNode.Name).Append(">");

                sb.Append(xmlEncrypt.Encrypt(sbXmlText.ToString())).Append("\r\n");
                sb.Append("</").Append(xmlEncrypt.NodeNameplate).Append(">").Append("\r\n");

                using (TextWriter textWriter = File.CreateText(filename))
                {
                    textWriter.Write(sb.ToString());

                    textWriter.Flush();
                }
            }

        }

    }
}