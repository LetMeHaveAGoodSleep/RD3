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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RD3.ViewModels
{
    public class EditUserViewModel : NavigationViewModel,IDialogAware
    {
        private User _user;
        public User User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        private string _confirmPwd;
        public string ConfirmPwd
        {
            get { return _confirmPwd; }
            set { SetProperty(ref _confirmPwd, value); }
        }

        public List<string> TypeList = new List<string>() { UserType.Admin.ToString(), UserType.Factory.ToString(), UserType.Service.ToString(), UserType.User.ToString() };

        public string Title => "";

        public event Action<IDialogResult> RequestClose;

        public DelegateCommand OKCommand => new(() =>
        {
            if (User.Password != ConfirmPwd)
            {
                MessageBox.Show(Language.GetValue("密码不一致").ToString());
                return;
            }
            User.Createtime= DateTime.Now;
            User.Creator = AppSession.CurrentUser.UserName;
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }
        );

        public DelegateCommand CancelCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public EditUserViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {

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
            User = parameters.GetValue<User>("User");
        }
    }
}
