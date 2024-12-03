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
    /// 协议解析器,分层处理
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
        /// 加载参数配置
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

        #region 属性
        
        /// <summary>
        /// 解析器的最底层Port
        /// </summary>
        public IPort LowPort
        {
            get { return lowPort; }
        }

        /// <summary>
        /// 解析器的最顶层Port
        /// </summary>
        public IPort TopPort
        {
            get { return topPort; }
        }

        #endregion

        #region IPortStatck 成员

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

        #region 开发人员须重载的方法

        /// <summary>
        /// 构造该协议解析器所包含的IPort序列
        /// </summary>
        /// <returns></returns>
        abstract protected IPort[] ConstructPorts();

        #endregion

    }
}