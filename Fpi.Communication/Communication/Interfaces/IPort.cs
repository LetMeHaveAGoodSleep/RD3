using System;
using Fpi.Communication.Manager;
using Fpi.Xml;

namespace Fpi.Communication.Interfaces
{
    public interface IPort : IConnector, IPortOwner
    {
        void Init(BaseNode config);

        Object Send(object dest, IByteStream data);

        IPort LowerPort { get; set; }

        IPortOwner PortOwner { get; set; }
    }
}