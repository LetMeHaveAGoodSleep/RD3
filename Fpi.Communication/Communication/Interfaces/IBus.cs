using Fpi.Xml;

namespace Fpi.Communication.Interfaces
{
    public interface IBus : IConnector
    {
        void Init(BaseNode config);

        bool Write(byte[] buf);

        bool Read(byte[] buf, int count, ref int bytesread);
    }
}