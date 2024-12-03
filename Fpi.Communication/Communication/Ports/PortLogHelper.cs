
using Fpi.Util.Sundry;
using Fpi.Properties;
using Fpi.Util;
namespace Fpi.Communication.Ports
{
    public class PortLogHelper
    {
        private PortLogHelper()
        {
        }

        private const string MsgType = "PortMessage";

        public static void TracePortMsg(string msg)
        {
            try
            {
                LogHelper.Debug(msg);
            }
            catch
            {
            }
        }

        public static void TracePortSendMsg(byte[] sendData)
        {
            string strBytes = StringUtil.BytesToString(sendData);
            TracePortSendMsg(strBytes);
        }

        public static void TracePortRecvMsg(byte[] recvData)
        {
            string strBytes = StringUtil.BytesToString(recvData);
            TracePortRecvMsg(strBytes);
        }

        public static void TracePortSendMsg(string sendData)
        {
            try
            {
                LogHelper.Debug(Resources.Send + ":" + sendData);
            }
            catch
            {
            }
        }

        public static void TracePortRecvMsg(string recvData)
        {
            try
            {
                LogHelper.Debug(Resources.Recv + ":" + recvData);
            }
            catch
            {
            }
        }

        public static void TracePortSendMsg(object source, string sendData)
        {
            try
            {
                LogHelper.Debug("[" + source.ToString() + "]" + Resources.Send + ":" + sendData);
            }
            catch
            {
            }
        }

        public static void TracePortRecvMsg(object source, string recvData)
        {
            try
            {
                LogHelper.Debug("[" + source.ToString() + "]" + Resources.Recv + ":" + recvData);
            }
            catch
            {
            }
        }

        public static void TracePortSendMsg(object source, byte[] sendData)
        {
            string strBytes = StringUtil.BytesToString(sendData);
            TracePortSendMsg(source, strBytes);
        }

        public static void TracePortRecvMsg(object source, byte[] recvData)
        {
            string strBytes = StringUtil.BytesToString(recvData);
            TracePortRecvMsg(source, strBytes);
        }
    }
}