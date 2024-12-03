using System;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.FpiPorts
{
    /*
     * ����Э����չ��� pan_xu 2014.10.21
     */
    public class InsertFilterPort : BasePort
    {
        private int localAddress = -1;   //���ػ�������ַ
        public InsertFilterPort()
        {
        }

        public override void Receive(Object source, IByteStream data)
        {
            /*
             * ��ַʶ��
             */
            byte[] buffer = data.GetBytes();
            if (buffer[1] != localAddress)       //buffer[1]ָĿ���ַ
                return;
            IPortOwner portOwner = PortOwner;
            PortLogHelper.TracePortRecvMsg(this.GetType().Name, data.GetBytes());
            portOwner.Receive(source, data);
        }

        public override Object Send(object dest, IByteStream data)
        {
            /*
             * ����ǰ���±�����ַ
             */
            byte[] buffer = data.GetBytes();
            localAddress = buffer[(int)(buffer[0]) + 2];   //buffer[0]�洢Ŀ���ַ����

            IPort lowerPort = LowerPort;
            PortLogHelper.TracePortSendMsg(this.GetType().Name, data.GetBytes());
            return lowerPort.Send(dest, data);
        }
    }
}