using Fpi.Communication;
using Fpi.Communication.Manager;
using Fpi.Communication.Ports;
using Fpi.Xml;
using Fpi.Communication.Interfaces;
using System;
using Fpi.Util.Reflection;

namespace Fpi.Communication.Protocols
{
    public delegate void ReceiveFromHandler(Pipe pipe, object[] formattedData);

    /// <summary>
    /// Э���еĽ�����
    /// ����������չ�Ľ���������ʹ����չ������������ʹ��Ĭ�Ͻ�����
    /// </summary>
    public class Receiver : ProtocolComponent, IPortOwner, IDisposable
    {
        public Receiver()
        {
        }
        ~Receiver()
        {
            Dispose();
        }
        //�Ƿ���Զ���Receiver����Ĭ�ϸ�Ϊtrue
        //modified by zhangyq.2010.3.16.
        //protected bool canExtended = false;
        protected bool canExtended = true;
        protected IPortOwner extendReceiver = null;
        public ReceiveFromHandler ReceiveFromEvent;

        /// <summary>��չ�Ľ�����</summary>
        public IPortOwner ExtendReceiver
        {
            get { return extendReceiver; }
        }

        /// <summary>�Ƿ���չ������</summary>
        public bool CanExtended
        {
            get { return canExtended; }
        }

        protected void SubmitFormattedData(Pipe pipe, object[] formattedData)
        {
            if (ReceiveFromEvent != null)
                ReceiveFromEvent(pipe, formattedData);
        }

        protected override void ActionPipe(Pipe pipe)
        {
            if (pipe != null)
            {
                extendReceiver = CreateExtendReceiver(pipe);
                LoadProperty(pipe);

            }
            else
            {
                extendReceiver = null;
            }
        }

        #region IPortOwner

        #region IPortOwner ��Ա

        public virtual void OnDisconnecting(IPort port)
        {
            if (extendReceiver == null)
            {
                ProtocolLogHelper.TraceMsg("Disconnected:" + pipe.name);
            }
            else 
            {
                extendReceiver.OnDisconnecting(port);
            }
        }

        public virtual void InitPortOwner(IPort port, BaseNode propertys)
        {
            if (extendReceiver != null)
            {
                extendReceiver.InitPortOwner(port, propertys);
            }
        }

        #endregion

        #region IReceivable ��Ա

        public void Receive(object source, IByteStream data)
        {
            if (pipe == null || !pipe.valid)
                return;

            try
            {
                if (extendReceiver != null)
                {
                    extendReceiver.Receive(source, data);
                }
                else
                {
                    ProcesseReceivedData(source, data);
                }
            }
            catch (Exception ex)
            {
                string error = string.Format("{0} receiver exception:{1}", owner, ex.Message);
                throw new Exception(error);
            }
        }

        virtual protected void ProcesseReceivedData(object source, IByteStream data)
        { 
        }

        #endregion

        #endregion

        /// <summary>
        /// ���ز�������
        /// </summary>
        /// <param name="pipe"></param>
        private void LoadProperty(Pipe pipe)
        {
            Property protocolProperty = pipe.GetProtocolProperty(owner.id);
            if (protocolProperty != null)
            {
                Property receiverProperty =
                    (extendReceiver != null
                    ? protocolProperty.GetProperty(extendReceiver.GetType().FullName)
                    : protocolProperty.GetProperty(GetType().FullName));

                InitPortOwner(owner.Parser.TopPort, receiverProperty);
            }
        }

        /// <summary>
        /// ������չ�Ľ�����
        /// </summary>
        /// <param name="pipe"></param>
        /// <returns></returns>
        private IPortOwner CreateExtendReceiver(Pipe pipe)
        {
            if (!canExtended)
                return null;
            string typeName = pipe.GetCustomReceiver(owner.id);
            if (typeName != null)
            {
                return ReflectionHelper.CreateInstance(typeName) as IPortOwner;
            }
            return null;
        }

        #region IDisposable ��Ա

        public virtual void Dispose()
        {

        }

        #endregion
    }
}