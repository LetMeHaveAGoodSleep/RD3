using HandyControl.Controls;
using Microsoft.VisualBasic;
using RD3.Common;
using RD3.Shared;
using RD3.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using XZ.SQLite;
using static MaterialDesignThemes.Wpf.Theme;

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
            tabMenu.SelectionChanged += TabControl_SelectionChanged;

            FormattedTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => 
            {
                FormattedTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            timer.Start();

            RefershPumpMFC();

            //调试模式下，就不需要让窗口不可移动
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                this.WindowStyle = WindowStyle.None;
                SourceInitialized += (s, e) =>
                {
                    IntPtr hwnd = new WindowInteropHelper(this).Handle;
                    HwndSource.FromHwnd(hwnd)?.AddHook(WndProc);
                    ForceFullScreen();
                };
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_NCHITTEST = 0x0084;
            if (msg == WM_NCHITTEST)
            {
                // 始终返回非可拖动区域标识
                handled = true;
                return (IntPtr)1; // HTNOWHERE
            }
            return IntPtr.Zero;
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd,int attr,ref int attrValue,int attrSize);

        private void ForceFullScreen()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            // 移除WS_THICKFRAME和WS_MAXIMIZEBOX样式
            SetWindowLong(hwnd, -16, 0x10000000);
            // 禁用DWM动画
            int disableAnim = 1;
            DwmSetWindowAttribute(hwnd, 3, ref disableAnim, sizeof(int));
        }


        private void RefershPumpMFC()
        {
            pump1.ResumePumpSetting(1);
            pump2.ResumePumpSetting(2);
            pump3.ResumePumpSetting(3);
            pump4.ResumePumpSetting(4);
            pump5.ResumePumpSetting(5);
            pump6.ResumePumpSetting(6);

            mfc1.ResumeMFCSetting(1);
            mfc2.ResumeMFCSetting(2);
            mfc3.ResumeMFCSetting(3);
            mfc4.ResumeMFCSetting(4);
        }

        private void PadMainView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.TabControl tabControl = sender as System.Windows.Controls.TabControl;
            if (e.AddedItems.Count < 1) return;

            if (e.AddedItems[0] is System.Windows.Controls.TabItem selectedItem)
            {
                if (selectedItem.Name == nameof(tabBatch))
                {
                    (batchView.DataContext as NewBatchViewModel).ReloadDataCommand.Execute();
                }

                if (selectedItem.Tag != null)
                {
                    var lastItem = e.RemovedItems[0] as System.Windows.Controls.TabItem;
                    lastItem.IsSelected = true;
                }
            }
        }

        private void WindowMinimizeEvent(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void WindowCloseEvent(object sender, MouseButtonEventArgs e)
        {
            if (AppSession.CurrentUser.Type != Shared.UserType.Admin)
            {
                HandyControl.Controls.Growl.WarningGlobal("非管理员不可关闭软件！");
            }
            else
            {
                if (HandyControl.Controls.MessageBox.Show("确定退出本系统？", "温馨提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                    Environment.Exit(0);
                    return;
                }
                this.WindowState = WindowState.Maximized;
            }
        }
    }
}
