using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Globalization;
using System.Xml;
using System.IO;

namespace Fpi.Xml
{
    public class XmlResouces
    {
        private DataSet xmResourceData = null;
        #region 单子模式

        private XmlResouces()
        {
            string datafile = Application.StartupPath;
            if(datafile.EndsWith(@"\"))
            {
                datafile +=   @"xml\ResourcesData.xml";
            }
            else
            {
                datafile += @"\xml\ResourcesData.xml";
            }
            if (File.Exists(datafile))
            {
                try
                {
                    xmResourceData = new DataSet();
                    xmResourceData.ReadXml(datafile);
                }
                catch (Exception)
                {
                    xmResourceData = null;
                }
               
            }
        }
        private static object syncObj = new object();
        private static XmlResouces instance = null;

        public static XmlResouces GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new XmlResouces();
                }
            }
            return instance;
        }

        #endregion

       
        /// <summary>
        /// 翻译xml串
        /// </summary>
        /// <param name="xmlstr"></param>
        /// <param name="culInfo"></param>
        public string TranslateXmlStr(string xmlName, string xmlstr, CultureInfo sourceCulInfo, CultureInfo destCulInfo)
        {
            if (xmResourceData == null) return xmlstr;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlstr);
            TranslateXml(xmlName, xmlDoc, sourceCulInfo, destCulInfo);
            return xmlDoc.InnerXml;
        }

        /// <summary>
        /// 翻译XmlDocument内容
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void TranslateXml(string xmlName, XmlDocument xmlDoc, CultureInfo sourceCulInfo, CultureInfo destCulInfo)
        {
            if (xmResourceData == null) return ;
            foreach (XmlNode xmlnode in xmlDoc.LastChild.ChildNodes)
            {
                TranslateXmlNode(xmlName,xmlnode, sourceCulInfo, destCulInfo);
            }
        }

        /// <summary>
        /// 翻译xml节点
        /// </summary>
        /// <param name="xmlnode"></param>
        /// <param name="parentNodeFullId"></param>
        private void TranslateXmlNode(string xmlName,XmlNode xmlnode, CultureInfo sourceCulInfo, CultureInfo destCulInfo)
        {
            try
            {
                if (!string.IsNullOrEmpty(xmlnode.Value))
                {
                    xmlnode.Value = GetString(xmlName, xmlnode.Value, sourceCulInfo, destCulInfo);
                }
            }
            catch
            {
            }
            if (xmlnode.ChildNodes != null)
            {
                foreach (XmlNode node in xmlnode.ChildNodes)
                {
                    TranslateXmlNode(xmlName, node, sourceCulInfo, destCulInfo);
                }
            }
        }

        /// <summary>
        /// 得到资源串
        /// </summary>
        /// <param name="xmlname"></param>
        /// <param name="keyvalue"></param>
        /// <param name="sourceCulInfo"></param>
        /// <param name="destCulInfo"></param>
        /// <returns></returns>
        private string GetString(string xmlname, string keyvalue, CultureInfo sourceCulInfo, CultureInfo destCulInfo)
        {
            string result = keyvalue;
            DataTable dt = null;
            try
            {
                dt = xmResourceData.Tables[xmlname];
            }
            catch
            {
            }
            if (dt == null) return result;
            string sourceCol = string.Empty;
            string destCol = string.Empty;
            if (sourceCulInfo == null)
            {
                sourceCol = "default";
            }
            else
            {
                sourceCol = sourceCulInfo.Name;
            }
            if (destCulInfo == null)
            {
                destCol = "default";
            }
            else
            {
                destCol = destCulInfo.Name;
            }
            string where = "[" + sourceCol + "]='" + keyvalue.Replace("'", "''") + "'";
            try
            {
                DataRow[] drs = dt.Select(where);
                if (drs != null && drs.Length > 0)
                {
                    object obj = drs[0][destCol];
                    if (obj != null && obj != DBNull.Value)
                    {
                        result = obj.ToString();
                    }
                }
            }
            catch
            {
            }
            return result;
        }

    }
}
