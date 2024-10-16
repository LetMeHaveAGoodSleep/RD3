using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class CO2Param : BindableBase
    {
        private string _cO2_PV = "-";
        public string CO2_PV
        {
            get { return _cO2_PV; }
            set { SetProperty(ref _cO2_PV, value); }
        }

        private ControlMode _controlMode = ControlMode.Fixed;
        public ControlMode ControlMode
        {
            get { return _controlMode; }
            set { SetProperty(ref _controlMode, value); }
        }
    }
}
