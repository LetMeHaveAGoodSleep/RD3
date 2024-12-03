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
    /// Զ��ͨѶЭ�����������������������������������Ĭ�����ã����н������Ǻ��ģ���������������
    /// </summary>
    public abstract class Protocol : IdNameNode, INaming,  ISupportPipe
    {
        #region ����ID

        //public static readonly string PropertyName_Protocol = Pipe.PropertyName_Protocol;

        ///// <summary>�����Ŀ�б����������</summary>
        public static readonly string PropertyName_OutputList = "OutputList";
        public static readonly string PropertyName_SenderInterval = "SenderInterval";
        public static readonly string PropertyName_SelectMp = "SelectMp";

        #endregion

        #region Э�鹹��

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

        #region ����

        protected Parser parser;
        protected Sender sender;
        protected Receiver receiver;
        protected ProtocolDesc protocolDesc;

        /// <summary>
        /// ������
        /// </summary>
        public Parser Parser
        {
            get { return parser; }
        }

        /// <summary>
        /// ������
        /// </summary>
        public Sender Sender
        {
            get { return sender; }
        }

        /// <summary>
        /// ������
        /// </summary>
        public Receiver Receiver
        {
            get { return receiver; }
        }

        /// <summary>
        /// ͨѶЭ�����������
        /// </summary>
        public ProtocolDesc ProtocolDesc
        {
            get { return protocolDesc; }
        }

        #endregion

        #region Ԥ����Ӧ����ԱӦʵ��

        #region INaming ��Ա

        public abstract string FriendlyName { get; }

        #endregion

        #region �������

        protected abstract Parser ConstructParser();
        protected virtual Sender ConstructSender()
        {
            return null;
        }
        protected virtual Receiver ConstructReceiver()
        {
            //Ĭ��ʹ�û���Receiver,�����û������Զ����Receiver
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

        #region ˽�з���

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
        //                        throw new ProtocolException(string.Format("{0} ����Э���������", parserType.GetType().FullName));
        //                    }

        //                    parser.Owner = this;
        //                    return parser;
        //                }
        //                catch (Exception ex)
        //                {
        //                    throw new ProtocolException("����Э��������쳣:" + ex.Message + "\r\nЭ��:" + this.FriendlyName);
        //                }
        //            }
        //            else
        //            {
        //                //throw new ProtocolException("Э�������δ���ã�Э��:" + this.FriendlyName);
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
        //                    throw new ProtocolException("����Э���������쳣:" + ex.Message + "\r\nЭ��:" + this.FriendlyName);
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
        //                    throw new ProtocolException("����Э��������쳣:" + ex.Message + "\r\nЭ��:" + this.FriendlyName);
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

        #region ISupportPipe ��Ա

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
        /// ��һ��pipe ���Э�����ʱ��
        /// ��Э�����ز������Դ� pipe �ġ�property ��ȡ��
        /// </summary>
        protected virtual void ActionPipe(Pipe pipe)
        {
            //��֮ǰ�й�����ͨ��
            if (this.pipe != null)
            {
                if (pipe == null || !pipe.Equals(this.pipe))
                {
                    //֮ǰͨ���뱾Э�������ϵ����֮ǰͨ�����й�Э����������
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