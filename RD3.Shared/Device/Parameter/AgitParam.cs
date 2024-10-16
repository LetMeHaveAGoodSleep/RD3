using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class AgitParam : BindableBase
    {
        private string _agit_PV = "-";
        public string Agit_PV
        {
            get { return _agit_PV; }
            set { SetProperty(ref _agit_PV, value); }
        }

        private ControlMode _controlMode = ControlMode.Fixed;
        public ControlMode ControlMode
        {
            get { return _controlMode; }
            set { SetProperty(ref _controlMode, value); }
        }
    }
}
