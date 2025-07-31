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
            FormattedTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => 
            {
                FormattedTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            timer.Start();

            RefershPumps(true);

            SourceInitialized += (s, e) => {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                HwndSource.FromHwnd(hwnd)?.AddHook(WndProc);
                ForceFullScreen();
            };
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


        private void RefershPumps(bool init)
        {
            pump1.ResumePumpSetting(1);
            pump2.ResumePumpSetting(2);
            pump3.ResumePumpSetting(3);
            pump4.ResumePumpSetting(4);
            pump5.ResumePumpSetting(5);
            pump6.ResumePumpSetting(6);

            mfc1.RefershControls("空气");
            mfc2.RefershControls("氧气");
            mfc3.RefershControls("氧气");
            mfc4.RefershControls("氮气");
        }

        private void PadMainView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (AppSession.CurrentUser.Type != Shared.UserType.Admin)
            {
                HandyControl.Controls.Growl.WarningGlobal("非管理员不可关闭软件！");
                e.Cancel = true;
            }
            else
            {
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
        }

        private void ButtonView_Click(object sender, RoutedEventArgs e)
        {
            object o = dataGridBatch.SelectedItem;
            List<RD3Batch> batches = new List<RD3Batch>();
            batches.Add(o as RD3Batch);
            ((PadMainViewModel)this.DataContext)?.CompareBatchCommand.Execute(batches);
        }
        private void ButtonDeleteBatch_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridBatch.SelectedItems.Count < 1)
            {
                return;
            }
            List<RD3Batch> list = dataGridBatch.SelectedItems.Cast<RD3Batch>().ToList();
            ((PadMainViewModel)this.DataContext)?.DeleteBatchCommand.Execute(list);
        }

        private void DataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu context = new ContextMenu();
            MenuItem item = new MenuItem() { Width = 100 };
            item.Header = "批次比较";
            item.Click += new RoutedEventHandler(CompareBatch_click);
            context.Items.Add(item);
            context.IsOpen = true;

            MenuItem dataOutPut = new MenuItem() { Width = 100 };
            dataOutPut.Header = "数据导出";
            dataOutPut.Click += new RoutedEventHandler(BatchDataOutPut_click);
            context.Items.Add(dataOutPut);
            context.IsOpen = true;

            MenuItem offlineData = new MenuItem() { Width = 100 };
            offlineData.Header = "离线数据";
            offlineData.Click += new RoutedEventHandler(BatchOffLinedata_click);
            context.Items.Add(offlineData);
            context.IsOpen = true;
        }

        /// <summary>
        /// 右键-数据导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BatchDataOutPut_click(object sender, RoutedEventArgs e)
        {
            if (dataGridBatch.SelectedItems.Count > 0)
            {
                List<RD3Batch> batches = new List<RD3Batch>();
                //1弹出选数据//2显示
                foreach (var item in dataGridBatch.SelectedItems)
                {
                    batches.Add(item as RD3Batch);
                    break;
                }
            ((PadMainViewModel)this.DataContext)?.OutputDataCommand.Execute(batches[0]);
            }
        }

        /// <summary>
        /// 导入离线数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BatchOffLinedata_click(object sender, RoutedEventArgs e)
        {
            if (dataGridBatch.SelectedItems.Count > 0)
            {
                List<RD3Batch> batches = new List<RD3Batch>();
                foreach (var item in dataGridBatch.SelectedItems)
                {
                    batches.Add(item as RD3Batch);
                    break;
                }
            ((PadMainViewModel)this.DataContext)?.OffLineDataCommand.Execute(batches[0]);
            }
        }

        /// <summary>
        /// 比较
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompareBatch_click(object sender, RoutedEventArgs e)
        {
            List<RD3Batch> batches = new List<RD3Batch>();
            //1弹出选数据//2显示
            foreach (var item in dataGridBatch.SelectedItems)
            {
                batches.Add(item as RD3Batch);
            }
            ((PadMainViewModel)this.DataContext)?.CompareBatchCommand.Execute(batches);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
