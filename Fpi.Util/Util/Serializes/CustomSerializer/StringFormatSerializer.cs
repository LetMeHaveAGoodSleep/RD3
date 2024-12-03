using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Fpi.Util.Serializes.CustomSerializer
{
    public class StringFormatSerializer : SerializerBase<StringFormat>
    {
        public override void GetData(StringFormat item, System.Runtime.Serialization.SerializationInfo info)
        {
            info.AddValue("alignment", item.Alignment.ToString());
            info.AddValue("lineAlignment", item.LineAlignment.ToString());
        }

        public override StringFormat SetData(StringFormat item, System.Runtime.Serialization.SerializationInfo info)
        {
            string alignment = info.GetString("alignment");
            string lineAlignment = info.GetString("linealignment");
            item.Alignment = (StringAlignment)Enum.Parse(typeof(StringAlignment), alignment);
            item.LineAlignment = (StringAlignment)Enum.Parse(typeof(StringAlignment), lineAlignment);
            return item;
        }
    }
}
