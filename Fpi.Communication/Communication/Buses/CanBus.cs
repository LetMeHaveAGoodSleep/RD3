#if! WINCE
using System;
using Fpi.Xml;
using Fpi.Communication.Exceptions;
using Fpi.Properties;

namespace Fpi.Communication.Buses
{
    public class CanBus : BaseBus
    {
        public static readonly string PropertyName_Port = "port";  

        private int port = -1;

        public override string FriendlyName
        {
            get { return "CAN 总线"; }
        }

        #region IBus 成员

        public override void Init(BaseNode config)
        {
            if (config == null)
                throw new CommunicationParamException(Resources.CANNotConfig);

            base.Init(config);
            port = Int32.Parse(config.GetPropertyValue(PropertyName_Port));
        }

        public override bool Write(byte[] buf)
        {
            return PhysicalCanBus.GetInstance().Write(port, buf);
        }

        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
            return PhysicalCanBus.GetInstance().Read(port, buf, count, ref bytesread);
        }

        #endregion

        #region IConnector 成员

        public override bool Open()
        {
            string linkInfo = port.ToString();
            bool flag = _Open();
            if (flag)
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.OpenCanSucceed,linkInfo));
            }
            else
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.OpenCanFailed, linkInfo));
            }

            connected = flag;

            return flag;
        }
        private bool _Open()
        {
            return PhysicalCanBus.GetInstance().Open(port);
        }

        public override bool Close()
        {
            string linkInfo = port.ToString();
            bool flag = _Close();
            if (flag)
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.CloseCanSucceed, linkInfo));
            }
            else
            {
                BusLogHelper.TraceBusMsg(string.Format(Resources.CloseCanFailed, linkInfo));
            }

            connected = !flag;

            return flag;
        }
        private bool _Close()
        {
            return PhysicalCanBus.GetInstance().Close(port);
        }

        #endregion
    }
}

#endif