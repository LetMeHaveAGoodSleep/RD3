using Fpi.Log;
using Fpi.Util.Sundry;

namespace Fpi.Communication
{
    public class LogHelper
    {
        private LogHelper()
        {
        }

        private const string MsgType = "CommunicationMessage";

        public static void TraceMsg(string msg)
        {
            LogUtil.Log(MsgType, msg);
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
            LogUtil.Log(MsgType, "Send:" + sendData);
        }

        public static void TraceRecvMsg(string recvData)
        {
            LogUtil.Log(MsgType, "Recv:" + recvData);
        }
    }
}