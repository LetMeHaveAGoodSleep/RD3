using System;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.AsynPorts
{
    /// <summary>
    /// Òì²½·¢ËÍ¶Ë¿Ú
    /// </summary>
    public class AsynSendPort : BasePort
    {
        private AsynThread asynThread;

        public AsynSendPort()
        {
        }

        public override bool Open()
        {
            bool result = base.Open();
            if (result)
            {
                asynThread = new AsynThread(new DataDelegate(SendDelegate));
                asynThread.Start();
            }
            return result;
        }

        private void SendDelegate(object obj, IByteStream data)
        {
            IPort port = LowerPort;
            port.Send(obj, data);
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
            IPortOwner portOwner = PortOwner;
            portOwner.Receive(source, data);
        }

        public override Object Send(object dest, IByteStream data)
        {
            if (asynThread != null)
            {
                asynThread.OnData(dest, data);
            }
            return null;
        }
    }
}