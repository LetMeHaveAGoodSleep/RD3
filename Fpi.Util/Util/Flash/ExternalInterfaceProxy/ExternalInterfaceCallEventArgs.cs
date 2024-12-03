using System;
using System.Collections.Generic;
using System.Text;

namespace Fpi.Util.Flash.ExternalInterfaceProxy
{
    /// <summary>
    /// 此CS文件中定义两个C#类型（类）：一个自定义委托类和一个事件参数类
    /// ExternalInterfaceProxy 类用它们来向侦听器通知来自ActionScript的函数调用。
    /// </summary>
    public delegate object ExternalInterfaceCallEventHandler(object sender, ExternalInterfaceCallEventArgs e);

    public class ExternalInterfaceCallEventArgs: System.EventArgs
    {
        private ExternalInterfaceCall _functionCall;

        public ExternalInterfaceCallEventArgs(ExternalInterfaceCall functionCall)
            : base()
        {
            _functionCall = functionCall;
        }

        public ExternalInterfaceCall FunctionCall
        {
            get { return _functionCall; }
        }
    }
}
