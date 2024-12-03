using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Fpi.Util.Serializes.CustomSerializer
{
    public class ArraySerializer : SerializerBase<object[]>
    {
        public override void GetData(object[] item, System.Runtime.Serialization.SerializationInfo info)
        {
            int i = 0;
            foreach (object o in item)
            {
                info.AddValue(i.ToString(), o);
                i++;
            }
        }

        public override object[] SetData(object[] item, System.Runtime.Serialization.SerializationInfo info)
        {
            List<object> result = new List<object>();

            SerializationInfoEnumerator itor = info.GetEnumerator();
            while (itor.MoveNext())
            {
                result.Add(itor.Value);
            }

            if (item == null)
            {
                return result.ToArray();
            }
            else
            {
                Array.Resize<object>(ref item, result.Count);
                Array.Copy(result.ToArray(), item, result.Count);
                return item;
            }
        }
    }

    public class ArraySerializer<T> : SerializerBase<T[]>
    {
        public override void GetData(T[] item, System.Runtime.Serialization.SerializationInfo info)
        {
            int i = 0;
            foreach (T o in item)
            {
                info.AddValue(i.ToString(), o);
                i++;
            }
        }

        public override T[] SetData(T[] item, System.Runtime.Serialization.SerializationInfo info)
        {
            List<T> result = new List<T>();

            SerializationInfoEnumerator itor = info.GetEnumerator();
            while (itor.MoveNext())
            {
                result.Add((T)itor.Value);
            }

            if (item == null)
            {
                return result.ToArray();
            }
            else
            {
                Array.Resize<T>(ref item, result.Count);
                Array.Copy(result.ToArray(), item, result.Count);
                return item;
            }
        }
    }

    public class Int32ArraySerializer : ArraySerializer<int>
    {
        public override void GetData(int[] items, System.Runtime.Serialization.SerializationInfo info)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (int item in items)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                sb.Append(item);
                i++;
            }

            info.AddValue("values", sb.ToString());
        }

        public override int[] SetData(int[] item, System.Runtime.Serialization.SerializationInfo info)
        {
            List<int> result = new List<int>();

            string values = info.GetString("values");
            string[] strItems = values.Split(',');
            foreach (string s in strItems)
            {
                int val;
                if (int.TryParse(s, out val))
                {
                    result.Add(val);
                }
            }

            if (item == null)
            {
                return result.ToArray();
            }
            else
            {
                Array.Resize<int>(ref item, result.Count);
                Array.Copy(result.ToArray(), item, result.Count);
                return item;
            }
        }
    }

    public class ByteArraySerializer : ArraySerializer<byte>
    {
        public override void GetData(byte[] items, System.Runtime.Serialization.SerializationInfo info)
        {
            if (items != null && items.Length != 0)
            {
                info.AddValue("values", Convert.ToBase64String(items));
            }
            else
            {
                info.AddValue("values", string.Empty);
            }
        }

        public override byte[] SetData(byte[] item, System.Runtime.Serialization.SerializationInfo info)
        {
            string base64 = info.GetString("values");
            if (!string.IsNullOrEmpty(base64))
            {
                byte[] result = Convert.FromBase64String(base64);
                if (item == null)
                {
                    return result;
                }
                else
                {
                    Array.Resize<byte>(ref item, result.Length);
                    Array.Copy(result, item, result.Length);
                    return item;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
