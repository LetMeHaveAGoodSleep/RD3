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
    /// Э��ģ��������Ϣ
    /// </summary>
    public class ProtocolDesc : ProtocolComponent
    {
        /// <summary>Sender ��ʱ���Ĵ������(����)</summary>
        protected int senderInterval;

        /// <summary>Э��༭�����ʵ����</summary>        
        protected Type impProtocolEdit;

        public ProtocolDesc()
        {
            InitConfig();
        }

        #region ����

        /// <summary>�������Ĵ������</summary>
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

        #region Ԥ����Ӧ����ԱӦʵ��

        /// <summary>
        /// ��ʼ��Э��������Ϣ
        /// </summary>
        protected virtual void InitConfig()
        {
            senderInterval = 0;
            impProtocolEdit = null;
        }

        /// <summary>
        /// ��ȡ��Э���Ĭ�ϲ�������
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
        /// ��ȡָ�� IPort �� IPortOwner ��Ĭ�ϲ�������
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