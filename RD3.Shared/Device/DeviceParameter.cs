using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class DeviceParameter : BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private WorkStatus _status;
        public WorkStatus Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        private float _temp;
        public float Temp
        {
            get { return _temp; }
            set { SetProperty(ref _temp, value); }
        }

        private float _pH;
        public float PH
        {
            get { return _pH; }
            set { SetProperty(ref _pH, value); }
        }

        private float _dO;
        public float DO
        {
            get { return _dO; }
            set { SetProperty(ref _dO, value); }
        }

        private float _agit;
        public float Agit
        {
            get { return _agit; }
            set { SetProperty(ref _agit, value); }
        }

        private float _base;
        public float Base
        {
            get { return _base; }
            set { SetProperty(ref _base, value); }
        }

        private float _acid;
        public float Acid
        {
            get { return _acid; }
            set { SetProperty(ref _acid, value); }
        }

        private float _aF;
        public float AF
        {
            get { return _aF; }
            set { SetProperty(ref _aF, value); }
        }

        private float _feed;
        public float Feed
        {
            get { return _feed; }
            set { SetProperty(ref _feed, value); }
        }
    }
}
