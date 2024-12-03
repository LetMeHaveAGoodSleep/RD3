using Fpi.Properties;
using Fpi.Util;
using Fpi.Util.Sundry;

namespace Fpi.Communication.Manager
{
    public class PipeLogHelper
    {
        private PipeLogHelper()
        {
        }

        private const string MsgType = "PipeMessage";

        public static void TraceMsg(string msg)
        {
            try
            {
                Fpi.Util.LogHelper.Debug(msg);
            }
            catch
            {
            }          
        }

        public static void TraceSendMsg(byte[] sendData)
        {
            string strBytes = StringUtil.BytesToString(sendData);
            TraceSendMsg(strBytes);
        }

        public static void TraceRecvMsg(byte[] recvData)
        {
            string strBytes = StringUtil.BytesToString(recvData);
            TraceRecvMsg(strBytes);
        }

        public static void TraceSendMsg(string sendData)
        {
            try
            {
                LogHelper.Debug(Resources.Send + ":" + sendData);
            }
            catch
            {
            }
        }

        public static void TraceRecvMsg(string recvData)
        {
            try
            {
                LogHelper.Debug(Resources.Recv + ":" + recvData);
            }
            catch
            {
            }
        }
    }
}