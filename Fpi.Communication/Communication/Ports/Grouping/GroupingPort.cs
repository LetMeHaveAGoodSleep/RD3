using Fpi.Communication.Manager;
using Fpi.Xml;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.Grouping
{
    /// <summary>
    /// GroupingPort 的摘要说明。
    /// </summary>
    public class GroupingPort : BasePort
    {
        public static readonly string PropertyName_WindowSize = "WindowSize";
        public static readonly string PropertyName_FrameSize = "FrameSize";
        public const int DEFAULT_WINDOW_SIZE = SlideWindow.DEFAULT_WINDOW_SIZE;
        public const int DEFAULT_FRAME_SIZE = 512;

        //帧大小
        private int frameSize = 0;
        private int windowSize = 0;

        private SenderSlideWindow senderSlideWindow;
        private ReceiverSlideWindow receiverSlideWindow;


        //构造函数
        public GroupingPort()
        {            
        }

        public override void Init(BaseNode config)
        {
            base.Init(config);

            string strWindowSize = GetProperty(PropertyName_WindowSize);
            if (!int.TryParse(strWindowSize, out windowSize))
            {
                windowSize = DEFAULT_WINDOW_SIZE;
            }

            string strFrameSize = GetProperty(PropertyName_FrameSize);
            if (!int.TryParse(strFrameSize, out frameSize))
            {
                frameSize = DEFAULT_FRAME_SIZE;
            }            
        }


        //设置帧大小
        public void SetFrameSize(int frameSize)
        {
            this.frameSize = frameSize;
        }


        //直接发送
        public override object Send(object dest, IByteStream data)
        {
            senderSlideWindow.AddToDataList(dest, data.GetBytes(), frameSize);
            Resend();
            return null;
        }


        public void Resend()
        {
            senderSlideWindow.SendGroupingFrame();
        }


        public override void Receive(object source, IByteStream data)
        {
            byte[] recvData = data.GetBytes();

            GroupingFrame gf = new GroupingFrame(source, recvData);


            //如果是应答帧
            if (gf.GetIsResponse())
            {
                senderSlideWindow.ReceiveGroupingFrame(gf);
            }
                //如果是请求帧
            else
            {
                //应答
                byte[] replayFrame = gf.GetReplayFrame();
                lowerPort.Send(source, new ByteArrayWrap(replayFrame));

                IPortOwner portOwner = PortOwner;
                receiverSlideWindow.ReceiveGroupingFrame(gf, portOwner);
                
			}
		}

        public override bool Open()
        {
            if (!base.Open())
            {
                return false;
            }
            senderSlideWindow = new SenderSlideWindow(lowerPort);
            receiverSlideWindow = new ReceiverSlideWindow(this, lowerPort);
            if (windowSize != 0)
            {
                senderSlideWindow.WindowSize = windowSize;
                receiverSlideWindow.WindowSize = windowSize;
            }

            ResendThread.GetInstance().AddPort(this);
            return true;
        }

        public override bool Close()
        {
            base.Close();
            ResendThread.GetInstance().RemovePort(this);

            ResendThread.GetInstance().Exit();
            senderSlideWindow = null;
            receiverSlideWindow = null;
            return true;
        }
    }
}