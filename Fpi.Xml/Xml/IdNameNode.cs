using System.Collections;
using System.Reflection;
using System.Text;
using System;

namespace Fpi.Xml
{
    /// <summary>
    /// IdNameNode 的摘要说明。
    /// Xml节点,必须带id和name字段
    /// </summary>
    [Serializable]
    public class IdNameNode : BaseNode, IComparer
    {
        public const string DefaultID = "DefaultID";
        public const string DefaultName = "DefaultName";

        //编号，key
        public string id;
        //名称
        public string name;

        public IdNameNode() : base()
        {
            id = DefaultID;
            name = DefaultName;
        }

        public IdNameNode(string id) : base()
        {
            this.id = id;
            name = DefaultName;
        }

        public IdNameNode(string id, string name) : base()
        {
            this.id = id;
            this.name = name;
        }


        //转换成xml文本，不包括Tag头尾，由具体子类实现.
        protected override string GetXmlText(int depth)
        {
            StringBuilder sb = new StringBuilder();
            //保证id， name在xml节点的最前面
            sb.Append(GetCellXmlText("id", id, depth));
            sb.Append(GetCellXmlText("name", name, depth));

            FieldInfo[] fields = this.GetType().GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                if (!fields[i].Name.Equals("id") && !fields[i].Name.Equals("name") && !fields[i].IsLiteral)
                {
                    AddOneField(sb, fields[i], depth);
                }
            }
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is string)
                return id.Equals((string) obj);
            if (obj is IdNameNode)
                return id.Equals(((IdNameNode) obj).id);
            return false;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            if (name == null)
            {
                return string.Empty;
            }
            else
            {
                return name;
            }
        }

        #region IComparer 成员

        public int Compare(object x, object y)
        {
            // TODO:  添加 IdNameNode.Compare 实现

            if ((x is IdNameNode) && (y is IdNameNode))
            {
                IdNameNode a = (IdNameNode) x;
                IdNameNode b = (IdNameNode) y;

                return a.id.CompareTo(b.id);
            }
            return 0;
        }

        #endregion
    }
}