using System;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.CommPorts
{
    public class SimplePort : BasePort
    {
        public SimplePort()
        {
        }

        public override void Receive(Object source, IByteStream data)
        {
            IPortOwner portOwner = PortOwner;
            if (portOwner != null)
            {
                PortLogHelper.TracePortRecvMsg(this.GetType().Name, data.GetBytes());
                portOwner.Receive(source, data);
            }
        }

        public override Object Send(object dest, IByteStream data)
        {
            IPort lowerPort = LowerPort;
            if (lowerPort != null)
            {
                PortLogHelper.TracePortSendMsg(this.GetType().Name, data.GetBytes());
                return lowerPort.Send(dest, data);
            }
            return null;
        }
    }
}