using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared.Role
{
    public class RoleFunciton
    {
        private uint _role;
        public uint Role
        {
            get { return (uint)_role; }
        }

        private UserType _type;
        public UserType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }
    }
}
