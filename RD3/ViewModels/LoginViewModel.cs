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

namespace RD3.ViewModels
{
    public class LoginViewModel : BindableBase, IDialogAware
    {
        public LoginViewModel(IEventAggregator aggregator)
        {
            UserDto = new ResgiterUserDto();
            ExecuteCommand = new DelegateCommand<string>(Execute);
            this.aggregator = aggregator;
            Init();
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
        private readonly IEventAggregator aggregator;

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

            if ((bool)AppSession.Users?.ToList().Exists(x => x.UserName == userName && x.Password == passWord))
            {
                AppSession.CurrentUser = AppSession.Users?.ToList().Find(x => x.UserName == userName);
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
                return;
            }

            aggregator.SendMessage("用户名或密码错误", "Login");
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

        public void Init()
        {
            if (!Directory.Exists(FileConst.ConfigDirectory))
            {
                Directory.CreateDirectory(FileConst.ConfigDirectory);
            }
            if (!File.Exists(FileConst.UserPath))
            {
                CreateJsonFile(FileConst.UserPath);
            }
            ReadJsonFile(FileConst.UserPath);
        }

        private void CreateJsonFile(string filePath)
        {
            User user = new User
            {
                UserName = "Admin",
                Password = "123456",
                Role = int.MaxValue
            };
            List<User> Users = new List<User>() { user };
            string json = JsonConvert.SerializeObject(Users);
            json = AESEncryption.Encrypt(json);
            File.WriteAllText(filePath, json);
        }

        private void ReadJsonFile(string filePath)
        {
            string jsonContent = AESEncryption.DecryptFile(filePath);
            List<User> users = JsonConvert.DeserializeObject<List<User>>(jsonContent);
            AppSession.Users.Clear();
            AppSession.Users.AddRange(users);
        }

        #endregion
    }
}
