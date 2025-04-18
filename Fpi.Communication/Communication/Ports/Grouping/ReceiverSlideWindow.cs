using System;
using Fpi.Communication.Interfaces;
using Fpi.Properties;

namespace Fpi.Communication.Ports.Grouping
{
    /// <summary>
    /// 发送端滑动窗口
    /// </summary>
    public class ReceiverSlideWindow : SlideWindow
    {
        private const int RECEIVE_IDLE_TIME = 30 * 60 * 1000;

        private const int MAX_FRAME_COUNT = 1000;

        private DateTime lastReceiveDataTime;

        //在dataList中接收的连续帧位置
        private int continuedReceivedIndexInDataList;
        private GroupingPort groupingPort;

        public ReceiverSlideWindow(GroupingPort groupingPort, IPort lowerPort)
            : base(lowerPort)
        {
            continuedReceivedIndexInDataList = 0;
            this.groupingPort = groupingPort;
            this.lastReceiveDataTime = DateTime.Now;
        }

        //比较索引大小
        private int CompareIndex(int index1, int index2)
        {
            if (index1 == index2)
                return 0;
            if (Math.Abs(index1 - index2) > (END_FRAME_INDEX / 2))
            {
                return index1 > index2 ? -1 : 1;
            }
            return (index1 > index2) ? 1 : -1;
        }

        //index1的后续索引是否index2
        private bool IsNextIndex(int index1, int index2)
        {
            if (index1 == END_FRAME_INDEX)
                return index2 == 0;
            return index2 == (index1 + 1);
        }

        //合并n帧成一个包数据
        private byte[] GetPackageData(int endIndex)
        {
            //获取数据
            byte[] packageData;
            if (endIndex == 0)
            {
                packageData = ((GroupingFrame) dataList[0]).GetPackageData();
            }
            else
            {
                int packageLength = 0;
                for (int i = 0; i <= endIndex; i++)
                {
                    packageLength += ((GroupingFrame) dataList[i]).GetPackageLength();
                }
                packageData = new byte[packageLength];
                int pos = 0;
                for (int i = 0; i <= endIndex; i++)
                {
                    GroupingFrame gf = (GroupingFrame) dataList[i];
                    int count = gf.GetPackageLength();
                    Buffer.BlockCopy(gf.GetData(), gf.GetDataIndex(), packageData, pos, count);
                    pos += count;
                }
            }
            //删除数据列表
            for (int k = 0; k <= endIndex; k++)
            {
                dataList.RemoveAt(0);
            }

            continuedReceivedIndexInDataList = 0;

            return packageData;
        }


        //处理数据列表
        private void DealDataList(IReceivable receiver)
        {
            lock (dataList)
            {
                if (dataList.Count <= 0)
                {
                    return;
                }

                GroupingFrame firstGf = (GroupingFrame)dataList[0];
                if (!firstGf.GetIsFirst())
                {
                    return;
                }

                if (dataList.Count == 1)
                {
                    if (firstGf.GetIsEnd())
                    {
                        receiver.Receive(groupingPort, new ByteArrayWrap(GetPackageData(0)));
                    }
                }
                else if (dataList.Count > 1)
                {
                    for (int i = continuedReceivedIndexInDataList; i < dataList.Count - 1; i++)
                    {
                        GroupingFrame gf1 = (GroupingFrame)dataList[i];
                        GroupingFrame gf2 = (GroupingFrame)dataList[i + 1];
                        if (IsNextIndex(gf1.GetIndex(), gf2.GetIndex()))
                        {
                            if (gf2.GetIsEnd())
                            {
                                receiver.Receive(groupingPort, new ByteArrayWrap(GetPackageData(i + 1)));
                            }
                        }
                        else
                        {
                            continuedReceivedIndexInDataList = i;
                            break;
                        }
                    }
                }
            }
        }

        //接收帧处理
        public void ReceiveGroupingFrame(GroupingFrame gf, IPortOwner receiver)
        {
            //LogHelper.TraceRecvMsg("帧号： " + gf.GetIndex());
            lock (dataList)
            {
                if (isReceiveIdleTooLong())
                {
                    this.dataList.Clear();
                }
                
                this.lastReceiveDataTime = DateTime.Now;

                if (dataList.Count <= 0)
                {
                    dataList.Add(gf);
                    DealDataList(receiver);
                }
                else
                {
                    int beginCompareIndex = Math.Max(0, dataList.Count - WindowSize);
                    bool findFlag = false;
                    for (int i = dataList.Count - 1; i >= beginCompareIndex; i--)
                    {
                        GroupingFrame tempGf = (GroupingFrame)dataList[i];
                        int compare = CompareIndex(gf.GetIndex(), tempGf.GetIndex());
                        if (compare > 0)
                        {
                            dataList.Insert(i + 1, gf);
                            
                            if (gf.GetIsFirst())
                            {
                                for (int j = 0; j <= i; j++)
                                {
                                    this.dataList.RemoveAt(0);
                                }
                            }
                            
                            DealDataList(receiver);

                            findFlag = true;
                            break;
                        }
                        else if (compare == 0)
                        {
                            findFlag = true;
                            break;
                        }
                    }

                    if (!findFlag && ! ((GroupingFrame) dataList[0]).GetIsFirst())
                    {
                        dataList.Insert(0, gf);
                        DealDataList(receiver);
                    }

                    if (dataList.Count > MAX_FRAME_COUNT)
                    {
                        PortLogHelper.TracePortMsg(Resources.DataTooLong);
                    }
                }
            }
        }

        private bool isReceiveIdleTooLong()
        {
            DateTime buffer = DateTime.Now;
            TimeSpan span = buffer - this.lastReceiveDataTime;
            return span.Milliseconds > RECEIVE_IDLE_TIME;
        }
    }
}