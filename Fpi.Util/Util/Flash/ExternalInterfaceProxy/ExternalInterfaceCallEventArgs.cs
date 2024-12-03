using System;
using System.Collections.Generic;
using System.Text;

namespace Fpi.Util.Flash.ExternalInterfaceProxy
{
    /// <summary>
    /// ��CS�ļ��ж�������C#���ͣ��ࣩ��һ���Զ���ί�����һ���¼�������
    /// ExternalInterfaceProxy ������������������֪ͨ����ActionScript�ĺ������á�
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
