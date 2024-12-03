using System.Collections;
using System.Reflection;
using System.Text;
using System;

namespace Fpi.Xml
{
    /// <summary>
    /// IdNameNode ��ժҪ˵����
    /// Xml�ڵ�,�����id��name�ֶ�
    /// </summary>
    [Serializable]
    public class IdNameNode : BaseNode, IComparer
    {
        public const string DefaultID = "DefaultID";
        public const string DefaultName = "DefaultName";

        //��ţ�key
        public string id;
        //����
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


        //ת����xml�ı���������Tagͷβ���ɾ�������ʵ��.
        protected override string GetXmlText(int depth)
        {
            StringBuilder sb = new StringBuilder();
            //��֤id�� name��xml�ڵ����ǰ��
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

        #region IComparer ��Ա

        public int Compare(object x, object y)
        {
            // TODO:  ��� IdNameNode.Compare ʵ��

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