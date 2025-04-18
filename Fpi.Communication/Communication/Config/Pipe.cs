using System;
using System.Xml;
using Fpi.Communication.Buses;
using Fpi.Communication.Exceptions;
using Fpi.Communication.Ports;
using Fpi.Instruments;
using Fpi.Util.Sundry;
using Fpi.Xml;
using Fpi.Communication.Protocols;
using Fpi.Communication.Interfaces;
using System.Collections;
using Fpi.Properties;

namespace Fpi.Communication.Manager
{
    public delegate void PipeValidChangedHandler(bool valid);

    public class Pipe : IdNameNode, IConnector
    {
        private static string propertyName_Sender = "Sender";
        private static string propertyName_Receiver = "Receiver";        

        #region 配置成员

        public bool valid;
        //added by zf  是否开机自动启动,默认启动
        public bool autoConnect = true;
        public bool autoReConnect = true;
        public string impBus;
        public string impProtocol;
        public ArrayList targets = new ArrayList();

        public override BaseNode Init(XmlNode node)
        {
            base.Init(node);
            _Instruments = GetInstruments();
            return this;
        }
        private Instrument[] GetInstruments()
        {
            if (targets == null)
                return new Instrument[0];

            Instrument[] array = new Instrument[targets.Count];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = InstrumentManager.GetInstance().GetInstrument((string)targets[i]);
            }
            return array;
        }

        #endregion        

        #region BuildPort\Send

        public object Send(string instrumentId, IByteStream data)
        {
            if (this.valid && connected)
            {
                return this.protocol.Parser.TopPort.Send(instrumentId, data);
            }
            else
            {
                PipeLogHelper.TraceMsg(string.Format(Resources.ChannelDisableAndSendFailed, this));
                return null;
            }
        }

        /// <summary>
        ///  构造通讯链路
        /// </summary>
        /// <returns>构建成功返回true,否则返回false</returns>
        public bool BuildLinkLayer()
        {
            if (!valid)
                return false;
            try
            {
                this.bus = BuildBus();
                if (this.protocol!=null&&this.protocol.Sender != null)
                {
                    this.protocol.Sender.Dispose();
                }
                if (this.protocol != null && this.protocol.Receiver != null)
                {
                    this.protocol.Receiver.Dispose();
                }
                this.protocol = BuildProtocol();

                this.protocol.Parser.LowPort.LowerPort = new BusPort(this.bus, this);
                this.protocol.Pipe = this;

                return true;
            }
            catch (Exception ex)
            {
                PipeLogHelper.TraceMsg(string.Format(Resources.ConstructRooterException, this, ex.Message));
                return false;
            }
        }

        private BaseBus BuildBus()
        {
            Property busProperty = GetBusProperty();
            BaseBus _bus = BusHelper.CreateBus(impBus, busProperty);
            return _bus;
        }

        private Protocol BuildProtocol()
        {
            Protocol p = ProtocolManager.CreateProtocol(impProtocol);
            return p;
        }

        #endregion

        #region 属性

        private BaseBus bus;
        private Protocol protocol;
        private Instrument[] _Instruments;

        public BaseBus Bus
        {
            get { return bus; }
        }
        public Protocol Protocol
        {
            get { return protocol; }
        }
        public Instrument[] Instruments
        {
            get
            {
                _Instruments = GetInstruments();                
                return _Instruments;
            }
        }

        #endregion

        #region IConnector 成员

        public event PipeValidChangedHandler PipeValidChangedEvent;
        protected void OnPipeValidChanged(bool valid)
        {
            connected = valid;
            if (PipeValidChangedEvent != null)
            {
                PipeValidChangedEvent(valid);
            }
        }

        bool connected = false;
        public bool Connected
        {
            get
            {
                if (this.protocol != null)
                    connected = this.protocol.Parser.TopPort.Connected;
                return connected;
            }
        }

        public bool Open()
        {
            try
            {
                if (!valid)
                {
                    throw new CommunicationException(string.Format(Resources.ChannelDisable, this));
                }
                if (this.bus == null)
                {
                    throw new CommunicationException(string.Format(Resources.ChannelRooterNotConfig, this));
                }
                if (this.protocol == null)
                {
                    throw new CommunicationException(string.Format(Resources.ChannelProtocolNotConfig, this));
                }

                lock (this)
                {
                    if (!connected)
                    {
                        bool result = this.protocol.Parser.TopPort.Open();
                        if (!result)
                        {
                            PipeLogHelper.TraceMsg(string.Format(Resources.ChannelOpenFailed, this));
                        }

                        OnPipeValidChanged(result);
                        return result;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                PipeLogHelper.TraceMsg(string.Format(Resources.ChannelOpenException, this, ex.Message));
                return false;
            }
        }

        public bool Close()
        {
            try
            {
                lock (this)
                {
                    if (connected)
                    {
                        bool result = this.protocol.Parser.TopPort.Close();
                        OnPipeValidChanged(!result);
                        return result;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                PipeLogHelper.TraceMsg(string.Format(Resources.ChannelCloseException, this, ex.Message));
                return false;
            }
        } 

        #endregion

        #region 通道配置信息存取等 方法

        /// <summary>
        /// 在通道协议栈中查找指定处理层Port
        /// </summary>
        /// <param name="portFullName"></param>
        /// <returns></returns>
        public IPort FindPort(string portFullName)
        {
            if (this.protocol == null)
                return null;

            foreach (IPort port in this.protocol.Parser.Ports)
            {
                if (port.GetType().FullName == portFullName)
                    return port;
            }
            return null;
        }

        public Property GetPortProperty(string portFullName)
        {
            return GetPortProperty(this.impProtocol, portFullName);
        }

        /// <summary>
        /// 通道的协议中各个Port（包括PortOwner）的配置属性，存储在协议属性的子属性集中
        /// </summary>
        /// <param name="portFullName">Port类型全名</param>
        public Property GetPortProperty(string impProtocol, string portFullName)
        {
            Property protocolProperty = GetProtocolProperty(impProtocol);
            Property portProperty = protocolProperty.GetProperty(portFullName);;
            if (portProperty == null)
            {
                portProperty = new Property(portFullName, Resources.ParamConfig);
                protocolProperty.AddProperty(portProperty);
            }
            return portProperty;
        }

        public string GetCustomSender()
        {
            return GetCustomSender(this.impProtocol);
        }

        /// <summary>
        /// 获取通道的协议中的自定义发送器 的实现类的全名称
        /// </summary>
        /// <returns></returns>
        public string GetCustomSender(string impProtocol)
        {
            Property protocolProperty = GetProtocolProperty(impProtocol);
            if (protocolProperty == null)
                return null;

            return protocolProperty.GetPropertyValue(propertyName_Sender);
        }

        public void SetCustomSender(string impProtocol, string impSender)
        {
            Property protocolProperty = GetProtocolProperty(impProtocol);
            if (protocolProperty == null)
                throw new Exception("protocolProperty is null");

            protocolProperty.SetProperty(propertyName_Sender, Resources.CustomSender, impSender);
        }
        
        public string GetCustomReceiver()
        {
            return this.GetCustomReceiver(this.impProtocol);
        }

        /// <summary>
        /// 获取通道的协议中的自定义接收器 的实现类的全名称
        /// </summary>
        /// <returns></returns>
        public string GetCustomReceiver(string impProtocol)
        {
            Property protocolProperty = GetProtocolProperty(impProtocol);
            if (protocolProperty == null)
                return null;

            return protocolProperty.GetPropertyValue(propertyName_Receiver);
        }

        public void SetCustomReceiver(string impProtocol, string impReceiver)
        {
            Property protocolProperty = GetProtocolProperty(impProtocol);
            if (protocolProperty == null)
                throw new Exception("protocolProperty is null");

            protocolProperty.SetProperty(propertyName_Receiver, Resources.CustomReceiver, impReceiver);
        }

        /// <summary>
        /// 获取通道的当前协议配置属性
        /// </summary>
        public Property GetProtocolProperty()
        {
            return GetProtocolProperty(this.impProtocol);
        }

        /// <summary>
        /// 获取通道的指定协议配置属性
        /// </summary>
        /// <param name="impProtocol"></param>
        /// <returns></returns>
        public Property GetProtocolProperty(string impProtocol)
        {
            if (impProtocol == null)
                return null;

            Property protocolProperty = GetProperty(impProtocol);
            if (protocolProperty == null)
            {
                protocolProperty = new Property(impProtocol, Resources.ProtocolConfig);
                AddProperty(protocolProperty);
            }
            return protocolProperty;
        }

        /// <summary>
        /// 获取通道的当前链路配置属性
        /// </summary>
        public Property GetBusProperty()
        {
            return GetBusProperty(this.impBus);
        }

        /// <summary>
        /// 获取通道的指定链路配置属性
        /// </summary>
        /// <param name="impBus"></param>
        /// <returns></returns>
        public Property GetBusProperty(string impBus)
        {
            if (impBus == null)
                return null;

            Property busProperty = GetProperty(impBus);
            if (busProperty == null)
            {
                busProperty = new Property(impBus, Resources.RooterConfig);
                AddProperty(busProperty);
            }
            return busProperty;
        }

        /// <summary>
        /// 清除指定参数的配置
        /// </summary>
        /// <param name="impProtocol"></param>
        public void ClearProperty(string propertyId)
        {
            if (propertyId != null)
                RemoveProperty(propertyId);
        }
        #endregion
    }
}