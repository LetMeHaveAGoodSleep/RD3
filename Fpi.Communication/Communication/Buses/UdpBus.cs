using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Fpi.Communication.Manager;
using System.Windows.Forms;
using Fpi.Xml;

//modified by zf
namespace Fpi.Communication.Buses
{
    public class UdpBus : BaseBus
    {
        public UdpBus()
        {

        }

        static public readonly string PropertyName_Host = "host";
        static public readonly string PropertyName_SendPort = "sendport";
        static public readonly string PropertyName_RecPort = "recvport";

        private string hostName;
        private int sendport;
        private int recport = -1;

        private UdpClient localPeer;
        private IPEndPoint remoteEP;

        public override string FriendlyName
        {
            get
            {
                return "UDP 通信";
            }
        }

        public override string InstanceName
        {
            get
            {
                return hostName + ":" + sendport;
            }
        }

        #region IBus 成员
        public override void Init(BaseNode config)
        {
            base.Init(config);
            this.hostName = config.GetPropertyValue(PropertyName_Host);
            this.sendport = Int32.Parse(config.GetPropertyValue(PropertyName_SendPort));
            this.recport = Int32.Parse(config.GetPropertyValue(PropertyName_RecPort));
        }
        public override bool Write(byte[] buf)
        {

            try
            {
                localPeer.Send(buf, buf.Length, remoteEP);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
            try
            {
                byte[] bytes = localPeer.Receive(ref remoteEP);
                try
                {
                    Fpi.Communication.Protocols.ProtocolLogHelper.TraceRecvMsg("Recv udp data");
                    Fpi.Communication.Interfaces.IByteStream bs = new ByteArrayWrap(bytes);
                    Fpi.Communication.Protocols.ProtocolLogHelper.TraceRecvMsg(bs);
                }
                catch
                {
                }
                if (bytes.Length > buf.Length)
                {
                    Buffer.BlockCopy(bytes, 0, buf, 0, buf.Length);
                    bytesread = buf.Length;
                }
                else
                {
                    Buffer.BlockCopy(bytes, 0, buf, 0, bytes.Length);
                    bytesread = bytes.Length;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new IOException(); 
            }
        }
        #endregion

        #region IConnector 成员
        public override bool Open()
        {
            remoteEP = new IPEndPoint(IPAddress.Parse(hostName), sendport);

            if (recport == -1)
                localPeer = new UdpClient();
            else
                localPeer = new UdpClient(this.recport);

            connected = true;
            return true;
        }

        public override bool Close()
        {

            remoteEP = null;
            if (localPeer != null)
            {
                localPeer.Close();
                localPeer = null;
            }
            connected = false;
            return true;
        }
        #endregion
    }
}
