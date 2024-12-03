using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using Fpi.Properties;
using Fpi.Util.Compress;
using Fpi.Util.Reflection;
using Fpi.Util.Security;
using Fpi.Util.Sundry;

namespace Fpi.Xml
{
    /// <summary>
    /// BaseNode ��ժҪ˵����
    /// Xml�ڵ�Ļ�����
    /// </summary>
    [Serializable]
    public class BaseNode : CloneableObject, IParentNode
    {
        #region �ֶ�����

        #region �洢���

        /// <summary>
        /// ��������
        /// </summary>
        public NodeList propertys = new NodeList();

        #endregion

        // �ö���������ֹ�����ռ�ż�����ҵ�BUG
        // ��Ϊ��̬���� modified by zyq.2009.12.18
        private static object _syncObj = new object();

        // ����Xml�����㷨 add by yjy,2010.04.24
        protected IXmlEncrypt xmlEncrypt = AesCryptHelper.GetInstance();

        // �ڵ���xml�ĵ��е�����
        protected string nodeName;
        protected string defaultFileName;
        // ��Ӧ�����ļ�
        protected string backupFileName;

        public object Tag { get; set; }

        //���ڵ�
        [UnSerializeField]
        protected object parentNode;

        //�õ�ȱʡ�����ļ�����
        public string DefaultFileName => defaultFileName;

        #endregion

        #region ����

        protected BaseNode()
        {
            //���ð��������ռ����������ΪXml�ڵ�----------------2011-7-29 �޸���:��ѵ��{
            nodeName = GetType().FullName;
            //nodeName = GetType().Name;
            //nodeName = nodeName.ToLower()[0] + nodeName.Substring(1);
            //���ð��������ռ����������ΪXml�ڵ�----------------2011-7-29 �޸���:��ѵ��}

            defaultFileName = ConstConfig.XmlPath + GetType().Name + ".xml";
            backupFileName = ConstConfig.XmlPath + GetType().Name + "_backup" + ".xml";
        }

        #endregion

        #region ����

        #region ��ʼ������

        public virtual BaseNode Init(XmlNode node)
        {
            this.nodeName = node.Name;
            foreach (XmlNode xn in node.ChildNodes) //���������ӽڵ� 
            {
                ParseXmlNode(xn.Name, xn);
            }
            return this;
        }

        #endregion

        #region Property

        public string GetPropertyValue(string id, string defaultValue)
        {
            Property prop = GetProperty(id);
            if (prop != null)
            {
                return prop.value;
            }
            return defaultValue;
        }

        public string GetPropertyValue(string id)
        {
            Property prop = GetProperty(id);
            if (prop != null)
            {
                return prop.value;
            }
            return null;
        }

        public Property GetProperty(string id)
        {
            if (propertys == null)
            {
                return null;
            }
            Property prop = (Property)propertys[id];
            return prop;
        }

        public void AddProperty(string propertyId)
        {
            Property prop = new Property(propertyId, propertyId);
            AddProperty(prop);
        }

        public void AddProperty(Property prop)
        {
            if (propertys == null)
            {
                propertys = new NodeList();
            }

            propertys.Add(prop);
        }

        public void SetProperty(string id, string value)
        {
            if (propertys == null)
            {
                propertys = new NodeList();
            }

            Property prop = (Property)propertys[id];
            if (prop == null)
            {
                prop = new Property();
                prop.id = id;
                prop.name = id;
                propertys.Add(prop);
            }
            prop.value = value;
        }

        public void SetProperty(string id, string name, string value)
        {
            if (propertys == null)
            {
                propertys = new NodeList();
            }

            Property prop = (Property)propertys[id];
            if (prop == null)
            {
                prop = new Property(id, name);
                propertys.Add(prop);
            }
            prop.value = value;
        }

        public void RemoveProperty(string id)
        {
            if (propertys == null)
                return;
            propertys.Remove(id);
        }

        //ȱʡ���Ժ������Ժϲ�
        public NodeList MergePropertys(NodeList defaultPropertys, NodeList newPropertys)
        {
            if (defaultPropertys == null)
            {
                return newPropertys;
            }
            if (newPropertys == null)
            {
                return defaultPropertys;
            }
            NodeList list = new NodeList();

            foreach (Property p in defaultPropertys)
            {
                list.Add(p.Clone());
            }

            foreach (Property p in newPropertys)
            {
                if (list.Contains(p.id))
                {
                    list.Remove(p.id);
                }
                list.Add(p.Clone());
            }
            return list;
        }

        #endregion

        #endregion

        #region ParentNode

        public virtual void SetParentNode(object parentNode)
        {
            this.parentNode = parentNode;
        }

        public object GetParentNode()
        {
            return this.parentNode;
        }

        /// <summary>��ȡ�� level �����ڵ�</summary>
        public object GetParentNode(uint level)
        {
            if (level < 1)
            {
                throw new ArgumentException(Resources.InputParam);
            }

            IParentNode parent = this;
            for (int i = 0; i < level; i++)
            {
                object ob = parent.GetParentNode();
                if (ob == null)
                {
                    return null;
                }

                parent = ob as IParentNode;
            }
            return parent;
        }

        #endregion

        #region ����

        /// <summary>
        /// ����Ĭ��XML�ļ�
        /// </summary>
        protected void loadXml()
        {
            try
            {
                //Ĭ�ϴ����ļ�����
                string fileName = defaultFileName;
                LoadXml(fileName);
            }
            catch (Exception ex)
            {
                //����Ӹ����ļ�����
                if (File.Exists(backupFileName))
                {
                    //�޸����ļ�
                    File.Copy(backupFileName, defaultFileName, true);
                    //���¼���
                    LoadXml(backupFileName);
                }
            }
        }

        /// <summary>
        /// ����XML�ļ����ַ�
        /// </summary>
        /// <param name="source"></param>
        /// <param name="isFile">����source ���ļ������ַ���</param>
        public void LoadXml(string source, bool isFile = true)
        {
            lock (_syncObj)
            {
                string oldNameSpace = NodeList.DefaultNameSpace;
                NodeList.DefaultNameSpace = this.GetType().Namespace;

                //xml�ĵ�
                XmlDocument xmlDoc = null;
                if (isFile) //xml�ļ�
                {
                    this.defaultFileName = source;
                    xmlDoc = CreateXmlDoc(source);
                    if (xmlDoc.LastChild.Name == xmlEncrypt.NodeNameplate)    //�ļ�������
                    {
                        //��xml���ݽ��н���
                        string decrptXml = xmlEncrypt.Decrypt(xmlDoc.LastChild.InnerText.Trim());
                        xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(decrptXml);
                    }
                }
                else //xml�ַ���
                {
                    xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(source);
                }
                XmlResouces.GetInstance().TranslateXml(this.GetType().Name, xmlDoc, null, Thread.CurrentThread.CurrentUICulture);
                Init(xmlDoc.LastChild);

                NodeList.DefaultNameSpace = oldNameSpace;
            }
        }

        /// <summary>
        /// xml����ʱ��������ԭʼ�ļ�û������ʱ��ֱ�Ӽ��أ���ԭʼ�ļ�������ʱ��ֱ�Ӽ��ر����ļ���ԭ
        /// </summary>
        /// <param name="xmlFile">xml�ļ�·��</param>
        /// 
        private XmlDocument CreateXmlDoc(string xmlFile)
        {
            string zipFileName = xmlFile.Replace(".xml", ".zip");
            string tempFileName = xmlFile.Replace(".xml", "_backup.xml");
            bool isZip = (!File.Exists(xmlFile) && File.Exists(zipFileName));

            XmlDocument xmlDoc = new XmlDocument();
            if (isZip)
            {
                Stream stream = CompressUtil.DecompressToStream(zipFileName);
                xmlDoc.Load(stream);
            }
            else
            {
                if (!File.Exists(xmlFile))
                {
                    //û��xml�ļ�,����һ����ʼ�ļ�
                    Save();
                }
                if (!File.Exists(xmlFile))
                {
                    throw new FileNotFoundException(string.Format(Resources.FileNotExist, xmlFile));
                }

                try
                {
                    //������ԭʼ�ļ�û������ʱ��ֱ�Ӽ���
                    xmlDoc.Load(xmlFile);
                }
                catch
                {

                    //��ԭʼ�ļ�������ʱ��ֱ�Ӽ��ر����ļ���ԭ
                    if (File.Exists(tempFileName))
                    {
                        File.Copy(tempFileName, xmlFile, true);
                        File.Delete(tempFileName);
                        xmlDoc.Load(xmlFile);
                    }
                }

            }
            return xmlDoc;
        }

        //��������
        protected void ParseXmlNode(string name, XmlNode xn)
        {
            //���ð��������ռ����������ΪXml�ڵ�----------------2011-7-29 �޸���:��ѵ��{
            FieldInfo field = null;
            if (!name.Contains("."))
            {
                field = this.GetType().GetField(name);
            }
            else
            {
                int index = name.LastIndexOf('.');
                if (index >= 0)
                {
                    string fieldName = name.ToLower()[index + 1] + name.Substring(index + 2);
                    field = this.GetType().GetField(fieldName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                }
            }
            //FieldInfo field = this.GetType().GetField(name);
            //���ð��������ռ����������ΪXml�ڵ�----------------2011-7-29 �޸���:��ѵ��}

            //XML�ļ��еĽڵ�������û�ж�Ӧ�ֶ�ʱ�������쳣-----------2011-7-29 �޸���:����ǿ{
            if (field == null)
                return;
            //throw new XmlException("not invalid node. name: " + name);
            //XML�ļ��еĽڵ�������û�ж�Ӧ�ֶ�ʱ�������쳣-----------2011-7-29 �޸���:����ǿ}

            //�ж��ǳ����򲻽��� add by zhangyq.2010.6.18
            if (field.IsLiteral)
            {
                return;
            }

            TypeCode tc = Type.GetTypeCode(field.FieldType);

            if (xn.InnerText.Length <= 0 && tc != TypeCode.Object)
            {
                return;
            }

            #region ������������ʵ��

            switch (tc)
            {
                case TypeCode.String:
                    field.SetValue(this, xn.InnerText);
                    break;
                case TypeCode.DateTime:
                    try
                    {
                        string format = "yyyyMMddHHmmss";
                        IFormatProvider culture = CultureInfo.CurrentCulture;

                        field.SetValue(this, DateTime.ParseExact(xn.InnerText, format, culture));
                    }
                    catch
                    {
                        field.SetValue(this, DateTime.MinValue);
                    }
                    break;
                case TypeCode.Int32:
                    try
                    {
                        field.SetValue(this, StringUtil.ParseInt(xn.InnerText));
                    }
                    catch
                    {
                        field.SetValue(this, Int32.MinValue);
                    }
                    break;
                case TypeCode.Single:
                    try
                    {
                        field.SetValue(this, Single.Parse(xn.InnerText));
                    }
                    catch
                    {
                        field.SetValue(this, float.NaN);
                    }
                    break;
                case TypeCode.Boolean:
                    field.SetValue(this, Boolean.Parse(xn.InnerText));
                    break;
                case TypeCode.Byte:
                    field.SetValue(this, StringUtil.ParseByte(xn.InnerText));
                    break;
                case TypeCode.Int16:
                    try
                    {
                        field.SetValue(this, StringUtil.ParseShort(xn.InnerText));
                    }
                    catch
                    {
                        field.SetValue(this, Int16.MinValue);
                    }
                    break;
                case TypeCode.UInt16:
                    try
                    {
                        field.SetValue(this, StringUtil.ParseUShort(xn.InnerText));
                    }
                    catch
                    {
                        field.SetValue(this, UInt16.MinValue);
                    }
                    break;
                case TypeCode.UInt32:
                    try
                    {
                        field.SetValue(this, StringUtil.ParseUInt(xn.InnerText));
                    }
                    catch
                    {
                        field.SetValue(this, UInt32.MinValue);
                    }
                    break;
                case TypeCode.Int64:
                    try
                    {
                        field.SetValue(this, StringUtil.ParseLong(xn.InnerText));
                    }
                    catch
                    {
                        field.SetValue(this, Int64.MinValue);
                    }
                    break;
                case TypeCode.UInt64:
                    try
                    {
                        field.SetValue(this, StringUtil.ParseULong(xn.InnerText));
                    }
                    catch
                    {
                        field.SetValue(this, UInt64.MinValue);
                    }
                    break;
                case TypeCode.Double:
                    try
                    {
                        field.SetValue(this, Double.Parse(xn.InnerText));
                    }
                    catch
                    {
                        field.SetValue(this, Double.NaN);
                    }
                    break;
                default:
                    SetObjectValue(field, xn);
                    break;
            }

            #endregion

        }

        private void SetObjectValue(FieldInfo field, XmlNode xn)
        {
            object obj;
            try
            {
                obj = ReflectionHelper.CreateInstance(field.FieldType);

                if (field.FieldType.IsAbstract) return;
                MethodInfo info = field.FieldType.GetMethod("GetInstance", BindingFlags.Static | BindingFlags.Public);
                if (info == null)
                {
                    obj = field.FieldType.Assembly.CreateInstance(field.FieldType.FullName);
                }
                else
                {
                    obj = info.Invoke(null, null);
                }
            }
            catch (Exception ex)
            {
                string error = string.Format(Resources.XmlException, field.FieldType, ex.Message);
                throw new XmlException(error);
            }

            if (obj is BaseNode)
            {
                if (xn.InnerText.Length <= 0)
                {
                    return;
                }

                (obj as IParentNode).SetParentNode(this);
                (obj as BaseNode).Init(xn);
                field.SetValue(this, obj);
            }
            else if (obj is NodeList)
            {
                (obj as IParentNode).SetParentNode(this);
                (obj as NodeList).LoadNodeList(xn);
                field.SetValue(this, obj);
            }
            else if (obj is ArrayList)
            {
                foreach (XmlNode node in xn.ChildNodes) //���������ӽڵ� 
                {
                    //Ϊ��֧��XML��ArrayList�ڵ��´��BaseNode����------------------2011.7.29 �޸���:��ѵ��{
                    string className = field.Name;
                    if (node.Name == className.Substring(0, className.Length - 1))
                    {
                        (obj as ArrayList).Add(node.InnerText.Trim());
                    }
                    else
                    {
                        try
                        {
                            object sonObj = ReflectionHelper.CreateInstance(node.Name);
                            if (sonObj is BaseNode)
                            {
                                if (node.InnerText.Length <= 0)
                                    continue;
                                (sonObj as IParentNode).SetParentNode(this);
                                (sonObj as BaseNode).Init(node);
                                (obj as ArrayList).Add(sonObj);
                            }
                            else
                            {
                                (obj as ArrayList).Add(node.InnerText.Trim());
                            }
                        }
                        catch
                        {
                            (obj as ArrayList).Add(node.InnerText.Trim());
                        }
                    }
                    //(obj as ArrayList).Add(node.InnerText.Trim());
                    //Ϊ��֧��XML��ArrayList�ڵ��´��BaseNode����------------------2011.7.29 �޸���:��ѵ��}
                }
                field.SetValue(this, obj);
            }
            else
            {
                throw new XmlException("not supported object: " + field.Name);
            }
        }

        #endregion

        #region �洢

        public virtual bool Save()
        {
            return Save(defaultFileName);
        }

        /// <summary>
        /// xml�ļ��ϵ籸��
        /// </summary>
        /// <param name="fileName">����·��</param>
        public bool Save(string fileName)
        {
            lock (_syncObj)
            {
                try
                {
                    //���ɸ����ļ�
                    string tempFileName = fileName.Replace(".xml", "_backup.xml");
                    //������д�븱��
                    using (TextWriter textWriter = File.CreateText(tempFileName))
                    {
                        textWriter.Write(ToXmlStringWithHead());
                        textWriter.Flush();
                    }
                    if (File.Exists(fileName))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        //ȥ��ֻ��Ȩ��
                        if (fi.IsReadOnly)
                        {
                            fi.IsReadOnly = false;
                        }

                        File.Delete(fileName);
                    }
                    //�������ļ���������ԭ
                    File.Copy(tempFileName, fileName, true);
                    //�޸ĳɹ���ɾ�������ļ�
                    File.Delete(tempFileName);
                    //���ó�ֻ��Ȩ��
                    File.SetAttributes(fileName, FileAttributes.ReadOnly);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        public string ToXmlStringWithHead()
        {
            string xmlText = ToXmlString();
            string head = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n";
            return head + xmlText;
        }

        public string ToXmlString()
        {
            //Mod by yjy 2010.04.24
            //Encrypt File Content
            StringBuilder sb = new StringBuilder();
            //Debug�����²����ܱ��� modified by zhangyq.2010.11.4
#if DEBUG
            string xmlstr = XmlResouces.GetInstance().TranslateXmlStr(this.GetType().Name, ToXmlString(0), Thread.CurrentThread.CurrentUICulture, null);
            sb.Append(xmlstr);

#else
            sb.Append("<").Append(xmlEncrypt.NodeNameplate).Append(">").Append("\r\n");
            string xmlstr = XmlResouces.GetInstance().TranslateXmlStr(this.GetType().Name, ToXmlString(0), Thread.CurrentThread.CurrentUICulture, null);
            sb.Append(xmlEncrypt.Encrypt(xmlstr)).Append("\r\n");
            sb.Append("</").Append(xmlEncrypt.NodeNameplate).Append(">").Append("\r\n");
#endif
            return sb.ToString();
            //return ToXmlString(0);
        }

        public string ToXmlString(int depth)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(XmlUtil.GetTab(depth)).Append("<").Append(nodeName).Append(">").Append("\r\n")
                .Append(GetXmlText(depth + 1))
                .Append(XmlUtil.GetTab(depth)).Append("</").Append(nodeName).Append(">").Append("\r\n");
            return sb.ToString();
        }

        /// <summary>
        /// ת����xml�ı���������Tagͷβ���ɾ�������ʵ��.
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        protected virtual string GetXmlText(int depth)
        {
            var sb = new StringBuilder();
            FieldInfo[] fields = this.GetType().GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].IsStatic || fields[i].IsNotSerialized || fields[i].IsLiteral)
                {
                    //��Ϊ��̬�ֶΡ����Ϊ�����л��ֶΡ��������򲻴洢
                    continue;
                }

                AddOneField(sb, fields[i], depth);
            }
            return sb.ToString();
        }

        protected void AddOneField(StringBuilder sb, FieldInfo field, int depth)
        {
            object obj = field.GetValue(this);
            if (obj is BaseNode node)
                sb.Append(node.ToXmlString(depth));
            else if (obj is NodeList nodeList)
            {
                nodeList.ManagerName = field.Name;
                sb.Append(nodeList.ToXmlString(depth));
            }
            else if (obj is ArrayList arrayList)
            {
                string arrayName = field.Name;
                string cellName = arrayName.Substring(0, arrayName.Length - 1);
                sb.Append(XmlUtil.GetTab(depth)).Append("<").Append(arrayName).Append(">").Append("\r\n");
                //Ϊ��֧��XML��ArrayList�ڵ��´��BaseNode����------------------2011.7.29 �޸���:��ѵ��{
                for (int j = 0; j < arrayList.Count; j++)
                {
                    if (arrayList[j] is BaseNode)
                    {
                        sb.Append((arrayList[j] as BaseNode).ToXmlString(depth + 1));
                    }
                    else
                    {
                        sb.Append(this.GetCellXmlText(cellName, arrayList[j], depth + 1));
                    }
                    //sb.Append(this.GetCellXmlText(cellName, ((ArrayList)obj)[j], depth + 1));
                }
                //Ϊ��֧��XML��ArrayList�ڵ��´��BaseNode����------------------2011.7.29 �޸���:��ѵ��}

                sb.Append(XmlUtil.GetTab(depth)).Append("</").Append(arrayName).Append(">").Append("\r\n");
            }
            else
            {
                sb.Append(this.GetCellXmlText(field.Name, obj, depth));
            }
        }

        //�õ�����xml��Ԫ�ı� ���磺<name>value</name>
        protected string GetCellXmlText(string name, object value, int depth)
        {
            if (value == null)
                return "";
            StringBuilder sb = new StringBuilder();
            sb.Append(XmlUtil.GetTab(depth)).Append("<").Append(name).Append(">")
                .Append((value is string)
                            ? (value as string).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                            : value)
                .Append("</").Append(name).Append(">").Append("\r\n");
            return sb.ToString();
        }

        #region ����������

        /// <summary>
        /// �ö�������ʽ����
        /// </summary>
        /// <returns></returns>
        public virtual bool SaveWithShortName()
        {
            return SaveWithShortName(defaultFileName);
        }

        /// <summary>
        /// �ö�������ʽ����
        /// </summary>
        /// <param name="fileName"></param>
        public virtual bool SaveWithShortName(string fileName)
        {
            lock (_syncObj)
            {
                try
                {
                    //���ɸ����ļ�
                    string tempFileName = fileName.Replace(".xml", "_backup.xml");
                    //������д�븱��
                    using (TextWriter textWriter = File.CreateText(tempFileName))
                    {
                        textWriter.Write(ToXmlStringWithShortNameWithHead());
                        textWriter.Flush();
                    }
                    if (File.Exists(fileName))
                    {
                        FileInfo fi = new FileInfo(fileName);
                        //ȥ��ֻ��Ȩ��
                        if (fi.IsReadOnly)
                        {
                            fi.IsReadOnly = false;
                        }

                        File.Delete(fileName);
                    }
                    //�������ļ���������ԭ
                    File.Copy(tempFileName, fileName, true);
                    //�޸ĳɹ���ɾ�������ļ�
                    File.Delete(tempFileName);
                    //���ó�ֻ��Ȩ��
                    File.SetAttributes(fileName, FileAttributes.ReadOnly);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        public string ToXmlStringWithShortNameWithHead()
        {
            string xmlText = ToXmlStringWithShortName();
            string head = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n";
            return head + xmlText;
        }

        public string ToXmlStringWithShortName()
        {
            //Mod by yjy 2010.04.24
            //Encrypt File Content
            StringBuilder sb = new StringBuilder();
            //Debug�����²����ܱ��� modified by zhangyq.2010.11.4
#if DEBUG
            string xmlstr = XmlResouces.GetInstance().TranslateXmlStr(this.GetType().Name, ToXmlStringWithShortName(0), Thread.CurrentThread.CurrentUICulture, null);
            sb.Append(xmlstr);

#else
            sb.Append("<").Append(xmlEncrypt.NodeNameplate).Append(">").Append("\r\n");
            string xmlstr = XmlResouces.GetInstance().TranslateXmlStr(this.GetType().Name, ToXmlString(0), Thread.CurrentThread.CurrentUICulture, null);
            sb.Append(xmlEncrypt.Encrypt(xmlstr)).Append("\r\n");
            sb.Append("</").Append(xmlEncrypt.NodeNameplate).Append(">").Append("\r\n");
#endif
            return sb.ToString();
            //return ToXmlString(0);
        }

        public string ToXmlStringWithShortName(int depth)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(XmlUtil.GetTab(depth)).Append("<").Append(GetType().Name).Append(">").Append("\r\n")
                .Append(GetXmlTextWithShortName(depth + 1))
                .Append(XmlUtil.GetTab(depth)).Append("</").Append(GetType().Name).Append(">").Append("\r\n");
            return sb.ToString();
        }

        //ת����xml�ı���������Tagͷβ���ɾ�������ʵ��.
        protected virtual string GetXmlTextWithShortName(int depth)
        {
            StringBuilder sb = new StringBuilder();
            FieldInfo[] fields = this.GetType().GetFields();
            foreach (FieldInfo field in fields.Where(field => !field.IsStatic && !field.IsNotSerialized && !field.IsLiteral))
            {
                AddOneFieldWithShortName(sb, field, depth);
            }
            return sb.ToString();
        }

        protected void AddOneFieldWithShortName(StringBuilder sb, FieldInfo field, int depth)
        {
            object obj = field.GetValue(this);
            if (obj is BaseNode node)
                sb.Append(node.ToXmlStringWithShortName(depth));
            else if (obj is NodeList nodeList)
            {
                nodeList.ManagerName = field.Name;
                sb.Append(nodeList.ToXmlStringWithShortName(depth));
            }
            else if (obj is ArrayList list)
            {
                string arrayName = field.Name;
                string cellName = arrayName.Substring(0, arrayName.Length - 1);
                sb.Append(XmlUtil.GetTab(depth)).Append("<").Append(arrayName).Append(">").Append("\r\n");
                //Ϊ��֧��XML��ArrayList�ڵ��´��BaseNode����------------------2011.7.29 �޸���:��ѵ��{
                foreach (object o in list)
                {
                    if (o is BaseNode baseNode)
                    {
                        sb.Append(baseNode.ToXmlStringWithShortName(depth + 1));
                    }
                    else
                    {
                        sb.Append(this.GetCellXmlText(cellName, o, depth + 1));
                    }
                    //sb.Append(this.GetCellXmlText(cellName, ((ArrayList)obj)[j], depth + 1));
                }
                //Ϊ��֧��XML��ArrayList�ڵ��´��BaseNode����------------------2011.7.29 �޸���:��ѵ��}

                sb.Append(XmlUtil.GetTab(depth)).Append("</").Append(arrayName).Append(">").Append("\r\n");
            }
            else
                sb.Append(this.GetCellXmlText(field.Name, obj, depth));
        }

        #endregion

        #endregion

        #region ����

        public string GetFileText()
        {
            string path = defaultFileName;
            TextReader textReader = null;
            try
            {
                textReader = File.OpenText(path);
                return textReader.ReadToEnd();
            }
            catch (Exception)
            {
                return string.Empty;
            }
            finally
            {
                if (textReader != null)
                    textReader.Close();
            }
        }

        public object GetTopBaseNode()
        {
            return _GetTopBaseNode(this);
        }

        private object _GetTopBaseNode(IParentNode node)
        {
            object parent = node.GetParentNode();
            if (parent == null)
            {
                return node;
            }
            else
            {
                return _GetTopBaseNode((IParentNode)parent);
            }
        }

        public virtual string GetNodeDescription()
        {
            return this.GetType().Name;
        }

        #endregion
    }
}