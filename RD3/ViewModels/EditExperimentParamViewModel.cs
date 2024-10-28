using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    internal class EditExperimentParamViewModel : BaseViewModel, IDialogAware
    {
        private DeviceParameter _currentDeviceParameter;
        public DeviceParameter CurrentDeviceParameter
        {
            get { return _currentDeviceParameter; }
            set { SetProperty(ref _currentDeviceParameter, value); }
        }

        public EditExperimentParamViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => "";

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
            CurrentDeviceParameter = parameters.GetValue<DeviceParameter>("DeviceParam").Clone() as DeviceParameter;
        }

        public DelegateCommand CancelCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public DelegateCommand ApplyCommand => new(() =>
        {
            DialogParameters keyValuePairs = new DialogParameters();
            keyValuePairs.Add("DeviceParam", CurrentDeviceParameter);
            DialogResult dialogResult = new DialogResult(ButtonResult.OK, keyValuePairs);
            RequestClose?.Invoke(dialogResult);
        });
    }
}
