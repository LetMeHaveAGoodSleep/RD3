using System;
using System.Text;
namespace Fpi.Communication.Converter
{
    /// <summary>
    /// IConvertable 的摘要说明。
    /// </summary>
    public class DataConverter : IDataConvertable
    {
        public DataConverter()
        {
        }

        private static object syncObj = new object();
        private static DataConverter instance = null;

        public static IDataConvertable GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new DataConverter();
                }
            }
            return instance;
        }

        #region IDataConvertable 成员

        public int GetTypeLength(string typeName)
        {
            //return System.Runtime.InteropServices.Marshal.SizeOf(Type.GetType(typeName));

            switch (typeName)
            {
                case "bit":
                    return -1;
                case "string":
                    return 1;
                case "byte":
                    return 1;
                case "uint":
                    return 2;
                case "int":
                    return 2;
                case "ulong":
                    return 4;
                case "long":
                    return 4;
                case "float":
                    return 4;
                default:
                    throw new NotSupportedException(typeName + " not suported in data converter。");
            }
        }

        public string ToString(byte[] value, int startIndex, int length)
        {
            for (int i = Math.Min(startIndex + length - 1, value.Length - 1); i >= startIndex; i--)
            {
                if (value[i] != (byte) 0)
                {
                    length = i + 1 - startIndex;
                    break;
                }
            }
            return UTF8Encoding.UTF8.GetString(value, startIndex, length);
        }

        private byte[] ReverseByte(byte[] value, int startIndex, int length)
        {
            byte[] b = new byte[length];
            for (int i = 0; i < b.Length; i++)
            {
                b[length - 1 - i] = value[startIndex + i];
            }
            return b;
        }


        public int ToInt32(byte[] value, int startIndex)
        {
            return (int) BitConverter.ToInt16(ReverseByte(value, startIndex, 4), 0);
        }

        public uint ToUInt32(byte[] value, int startIndex)
        {
            return (uint) BitConverter.ToUInt16(ReverseByte(value, startIndex, 4), 0);
        }

        public long ToInt64(byte[] value, int startIndex)
        {
            return (long) BitConverter.ToInt32(ReverseByte(value, startIndex, 4), 0);
        }

        public ulong ToUInt64(byte[] value, int startIndex)
        {
            return (ulong) BitConverter.ToUInt32(ReverseByte(value, startIndex, 4), 0);
        }

        public float ToSingle(byte[] value, int startIndex)
        {
            //return BitConverter.ToSingle(value, startIndex);
            return BitConverter.ToSingle(ReverseByte(value, startIndex, 4), 0);
        }

        public byte[] GetBytes(string value)
        {
            return UTF8Encoding.UTF8.GetBytes(value);
        }


        public byte[] GetBytes(short value)
        {
            return ReverseByte(BitConverter.GetBytes(value), 0, 2);
        }

        public byte[] GetBytes(ushort value)
        {
            return ReverseByte(BitConverter.GetBytes(value), 0, 2);
        }

        public byte[] GetBytes(int value)
        {
            return ReverseByte(BitConverter.GetBytes( value), 0, 4);
        }

        public byte[] GetBytes(uint value)
        {
            return ReverseByte(BitConverter.GetBytes(value), 0, 4);
        }

        public byte[] GetBytes(long value)
        {
            return ReverseByte(BitConverter.GetBytes( value), 0, 4);
        }

        public byte[] GetBytes(ulong value)
        {
            return ReverseByte(BitConverter.GetBytes( value), 0, 4);
        }

        public byte[] GetBytes(float value)
        {
            //return BitConverter.GetBytes(value);
            return ReverseByte(BitConverter.GetBytes((float) value), 0, 4);
        }

        #endregion
    }
}