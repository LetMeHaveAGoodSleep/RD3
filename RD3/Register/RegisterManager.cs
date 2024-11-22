using Microsoft.Win32;
using Prism.Ioc;
using RD3.Common;
using RD3.Extensions;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RD3
{
   public static class RegisterManager
    {
        private static DispatcherTimer _registerTimer;
        public static bool IsRegistered
        {
            get;
            private set;
        } = false;

        public static void MonitorRegister()
        {
            if (_registerTimer != null) return;
            _registerTimer = new DispatcherTimer();
            _registerTimer.Interval = TimeSpan.FromMinutes(5);
            _registerTimer.Tick += (async (s, e) =>
            {
                switch (CheckRegistry())
                {
                    case RegistrationStatus.NoRegister:
                        await DialogExtensions.Info("温馨提示", "请注册软件!");
                        IsRegistered = false;
                        break;
                    case RegistrationStatus.Expired:
                        await DialogExtensions.Info("温馨提示", "注册码已过期!");
                        IsRegistered = false;
                        break;
                }
            });
            _registerTimer.Start();
        }

        public static RegistrationStatus CheckRegistry()
        {
            IsRegistered = false;
            var registration = GetRegistrationCode();
            if (string.IsNullOrEmpty(registration)) return RegistrationStatus.NoRegister;
            var information = AESEncryption.Decrypt(registration.ToString());
            try
            {
                var array = information.Split('_');
                if (array.Length < 4) return RegistrationStatus.NoRegister;
                DateTime resgistrationTime = Convert.ToDateTime(array[3]);
                if (resgistrationTime < DateTime.Now)
                {
                    return RegistrationStatus.Expired;
                }
                else
                {
                    AppSession.ResgistrationTime = resgistrationTime;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                return RegistrationStatus.NoRegister;
            }

            IsRegistered = true;
            return RegistrationStatus.Success;
        }

        public static string GetRegistrationCode()
        {
            string result = string.Empty;
            using (var software = Registry.CurrentUser.OpenSubKey("Software\\Pioreactor"))
            {
                result = software?.GetValue("Registration")?.ToString();
            }
            return result;
        }

        public static void SetRegistry(string code) 
        {
            using (var software = Registry.CurrentUser.OpenSubKey("Software\\Pioreactor"))
            {
                software?.SetValue("Registration",code);
            }
            IsRegistered = true;
        }
    }
}
