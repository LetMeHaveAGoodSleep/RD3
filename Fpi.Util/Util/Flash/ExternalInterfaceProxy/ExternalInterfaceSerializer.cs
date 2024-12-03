using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;
using System.IO;

namespace Fpi.Util.Flash.ExternalInterfaceProxy
{
    /// <summary>
    /// c#与Flash之间的通讯是通过XML格式传输的
    /// 此类负责解析与封装成xml格式消息及.NET对象的任务的类
    /// </summary>
    public class ExternalInterfaceSerializer
    {

        private ExternalInterfaceSerializer()
        { }

        /// <summary>
        /// 将函数名和C# ArrayList类型的参数编码为相应的XML格式
        /// </summary>
        public static string EncodeInvoke(string functionName, object[] arguments)
        {
            StringBuilder sb = new StringBuilder();
            XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb));

            // <invoke name="functionName" returntype="xml">
            writer.WriteStartElement("invoke");
            writer.WriteAttributeString("name", functionName);
            writer.WriteAttributeString("returntype", "xml");

            if (arguments != null && arguments.Length > 0)
            {
                // <arguments>
                writer.WriteStartElement("arguments");

                // individual arguments
                foreach (object value in arguments)
                {
                    WriteElement(writer, value);
                }

                // </arguments>
                writer.WriteEndElement();
            }

            // </invoke>
            writer.WriteEndElement();

            writer.Flush();
            writer.Close();

            return sb.ToString();
        }

        /// <summary>
        /// 将结果值编码为相应的XML格式
        /// </summary>
        public static string EncodeResult(object value)
        {
            StringBuilder sb = new StringBuilder();
            XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb));

            WriteElement(writer, value);

            writer.Flush();
            writer.Close();

            return sb.ToString();
        }


        /// <summary>
        /// 对来自ActionScript的函数调用进行解码
        /// FlashCall事件对象的request属性被传递给 DecodeInvoke()方法
        /// 同时它将调用转化为一个ExternalInterfaceCall对象
        /// </summary>
        public static ExternalInterfaceCall DecodeInvoke(string xml)
        {
            XmlTextReader reader = new XmlTextReader(xml, XmlNodeType.Document, null);
            reader.Read();

            string functionName = reader.GetAttribute("name");
            ExternalInterfaceCall result = new ExternalInterfaceCall(functionName);

            reader.ReadStartElement("invoke");
            reader.ReadStartElement("arguments");

            while (reader.NodeType != XmlNodeType.EndElement && reader.Name != "arguments")
            {
                result.AddArgument(ReadElement(reader));
            }

            reader.ReadEndElement();
            reader.ReadEndElement();

            return result;
        }


        /// <summary>
        /// 对作为ActionScript函数调用的结果接收的XML进行解码
        /// </summary>
        public static object DecodeResult(string xml)
        {
            XmlTextReader reader = new XmlTextReader(xml, XmlNodeType.Document, null);
            reader.Read();
            return ReadElement(reader);
        }


        #region 私有方法

        private static void WriteElement(XmlTextWriter writer, object value)
        {
            if (value == null)
            {
                writer.WriteStartElement("null");
                writer.WriteEndElement();
            }
            else if (value is string)
            {
                writer.WriteStartElement("string");
                writer.WriteString(value.ToString());
                writer.WriteEndElement();
            }
            else if (value is bool)
            {
                writer.WriteStartElement((bool)value ? "true" : "false");
                writer.WriteEndElement();
            }
            else if (value is Single || value is Double || value is int || value is uint)
            {
                // ActionScrit1.0/2.0中数据的类型只有Number
                // ActionScrit13.0中数据类型有int/uint/Number
                // 这里统一为Number类型
                writer.WriteStartElement("number");
                writer.WriteString(value.ToString());
                writer.WriteEndElement();
            }
            else if (value is ArrayList)
            {
                WriteArray(writer, (ArrayList)value);
            }
            else if (value is Hashtable)
            {
                WriteObject(writer, (Hashtable)value);
            }
            else
            {
                //若参数格式不能序列话，则缺省设置为null型
                writer.WriteStartElement("null");
                writer.WriteEndElement();
            }
        }


        private static void WriteArray(XmlTextWriter writer, ArrayList array)
        {
            writer.WriteStartElement("array");

            int len = array.Count;

            for (int i = 0; i < len; i++)
            {
                writer.WriteStartElement("property");
                writer.WriteAttributeString("id", i.ToString());
                WriteElement(writer, array[i]);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }


        private static void WriteObject(XmlTextWriter writer, Hashtable table)
        {
            writer.WriteStartElement("object");

            foreach (DictionaryEntry entry in table)
            {
                writer.WriteStartElement("property");
                writer.WriteAttributeString("id", entry.Key.ToString());
                WriteElement(writer, entry.Value);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }


        private static object ReadElement(XmlTextReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element)
            {
                throw new XmlException();
            }

            if (reader.Name == "true")
            {
                reader.Read();
                return true;
            }

            if (reader.Name == "false")
            {
                reader.Read();
                return false;
            }

            if (reader.Name == "null" || reader.Name == "undefined")
            {
                reader.Read();
                return null;
            }

            if (reader.IsStartElement("number"))
            {
                reader.ReadStartElement("number");
                double value = Double.Parse(reader.Value);
                reader.Read();
                reader.ReadEndElement();
                return value;
            }

            if (reader.IsStartElement("string"))
            {
                reader.ReadStartElement("string");
                string value = reader.Value;
                reader.Read();
                reader.ReadEndElement();
                return value;
            }

            if (reader.IsStartElement("array"))
            {
                reader.ReadStartElement("array");
                ArrayList value = ReadArray(reader);
                reader.ReadEndElement();
                return value;
            }

            if (reader.IsStartElement("object"))
            {
                reader.ReadStartElement("object");
                Hashtable value = ReadObject(reader);
                reader.ReadEndElement();
                return value;
            }
            throw new XmlException();
        }


        private static ArrayList ReadArray(XmlTextReader reader)
        {
            ArrayList result = new ArrayList();

            while (reader.NodeType != XmlNodeType.EndElement && reader.Name != "array")
            {
                int id = int.Parse(reader.GetAttribute("id"));
                reader.ReadStartElement("property");
                result.Add(ReadElement(reader));
                reader.ReadEndElement();
            }

            return result;
        }


        private static Hashtable ReadObject(XmlTextReader reader)
        {
            Hashtable result = new Hashtable();

            while (reader.NodeType != XmlNodeType.EndElement && reader.Name != "object")
            {
                string id = reader.GetAttribute("id");
                reader.ReadStartElement("property");
                result.Add(id, ReadElement(reader));
                reader.ReadEndElement();
            }

            return result;
        }

        #endregion
    }
}
