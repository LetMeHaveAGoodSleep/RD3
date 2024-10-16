using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class PHParam : BindableBase
    {
        private string _pH_PV = "-";
        public string PH_PV
        {
            get { return _pH_PV; }
            set { SetProperty(ref _pH_PV, value); }
        }

        private ControlMode _controlMode = ControlMode.Fixed;
        public ControlMode ControlMode
        {
            get { return _controlMode; }
            set { SetProperty(ref _controlMode, value); }
        }
    }
}
