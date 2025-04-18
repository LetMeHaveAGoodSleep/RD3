using System;
using Fpi.Util.Compress;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.CommPorts
{
    public class CompressPort : BasePort
    {
        public CompressPort()
        {
        }

        //接收数据帧

        public override void Receive(Object source, IByteStream data)
        {
            byte[] receiveData = data.GetBytes();

            byte[] compressedData = CompressUtil.DecompressBytes(receiveData);
            PortLogHelper.TracePortRecvMsg(this.GetType().Name, compressedData);
            portOwner.Receive(this, new ByteArrayWrap(compressedData));
        }

        public override Object Send(object dest, IByteStream data)
        {
            IPort port = LowerPort;
            byte[] deCompressedData = CompressUtil.CompressBytes(data.GetBytes());
            PortLogHelper.TracePortSendMsg(this.GetType().Name, deCompressedData);
            return port.Send(dest, new ByteArrayWrap(deCompressedData));
        }
    }
}