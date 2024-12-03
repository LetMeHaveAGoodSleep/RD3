using System;
using Fpi.Communication.Crc;
using Fpi.Communication.Interfaces;
using Fpi.Util;
using Fpi.Util.Sundry;

namespace Fpi.Communication.Ports.FpiPorts
{
    /// <summary>
    /// 7B7D开始和结束的帧。
    /// </summary>
    public class BracketPort : BasePort
    {
        private const byte startByte = 0x7b;
        private const byte endByte = 0x7d;


        private byte[] readBuffer;


        public BracketPort()
            : base()
        {
            readBuffer = new byte[MAX_FRAME_SIZE*2];
        }


        //接收数据帧
        public override void Receive(Object source, IByteStream data)
        {
            byte[] receiveData = data.GetBytes();

            readedBytes = receiveData.Length;

            if (recevicedDataSize + receiveData.Length > readBuffer.Length)
            {
                recevicedDataSize = 0;
                headIndex = -1;
            }

            Buffer.BlockCopy(receiveData, 0, readBuffer, recevicedDataSize, receiveData.Length);

            //帧尾0x7d在frameBuffer中的位置
            int tailIndex = -1;
            //查找7b，7d
            for (int i = 0; i < readedBytes; i++)
            {
                //找到帧头，记录位置
                if (receiveData[i] == startByte)
                {
                    headIndex = recevicedDataSize + i;
                }
                    //找到帧尾，处理数据
                else if (receiveData[i] == endByte)
                {
                    tailIndex = i + recevicedDataSize;

                    //前面已经找到帧头
                    if (headIndex >= 0)
                    {
                        //得到帧长度
                        int dataLength = tailIndex - headIndex - 1;
                        //数据长度有效
                        if ((dataLength <= MAX_FRAME_SIZE*2) && (dataLength >= MIN_FRAME_SIZE))
                        {
                            ////记录到日志
                            byte[] content = new byte[dataLength + 2];
                            Buffer.BlockCopy(readBuffer, headIndex, content, 0, dataLength + 2);

                            byte[] dataWithCrc = new byte[dataLength];
                            Buffer.BlockCopy(readBuffer, headIndex + 1, dataWithCrc, 0, dataWithCrc.Length);
                            //校验crc
                            //if (Crc16.CalcCrc2(readBuffer, headIndex+1, dataCount) == BitConverter.ToUInt16(readBuffer, headIndex+1 + dataCount))
                            if (Crc16.IsValidCrcInBracketPort(dataWithCrc))
                            {
                                byte[] tempData = new byte[dataWithCrc.Length - 4];
                                Buffer.BlockCopy(dataWithCrc, 0, tempData, 0, tempData.Length);
                                IPortOwner portOwner = PortOwner;
                                PortLogHelper.TracePortRecvMsg(this.GetType().Name, tempData);
                                portOwner.Receive(this, new ByteArrayWrap(tempData));
                            }
                                //crc错误
                            else
                            {
                                LogHelper.Debug("crc error");
                            }
                        } //数据长度有效
                    }
                    //重新定位帧头
                    headIndex = -1;
                } //end of 帧尾
            } //end of for

            recevicedDataSize += readedBytes;

            //已经找到帧尾
            if (tailIndex > 0)
            {
                //frameBuffer数组向前移动 tailIndex + 1
                int moveSize = tailIndex + 1;
                Buffer.BlockCopy(readBuffer, moveSize, readBuffer, 0, recevicedDataSize - moveSize);
                if (headIndex > 0)
                {
                    headIndex -= moveSize;
                }
                recevicedDataSize -= moveSize;
            }
        }

        //根据协议打包数据
        private byte[] PackFrame(byte[] data)
        {
            if ((data == null) || (data.Length > MAX_FRAME_SIZE) || (data.Length < MIN_FRAME_SIZE))
                return null;
            byte[] packedData = new byte[data.Length + 2 + 4]; //2表示7b7d长度，4表示crc长度
            packedData[0] = (byte) 0x7b;
            Buffer.BlockCopy(data, 0, packedData, 1, data.Length);
            byte[] crc = Crc16.GetCrcInBracketPort(data);
            Buffer.BlockCopy(crc, 0, packedData, 1 + data.Length, crc.Length);
            packedData[packedData.Length - 1] = (byte) 0x7d;
            return packedData;
        }

        public override Object Send(object dest, IByteStream data)
        {
            byte[] buffer = this.PackFrame(data.GetBytes());
            IPort lowerPort = LowerPort;
            IByteStream bs = new ByteArrayWrap(buffer);
            PortLogHelper.TracePortSendMsg(this.GetType().Name, buffer);
            return lowerPort.Send(dest, bs);


            //IPort port = LowerPort;
            //return port.Send(dest, new ByteArrayWrap(PackFrame(data.GetBytes())));
        }
    }
}