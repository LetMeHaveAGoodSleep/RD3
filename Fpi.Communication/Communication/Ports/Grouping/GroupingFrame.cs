using System;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.Grouping
{
    /// <summary>
    /// GroupingFrame 的摘要说明。
    /// </summary>
    public class GroupingFrame : IByteStream
    {
        //帧信息
        public const int INFO_LENGTH = 3;
		//帧信息
		private byte[] info;
		//数据
		private byte[] data;
        //发送目标或者接收源
        private object obj;

		//对发送帧，保存发送次数
		private int sendTimes;
		//发送状态
		private int sendState;
		//发送时间
		private DateTime sendTime = DateTime.MinValue;

		//帧构造函数
		public GroupingFrame(object obj,  byte[] data)
		{
            this.obj = obj;
			this.data = data;
			sendTimes = 0;
			sendState = SendState.ToSend;
            this.info = new byte[INFO_LENGTH];
            Buffer.BlockCopy(data, 0, this.info, 0, INFO_LENGTH);
		}

		//得到帧数据开始位置
		public byte GetDataIndex()
		{
			return INFO_LENGTH;
		}

		//得到帧数据长度
		public int GetFrameDataLength()
		{
			return data.Length - INFO_LENGTH;
		}

		//是否应答帧
		public bool GetIsResponse()
		{
			return (info[0] & 0x80) > 0;
		}
			
		//是否结束帧
		public bool GetIsEnd()
		{
			return (info[0] & 0x40) > 0;
		}

        // 是否起始帧
        public bool GetIsFirst()
        {
            return (info[0] & 0x20) > 0;
        }
			
		//活动帧序号
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

		//获得数据，包括帧信息
		public byte[] GetData()
		{
			return data;
		}
        
        public object GetObj()
        {
            return obj;
        }

		//得到应答帧
		public byte[] GetReplayFrame()
		{
			byte[] replayFrame = new byte[INFO_LENGTH];
            Buffer.BlockCopy(this.info, 0, replayFrame, 0, INFO_LENGTH);

			//请求帧变成应答帧
			replayFrame[0] = (byte) (replayFrame[0] | (byte)0x80);
			return replayFrame;
		}

		//增加发送次数
		public int IncSendTimes()
		{
            lock (this)
            {
                return ++sendTimes;
            }
		}

		//获得发送次数
		public int GetSendTimes()
		{
			return sendTimes;
		}

		
		//得到发送状态
		public int GetSendState()
		{
			return sendState;
		}

		//设置发送状态
		public void SetSendState(int sendState)
		{
            lock (this)
            {
                this.sendState = sendState;
            }
		}

		//发送帧处理
		public void SendDeal()
		{
            lock (this)
            {
                sendTime = DateTime.Now;
                sendState = SendState.Sended;
                sendTimes++;
            }
		}

		//是否需要发送
		public bool WillSend(int timeout)
		{
            lock (this)
            {
                return (sendState == SendState.ToSend) ||
                    ((sendState == SendState.Sended) &&
                    (sendTime.AddMilliseconds(timeout).CompareTo(DateTime.Now) <= 0));
            }
		}

		//得到包内数据
		public byte[] GetPackageData()
		{
			byte[] packageData = new byte[data.Length - INFO_LENGTH];
			Buffer.BlockCopy(data, INFO_LENGTH, packageData, 0, packageData.Length);
			return packageData;
		}

		//得到包内数据长度
		public int GetPackageLength()
		{
			return data.Length - INFO_LENGTH;
		}

        #region IByteStream 成员

        public byte[] GetBytes()
        {
            return this.data;
        }

        #endregion
    }
}