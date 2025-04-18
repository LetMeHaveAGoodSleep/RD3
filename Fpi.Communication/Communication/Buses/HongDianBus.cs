using System;
using Fpi.Xml;
using Fpi.Communication.Manager;
using Fpi.Communication.Exceptions;
using Fpi.Properties;


namespace Fpi.Communication.Buses
{
    public class HongDianBus : BaseBus
    {
        public static readonly string PropertyName_ChannelIndex = "channelIndex";

        public HongDianBus()
        {
        }

        protected PhysicalHongDianBus bus = null;
        protected int index = -1;

        #region 属性

        public string Port
        {
            get
            {
                if (bus.CommBus != null)
                {
                    return bus.CommBus.Port;
                }
                throw new Exception(Resources.HongDianCommNotConfig);
            }
        }

        public int Baud
        {
            get
            {
                if (bus.CommBus != null)
                {
                    return bus.CommBus.Buad;
                }
                throw new Exception(Resources.HongDianCommNotConfig);
            }
        }

        public int Index
        {
            get { return index; }
        }

        public override string FriendlyName
        {
            get { return "宏电多通道 GPRS"; }
        }
        public override string InstanceName
        {
            get
            {
                if (bus.CommBus != null)
                {
                    return bus.CommBus.Port;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        #endregion

        #region IBus 成员

        public override void Init(BaseNode config)
        {
            if (config == null)
                throw new CommunicationParamException(Resources.HongDianGPRSNotConfig);

            base.Init(config);
            bus = PhysicalHongDianBus.GetInstance();
            bus.Init(config);

            index = Int32.Parse(config.GetPropertyValue(PropertyName_ChannelIndex));
        }

        public override bool Write(byte[] buf)
        {
            lock (bus)
            {
                return bus.Write(index, buf);
            }
        }

        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
            return bus.Read(index, buf, count, ref bytesread);
        }

        #endregion

        #region IConnector 成员

        private bool isOpen;
        private bool isClose;

        public override bool Open()
        {
            if (!isOpen)
            {
                isOpen = bus.Open(index);
            }
            connected = isOpen;
            return isOpen;
        }

        public override bool Close()
        {
            if (!isClose)
            {
                isClose = bus.Close(index);
            }
            connected = !isClose;
            return isClose;
        }

        #endregion
    }
}