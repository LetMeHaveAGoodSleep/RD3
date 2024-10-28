using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RD3.ViewModels
{
    public class SelfCheckViewModel : NavigationViewModel, IDialogAware
    {
        private string content;
        public String Content
        {
            get { return content; }
            set { content = value; RaisePropertyChanged(); }
        }

        private DispatcherTimer countdownTimer;

        public SelfCheckViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => "仪器自检";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            // 初始化DispatcherTimer  
            countdownTimer = new DispatcherTimer();
            countdownTimer.Interval = TimeSpan.FromSeconds(1); // 设置时间间隔为1秒  
            countdownTimer.Tick += CountdownTimer_Tick; ; // 订阅Tick事件 
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            CheckSelf();
            CloseDialog();
            countdownTimer?.Stop();
            countdownTimer = null;
        }

        private void CloseDialog()
        {
            var buttonResult = ButtonResult.OK;
            RequestClose?.Invoke(new DialogResult(buttonResult));
        }
        private void CheckSelf()
        {
            Content += "通讯配置中\r\n";
            Content += "通讯配置完成\r\n";
            Content += "数采模块初始化中\r\n";
            Content += "数采模块初始化完成\r\n";
            Content += "数采模块初始化中\r\n";
            Content += "数采模块初始化完成\r\n";
            Content += "数采模块初始化中\r\n";
            Content += "数采模块初始化完成\r\n";
        }
    }
}
