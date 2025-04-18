using System;
using System.Runtime.InteropServices;
using Fpi.Communication.Manager;
using Fpi.Util.Sundry;
using System.Threading;
using Fpi.Util.WinApiUtil;
using Fpi.Properties;


namespace Fpi.Communication.Buses
{
    public class UsbBus : BaseBus
    {
        private IntPtr hBus = IntPtr.Zero;

        public UsbBus() : base()
        {
        }

#if WINCE
		private IntPtr hAppAttachEvent;
		private IntPtr hAppRemoveEvent;
		private const string USB_BUS_NAME = "LPT1:";		

        #region IBus 成员

        override public void Init(Bus config)
        {
            base.Init(config);
            hAppAttachEvent = WinApiWrapper.CreateEvent(false, false, "FPI_USB_ATTACH_EVENT");
            hAppRemoveEvent = WinApiWrapper.CreateEvent(false, false, "FPI_USB_REMOVE_EVENT");
            (new Thread(new ThreadStart(UsbAttachProcessFunc))).Start();
            (new Thread(new ThreadStart(UsbRemoveProcessFunc))).Start();
        }

        public override bool Write(byte[] buf)
        {
            return WinApiWrapper.WriteFile(hBus, buf);
        }

        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
            WinApiWrapper.ReadFile(hBus, buf, count, ref bytesread, IntPtr.Zero);
            return (bytesread > 0);
        }
        #endregion

        #region IConnector 成员

        public override bool Open()
        {
            bool result = true;
            if (hBus == IntPtr.Zero)
            {
                hBus = WinApiWrapper.CreateFile(USB_BUS_NAME);

                if (hBus == (IntPtr)WinApiWrapper.INVALID_HANDLE_VALUE)
                {
                    result = false;
                }
            }

            connected = result;
            return result;
        }

        public override bool Close()
        {
            if (hBus != IntPtr.Zero)
            {
                WinApiWrapper.CloseHandle(hBus);
                hBus = IntPtr.Zero;
            }
            connected = false;
            return true;
        }

        private void UsbAttachProcessFunc()
        {
            while (true)
            {
#if TEST
                Fpi.Log.LogHelper.Debug("UsbAttachProcessFunc()");
#endif
                WinApiWrapper.WaitForSingleObject(hAppAttachEvent, uint.MaxValue);
                Close();
                Open();
            }
        }

        private void UsbRemoveProcessFunc()
        {
            while (true)
            {
#if TEST
                Fpi.Log.LogHelper.Debug("UsbRemoveProcessFunc()");
#endif
                WinApiWrapper.WaitForSingleObject(hAppRemoveEvent, uint.MaxValue);
                Close();
            }
        }

        #endregion


#else

        #region DllImport

        [DllImport("UsbControl.dll", EntryPoint="F32x_GetNumDevices", SetLastError=true)]
        internal static extern int F32x_GetNumDevices(ref int dwNumDevices);

        [DllImport("UsbControl.dll", EntryPoint="F32x_Open", SetLastError=true)]
        internal static extern IntPtr F32x_Open(int dwDevice);

        [DllImport("UsbControl.dll", EntryPoint="F32x_Close", SetLastError=true)]
        internal static extern int F32x_Close(IntPtr cyHandle);

        [DllImport("UsbControl.dll", EntryPoint="OpenUSBWRfile", SetLastError=true)]
        internal static extern IntPtr OpenUSBWRfile(char[] sPipeName);

        #endregion

        internal const Int32 F32x_SUCCESS = 0x00;

        private IntPtr hRead;
        private IntPtr hWrite;

        public override string FriendlyName
        {
            get { return "USB 通信"; }
        }

        #region IBus 成员

        public override void Init(Fpi.Xml.BaseNode config)
        {
        }

        public override bool Write(byte[] buf)
        {
            return WinApiWrapper.WriteFile(hWrite, buf);
        }

        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
#if READTEST
			return protocol.ReadTest(buf, count, ref bytesread);
#else
            WinApiWrapper.ReadFile(hRead, buf, buf.Length, ref bytesread, IntPtr.Zero);
            return (bytesread > 0);
#endif
        }

        #endregion

        #region IConnector 成员

        public override bool Open()
        {
            bool flag = _Open();
            if (flag)
            {
                BusLogHelper.TraceBusMsg(Resources.OpenUsbSucceed);
            }
            else
            {
                BusLogHelper.TraceBusMsg(Resources.OpenUsbFailed);
            }
            connected = flag;


            return flag;
        }

        private bool _Open()
        {
            int number = 0;
            if (F32x_SUCCESS != F32x_GetNumDevices(ref number) || (number == 0))
            {
                return false;
            }

            hBus = F32x_Open(0);
            hWrite = OpenUSBWRfile("PIPE01".ToCharArray());
            hRead = OpenUSBWRfile("PIPE00".ToCharArray());
            return (hBus != IntPtr.Zero && hWrite != IntPtr.Zero && hRead != IntPtr.Zero);
        }

        public override bool Close()
        {
            bool flag = _Close();
            if (flag)
            {
                BusLogHelper.TraceBusMsg(Resources.CloseUsbSucceed);
            }
            else
            {
                BusLogHelper.TraceBusMsg(Resources.CloseUsbFailed);
            }

            connected = !flag;
            return flag;
        }

        private bool _Close()
        {
            return true;
        }

        #endregion

#endif


    }
}