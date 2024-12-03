using System;
using System.Collections.Generic;
using System.Text;
using Fpi.Communication.Ports;
using System.Threading;
using Fpi.Communication;
using Fpi.Util;
using Fpi.Communication.Crc;
using Fpi.Communication.Interfaces;
using Fpi.Util.WinApiUtil;

namespace Fpi.Communication.Ports.ModBus
{
    public class ModBusPort : BasePort
    {
        static public readonly string PropertyName_WaitTime = "waitTime";
        private uint waitTime = 200;//modbus ֡��� 200ms�����ڸ�Э��������֡ͷ֡β
        private byte[] readBuffer;
        //���ݷ����߳�
        private Thread dataReceiveThread = null;
        //���ݽ��մ����¼�
        private IntPtr receiveEvent = WinApiWrapper.CreateEvent(false, false, "dcon_receive_event");

        public ModBusPort()
            : base()
        {
            readBuffer = new byte[MAX_FRAME_SIZE * 2];
            waitTime = uint.Parse(this.GetProperty(PropertyName_WaitTime, "200"));

        }

        public override void Receive(Object source, IByteStream data)
        {
            byte[] receiveData = data.GetBytes();

            readedBytes = receiveData.Length;

            if (recevicedDataSize + receiveData.Length > readBuffer.Length)
            {
                recevicedDataSize = 0;
                headIndex = -1;
            }
            GetDataReceiveThread();
            lock (this)
            {
                Buffer.BlockCopy(receiveData, 0, readBuffer, recevicedDataSize, receiveData.Length);
                recevicedDataSize += readedBytes;
            }
            WinApiWrapper.SetEvent(receiveEvent);


        }

        protected override IByteStream PackData(object dest, IByteStream data)
        {
            byte[] newData = new byte[data.GetBytes().Length + 2];

            //����CrcУ��λ
            ushort crc = Crc16.CalcCrc(data.GetBytes(), data.GetBytes().Length);
            byte[] crcBytes = BitConverter.GetBytes(crc);
            int offset = 0;

            Buffer.BlockCopy(data.GetBytes(), 0, newData, offset, data.GetBytes().Length);
            offset += data.GetBytes().Length;
            Buffer.BlockCopy(crcBytes, 0, newData, offset, 2);

            return ByteArrayWrap.Build(newData);
        }


        private void DataReceiveThreadFunc()
        {
            while (this.connected)
            {
                uint waitValue = (uint)WinApiWrapper.WaitForSingleObject(receiveEvent, (uint)waitTime);
                //Event����(��������)
                if (waitValue == (uint)APIConstants.WAIT_OBJECT_0)
                {
                    continue;
                }
                //timeout����
                else if (waitValue == (uint)APIConstants.WAIT_TIMEOUT)
                {
                    IPortOwner portOwner = PortOwner;

                    if ((portOwner != null) && (recevicedDataSize > 0))
                    {
                        byte[] data;
                        lock (this)
                        {
                            data = new byte[recevicedDataSize];
                            Buffer.BlockCopy(readBuffer, 0, data, 0, recevicedDataSize);
                            recevicedDataSize = 0;
                        }
                        if (data.Length < 2)
                            continue;
                        if (Crc16.CalcCrc(data, data.Length - 2) == BitConverter.ToUInt16(data, data.Length - 2))
                        {
                            byte[] tempData = new byte[data.Length -2 ];
                            Buffer.BlockCopy(data, 0, tempData, 0, data.Length - 2);
                            PortLogHelper.TracePortRecvMsg(this.GetType().Name, tempData);
                            portOwner.Receive(this, new ByteArrayWrap(tempData));
                        }
                    }
                }
                else
                {
                    //��������
                    continue;
                }
            }
            dataReceiveThread = null;
        }
        private void GetDataReceiveThread()
        {
            if (dataReceiveThread == null)
            {
                dataReceiveThread = new Thread(new ThreadStart(DataReceiveThreadFunc));
                dataReceiveThread.Name = "Modbus Parse Protocol Thread";
                dataReceiveThread.Start();
            }
        }
    }
}
