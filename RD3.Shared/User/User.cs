using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RD3.Shared
{
    public class User : BindableBase
    {
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }

        private string _password = string.Empty;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public uint Role
        {
            get { return (uint)_type; }
        }

        public DateTime Createtime { get; set; }

        public string Creator { get; set; }

        private UserType _type;
        public UserType Type
        {
            get { return _type; }
            set
            {
                SetProperty(ref _type, value);
            }
        }
        private string _typeName;
        public string TypeName
        {
            get { return _typeName; }
            set { SetProperty(ref _typeName, value); }
        }
    }

    [TypeConverter(typeof(DisplayEnumConverter))]
    public enum UserType
    {
        [Display(Name = "User")]
        User,
        [Display(Name = "Service")]
        Service,
        [Display(Name = "Factory")]
        Factory,
        [Display(Name = "Admin")]
        Admin
    }
}
