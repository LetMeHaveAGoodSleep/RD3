using HandyControl.Controls;
using RD3.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RD3.Views
{
    /// <summary>
    /// PadMainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PadMainView : HandyControl.Controls.GlowWindow
    {
        public PadMainView()
        {
            InitializeComponent();
            this.Closing += PadMainView_Closing;
            FormattedTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += (s, e) => 
            {
                FormattedTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            timer.Start();
        }

        private void PadMainView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (AppSession.CurrentUser.Type != Shared.UserType.Admin)
            {
                HandyControl.Controls.Growl.WarningGlobal("非管理员不可关闭软件！");
                e.Cancel = true;
            }
        }
    }
}
