using RD3.Extensions;
using RD3.Shared.Dtos;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RD3.Common;
using RD3.Shared;
using System.Windows;
using System.IO;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Prism.Regions;
using Prism.Ioc;
using HandyControl.Controls;
using RD3.Views;

namespace RD3.ViewModels
{
    public class LoginViewModel : BaseViewModel, IDialogAware
    {
        private IDialogService _dialogService;

        public LoginViewModel(IContainerProvider containerProvider, IEventAggregator aggregator, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            UserDto = new ResgiterUserDto();
            ExecuteCommand = new DelegateCommand<string>(Execute);
            _dialogService = containerProvider.Resolve<IDialogService>();
        }

        public string Title { get; set; } = AppSession.CompanyName;

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            LoginOut();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var result = RegisterManager.CheckRegistry();
            switch (result)
            {
                case RegistrationStatus.Success:
                    
                    break;
                default:
                    _dialogService.ShowDialog(nameof(RegisterView), callback =>
                    {
                        if (callback.Result != ButtonResult.OK)
                        {
                            Environment.Exit(0);
                            return;
                        }
                    });
                    break;
            }
        }

        #region Login

        private int selectIndex;

        public int SelectIndex
        {
            get { return selectIndex; }
            set { selectIndex = value; RaisePropertyChanged(); }
        }


        public DelegateCommand<string> ExecuteCommand { get; private set; }


        private string userName;

        public string UserName
        {
            get { return userName; }
            set { userName = value; RaisePropertyChanged(); }
        }

        private string passWord;
        public string PassWord
        {
            get { return passWord; }
            set { passWord = value; RaisePropertyChanged(); }
        }

        private void Execute(string obj)
        {
            switch (obj)
            {
                case "Login": Login(); break;
                case "LoginOut": LoginOut(); break;
                case "Resgiter": Resgiter(); break;
                case "ResgiterPage": SelectIndex = 1; break;
                case "Return": SelectIndex = 0; break;
            }
        }

        private ResgiterUserDto userDto;

        public ResgiterUserDto UserDto
        {
            get { return userDto; }
            set { userDto = value; RaisePropertyChanged(); }
        }

        private void Login()
        {
            if (string.IsNullOrWhiteSpace(UserName) ||
                string.IsNullOrWhiteSpace(PassWord))
            {
                return;
            }
            User user = UserManager.GetInstance().Users?.ToList().Find(t => t.UserName == userName && t.Password == passWord);
            if (user != null)
            {
                AppSession.CurrentUser = user;
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
                return;
            }
            aggregator.SendMessage(Language.GetValue("用户名或密码错误").ToString(), nameof(LoginViewModel));
        }

        private  void Resgiter()
        {
            if (string.IsNullOrWhiteSpace(UserDto.Account) ||
                string.IsNullOrWhiteSpace(UserDto.UserName) ||
                string.IsNullOrWhiteSpace(UserDto.PassWord) ||
                string.IsNullOrWhiteSpace(UserDto.NewPassWord))
            {
                aggregator.SendMessage("请输入完整的注册信息！", "Login");
                return;
            }

            if (UserDto.PassWord != UserDto.NewPassWord)
            {
                aggregator.SendMessage("密码不一致,请重新输入！", "Login");
                return;
            }

            //var resgiterResult = await loginService.Resgiter(new Shared.Dtos.UserDto()
            //{
            //    Account = UserDto.Account,
            //    UserName = UserDto.UserName,
            //    PassWord = UserDto.PassWord
            //});

            //if (resgiterResult != null && resgiterResult.Status)
            //{
            //    aggregator.SendMessage("注册成功", "Login");
            //    //注册成功,返回登录页页面
            //    SelectIndex = 0;
            //}
            //else
            //    aggregator.SendMessage(resgiterResult.Message, "Login");
        }

        void LoginOut()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.No));
        }
        #endregion
    }
}
