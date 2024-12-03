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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation.Text;
using System.Windows.Threading;

namespace RD3.ViewModels
{
    public class SelfCheckViewModel : BaseViewModel, IDialogAware
    {
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
            aggregator.ResgiterMessage((MessageModel model) =>
            {
                Content = model.Message;
            }, nameof(SelfCheckViewModel));
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
            //初始化DispatcherTimer
           countdownTimer = new DispatcherTimer();
            countdownTimer.Interval = TimeSpan.FromSeconds(1); // 设置时间间隔为1秒  
            countdownTimer.Tick += CountdownTimer_Tick; ; // 订阅Tick事件 
            countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            LoadConfig();
            CloseDialog();
            countdownTimer?.Stop();
            countdownTimer = null;
        }

        private void LoadConfig()
        {
            InitializationListener agent = containerProvider.Resolve<InitializationListener>();
            LibraryManager.GetInstance().InitAllClass(agent);
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
