/*
 * -----------------------------------------------------------------
 * 功能：
 * 从具有 Flash Player ActiveX 控件的应用程序调用 ActionScript 函数
 * 从 ActionScript 接收函数调用，并在 ActiveX 容器中处理它们
 * 使用 ExternalInterfaceProxy 类来隐藏已序列化的 XML 格式的详细信息
 * Flash Player 使用该格式将消息发送到 ActiveX 容器
 * -----------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Text;
using AxShockwaveFlashObjects;
using System.Runtime.InteropServices;

namespace Fpi.Util.Flash.ExternalInterfaceProxy
{
    /// <summary>
    /// 作为 Flash ActiveX 控件的包装以进行 External Interface 通信的类
    /// 为从 ActionScript 进行调用和接收调用提供了机制。
    /// </summary>
    public class ExternalInterfaceProxy
    {
        private AxShockwaveFlash _flashControl;
        public ExternalInterfaceProxy(AxShockwaveFlash flashControl)
        {
            _flashControl = flashControl;
            _flashControl.FlashCall += new _IShockwaveFlashEvents_FlashCallEventHandler(_flashControl_FlashCall);
        }

        /// <summary>
        /// C#访问在ActionScript ExternalInterface类中注册的方法
        /// 执行ActionScrip 函数调用
        /// </summary>
        /// <param name="functionName">ActionScript中的方法名</param>
        /// <param name="args">参数</param>
        /// <returns>ActionScript方法返回的操作结果</returns>
        public object Call(string functionName, params object[] arguments)
        {
            try
            {
                string request = ExternalInterfaceSerializer.EncodeInvoke(functionName, arguments);
                string response = _flashControl.CallFunction(request);
                object result = ExternalInterfaceSerializer.DecodeResult(response);
                return result;
            }
            catch (COMException ex)
            {
                throw new Exception(ex.Message);
            }
        }


        /// <summary>
        /// 当函数调用来自Flash Player时，ExternalInterfaceProxy 类将调度该事件
        /// 即ActionScript访问C#事件通知
        /// </summary>
        public event ExternalInterfaceCallEventHandler ExternalInterfaceCall;

        protected virtual object OnExternalInterfaceCall(ExternalInterfaceCallEventArgs e)
        {
            if (ExternalInterfaceCall != null)
            {
                return ExternalInterfaceCall(this, e);
            }
            return null;
        }


        /// <summary>
        /// 来自ActionScript的函数调用会导致Flash ActiveX控件调度它的FlashCall事件
        /// </summary>
        /// <param name="e">使用事件对象的request属性(“e.request”)来执行某些操作</param>
        private void _flashControl_FlashCall(object sender, _IShockwaveFlashEvents_FlashCallEvent e)
        {
            ExternalInterfaceCall functionCall = ExternalInterfaceSerializer.DecodeInvoke(e.request);
            ExternalInterfaceCallEventArgs eventArgs = new ExternalInterfaceCallEventArgs(functionCall);
            object response = OnExternalInterfaceCall(eventArgs);
            _flashControl.SetReturnValue(ExternalInterfaceSerializer.EncodeResult(response));
        }

    }
}
