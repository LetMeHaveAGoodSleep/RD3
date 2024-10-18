using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class ExperimentParamTemplate : BindableBase
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

        private TempParam _tempParam = new();
        public TempParam TempParam
        {
            get { return _tempParam; }
            set { SetProperty(ref _tempParam, value); }
        }

        private PHParam _pHParam = new();
        public PHParam PHParam
        {
            get { return _pHParam; }
            set { SetProperty(ref _pHParam, value); }
        }

        private DOParam _dOParam = new();
        public DOParam DOParam
        {
            get { return _dOParam; }
            set { SetProperty(ref _dOParam, value); }
        }

        private AgitParam _agitParam = new();
        public AgitParam AgitParam
        {
            get { return _agitParam; }
            set { SetProperty(ref _agitParam, value); }
        }

        private BaseParam _baseParam = new();
        public BaseParam BaseParam
        {
            get { return _baseParam; }
            set { SetProperty(ref _baseParam, value); }
        }

        private AcidParam _acidParam = new();
        public AcidParam AcidParam
        {
            get { return _acidParam; }
            set { SetProperty(ref _acidParam, value); }
        }

        private AFParam _aFParam = new();
        public AFParam AFParam
        {
            get { return _aFParam; }
            set { SetProperty(ref _aFParam, value); }
        }

        private FeedParam _feedParam = new();
        public FeedParam FeedParam
        {
            get { return _feedParam; }
            set { SetProperty(ref _feedParam, value); }
        }

        private AirParam _airParam = new();
        public AirParam AirParam
        {
            get { return _airParam; }
            set { SetProperty(ref _airParam, value); }
        }

        private CO2Param _cO2Param = new();
        public CO2Param CO2Param
        {
            get { return _cO2Param; }
            set { SetProperty(ref _cO2Param, value); }
        }

        private O2Param _o2Param = new();
        public O2Param O2Param
        {
            get { return _o2Param; }
            set { SetProperty(ref _o2Param, value); }
        }

        private N2Param _n2Param = new();
        public N2Param N2Param
        {
            get { return _n2Param; }
            set { SetProperty(ref _n2Param, value); }
        }
    }
}
