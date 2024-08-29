using HandyControl.Controls;
using HandyControl.Data;
using Newtonsoft.Json;
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    internal class UserViewModel : NavigationViewModel, IDialogAware
    {
        private readonly IDialogService dialogService;

        public string Title => AppSession.CompanyName;

        public event Action<IDialogResult> RequestClose;

        private User CurrentUser;

        private ObservableCollection<User> _users = new ObservableCollection<User>();
        public ObservableCollection<User> Users { get { return _users; } set { SetProperty(ref _users, value); } }

        private ObservableCollection<User> _userCol = new ObservableCollection<User>();
        public ObservableCollection<User> UserCol { get { return _userCol; } set { SetProperty(ref _userCol, value); } }

        private int _pageCount = 10;
        public int PageCount
        {
            get { return _pageCount; }
            set { SetProperty(ref _pageCount, value); }
        }
        private int _pageIndex;
        public int PageIndex
        {
            get { return _pageIndex; }
            set { SetProperty(ref _pageIndex, value); }
        }

        private int _dataCountPerPage = 10;
        public int DataCountPerPage
        {
            get { return _dataCountPerPage; }
            set { SetProperty(ref _dataCountPerPage, value); }
        }

        public DelegateCommand CloseCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.OK)));

        public DelegateCommand AddUserCommand => new(() =>
        {
            User user = new User();
            DialogParameters pairs = new DialogParameters
            {
                { "User", user },
                {"Mode", "Add"  }
            };
            dialogService?.ShowDialog("EditUserView", pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                Users.Add(user);
                UserManager.GetInstance().Save(Users);
                PageUpdated(new FunctionEventArgs<int>(PageIndex));
            });
        });

        public DelegateCommand<User> EditCommand => new((User user) =>
        {
            DialogParameters pairs = new DialogParameters
            {
                { "User", user },
                {"Mode", "Add"  }
            };
            dialogService?.ShowDialog("EditUserView", pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                UserManager.GetInstance().Save(Users);
            });
        });

        public DelegateCommand<User> DeleteCommand => new((User user) =>
        {
            UserCol.Remove(user);
            Users.Remove(user);
            UserManager.GetInstance().Save(Users);
        });

        public DelegateCommand<FunctionEventArgs<string>> SearchCommand => new((FunctionEventArgs<string> e) =>
        {
            string key = e.Info;
            if (string.IsNullOrEmpty(key))
            {
                Users = new ObservableCollection<User>(UserManager.GetInstance().Users);
            }
            else
            {
                var collection = Users.Where(t => t.UserName.Contains(key) || t.Creator.Contains(key) || t.Type.ToString().Contains(key));
                Users = new ObservableCollection<User>(collection);
            }
            if (PageIndex != 1)
            {
                PageIndex = 1;
            }
            else
            {
                var data = Users.Take(DataCountPerPage);
                UserCol = new ObservableCollection<User>(data);
            }
        });

        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand => new(PageUpdated);

        public UserViewModel(IContainerProvider containerProvider, IDialogService dialog) : base(containerProvider)
        {
            PageIndex = 1;
            dialogService = dialog;
            Users = new ObservableCollection<User>(UserManager.GetInstance().Users);
            PageCount = Users.Count / DataCountPerPage + (Users.Count % DataCountPerPage != 0 ? 1 : 0);
            var data = Users.Take(DataCountPerPage);
            UserCol = new ObservableCollection<User>(data);
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

        private void PageUpdated(FunctionEventArgs<int> info)
        {
            var data = Users.Skip((info.Info - 1) * DataCountPerPage).Take(DataCountPerPage);
            UserCol = new ObservableCollection<User>(data);
        }
    }
}
