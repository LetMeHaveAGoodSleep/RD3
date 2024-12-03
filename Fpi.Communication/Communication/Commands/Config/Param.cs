using System.Xml;
using Fpi.Xml;

namespace Fpi.Communication.Commands.Config
{
    /// <summary>
    /// Param 的摘要说明。
    /// </summary>
    public class Param : IdNameNode
    {
        public string section;
        public string type;
        public int length;
        public string unit;
        public bool visible;
        public Display display;

        public Param()
        {
            this.length = 1;
            this.valid = true;
            this.visible = true;
        }

        public override BaseNode Init(XmlNode node)
        {
            this.valid = true;
            this.visible = true;
            return base.Init(node);
        }


        private int offset;
        private int bitOffset;

        public int GetOffset()
        {
            return offset;
        }

        public void SetOffset(int offset)
        {
            this.offset = offset;
        }

        public int GetBitOffset()
        {
            return bitOffset;
        }

        public void SetBitOffset(int bitOffset)
        {
            this.bitOffset = bitOffset;
        }

        public bool IsIntType()
        {
            string tempType = type;
            //if ((display != null) && (display.type != null))
            //	tempType = display.type;
            return (tempType == "bit") || (tempType == "byte") || (tempType == "int") || (tempType == "long")
                   || (tempType == "uint") || (tempType == "ulong");
        }

        private bool valid;

        public void SetValid(bool valid)
        {
            this.valid = valid;
        }

        public bool GetValid()
        {
            return this.valid;
        }
    }
}