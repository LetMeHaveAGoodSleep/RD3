using System;
using Fpi.Communication.Interfaces;
using Fpi.Properties;

namespace Fpi.Communication.Ports.Grouping
{
    /// <summary>
    /// ���Ͷ˻�������
    /// </summary>
    public class SenderSlideWindow : SlideWindow
    {
        //����ط�����
        private const int MAX_RESEND_TIMES = 6;
		private const int DEFAULT_TIMEOUT = 60000;
		//��ǰ���ͱ�ţ�0��ʾ��ʼ֡���յ��˱�ŵ�֡��ǰ����յ�֡�����
		private int sendIndex;
		//��ʱ�ط�ʱ��
		private int timeout;

        public SenderSlideWindow(IPort lowerPort)
            : base(lowerPort)
		{
			sendIndex = INIT_FRAME_INDEX;
			timeout = DEFAULT_TIMEOUT;
		}

		//�õ����ͱ��,֡�����0��ʾǰ��֡�Ѿ������������Ŵ�1��ʼ
		public int GetSendIndex()
		{
            lock (this)
            {
                int result = sendIndex;
                if (++sendIndex > END_FRAME_INDEX)
                {
                    sendIndex = 0;
                }
                return result;
            }
		}

		//��ʱʱ������
		public void SetTimeout(int timeout)
		{
			this.timeout = timeout;
		}

		//�õ���������
		public int GetWindowIndex()
		{
            lock (dataList)
            {
                if (dataList.Count <= 0)
                    return -1;
                return ((GroupingFrame)dataList[0]).GetIndex();
            }
		}
		
		//�Ƿ��ڻ���������
		protected bool InSlideWindow(int index)
		{
			int windowIndex =  GetWindowIndex();
			if (windowIndex < 0)
				return false;
			int endIndex = windowIndex + WindowSize - 1;
			if ((endIndex > END_FRAME_INDEX) && (index < (endIndex - END_FRAME_INDEX)))
			{
				index += END_FRAME_INDEX + 1;
			}
			return (index >= windowIndex) && (index <= endIndex);
		}

        public void AddToDataList(object dest, byte[] data, int frameSize)
        {
            lock (dataList)
            {
                for (int i = 0; i < data.Length; i += frameSize)
                {

                    bool isEnd = (i + frameSize) >= data.Length;
                    bool isFirst = (i == 0);

                    byte[] temp = BitConverter.GetBytes(GetSendIndex());
                    if (BitConverter.IsLittleEndian)
                    {
						Array.Reverse(temp,0,temp.Length);
                    }
                    byte[] info = new byte[3];
                    info[0] = (byte)((isEnd ? 0x40 : 0x00) + (isFirst ? 0x20 : 0x00) + (temp[1] & 0x1F));
                    info[1] = temp[2];
                    info[2] = temp[3];
                    //byte info = (byte) ((isEnd ? 0x40 : 0x00) + (isFirst ? 0x20 : 0x00) + getSendIndex());
                    int thisFrameSize = Math.Min(data.Length - i, frameSize) + GroupingFrame.INFO_LENGTH;
                    byte[] frameData = new byte[thisFrameSize];
                    int offset = 0;
                    Buffer.BlockCopy(info, 0, frameData, offset, info.Length);
                    offset += info.Length;
                    Buffer.BlockCopy(data, i, frameData, offset, thisFrameSize - GroupingFrame.INFO_LENGTH);

                    dataList.Add(new GroupingFrame(dest, frameData));
                }
            }
        }

        //����֡
        public void SendGroupingFrame()
        {
            lock (dataList)
            {
                for (int i = 0; (i < WindowSize) && (i < dataList.Count); i++)
                {
                    GroupingFrame gf = (GroupingFrame) dataList[i];
                    if (gf.WillSend(timeout))
                    {
                        //Fpi.Log.LogHelper.TraceInfo("grouping send: " + Fpi.Util.StringParser.BytesToString(gf.GetData()));

                        lowerPort.Send(gf.GetObj(), gf);
                        gf.SendDeal();

                        //һֱ��ʱ��˵����·���ϣ������ǰ�����б�
                        if (gf.GetSendTimes() > MAX_RESEND_TIMES)
                        {
                            dataList.Clear();
                            sendIndex = INIT_FRAME_INDEX;
                            PortLogHelper.TracePortMsg("Resend Timeout��Frame:" + gf.GetIndex());
                        }

                    }

                }
            }
        }

		//����֡����
		public void ReceiveGroupingFrame(GroupingFrame gf)
		{
			//Fpi.Log.LogHelper.TraceInfo("grouping receive response: " + Fpi.Util.StringParser.BytesToString(gf.GetData()));

            if (InSlideWindow(gf.GetIndex()))
            {
                //Fpi.Log.LogHelper.TraceInfo("grouping send in slide window ");
                //LogHelper.TraceRecvMsg("Ӧ��֡�ţ� " + gf.GetIndex());

                lock (dataList)
                {
                    bool movedFlag = false;
                    //���ý���֡����״̬Ϊ�ѽ���
                    for (int i = 0; i < Math.Min(WindowSize, dataList.Count); i++)
                    {
                        GroupingFrame tempGf = (GroupingFrame)dataList[i];
                        if (tempGf.GetIndex() == gf.GetIndex())
                        {
                            tempGf.SetSendState(SendState.Replayed);
                            break;
                        }
                    }

                    //��������ǰ��,����ǰ����������״̬���ѻ�Ӧ��֡�ӷ����б����
                    for (int i = 0; (i < WindowSize) && (dataList.Count > 0); i++)
                    {
                        GroupingFrame tempGf = (GroupingFrame) dataList[0];
                        if (tempGf.GetSendState() == SendState.Replayed)
                        {
                            dataList.RemoveAt(0);
                            movedFlag = true;
                        }
                        else
                        {
                            break;
                        }
                    }
                    //��������Ѿ�����
                    if (movedFlag)
                    {
                        //LogHelper.TraceMsg("�ƶ�����");
                        SendGroupingFrame();
                    }
                }

               
            }
            else
            {
                PortLogHelper.TracePortMsg(string.Format(Resources.FrameNotInWindow, gf.GetIndex()));
            }
		}
	}
}