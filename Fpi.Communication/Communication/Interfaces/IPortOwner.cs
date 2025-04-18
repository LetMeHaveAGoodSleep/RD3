using Fpi.Xml;

namespace Fpi.Communication.Interfaces
{
    public interface IPortOwner : IReceivable
    {
        void OnDisconnecting(IPort port);
        void InitPortOwner(IPort port, BaseNode propertys);
    }
}