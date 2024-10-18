using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RD3.Shared
{
    public static class FrameHelper
    {
        private const byte StartFlag = 0x7E; // 帧头
        private const int CommandLength = 2; // 命令长度
        private const int ActionLength = 1; // 动作长度
        private const int DataLengthSize = 2; // 数据长度字段大小
        private const int CRCSize = 1; // CRC8大小

        public static byte[] FrameData(byte[] command, byte[] data)
        {
            List<byte> framedData = new List<byte>();

            // 帧头
            framedData.Add(StartFlag);
            framedData.Add(StartFlag);

            // 命令
            framedData.AddRange(command);

            // 数据长度
            byte[] dataLengthBytes = BitConverter.GetBytes((ushort)data.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(dataLengthBytes); // 确保大端字节序
            framedData.AddRange(dataLengthBytes);

            // 数据段
            framedData.AddRange(data);

            // CRC8校验
            byte crc = Crc8CheckSum(framedData.ToArray());
            framedData.Add(crc);

            return framedData.ToArray();
        }

        public static Tuple<byte[], byte[]> UnframeData(byte[] framedData)
        {
            LogHelper.Info(BitConverter.ToString(framedData).Replace("-", " "));

            if (framedData.Length < CommandLength + DataLengthSize + CRCSize + 2)
                throw new ArgumentException("Data is too short to be a valid frame.");

            // 检查帧头
            if (framedData[0] != StartFlag || framedData[1] != StartFlag)
                throw new ArgumentException("Invalid frame header.");

            // 获取命令
            byte[] command = framedData.Skip(2).Take(CommandLength).ToArray();

            // 获取数据长度
            byte[] dataLengthBytes = framedData.Skip(2 + CommandLength).Take(DataLengthSize).ToArray();
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(dataLengthBytes); // 确保大端字节序
            ushort dataLength = (ushort)(BitConverter.ToUInt16(dataLengthBytes, 0) - 1);

            // 获取数据
            byte[] data = framedData.Skip(2 + CommandLength + DataLengthSize + ActionLength).Take((int)dataLength).ToArray();

            // 计算并验证CRC8
            byte calculatedCRC = Crc8CheckSum(framedData.Take(framedData.Length - CRCSize).ToArray());
            if (calculatedCRC != framedData.Last())
                throw new ArgumentException("CRC8 check failed.");

            return Tuple.Create(command, data);
        }

        #region 计算Crc8校验值 Crc8CheckSum()
        /// <summary>
        /// 计算Crc8校验值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte Crc8CheckSum(byte[] data)
        {
            byte i;
            byte Crc8 = 0;
            for (int j = 0; j < data.Length; j++)
            {
                for (i = 0x80; i > 0; i >>= 1)
                {
                    if ((Crc8 & 0x80) != 0)
                    {
                        Crc8 <<= 1;
                        Crc8 ^= 0x31;
                    }
                    else
                    {
                        Crc8 <<= 1;
                    }
                    if ((data[j] & i) > 0)
                    {
                        Crc8 ^= 0x31;
                    }
                }
            }
            return Crc8;
        }
        #endregion
    }
}
