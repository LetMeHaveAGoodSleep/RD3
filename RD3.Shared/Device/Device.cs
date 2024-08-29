using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class Device : BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private WorkStatus _status;
        public WorkStatus Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        private string _iPAdress;
        public string IPAdress
        {
            get { return _iPAdress; }
            set { SetProperty(ref _iPAdress, value); }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { SetProperty(ref _port, value); }
        }

        private bool _connected;
        public bool Connected
        {
            get { return _connected; }
            set { SetProperty(ref _connected, value); }
        }
    }
}
