using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class AcidParam : BindableBase
    {
        private string _acid_PV = "-";
        public string Acid_PV
        {
            get { return _acid_PV; }
            set { SetProperty(ref _acid_PV, value); }
        }

        private ControlMode _controlMode = ControlMode.Fixed;
        public ControlMode ControlMode
        {
            get { return _controlMode; }
            set { SetProperty(ref _controlMode, value); }
        }

        private bool _isFixed;
        public bool IsFixed
        {
            get { return _isFixed; }
            set { SetProperty(ref _isFixed, value); }
        }
    }
}
