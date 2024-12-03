using System;
using System.Text;
using Fpi.Communication;
using Fpi.Communication.Manager;
using Fpi.Communication.Ports;
using Fpi.Communication.Exceptions;
using Fpi.Xml;
using Fpi.Communication.Interfaces;

namespace Fpi.Communication.Protocols
{
    /// <summary>
    /// Э�������,�ֲ㴦��
    /// </summary>
    public abstract class Parser : ProtocolComponent, IPortStatck
    {
        protected IPort lowPort;
        protected IPort topPort;

        public Parser()
        {
            ports = ConstructPorts();
            for (int i = ports.Length-1; i >0; i--)
            {
                ports[i].LowerPort = ports[i - 1];
            }

            lowPort = ports[0];
            topPort = ports[ports.Length - 1];
        }

        protected override void ActionPipe(Pipe pipe)
        {
            base.ActionPipe(pipe);

            if (pipe != null)
            {
                LoadProperty(pipe);
            }
        }

        /// <summary>
        /// ���ز�������
        /// </summary>
        /// <param name="pipe"></param>
        private void LoadProperty(Pipe pipe)
        {
            Property protocolProperty = pipe.GetProtocolProperty(owner.id);
            if (protocolProperty != null)
            {
                foreach (IPort port in ports)
                {
                    Property portProperty = protocolProperty.GetProperty(port.GetType().FullName);
                    port.Init(portProperty);
                }
            }
        }

        #region ����
        
        /// <summary>
        /// ����������ײ�Port
        /// </summary>
        public IPort LowPort
        {
            get { return lowPort; }
        }

        /// <summary>
        /// �����������Port
        /// </summary>
        public IPort TopPort
        {
            get { return topPort; }
        }

        #endregion

        #region IPortStatck ��Ա

        protected IPort[] ports;
        public IPort[] Ports
        {
            get
            {
                if (ports == null)
                {
                    ports = ConstructPorts();
                }
                return ports;
            }
        }

        #endregion

        #region ������Ա�����صķ���

        /// <summary>
        /// �����Э���������������IPort����
        /// </summary>
        /// <returns></returns>
        abstract protected IPort[] ConstructPorts();

        #endregion

    }
}