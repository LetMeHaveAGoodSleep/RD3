using System;
using System.Collections.Generic;
using System.Text;

namespace Fpi.Util.Flash.FlashShieldRightKey
{
    /// <summary>
    /// Flash��������һ���˵�
    /// ��ʹ����������ѡ����Menu=False
    /// Ҳһ�����С�����Flash Player 7-9���������Ĳ˵�
    /// ����ͨ����Ϣ��������Flash�ؼ��е��Ҽ�
    /// ���⣬����ԱҲ�����Լ������Ӧ���Ҽ��˵�
    /// -----------------------------------------------------------------
    /// ��Ҫע�⣬��Form.Designer.cs�ļ���(�����Form�����վ���Ĵ�����):
    /// this.axShockwaveFlash1 = new NameSpace. FlashShieldClass();
    /// -----------------------------------------------------------------
    /// </summary>
    public class FlashShieldKey : AxShockwaveFlashObjects.AxShockwaveFlash 
    {
        //����Flash�Ҽ�����
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // ����Ҽ���ֵ��ͬʱ���������Ӧ�Ĳ˵�
            if (m.Msg == 0x0204) 
            {
                return;
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}
