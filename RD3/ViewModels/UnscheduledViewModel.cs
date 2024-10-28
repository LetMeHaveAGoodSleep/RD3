using ImTools;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Extensions;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace RD3.ViewModels
{
    public class UnscheduledViewModel : BaseViewModel, IDialogAware
    {
        private UnScheduleAction _scheduleAction;

        private string _subTitle;
         public string SubTitle
        {
            get { return _subTitle; }
            set { SetProperty(ref _subTitle, value); }
        }

        private double _volume;
        public double Volume
        {
            get { return _volume; }
            set { SetProperty(ref _volume, value); }
        }

        public List<string> Devices { get; set; }

        public DelegateCommand CancelCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public DelegateCommand OKCommand => new(() => 
        {
            DialogParameters dialogParameters = new DialogParameters()
            {
                { nameof(Volume),Volume },
                {"Time",DateTime.Now },
                {"ActionType",_scheduleAction },
                {"Devices", Devices}
            };
            IDialogResult result = new DialogResult(ButtonResult.OK,dialogParameters);
            RequestClose?.Invoke(result);
        });

        public UnscheduledViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => string.Empty;

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
            _scheduleAction = parameters.GetValue<UnScheduleAction>("ActionType");
            SubTitle = EnumUtil.GetEnumDescription(_scheduleAction);
        }
    }
}
