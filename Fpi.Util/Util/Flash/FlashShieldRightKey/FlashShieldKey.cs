using System;
using System.Collections.Generic;
using System.Text;

namespace Fpi.Util.Flash.FlashShieldRightKey
{
    /// <summary>
    /// Flash播放器有一个菜单
    /// 即使是在属性里选择了Menu=False
    /// 也一样会有“关于Flash Player 7-9播放器”的菜单
    /// 此类通过消息拦截屏蔽Flash控件中的右键
    /// 另外，程序员也可以自己添加相应的右键菜单
    /// -----------------------------------------------------------------
    /// 需要注意，在Form.Designer.cs文件中(这里的Form名按照具体的窗体名):
    /// this.axShockwaveFlash1 = new NameSpace. FlashShieldClass();
    /// -----------------------------------------------------------------
    /// </summary>
    public class FlashShieldKey : AxShockwaveFlashObjects.AxShockwaveFlash 
    {
        //屏蔽Flash右键功能
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            // 鼠标右键键值，同时可以添加相应的菜单
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
