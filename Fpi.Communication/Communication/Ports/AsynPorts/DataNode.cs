using Fpi.Communication.Interfaces;
namespace Fpi.Communication.Ports.AsynPorts
{
    /// <summary>
    /// �첽�˿�
    /// </summary>
    public class DataNode
    {
        public object obj;
        public IByteStream data;

        public DataNode(object obj, IByteStream data)
        {
            this.obj = obj;
            this.data = data;
        }
    }
}