using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Fpi.Util.Serializes.CustomSerializer
{
    public class FontSerializer : SerializerBase<Font>
    {
        public override void GetData(Font item, System.Runtime.Serialization.SerializationInfo info)
        {
            info.AddValue("name", item.Name);
            info.AddValue("size", item.Size);
            info.AddValue("underline", item.Underline);
            info.AddValue("strikeout", item.Strikeout);
            info.AddValue("italic", item.Italic);
            info.AddValue("bold", item.Bold);

        }

        public override Font SetData(Font item, System.Runtime.Serialization.SerializationInfo info)
        {
            string name = info.GetString("name");
            string size = info.GetString("size");
            bool underline = info.GetBoolean("underline");
            bool strikeout = info.GetBoolean("strikeout");
            bool italic = info.GetBoolean("italic");
            bool bold = info.GetBoolean("bold");
            FontStyle fs = FontStyle.Regular;
            if (underline)
            {
                fs = fs | FontStyle.Underline;
            }
            if (strikeout)
            {
                fs = fs | FontStyle.Strikeout;
            }
            if (italic)
            {
                fs = fs | FontStyle.Italic;
            }
            if (bold)
            {
                fs = fs | FontStyle.Bold;
            }
            return new Font(name, Convert.ToSingle(size), fs);
        }
    }
}
