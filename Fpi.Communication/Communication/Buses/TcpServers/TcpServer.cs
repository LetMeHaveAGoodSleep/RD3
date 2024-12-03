using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Fpi.Communication.Manager;
using Fpi.Util.Sundry;
using Fpi.Communication.Exceptions;
using Fpi.Util.WinApiUtil;
using Fpi.Properties;


namespace Fpi.Communication.Buses
{
    public class TcpServer : BaseBus
    {
        public static readonly string PropertyName_Port = "port";

        private int port;
        private TcpClient clientSocket;
        private NetworkStream netStream;
        private IntPtr acceptEvent;
        private TcpListener listener;

        private AutoResetEvent readEvent;

        //只接收一个client
        private void StartListening()
        {
            connected = true;
            listener = new TcpListener(IPAddress.Any, port);

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

                    clientSocket = listener.AcceptTcpClient();
                    //只支持一个客户端，关闭上一次连接-----------------------------------------------------2011.10.8. 修改人：毛洪兵 {
                    if (netStream != null)
                    {
                        netStream.Close();
                    }
                    //只支持一个客户端，关闭上一次连接-----------------------------------------------------2011.10.8. 修改人：毛洪兵 }
                    netStream = clientSocket.GetStream();
                    //WinApiWrapper.SetEvent(acceptEvent);
                    readEvent.Set();
                    BusLogHelper.TraceBusMsg(string.Format(Resources.AcceptTcpConnect, clientSocket.ToString()));
                }
                catch (Exception ex)
                {
                    BusLogHelper.TraceBusMsg(string.Format(Resources.TcpListenError, ex.Message));
                }
            }
        }

        public override string FriendlyName
        {
            get { return "TCP 服务端（单客户端）"; }
        }

        #region IConnector 成员

        public override void Init(Fpi.Xml.BaseNode config)
        {
            if (config == null)
                throw new CommunicationParamException(Resources.TcpLocalPortNotConfig);

            base.Init(config);
            port = Int32.Parse(config.GetPropertyValue(PropertyName_Port));
            readEvent = new AutoResetEvent(false);
        }

        public override bool Write(byte[] buf)
        {
            if (netStream != null)
            {
                netStream.Write(buf, 0, buf.Length);
                netStream.Flush();
                return true;
            }
            return false;
        }

        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
            if (netStream != null)
            {
                //Add by yjy 2010.05.18
                //catch read Exception for closing force
                try
                {
                    bytesread = netStream.Read(buf, 0, buf.Length);
                    //客户端断开后，会一直读，避免耗费CPU,添加等待时间----------------------------------2011.10.8 新增人:毛洪兵
                    if (bytesread == 0)
                    {
                        Thread.Sleep(20);
                    }
                    return true;
                }
                catch (System.IO.IOException ioEx)
                {
                    Thread.Sleep(20);
                    BusLogHelper.TraceBusMsg(String.Format(Resources.TcpServerReadError, ioEx.Message));
                }
            }

            readEvent.WaitOne();
            //WinApiWrapper.WaitForSingleObject(acceptEvent, UInt32.MaxValue);

            return false;
        }

        #endregion

        #region IConnector 成员

        public override bool Open()
        {
            bool flag = _Open();
            if (flag)
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.OpenTcpPortSucceed, port));
            }
            else
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.OpenTcpPortFailed, port));
            }

            connected = flag;
            return flag;
        }

        private bool _Open()
        {
            //acceptEvent = WinApiWrapper.CreateEvent(false, false, "accept event");
            (new Thread(new ThreadStart(StartListening))).Start();
            return true;
        }

        public override bool Close()
        {
            bool flag = _Close();
            if (flag)
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.CloseTcpPortSucceed, port));
            }
            else
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.CloseTcpPortFailed, port));
            }

            connected = !flag;
            return flag;
        }

        private bool _Close()
        {
            //WinApiWrapper.SetEvent(acceptEvent);
            readEvent.Set();

            connected = false;

            if (listener != null)
            {
                listener.Stop();
            }

            if (netStream != null)
            {
                netStream.Close();
                netStream = null;
            }

            if (clientSocket != null)
            {
                clientSocket.Close();
            }

            return true;
        }

        #endregion

        public override string InstanceName
        {
            get
            {
                return string.Format("TCP端口{0}", port);
            }
        }
    }
}