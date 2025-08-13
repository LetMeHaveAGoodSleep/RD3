using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class DOParam : BindableBase, ICloneable
    {
        private float _dO_PV = 0f;
        public float DO_PV
        {
            get { return _dO_PV; }
            set { SetProperty(ref _dO_PV, value); }
        }

        private float _initialFeed = 0f;
        public float InitialFeed
        {
            get => _initialFeed;
            set { SetProperty(ref _initialFeed, value); }
        }

        private float _initialTemp = 0f;
        public float InitialTemp
        {
            get => _initialTemp;
            set { SetProperty(ref _initialTemp, value); }
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

        private ControlMode _controlMode = ControlMode.Enable;
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

        private bool _isDirect = true;
        public bool IsDirect
        {
            get { return _isDirect; }
            set { SetProperty(ref _isDirect, value); }
        }

        private bool _isReverse = true;
        public bool IsReverse
        {
            get { return _isReverse; }
            set { SetProperty(ref _isReverse, value); }
        }

        private bool _lastIsControling = false;
        [JsonIgnore]
        public bool LastIsControling
        {
            get { return _lastIsControling; }
            private set { SetProperty(ref _lastIsControling, value); }
        }

        private bool _isControling = false;
        public bool IsControling
        {
            get { return _isControling; }
            set 
            {
                LastIsControling = _isControling;
                SetProperty(ref _isControling, value); 
            }
        }

        private bool _agitAssociated = true;
        public bool AgitAssociated
        {
            get { return _agitAssociated; }
            set { SetProperty(ref _agitAssociated, value); }
        }

        private bool _airAssociated = true;
        public bool AirAssociated
        {
            get { return _airAssociated; }
            set { SetProperty(ref _airAssociated, value); }
        }

        private bool _o2Associated = true;
        public bool O2Associated
        {
            get { return _o2Associated; }
            set { SetProperty(ref _o2Associated, value); }
        }

        private bool _co2Associated = true;
        public bool CO2Associated
        {
            get { return _co2Associated; }
            set { SetProperty(ref _co2Associated, value); }
        }

        private bool _n2Associated = true;
        public bool N2Associated
        {
            get { return _n2Associated; }
            set { SetProperty(ref _n2Associated, value); }
        }

        private bool _feedAssociated = true;
        public bool FeedAssociated
        {
            get { return _feedAssociated; }
            set { SetProperty(ref _feedAssociated, value); }
        }

        private CycleParam _agitCycle = new();
        public CycleParam AgitCycle
        {
            get { return _agitCycle; }
            set { SetProperty(ref _agitCycle, value); }
        }

        private CycleParam _airCycle = new();
        public CycleParam AirCycle
        {
            get { return _airCycle; }
            set { SetProperty(ref _airCycle, value); }
        }

        private CycleParam _o2Cycle = new();
        public CycleParam O2Cycle
        {
            get { return _o2Cycle; }
            set { SetProperty(ref _o2Cycle, value); }
        }

        private TimeSeries _timeSeries = new TimeSeries();
        public TimeSeries TimeSeries
        {
            get => _timeSeries;
            set
            {
                SetProperty(ref _timeSeries, value);
            }
        }

        private bool _isEmergency = true;
        public bool IsEmergency
        {
            get { return _isEmergency; }
            set { SetProperty(ref _isEmergency, value); }
        }

        private bool _isCycle = false;
        public bool IsCycle
        {
            get { return _isCycle; }
            private set { SetProperty(ref _isCycle, value); }
        }

        private DOControlStrategy _controlStrategy = DOControlStrategy.Step;
        public DOControlStrategy ControlStrategy
        {
            get { return _controlStrategy; }
            set
            {
                if (value == DOControlStrategy.Cycle)
                {
                    IsCycle = true;
                }
                else
                {
                    IsCycle = false;
                }
                SetProperty(ref _controlStrategy, value);
            }
        }

        private PIDInfo _directPID;
        public PIDInfo DirectPID
        {
            get => _directPID;
            set
            {
                SetProperty(ref _directPID, value);
            }
        }

        private PIDInfo _reversePID;
        public PIDInfo ReversePID
        {
            get => _reversePID;
            set
            {
                SetProperty(ref _reversePID, value);
            }
        }
        public object Clone()
        {
            var clonedObject = ObjectCloner.DeepCopy(this);
            return clonedObject;
        }
    }
}
