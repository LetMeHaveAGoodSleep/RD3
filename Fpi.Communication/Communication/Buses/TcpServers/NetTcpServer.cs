//=================================================================================================
//说明：支持连接多个客户端的TCP服务器Bus,要求每个客户端连上来使用相同的协议
//     将每个客户端唯一信息与TcpClient socket绑定，这样对上层只需要一个pipe,
//     发送接收命令到哪个客户端对上层应用透明
//创建人：张永强
//创建时间：2011-7-1
//=================================================================================================

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Fpi.Util.Reflection;
using System.Threading;
using System.IO;
using Fpi.Properties;
using Fpi.Communication.Exceptions;

namespace Fpi.Communication.Buses
{
    /// <summary>
    /// 支持连接多个客户端的TcpServer Bus
    /// 创建人：张永强
    /// 创建时间：2011-6-30
    /// </summary>
    public class NetTcpServer : BaseBus
    {
        private static readonly string PropertyName_Port = "port";
        private static readonly string PropertyName_IP = "ip";
        private static readonly string PropertyName_ClientKeyPacket = "clientKeyPacket";
        private int port;
        private IPAddress ip;
        private string clientKeyPacketImp;
        private ClientKeyPacket keyPacket;

        private TcpClientList tcpClientList;
        private TcpListener listener;
        private ReadWaitNode readWaitNode = new ReadWaitNode(false);

        private void StartListening()
        {
            connected = true;
            listener = new TcpListener(ip, port);
            bool listening = false;
            
            while (connected)
            {
                try
                {
                    if (!listening)
                    {
                        listener.Start();
                        listening = true;
                    }
                    TcpClient clientSocket = listener.AcceptTcpClient();
                    NetTcpClient tcpclient = new NetTcpClient(clientSocket, this.clientKeyPacketImp, readWaitNode);
                    tcpClientList.Add(tcpclient);
                    if (keyPacket.SendCmd != null)
                    {
                        tcpclient.Write(keyPacket.SendCmd);
                    }
                    BusLogHelper.TraceBusMsg(string.Format(Resources.AcceptTcpConnect, clientSocket.ToString()));
                }
                catch (Exception ex)
                {
                    BusLogHelper.TraceBusMsg(string.Format(Resources.TcpListenError, ex.Message));
                }
            }
        }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get { return this.port; }
        }
        /// <summary>
        /// IP地址
        /// </summary>
        public IPAddress IP
        {
            get { return this.ip; }
        }
        public override string FriendlyName
        {
            get
            {
                return "TCP 服务端（多客户端）";
            }
        }
        public override void Init(Fpi.Xml.BaseNode config)
        {
            if (config == null)
                throw new CommunicationParamException(Resources.TcpLocalPortNotConfig);

            base.Init(config);
            port = Int32.Parse(config.GetPropertyValue(PropertyName_Port));
            ip = IPAddress.Parse(config.GetPropertyValue(PropertyName_IP));
            clientKeyPacketImp = config.GetPropertyValue(PropertyName_ClientKeyPacket);
            keyPacket = (ClientKeyPacket)ReflectionHelper.CreateInstance(clientKeyPacketImp);
            tcpClientList = new TcpClientList();
        }
        public override bool Write(byte[] buf)
        {
            //根据包找到对应的客户端
            object key = keyPacket.GetKeyfromData(buf);
            NetTcpClient tcpClient = tcpClientList[key];
            if (tcpClient != null)
            {
                tcpClient.Write(buf);
                return true;
            }
            else
            {
                return false;
            }
        }

        public NetTcpClient GetNetTcpClient(object key)
        {
            //根据包找到对应的客户端
            //object key = add;
            NetTcpClient tcpClient = tcpClientList[key];
            if (tcpClient != null)
            {
                return tcpClient;
            }
            else
            {
                return null;
            }
        }

        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
            readWaitNode.WaitOne();
            return readWaitNode.TcpClient.Read(buf, count, ref bytesread);
        }

        public override bool Open()
        {
            (new Thread(new ThreadStart(StartListening))).Start();
            return true;
        }

        public override bool Close()
        {
            connected = false;
            if (this.listener != null)
            {
                this.listener.Stop();
            }
            foreach (NetTcpClient client in this.tcpClientList)
            {
                client.Close();
            }
            return true;
        }

    }

    /// <summary>
    /// 与服务器相连的一个客户端
    /// 创建人：张永强
    /// 创建时间：2011-6-30
    /// </summary>
    public class NetTcpClient
    {
        private bool connected;
        private ReadWaitNode readWaitNode;
        private ClientKeyPacket clientKeyPacket;
        private TcpClient clientSocket;

        public TcpClient ClientSocket
        {
            get { return clientSocket; }
        }
        private NetworkStream netStream;

        private const int BUFFERSIZE = 10240;
        private byte[] dataBuffer = new byte[BUFFERSIZE];
        private byte[] tempBuffer = new byte[BUFFERSIZE];
        private int receivedPos = 0;
        private object key;
        private bool keyGetted;

        public event EventHandler DisConnected;
        /// <summary>
        /// 客户端唯一标识键
        /// </summary>
        public object Key
        {
            get { return this.key; }
            set { this.key = value; }
        }
        /// <summary>
        /// 是否已得到唯一标识
        /// </summary>
        public bool KeyGetted
        {
            get { return this.keyGetted; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientKeyPacketImp">解析key的包对象</param>
        /// <param name="stream">客户端流</param>
        /// <param name="readwait">读数据事件</param>
        public NetTcpClient(TcpClient clientsocket,string clientKeyPacketImp,ReadWaitNode readwait)
        {
            connected = true;
            keyGetted = false;
            this.clientSocket = clientsocket;
            this.netStream = clientsocket.GetStream();
            
            //创建解析键的对象
            clientKeyPacket = (ClientKeyPacket)ReflectionHelper.CreateInstance(clientKeyPacketImp);
            clientKeyPacket.Init();

            this.readWaitNode = readwait;

            //开启读数据线程
            Thread readThread = new Thread(ReadThread);
            readThread.Priority = ThreadPriority.AboveNormal;
            readThread.Start();
            
        }
        /// <summary>
        /// 读数据线程
        /// </summary>
        private void ReadThread()
        {
            while (connected)
            {
                int readBytes = 0;
                try
                {
                    readBytes = netStream.Read(tempBuffer, 0, BUFFERSIZE);
                    if (readBytes == 0)
                    {
                        throw new IOException();
                    }
                }
                catch (IOException)
                {
                    this.Close();
                    if (this.DisConnected != null)
                    {
                        this.DisConnected(this, null);
                    }
                }
                //解析数据从中获取客户端唯一key
                byte[] data = new byte[readBytes];
                Buffer.BlockCopy(tempBuffer, 0, data, 0, readBytes);
                if (!clientKeyPacket.KeyGetted)
                {
                    clientKeyPacket.PutData(data);
                    if (clientKeyPacket.KeyGetted)
                    {
                        this.key = this.clientKeyPacket.ClientKey;
                    }
                }

                //将收到数据存入缓冲区
                if (receivedPos+readBytes >= BUFFERSIZE)
                {
                    receivedPos = 0;
                }
                lock (this.dataBuffer)
                {
                    Buffer.BlockCopy(tempBuffer, 0, dataBuffer, receivedPos, readBytes);
                }
                receivedPos += readBytes;
                //触发读事件
                readWaitNode.TcpClient = this;
                readWaitNode.Set();
            }
        }
        /// <summary>
        /// 从该客户端读取数据
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="count"></param>
        /// <param name="bytesread"></param>
        public bool Read(byte[] buf, int count, ref int bytesread)
        {
            bytesread = Math.Min(count, receivedPos);
            lock (this.dataBuffer)
            {
                Buffer.BlockCopy(dataBuffer, 0, buf, 0, bytesread);
                receivedPos -= bytesread;
                Buffer.BlockCopy(dataBuffer, bytesread, dataBuffer, 0, receivedPos);
            }
            return true;
        }
        /// <summary>
        /// 向客户端写数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool Write(byte[] buffer)
        {
            netStream.Write(buffer, 0, buffer.Length);
            netStream.Flush();
            return true;
        }

        /// <summary>
        /// 关闭客户端
        /// </summary>
        public void Close()
        {
            this.connected = false;
            if (this.netStream != null)
            {
                this.netStream.Close();
            }
            if (this.clientSocket != null)
            {
                this.clientSocket.Close();
            }
        }

    }

    /// <summary>
    /// 已连接上的客户端列表
    /// 创建人：张永强
    /// 创建时间：2011-6-30
    /// </summary>
    public class TcpClientList :IEnumerable<NetTcpClient>
    {
        private List<NetTcpClient> clientList = new List<NetTcpClient>();
        public NetTcpClient this[object key]
        {
            get
            {
                foreach (NetTcpClient client in clientList)
                {
                    if (client.Key != null && client.Key.Equals(key))
                    {
                        return client;
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// 向列表中添加一个客户端
        /// </summary>
        /// <param name="client"></param>
        public void Add(NetTcpClient client)
        {
            client.DisConnected += new EventHandler(client_DisConnected);
            clientList.Add(client);
        }

        void client_DisConnected(object sender, EventArgs e)
        {
            clientList.Remove(sender as NetTcpClient);
        }

        #region IEnumerable<OneTcpClient> 成员

        public IEnumerator<NetTcpClient> GetEnumerator()
        {
            return clientList.GetEnumerator();
        }

        #endregion

        #region IEnumerable 成员

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return clientList.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// 等待读事件的类：主要封装了NetTcpClient到里面，用来获取触发读事件的客户端
    /// 创建人：张永强
    /// 创建时间：2011-7-1
    /// </summary>
    public class ReadWaitNode
    {
        private AutoResetEvent readEvent;
        private NetTcpClient tcpClient = null;

        public NetTcpClient TcpClient
        {
            get { return this.tcpClient; }
            set { this.tcpClient = value; }
        }
        public ReadWaitNode(bool initialState)
        {
            readEvent = new AutoResetEvent(initialState);
        }
        public bool WaitOne()
        {
            return readEvent.WaitOne();
        }
        public bool Reset()
        {
            return readEvent.Reset();
        }
        public bool Set()
        {
            return readEvent.Set();
        }
    }
}
