using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class RealTimeParam : BindableBase
    {
        private float _temp = 0f;
        public float Temp
        {
            get { return _temp; }
            set { SetProperty(ref _temp, value); }
        }

        private float _pH = 0f;
        public float PH
        {
            get { return _pH; }
            set { SetProperty(ref _pH, value); }
        }

        private float _dO = 0f;
        public float DO
        {
            get { return _dO; }
            set { SetProperty(ref _dO, value); }
        }

        private float _agit = 0f;
        public float Agit
        {
            get { return _agit; }
            set { SetProperty(ref _agit, value); }
        }

        private float _base = 0f;
        public float Base
        {
            get { return _base; }
            set { SetProperty(ref _base, value); }
        }

        private float _acid = 0f;
        public float Acid
        {
            get { return _acid; }
            set { SetProperty(ref _acid, value); }
        }

        private float _aF = 0f;
        public float AF
        {
            get { return _aF; }
            set { SetProperty(ref _aF, value); }
        }

        private float _feed = 0f;
        public float Feed
        {
            get { return _feed; }
            set { SetProperty(ref _feed, value); }
        }

        private float _air = 0f;
        public float Air
        {
            get { return _air; }
            set { SetProperty(ref _air, value); }
        }

        private float _cO2 = 0f;
        public float CO2
        {
            get { return _cO2; }
            set { SetProperty(ref _cO2, value); }
        }

        private float _o2 = 0f;
        public float O2
        {
            get { return _o2; }
            set { SetProperty(ref _o2, value); }
        }

        private float _n2 = 0f;
        public float N2
        {
            get { return _n2; }
            set { SetProperty(ref _n2, value); }
        }

        private float _inoculate = 0f;
        public float Inoculate
        {
            get { return _inoculate; }
            set { SetProperty(ref _inoculate, value); }
        }

        private WorkStatus  _workStatus;
        public WorkStatus WorkStatus
        {
            get { return _workStatus; }
            set { SetProperty(ref _workStatus, value); }
        }

        private byte[] _alarmBytes;
        public byte[] AlarmBytes
        {
            get { return _alarmBytes; }
            set { SetProperty(ref _alarmBytes, value); }
        }
    }
}
