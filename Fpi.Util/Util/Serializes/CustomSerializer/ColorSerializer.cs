using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Fpi.Util.Serializes.CustomSerializer
{
    public class ColorSerializer : SerializerBase<Color>
    {
        public override void GetData(Color item, System.Runtime.Serialization.SerializationInfo info)
        {
            info.AddValue("a", item.A);
            info.AddValue("r", item.R);
            info.AddValue("g", item.G);
            info.AddValue("b", item.B);
        }

        public override Color SetData(Color item, System.Runtime.Serialization.SerializationInfo info)
        {
            int a = info.GetByte("a");
            int r = info.GetByte("r");
            int g = info.GetByte("g");
            int b = info.GetByte("b");
            return Color.FromArgb(a, r, g, b);
        }
    }
}
