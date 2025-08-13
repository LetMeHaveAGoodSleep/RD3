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
    public class PHParam : BindableBase,ICloneable
    {
        private float _pH_PV = 0f;
        public float PH_PV
        {
            get { return _pH_PV; }
            set { SetProperty(ref _pH_PV, value); }
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

        private ControlMode _controlMode = ControlMode.Enable;
        public ControlMode ControlMode
        {
            get { return _controlMode; }
            set { SetProperty(ref _controlMode, value); }
        }

        private PHControlMode _pHControlMode = PHControlMode.PID;
        public PHControlMode PHControlMode
        {
            get { return _pHControlMode; }
            set { SetProperty(ref _pHControlMode, value); }
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

        private bool _acidAssociated = true;
        public bool AcidAssociated
        {
            get { return _acidAssociated; }
            set { SetProperty(ref _acidAssociated, value); }
        }

        private bool _baseAssociated = true;
        public bool BaseAssociated
        {
            get { return _baseAssociated; }
            set { SetProperty(ref _baseAssociated, value); }
        }

        private bool _feed1Associated = false;
        public bool Feed1Associated
        {
            get => _feed1Associated;
            set => SetProperty(ref _feed1Associated, value);
        }

        private bool _feed2Associated = false;
        public bool Feed2Associated
        {
            get => _feed2Associated;
            set => SetProperty(ref _feed2Associated, value);
        }

        private FeedTimer _timer = FeedTimer.Hour;
        public FeedTimer Timer
        {
            get => _timer;
            set
            {
                SetProperty(ref _timer, value);
            }
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

        private bool _isEmergency = true;
        public bool IsEmergency
        {
            get { return _isEmergency; }
            set { SetProperty(ref _isEmergency, value); }
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

        private PIDInfo _acidPID;
        public PIDInfo AcidPID
        {
            get => _acidPID;
            set
            {
                SetProperty(ref _acidPID, value);
            }
        }

        private PIDInfo _basePID;
        public PIDInfo BasePID
        {
            get => _basePID;
            set
            {
                SetProperty(ref _basePID, value);
            }
        }

        public object Clone()
        {
            var clonedObject = ObjectCloner.DeepCopy(this);
            return clonedObject;
        }
    }

    public class TimeSeries1 : BindableBase
    {
        #region 20个时序操作
        private float _time1;
        public float Time1
        {
            get => _time1;
            set
            {
                SetProperty(ref _time1, value);
            }
        }

        private float _pH1;
        public float PH1
        {
            get => _pH1;
            set
            {
                SetProperty(ref _pH1, value);
            }
        }

        private float _time2;
        public float Time2
        {
            get => _time2;
            set
            {
                SetProperty(ref _time2, value);
            }
        }

        private float _pH2;
        public float PH2
        {
            get => _pH2;
            set
            {
                SetProperty(ref _pH2, value);
            }
        }

        private float _time3;
        public float Time3
        {
            get => _time3;
            set
            {
                SetProperty(ref _time3, value);
            }
        }

        private float _pH3;
        public float PH3
        {
            get => _pH3;
            set
            {
                SetProperty(ref _pH3, value);
            }
        }

        private float _time4;
        public float Time4
        {
            get => _time4;
            set
            {
                SetProperty(ref _time4, value);
            }
        }

        private float _pH4;
        public float PH4
        {
            get => _pH4;
            set
            {
                SetProperty(ref _pH4, value);
            }
        }

        private float _time5;
        public float Time5
        {
            get => _time5;
            set
            {
                SetProperty(ref _time5, value);
            }
        }

        private float _pH5;
        public float PH5
        {
            get => _pH5;
            set
            {
                SetProperty(ref _pH5, value);
            }
        }

        private float _time6;
        public float Time6
        {
            get => _time6;
            set
            {
                SetProperty(ref _time6, value);
            }
        }

        private float _pH6;
        public float PH6
        {
            get => _pH6;
            set
            {
                SetProperty(ref _pH6, value);
            }
        }

        private float _time7;
        public float Time7
        {
            get => _time7;
            set
            {
                SetProperty(ref _time7, value);
            }
        }

        private float _pH7;
        public float PH7
        {
            get => _pH7;
            set
            {
                SetProperty(ref _pH7, value);
            }
        }

        private float _time8;
        public float Time8
        {
            get => _time8;
            set
            {
                SetProperty(ref _time8, value);
            }
        }

        private float _pH8;
        public float PH8
        {
            get => _pH8;
            set
            {
                SetProperty(ref _pH8, value);
            }
        }

        private float _time9;
        public float Time9
        {
            get => _time9;
            set
            {
                SetProperty(ref _time9, value);
            }
        }

        private float _pH9;
        public float PH9
        {
            get => _pH9;
            set
            {
                SetProperty(ref _pH9, value);
            }
        }

        private float _time10;
        public float Time10
        {
            get => _time10;
            set
            {
                SetProperty(ref _time10, value);
            }
        }

        private float _pH10;
        public float PH10
        {
            get => _pH10;
            set
            {
                SetProperty(ref _pH10, value);
            }
        }

        private float _time11;
        public float Time11
        {
            get => _time11;
            set
            {
                SetProperty(ref _time11, value);
            }
        }

        private float _pH11;
        public float PH11
        {
            get => _pH11;
            set
            {
                SetProperty(ref _pH11, value);
            }
        }

        private float _time12;
        public float Time12
        {
            get => _time12;
            set
            {
                SetProperty(ref _time12, value);
            }
        }

        private float _pH12;
        public float PH12
        {
            get => _pH12;
            set
            {
                SetProperty(ref _pH12, value);
            }
        }

        private float _time13;
        public float Time13
        {
            get => _time13;
            set
            {
                SetProperty(ref _time13, value);
            }
        }

        private float _pH13;
        public float PH13
        {
            get => _pH13;
            set
            {
                SetProperty(ref _pH13, value);
            }
        }

        private float _time14;
        public float Time14
        {
            get => _time14;
            set
            {
                SetProperty(ref _time14, value);
            }
        }

        private float _pH14;
        public float PH14
        {
            get => _pH14;
            set
            {
                SetProperty(ref _pH14, value);
            }
        }

        private float _time15;
        public float Time15
        {
            get => _time15;
            set
            {
                SetProperty(ref _time15, value);
            }
        }

        private float _pH15;
        public float PH15
        {
            get => _pH15;
            set
            {
                SetProperty(ref _pH15, value);
            }
        }

        private float _time16;
        public float Time16
        {
            get => _time16;
            set
            {
                SetProperty(ref _time16, value);
            }
        }

        private float _pH16;
        public float PH16
        {
            get => _pH16;
            set
            {
                SetProperty(ref _pH16, value);
            }
        }

        private float _time17;
        public float Time17
        {
            get => _time17;
            set
            {
                SetProperty(ref _time17, value);
            }
        }

        private float _pH17;
        public float PH17
        {
            get => _pH17;
            set
            {
                SetProperty(ref _pH17, value);
            }
        }

        private float _time18;
        public float Time18
        {
            get => _time18;
            set
            {
                SetProperty(ref _time18, value);
            }
        }

        private float _pH18;
        public float PH18
        {
            get => _pH18;
            set
            {
                SetProperty(ref _pH18, value);
            }
        }

        private float _time19;
        public float Time19
        {
            get => _time19;
            set
            {
                SetProperty(ref _time19, value);
            }
        }

        private float _pH19;
        public float PH19
        {
            get => _pH19;
            set
            {
                SetProperty(ref _pH19, value);
            }
        }

        private float _time20;
        public float Time20
        {
            get => _time20;
            set
            {
                SetProperty(ref _time20, value);
            }
        }

        private float _pH20;
        public float PH20
        {
            get => _pH20;
            set
            {
                SetProperty(ref _pH20, value);
            }
        }
        #endregion

        private FeedTimer _timer = FeedTimer.Day;
        public FeedTimer Timer
        {
            get => _timer;
            set
            {
                SetProperty(ref _timer, value);
            }
        }

        public List<Tuple<float, float>> Tuples
        {
            get
            {
                List<Tuple<float, float>> list = [];

                Type type = this.GetType();
                for (int i = 1; i <= 20; i++)
                {
                    string timePropertyName = $"Time{i}";
                    string phPropertyName = $"PH{i}";

                    float timeValue = (float)type.GetProperty(timePropertyName).GetValue(this) * (int)this.Timer;
                    float phValue = (float)type.GetProperty(phPropertyName).GetValue(this);

                    list.Add(Tuple.Create(timeValue, phValue));
                }
                return list;
            }
        }
    }
}
