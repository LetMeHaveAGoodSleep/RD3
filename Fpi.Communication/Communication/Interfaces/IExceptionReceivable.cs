using System;

namespace Fpi.Communication.Interfaces
{
    public interface IExceptionReceivable
    {
        void Receive(Object source, object result, Exception ex);
    }
}