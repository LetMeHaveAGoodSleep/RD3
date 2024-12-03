using Fpi.Communication;
using Fpi.Communication.Interfaces;
using Fpi.Communication.Ports.CommPorts;

namespace Fpi.Communication.Protocols.Common.Simple
{
    /// <summary>
    /// ��򵥵�Э�������
    /// </summary>
    public class SimpleParser : Parser
    {
        protected override IPort[] ConstructPorts()
        {
            return new IPort[] { new SimplePort() };
        }
    }
}