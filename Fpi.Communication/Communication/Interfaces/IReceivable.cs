using System;

namespace Fpi.Communication.Interfaces
{
    public interface IReceivable
    {
        void Receive(Object source, IByteStream data);
    }
}