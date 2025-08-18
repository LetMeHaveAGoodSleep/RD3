using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace RD3.Controls
{
    public class TextBoxNoAutoKeyboard : TextBox
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new FrameworkElementAutomationPeer(this);
        }
    }
}
