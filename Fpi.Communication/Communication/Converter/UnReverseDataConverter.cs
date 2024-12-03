using System;
namespace Fpi.Communication.Converter
{
    /// <summary>
    /// Class1 的摘要说明。
    /// </summary>
    public class UnReverseDataConverter : IDataConvertable
    {
        private IDataConvertable convert;

        private UnReverseDataConverter()
        {
            convert = DataConverter.GetInstance();
        }

        private static object syncObj = new object();
        private static UnReverseDataConverter instance = null;

        public static IDataConvertable GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new UnReverseDataConverter();
                }
            }
            return instance;
        }

        public int GetTypeLength(string typeName)
        {
            return this.convert.GetTypeLength(typeName);
        }

        public int ToInt32(byte[] value, int startIndex)
        {
            return this.convert.ToInt32(value, startIndex);
        }

        public uint ToUInt32(byte[] value, int startIndex)
        {
            return this.convert.ToUInt32(value, startIndex);
        }

        public long ToInt64(byte[] value, int startIndex)
        {
            return this.convert.ToInt64(value, startIndex);
        }

        public ulong ToUInt64(byte[] value, int startIndex)
        {
            return this.convert.ToUInt64(value, startIndex);
        }

        public float ToSingle(byte[] value, int startIndex)
        {
            return BitConverter.ToSingle(value, startIndex);
        }

        public string ToString(byte[] value, int startIndex, int length)
        {
            return this.convert.ToString(value, startIndex, length);
        }

        public byte[] GetBytes(int value)
        {
            return this.convert.GetBytes(value);
        }

        public byte[] GetBytes(uint value)
        {
            return this.convert.GetBytes(value);
        }

        public byte[] GetBytes(long value)
        {
            return this.convert.GetBytes(value);
        }

        public byte[] GetBytes(ulong value)
        {
            return this.convert.GetBytes(value);
        }

        public byte[] GetBytes(float value)
        {
            return BitConverter.GetBytes(value);
        }

        public byte[] GetBytes(string value)
        {
            return this.convert.GetBytes(value);
        }
    }
}