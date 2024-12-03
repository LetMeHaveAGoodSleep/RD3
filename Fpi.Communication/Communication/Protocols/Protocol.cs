using System;
using Fpi.Communication;
using Fpi.Communication.Manager;
using Fpi.Communication.Ports;
using Fpi.Communication.Exceptions;
using Fpi.Util.Sundry;
using Fpi.Xml;
using Fpi.Util.Interfaces;
using Fpi.Communication.Interfaces;
using System.Collections.Generic;
using Fpi.Properties;

namespace Fpi.Communication.Protocols
{
    /// <summary>
    /// 远程通讯协议组件包（含解析器、发送器、接收器、默认配置，其中解析器是核心，必须包含此组件）
    /// </summary>
    public abstract class Protocol : IdNameNode, INaming,  ISupportPipe
    {
        #region 属性ID

        //public static readonly string PropertyName_Protocol = Pipe.PropertyName_Protocol;

        ///// <summary>输出项目列表，按测量点分</summary>
        public static readonly string PropertyName_OutputList = "OutputList";
        public static readonly string PropertyName_SenderInterval = "SenderInterval";
        public static readonly string PropertyName_SelectMp = "SelectMp";

        #endregion

        #region 协议构造

        protected List<ProtocolComponent> components = new List<ProtocolComponent>();
        public Protocol()
        {
            this.id = this.GetType().FullName;
            this.name = this.GetType().Name;

            parser = ConstructParser();
            components.Add(parser);
            if (parser == null)
            {
                throw new ProtocolException(string.Format(Resources.NotContainParser, this));
            }

            sender = ConstructSender();
            if (sender != null)
            {
                components.Add(sender);
            }

            receiver = ConstructReceiver();
            if (receiver != null)
            {
                components.Add(receiver);
                parser.TopPort.PortOwner = receiver;
            }

            protocolDesc = ConstructProtocolDesc();
            if (protocolDesc != null)
            {
                components.Add(protocolDesc);
            }

            foreach (ProtocolComponent pc in components)
            {
                pc.Owner = this;
            }
        }

        #endregion

        #region 属性

        protected Parser parser;
        protected Sender sender;
        protected Receiver receiver;
        protected ProtocolDesc protocolDesc;

        /// <summary>
        /// 解析器
        /// </summary>
        public Parser Parser
        {
            get { return parser; }
        }

        /// <summary>
        /// 发送器
        /// </summary>
        public Sender Sender
        {
            get { return sender; }
        }

        /// <summary>
        /// 接收器
        /// </summary>
        public Receiver Receiver
        {
            get { return receiver; }
        }

        /// <summary>
        /// 通讯协议的配置描述
        /// </summary>
        public ProtocolDesc ProtocolDesc
        {
            get { return protocolDesc; }
        }

        #endregion

        #region 预留给应用人员应实现

        #region INaming 成员

        public abstract string FriendlyName { get; }

        #endregion

        #region 构造组件

        protected abstract Parser ConstructParser();
        protected virtual Sender ConstructSender()
        {
            return null;
        }
        protected virtual Receiver ConstructReceiver()
        {
            //默认使用基类Receiver,便于用户配置自定义的Receiver
            //modified by zhangyq.2010.3.16.
            return new Receiver();
            //return null;
        }
        protected virtual ProtocolDesc ConstructProtocolDesc()
        {
            return new ProtocolDesc();
        }

        #endregion 

        #endregion

        #region 私有方法

        //private Parser CreateParser()
        //{
        //    lock (this)
        //    {
        //        if (parser == null)
        //        {
        //            if (parserType != null)
        //            {
        //                try
        //                {
        //                    parser = ReflectionHelper.CreateInstance(parserType) as Parser;
        //                    if (parser == null)
        //                    {
        //                        throw new ProtocolException(string.Format("{0} 不是协议解析器！", parserType.GetType().FullName));
        //                    }

        //                    parser.Owner = this;
        //                    return parser;
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new ProtocolException("创建协议解析器异常:" + ex.Message + "\r\n协议:" + this.FriendlyName);
        //                }
        //            }
        //            else
        //            {
        //                //throw new ProtocolException("协议解析器未配置！协议:" + this.FriendlyName);
        //                return null;
        //            }
        //        }
        //        return parser;
        //    }
        //}

        //private Sender CreateSender()
        //{
        //    lock (this)
        //    {
        //        if (sender == null)
        //        {
        //            if (senderType != null)
        //            {
        //                try
        //                {
        //                    sender = ReflectionHelper.CreateInstance(senderType) as Sender;
        //                    sender.Owner = this;
        //                    return sender;
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new ProtocolException("创建协议主发器异常:" + ex.Message + "\r\n协议:" + this.FriendlyName);
        //                }
        //            }
        //            else
        //            {
        //                return null;
        //            }
        //        }

        //        return sender;
        //    }
        //}

        //private Receiver CreateReceiver()
        //{
        //    lock (this)
        //    {
        //        if (receiver == null)
        //        {
        //            if (receiverType != null)
        //            {
        //                try
        //                {
        //                    receiver = ReflectionHelper.CreateInstance(receiverType) as Receiver;
        //                    receiver.Owner = this;
        //                    return receiver;
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new ProtocolException("创建协议接收器异常:" + ex.Message + "\r\n协议:" + this.FriendlyName);
        //                }
        //            }
        //            else
        //            {
        //                return null;
        //            }
        //        }

        //        return receiver;
        //    }
        //}

        #endregion

        public override string ToString()
        {
            return FriendlyName;
        }

        #region ISupportPipe 成员

        protected Pipe pipe;
        public Pipe Pipe
        {
            get { return pipe; }
            set
            {
                ActionPipe(value);
                pipe = value;
            }
        }

        /// <summary>
        /// 当一个pipe 与该协议关联时，
        /// 该协议的相关参数可以从 pipe 的　property 里取出
        /// </summary>
        protected virtual void ActionPipe(Pipe pipe)
        {
            //若之前有关联的通道
            if (this.pipe != null)
            {
                if (pipe == null || !pipe.Equals(this.pipe))
                {
                    //之前通道与本协议脱离关系，将之前通道中有关协议的配置清除
                    this.pipe.ClearProperty(this.id);
                }
            }

            foreach (ProtocolComponent pc in components)
            {
                pc.Pipe = pipe;
            }
        }

        #endregion
    }
}