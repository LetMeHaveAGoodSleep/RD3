using System.Collections;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Ports.Grouping
{
    /// <summary>
    /// �������ڣ���ÿ��Ŀ�ĵ�ַ����Ӧһ����������
    /// </summary>
    public class SlideWindow
    {
		//֡��Ŵ�С
		protected const int END_FRAME_INDEX = 2 * 1024 * 1024 - 1;
		//��ʼ֡���յ��˱�ŵ�֡��ǰ����յ�֡�����
		protected const int INIT_FRAME_INDEX = 0;
		//ȱʡ���ڴ�С
		public const int DEFAULT_WINDOW_SIZE = 3;
		//�������ڴ�С
        private int windowSize;

		//�����б�
		protected ArrayList dataList = new ArrayList();
		//���Ͷ˿�
		protected IPort lowerPort;

		//���캯��
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

        //���ô��ڴ�С
        public void SetWindowSize(int windowSize)
        {
            this.windowSize = windowSize;
        }

        //��շ����б�
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