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
    public class EditUserViewModel : BaseViewModel, IDialogAware
    {
        private string _mode;
        private User _user;
        public User User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        private UserType _userType;
        public UserType UserType
        {
            get { return _userType; }
            set { SetProperty(ref _userType, value); }
        }

        private string _confirmPwd;
        public string ConfirmPwd
        {
            get { return _confirmPwd; }
            set { SetProperty(ref _confirmPwd, value); }
        }

        public string Title => "";

        public event Action<IDialogResult> RequestClose;

        public DelegateCommand OKCommand => new(() =>
        {
            if (User.Password != ConfirmPwd)
            {
                MessageBox.Show(Language.GetValue("密码不一致").ToString());
                return;
            }
            var collection = UserManager.GetInstance().Users.Where(t => t.UserName == User.UserName);
            int count = _mode == "Add" ? 1 : 2;
            if (collection.Count() > count)
            {
                MessageBox.Show(Language.GetValue(string.Format("已存在用户名‘{0}’", User.UserName)).ToString());
                return;
            }
            User.Createtime = DateTime.Now;
            User.Creator = AppSession.CurrentUser.UserName;
            User.Type = _userType;
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        });

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
            UserType = (UserType)User?.Type;
            _mode = parameters.GetValue<string>("Mode");
            aggregator.SendMessage("", nameof(EditUserViewModel), User);
        }
    }
}
