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
        #region 协议解析常量及模版变量

        /// <summary>默认编码 ASCII</summary>
        protected Encoding encoding = Encoding.UTF8;

        /// <summary>最小数据帧长度</summary>
        protected const int MIN_FRAME_SIZE = 1;

        /// <summary>最大数据帧长度</summary>
        protected const int MAX_FRAME_SIZE = 8192;

        /// <summary>读取数据的缓存</summary>
        protected byte[] frameBuffer;

        /// <summary>记录每次读取数据长度</summary>
        protected int readedBytes = 0;

        /// <summary>当前已经接收数据长度</summary>
        protected int recevicedDataSize = 0;

        /// <summary>帧头在tempReadBuffer中的位置</summary>
        protected int headIndex = -1;

        #endregion

        protected IPortOwner portOwner;     //上层Port
        protected IPort lowerPort;          //底层Port
        protected BaseNode config;          //端口属性配置节点

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


        #region IReceivable 成员

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

        #region IPort 成员

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

        #region IPortOwner 成员

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

        #region IConnector 成员

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

        #region IDisposable 成员

        virtual public void Dispose()
        {
            if (connected)
            {
                Close();
            }
        }

        #endregion

        #region INaming 成员

        virtual public string FriendlyName
        {
            get { return GetType().Name; }
        }

        #endregion

    }
}