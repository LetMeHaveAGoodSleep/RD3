using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class GasParam : BasicParam
    {
        private int _mfcNo = -1;
        public int MFCNo
        {
            get => _mfcNo;
            set { SetProperty(ref _mfcNo, value); }
        }

        private GasType _gasType = GasType.Air;
        public GasType GasType
        {
            get => _gasType;
            set { SetProperty(ref _gasType, value); }
        }

        private float _flowSpeed;
        public float FlowSpeed
        {
            get => _flowSpeed;
            set 
            { 
                SetProperty(ref _flowSpeed, value);
                SP = value;
            }
        }
    }
}
