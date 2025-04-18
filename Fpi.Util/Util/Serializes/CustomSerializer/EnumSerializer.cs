using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Fpi.Util.Serializes.CustomSerializer
{
    public class EnumSerializer : SerializerBase<Enum>
    {
        public override void GetData(Enum item, System.Runtime.Serialization.SerializationInfo info)
        {
            info.AddValue("enumvalue", item.ToString());
        }

        public override Enum SetData(Enum item, System.Runtime.Serialization.SerializationInfo info)
        {
            string enumvalue = info.GetString("enumvalue");

            foreach (Enum e in Enum.GetValues(item.GetType()))
            {
                if (e.ToString() == enumvalue)
                {
                    return e;
                }
            }
            return null;
        }
    }
}
