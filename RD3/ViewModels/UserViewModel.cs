using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    internal class UserViewModel : BindableBase, IDialogAware
    {
        private readonly IDialogService dialogService;

        public string Title => AppSession.CompanyName;

        public event Action<IDialogResult> RequestClose;

        public ObservableCollection<User> Users { 
            get { return AppSession.Users; } 
        }

        public DelegateCommand CloseCommand { get; private set; }

        public DelegateCommand<string> ExcuteCommand { get; private set; }

        public UserViewModel(IDialogService dialog) 
        {
            dialogService = dialog;
            CloseCommand = new DelegateCommand(() => 
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            });

            ExcuteCommand = new DelegateCommand<string>(Execute);
        }

        private void Execute(string obj)
        {
            switch (obj)
            {
                case "Add":
                    dialogService.ShowDialog("AddUser", callback =>
                    {
                    });
                    break;
                case "Edit":
                    dialogService.ShowDialog("AddUser", new DialogParameters($"UserName={AppSession.CurrentUser.UserName}"), callback =>
                    {
                    });
                    break;

            }
        }

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
    }
}
