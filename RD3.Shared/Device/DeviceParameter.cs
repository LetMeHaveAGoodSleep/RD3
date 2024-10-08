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
        const string format = "F2";

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private WorkStatus _status = WorkStatus.Idle;
        public WorkStatus Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        private float _temp = 0f;
        public float Temp
        {
            get { return _temp; }
            set { SetProperty(ref _temp, value); }
        }

        private string _temp_PV = "-";
        public string Temp_PV
        {
            get { return _temp_PV; }
            set { SetProperty(ref _temp_PV, value); }
        }

        private float _pH = 0f;
        public float PH
        {
            get { return _pH; }
            set { SetProperty(ref _pH, value); }
        }

        private string _pH_PV = "-";
        public string PH_PV
        {
            get { return _pH_PV; }
            set { SetProperty(ref _pH_PV, value); }
        }

        private float _dO = 0f;
        public float DO
        {
            get { return _dO; }
            set { SetProperty(ref _dO, value); }
        }

        private string _dO_PV = "-";
        public string DO_PV 
        {
            get { return _dO_PV; }
            set { SetProperty(ref _dO_PV, value); }
        }

        private float _agit = 0f;
        public float Agit
        {
            get { return _agit; }
            set { SetProperty(ref _agit, value); }
        }

        private string _agit_PV = "-";
        public string Agit_PV
        {
            get { return _agit_PV; }
            set { SetProperty(ref _agit_PV, value); }
        }

        private float _base = 0f;
        public float Base
        {
            get { return _base; }
            set { SetProperty(ref _base, value); }
        }

        private string _base_PV = "-";
        public string Base_PV
        {
            get { return _base_PV; }
            set { SetProperty(ref _base_PV, value); }
        }

        private float _acid = 0f;
        public float Acid
        {
            get { return _acid; }
            set { SetProperty(ref _acid, value); }
        }

        private string _acid_PV = "-";
        public string Acid_PV
        {
            get { return _acid_PV; }
            set { SetProperty(ref _acid_PV, value); }
        }

        private float _aF = 0f;
        public float AF
        {
            get { return _aF; }
            set { SetProperty(ref _aF, value); }
        }

        private string _aF_PV = "-";
        public string AF_PV
        {
            get { return _aF_PV; }
            set { SetProperty(ref _aF_PV, value); }
        }

        private float _feed = 0f;
        public float Feed
        {
            get { return _feed; }
            set { SetProperty(ref _feed, value); }
        }

        private string _feed_PV = "-";
        public string Feed_PV
        {
            get { return _feed_PV; }
            set { SetProperty(ref _feed_PV, value); }
        }

        private float _air = 0f;
        public float Air
        {
            get { return _air; }
            set { SetProperty(ref _air, value); }
        }

        private string _air_PV = "-";
        public string Air_PV
        {
            get { return _air_PV; }
            set { SetProperty(ref _air_PV, value); }
        }

        private float _cO2 = 0f;
        public float CO2
        {
            get { return _cO2; }
            set { SetProperty(ref _cO2, value); }
        }

        private string _cO2_PV = "-";
        public string CO2_PV
        {
            get { return _cO2_PV; }
            set { SetProperty(ref _cO2_PV, value); }
        }

        private float _o2 = 0f;
        public float O2
        {
            get { return _o2; }
            set { SetProperty(ref _o2, value); }
        }

        private string _o2_PV = "-";
        public string O2_PV
        {
            get { return _o2_PV; }
            set { SetProperty(ref _o2_PV, value); }
        }

        private float _n2 = 0f;
        public float N2
        {
            get { return _n2; }
            set { SetProperty(ref _n2, value); }
        }

        private string _n2_PV = "-";
        public string N2_PV
        {
            get { return _n2_PV; }
            set { SetProperty(ref _n2_PV, value); }
        }

        private float _sample = 0f;
        public float Sample
        {
            get { return _sample; }
            set { SetProperty(ref _sample, value); }
        }

        private string _sample_PV = "-";
        public string Sample_PV
        {
            get { return _sample_PV; }
            set { SetProperty(ref _sample_PV, value); }
        }

        private float _liquid = 0f;
        public float Liquid
        {
            get { return _liquid; }
            set { SetProperty(ref _liquid, value); }
        }

        private string _liquid_PV = "-";
        public string Liquid_PV
        {
            get { return _liquid_PV; }
            set { SetProperty(ref _liquid_PV, value); }
        }

        private float _inoculate = 0f;
        public float Inoculate
        {
            get { return _inoculate; }
            set { SetProperty(ref _inoculate, value); }
        }

        private string _inoculate_PV = "-";
        public string Inoculate_PV
        {
            get { return _inoculate_PV; }
            set { SetProperty(ref _inoculate_PV, value); }
        }

        private float _harvest = 0f;
        public float Harvest
        {
            get { return _harvest; }
            set { SetProperty(ref _harvest, value); }
        }

        private string _harvest_PV = "-";
        public string Harvest_PV
        {
            get { return _harvest_PV; }
            set { SetProperty(ref _harvest_PV, value); }
        }
    }
}
