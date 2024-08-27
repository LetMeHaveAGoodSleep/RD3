using HandyControl.Controls;
using HandyControl.Data;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class CommunicationViewModel : NavigationViewModel, IDialogAware
    {
        private IDialogService dialog;
        public ObservableCollection<ClientConfig> ClientCol
        {
            get { return CommunicationManager.GetInstance().Clients; }
        }

        public DelegateCommand CloseCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.OK)));

        public DelegateCommand<ClientConfig> EditCommand => new((ClientConfig client) => 
        {
            DialogParameters pairs = new DialogParameters
            {
                { "Client", client }
            };
            dialog?.ShowDialog("EditClientView", pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                CommunicationManager.GetInstance().Save();
            });
        });
        public DelegateCommand<ClientConfig> DeleteCommand => new((ClientConfig client) =>
        {
            ClientCol.Remove(client);
            CommunicationManager.GetInstance().Save();
        });

        public string Title => "通讯配置";

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
            
        }

        public CommunicationViewModel(IContainerProvider containerProvider, IDialogService dialogService) : base(containerProvider)
        {
            dialog = dialogService;
        }
    }
}
