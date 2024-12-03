using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Fpi.Communication.Exceptions;
using Fpi.Communication.Manager;
using Fpi.Util.Sundry;
using Fpi.Util.WinApiUtil.CommDataType;
using Fpi.Xml;
using Microsoft.Win32;
#if !WINCE
using Fpi.Util.WinApiUtil;
#else
using Fpi.UI.Common.CE.Configure;
#endif

namespace Fpi.Communication.Buses
{
    /// <summary>
    /// CommBus 的摘要说明。
    /// </summary>
    public class CommBus : BaseBus
    {
        public static readonly string PropertyName_Port = "port";
        public static readonly string PropertyName_Baud = "baud";
        public static readonly string PropertyName_DataBit = "dataBit";
        public static readonly string PropertyName_StopBit = "stopBit";
        public static readonly string PropertyName_Parity = "parity";
        public static readonly string PropertyName_SendInterTime = "sendInterTime";

        protected IntPtr hPort = IntPtr.Zero;
        private IntPtr closeEvent;
        private string closeEventName = "CloseEvent";
        private DCB dcb = new DCB();
        private DetailedPortSettings portSettings;

        // default Rx buffer is 1024 bytes
        private int rxBufferSize = 1024*10;
        // default Tx buffer is 1024 bytes
        private int txBufferSize = 1024*10;

        private IntPtr txOverlapped = IntPtr.Zero;
        private IntPtr rxOverlapped = IntPtr.Zero;

        //端口错误处理事件
        public delegate void PortErrorEvent(string Description);

        public event PortErrorEvent OnError;

        private CommEventFlags eventFlags;
        private AutoResetEvent rxEvent;

        private bool needWaitRxCharEvent = true;


        #region 属性

        protected string _Port;
        protected int _Baud;

        public string Port
        {
            get { return _Port; }
        }

        public int Buad
        {
            get { return _Baud; }
        }

        public override string FriendlyName
        {
            get { return "串口(RS232)"; }
        }
        public override string InstanceName
        {
            get
            {
                return _Port;
            }
        }
        #endregion

        #region IBus 成员

        public override void Init(BaseNode config)
        {
            if (config == null)
                throw new CommunicationParamException("串口参数未配置。");

            base.Init(config);

            closeEvent = WinApiWrapper.CreateEvent(true, false, closeEventName);

            int baud = Int32.Parse(config.GetPropertyValue(PropertyName_Baud, "57600"));
            int dataBit = Int32.Parse(config.GetPropertyValue(PropertyName_DataBit, "8"));
            string stopbits = config.GetPropertyValue(PropertyName_StopBit, "1");
            int parity = Int32.Parse(config.GetPropertyValue(PropertyName_Parity, "0"));

            portSettings = new HandshakeNone();
            portSettings.BasicSettings.BaudRate = (BaudRates) baud;
            portSettings.BasicSettings.ByteSize = (byte) dataBit;
            switch (stopbits)
            {
                case "1":
                    portSettings.BasicSettings.StopBits = StopBits.one;
                    break;
                case "1.5":
                    portSettings.BasicSettings.StopBits = StopBits.onePointFive;
                    break;
                case "2":
                    portSettings.BasicSettings.StopBits = StopBits.two;
                    break;
            }

            //portSettings.BasicSettings.StopBits = (StopBits) stopBit;
            portSettings.BasicSettings.Parity = (Parity) parity;
            //Add By DRH 2008.06.16

            _Baud = baud;
            _Port = config.GetPropertyValue(PropertyName_Port);
        }

        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
            if ((!needWaitRxCharEvent) || (needWaitRxCharEvent && WaitRxCharEvent()))
            {
                // make sure the port handle is valid
                if (hPort == (IntPtr) WinApiWrapper.INVALID_HANDLE_VALUE)
                {
                    bytesread = 0;
                    return true;
                }

                // data came in, put it in the buffer and set the event

                if (!WinApiWrapper.ReadFile(hPort, buf, count, ref bytesread, rxOverlapped))
                {
                    string error = String.Format("ReadFile Failed: {0}", Marshal.GetLastWin32Error());
                    if (OnError != null)
                    {
                        OnError(error);
                    }
                    throw new CommunicationException(error);
                }

                needWaitRxCharEvent = (bytesread <= 0);

                return true;
            }
            bytesread = 0;
            return false;
        }

        public override bool Write(byte[] buf)
        {
            int intertime = Int32.Parse(config.GetPropertyValue(PropertyName_SendInterTime, "0"));
            if (intertime > 0)
            {
                Thread.Sleep(intertime);
            }
            if (WinApiWrapper.WriteFile(hPort, buf, txOverlapped))
            {
                return true;
            }
            if (Marshal.GetLastWin32Error() == (int) APIErrors.ERROR_IO_PENDING)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region IConnector 成员

        public override bool Open()
        {
            bool flag = false;
            int waitTime = 100;

            for (int i = 1; i <= 3; i++)
            {
                flag = _Open();
                if (flag)
                {
                    break;
                }

                Thread.Sleep(waitTime);
                waitTime *= 2;
            }

            string info = flag ? "打开串口成功:" : "打开串口失败:";
            info += _Port;
            BusLogHelper.TraceBusMsg(info);

            connected = flag;
            return flag;
        }

        private bool _Open()
        {
#if !WINCE
            OVERLAPPED o = new OVERLAPPED();
            txOverlapped = WinApiWrapper.LocalAlloc(0x40, Marshal.SizeOf(o));
            o.Offset = 0;
            o.OffsetHigh = 0;
            o.hEvent = IntPtr.Zero;
            Marshal.StructureToPtr(o, txOverlapped, true);
#endif

            hPort = WinApiWrapper.CreateFile(GetPortName());

            if (hPort == (IntPtr) WinApiWrapper.INVALID_HANDLE_VALUE)
            {
                return false;
            }

            // set queue sizes
            WinApiWrapper.SetupComm(hPort, rxBufferSize, txBufferSize);
            TransferPortSettings2DCB();
            WinApiWrapper.SetCommState(hPort, dcb);

            // set the Comm timeouts
            CommTimeouts ct = new CommTimeouts();

            // reading we'll return immediately
            // this doesn't seem to work as documented
            ct.ReadIntervalTimeout = uint.MaxValue; // this = 0xffffffff
            ct.ReadTotalTimeoutConstant = 0;
            ct.ReadTotalTimeoutMultiplier = 0;

            // writing we'll give 5 seconds
            ct.WriteTotalTimeoutConstant = 5000;
            ct.WriteTotalTimeoutMultiplier = 0;

            WinApiWrapper.SetCommTimeouts(hPort, ct);

            eventFlags = new CommEventFlags();
            rxEvent = new AutoResetEvent(false);

#if WINCE
			WinApiWrapper.SetCommMask(hPort, CommEventFlags.ALLCE);
#else
            WinApiWrapper.SetCommMask(hPort, CommEventFlags.ALLPC);
            // set up the overlapped tx IO
            // AutoResetEvent are = new AutoResetEvent(false);
            o = new OVERLAPPED();
            rxOverlapped = WinApiWrapper.LocalAlloc(0x40, Marshal.SizeOf(o));
            o.Offset = 0;
            o.OffsetHigh = 0;
            o.hEvent = rxEvent.Handle;
            Marshal.StructureToPtr(o, rxOverlapped, true);
#endif
            return true;
        }

        public override bool Close()
        {
            bool flag = _Close();
            if (flag)
            {
                BusLogHelper.TraceBusMsg("关闭串口成功:" + _Port);
            }
            else
            {
                BusLogHelper.TraceBusMsg("关闭串口失败:" + _Port);
            }

            connected = !flag;
            return flag;
        }

        private bool _Close()
        {
#if !WINCE
            if (txOverlapped != IntPtr.Zero)
            {
                WinApiWrapper.LocalFree(txOverlapped);
                txOverlapped = IntPtr.Zero;
            }
#endif
            if (WinApiWrapper.CloseHandle(hPort))
            {
                WinApiWrapper.SetEvent(closeEvent);
                hPort = (IntPtr) WinApiWrapper.INVALID_HANDLE_VALUE;
                WinApiWrapper.SetEvent(closeEvent);
                return true;
            }

            return false;
        }

        #endregion

        #region Private Method

        private string GetPortName()
        {
            string port = _Port;

            int commNum = int.Parse(port.Substring(3));

            if (commNum >= 10 && commNum <= 255)
            {
                return @"\\.\" + port;
            }
            else
            {
                return port + ":";
            }
        }

        // transfer the port settings to a DCB structure
        private void TransferPortSettings2DCB()
        {
            dcb.BaudRate = (uint) portSettings.BasicSettings.BaudRate;
            dcb.ByteSize = portSettings.BasicSettings.ByteSize;
            dcb.EofChar = (sbyte) portSettings.EOFChar;
            dcb.ErrorChar = (sbyte) portSettings.ErrorChar;
            dcb.EvtChar = (sbyte) portSettings.EVTChar;
            dcb.fAbortOnError = portSettings.AbortOnError;
            dcb.fBinary = true;
            dcb.fDsrSensitivity = portSettings.DSRSensitive;
            dcb.fDtrControl = (DCB.DtrControlFlags) portSettings.DTRControl;
            dcb.fErrorChar = portSettings.ReplaceErrorChar;
            dcb.fInX = portSettings.InX;
            dcb.fNull = portSettings.DiscardNulls;
            dcb.fOutX = portSettings.OutX;
            dcb.fOutxCtsFlow = portSettings.OutCTS;
            dcb.fOutxDsrFlow = portSettings.OutDSR;
            dcb.fParity = (portSettings.BasicSettings.Parity == Parity.None) ? false : true;
            dcb.fRtsControl = (DCB.RtsControlFlags) portSettings.RTSControl;
            dcb.fTXContinueOnXoff = portSettings.TxContinueOnXOff;
            dcb.Parity = (byte) portSettings.BasicSettings.Parity;
            dcb.StopBits = (byte) portSettings.BasicSettings.StopBits;
            dcb.XoffChar = (sbyte) portSettings.XoffChar;
            dcb.XonChar = (sbyte) portSettings.XonChar;
            dcb.XonLim = dcb.XoffLim = (ushort) (rxBufferSize/10);
        }

        private bool WaitRxCharEvent()
        {
            // wait for a Comm event
            if (!WinApiWrapper.WaitCommEvent(hPort, ref eventFlags))
            {
                int e = Marshal.GetLastWin32Error();

                if (e == (int) APIErrors.ERROR_IO_PENDING)
                {
                    // IO pending so just wait and try again
                    rxEvent.WaitOne();
                    Thread.Sleep(0);
                    return false;
                }

                if (e == (int) APIErrors.ERROR_INVALID_HANDLE)
                {
                    // Calling Port.Close() causes hPort to become invalid
                    // Since Thread.Abort() is unsupported in the CF, we must
                    // accept that calling Close will throw an error here.

                    // Close signals the closeEvent, so wait on it
                    // We wait 1 second, though Close should happen much sooner
                    int eventResult = WinApiWrapper.WaitForSingleObject(closeEvent, 1000);

                    if (eventResult == (int) APIConstants.WAIT_OBJECT_0)
                    {
                        // the event was set so close was called
                        hPort = (IntPtr) WinApiWrapper.INVALID_HANDLE_VALUE;
                        throw new CommunicationException("port closed");
                    }
                }

                // WaitCommEvent failed
                // 995 means an exit was requested (thread killed)
                if (e == 995)
                {
                    //throw new System.Threading.ThreadInterruptedException("thread killed");
                }
                else
                {
                    string error = String.Format("Wait Failed: {0}", e);
                    throw new CommunicationException(error);
                }
            }

            // Re-specify the set of events to be monitored for the port.
#if WINCE
			WinApiWrapper.SetCommMask(hPort, CommEventFlags.ALLCE);
#else
            WinApiWrapper.SetCommMask(hPort, CommEventFlags.ALLPC);
#endif

            // check the event for errors

            #region >>>> error checking <<<<

            if (((uint) eventFlags & (uint) CommEventFlags.ERR) != 0)
            {
                CommErrorFlags errorFlags = new CommErrorFlags();
                CommStat commStat = new CommStat();

                // get the error status
                if (!WinApiWrapper.ClearCommError(hPort, ref errorFlags, commStat))
                {
                    // ClearCommError failed!
                    string error = String.Format("ClearCommError Failed: {0}", Marshal.GetLastWin32Error());
                    throw new CommunicationException(error);
                }

                if (((uint) errorFlags & (uint) CommErrorFlags.BREAK) != 0)
                {
                    // BREAK can set an error, so make sure the BREAK bit is set an continue
                    eventFlags |= CommEventFlags.BREAK;
                }
                else
                {
                    // we have an error. Build a meaningful string and throw an exception
                    StringBuilder s = new StringBuilder("UART Error: ", 80);
                    if ((errorFlags & CommErrorFlags.FRAME) != 0)
                    {
                        s = s.Append("Framing,");
                    }
                    if ((errorFlags & CommErrorFlags.IOE) != 0)
                    {
                        s = s.Append("IO,");
                    }
                    if ((errorFlags & CommErrorFlags.OVERRUN) != 0)
                    {
                        s = s.Append("Overrun,");
                    }
                    if ((errorFlags & CommErrorFlags.RXOVER) != 0)
                    {
                        s = s.Append("Receive Overflow,");
                    }
                    if ((errorFlags & CommErrorFlags.RXPARITY) != 0)
                    {
                        s = s.Append("Parity,");
                    }
                    if ((errorFlags & CommErrorFlags.TXFULL) != 0)
                    {
                        s = s.Append("Transmit Overflow,");
                    }

                    // no known bits are set
                    if (s.Length == 12)
                    {
                        s = s.Append("Unknown");
                    }

                    // raise an error event
                    if (OnError != null)
                        OnError(s.ToString());

                    return false;
                }
            } // if(((uint)eventFlags & (uint)CommEventFlags.ERR) != 0)

            #endregion

            return ((uint) eventFlags & (uint) CommEventFlags.RXCHAR) != 0;
        }

        #endregion

        /// <summary>不支持 CF 1.1 及以下版本</summary>
        public static string[] GetCommArray()
        {
#if !WINCE
            try
            {
                string strComm = @"Hardware\DeviceMap\SerialComm";
                RegistryKey key = Registry.LocalMachine.OpenSubKey(strComm);

                string[] commNames = key.GetValueNames();

                string[] commValues = new string[commNames.Length];

                for (int i = 0; i < commNames.Length; i++)
                {
                    commValues[i] = (string) key.GetValue(commNames[i]);
                }
                return commValues;
            }
            catch (Exception)
            {
                return new string[0];
            }
#else
			return new string[0];
#endif
        }

        public static string[] GetBaudArray()
        {
            string[] baud = new string[] {"1200","2400", "4800", "9600", "19200", "38400", "57600", "115200"};
            return baud;
        }
    }
}