using Fpi.Communication.Ports.NumberPorts;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.SyncPorts
{
    public class NumberSyncPort : SyncPort
    {
        public NumberSyncPort()
        {
        }

        protected override IByteStream ConvertData(IByteStream data)
        {
            return (lowerPort as NumberPort).BuildNumberData(data);
        }
    }
}