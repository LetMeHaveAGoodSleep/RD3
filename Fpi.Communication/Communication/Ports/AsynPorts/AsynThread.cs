using System;
using System.Collections;
using System.Threading;
using Fpi.Communication.Interfaces;
using Fpi.Util;

namespace Fpi.Communication.Ports.AsynPorts
{
    public delegate void DataDelegate(object obj, IByteStream data);

    /// <summary>
    /// 异步端口
    /// </summary>
    public class AsynThread
    {
        //发送队列最大帧数
        private const int MAX_QUEUE_SIZE = 512;

        private bool alive;

        private Queue dataQueue = new Queue();

        //发送触发事件
        private AutoResetEvent dataEvent;

        private DataDelegate dataDelegate;

        public AsynThread(DataDelegate dataDelegate)
        {
            this.dataDelegate = dataDelegate;
            dataEvent = new AutoResetEvent(false);
        }

        public void Start()
        {
            alive = true;
            Thread receiveThread = new Thread(new ThreadStart(AsynDataThreadFunc));
            receiveThread.Start();
        }

        public void Stop()
        {
            alive = false;
            lock (dataQueue)
            {
                dataQueue.Clear();
            }
        }

        public void OnData(object dest, IByteStream data)
        {
            lock (dataQueue)
            {
                if (dataQueue.Count < MAX_QUEUE_SIZE)
                {
                    dataQueue.Enqueue(new DataNode(dest, data));
                    dataEvent.Set();
                }
            }
        }

        private void AsynDataThreadFunc()
        {
            while (alive)
            {
                try
                {
                    //等待数据发送事件通知
                    dataEvent.WaitOne();
                    //发送队列中所有数据
                    while (alive)
                    {
                        DataNode dataNode = null;
                        lock (dataQueue)
                        {
                            if (dataQueue.Count <= 0)
                                break;
                            dataNode = (DataNode) dataQueue.Dequeue();
                        }

                        dataDelegate(dataNode.obj, dataNode.data);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("AsynDataThreadFunc" + ex.ToString());
                }
            }
        }
    }
}