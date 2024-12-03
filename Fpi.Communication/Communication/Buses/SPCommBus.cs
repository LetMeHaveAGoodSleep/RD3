using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using Fpi.Communication.Exceptions;
using Fpi.Properties;

namespace Fpi.Communication.Buses
{
    /// <summary>
    /// 使用.net SerialPort的串口Bus
    /// add by zhangyq.2010.8.13.
    /// </summary>
    public class SPCommBus : BaseBus
    {
        public static readonly string PropertyName_Port = "port";
        public static readonly string PropertyName_Baud = "baud";
        public static readonly string PropertyName_DataBit = "dataBit";
        public static readonly string PropertyName_StopBit = "stopBit";
        public static readonly string PropertyName_Parity = "parity";

        private SerialPort sp = null;

        public override string FriendlyName
        {
            get
            {
                return "串口(RS232)_SP";
            }
        }

        public override string InstanceName
        {
            get
            {
                if (sp != null)
                {
                    return sp.PortName;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        #region IBus成员
        public override void Init(Fpi.Xml.BaseNode config)
        {
            if (config == null)
                throw new Exception(Resources.CommParamNotConfig);
            base.Init(config);

            sp = new SerialPort();
            sp.PortName = config.GetPropertyValue(PropertyName_Port);
            sp.BaudRate = Int32.Parse(config.GetPropertyValue(PropertyName_Baud, "57600"));
            sp.DataBits = Int32.Parse(config.GetPropertyValue(PropertyName_DataBit, "8"));
            string stopbits = config.GetPropertyValue(PropertyName_StopBit, "1");
            switch (stopbits)
            {
                case "1":
                    sp.StopBits = StopBits.One;
                    break;
                case "1.5":
                    sp.StopBits = StopBits.OnePointFive;
                    break;
                case "2":
                    sp.StopBits = StopBits.Two;
                    break;
                default:
                    sp.StopBits = StopBits.None;
                    break;
            }
            //sp.StopBits = (StopBits)stopbits;
            sp.Parity = (Parity)Int32.Parse(config.GetPropertyValue(PropertyName_Parity, "0"));
            sp.ReadTimeout = SerialPort.InfiniteTimeout;
            sp.WriteTimeout = SerialPort.InfiniteTimeout;
            sp.ReadBufferSize = 1024 * 10;
            sp.WriteBufferSize = 1024 * 10;
        }

        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
            bytesread = sp.Read(buf, 0, count);
            return true;
        }

        public override bool Write(byte[] buf)
        {
            sp.Write(buf, 0, buf.Length);
            return true;
        }
        #endregion

        #region IConnector成员
        public override bool Open()
        {
            if (!sp.IsOpen)
            {
                sp.Open();
            }
            return true;
        }

        public override bool Close()
        {
            sp.Close();
            return true;
        }
        #endregion
    }
}
