using Fpi.Communication.Interfaces;
namespace Fpi.Communication.Ports.SyncPorts.ResendKeys
{
	/// <summary>
	/// Fpi485ResendKey ��ժҪ˵����
	/// </summary>
	public class Fpi485ResendKey : IResendKey
	{
		public Fpi485ResendKey()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
		}
		#region IResendKey ��Ա

		private void ParseKey(bool isSendData, byte[] data, out ulong address, out byte commandCode, out byte extendCode)
		{
			int targetAddrLength = (int)data[0];
			int sourceAddrLength = data[targetAddrLength+1];
			address = 0;
			if (isSendData)
			{
				for (int i=0; i<targetAddrLength; i++)
					address += (ulong)(data[i+1] << (i*8));
			}
			else
			{
				for (int i=0; i<sourceAddrLength; i++)
					address += (ulong)(data[targetAddrLength+2+i] << (i*8));
			}
			commandCode = data[sourceAddrLength + targetAddrLength + 2];
			extendCode = data[sourceAddrLength + targetAddrLength + 3];
		}

		public object GetSendKey(IByteStream bs)
		{
            byte[] data = bs.GetBytes();
			ulong address;
			byte commandCode;
			byte extendCode;
			ParseKey(true, data, out address, out commandCode, out extendCode);
			return (address << 16) + (ulong)(commandCode << 8) + (ulong)extendCode;
		}

        public object GetReceiveKey(IByteStream bs)
		{
            byte[] data = bs.GetBytes();
            ulong address;
			byte commandCode;
			byte extendCode;
			ParseKey(false, data, out address, out commandCode, out extendCode);
			//��Ӧ֡������֡��Ӧ��Ӧ
			if (extendCode == (byte)0xaa)
				extendCode = (byte)0x55;
			else if (extendCode == (byte)0x99)
				extendCode = (byte)0x66;
			return (address << 16) + (ulong)(commandCode << 8) + (ulong)extendCode;
		}

		public bool IsUniqueKey()
		{
			//Fpi485Э��֡����Ψһ
			return false;
		}

		#endregion
	}
}
