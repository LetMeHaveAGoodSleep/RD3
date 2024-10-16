using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class DOParam : BindableBase
    {
        private string _dO_PV = "-";
        public string DO_PV
        {
            get { return _dO_PV; }
            set { SetProperty(ref _dO_PV, value); }
        }

        private ControlMode _controlMode = ControlMode.Fixed;
        public ControlMode ControlMode
        {
            get { return _controlMode; }
            set { SetProperty(ref _controlMode, value); }
        }
    }
}
