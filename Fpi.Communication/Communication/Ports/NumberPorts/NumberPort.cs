using System;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.NumberPorts
{
    public class NumberPort : BasePort
    {
        public NumberPort()
        {
        }

        private byte frameNumber = 1;

        private byte GetFrameNumber()
        {
            lock (this)
            {
                byte result = frameNumber++;
                if (frameNumber > 100)
                {
                    frameNumber = 1;
                }
                return result;
            }
        }

        public NumberData BuildNumberData(IByteStream data)
        {
            byte number = GetFrameNumber();
            NumberData nd = new NumberData(number, data);
            return nd;
        }

        public override void Receive(Object source, IByteStream data)
        {
            IPortOwner portOwner = PortOwner;
            NumberData nd = new NumberData(data.GetBytes());
            portOwner.Receive(source, nd);
        }

        public override Object Send(object dest, IByteStream data)
        {
            IPort lowerPort = LowerPort;
            if (data is NumberData)
            {
                return lowerPort.Send(dest, data);
            }
            else
            {
                byte number = GetFrameNumber();
                NumberData nd = new NumberData(number, data);
                return lowerPort.Send(dest, nd);
            }
        }
    }
}