using System;
using Fpi.Communication.Manager;
using Fpi.Util.Sundry;
using Fpi.Xml;
using Fpi.Communication.Interfaces;
using Fpi.Util.Reflection;
using Fpi.Properties;

namespace Fpi.Communication.Protocols
{
    /// <summary>
    /// 协议模版描述信息
    /// </summary>
    public class ProtocolDesc : ProtocolComponent
    {
        /// <summary>Sender 定时器的触发间隔(毫秒)</summary>
        protected int senderInterval;

        /// <summary>协议编辑界面的实现类</summary>        
        protected Type impProtocolEdit;

        public ProtocolDesc()
        {
            InitConfig();
        }

        #region 属性

        /// <summary>发送器的触发间隔</summary>
        public int SenderInterval
        {
            get
            {
                int value = senderInterval;
                try
                {
                    string strValue = ProtocolHelper.GetConfigValue(pipe, Protocol.PropertyName_SenderInterval);
                    int.TryParse(strValue, out value);
                }
                catch
                {
                }

                return value;
            }
            set
            {
                senderInterval = value * 1000;
                Property prop = ProtocolHelper.GetConfig(pipe, Protocol.PropertyName_SenderInterval);
                prop.value = (value*1000).ToString();
                PortManager.GetInstance().Save();
              
            }
        }

        #endregion

        #region 预留给应用人员应实现

        /// <summary>
        /// 初始化协议配置信息
        /// </summary>
        protected virtual void InitConfig()
        {
            senderInterval = 0;
            impProtocolEdit = null;
        }

        /// <summary>
        /// 获取该协议的默认参数配置
        /// </summary>
        /// <returns></returns>
        public Property GetDefaultParam()
        {
            Property protocolProperty = new Property(owner.id, Resources.ProtocolConfig);

            Property p;
            foreach(IPort port in owner.Parser.Ports)
            {
                p = GetDefaultPortParam(port.GetType());
                if (p != null)
                    protocolProperty.AddProperty(p);
            }

            if (owner.Receiver != null)
            {
                p = GetDefaultPortParam(owner.Receiver.GetType());
                if (p != null)
                    protocolProperty.AddProperty(p);

                if (owner.Receiver.ExtendReceiver != null)
                {
                    p = GetDefaultPortParam(owner.Receiver.ExtendReceiver.GetType());
                    if (p != null)
                        protocolProperty.AddProperty(p);
                }
            }

            return protocolProperty;
        }

        /// <summary>
        /// 获取指定 IPort 或 IPortOwner 的默认参数配置
        /// </summary>
        /// <param name="portType"></param>
        /// <returns></returns>
        protected virtual Property GetDefaultPortParam(Type portType)
        {
            return null;
        }
        

        #endregion
    }
}