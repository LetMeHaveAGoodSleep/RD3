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

        private float _temp = 0f;
        public float Temp
        {
            get { return _temp; }
            set { SetProperty(ref _temp, value); }
        }

        private TempParam _tempParam = new();
        public TempParam TempParam
        {
            get { return _tempParam; }
            set { SetProperty(ref _tempParam, value); }
        }
        
        private float _pH = 0f;
        public float PH
        {
            get { return _pH; }
            set { SetProperty(ref _pH, value); }
        }

        private PHParam _pHParam = new();
        public PHParam PHParam
        {
            get { return _pHParam; }
            set { SetProperty(ref _pHParam, value); }
        }

        private float _dO = 0f;
        public float DO
        {
            get { return _dO; }
            set { SetProperty(ref _dO, value); }
        }

        private DOParam _dOParam = new();
        public DOParam DOParam
        {
            get { return _dOParam; }
            set { SetProperty(ref _dOParam, value); }
        }

        private float _agit = 0f;
        public float Agit
        {
            get { return _agit; }
            set { SetProperty(ref _agit, value); }
        }

        private AgitParam _agitParam = new();
        public AgitParam AgitParam
        {
            get { return _agitParam; }
            set { SetProperty(ref _agitParam, value); }
        }

        private float _base = 0f;
        public float Base
        {
            get { return _base; }
            set { SetProperty(ref _base, value); }
        }

        private BaseParam _baseParam = new();
        public BaseParam BaseParam
        {
            get { return _baseParam; }
            set { SetProperty(ref _baseParam, value); }
        }

        private float _acid = 0f;
        public float Acid
        {
            get { return _acid; }
            set { SetProperty(ref _acid, value); }
        }

        private AcidParam _acidParam = new();
        public AcidParam AcidParam
        {
            get { return _acidParam; }
            set { SetProperty(ref _acidParam, value); }
        }

        private float _aF = 0f;
        public float AF
        {
            get { return _aF; }
            set { SetProperty(ref _aF, value); }
        }

        private AFParam _aFParam = new();
        public AFParam AFParam
        {
            get { return _aFParam; }
            set { SetProperty(ref _aFParam, value); }
        }

        private float _feed = 0f;
        public float Feed
        {
            get { return _feed; }
            set { SetProperty(ref _feed, value); }
        }

        private FeedParam _feedParam = new();
        public FeedParam FeedParam
        {
            get { return _feedParam; }
            set { SetProperty(ref _feedParam, value); }
        }
        

        private float _air = 0f;
        public float Air
        {
            get { return _air; }
            set { SetProperty(ref _air, value); }
        }

        private AirParam _airParam = new();
        public AirParam AirParam
        {
            get { return _airParam; }
            set { SetProperty(ref _airParam, value); }
        }

        private float _cO2 = 0f;
        public float CO2
        {
            get { return _cO2; }
            set { SetProperty(ref _cO2, value); }
        }

        private CO2Param _cO2Param = new();
        public CO2Param CO2Param
        {
            get { return _cO2Param; }
            set { SetProperty(ref _cO2Param, value); }
        }

        private float _o2 = 0f;
        public float O2
        {
            get { return _o2; }
            set { SetProperty(ref _o2, value); }
        }

        private O2Param _o2Param = new();
        public O2Param O2Param
        {
            get { return _o2Param; }
            set { SetProperty(ref _o2Param, value); }
        }

        private float _n2 = 0f;
        public float N2
        {
            get { return _n2; }
            set { SetProperty(ref _n2, value); }
        }

        private N2Param _n2Param = new();
        public N2Param N2Param
        {
            get { return _n2Param; }
            set { SetProperty(ref _n2Param, value); }
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
