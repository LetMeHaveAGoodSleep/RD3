using System;
using System.Collections;
using System.Text;
using System.Xml;
using Fpi.Util.Sundry;
using Fpi.Util.Reflection;

namespace Fpi.Xml
{
    /// <summary>
    /// NodeList 
    /// ����XML�������ӽڵ㡣
    /// </summary>
    [Serializable]
    public class NodeList : CloneableObject, IEnumerable, IParentNode, ITable
    {
        //���ýڵ�namespace, �ɸ��ݾ���Ӧ���޸�
        public static string DefaultNameSpace = "Fpi.Communication.Commands";

        private string _nameSapce;

        public string NameSapce
        {
            get => this._nameSapce;
            set => this._nameSapce = value;
        }

        //ͬ������
        protected static object SyncObj = new object();

        //�ڵ����������

        public string ManagerName { get; set; }

        public NodeList()
        {
            this._nameSapce = DefaultNameSpace;
        }

        //���������ӽڵ������
        private ArrayList _list = new ArrayList();

        public ArrayList List
        {
            get => this._list;
            set => this._list = value;
        }

        public ArrayList GetNodeList()
        {
            return _list;
        }

        public IdNameNode[] GetItems()
        {
            IdNameNode[] items = new IdNameNode[this._list.Count];
            this._list.CopyTo(items);
            return items;
        }

        public int IndexOf(object value)
        {
            return _list.IndexOf(value);
        }

        public void AddNode(IdNameNode node)
        {
            _list.Add(node);
        }

        public void Remove(string id)
        {
            _list.Remove(this[id]);
        }

        public bool Contains(string id)
        {
            return (FindNode(id) != null);
        }

        public int GetCount()
        {
            return _list.Count;
        }

        public IdNameNode FindNode(string id)
        {
            foreach (IdNameNode node in _list)
            {
                if (node.id.Equals(id))
                    return node;
            }
            return null;
        }

        public IdNameNode FindNode(int id)
        {
            foreach (IdNameNode node in _list)
            {
                try
                {
                    int nodeId = StringUtil.ParseInt(node.id);
                    if (id == nodeId)
                        return node;
                }
                catch (Exception)
                {
                }
            }
            return null;
        }

        public IdNameNode FindNodeByName(string name)
        {
            foreach (IdNameNode node in _list)
            {
                if (name.Equals(node.name))
                    return node;
            }
            return null;
        }


        public IdNameNode this[int index]
        {
            get
            {
                if ((index < 0) || (index >= _list.Count))
                    return null;
                return (IdNameNode)_list[index];
            }
            set
            {
                if ((index < 0) || (index >= _list.Count))
                    return;
                _list[index] = value;
            }
        }

        public IdNameNode this[string id]
        {
            get => FindNode(id);
            set
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    if (this[i].id.Equals(id))
                    {
                        this[i] = value;
                        return;
                    }
                }
            }
        }

        public int GetIndex(string id)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                if ((_list[i] as IdNameNode).id.Equals(id))
                {
                    return i;
                }
            }
            return -1;
        }

        public void SetIndex(string id, int index)
        {
            if (index < 0 || index > _list.Count - 1)
            {
                throw new Exception(string.Format(Fpi.Properties.Resources.ParamOutScope, "index"));
            }

            int x = GetIndex(id);
            object ob = _list[index];

            _list[index] = _list[x];
            _list[x] = ob;
        }


        //���ڵ�
        [UnSerializeFieldAttribute]
        protected object parentNode;

        public void SetParentNode(object parentNode)
        {
            this.parentNode = parentNode;
        }

        public object GetParentNode()
        {
            return this.parentNode;
        }

        //��������ӽڵ�
        public void LoadNodeList(XmlNode node)
        {
            this.ManagerName = node.Name;
            XmlNodeList nodeList = node.ChildNodes;

            foreach (XmlNode xn in nodeList) //���������ӽڵ� 
            {
                string className = xn.Name;
                if (className.ToLower().Equals("property"))
                {
                    className = typeof(Property).FullName;
                }
                else
                {
                    //���ð��������ռ����������ΪXml�ڵ�----------------2011-7-29 �޸���:��ѵ��{
                    if (!className.Contains(".")) //����ԭ��XML�ĵ�
                    {
                        className = this._nameSapce + "." + className.ToUpper().Substring(0, 1) + className.Substring(1);
                    }
                    //className = this.nameSapce + "." + className.ToUpper().Substring(0, 1) + className.Substring(1);
                    //���ð��������ռ����������ΪXml�ڵ�----------------2011-7-29 �޸���:��ѵ��}
                    Type type = ReflectionHelper.FindType(className);
                    if (type == null)
                    {
                        type = GetInstanceType(xn.Name);
                        if (type != null)
                        {
                            className = type.FullName;
                        }
                    }
                }

                object obj;
                try
                {
                    obj = ReflectionHelper.CreateInstance(className);
                    (obj as IParentNode).SetParentNode(this);
                }
                catch (NullReferenceException ex)
                {
                    throw new XmlException(className + " not exits. " + ex.Message);
                }
                try
                {
                    _list.Add((obj as IdNameNode).Init(xn));
                }
                catch (NullReferenceException ex)
                {
                    throw new XmlException(className + " should be IdNameNode class or Init error. " + ex.Message);
                }
            }
        }

        public string ToXmlString()
        {
            return ToXmlString(0);
        }

        //ת����XML��ʽ�ı�
        public string ToXmlString(int depth)
        {
            StringBuilder sb = new StringBuilder();
            if (_list.Count <= 0)
            {
                return "";
            }

            sb.Append(XmlUtil.GetTab(depth)).Append("<").Append(ManagerName).Append(">").Append("\r\n");
            for (int i = 0; i < _list.Count; i++)
            {
                sb.Append(((BaseNode)_list[i]).ToXmlString(depth + 1)).Append("\r\n");
            }
            sb.Append(XmlUtil.GetTab(depth)).Append("</").Append(ManagerName).Append(">")
                .Append("\r\n").Append("\r\n");
            return sb.ToString();
        }

        /// <summary>
        /// �ö�������ʽ���棬ת����XML��ʽ�ı�
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public string ToXmlStringWithShortName(int depth)
        {
            StringBuilder sb = new StringBuilder();
            if (_list.Count <= 0)
            {
                return "";
            }

            sb.Append(XmlUtil.GetTab(depth)).Append("<").Append(ManagerName).Append(">").Append("\r\n");
            for (int i = 0; i < _list.Count; i++)
            {
                sb.Append(((BaseNode)_list[i]).ToXmlStringWithShortName(depth + 1)).Append("\r\n");
            }
            sb.Append(XmlUtil.GetTab(depth)).Append("</").Append(ManagerName).Append(">")
                .Append("\r\n").Append("\r\n");
            return sb.ToString();
        }

        #region IEnumerable ��Ա

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region ITable ��Ա

        public bool Add(object newRecord)
        {
            if (newRecord == null)
            {
                return false;
            }
            // TODO:  ��� NodeList.Add ʵ��
            IdNameNode node = (IdNameNode)newRecord;
            if (!this.Contains(node.id))
            {
                this.AddNode(node);
                return true;
            }
            return false;
        }

        public bool Remove(object existRecord)
        {
            if (existRecord == null)
            {
                return false;
            }
            // TODO:  ��� NodeList.Fpi.Xml.ITable.Remove ʵ��
            IdNameNode node = (IdNameNode)existRecord;
            if (this.Contains(node.id))
            {
                this.Remove(node.id);
                return true;
            }
            return false;
        }

        public bool Update(object oldRecord, object newRecord)
        {
            // TODO:  ��� NodeList.Update ʵ��
            IdNameNode node = (IdNameNode)oldRecord;
            if (this.Contains(node.id))
            {
                int index = this.GetIndex(node.id);
                this._list[index] = newRecord;
                return true;
            }
            return false;
        }

        public int GetIndex(object record)
        {
            // TODO:  ��� NodeList.Fpi.Xml.ITable.GetIndex ʵ��
            IdNameNode node = (IdNameNode)record;
            if (this.Contains(node.id))
            {
                return this.GetIndex(node.id);
            }
            return -1;
        }

        public object GetRecord(int index)
        {
            if (this._list.Count > index && index > 0)
            {
                return this._list[index];
            }
            return null;
        }


        public void Sort()
        {
            // TODO:  ��� NodeList.Sort ʵ��
            this._list.Sort((IComparer)NodeListCompare.GetInstance());
        }

        public void Reverse()
        {
            // TODO:  ��� NodeList.Reverse ʵ��
            this._list.Reverse();
        }

        public void Clear()
        {
            // TODO:  ��� NodeList.Clear ʵ��
            this._list.Clear();
        }

        public void Dispose()
        {
            Clear();
            this._list = null;
        }

        #endregion


        #region NodeList ��Ա���ͻ�ȡ�������ò�ͬ·��

        Type memberType;
        Type[] types;

        /// <summary>
        /// NodeList ��Ա���ͻ�ȡ�������ò�ͬ·��
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        Type GetInstanceType(string className)
        {
            if (types == null)
            {
                types = GetTypes();
            }

            foreach (Type t in types)
            {
                if (t.Name.ToLower().EndsWith(className.ToLower()))
                {
                    return t;
                }
            }
            return null;
        }

        Type[] GetTypes()
        {
            if (memberType == null)
            {
                memberType = GetMemberType();
            }
            Type[] types = ReflectionHelper.GetChildTypes(memberType);
            return types;
        }

        /// <summary>��ȡ��Ա������</summary>
        Type GetMemberType()
        {
            string className = ManagerName[0].ToString().ToUpper() + ManagerName.Substring(1, ManagerName.Length - 2);
            string typeName = _nameSapce + "." + className;

            Type type = ReflectionHelper.FindType(typeName);
            return type;
        }

        #endregion

        #region ICloneable ��Ա
        public override object Clone()
        {

            NodeList ob = new NodeList();
            ArrayList list = new ArrayList();

            foreach (IdNameNode node in this._list)
            {
                IdNameNode newnode = node.Clone() as IdNameNode;
                (newnode as IParentNode).SetParentNode(this);
                list.Add(newnode);

            }
            ob.List = list;

            return ob;

        }


        #endregion
    }

    public class NodeListCompare : IComparer
    {
        private static object syncObj = new object();
        private static NodeListCompare instance = null;

        public static NodeListCompare GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new NodeListCompare();
                }
            }
            return instance;
        }

        private NodeListCompare()
        {
        }

        #region IComparer ��Ա

        public int Compare(object x, object y)
        {
            if ((x is IdNameNode) && (y is IdNameNode))
            {
                IdNameNode a = (IdNameNode)x;
                IdNameNode b = (IdNameNode)y;

                return a.id.CompareTo(b.id);
            }
            return 0;
        }

        #endregion
    }
}