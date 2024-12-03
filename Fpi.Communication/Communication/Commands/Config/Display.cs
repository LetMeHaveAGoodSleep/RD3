using System;
using System.Xml;
using Fpi.Util.Sundry;
using Fpi.Xml;
using Fpi.Properties;

namespace Fpi.Communication.Commands.Config
{
    /// <summary>
    /// Display ��ժҪ˵����
    /// </summary>
    public class Display : BaseNode
    {
        public string minValue;
        public string maxValue;
        public int maxLength;
        public string items;
        public string values;
        public string defaultValue;
        public string type;
        public string formula;
        public string inverseFormula;
        public int decimalDigits; //С��λ��
        public bool supportCHN; //֧����������
        public int toBase; //����byte��int���Ͳ�������ʾ���ƣ���2,8,10,16��,Ŀǰ��֧��10������16���ơ� add by DRH


        public Display()
        {
            //
            // TODO: �ڴ˴���ӹ��캯���߼�
            //
        }


        public override BaseNode Init(XmlNode node)
        {
            decimalDigits = 2;
            toBase = 10;
            supportCHN = false;
            ;
            return base.Init(node);
        }

        public string[] GetItems()
        {
            if (items == null)
            {
                return new string[0];
            }

            string[] itemNames = items.Split(';');

            for (int i = 0; i < itemNames.Length; i++)
            {
                itemNames[i] = itemNames[i].Trim();
            }
            return itemNames;
        }

        public string[] GetValues()
        {
            if (values == null)
            {
                return new string[0];
            }

            string[] itemValues = values.Split(';');

            for (int i = 0; i < itemValues.Length; i++)
            {
                itemValues[i] = itemValues[i].Trim();
            }
            return itemValues;
        }

        public string GetItemByValue(uint val)
        {
            string[] items = this.GetItems();
            string[] values = GetValues();
            for (int i = 0; i < values.Length; i++)
            {
                if (StringUtil.ParseUInt(values[i]) == val)
                    return items[i];
            }
            return "";
        }

        public bool ContainsValue(int value)
        {
            string[] stringValues = GetValues();
            for (int i = 0; i < stringValues.Length; i++)
            {
                if (StringUtil.ParseInt(stringValues[i]) == value)
                {
                    return true;
                }
            }
            return false;
        }

        public IdNameNode[] GetIdNameItems()
        {
            string[] id = this.GetValues();
            string[] name = this.GetItems();

            if (id.Length != name.Length)
            {
                throw new Exception(Resources.ConfigFileError);
            }

            IdNameNode[] items = new IdNameNode[id.Length];

            for (int i = 0; i < id.Length; i++)
            {
                items[i] = new IdNameNode(id[i], name[i]);
            }

            return items;
        }
    }
}