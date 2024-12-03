using System;
using System.Collections;
using Fpi.Communication.Manager;
using Fpi.Util.Sundry;
using Fpi.Xml;
using Fpi.Communication.Interfaces;
using Fpi.Util.Reflection;
using Fpi.Communication.Ports.SyncPorts.ResendKeys;
using Fpi.Properties;

namespace Fpi.Communication.Ports.SyncPorts
{
    /// <summary>
    /// 7B7D��ʼ�ͽ�����֡��
    /// </summary>
    public class SyncPort : BasePort
    {
        public const int DEFAULT_TIMEOUT = 3000;
        public const int DEFAULT_TRYTIMES = 1;
        public const bool DEFAULT_UNIQUE = false;

        public static readonly string PropertyName_ResendKey = "resendKey";
        public static readonly string PropertyName_Timeout = "timeout";
        public static readonly string PropertyName_TryTimes = "tryTimes";
        public static readonly string PropertyName_Unique = "unique";


        private IResendKey resendKey;

        //��ʱʱ��
        private int timeout;

        //�ظ�����
        private int tryTimes;

        //��ʶ�Ƿ�ȫ��ͬ������
        private bool unique;

        private Hashtable nodeTable = Hashtable.Synchronized(new Hashtable());

        public SyncPort()
        {
        }

        ~SyncPort()
        {
            nodeTable.Clear();
        }


        public override void Init(BaseNode config)
        {
            base.Init(config);

            string _resendKey = GetProperty(PropertyName_ResendKey);
            if (_resendKey == null)
            {
                resendKey = null;
            }
            else
            {
                resendKey = (IResendKey) ReflectionHelper.CreateInstance(_resendKey);
            }

            timeout = StringUtil.ParseInt(GetProperty(PropertyName_Timeout, DEFAULT_TIMEOUT.ToString()));
            tryTimes = StringUtil.ParseInt(GetProperty(PropertyName_TryTimes, DEFAULT_TRYTIMES.ToString()));
            unique = bool.Parse(GetProperty(PropertyName_Unique, DEFAULT_UNIQUE.ToString()));
        }

        //û������keyʱ��ͨ�õ�ͬ�����ͽڵ�
        private SyncSendNode commSyncSendNode = new SyncSendNode();

        private SyncSendNode GetSyncSendNode(IByteStream data, bool isSend)
        {
            if (resendKey == null)
            {
                return commSyncSendNode;
            }

            object key = isSend ? resendKey.GetSendKey(data) : resendKey.GetReceiveKey(data);

            if (key == null)
            {
                return commSyncSendNode;
            }

            //�Խ��գ����Է��ؿ�
            if (!isSend)
            {
                if (nodeTable.ContainsKey(key))
                {
                    return nodeTable[key] as SyncSendNode;
                }
                else
                {
                    return null;
                }
            }

            //�����key
            lock (nodeTable)
            {
                if (nodeTable.Contains(key))
                {
                    return (SyncSendNode) nodeTable[key];
                }

                if (nodeTable.Count > 50)
                {
                    nodeTable.Clear();
                }

                SyncSendNode tempNode = new SyncSendNode();
                nodeTable.Add(key, tempNode);
                return tempNode;
            }
        }

        public override Object Send(object dest, IByteStream data)
        {
            //Ӧ�ò�ͬ�ĳ�ʱ����
            int _timeOut = this.timeout;
            int _tryTimes = this.tryTimes;

            CheckTimeOut(ref _timeOut, ref _tryTimes, data);
            data = ConvertData(data);
            CheckTimeOut(ref _timeOut, ref _tryTimes, data);

            if (_timeOut == 0)
            {
                IPort lowerPort = LowerPort;
                return lowerPort.Send(dest, data);
            }

            SyncSendNode waitNode = GetSyncSendNode(data, true);

            object lockObj = waitNode;
            if (unique)
            {
                lockObj = this;
            }

            lock (lockObj)
            {
                //���Է���tryTimes��
                for (int i = 0; i < _tryTimes; i++)
                {
                    waitNode.SetSyncSending(true);

                    IPort lowerPort = LowerPort;
                    lowerPort.Send(dest, data);

                    if (waitNode.WaitOne(_timeOut))
                    {
                        object result = waitNode.GetResult();
                        waitNode.Init();
                        return result;
                    }
                }
            }

            waitNode.Init();
            throw new TimeoutException(Resources.CommunicationTimeOut);
        }

        public override void Receive(Object source, IByteStream data)
        {
            SyncSendNode waitNode = GetSyncSendNode(data, false);

            //���û���ҵ�ͬ���ڵ㣬���͵��ϲ�˿�
            if (waitNode == null || !waitNode.IsSyncSending())
            {
                IPortOwner portOwner = PortOwner;
                if (portOwner != null)
                {
                    portOwner.Receive(source, data);
                }
            }
            else
            {
//ͬ��ʱ
                waitNode.SetResult(data);
                waitNode.Set();
            }
        }

        private void CheckTimeOut(ref int _timeOut, ref int _tryTimes, IByteStream data)
        {
            //Ӧ�ò�ͬ�ĳ�ʱ����
            if (data is IOvertime)
            {
                IOvertime to = data as IOvertime;
                if (0 != to.TimeOut)
                {
                    _timeOut = to.TimeOut;
                }
                if (0 != to.TryTimes)
                {
                    _tryTimes = to.TryTimes;
                }
            }
        }

        protected virtual IByteStream ConvertData(IByteStream data)
        {
            return data;
        }
    }
}