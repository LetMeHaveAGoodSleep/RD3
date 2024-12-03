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
        //֡ͷ����
        public static readonly byte[] frameHead = {0x7d, 0x7b};
        //֡β����
        public static readonly byte[] frameTail = {0x7d, 0x7d};
        //CRC����
        private const int CRC_SIZE = 2;
        //֡ͷ����
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
            //������󳤶Ȼ��߳���Ϊ0
            if ((data == null) || (data.Length <= 0) || (data.Length > (MAX_FRAME_SIZE - CRC_SIZE)))
                return null;
            //crcֵ

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


            //�����֡ͷ���ȣ�2byte��ʾ֡���ȣ����ݳ��� ��crc���� ��֡β����

            byte[] packedData = new byte[frameHead.Length + count + frameTail.Length];
            //֡ͷ
            Buffer.BlockCopy(frameHead, 0, packedData, 0, frameHead.Length);
            //֡����

            Buffer.BlockCopy(packBuffer, 0, packedData, frameHead.Length, count);
            //֡β
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

            //֡β��readBuffer�е�λ��
            int tailIndex = -1;

            //����֡ͷ��֡β

            int startIndex = Math.Max(0, recevicedDataSize - HEAD_SIZE + 1);
            int endIndex = recevicedDataSize + readedBytes - HEAD_SIZE;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (frameBuffer[i] == startByte)
                {
                    //�ҵ�֡ͷ
                    if (frameBuffer[i + 1] == 0x7b)
                    {
                        headIndex = i;
                    }
                        //�ҵ�֡β
                    else if (frameBuffer[i + 1] == 0x7d)
                    {
                        tailIndex = i;
                        //ǰ���Ѿ��ҵ�֡ͷ
                        if (headIndex >= 0)
                        {
                            //����֡��ʼλ��

                            int frameStart = headIndex + HEAD_SIZE;
                            //����֡����λ��

                            int frameEnd = tailIndex;
                            //����֡����

                            int frameLength = frameEnd - frameStart;
                            //���ݳ�����Ч
                            if ((frameLength <= MAX_FRAME_SIZE*2) && (frameLength >= MIN_FRAME_SIZE))
                            {
                                ////��¼����־
                                byte[] content = new byte[frameLength + 2 + CRC_SIZE];
                                Array.Copy(frameBuffer, headIndex, content, 0, content.Length);

                                //ȥ��0x7d�����0x82
                                int dataCount = 1;
                                for (int j = frameStart + 1; j < frameEnd; j++)
                                {
                                    if (!((frameBuffer[j] == insertByte) && (frameBuffer[j - 1] == startByte)))
                                    {
                                        frameBuffer[frameStart + dataCount++] = frameBuffer[j];
                                    }
                                }
                                dataCount -= 2;

                                //У��crc
                                if (Crc16.CalcCrc(frameBuffer, frameStart, dataCount) ==
                                    BitConverter.ToUInt16(frameBuffer, frameStart + dataCount))
                                {
                                    byte[] tempData = new byte[dataCount];
                                    Buffer.BlockCopy(frameBuffer, frameStart, tempData, 0, dataCount);

                                    SubmitData(tempData);
                                }
                                    //crc����
                                else
                                {
                                    LogHelper.Debug("crc error");
                                }
                            } //���ݳ�����Ч
                        } //end of ǰ���Ѿ��ҵ�֡ͷ

                        //���¶�λ֡ͷ
                        headIndex = -1;

                        //��һ�в���ɾ�������ҵ�����һ֡����һ֡Ӧ�ô� i++ ��ʼ���㣬��Ϊ֡β����Ϊ2,����ǰ�� i = ֡β��һ���ֽ�
                        i++;
                    } //end of ֡β
                } //if (readBuffer[i] == startByte)
            }

            recevicedDataSize += readedBytes;

            //�Ѿ��ҵ�֡β
            if (tailIndex > 0)
            {
                //readBuffer������ǰ�ƶ� tailIndex + 2
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