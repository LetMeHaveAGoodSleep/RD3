using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class O2Param : BindableBase
    {
        private string _o2_PV = "-";
        public string O2_PV
        {
            get { return _o2_PV; }
            set { SetProperty(ref _o2_PV, value); }
        }

        private ControlMode _controlMode = ControlMode.Fixed;
        public ControlMode ControlMode
        {
            get { return _controlMode; }
            set { SetProperty(ref _controlMode, value); }
        }
    }
}
