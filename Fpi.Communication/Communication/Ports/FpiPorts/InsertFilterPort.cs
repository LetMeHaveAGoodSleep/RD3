using System;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.FpiPorts
{
    /*
     * 插入协议接收过滤 pan_xu 2014.10.21
     */
    public class InsertFilterPort : BasePort
    {
        private int localAddress = -1;   //工控机本机地址
        public InsertFilterPort()
        {
        }

        public override void Receive(Object source, IByteStream data)
        {
            /*
             * 地址识别
             */
            byte[] buffer = data.GetBytes();
            if (buffer[1] != localAddress)       //buffer[1]指目标地址
                return;
            IPortOwner portOwner = PortOwner;
            PortLogHelper.TracePortRecvMsg(this.GetType().Name, data.GetBytes());
            portOwner.Receive(source, data);
        }

        public override Object Send(object dest, IByteStream data)
        {
            /*
             * 发送前记下本主地址
             */
            byte[] buffer = data.GetBytes();
            localAddress = buffer[(int)(buffer[0]) + 2];   //buffer[0]存储目标地址长度

            IPort lowerPort = LowerPort;
            PortLogHelper.TracePortSendMsg(this.GetType().Name, data.GetBytes());
            return lowerPort.Send(dest, data);
        }
    }
}