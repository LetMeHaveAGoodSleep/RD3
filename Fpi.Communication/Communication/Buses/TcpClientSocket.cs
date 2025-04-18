using System;
using System.IO;
using System.Net.Sockets;
using Fpi.Communication.Manager;
using Fpi.Communication.Exceptions;
using Fpi.Properties;

namespace Fpi.Communication.Buses
{
    public class TcpClientSocket : BaseBus
    {
        public static readonly string PropertyName_Host = "host";
        public static readonly string PropertyName_Port = "port";

        private string hostName;
        private int port;
        private TcpClient clientSocket;
        private NetworkStream netStream;
        private int readErrorCount = 0;

        public override string FriendlyName
        {
            get { return "TCP 客户端"; }
        }
        public override string InstanceName
        {
            get
            {
                return hostName + ":" + port;
            }
        }
        #region IBus 成员

        public override void Init(Fpi.Xml.BaseNode config)
        {
            if (config == null)
                throw new CommunicationParamException(Resources.RemoteServerNotConfig);

            base.Init(config);
            this.hostName = config.GetPropertyValue(PropertyName_Host);
            this.port = Int32.Parse(config.GetPropertyValue(PropertyName_Port));
        }

        public override bool Write(byte[] buf)
        {
            netStream.Write(buf, 0, buf.Length);
            netStream.Flush();
            return true;
        }

        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
            bytesread = netStream.Read(buf, 0, buf.Length);
            if (bytesread <= 0)
            {
                //if (++readErrorCount > 10)
                //{
                //    throw new IOException();
                //}
                if (++readErrorCount > 1000)
                {
                    readErrorCount = 0;
                    throw new IOException();
                }
            }
            else
            {
                readErrorCount = 0;
            }
            return true;
        }

        #endregion

        #region IConnector 成员

        public override bool Open()
        {
            string linkInfo = hostName + ":" + port;
            bool flag = _Open();
            if (flag)
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.OpenTcpSucceed, linkInfo));
            }
            else
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.OpenTcpFailed, linkInfo));
            }

            connected = flag;

            return flag;
        }

        private bool _Open()
        {
            try
            {
                clientSocket = new TcpClient(hostName, port);
                netStream = clientSocket.GetStream();
            }
            catch (SocketException)
            {
                return false;
            }
            return true;
        }

        public override bool Close()
        {
            string linkInfo = hostName + ":" + port;
            bool flag = _Close();
            if (flag)
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.CloseTcpSucceed, linkInfo));
            }
            else
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.CloseTcpFailed, linkInfo));
            }

            connected = !flag;
            return flag;
        }

        private bool _Close()
        {
            netStream.Close();
            clientSocket.Close();
            return true;
        }

        #endregion
    }
}