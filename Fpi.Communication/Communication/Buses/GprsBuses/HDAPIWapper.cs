using System;
using System.Runtime.InteropServices;

namespace Fpi.Communication.Buses.GprsBuses
{


    #region HongDian Gprs DLL structs

    [StructLayout(LayoutKind.Sequential)]
    internal struct GprsDataRecord
    {
        //
        //typedef struct GPRS_DATA_RECORD
        //{
        //    char       m_userid[12];		     //终端模块号码
        //    char       m_recv_date[20];		//接收到数据包的时间
        //    char       m_data_buf[MAX_RECEIVE_BUF]; //存储接收到的数据
        //    uint16     m_data_len;			//接收到的数据包长度
        //    uint8      m_data_type;	          //接收到的数据包类型,0x09：用户数据包 0 不认识类型
        //}data_record;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HDAPIWrapper.USER_ID_SIZE)]
        public byte[] userID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HDAPIWrapper.DATETIME_SIZE)]
        public byte[] recvTime;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = HDAPIWrapper.MAX_RECEIVE_BUF)]
        public byte[] dataBuf;
        public ushort dataLen;
        public byte dataType;
    }




    #endregion

    /// <summary>
    /// HDAPI 的摘要说明。
    /// </summary>
    /// 

    public class HDAPIWrapper
    {
        // Consts definition

        #region Gprs constants
        public const int DATA_PACKAGE_SIZE = 1060;
        public const int USER_INFO_SIZE = 68;
        public const int MESSAGE_SIZE = 512;
        public const int MAX_RECEIVE_BUF = 1024;
        public const int USER_ID_SIZE = 12;
        public const int DATETIME_SIZE = 20;
        #endregion

        #region communication mode
        /*0:阻塞模式, 可应用于无窗口环境下的编程,适用于支持线程的开发环境;
		 * 1:非阻塞模式, 可应用于无窗口环境下的编程,亦可适用于不支持线程的开发环境;
		 * 2:适用于窗口环境下的消息编程,兼容早期版本.必须在调用函数start_gprs_server之前调用*/
        public enum GprsMode : int
        {
            BlockMode = 0,
            NonBlockMode = 1,
            LegacyMode = 2
        }
        #endregion

        #region The GPRS Delegate Define
        internal delegate void LogonCALLBACKDelegate(IntPtr userInfo);
        #endregion

        #region HongDian Gprs DLL imports

        // Server start/stop API
        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "start_gprs_server_fpi", CharSet = CharSet.Auto)]
        internal static extern int start_gprs_server(IntPtr hDllModule, IntPtr hWnd, int wMsg, int ServerPort, IntPtr msg);

        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "stop_gprs_server_fpi", CharSet = CharSet.Auto)]
        internal static extern int stop_gprs_server(IntPtr hDllModule, IntPtr msg);


        // Data processing API
        //This method use the usedefine GprsDataRecord act as the data store 
        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "do_read_proc_fpi", CharSet = CharSet.Auto)]
        internal static extern int do_read_proc(IntPtr hDllModule, ref GprsDataRecord recvData, IntPtr msg, bool reply);

        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "do_send_user_data_fpi", CharSet = CharSet.Auto)]
        internal static extern int do_send_user_data(IntPtr hDllModule, byte[] user, byte[] data, int len, IntPtr msg);

        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "SetWorkMode_fpi", CharSet = CharSet.Auto)]
        internal static extern int SetWorkMode(IntPtr hDllModule, GprsMode mode);


        // User managerment API
        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "get_max_user_amount_fpi", CharSet = CharSet.Auto)]
        internal static extern int get_max_user_amount(IntPtr hDllModule);

        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "get_online_user_amount_fpi", CharSet = CharSet.Auto)]
        internal static extern int get_online_user_amount(IntPtr hDllModule);


        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "get_user_info_fpi", CharSet = CharSet.Auto)]
        internal static extern int get_user_info(IntPtr hDllModule, byte[] userid, IntPtr userInfo);

        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "get_user_at_fpi", CharSet = CharSet.Auto)]
        internal static extern int get_user_at(IntPtr hDllModule, int index, IntPtr userInfo);


        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "get_server_name_fpi", CharSet = CharSet.Auto)]
        internal static extern int get_server_name(IntPtr hDllModule, byte[] namebuf, int len, byte[] msg);


        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "do_close_one_user_fpi", CharSet = CharSet.Auto)]
        internal static extern int do_close_one_user(IntPtr hDllModule, byte[] userid, IntPtr msg);

        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "do_close_all_user_fpi", CharSet = CharSet.Auto)]
        internal static extern int do_close_all_user(IntPtr hDllModule, IntPtr msg);




        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "AddOneUser_fpi", CharSet = CharSet.Auto)]
        internal static extern int AddOneUser(IntPtr hDllModule, IntPtr pUserInfo);

        [DllImport("GprsPluginForCsharp.dll", EntryPoint = "SetLogonCALLBACK_fpi", CharSet = CharSet.Auto)]
        internal static extern int SetLogonCALLBACK(IntPtr hDllModule, LogonCALLBACKDelegate logonProc);


        #endregion
    }
}
