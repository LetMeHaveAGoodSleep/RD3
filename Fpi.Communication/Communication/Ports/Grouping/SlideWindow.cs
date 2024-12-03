using System.Collections;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.Grouping
{
    /// <summary>
    /// 滑动窗口，对每个目的地址，对应一个滑动窗口
    /// </summary>
    public class SlideWindow
    {
		//帧编号大小
		protected const int END_FRAME_INDEX = 2 * 1024 * 1024 - 1;
		//初始帧，收到此编号的帧，前面接收的帧都清除
		protected const int INIT_FRAME_INDEX = 0;
		//缺省窗口大小
		public const int DEFAULT_WINDOW_SIZE = 3;
		//滑动窗口大小
        private int windowSize;

		//数据列表
		protected ArrayList dataList = new ArrayList();
		//发送端口
		protected IPort lowerPort;

		//构造函数
        public SlideWindow(IPort lowerPort)
        {
            this.lowerPort = lowerPort;
            windowSize = DEFAULT_WINDOW_SIZE;
        }

        ~SlideWindow()
        {
            ClearDataList();
        }

        public int WindowSize
        {
            get { return windowSize; }
            set { windowSize = value; }
        }

        //设置窗口大小
        public void SetWindowSize(int windowSize)
        {
            this.windowSize = windowSize;
        }

        //清空发送列表
        public void ClearDataList()
        {
            lock (dataList)
            {
                dataList.Clear();
            }
        }

        public ArrayList GetDataList()
        {
            return this.dataList;
        }
    }
}