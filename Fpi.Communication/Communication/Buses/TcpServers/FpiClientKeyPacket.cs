using System;
using System.Collections.Generic;
using System.Text;

namespace Fpi.Communication.Buses
{
    /// <summary>
    /// 该类功能：从FPI协议中解析得到与TcpClient绑定的key
    /// 对FPI协议来说Key即为目标地址
    /// 创建人：张永强
    /// 创建时间：2011-7-1
    /// </summary>
    public class FpiClientKeyPacket:ClientKeyPacket
    {
        //帧头定义
        private static readonly byte[] frameHead = { 0x7d, 0x7b };
        //帧尾定义
        private static readonly byte[] frameTail = { 0x7d, 0x7d };
        //CRC长度
        private const int CRC_SIZE = 2;
        //帧头长度
        private const int HEAD_SIZE = 2;

        protected override bool ParseData()
        {
            if(bufferPos < 14)
            {
                return false;
            }
            int headIndex=-1, tailIndex=-1;
            for (int i = 0; i < bufferPos-1; i++)
            {
                //找到帧头
                if (dataBuffer[i] == frameHead[0] && dataBuffer[i + 1] == frameHead[1])
                {
                    headIndex = i;
                }
                if (headIndex>=0 && dataBuffer[i] == frameTail[0] && dataBuffer[i + 1] == frameTail[1])
                {
                    tailIndex = i;
                    break;
                }
            }
            if (headIndex >= 0 && tailIndex >= 0)
            {
                //找到源地址，作为与客户端绑定的Key
                clientKey = dataBuffer[headIndex + 5];
                return true;
            }
            else
            {
                return false;
            }

        }
        /// <summary>
        /// 得到FPI协议发送命令的Key,即为目标仪器地址
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override object GetKeyfromData(byte[] data)
        {
            if(data.Length < 14 || data[0] != 0x7D || data[1] != 0x7B ||
                data[data.Length-1]!= 0x7D || data[data.Length -2] != 0x7D)
            {
                return null;
            }
            //目标仪器地址
            byte address = data[3];
            return address;
        }
    }
}
