using System;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.Grouping
{
    /// <summary>
    /// GroupingFrame ��ժҪ˵����
    /// </summary>
    public class GroupingFrame : IByteStream
    {
        //֡��Ϣ
        public const int INFO_LENGTH = 3;
		//֡��Ϣ
		private byte[] info;
		//����
		private byte[] data;
        //����Ŀ����߽���Դ
        private object obj;

		//�Է���֡�����淢�ʹ���
		private int sendTimes;
		//����״̬
		private int sendState;
		//����ʱ��
		private DateTime sendTime = DateTime.MinValue;

		//֡���캯��
		public GroupingFrame(object obj,  byte[] data)
		{
            this.obj = obj;
			this.data = data;
			sendTimes = 0;
			sendState = SendState.ToSend;
            this.info = new byte[INFO_LENGTH];
            Buffer.BlockCopy(data, 0, this.info, 0, INFO_LENGTH);
		}

		//�õ�֡���ݿ�ʼλ��
		public byte GetDataIndex()
		{
			return INFO_LENGTH;
		}

		//�õ�֡���ݳ���
		public int GetFrameDataLength()
		{
			return data.Length - INFO_LENGTH;
		}

		//�Ƿ�Ӧ��֡
		public bool GetIsResponse()
		{
			return (info[0] & 0x80) > 0;
		}
			
		//�Ƿ����֡
		public bool GetIsEnd()
		{
			return (info[0] & 0x40) > 0;
		}

        // �Ƿ���ʼ֡
        public bool GetIsFirst()
        {
            return (info[0] & 0x20) > 0;
        }
			
		//�֡���
		public int GetIndex()
		{
            byte[] temp = new byte[INFO_LENGTH + 1];
            temp[0] = 0x00;
            Buffer.BlockCopy(this.info, 0, temp, 1, INFO_LENGTH);
            temp[1] = (byte)(temp[1] & 0x1F);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(temp,0,temp.Length);
            }
            return BitConverter.ToInt32(temp, 0);
		}

		//������ݣ�����֡��Ϣ
		public byte[] GetData()
		{
			return data;
		}
        
        public object GetObj()
        {
            return obj;
        }

		//�õ�Ӧ��֡
		public byte[] GetReplayFrame()
		{
			byte[] replayFrame = new byte[INFO_LENGTH];
            Buffer.BlockCopy(this.info, 0, replayFrame, 0, INFO_LENGTH);

			//����֡���Ӧ��֡
			replayFrame[0] = (byte) (replayFrame[0] | (byte)0x80);
			return replayFrame;
		}

		//���ӷ��ʹ���
		public int IncSendTimes()
		{
            lock (this)
            {
                return ++sendTimes;
            }
		}

		//��÷��ʹ���
		public int GetSendTimes()
		{
			return sendTimes;
		}

		
		//�õ�����״̬
		public int GetSendState()
		{
			return sendState;
		}

		//���÷���״̬
		public void SetSendState(int sendState)
		{
            lock (this)
            {
                this.sendState = sendState;
            }
		}

		//����֡����
		public void SendDeal()
		{
            lock (this)
            {
                sendTime = DateTime.Now;
                sendState = SendState.Sended;
                sendTimes++;
            }
		}

		//�Ƿ���Ҫ����
		public bool WillSend(int timeout)
		{
            lock (this)
            {
                return (sendState == SendState.ToSend) ||
                    ((sendState == SendState.Sended) &&
                    (sendTime.AddMilliseconds(timeout).CompareTo(DateTime.Now) <= 0));
            }
		}

		//�õ���������
		public byte[] GetPackageData()
		{
			byte[] packageData = new byte[data.Length - INFO_LENGTH];
			Buffer.BlockCopy(data, INFO_LENGTH, packageData, 0, packageData.Length);
			return packageData;
		}

		//�õ��������ݳ���
		public int GetPackageLength()
		{
			return data.Length - INFO_LENGTH;
		}

        #region IByteStream ��Ա

        public byte[] GetBytes()
        {
            return this.data;
        }

        #endregion
    }
}