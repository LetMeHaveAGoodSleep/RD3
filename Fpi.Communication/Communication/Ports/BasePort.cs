using System;
using System.Text;
using Fpi.Communication.Manager;
using Fpi.Util.Exeptions;
using Fpi.Xml;
#if !WINCE
using Fpi.Communication.Interfaces;
using Fpi.Util.Interfaces;
using Fpi.Util.Sundry;
#else
using Fpi.UI.Common.CE.Configure;
#endif

namespace Fpi.Communication.Ports
{
    public class BasePort : IPort, IDisposable,INaming
    {
        #region Э�����������ģ�����

        /// <summary>Ĭ�ϱ��� ASCII</summary>
        protected Encoding encoding = Encoding.UTF8;

        /// <summary>��С����֡����</summary>
        protected const int MIN_FRAME_SIZE = 1;

        /// <summary>�������֡����</summary>
        protected const int MAX_FRAME_SIZE = 8192;

        /// <summary>��ȡ���ݵĻ���</summary>
        protected byte[] frameBuffer;

        /// <summary>��¼ÿ�ζ�ȡ���ݳ���</summary>
        protected int readedBytes = 0;

        /// <summary>��ǰ�Ѿ��������ݳ���</summary>
        protected int recevicedDataSize = 0;

        /// <summary>֡ͷ��tempReadBuffer�е�λ��</summary>
        protected int headIndex = -1;

        #endregion

        protected IPortOwner portOwner;     //�ϲ�Port
        protected IPort lowerPort;          //�ײ�Port
        protected BaseNode config;          //�˿��������ýڵ�

        public BasePort()
        {
            frameBuffer = new byte[MAX_FRAME_SIZE];
        }

        public BasePort(IPort lowerPort)
        {
            LowerPort = lowerPort;
        }

        public string GetProperty(string id)
        {
            return GetProperty(id, null);
        }

        public string GetProperty(string id, string defaultValue)
        {
            if (config == null)
            {
                return defaultValue;
            }
            else
            {
                return config.GetPropertyValue(id, defaultValue);
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(FriendlyName))
            {
                return FriendlyName;
            }
            return this.GetType().Name;
        }


        #region IReceivable ��Ա

        public virtual void Receive(Object source, IByteStream data)
        {
            object newSource = this;
            data = ParseData(source, ref newSource, data);
            if (data != null)
            {
                PortLogHelper.TracePortRecvMsg(this.GetType().Name,data.GetBytes());
                portOwner.Receive(newSource, data);
            }
        }

        #endregion

        #region IPort ��Ա

        public virtual void Init(BaseNode config)
        {
            this.config = config;
        }
        
        public virtual object Send(object dest, IByteStream data)
        {
            IByteStream newData = PackData(dest, data);
            if (data is IOvertime)
            {
                newData = new TimeoutByteStream(newData, (IOvertime) data);
            }
            PortLogHelper.TracePortSendMsg(this.GetType().Name, data.GetBytes());
            object result = lowerPort.Send(dest, newData);
            if (result != null && result is IByteStream)
            {
                object newSource = this;
                return ParseData(this, ref newSource, (IByteStream) result);
            }
            return result;
        }

        public IPort LowerPort
        {
            get { return lowerPort; }
            set
            {
                if (value != null)
                {
                    this.lowerPort = value;
                    lowerPort.PortOwner = this;
                }
            }
        }

        public IPortOwner PortOwner
        {
            get { return portOwner; }
            set { portOwner = value; }
        }

        protected virtual IByteStream PackData(object dest, IByteStream data)
        {
            return data;
        }

        protected virtual IByteStream ParseData(object recvSource, ref object newSource, IByteStream data)
        {
            newSource = this;
            return data;
        }

        #endregion

        #region IPortOwner ��Ա

        public virtual void OnDisconnecting(IPort port)
        {
            connected = false;
            if (portOwner != null)
            {
                portOwner.OnDisconnecting(this);
            }
        }

        public virtual void InitPortOwner(IPort port, BaseNode propertys)
        {
        }

        #endregion

        #region IConnector ��Ա

        protected bool connected;

        public bool Connected
        {
            get { return connected; }
        }

        public virtual bool Open()
        {
            if (lowerPort == null)
            {
                throw new PlatformException("lowerPort is null !");
            }

            lock (this)
            {
                connected = lowerPort.Open();
                return connected;
            }
        }

        public virtual bool Close()
        {
            if (lowerPort == null)
            {
                throw new PlatformException("lowerPort is null !");
            }

            lock (this)
            {
                bool result = lowerPort.Close();
                connected = !result;
                return result;
            }
        }

        #endregion

        #region IDisposable ��Ա

        virtual public void Dispose()
        {
            if (connected)
            {
                Close();
            }
        }

        #endregion

        #region INaming ��Ա

        virtual public string FriendlyName
        {
            get { return GetType().Name; }
        }

        #endregion

    }
}