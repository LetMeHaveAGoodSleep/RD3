using System;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.NumberPorts
{
    public class NumberData : IByteStream
    {
        public byte number;
        public IByteStream data;

        public NumberData(byte[] data)
        {
            number = data[0];
            byte[] result = new byte[data.Length - 1];
            Buffer.BlockCopy(data, 1, result, 0, result.Length);
            this.data = new ByteArrayWrap(result);
        }

        public NumberData(byte number, IByteStream data)
        {
            this.number = number;
            this.data = data;
        }

        //是否命令请求编号
        public static bool IsRequest(byte num)
        {
            return (num & 0x80) == 0;
        }

        //获取回应编号
        public static byte GetResponseNum(byte num)
        {
            return (byte) (num | 0x80);
        }

        #region IByteStream 成员

        public byte[] GetBytes()
        {
            byte[] bytes = data.GetBytes();
            byte[] result = new byte[1 + bytes.Length];
            result[0] = number;
            Buffer.BlockCopy(bytes, 0, result, 1, bytes.Length);
            return result;
        }

        #endregion
    }
}