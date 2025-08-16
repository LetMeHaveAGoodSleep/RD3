using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace RD3.ViewModels
{
    public class BaseViewModel : BindableBase
    {
        private static readonly string KeyboardFilePath = string.Concat(AppDomain.CurrentDomain.BaseDirectory, "InputKeyboard.exe");
        private static KeyBoardType ShowingType;
        private static string? ShowingTitle;
        private static bool MultiTouch = false;

        public readonly IContainerProvider ContainerProvider;
        public readonly IEventAggregator aggregator;

        public readonly ILanguage Language;
        public ICommandWrapper CommandWrapper
        {
            get => InstrumentSolution.GetInstance().CommandWrapper;
        }

        public readonly IDialogHostService DialogHostService;

        public DelegateCommand OpenNumberKeyBoardCommand => new(() =>
        {
            ShowKeyboard(KeyBoardType.Number);
        });

        public DelegateCommand OpenNormalKeyBoardCommand => new(() => 
        {
            ShowKeyboard();
        });

        public BaseViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService)
        {
            this.ContainerProvider = containerProvider;
            this.aggregator = containerProvider.Resolve<IEventAggregator>();
            this.Language = containerProvider.Resolve<ILanguage>();
            //DialogService = containerProvider.Resolve<IDialogService>(); 
            if (AppSession.LanguageName == Const.CHNLanguage)
            {
                Language.LoadResourceKey("zh_CN");//默认显示中文
            }
            else
            {
                Language.LoadResourceKey("en_US");
            }

            DialogHostService = dialogHostService;
        }

        public static void ShowKeyboard(KeyBoardType type = KeyBoardType.Normal, string? title = null)
        {
            if (string.IsNullOrEmpty(title)) title = "";
            string arguments = string.Concat("layout=", type.ToString(), " opacity=0.85 multitouch=", MultiTouch, string.IsNullOrEmpty(title) ? "" : string.Concat(" title=", title));
            if (type.Equals(ShowingType) && title.Equals(ShowingTitle))
            {
                try
                {
                    var current = Process.GetCurrentProcess();
                    var ps = Process.GetProcessesByName("InputKeyboard");
                    if (ps.Length > 0) return;
                    Process.Start(new ProcessStartInfo(KeyboardFilePath, arguments));
                }
                catch (Exception)
                {
                    Process.Start(new ProcessStartInfo(KeyboardFilePath, arguments));
                }
            }
            else
            {
                Process.Start(new ProcessStartInfo(KeyboardFilePath, arguments));
                ShowingType = type;
            }
        }

    }
}
