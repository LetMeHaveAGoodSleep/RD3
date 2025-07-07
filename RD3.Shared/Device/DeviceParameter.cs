using Fpi.Util.Sundry;
using MathNet.Symbolics;
using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
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

        private string _id;
        [JsonIgnore]
        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
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

        //private float _temp = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float Temp
        //{
        //    get { return _temp; }
        //    set { SetProperty(ref _temp, value); }
        //}

        private TempParam _tempParam = new();
        public TempParam TempParam
        {
            get { return _tempParam; }
            set { SetProperty(ref _tempParam, value); }
        }

        //private float _pH = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float PH
        //{
        //    get { return _pH; }
        //    set { SetProperty(ref _pH, value); }
        //}

        private PHParam _pHParam = new();
        public PHParam PHParam
        {
            get { return _pHParam; }
            set { SetProperty(ref _pHParam, value); }
        }

        //private float _dO = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float DO
        //{
        //    get { return _dO; }
        //    set { SetProperty(ref _dO, value); }
        //}

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

        //private int _agit = 0;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public int Agit
        //{
        //    get { return _agit; }
        //    set { SetProperty(ref _agit, value); }
        //}

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

        private float _pump1FlowSpeed = 0f;
        [JsonIgnore]
        [ReflectionAttribute("Pump1FlowRate")]
        public float Pump1FlowSpeed
        {
            get { return _pump1FlowSpeed; }
            set { SetProperty(ref _pump1FlowSpeed, value); }
        }

        //private float _pump1Flow = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float Pump1Flow
        //{
        //    get { return _pump1Flow; }
        //    set { SetProperty(ref _pump1Flow, value); }
        //}

        //private float _pump1FlowCapacity = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float Pump1FlowCapacity
        //{
        //    get { return _pump1FlowCapacity; }
        //    set { SetProperty(ref _pump1FlowCapacity, value); }
        //}

        private float _pump2FlowSpeed = 0f;
        [JsonIgnore]
        [ReflectionAttribute("Pump2FlowRate")]
        public float Pump2FlowSpeed
        {
            get { return _pump2FlowSpeed; }
            set { SetProperty(ref _pump2FlowSpeed, value); }
        }

        //private float _pump2Flow = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float Pump2Flow
        //{
        //    get { return _pump2Flow; }
        //    set { SetProperty(ref _pump2Flow, value); }
        //}

        //private float _pump2FlowCapacity = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float Pump2FlowCapacity
        //{
        //    get { return _pump2FlowCapacity; }
        //    set { SetProperty(ref _pump2FlowCapacity, value); }
        //}

        private float _pump3FlowSpeed = 0f;
        [JsonIgnore]
        [ReflectionAttribute("Pump3FlowRate")]
        public float Pump3FlowSpeed
        {
            get { return _pump3FlowSpeed; }
            set { SetProperty(ref _pump3FlowSpeed, value); }
        }

        //private float _pump3Flow = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float Pump3Flow
        //{
        //    get { return _pump3Flow; }
        //    set { SetProperty(ref _pump3Flow, value); }
        //}

        //private float _pump3FlowCapacity = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float Pump3FlowCapacity
        //{
        //    get { return _pump3FlowCapacity; }
        //    set { SetProperty(ref _pump3FlowCapacity, value); }
        //}

        private float _pump4FlowSpeed = 0f;
        [JsonIgnore]
        [ReflectionAttribute("Pump4FlowRate")]
        public float Pump4FlowSpeed
        {
            get { return _pump4FlowSpeed; }
            set { SetProperty(ref _pump4FlowSpeed, value); }
        }

        //private float _pump4Flow = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float Pump4Flow
        //{
        //    get { return _pump4Flow; }
        //    set { SetProperty(ref _pump4Flow, value); }
        //}

        //private float _pump4FlowCapacity = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float Pump4FlowCapacity
        //{
        //    get { return _pump4FlowCapacity; }
        //    set { SetProperty(ref _pump4FlowCapacity, value); }
        //}

        private float _pump5FlowSpeed = 0f;
        [JsonIgnore]
        [ReflectionAttribute("Pump5FlowRate")]
        public float Pump5FlowSpeed
        {
            get { return _pump5FlowSpeed; }
            set { SetProperty(ref _pump5FlowSpeed, value); }
        }

        //private float _pump5Flow = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float Pump5Flow
        //{
        //    get { return _pump5Flow; }
        //    set { SetProperty(ref _pump5Flow, value); }
        //}

        //private float _pump5FlowCapacity = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float Pump5FlowCapacity
        //{
        //    get { return _pump5FlowCapacity; }
        //    set { SetProperty(ref _pump5FlowCapacity, value); }
        //}

        private float _pump6FlowSpeed = 0f;
        [JsonIgnore]
        [ReflectionAttribute("Pump6FlowRate")]
        public float Pump6FlowSpeed
        {
            get { return _pump6FlowSpeed; }
            set { SetProperty(ref _pump6FlowSpeed, value); }
        }

        //private float _pump6FlowCapacity = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float Pump6FlowCapacity
        //{
        //    get { return _pump6FlowCapacity; }
        //    set { SetProperty(ref _pump6FlowCapacity, value); }
        //}

        private float _mfc1FlowSpeed = 0f;
        [JsonIgnore]
        [ReflectionAttribute("MFC1FlowRate")]
        public float MFC1FlowSpeed
        {
            get { return _mfc1FlowSpeed; }
            set { SetProperty(ref _mfc1FlowSpeed, value); }
        }

        //private float _mfc1FlowCapacity = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float MFC1FlowCapacity
        //{
        //    get { return _mfc1FlowCapacity; }
        //    set { SetProperty(ref _mfc1FlowCapacity, value); }
        //}

        private float _mfc2FlowSpeed = 0f;
        [JsonIgnore]
        [ReflectionAttribute("MFC2FlowRate")]
        public float MFC2FlowSpeed
        {
            get { return _mfc2FlowSpeed; }
            set { SetProperty(ref _mfc2FlowSpeed, value); }
        }

        //private float _mfc2FlowCapacity = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float MFC2FlowCapacity
        //{
        //    get { return _mfc2FlowCapacity; }
        //    set { SetProperty(ref _mfc2FlowCapacity, value); }
        //}

        private float _mfc3FlowSpeed = 0f;
        [JsonIgnore]
        [ReflectionAttribute("MFC3FlowRate")]
        public float MFC3FlowSpeed
        {
            get { return _mfc3FlowSpeed; }
            set { SetProperty(ref _mfc3FlowSpeed, value); }
        }

        //private float _mfc3FlowCapacity = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float MFC3FlowCapacity
        //{
        //    get { return _mfc3FlowCapacity; }
        //    set { SetProperty(ref _mfc3FlowCapacity, value); }
        //}

        private float _mfc4FlowSpeed = 0f;
        [JsonIgnore]
        [ReflectionAttribute("MFC4FlowRate")]
        public float MFC4FlowSpeed
        {
            get { return _mfc4FlowSpeed; }
            set { SetProperty(ref _mfc4FlowSpeed, value); }
        }

        //private float _mfc4FlowCapacity = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float MFC4FlowCapacity
        //{
        //    get { return _mfc4FlowCapacity; }
        //    set { SetProperty(ref _mfc4FlowCapacity, value); }
        //}

        private float _mfc5FlowSpeed = 0f;
        [JsonIgnore]
        [ReflectionAttribute("MFC5FlowRate")]
        public float MFC5FlowSpeed
        {
            get { return _mfc5FlowSpeed; }
            set { SetProperty(ref _mfc5FlowSpeed, value); }
        }

        //private float _mfc5FlowCapacity = 0f;
        //[JsonIgnore]
        //[ReflectionAttribute]
        //public float MFC5FlowCapacity
        //{
        //    get { return _mfc5FlowCapacity; }
        //    set { SetProperty(ref _mfc5FlowCapacity, value); }
        //}

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

        private bool _feedDOAssociated = false;
        public bool FeedDOAssociated
        {
            get => _feedDOAssociated;
            set { SetProperty(ref _feedDOAssociated, value); }
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

        private bool _tempDOAssociated = false;
        public bool TempDOAssociated
        {
            get => _tempDOAssociated;
            set { SetProperty(ref _tempDOAssociated, value); }
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

        private float _kalmanConstant = 0f;
        public float KalmanConstant
        {
            get => _kalmanConstant;
            set { SetProperty(ref _kalmanConstant, value); }
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
