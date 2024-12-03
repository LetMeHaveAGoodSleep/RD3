using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
namespace Fpi.Util.Serializes.CustomSerializer
{
    public class XmlDataObject : DataOjbect
    {
        /// <summary>
        /// 根节点名称
        /// </summary>
        private string root = "Root";

        /// <summary>
        /// XML根节点名称
        /// </summary>
        public virtual string RootName
        {
            get { return this.root; }

            set { this.root = value; }
        }

        public override bool Load(System.IO.Stream input)
        {
            XPathDocument xpd = new XPathDocument(input);
            XPathNavigator xtor = xpd.CreateNavigator();
            xtor.MoveToRoot();
            xtor.MoveToChild(XPathNodeType.Element);
            XPathNodeIterator subNode = xtor.SelectChildren(XPathNodeType.Element);
            foreach (XPathNavigator xn in subNode)
            {
                this.ReadMember(xn);
            }

            return true;
        }

        public virtual bool ReadMember(XPathNavigator subNode)
        {
            return false;
        }

        public override void Save(System.IO.Stream output)
        {
            this.Save(output, this.RootName);
        }

        public void WriteAttribute(XmlTextWriter writer, string name, object value)
        {
            if (value != null)
            {
                writer.WriteStartAttribute(name, string.Empty);
                writer.WriteValue(value);
                writer.WriteEndAttribute();
            }
        }

        public virtual void Save(Stream output, string rootName)
        {
            XmlTextWriter xtw = new XmlTextWriter(output, ASCIIEncoding.Unicode);
            xtw.Formatting = Formatting.Indented;
            xtw.WriteStartDocument(true);
            xtw.WriteStartElement(rootName);
            this.WriteMember(xtw);
            xtw.WriteEndElement();
            xtw.Flush();
        }

        public virtual void SaveTo(XmlTextWriter writer, string rootName, IEnumerable<KeyValuePair<string, string>> attributes)
        {
            writer.WriteStartElement(rootName);
            if (attributes != null)
            {
                foreach (KeyValuePair<string, string> attri in attributes)
                {
                    if (string.IsNullOrEmpty(attri.Key))
                    {
                        continue;
                    }

                    if (attri.Value == null)
                    {
                        continue;
                    }

                    writer.WriteAttributeString(attri.Key, attri.Value.ToString());
                }
            }

            this.WriteMember(writer);
            writer.WriteEndElement();
            writer.Flush();
        }

        public virtual bool LoadFrom(XPathNavigator reader,IDictionary<string,string> attributes)
        {
            XPathNodeIterator attrs = reader.SelectChildren(XPathNodeType.Attribute);
            foreach (XPathNavigator subNode in attrs)
            {
                attributes.Add(subNode.Name, subNode.Value);
            }

            XPathNodeIterator subNodes = reader.SelectChildren(XPathNodeType.Element);
            foreach (XPathNavigator subNode in subNodes)
            {
                this.ReadMember(subNode);
            }

            return true;
        }

        public virtual void WriteMember(XmlTextWriter writer)
        {

        }
    }
}
