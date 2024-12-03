using System;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.FpiPorts
{
    public class UsbSpecPort : BasePort
    {
        private const int ONCE_READ_COUNT = 1088;
        private const int BUF_MAX_SIZE = 10240;
        private const int HEAD_LENGTH = 64;
        private const int USERDATA_SIZE = 1024;
        private const int FRAME_SIZE = HEAD_LENGTH + USERDATA_SIZE;

        public UsbSpecPort() : base()
        {
            frameBuffer = new byte[BUF_MAX_SIZE];
        }


        //
        private int FindFrameDataStart(byte[] buf)
        {
            for (int i = 0; i < buf.Length; i += HEAD_LENGTH)
            {
                if (buf[i] == 0xaa || buf[i] == 0xbb)
                {
                    int headdata = 1;
                    int leftPos = i;

                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (buf[j] == buf[i])
                        {
                            if (++headdata == HEAD_LENGTH)
                            {
                                return j;
                            }
                        }
                        else
                        {
                            leftPos = j + 1;
                            break;
                        }
                    }
                    for (int j = i + 1; ((j < i + HEAD_LENGTH) && (j < buf.Length)); j++)
                    {
                        if (buf[j] == buf[i])
                        {
                            if (++headdata == HEAD_LENGTH)
                            {
                                return leftPos;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            return -1;
        }



        public override void Receive(Object source, IByteStream data)
        {
            byte[] recvData = data.GetBytes();

            readedBytes = recvData.Length;

            Buffer.BlockCopy(recvData, 0, frameBuffer, recevicedDataSize, readedBytes);
            recevicedDataSize += readedBytes;

            while (true)
            {
                int startIndex = FindFrameDataStart(frameBuffer);
                //not find frame start
                if (startIndex < 0)
                {
                    if (recevicedDataSize >= HEAD_LENGTH)
                    {
                        Buffer.BlockCopy(frameBuffer, recevicedDataSize - (HEAD_LENGTH - 1), frameBuffer, 0,
                                         (HEAD_LENGTH - 1));
                        recevicedDataSize = (HEAD_LENGTH - 1);
                    }
                    break;
                }
                else
                {
                    if (recevicedDataSize >= (startIndex + FRAME_SIZE))
                    {
                        byte[] datatemp = new byte[USERDATA_SIZE + 1];
                        Buffer.BlockCopy(frameBuffer, startIndex + (HEAD_LENGTH - 1), datatemp, 0, datatemp.Length);
                        IPortOwner portOwner = PortOwner;
                        PortLogHelper.TracePortRecvMsg(this.GetType().Name, datatemp);
                        portOwner.Receive(this, new ByteArrayWrap(datatemp));
                        Buffer.BlockCopy(frameBuffer, startIndex + FRAME_SIZE, frameBuffer, 0,
                                         recevicedDataSize - (startIndex + FRAME_SIZE));
                        recevicedDataSize -= (startIndex + FRAME_SIZE);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }


        //根据协议打包数据
        private byte[] PackFrame(byte[] data)
        {
            if ((data == null) || (data.Length > MAX_FRAME_SIZE) || (data.Length < MIN_FRAME_SIZE))
                return null;
            byte[] packedData = new byte[data.Length + 2];
            packedData[0] = (byte) 0x7b;
            Buffer.BlockCopy(data, 0, packedData, 1, data.Length);
            packedData[packedData.Length - 1] = (byte) 0x7d;
            return packedData;
        }

        public override Object Send(object dest, IByteStream data)
        {
            IPort lowerPort = LowerPort;
            PortLogHelper.TracePortSendMsg(this.GetType().Name, data.GetBytes());
            return lowerPort.Send(dest, new ByteArrayWrap(PackFrame(data.GetBytes())));
        }
    }
}