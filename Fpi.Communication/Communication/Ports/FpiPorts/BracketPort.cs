using System;
using Fpi.Communication.Crc;
using Fpi.Communication.Interfaces;
using Fpi.Util;
using Fpi.Util.Sundry;

namespace Fpi.Communication.Ports.FpiPorts
{
    /// <summary>
    /// 7B7D��ʼ�ͽ�����֡��
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


        //��������֡
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

            //֡β0x7d��frameBuffer�е�λ��
            int tailIndex = -1;
            //����7b��7d
            for (int i = 0; i < readedBytes; i++)
            {
                //�ҵ�֡ͷ����¼λ��
                if (receiveData[i] == startByte)
                {
                    headIndex = recevicedDataSize + i;
                }
                    //�ҵ�֡β����������
                else if (receiveData[i] == endByte)
                {
                    tailIndex = i + recevicedDataSize;

                    //ǰ���Ѿ��ҵ�֡ͷ
                    if (headIndex >= 0)
                    {
                        //�õ�֡����
                        int dataLength = tailIndex - headIndex - 1;
                        //���ݳ�����Ч
                        if ((dataLength <= MAX_FRAME_SIZE*2) && (dataLength >= MIN_FRAME_SIZE))
                        {
                            ////��¼����־
                            byte[] content = new byte[dataLength + 2];
                            Buffer.BlockCopy(readBuffer, headIndex, content, 0, dataLength + 2);

                            byte[] dataWithCrc = new byte[dataLength];
                            Buffer.BlockCopy(readBuffer, headIndex + 1, dataWithCrc, 0, dataWithCrc.Length);
                            //У��crc
                            //if (Crc16.CalcCrc2(readBuffer, headIndex+1, dataCount) == BitConverter.ToUInt16(readBuffer, headIndex+1 + dataCount))
                            if (Crc16.IsValidCrcInBracketPort(dataWithCrc))
                            {
                                byte[] tempData = new byte[dataWithCrc.Length - 4];
                                Buffer.BlockCopy(dataWithCrc, 0, tempData, 0, tempData.Length);
                                IPortOwner portOwner = PortOwner;
                                PortLogHelper.TracePortRecvMsg(this.GetType().Name, tempData);
                                portOwner.Receive(this, new ByteArrayWrap(tempData));
                            }
                                //crc����
                            else
                            {
                                LogHelper.Debug("crc error");
                            }
                        } //���ݳ�����Ч
                    }
                    //���¶�λ֡ͷ
                    headIndex = -1;
                } //end of ֡β
            } //end of for

            recevicedDataSize += readedBytes;

            //�Ѿ��ҵ�֡β
            if (tailIndex > 0)
            {
                //frameBuffer������ǰ�ƶ� tailIndex + 1
                int moveSize = tailIndex + 1;
                Buffer.BlockCopy(readBuffer, moveSize, readBuffer, 0, recevicedDataSize - moveSize);
                if (headIndex > 0)
                {
                    headIndex -= moveSize;
                }
                recevicedDataSize -= moveSize;
            }
        }

        //����Э��������
        private byte[] PackFrame(byte[] data)
        {
            if ((data == null) || (data.Length > MAX_FRAME_SIZE) || (data.Length < MIN_FRAME_SIZE))
                return null;
            byte[] packedData = new byte[data.Length + 2 + 4]; //2��ʾ7b7d���ȣ�4��ʾcrc����
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