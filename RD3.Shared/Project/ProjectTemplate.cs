using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class ProjectTemplate : BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public DateTime CreatDate { get; set; }

        public string Creator { get; set; }

        private int _usageTime;
        public int UsageTime
        {
            get { return _usageTime; }
            set { SetProperty(ref _usageTime, value); }
        }

        private float _pH;
        public float PH
        {
            get { return _pH; }
            set { SetProperty(ref _pH, value); }
        }

        private float _temp;
        public float Temp
        {
            get { return _temp; }
            set { SetProperty(ref _temp, value); }
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

        private float _air;
        public float Air
        {
            get { return _air; }
            set { SetProperty(ref _air, value); }
        }

        private float _n2;
        public float N2
        {
            get { return _n2; }
            set { SetProperty(ref _n2, value); }
        }

        private float _o2;
        public float O2
        {
            get { return _o2; }
            set { SetProperty(ref _o2, value); }
        }

        private float _cO2;
        public float CO2
        {
            get { return _cO2; }
            set { SetProperty(ref _cO2, value); }
        }
    }
}
