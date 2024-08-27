using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RD3.Shared
{
    public class User : BindableBase,ICloneable
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

        public int Role
        {
            get { return (int)_type; }
        }

        public DateTime Createtime { get; set; }

        public string Creator { get; set; }

        private UserType _type;
        public UserType Type
        {
            //get { return (UserType)Enum.Parse(typeof(UserType), TypeName); }
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }
        private string _typeName;
        public string TypeName
        {
            get { return _typeName; }
            set { SetProperty(ref _typeName, value); }
        }

        public object Clone()
        {
            User user = new User()
            {
                UserName = UserName,
                Password = Password,
                Createtime = Createtime,
                Creator = Creator,
                Type = Type,
                TypeName = TypeName
            };
            return user;
        }
    }

    [TypeConverter(typeof(DisplayEnumConverter))]
    public enum UserType
    {
        [Display(Name = "Admin")]
        Admin,
        [Display(Name = "Factory")]
        Factory,
        [Display(Name = "Service")]
        Service,
        [Display(Name = "User")]
        User
    }
}
