namespace Fpi.Util.Serializes.CustomSerializer
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Runtime.Serialization;

    /// <summary>
    /// 图像对象序列化器
    /// </summary>
    public class ImageSerializer : SerializerBase<Image>
    {
        public override void GetData(Image item, System.Runtime.Serialization.SerializationInfo info)
        {
            ISerializable serializer = item.Clone() as ISerializable;
            if (serializer != null)
            {
                serializer.GetObjectData(info, new StreamingContext());
                if (item.Tag != null)
                {
                    info.AddValue("tag", item.Tag);
                }
            }
            else
            {
                info.AddValue("Data", new byte[0]);
            }
        }

        public override Image SetData(Image item, SerializationInfo info)
        {
            try
            {
                SerializationInfoEnumerator itor = info.GetEnumerator();
                Image img = null;
                while (itor.MoveNext())
                {
                    if (string.Equals(itor.Name, "data", StringComparison.OrdinalIgnoreCase))
                    {
                        MemoryStream ms = new MemoryStream();
                        byte[] data = itor.Value as byte[];
                        ms.Write(data, 0, data.Length);
                        ms.Position = 0;
                        img = Image.FromStream(ms);
                    }

                }
                itor = info.GetEnumerator();
                while (itor.MoveNext())
                {
                    if (string.Equals(itor.Name, "tag", StringComparison.OrdinalIgnoreCase))
                    {
                        img.Tag = itor.Value;
                    }
                }
                return img;
            }
            finally
            {
            }
        }
    }
}
