using Fpi.Util.Sundry;
using Fpi.Properties;
using Fpi.Util;
namespace Fpi.Communication.Buses
{
    public class BusLogHelper
    {
        private BusLogHelper()
        {
        }

        private const string MsgType = "BusMessage";

        public static void TraceBusMsg(string msg)
        {
            try
            {
                Fpi.Util.LogHelper.Debug(msg);
            }
            catch
            {
            }          
        }

        public static void TraceBusSendMsg(byte[] sendData)
        {
            string strBytes = StringUtil.BytesToString(sendData);
            TraceBusSendMsg(strBytes);
        }

        public static void TraceBusRecvMsg(byte[] recvData)
        {
            string strBytes = StringUtil.BytesToString(recvData);
            TraceBusRecvMsg(strBytes);
        }

        public static void TraceBusSendMsg(string sendData)
        {
            try
            {
                LogHelper.Debug(Resources.Send + ":" + sendData);
            }
            catch
            {
            }           
        }

        public static void TraceBusRecvMsg(string recvData)
        {
            try
            {
                LogHelper.Debug(Resources.Recv + ":" + recvData);
            }
            catch
            {
            }
        }

        public static void TraceBusSendMsg(object source, string sendData)
        {
            try
            {
                LogHelper.Debug("[" + source.ToString() + "]" + Resources.Send + ":" + sendData);
            }
            catch
            {
            }
        }

        public static void TraceBusRecvMsg(object source, string recvData)
        {
            try
            {
                LogHelper.Debug("[" + source.ToString() + "]" + Resources.Recv + ":" + recvData);
            }
            catch
            {
            }
        }

        public static void TraceBusSendMsg(object source, byte[] sendData)
        {
            string strBytes = StringUtil.BytesToString(sendData);
            TraceBusSendMsg(source, strBytes);
        }

        public static void TraceBusRecvMsg(object source, byte[] recvData)
        {
            string strBytes = StringUtil.BytesToString(recvData);
            TraceBusRecvMsg(source, strBytes);
        }
    }
}