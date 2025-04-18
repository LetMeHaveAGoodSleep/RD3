using Fpi.Communication.Ports;

namespace Fpi.Communication.Interfaces
{
    public interface IPortStatck
    {
        IPort[] Ports
        {
            get;
        }
    }
}