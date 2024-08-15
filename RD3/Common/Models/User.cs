using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Common
{
    public class User : BindableBase
    {
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        /// <summary>
        /// 权限等级
        /// 数值越大，权限越大
        /// </summary>
        private int _role;
        public int Role
        {
            get { return _role; }
            set { SetProperty(ref _role, value); }
        }

        public DateTime Createtime { get; set; }

        public String Creator { get; set; }

        private UserType _type;
        public UserType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }
    }

    public enum UserType
    {
        Admin,
        Factory,
        Service,
        User
    }
}
