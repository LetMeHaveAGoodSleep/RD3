using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using Fpi.Util.Security;
using Fpi.Properties;

namespace Fpi.Xml
{
    /// <summary>
    /// ConstConfig ��ժҪ˵����
    /// ����xml�����ļ��Ľ�����
    /// xml�ļ��ĸ�ʽ�������£����ұ�����configĿ¼��
    /// 
    /// <const>    
    ///		<name1>value1</name1>
    ///		<name2>value2</name2>
    ///		...
    ///	</const>
    ///	
    /// </summary>
    public class ConstConfig
    {
        private static readonly Hashtable table = new Hashtable();

        private static readonly IXmlEncrypt xmlEncrypt = AesCryptHelper.GetInstance();

        public static string AppPath { get; set; } = Application.StartupPath;

        static ConstConfig()
        {
            LoadXml();
        }

        static void LoadXml()
        {
            TextWriter textWriter = null;
            try
            {
                if (!File.Exists(FileName))
                {
                    string topNodeName = "Const";
                    textWriter = File.CreateText(FileName);
                    string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<"
                                 + topNodeName + ">\r\n</" + topNodeName + ">";
                    textWriter.Write(xml);
                    textWriter.Close();
                    textWriter = null;
                }

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(FileName);
                //XmlNode appNode = xmlDoc.LastChild;

                XmlNode appNode = xmlDoc.LastChild;
                if (appNode.Name == xmlEncrypt.NodeNameplate)    //�ļ�������
                {
                    string decrptXml = xmlEncrypt.Decrypt(appNode.InnerText.Trim());

                    XmlDocument tXmlDoc = new XmlDocument();
                    tXmlDoc.LoadXml(decrptXml);

                    appNode = tXmlDoc.LastChild;
                }

                foreach (XmlNode xn in appNode.ChildNodes) //���������ӽڵ� 
                {
                    table.Add(xn.Name.ToLower(), xn.InnerText);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format(Resources.InitFail,FileName,ex.Message));
            }
            finally
            {
                if (textWriter != null)
                    textWriter.Close();
            }
        }

        public static string GetPath(string type)
        {
            return Path.Combine(AppPath, type)+"\\";
        }

        public static string DBPath => GetPath("DB");

        public static string LogPath => GetPath("Log");

        public static string BackPath => GetPath("Back");

        public static string XmlPath => GetPath("Config");

        public static string ImgPath
        {
            get 
            {
                //Ϊ��֧�ֶ����ԣ�ͼƬ����������Ϣ��ͬ���ڲ�ͬ·����
                //add by zhangyq.2011-7-6
                string cultureName = System.Globalization.CultureInfo.CurrentCulture.Name;
                if (cultureName.Equals("zh-CN"))
                {
                    return GetPath("Images");
                }
                else
                {
                    return GetPath(@"Images\" + cultureName);
                }
            }
        }

        public static string TemplatePath => GetPath("Template");

        private static  string FileName => Path.Combine(XmlPath, "Const.xml");

        public static Hashtable GetKeyValueTable()
        {
            return table;
        }

        public static void ReLoad()
        {
            table.Clear();
#if WINCE	
            lock(FileSystem.fileLockObj)
			{
				LoadXml();
			}
#else
            LoadXml();
#endif
        }

        public static string GetValue(string name)
        {
            return (string)table[name.ToLower()];
        }
    }
}