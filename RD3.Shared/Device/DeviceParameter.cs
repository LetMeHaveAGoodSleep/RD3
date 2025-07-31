using Fpi.Util.Sundry;
using MathNet.Symbolics;
using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RD3.Shared
{
    public class DeviceParameter : RealTimeParam, ICloneable
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private int _serialNumber;
        public int SerialNumber
        {
            get { return _serialNumber; }
            set { SetProperty(ref _serialNumber, value); }
        }

        private ReactorStatus _reactorStatus = ReactorStatus.DisConnected;
        [JsonIgnore]
        public ReactorStatus ReactorStatus
        {
            get { return _reactorStatus; }
            set
            {
                switch (value)
                {
                    case ReactorStatus.DisConnected:
                        ReactorStatusName = "未连接";
                        break;
                    case ReactorStatus.Connected:
                        ReactorStatusName = "已连接";
                        break;
                    case ReactorStatus.Simulated:
                        ReactorStatusName = "虚拟模式";
                        break;
                }
                SetProperty(ref _reactorStatus, value);
            }
        }

        private string _reactorStatusName;
        [JsonIgnore]
        public string ReactorStatusName
        {
            get => _reactorStatusName;
            private set { SetProperty(ref _reactorStatusName, value); }
        }

        private string _ip;
        [JsonIgnore]
        public string Ip
        {
            get => _ip;
            set { SetProperty(ref _ip, value); }
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

        private bool _isDOLimit = false;
        [JsonIgnore]
        public bool IsDOLimit
        {
            get { return _isDOLimit; }
            set { SetProperty(ref _isDOLimit, value); }
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

        private GasParam _airParam = new();
        public GasParam AirParam
        {
            get { return _airParam; }
            set { SetProperty(ref _airParam, value); }
        }

        private GasParam _cO2Param = new();
        public GasParam CO2Param
        {
            get { return _cO2Param; }
            set { SetProperty(ref _cO2Param, value); }
        }

        private GasParam _o2Param = new();
        public GasParam O2Param
        {
            get { return _o2Param; }
            set { SetProperty(ref _o2Param, value); }
        }

        private GasParam _n2Param = new();
        public GasParam N2Param
        {
            get { return _n2Param; }
            set { SetProperty(ref _n2Param, value); }
        }

        private PumpMFCSetting _pumpMFCSetting = new();
        [JsonIgnore]
        public PumpMFCSetting PumpMFCSetting
        {
            get => _pumpMFCSetting;
            set { SetProperty(ref _pumpMFCSetting, value); }
        }

        DefoamingSetting _defoamingSetting = new DefoamingSetting();
        public DefoamingSetting DefoamingSetting
        {
            get => _defoamingSetting;
            set { SetProperty(ref _defoamingSetting, value); }
        }

        FeedParam _feedParam1 = new FeedParam() { Index = 1 };
        public FeedParam FeedParam1
        {
            get => _feedParam1;
            set { SetProperty(ref _feedParam1, value); }
        }

        FeedParam _feedParam2 = new FeedParam() { Index = 2 };
        public FeedParam FeedParam2
        {
            get => _feedParam2;
            set { SetProperty(ref _feedParam2, value); }
        }

        private bool _doFilterEnable = false;
        public bool DOFilterEnable
        {
            get => _doFilterEnable;
            set { SetProperty(ref _doFilterEnable, value); }
        }

        private bool _doRegulationLimit = false;
        [JsonIgnore]
        public bool DORegulationLimit
        {
            get => _doRegulationLimit;
            set { SetProperty(ref _doRegulationLimit, value); }
        }

        private bool _feedSuspend = false;
        /// <summary>
        /// 如果DO调控时，需要降低补料，则为true；否则为false
        /// </summary>
        [JsonIgnore]
        public bool FeedSuspend
        {
            get => _feedSuspend;
            set { SetProperty(ref _feedSuspend, value); }
        }

        private float _tempDOLowerLimit = 25;
        public float TempDOLowerLimit
        {
            get => _tempDOLowerLimit;
            set { SetProperty(ref _tempDOLowerLimit, value); }
        }

        private float _feedDOLowerLimit = 0;
        public float FeedDOLowerLimit
        {
            get => _feedDOLowerLimit;
            set { SetProperty(ref _feedDOLowerLimit, value); }
        }

        private float _agitSampleCycle = 1f;
        public float AgitSampleCycle
        {
            get => _agitSampleCycle;
            set 
            {
                SetProperty(ref _agitSampleCycle, value);
            }
        }

        private float _agitSampleFrequency = Convert.ToSingle(1.0f / (2 * Math.PI * Math.Max(1, 0.001)));
        public float AgitSampleFrequency
        {
            get => _agitSampleFrequency;
            set 
            {
                AgitTimeConstant = Convert.ToSingle(1.0f / (2 * Math.PI * Math.Max(value, 0.001)));
                SetProperty(ref _agitSampleFrequency, value);
            }
        }

        private float _agitTimeConstant = 1f;
        public float AgitTimeConstant
        {
            get => _agitTimeConstant;
            private set { SetProperty(ref _agitTimeConstant, value); }
        }

        private AdaptivepHParameter _adaptivepHParameter = new AdaptivepHParameter();
        public AdaptivepHParameter AdaptivepHParameter
        {
            get => _adaptivepHParameter;
            private set { SetProperty(ref _adaptivepHParameter, value); }
        }

        public object Clone()
        {
            var clonedObject = ObjectCloner.DeepCopy(this);
            return clonedObject;
        }
    }
}
