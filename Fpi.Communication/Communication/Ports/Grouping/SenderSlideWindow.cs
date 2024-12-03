using System;
using Fpi.Communication.Interfaces;
using Fpi.Properties;

namespace Fpi.Communication.Ports.Grouping
{
    /// <summary>
    /// 发送端滑动窗口
    /// </summary>
    public class SenderSlideWindow : SlideWindow
    {
        //最多重发次数
        private const int MAX_RESEND_TIMES = 6;
		private const int DEFAULT_TIMEOUT = 60000;
		//当前发送编号，0表示初始帧，收到此编号的帧，前面接收的帧都清除
		private int sendIndex;
		//超时重发时间
		private int timeout;

        public SenderSlideWindow(IPort lowerPort)
            : base(lowerPort)
		{
			sendIndex = INIT_FRAME_INDEX;
			timeout = DEFAULT_TIMEOUT;
		}

		//得到发送编号,帧序号是0表示前面帧已经清除，否则序号从1开始
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

		//超时时间设置
		public void SetTimeout(int timeout)
		{
			this.timeout = timeout;
		}

		//得到窗口索引
		public int GetWindowIndex()
		{
            lock (dataList)
            {
                if (dataList.Count <= 0)
                    return -1;
                return ((GroupingFrame)dataList[0]).GetIndex();
            }
		}
		
		//是否在滑动窗口内
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

        //发送帧
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

                        //一直超时，说明链路故障，清除当前发送列表
                        if (gf.GetSendTimes() > MAX_RESEND_TIMES)
                        {
                            dataList.Clear();
                            sendIndex = INIT_FRAME_INDEX;
                            PortLogHelper.TracePortMsg("Resend Timeout。Frame:" + gf.GetIndex());
                        }

                    }

                }
            }
        }

		//接收帧处理
		public void ReceiveGroupingFrame(GroupingFrame gf)
		{
			//Fpi.Log.LogHelper.TraceInfo("grouping receive response: " + Fpi.Util.StringParser.BytesToString(gf.GetData()));

            if (InSlideWindow(gf.GetIndex()))
            {
                //Fpi.Log.LogHelper.TraceInfo("grouping send in slide window ");
                //LogHelper.TraceRecvMsg("应答帧号： " + gf.GetIndex());

                lock (dataList)
                {
                    bool movedFlag = false;
                    //设置接收帧发送状态为已接收
                    for (int i = 0; i < Math.Min(WindowSize, dataList.Count); i++)
                    {
                        GroupingFrame tempGf = (GroupingFrame)dataList[i];
                        if (tempGf.GetIndex() == gf.GetIndex())
                        {
                            tempGf.SetSendState(SendState.Replayed);
                            break;
                        }
                    }

                    //滑动窗口前移,即把前面连续发送状态是已回应的帧从发送列表清除
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
                    //如果窗口已经滑动
                    if (movedFlag)
                    {
                        //LogHelper.TraceMsg("移动窗口");
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