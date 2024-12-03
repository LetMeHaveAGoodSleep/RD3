using System;
using Fpi.Communication.Crc;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.FpiPorts
{
    public class HJ212Port : BasePort
    {
        //CRC长度(byte)
        private const int CRC_SIZE = 4;
        //帧长度的字节数
        private const int COUNT_SIZE = 4;
        private const int HEAD_SIZE = 2;

        //帧头定义，帧头不允许为空数组，这里可以根据需要定义不同的帧头
        public static readonly byte[] frameHead = {0x23, 0x23}; //##作为包尾
        //帧尾定义，帧头可以为空数组，这里可以根据需要定义不同的帧尾
        public static readonly byte[] frameTail = {0x0d, 0x0a};

        public HJ212Port()
        {
        }

        private int FindHeadIndex()
        {
            if (headIndex >= 0)
            {
                return headIndex;
            }

            int endIndex = recevicedDataSize - HEAD_SIZE;

            for (int i = 0; i <= endIndex; i++)
            {
                if ((frameBuffer[i] == 0x23) && (frameBuffer[i + 1] == 0x23))
                {
                    headIndex = i;
                    return headIndex;
                }
            }
            if (frameBuffer[endIndex + 1] == 0x23)
            {
                frameBuffer[0] = 0x23;
                headIndex = -1;
                recevicedDataSize = 1;
            }
            else
            {
                ResetBuffer();
            }
            return -1;
        }

        protected uint GetUint(byte[] data, int index)
        {
            uint length = 0;
            length += (uint) (1000*(data[index] - 0x30));
            length += (uint) (100*(data[index + 1] - 0x30));
            length += (uint) (10*(data[index + 2] - 0x30));
            length += (uint) ((data[index + 3] - 0x30));
            return length;
        }

        private bool ReceiveOneFrame()
        {
            if (FindHeadIndex() < 0)
            {
                return false;
            }

            int validDataCount = recevicedDataSize - headIndex;

            if (validDataCount <= 10)
            {
                return false;
            }

            int dataCount = (int) GetUint(frameBuffer, headIndex + 2);

            if (validDataCount - 10 < dataCount)
            {
                return false;
            }

            uint countCrc, countCrc2, rcvCrc;

            byte[] data = new byte[dataCount];

            Buffer.BlockCopy(frameBuffer, headIndex + 6, data, 0, dataCount);

            countCrc = Crc16.CalcCrc(data, (int) dataCount);
            countCrc2 = ((countCrc >> 8) + (countCrc << 8)) & 0xffff;

            string crcString = "";
            try
            {
                crcString = encoding.GetString(frameBuffer, headIndex + 6 + dataCount, 4);
                crcString = crcString.Trim();
                rcvCrc = Convert.ToUInt16(crcString, 16);
            }
            catch
            {
                ResetBuffer();
                return false;
            }


            if (countCrc == rcvCrc || countCrc2 == rcvCrc || 0xffff == rcvCrc)
            {
                IPortOwner portOwner = PortOwner;
                IByteStream bs = new StringWrap(data);
                PortLogHelper.TracePortRecvMsg(this.GetType().Name, data);
                portOwner.Receive(this, bs);

                if (validDataCount - 10 == dataCount)
                {
                    ResetBuffer();
                    return false;
                }

                //frameBuffer数组向前移动
                int moveSize = headIndex + dataCount + 10;
                Buffer.BlockCopy(frameBuffer, moveSize, frameBuffer, 0, recevicedDataSize - moveSize);

                headIndex = -1;
                recevicedDataSize -= moveSize;
                return true;
            }
            else
            {
                ResetBuffer();
                return false;
            }
        }

        private void ResetBuffer()
        {
            headIndex = -1;
            recevicedDataSize = 0;
        }

        private byte[] PackFrame(byte[] data)
        {
            //超过最大长度或者长度为0
            if ((data == null)
                || (data.Length <= 0)
                || (data.Length > (MAX_FRAME_SIZE - CRC_SIZE)))
            {
                return null;
            }

            //打包，帧头长度＋4byte表示帧长度＋数据长度 ＋crc长度 ＋帧尾长度
            byte[] packedData = new byte[frameHead.Length + COUNT_SIZE + data.Length + CRC_SIZE + frameTail.Length];

            //帧头
            Buffer.BlockCopy(frameHead, 0, packedData, 0, frameHead.Length);

            //4byte表示帧长度
            byte[] bytes = encoding.GetBytes(Convert.ToString(data.Length, 10));
            byte[] length = new byte[4];
            for (int i = 0; i < length.Length; i++)
            {
                Buffer.SetByte(length, i, 48);
            }
            Buffer.BlockCopy(bytes, 0, length, 4 - bytes.Length, bytes.Length);

            string tmp = encoding.GetString(data, 0, data.Length);
            Buffer.BlockCopy(length, 0, packedData, frameHead.Length, COUNT_SIZE);

            //帧数据
            Buffer.BlockCopy(data, 0, packedData, frameHead.Length + COUNT_SIZE, data.Length);

            //crc值			
            string strCrc = Convert.ToString(Crc16.CalcCrc2(data, data.Length), 16);
            string strTmp = "";
            if (strCrc.Length < 4)
            {
                for (int i = 0; i < (4 - strCrc.Length); i++)
                {
                    strTmp += "0";
                }
            }
            strCrc = strTmp + strCrc;
            bytes = encoding.GetBytes(strCrc);
            Buffer.BlockCopy(bytes, 0, packedData, packedData.Length - CRC_SIZE - frameTail.Length, CRC_SIZE);

            //帧尾
            if (frameTail.Length > 0)
            {
                Buffer.BlockCopy(frameTail, 0, packedData, packedData.Length - frameTail.Length, frameTail.Length);
            }

            return packedData;
        }

        public override void Receive(Object source, IByteStream data)
        {
            byte[] recvData = data.GetBytes();
            readedBytes = recvData.Length;


            if (recevicedDataSize + readedBytes > frameBuffer.Length)
            {
                ResetBuffer();
            }

            Buffer.BlockCopy(recvData, 0, frameBuffer, recevicedDataSize, readedBytes);

            recevicedDataSize += readedBytes;

            while (ReceiveOneFrame())
            {
            }
        }

        public override Object Send(object dest, IByteStream data)
        {
            IPort lowerPort = LowerPort;
            PortLogHelper.TracePortSendMsg(this.GetType().Name, data.GetBytes());
            return lowerPort.Send(dest, new ByteArrayWrap(PackFrame(data.GetBytes())));
        }
    }
}