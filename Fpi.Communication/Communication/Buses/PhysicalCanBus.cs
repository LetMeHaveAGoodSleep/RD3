using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace Fpi.Communication.Buses
{
    public class PhysicalCanBus
    {
        #region 单子模式

        private PhysicalCanBus()
        {
            isOpened = false;
        }

        private static object syncObj = new object();
        private static PhysicalCanBus instance = null;

        public static PhysicalCanBus GetInstance()
        {
            lock (syncObj)
            {
                if (instance == null)
                {
                    instance = new PhysicalCanBus();
                }
            }
            return instance;
        }

        #endregion

        #region DllImport

        [DllImport("candriver.dll", EntryPoint="CAN_Open", SetLastError=true)]
        public static extern uint CAN_Open(int hDeviceContext, IntPtr Access, int ShareMode);

        [DllImport("candriver.dll", EntryPoint="CAN_Close", SetLastError=true)]
        public static extern bool CAN_Close(int hOpenContext);

        [DllImport("candriver.dll", EntryPoint="CAN_Read", SetLastError=true)]
        public static extern uint CAN_Read(int hOpenContext, byte[] pBuffer, int Count);

        [DllImport("candriver.dll", EntryPoint="CAN_Write", SetLastError=true)]
        public static extern uint CAN_Write(int hOpenContext, byte[] pBuffer, int Count);

        [DllImport("candriver.dll", EntryPoint="CAN_Init", SetLastError=true)]
        public static extern uint CAN_Init(int dwContext);

        [DllImport("candriver.dll", EntryPoint="WaitForCanRXEVENT", SetLastError=true)]
        public static extern int WaitForCanRXEVENT();

        #endregion

        private bool isOpened;
        private int openCount = 0;
        private const int BUFFERSIZE = 10240;
        private const int ONCE_READ_COUNT = 5120;
        private byte[] readbuffer = new byte[ONCE_READ_COUNT];
        private Hashtable portTable = new Hashtable(); //存放每个端口数据缓冲区的哈希表

        public bool Init(int port)
        {
            lock (portTable)
            {
                if (!portTable.Contains(port))
                {
                    byte[] channelid = this.GetChannelId(port);
                    ChannelBuffer channelbuf = new ChannelBuffer(channelid);
                    portTable.Add(port, channelbuf);
                }
            }
            return true;
        }

        public bool Open(int port)
        {
            Init(port);
            if (openCount++ <= 0)
            {
#if !ONSIMULATE				
                isOpened = (CAN_Open(0, IntPtr.Zero, 1) != 0);
#else
				isOpened = true;
#endif
                if (isOpened)
                {
                    Thread readThread = new Thread(new ThreadStart(ReadThreadFunc));
                    readThread.Start();
                }
                return isOpened;
            }
            return true;
        }

        public bool Close(int port)
        {
            lock (portTable)
            {
                ChannelBuffer channelbuf = (ChannelBuffer) portTable[port];
                if (channelbuf != null)
                {
                    channelbuf.NotifyData();
                }
                portTable.Remove(port);
            }

            if (--openCount <= 0)
            {
                isOpened = false;
#if !ONSIMULATE
                return CAN_Close(0);
#else
				return true;
#endif
            }
            return true;
        }

        public bool Write(int port, byte[] buf)
        {
            if (buf.GetLength(0) > 0 && buf.GetLength(0) < BUFFERSIZE)
            {
                int frames = ((buf.Length - 1)/8 + 1);
                int length = frames*10;
                byte[] canBytes = new byte[length];
                byte[] channelId = this.GetChannelId(port);
                for (int i = 0; i < frames - 1; i++)
                {
                    Buffer.BlockCopy(channelId, 0, canBytes, i*10, 2);
                    Buffer.BlockCopy(buf, i*8, canBytes, i*10 + 2, 8);
                }
                //deal last section
                Buffer.BlockCopy(channelId, 0, canBytes, (frames - 1)*10, 2);
                int lastFrameLength = buf.Length - (frames - 1)*8;
                Buffer.BlockCopy(buf, (frames - 1)*8, canBytes, (frames - 1)*10 + 2, lastFrameLength);
                for (int i = 0; i < (8 - lastFrameLength); i++)
                {
                    canBytes[canBytes.Length - 1 - i] = 0x00;
                }
#if !ONSIMULATE
                return (CAN_Write(0, canBytes, canBytes.Length) != 0);
#else

				return true;
#endif
            }
            else
            {
                return false;
            }
        }

        public bool Read(int port, byte[] buf, int count, ref int bytesread)
        {
            if (portTable.Contains(port))
            {
                ChannelBuffer chbuf = (ChannelBuffer) portTable[port];
                chbuf.GetData(ref buf, count, ref bytesread);
                return true;
            }
            else
            {
                return false;
            }
        }

        #region port与channelid的映射关系

        private int GetPortId(byte[] buffer, int startIndex)
        {
            return buffer[startIndex];
        }

        private byte[] GetChannelId(int port)
        {
            byte[] channelid = new byte[2] {0x00, 0x00};
            channelid[0] = (byte) port;
            return channelid;
        }

        #endregion

        private bool ReadCan()
        {
#if !ONSIMULATE
            int rxBufferSize = (int) CAN_Read(0, readbuffer, ONCE_READ_COUNT);
#else


			
			int rxBufferSize = 100;
			for(int i= 0;i<2;i++)
			{
				byte[] cmddata = new byte[9]{0x02,0x0a,0xf3,0x01,0x01,0x86,0x55,0x00,0x00};
				if(i==0)
				{
					cmddata[4] = (byte)0x12;
				}
				else
				{
					cmddata[4] = (byte)0x12;
				}
				InsertProtocol bp = new InsertProtocol();
				byte[] packdata = bp.PackFrame(cmddata);

				byte[] ch = new byte[2];
				if(i==0)
				{
					ch[0] = (byte)0x12;
				}
				else
				{
					ch[0] = (byte)0x12;
				}
				ch[1] = 0x00;
				Buffer.BlockCopy(ch,0,readbuffer,2*i*10+0,2);
				Buffer.BlockCopy(packdata,0,readbuffer,2*i*10+2,8);
				Buffer.BlockCopy(ch,0,readbuffer,2*i*10+10,2);
				Buffer.BlockCopy(packdata,8,readbuffer,2*i*10+12,packdata.Length - 8);
				readbuffer[2*i*10+19] = 0x00;

			}

#endif
            //如果读取数据太大，超出范围则丢弃
            if (rxBufferSize > ONCE_READ_COUNT || rxBufferSize <= 0)
            {
                return false;
            }

            int frameCount = ((rxBufferSize - 1)/10 + 1); //总帧数

            for (int i = 0; i < frameCount; i++)
            {
                int port = this.GetPortId(readbuffer, i*10);
                if (!portTable.Contains(port))
                {
                    continue;
                }

                ChannelBuffer channelBuf = (ChannelBuffer) portTable[port];
                int copylength;
                if (i == frameCount - 1) //最后一帧
                {
                    copylength = rxBufferSize - (frameCount - 1)*10 - 2;
                }
                else
                {
                    copylength = 8;
                }

                channelBuf.PutData(readbuffer, i*10 + 2, copylength);
            }

            foreach (ChannelBuffer channelBuf in portTable.Values)
            {
                channelBuf.NotifyData();
            }

            return true;
        }

        //数据读取线程
        private void ReadThreadFunc()
        {
            //通知Open函数接收线程已经开始运行，Open函数可以返回
            while (isOpened)
            {
                ReadCan();
                Thread.Sleep(10);
            }
        }

        #region 通道缓存

        private class ChannelBuffer
        {
            public byte[] channelId;
            public byte[] channelBuffer;
            public int channelPos;

            private AutoResetEvent readEvent;
            private bool hasNewData;

            public ChannelBuffer(byte[] chid)
            {
                channelId = chid;
                channelBuffer = new byte[BUFFERSIZE];
                channelPos = 0;
                readEvent = new AutoResetEvent(false);
                hasNewData = false;
            }

            public void PutData(byte[] data, int startIndex, int count)
            {
                lock (channelBuffer)
                {
                    if (channelPos + count >= BUFFERSIZE)
                    {
                        channelPos = 0;
                    }
                    if (channelPos < 0)
                    {
                        string a = channelPos.ToString();
                    }
                    Buffer.BlockCopy(data, startIndex, channelBuffer, channelPos, count);

                    channelPos += count;
                }
                hasNewData = true;
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
                        Buffer.BlockCopy(channelBuffer, 0, buf, 0, count);
                        channelPos = channelPos - count;
                        Buffer.BlockCopy(channelBuffer, count, channelBuffer, 0, channelPos);
                        bytesread = count;
                    }
                    else
                    {
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

            public void WaitData()
            {
                readEvent.WaitOne();
            }
        }

        #endregion
    }
}
