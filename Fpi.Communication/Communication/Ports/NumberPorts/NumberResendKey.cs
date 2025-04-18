using Fpi.Communication.Ports.SyncPorts;
using Fpi.Communication.Interfaces;
using Fpi.Communication.Ports.SyncPorts.ResendKeys;

namespace Fpi.Communication.Ports.NumberPorts
{
    public class NumberResendKey : IResendKey
    {
        public NumberResendKey()
        {
        }

        #region IResendKey 成员

        public object GetSendKey(IByteStream data)
        {
            if (data is NumberData)
            {
                byte[] bytes = data.GetBytes();
                return bytes[0];
            }
            else
            {
                return null;
            }
        }

        public object GetReceiveKey(IByteStream data)
        {
            byte[] bytes = data.GetBytes();
            byte res = (byte) (bytes[0] ^ 0x80); //第一位取非
            return res;
        }

        #endregion
    }
}