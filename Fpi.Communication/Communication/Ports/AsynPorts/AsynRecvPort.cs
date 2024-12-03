using System;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.AsynPorts
{
    /// <summary>
    /// 异步接收端口
    /// </summary>
    public class AsynRecvPort : BasePort
    {
        private AsynThread asynThread;

        public AsynRecvPort()
        {
        }

        public override bool Open()
        {
            bool result = base.Open();
            if (result)
            {
                asynThread = new AsynThread(new DataDelegate(RecvDelegate));
                asynThread.Start();
            }
            return result;
        }

        private void RecvDelegate(object obj, IByteStream data)
        {
            IPortOwner portOwner = PortOwner;
            portOwner.Receive(obj, data);
        }

        public override bool Close()
        {
            bool result = base.Close();
            if (asynThread != null)
            {
                asynThread.Stop();
                asynThread = null;
            }
            return result;
        }

        public override void Receive(Object source, IByteStream data)
        {
            if (asynThread != null)
            {
                asynThread.OnData(source, data);
            }
        }

        public override Object Send(object dest, IByteStream data)
        {
            IPort port = LowerPort;
            return port.Send(dest, data);
        }


    }
}