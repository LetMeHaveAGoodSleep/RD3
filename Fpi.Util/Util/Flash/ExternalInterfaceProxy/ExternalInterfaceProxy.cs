/*
 * -----------------------------------------------------------------
 * ���ܣ�
 * �Ӿ��� Flash Player ActiveX �ؼ���Ӧ�ó������ ActionScript ����
 * �� ActionScript ���պ������ã����� ActiveX �����д�������
 * ʹ�� ExternalInterfaceProxy �������������л��� XML ��ʽ����ϸ��Ϣ
 * Flash Player ʹ�øø�ʽ����Ϣ���͵� ActiveX ����
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
    /// ��Ϊ Flash ActiveX �ؼ��İ�װ�Խ��� External Interface ͨ�ŵ���
    /// Ϊ�� ActionScript ���е��úͽ��յ����ṩ�˻��ơ�
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
        /// C#������ActionScript ExternalInterface����ע��ķ���
        /// ִ��ActionScrip ��������
        /// </summary>
        /// <param name="functionName">ActionScript�еķ�����</param>
        /// <param name="args">����</param>
        /// <returns>ActionScript�������صĲ������</returns>
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
        /// ��������������Flash Playerʱ��ExternalInterfaceProxy �ཫ���ȸ��¼�
        /// ��ActionScript����C#�¼�֪ͨ
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
        /// ����ActionScript�ĺ������ûᵼ��Flash ActiveX�ؼ���������FlashCall�¼�
        /// </summary>
        /// <param name="e">ʹ���¼������request����(��e.request��)��ִ��ĳЩ����</param>
        private void _flashControl_FlashCall(object sender, _IShockwaveFlashEvents_FlashCallEvent e)
        {
            ExternalInterfaceCall functionCall = ExternalInterfaceSerializer.DecodeInvoke(e.request);
            ExternalInterfaceCallEventArgs eventArgs = new ExternalInterfaceCallEventArgs(functionCall);
            object response = OnExternalInterfaceCall(eventArgs);
            _flashControl.SetReturnValue(ExternalInterfaceSerializer.EncodeResult(response));
        }

    }
}
