using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class AirParam : BindableBase
    {
        private float _air_PV = 0f;
        public float Air_PV
        {
            get { return _air_PV; }
            set { SetProperty(ref _air_PV, value); }
        }

        private float _dBand = 0f;
        public float DBand
        {
            get { return _dBand; }
            set { SetProperty(ref _dBand, value); }
        }

        private float _alarmLowerLimit;
        public float AlarmLowerLimit
        {
            get { return _alarmLowerLimit; }
            set { SetProperty(ref _alarmLowerLimit, value); }
        }

        private float _alarmUpperLimit;
        public float AlarmUpperLimit
        {
            get { return _alarmUpperLimit; }
            set { SetProperty(ref _alarmUpperLimit, value); }
        }

        private ControlMode _controlMode = ControlMode.Fixed;
        public ControlMode ControlMode
        {
            get { return _controlMode; }
            set { SetProperty(ref _controlMode, value); }
        }

        private float _lowerLimit;
        public float LowerLimit
        {
            get { return _lowerLimit; }
            set { SetProperty(ref _lowerLimit, value); }
        }

        private float _upperLimit;
        public float UpperLimit
        {
            get { return _upperLimit; }
            set { SetProperty(ref _upperLimit, value); }
        }
    }
}
