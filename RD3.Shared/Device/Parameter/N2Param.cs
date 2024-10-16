using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class N2Param : BindableBase
    {
        private string _n2_PV = "-";
        public string N2_PV
        {
            get { return _n2_PV; }
            set { SetProperty(ref _n2_PV, value); }
        }

        private ControlMode _controlMode = ControlMode.Fixed;
        public ControlMode ControlMode
        {
            get { return _controlMode; }
            set { SetProperty(ref _controlMode, value); }
        }
    }
}
