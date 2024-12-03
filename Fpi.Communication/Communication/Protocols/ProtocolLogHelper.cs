using Fpi.Communication;
using Fpi.Util.Sundry;
using Fpi.Communication.Interfaces;
using Fpi.Util;

namespace Fpi.Communication.Protocols
{
    /// <summary>
    /// 协议日志输出帮助
    /// </summary>
    public class ProtocolLogHelper
    {
        private const string MsgType = "ProtocolMessage";

        #region 调试
        /// <summary>
        /// 调试接收信息
        /// </summary>
        /// <param name="data"></param>
        public static void TraceSendMsg(object data)
        {
            if (data == null)
            {
                return;
            }

            string info = string.Empty;
            if (data is IByteStream)
            {
                info = (data as IByteStream).ToString();
            }
            else if (data is byte[])
            {
                info = StringUtil.BytesToString((byte[]) data);
            }
            else
            {
                info = data.ToString();
            }

            TraceMsg (Fpi.Properties.Resources.SendData + info);
        }
        /// <summary>
        /// 调试发送信息
        /// </summary>
        /// <param name="data"></param>
        public static void TraceRecvMsg(object data)
        {
            if (data == null)
            {
                return;
            }

            string info = string.Empty;
            if (data is IByteStream)
            {
                info = (data as IByteStream).ToString();
            }
            else if (data is byte[])
            {
                info = StringUtil.BytesToString((byte[]) data);
            }
            else
            {
                info = data.ToString();
            }

            TraceMsg(Fpi.Properties.Resources.ReceiveData + info);
        }
        /// <summary>
        /// 调试信息
        /// </summary>
        /// <param name="msg"></param>
        public static void TraceMsg(string msg)
        {
            try
            {
                LogHelper.Debug(msg);
            }
            catch
            {
            }
        }
        #endregion
        #region 显示
        /// <summary>
        /// 接收信息
        /// </summary>
        /// <param name="data"></param>
        public static void ShowSendMsg(object data)
        {
            if (data == null)
            {
                return;
            }

            string info = string.Empty;
            if (data is IByteStream)
            {
                info = (data as IByteStream).ToString();
            }
            else if (data is byte[])
            {
                info = StringUtil.BytesToString((byte[])data);
            }
            else
            {
                info = data.ToString();
            }

            ShowMsg(Fpi.Properties.Resources.SendData + info);
        }
        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="data"></param>
        public static void ShowRecvMsg(object data)
        {
            if (data == null)
            {
                return;
            }

            string info = string.Empty;
            if (data is IByteStream)
            {
                info = (data as IByteStream).ToString();
            }
            else if (data is byte[])
            {
                info = StringUtil.BytesToString((byte[])data);
            }
            else
            {
                info = data.ToString();
            }

            ShowMsg(Fpi.Properties.Resources.ReceiveData + info);
        }
        /// <summary>
        /// 信息
        /// </summary>
        /// <param name="msg"></param>
        public static void ShowMsg(string msg)
        {
            LogHelper.Info(msg);
        }
        #endregion
    }
}