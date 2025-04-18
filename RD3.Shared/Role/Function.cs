using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class Function
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        private string _nameSpace;
        public string NameSpace
        {
            get { return _nameSpace; }
            set { _nameSpace = value; }
        }
        
        private UserType _minUserType;
        public UserType MinUserType
        {
            get { return _minUserType; }
            set { _minUserType = value; }
        }
    }
}
