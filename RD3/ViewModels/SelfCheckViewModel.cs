using Fpi.Assembly;
using Fpi.Util.Interfaces.Initialize;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Common.Events;
using RD3.Extensions;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation.Text;
using System.Windows.Threading;
using XZ.SQLite;

namespace RD3.ViewModels
{
    public class SelfCheckViewModel : BaseViewModel, IDialogAware
    {
        private SubscriptionToken token = null;

        private string _content;
        public String Content
        {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }

        private DispatcherTimer countdownTimer;

        public SelfCheckViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            _content = Language.GetValue("Application is initializing...").ToString();
        }

        public string Title => "仪器自检";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            token.Dispose();
            token = null;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            //初始化DispatcherTimer
            countdownTimer = new DispatcherTimer();
            countdownTimer.Interval = TimeSpan.FromSeconds(1); // 设置时间间隔为1秒  
            countdownTimer.Tick += CountdownTimer_Tick; ; // 订阅Tick事件 
            countdownTimer.Start();

            token = aggregator.ResgiterMessage((MessageModel model) =>
               {
                   Content = model.Message;
               }, nameof(SelfCheckViewModel));
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            LoadConfig();
            CloseDialog();

            PropertyInfo[] propertyInfos = typeof(RealTimeParam).GetProperties().Where(c => c.CanWrite && c.CanRead && (c.PropertyType == typeof(double) || c.PropertyType == typeof(float) || c.PropertyType == typeof(int) || c.PropertyType == typeof(string))).ToArray();
            RD3SQLHelper.CreateRealTimeParamTable1(propertyInfos);

            EnhancedSqliteBackupService backupService = new EnhancedSqliteBackupService(@"hisDatas\xzrd3.db", AppDomain.CurrentDomain.BaseDirectory + @"\DatabaseBackups");
            backupService.Start();

            countdownTimer?.Stop();
            countdownTimer = null;
        }

        private void LoadConfig()
        {
            InitializationListener agent = ContainerProvider.Resolve<InitializationListener>();
            LibraryManager.GetInstance().InitAllClass(agent);
            ClockSupervisor.GetInstance();
        }


        private void CloseDialog()
        {
            var buttonResult = ButtonResult.OK;
            RequestClose?.Invoke(new DialogResult(buttonResult));
        }
    }

    class InitializationListener : IInitializationListener
    {
        readonly ILanguage _language;
        readonly IEventAggregator _aggregator;

        public InitializationListener()
        {
            var containerProvider = (System.Windows.Application.Current as App).Container;
            _language = containerProvider.Resolve<ILanguage>();
            _aggregator = containerProvider.Resolve<IEventAggregator>();
        }
        

        public void OnInitException(object source, InitException ex, bool fatal)
        {
            InitMember im = (InitMember)source;
            if (fatal)
            {
                var dialogResult = DialogExtensions.Info("温馨提示", string.Format(_language.GetValue("An exception occurred while loading {0}! The program will exit soon.").ToString(), im.name));
                Environment.Exit(0);
            }
            else
            {
                string info = string.Format(_language.GetValue("Loading {0}\r\nException: {1}\r\nSource: {2}").ToString(), im.name, ex.Message, ex.StackTrace);
                _aggregator.SendMessage(info, nameof(SelfCheckViewModel));
            }
        }

        public void BeforeInit(object source)
        {
            InitMember im = (InitMember)source;

            string info = string.IsNullOrEmpty(im.description) ? string.Format(_language.GetValue("Loading {0}...").ToString(), im.name) : im.description;

            _aggregator.SendMessage(info, nameof(SelfCheckViewModel));
        }

        public void AfterInit(object source)
        {
            InitMember im = (InitMember)source;

            string info;

            if (string.IsNullOrEmpty(im.description))
            {
                info = im.name + _language.GetValue("Loading completed").ToString();
            }
            else
            {
                info = im.description;
            }

            _aggregator.SendMessage(info, nameof(SelfCheckViewModel));
        }
    }
}
