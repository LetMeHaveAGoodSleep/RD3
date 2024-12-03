using Fpi.Communication.Manager;
namespace Fpi.Communication.Protocols.Interfaces
{
    public interface ISender
    {
        void SendData(Pipe pipe);
    }
}