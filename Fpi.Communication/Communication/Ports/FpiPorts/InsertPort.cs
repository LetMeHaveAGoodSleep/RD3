using System;
using Fpi.Communication.Crc;

using Fpi.Communication.Interfaces;
using Fpi.Util.Sundry;
using System.Resources;
using Fpi.Util;
namespace Fpi.Communication.Ports.FpiPorts
{
    public class InsertPort : BasePort
    {
        //帧头定义
        public static readonly byte[] frameHead = {0x7d, 0x7b};
        //帧尾定义
        public static readonly byte[] frameTail = {0x7d, 0x7d};
        //CRC长度
        private const int CRC_SIZE = 2;
        //帧头长度
        private const int HEAD_SIZE = 2;

        private const byte startByte = 0x7d;
        private const byte insertByte = 0x82; //!0x7d

        public InsertPort()
            : base()
        {
            frameBuffer = new byte[MAX_FRAME_SIZE*2];
        }


        private byte[] PackFrame(byte[] data)
        {
            //超过最大长度或者长度为0
            if ((data == null) || (data.Length <= 0) || (data.Length > (MAX_FRAME_SIZE - CRC_SIZE)))
                return null;
            //crc值

            ushort crc = Crc16.CalcCrc(data, data.Length);
            byte[] crcBytes = BitConverter.GetBytes(crc);

            int count = 0;
            byte[] packBuffer = new byte[MAX_FRAME_SIZE*2];

            for (int i = 0; i < data.Length; i++)
            {
                packBuffer[count++] = data[i];
                if (data[i] == startByte)
                {
                    packBuffer[count++] = insertByte;
                }
            }
            for (int i = 0; i < crcBytes.Length; i++)
            {
                packBuffer[count++] = crcBytes[i];
                if (crcBytes[i] == startByte)
                {
                    packBuffer[count++] = insertByte;
                }
            }


            //打包，帧头长度＋2byte表示帧长度＋数据长度 ＋crc长度 ＋帧尾长度

            byte[] packedData = new byte[frameHead.Length + count + frameTail.Length];
            //帧头
            Buffer.BlockCopy(frameHead, 0, packedData, 0, frameHead.Length);
            //帧数据

            Buffer.BlockCopy(packBuffer, 0, packedData, frameHead.Length, count);
            //帧尾
            Buffer.BlockCopy(frameTail, 0, packedData, packedData.Length - frameTail.Length, frameTail.Length);

            return packedData;
        }

        public override void Receive(Object source, IByteStream data)
        {
            byte[] receiveData = data.GetBytes();
            readedBytes = receiveData.Length;

            if (recevicedDataSize + receiveData.Length > frameBuffer.Length)
            {
                recevicedDataSize = 0;
                headIndex = -1;
            }

            Buffer.BlockCopy(receiveData, 0, frameBuffer, recevicedDataSize, receiveData.Length);

            //帧尾在readBuffer中的位置
            int tailIndex = -1;

            //查找帧头、帧尾

            int startIndex = Math.Max(0, recevicedDataSize - HEAD_SIZE + 1);
            int endIndex = recevicedDataSize + readedBytes - HEAD_SIZE;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (frameBuffer[i] == startByte)
                {
                    //找到帧头
                    if (frameBuffer[i + 1] == 0x7b)
                    {
                        headIndex = i;
                    }
                        //找到帧尾
                    else if (frameBuffer[i + 1] == 0x7d)
                    {
                        tailIndex = i;
                        //前面已经找到帧头
                        if (headIndex >= 0)
                        {
                            //数据帧开始位置

                            int frameStart = headIndex + HEAD_SIZE;
                            //数据帧结束位置

                            int frameEnd = tailIndex;
                            //数据帧长度

                            int frameLength = frameEnd - frameStart;
                            //数据长度有效
                            if ((frameLength <= MAX_FRAME_SIZE*2) && (frameLength >= MIN_FRAME_SIZE))
                            {
                                ////记录到日志
                                byte[] content = new byte[frameLength + 2 + CRC_SIZE];
                                Array.Copy(frameBuffer, headIndex, content, 0, content.Length);

                                //去除0x7d后面的0x82
                                int dataCount = 1;
                                for (int j = frameStart + 1; j < frameEnd; j++)
                                {
                                    if (!((frameBuffer[j] == insertByte) && (frameBuffer[j - 1] == startByte)))
                                    {
                                        frameBuffer[frameStart + dataCount++] = frameBuffer[j];
                                    }
                                }
                                dataCount -= 2;

                                //校验crc
                                if (Crc16.CalcCrc(frameBuffer, frameStart, dataCount) ==
                                    BitConverter.ToUInt16(frameBuffer, frameStart + dataCount))
                                {
                                    byte[] tempData = new byte[dataCount];
                                    Buffer.BlockCopy(frameBuffer, frameStart, tempData, 0, dataCount);

                                    SubmitData(tempData);
                                }
                                    //crc错误
                                else
                                {
                                    LogHelper.Debug("crc error");
                                }
                            } //数据长度有效
                        } //end of 前面已经找到帧头

                        //重新定位帧头
                        headIndex = -1;

                        //下一行不可删除，当找到完整一帧后，下一帧应该从 i++ 开始计算，因为帧尾长度为2,而当前的 i = 帧尾第一个字节
                        i++;
                    } //end of 帧尾
                } //if (readBuffer[i] == startByte)
            }

            recevicedDataSize += readedBytes;

            //已经找到帧尾
            if (tailIndex > 0)
            {
                //readBuffer数组向前移动 tailIndex + 2
                int moveSize = tailIndex + HEAD_SIZE;
                Buffer.BlockCopy(frameBuffer, moveSize, frameBuffer, 0, recevicedDataSize - moveSize);
                if (headIndex > 0)
                {
                    headIndex -= moveSize;
                }
                recevicedDataSize -= moveSize;
            }
        }

        public override Object Send(object dest, IByteStream data)
        {
            byte[] buffer = this.PackFrame(data.GetBytes());
            IPort lowerPort = LowerPort;
            IByteStream bs = new ByteArrayWrap(buffer);
            PortLogHelper.TracePortSendMsg(this.GetType().Name, buffer);
            return lowerPort.Send(dest, bs);
        }

        private void SubmitData(byte[] data)
        {
            IPortOwner portOwner = PortOwner;
            if (portOwner != null)
            {
                PortLogHelper.TracePortRecvMsg(this.GetType().Name, data);
                IByteStream bs = new ByteArrayWrap(data);
                portOwner.Receive(this, bs);
            }
        }
    }
}