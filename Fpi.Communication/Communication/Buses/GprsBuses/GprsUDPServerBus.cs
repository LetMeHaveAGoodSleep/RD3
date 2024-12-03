using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using Fpi.Communication.Buses.GprsBuses;
using Fpi.Communication.Manager;
using Fpi.Util;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Fpi.Communication.Exceptions;
using Fpi.Xml;
using Fpi.Util.WinApiUtil;

//by zf
namespace Fpi.Communication.Buses
{
    public class GprsUDPServerBus : BaseBus 
        //, IDisposable
    {

        private const string PropertyName_Port = "port";
        private const int WAIT_TIME = 2000;

        private int port;                   //communication port
        private byte[] dtuId;               //DTU MobuleNO  DTU:HDGprs

        private IntPtr lib;                 //Gprs_port file IntPtr
        private IntPtr msgPtr;              //msgPtr IntPtr

        //HDAPI parameter 
        private GprsDataRecord record = new GprsDataRecord();

        public GprsUDPServerBus()
		{
		}

        public override string FriendlyName
        {
            get { return "GPRS UDP 服务端"; }
        }

        /// <summary>
        /// 
        /// <paramater>DllFileNmae:GprsDll</paramater>
        /// </summary>
        public override void Init(BaseNode config)
        {
            base.Init(config);

            //Get Port
            port = int.Parse(config.GetPropertyValue(PropertyName_Port));

            string gprsDllFolder = System.Windows.Forms.Application.StartupPath;
            //source GprsDll file
            string dllFilePathName = gprsDllFolder + @"\GprsDll\gprs_dll.dll";
            //GprsPort file name and address
            string dllNewFilePathName = gprsDllFolder + @"\GprsDll\gprs_dll_" + port.ToString() + ".dll";

            //Gprs Dll file copy 
            if (!System.IO.File.Exists(dllFilePathName))
            {
                throw new System.IO.FileNotFoundException("Gprs dll not Found", dllFilePathName);
            }
            else if (!System.IO.File.Exists(dllNewFilePathName))
            {
                System.IO.File.Copy(dllFilePathName, dllNewFilePathName);    
            }
            lib = WinApiWrapper.LoadLibrary(dllNewFilePathName);
        }

        #region BaseBus抽象类
        //open GPRS port
        public override bool Open()
        {
            if (connected)
            {
                return false;
            }

            //HD mode set:Before we call start_gprs_server, first set the work mode to non block mode.
            HDAPIWrapper.SetWorkMode(lib, HDAPIWrapper.GprsMode.NonBlockMode);

            msgPtr = Marshal.AllocHGlobal(HDAPIWrapper.MESSAGE_SIZE);
            // When working in non block mode, hWnd and hMsg becomes meaningless.
            int ret = HDAPIWrapper.start_gprs_server(lib, msgPtr, 0, port, msgPtr);
            // when start failed
            if (ret != 0)
            {
                Fpi.Util.LogHelper.Error("Start DTU server failed!");
                return false;
            }
            connected = true;


            Fpi.Util.LogHelper.Info("服务器GPRS端口:" + port + ",正在建立与Gprs模块的通讯......,请稍候！");

            //thread of get DTU model information
            Thread getDTUInfoThread = new Thread(new ThreadStart(ProcessDTUInfo));
            getDTUInfoThread.Name = "Gprs_" + port.ToString() + "_GetUser";
            getDTUInfoThread.IsBackground = true;
            getDTUInfoThread.Start();

            //link state judge thread
            Thread linkStateThread = new Thread(new ThreadStart(JudgeLinkState));
            linkStateThread.Name = "Gprs_" + port.ToString() + "_JudgeState";
            linkStateThread.IsBackground = true;

            linkStateThread.Start();
            return true;
        }

        public override bool Write(byte[] buf)
        {
            if (!connected)
            {
                throw new CommunicationException("bus is closed");
            }
            if (dtuId != null)
            {
                return HDAPIWrapper.do_send_user_data(lib, dtuId, buf, buf.Length, msgPtr) == 0;

            }
            return false;
        }

        public override bool Read(byte[] buf, int count, ref int bytesread)
        {
            if (!connected)
            {
                throw new CommunicationException("bus is closed");
            }

            int ret = HDAPIWrapper.do_read_proc(lib, ref record, msgPtr, false);

            if (ret == 0 && record.dataLen > 0)
            {
                Buffer.BlockCopy(record.dataBuf, 0, buf, 0, record.dataLen);
                bytesread = record.dataLen;
                return true;
            }
            else
            {
                Thread.Sleep(WAIT_TIME);
                bytesread = 0;
                return false;
            }
        }

        public override bool Close()
        {
            //LGA007 客户端软件开发时修改
            //if (!connected)
            //{
            //    return false;
            //}

            connected = false;

            int ret = HDAPIWrapper.stop_gprs_server(lib, msgPtr);

            //LGA007 没有开启时返回-1
            if (ret != 0 && ret != -1 )
            {
                BusLogHelper.TraceBusMsg("关闭Gprs端口失败:" + port);
                return false;
            }

            if (msgPtr != null)
            {
                Marshal.FreeHGlobal(msgPtr);
            }

            BusLogHelper.TraceBusMsg("关闭Gprs端口成功:" + port);
            return true;
        }
        #endregion

        #region Link Judge thread
        private void JudgeLinkState()
        {
            while (connected)
            {
                for (int i = 0; i < 30; i++)
                {
                    if (!IsTimeOut(0))
                    {
                        LogHelper.Info("服务器端口:" + port + "与GPRS模块已建立连接......,可以连接仪器了");
                        return;
                    }
                    Thread.Sleep(WAIT_TIME);
                    if (!connected)
                    {
                        //for close net work;
                        return;
                    }
                }
                //run 20sec
                LogHelper.Info("服务器端口:" + port + "与GPRS模块连接失败");
                Close();
            }
        }

        //(first time) Link Success : return false
        private bool IsTimeOut(int timeoutSecond)
        {
            //get DTU number
            int count = HDAPIWrapper.get_online_user_amount(lib);

            if (count != 1)
            {
                return true;
            }
            else
            {
                byte[] data = GetDTUInfo(0);
                if (data == null)
                {
                    return true;
                }

                double addSeconed = 0;
                for (int i = 46; i < 50; i++)
                {
                    addSeconed = addSeconed + ((int)data[i]) * Math.Pow(2, (i - 46) * 8);
                }

                int realTimeoutSecond;

                if (timeoutSecond < 1)
                {
                    realTimeoutSecond = 45;
                }
                else
                {
                    realTimeoutSecond = timeoutSecond;
                }

                System.TimeSpan timeoutSpan = System.DateTime.UtcNow - (new DateTime(1970, 1, 1));
                int difSecond = (int)(timeoutSpan.TotalSeconds - addSeconed);
                return (difSecond > realTimeoutSecond);
            } 
        }
        #endregion 

        #region ProcessDTUInfo thread
        private void ProcessDTUInfo()
        {
            while (connected)
            {
                //get DTU number
                int count = HDAPIWrapper.get_online_user_amount(lib);

                if (count == 1)
                {
                    dtuId = GetDTUInfo(0);
                }
                else
                {
                    dtuId = null;
                }
                Thread.Sleep(10000);
            }
        }

        private byte[] GetDTUInfo(int index)
        {
            IntPtr userPtr = Marshal.AllocHGlobal(HDAPIWrapper.USER_INFO_SIZE);
            byte[] result = null;
            try
            {
                int ret = HDAPIWrapper.get_user_at(lib, index, userPtr);

                if (ret != 0)
                {
                    LogHelper.Error("Get DTU information failed");
                }
                else
                {
                    result = new byte[HDAPIWrapper.USER_INFO_SIZE];
                    Marshal.Copy(userPtr, result, 0, HDAPIWrapper.USER_INFO_SIZE);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(userPtr);
            }

            return result;
        }
        #endregion 
    }
}
