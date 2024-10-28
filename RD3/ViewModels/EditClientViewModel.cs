using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
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
    public class EditClientViewModel : BaseViewModel, IDialogAware
    {
        private ClientConfig _clientConfig;

        public EditClientViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public ClientConfig Client
        {
            get { return _clientConfig; }
            set { SetProperty(ref _clientConfig, value); }
        }
        public string Title => "";

        public event Action<IDialogResult> RequestClose;

        public DelegateCommand OKCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.OK)));

        public DelegateCommand CancelCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Client = parameters.GetValue<ClientConfig>("Client");
        }
    }
}
