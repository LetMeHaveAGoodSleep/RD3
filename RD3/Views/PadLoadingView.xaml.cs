using Fpi.Assembly;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Shared;
using RD3.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using XZ.SQLite;

namespace RD3.Views
{
    /// <summary>
    /// PadLoadingView.xaml 的交互逻辑
    /// </summary>
    public partial class PadLoadingView : Window
    {
        private DispatcherTimer _countdownTimer;

        public PadLoadingView()
        {
            InitializeComponent();

            //初始化DispatcherTimer
            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(1); // 设置时间间隔为1秒  
            _countdownTimer.Tick += CountdownTimer_Tick; // 订阅Tick事件 
            _countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            LoadConfig();

            PropertyInfo[] propertyInfos = typeof(RealTimeParam).GetProperties().Where(c => c.CanWrite && c.CanRead && (c.PropertyType == typeof(double) || c.PropertyType == typeof(float) || c.PropertyType == typeof(int) || c.PropertyType == typeof(string))).ToArray();
            RD3SQLHelper.CreateRealTimeParamTable1(propertyInfos);

            EnhancedSqliteBackupService backupService = new EnhancedSqliteBackupService(@"hisDatas\xzrd3.db", AppDomain.CurrentDomain.BaseDirectory + @"\DatabaseBackups");
            backupService.Start();

            _countdownTimer?.Stop();
            _countdownTimer = null;
        }

        private void LoadConfig()
        {
            InitializationListener agent = (System.Windows.Application.Current as App).Container.Resolve<InitializationListener>();
            LibraryManager.GetInstance().InitAllClass(agent);
            ClockSupervisor.GetInstance();
        }
    }
}
