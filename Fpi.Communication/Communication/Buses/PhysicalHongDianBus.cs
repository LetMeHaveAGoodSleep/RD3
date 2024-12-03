#region Э���ʽ

/*
         TE:�նˣ���PC
         DTU:���ݴ��䵥Ԫ�������ģ��
         
         
         Э���ʽ
         ---------------------------------------------------
         |֡ͷ |  ���ݳ��� | ������	| ��չ�� | ���� | ֡β |
         --------------------------------------------------
         |  1B |     2B	   |   1B	|   1B	 |<=1024|  1B  |
         --------------------------------------------------- 
          
         
         ��λ������ TE ����DTU
         ---------------------------------------------------
         |֡ͷ |  ���ݳ��� | ������	| ��չ�� | ���� | ֡β |
         --------------------------------------------------
         |0x7E |   XX	   |  0x09	|   XX	 |  XX	| 0x68 |
         ---------------------------------------------------          
          
         ��λ������ DTU ����TE
         ---------------------------------------------------
         |֡ͷ |  ���ݳ��� | ������	| ��չ�� | ���� | ֡β |
         --------------------------------------------------
         |0x7E |   XX	   |  0x8A	|   XX	 |  XX	| 0x68 |
         ---------------------------------------------------
         
          
         ��λ����ѯ����ͨ�� TE ����DTU
         ---------------------------------------------------
         |֡ͷ |  ���ݳ��� | ������	| ��չ�� | ���� | ֡β |
         --------------------------------------------------
         |0x7E |    0x00   |  0x04	|  0x00	 |  -	| 0x68 |
         ---------------------------------------------------
          
         ��λ����ѯ����ͨ���ظ� DTU ����TE
         ---------------------------------------------------
         |֡ͷ |  ���ݳ��� | ������	| ��չ�� | ���� | ֡β |
         --------------------------------------------------
         |0x7E |    0x00   |  0x84	|  XX	 |  -	| 0x68 |
         ---------------------------------------------------

         */

#endregion

using System;
using System.Collections;
using System.Threading;
using Fpi.Communication.Manager;
using Fpi.Util.Sundry;
using Fpi.Communication.Converter;
using Fpi.Util.Exeptions;
using Fpi.Xml;
using Fpi.Util.WinApiUtil;
using Fpi.Properties;

namespace Fpi.Communication.Buses
{
    public class PhysicalHongDianBus
    {
        #region ����ģʽ

        private PhysicalHongDianBus()
        {
            isOpened = false;
        }

        private static object syncObj = new object();
        private static PhysicalHongDianBus instance = null;

        public static PhysicalHongDianBus GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new PhysicalHongDianBus();
                }
            }
            return instance;
        }

        #endregion

        #region Э�鳣��

        private const byte Head = 0x68; //0x7E;
        private const byte Tail = 0x7E; //0x68;

        private const byte SendCmd = 0x09;
        private const byte RecvCmd = 0x8A;
        private const byte QueryCmd = 0x04;
        private const byte QueryResCmd = 0x84;

        #endregion

        private const int MAX_LEN = 1000;
        private const int SendTimeSpan = 100; //���η��͵ļ��(����)

        private CommBus commBus = null;
        private bool isOpened;
        private int openCount = 0;
        private Thread readThread;
        private DateTime lastSendTime = DateTime.MinValue;

        /// <summary>���ÿ���˿����ݻ������Ĺ�ϣ��</summary>
        private Hashtable portTable = new Hashtable();

        public void Init(BaseNode config)
        {
            commBus = new CommBus();
            commBus.Init(config);
        }

        private bool Init(int index)
        {
            lock (portTable)
            {
                if (!portTable.Contains(index))
                {
                    ChannelBuffer channelbuf = new ChannelBuffer(index);
                    portTable.Add(index, channelbuf);
                }
            }
            return true;
        }

        public CommBus CommBus
        {
            get { return commBus; }
        }

        public bool Open(int index)
        {
            Init(index);
            if (openCount++ <= 0)
            {
                isOpened = commBus.Open();
                if (isOpened)
                {
                    readThread = new Thread(new ThreadStart(ReadThreadFunc));

                    readThread.Name = "MultiGPRS ReadData Thread";

                    readThread.Start();
                }
                return isOpened;
            }
            return true;
        }

        public bool Close(int index)
        {
            lock (portTable)
            {
                ChannelBuffer channelbuf = (ChannelBuffer) portTable[index];
                if (channelbuf != null)
                {
                    channelbuf.NotifyData();
                }
                portTable.Remove(index);
            }

            if (--openCount <= 0)
            {
                isOpened = false;

                if (readThread != null && readThread.ThreadState != ThreadState.Stopped)
                {
                    readThread.Abort();
                    readThread = null;
                }


                return commBus.Close();
            }
            return true;
        }

        public bool Write(int index, byte[] sendData)
        {
            bool res = true;
            int currPos = 0;
            while (currPos < sendData.Length)
            {
                int count = sendData.Length - currPos;
                byte[] buf = new byte[count > MAX_LEN ? MAX_LEN : count];
                Buffer.BlockCopy(sendData, currPos, buf, 0, buf.Length);
                currPos += buf.Length;

                TimeSpan ts = DateTime.Now - lastSendTime;
                if (ts.TotalMilliseconds < SendTimeSpan)
                {
                    Thread.Sleep(SendTimeSpan);
                }

                #region ��װ������

                short len = (short) buf.Length;
                byte[] data = new byte[len + 6];

                int offset = 0;
                data[offset++] = Head;

                byte[] tmp = BitConverter.GetBytes(len);
                if (BitConverter.IsLittleEndian)
                {
                    byte b = tmp[0];
                    tmp[0] = tmp[1];
                    tmp[1] = b;
                }
                Buffer.BlockCopy(tmp, 0, data, offset, tmp.Length);
                offset += tmp.Length;

                data[offset++] = SendCmd;

                byte extCmd = (byte) index;
                data[offset++] = extCmd;

                Buffer.BlockCopy(buf, 0, data, offset, buf.Length);
                offset += buf.Length;

                data[offset] = Tail;

                res = commBus.Write(data);

                lastSendTime = DateTime.Now;

                #endregion
            }
            return res;
        }

        public bool Read(int index, byte[] buf, int count, ref int bytesread)
        {
            if (portTable.Contains(index))
            {
                ChannelBuffer chbuf = (ChannelBuffer) portTable[index];
                chbuf.GetData(ref buf, count, ref bytesread);
                return true;
            }
            else
            {
                throw new PlatformException("channel not initialize or not open!");
            }
        }

        #region ���ݶ�ȡ�����

        //���ݶ�ȡ�߳�
        private void ReadThreadFunc()
        {
            //֪ͨOpen���������߳��Ѿ���ʼ���У�Open�������Է���
            while (isOpened)
            {
                if (commBus.Connected)
                {
                    ReadCommBus();
                }
            }
        }

        private const int ONCE_READ_COUNT = 5120;
        private const int MAX_FRAME_SIZE = 8192;
        private const int BUFFERSIZE = 10240;
        private const int HEAD_SIZE = 1; //֡ͷ����
        private int headIndex = -1; //֡ͷ��readBuffer�е�λ��
        private int readedBytes = 0; //��¼ÿ�ζ�ȡ���ݳ���
        private int recevicedDataSize = 0; //��ǰ�Ѿ��������ݳ���
        private byte[] readBuffer = new byte[MAX_FRAME_SIZE*2];

        private bool ReadCommBus()
        {
            byte[] tmpBuffer = new byte[ONCE_READ_COUNT];
            bool res = commBus.Read(tmpBuffer, ONCE_READ_COUNT, ref readedBytes);

            if (!res)
            {
//��ȡʧ��
                return res;
            }

            //���ܽ�������̫�󣬿϶������쳣����ջ�����
            if (recevicedDataSize + readedBytes > readBuffer.Length)
            {
                recevicedDataSize = 0;
                headIndex = -1;
            }

            Buffer.BlockCopy(tmpBuffer, 0, readBuffer, recevicedDataSize, readedBytes);


            //֡β��readBuffer�е�λ��
            int tailIndex = -1;

            int startIndex = Math.Max(0, recevicedDataSize - HEAD_SIZE + 1);
            int endIndex = recevicedDataSize + readedBytes - HEAD_SIZE;

            if (headIndex > 0)
            {
//��֮ǰ���ҵ�֡ͷ
                startIndex = headIndex;
            }

            #region ֡ͷû�ҹ�

            for (int i = startIndex; i <= endIndex; i++)
            {
                //�ж�֡ͷ���Һ������ݳ���
                if (readBuffer[i] == Head && endIndex > (i + 5))
                {
                    if (readBuffer[i + 3] != RecvCmd)
                    {
                        continue;
                    }

                    //���ݳ���
                    short len = (short) DataConverter.GetInstance().ToInt32(readBuffer, i + 1);
                    //short len = BitConverter.ToInt16(readBuffer, i + 1);

                    if (len > 1024)
                    {
//û�д���1024������֡
                        continue;
                    }


                    //�ҵ�֡ͷ
                    headIndex = i;

                    //����֡β��λ��
                    int tailPos = i + len + 5;

                    //ʣ�����ݳ��Ȳ�����ȷ��֡β
                    if (tailPos > endIndex)
                    {
                        break;
                    }

                    //֡ͷ�ÿ�
                    headIndex = -1;

                    //ָ��λ�ò���֡β��������ǰ�ֽڣ�����ȷ��֡ͷ
                    if (readBuffer[tailPos] != Tail)
                    {
                        continue;
                    }
                    else
                    {
                        #region ����һ֡

                        //�ҵ�֡β
                        tailIndex = tailPos;

                        //���� :�����֡���չ��(ͨ��)������
                        byte[] tmp = new byte[len + 2];
                        Buffer.BlockCopy(readBuffer, i + 3, tmp, 0, tmp.Length);
                        ProcessRecvFrameData(tmp);

                        //��֡β��һ�ֽ�����һ��֡ͷ
                        i = tailIndex + 1;

                        #endregion
                    }
                }
                else
                {
                    //֡ͷ�ÿ�
                    headIndex = -1;
                }
            }

            #endregion

            recevicedDataSize += readedBytes;

            //�Ѿ��ҵ�֡β
            if (tailIndex > 0)
            {
                //readBuffer������ǰ�ƶ�
                int moveSize = tailIndex + HEAD_SIZE;
                Buffer.BlockCopy(readBuffer, moveSize, readBuffer, 0, recevicedDataSize - moveSize);
                if (headIndex > 0)
                {
                    headIndex -= moveSize;
                }
                recevicedDataSize -= moveSize;
            }
            return true;
        }

        private void ProcessRecvFrameData(byte[] frame)
        {
            /*
             ----------------------
             |������| ��չ�� | ����|
             -----------------------
             |  1B  |   1B   |  N  |
             -----------------------
             */

            byte cmd = frame[0];
            byte ext = frame[1];
            byte[] data = new byte[frame.Length - 2];
            Buffer.BlockCopy(frame, 2, data, 0, data.Length);

            //ͨ����
            int index = ext & 7;

            switch (cmd)
            {
                case RecvCmd:
                    {
//��������
                        if (portTable.ContainsKey(index))
                        {
                            ChannelBuffer cb = portTable[index] as ChannelBuffer;
                            cb.PutData(data);
                        }
                        break;
                    }
                case QueryResCmd:
                    {
//��ѯͨ���ظ�
                        channelExtCode = ext;
                        if (IntPtr.Zero != QueryChannelEvent)
                        {
                            WinApiWrapper.SetEvent(QueryChannelEvent);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        #endregion

        #region ��ѯͨ���Ƿ����

        private AutoResetEvent are = new AutoResetEvent(false);
        private IntPtr QueryChannelEvent = IntPtr.Zero;
        private byte channelExtCode;

        private bool GetChannelActive(int index)
        {
            QueryChannelEvent = WinApiWrapper.CreateEvent(true, false, "QueryChannel");

            short len = 0;
            byte[] data = new byte[6];

            int offset = 0;
            data[offset++] = Head;

            //byte[] tmp = BitConverter.GetBytes(len);
            byte[] tmp = DataConverter.GetInstance().GetBytes((int) len);
            Buffer.BlockCopy(tmp, 0, data, offset, tmp.Length);
            offset += tmp.Length;

            data[offset++] = QueryCmd;

            byte extCmd = 0x00;
            data[offset++] = extCmd;

            data[offset] = Tail;

            bool res = commBus.Write(data);
            if (!res)
            {
                return res;
            }

            int eventResult = WinApiWrapper.WaitForSingleObject(QueryChannelEvent, 300);
            if (eventResult == (int) APIConstants.WAIT_OBJECT_0)
            {
                return false;
            }

            int i = ((int) Math.Pow(2, index - 1)) & channelExtCode;
            QueryChannelEvent = IntPtr.Zero;

            return (i == 1);
        }

        #endregion

        #region ͨ������

        private class ChannelBuffer
        {
            public int index;
            public byte[] channelBuffer;
            public int channelPos;
            private AutoResetEvent readEvent;
            private bool hasNewData;

            public ChannelBuffer(int index)
            {
                this.index = index;

                hasNewData = false;
                channelPos = 0;
                channelBuffer = new byte[BUFFERSIZE];
                readEvent = new AutoResetEvent(false);
            }

            public void PutData(byte[] data)
            {
                lock (channelBuffer)
                {
                    if (channelPos + data.Length >= BUFFERSIZE)
                    {
                        channelPos = 0;
                    }

                    Buffer.BlockCopy(data, 0, channelBuffer, channelPos, data.Length);

                    channelPos += data.Length;
                }
                hasNewData = true;
                NotifyData();
            }

            public void GetData(ref byte[] buf, int count, ref int bytesread)
            {
                if (channelPos <= 0)
                {
                    WaitData();
                }

                lock (channelBuffer)
                {
                    if (count < channelPos)
                    {
//�� count ������
                        Buffer.BlockCopy(channelBuffer, 0, buf, 0, count);
                        channelPos = channelPos - count;
                        Buffer.BlockCopy(channelBuffer, count, channelBuffer, 0, channelPos);
                        bytesread = count;
                    }
                    else
                    {
//û count ������
                        Buffer.BlockCopy(channelBuffer, 0, buf, 0, channelPos);
                        bytesread = channelPos;
                        channelPos = 0;
                    }
                }
            }

            public void NotifyData()
            {
                if (hasNewData)
                {
                    readEvent.Set();
                    hasNewData = false;
                }
            }

            private void WaitData()
            {
                readEvent.WaitOne();
            }
        }

        #endregion       
    }
}