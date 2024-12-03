using Fpi.Communication.Ports.SyncPorts;
using Fpi.Communication.Interfaces;
using Fpi.Communication.Ports.SyncPorts.ResendKeys;

namespace Fpi.Communication.Commands
{
    /// <summary>
    /// �����ط���������ΪCommandPort�ϲ�ͬ���˿��ط���
    /// </summary>
    public class CommandResendKey : IResendKey
    {
        #region IResendKey ��Ա

        public object GetSendKey(IByteStream data)
        {
            ulong address;
            byte commandCode;
            byte extendCode;
            ParseKey(true, data.GetBytes(), out address, out commandCode, out extendCode);
            return (address << 16) + (ulong) (commandCode << 8) + (ulong) extendCode;
        }

        public object GetReceiveKey(IByteStream data)
        {
            ulong address;
            byte commandCode;
            byte extendCode;
            ParseKey(false, data.GetBytes(), out address, out commandCode, out extendCode);
            //��Ӧ֡������֡��Ӧ��Ӧ
            if (extendCode == (byte) 0xaa)
                extendCode = (byte) 0x55;
            else if (extendCode == (byte) 0x99)
                extendCode = (byte) 0x66;
            return (address << 16) + (ulong) (commandCode << 8) + (ulong) extendCode;
        }


        private void ParseKey(bool isSendData, byte[] data, out ulong address, out byte commandCode, out byte extendCode)
        {
            int targetAddrLength = (int) data[0];
            int sourceAddrLength = data[targetAddrLength + 1];
            address = 0;
            if (isSendData)
            {
                for (int i = 0; i < targetAddrLength; i++)
                    address += (ulong) (data[i + 1] << (i*8));
            }
            else
            {
                for (int i = 0; i < sourceAddrLength; i++)
                    address += (ulong) (data[targetAddrLength + 2 + i] << (i*8));
            }
            commandCode = data[sourceAddrLength + targetAddrLength + 2];
            extendCode = data[sourceAddrLength + targetAddrLength + 3];
        }

        #endregion
    }
}