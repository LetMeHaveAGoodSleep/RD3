using HandyControl.Tools.Extension;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Extensions;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class RegisterViewModel : BaseViewModel, IDialogAware
    {
        private string _registrationCode;
        public string RegistrationCode
        {
            get => _registrationCode;
            set { SetProperty(ref _registrationCode, value); }
        }

        public DelegateCommand RegisterCommand => new(async () => 
        {
            if (string.IsNullOrEmpty(_registrationCode)) 
            {
                await DialogHostService.Info("温馨提示", "请输入注册码!", "Register");
                return;
            }
            if (!AESEncryption.IsEncrypt(_registrationCode))
            {
                await DialogHostService.Info("温馨提示", "请输入正确的注册码!", "Register");
                return;
            }
            string code = AESEncryption.Decrypt(_registrationCode);
            //Todo 仪器码验证，时间验证
            try
            {
                var array = code.Split('_');
                if (array.Length < 4) 
                {
                    await DialogExtensions.Info("温馨提示", "请输入正确的注册码!", "Register");
                    return;
                }
                string insId = array[0];
                DateTime dateTime = Convert.ToDateTime(array[3]);
                if (dateTime < DateTime.Now)
                {
                    await DialogExtensions.Info("温馨提示", "注册码已过期!", "Register");
                    return;
                }
            }
            catch (Exception ex) 
            {
                await DialogExtensions.Info("温馨提示", "请输入正确的注册码!", "Register");
                LogHelper.Error(ex);
                return;
            }
            RegisterManager.SetRegistry(_registrationCode);
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        });

        public DelegateCommand CloseCommand => new(() => 
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        });

        public RegisterViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => AppSession.CompanyName;

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
            RegistrationCode = parameters.GetValue<string>("RegistrationCode");
        }
    }
}
