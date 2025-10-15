using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class TempParam : BindableBase, IAuditRecord,ICloneable
    {
        private float _temp_PV = 37f;
        public float Temp_PV
        {
            get { return _temp_PV; }
            set { SetProperty(ref _temp_PV, value); }
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

        private float _lowerLimit = 25;
        public float LowerLimit
        {
            get { return _lowerLimit; }
            set { SetProperty(ref _lowerLimit, value); }
        }

        private float _upperLimit = 75;
        public float UpperLimit
        {
            get { return _upperLimit; }
            set { SetProperty(ref _upperLimit, value); }
        }

        private ControlMode _controlMode = ControlMode.Enable;
        public ControlMode ControlMode
        {
            get { return _controlMode; }
            set { SetProperty(ref _controlMode, value); }
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

        private bool _isEnable = false;
        public bool IsEnable
        {
            get { return _isEnable; }
            set 
            {
                SetProperty(ref _isEnable, value);
                if (value)
                {
                    TecControlMode = TecControlMode.PIDControl;
                }
                else
                {
                    TecControlMode = TecControlMode.Close;
                }
            }
        }

        private TecControlMode _tecControlMode = TecControlMode.Close;
        public TecControlMode TecControlMode
        {
            get { return _tecControlMode; }
            set { SetProperty(ref _tecControlMode, value); }
        }
        

        private bool _cooling;
        public bool Cooling
        {
            get { return _cooling; }
            set { SetProperty(ref _cooling, value); }
        }

        private bool _heating;
        public bool Heating
        {
            get { return _heating; }
            set { SetProperty(ref _heating, value); }
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

        private TimeSeries _timeSeries = new TimeSeries();
        public TimeSeries TimeSeries
        {
            get => _timeSeries;
            set
            {
                SetProperty(ref _timeSeries, value);
            }
        }

        private TECParam _tecParam;
        public TECParam PIDParam
        {
            get => _tecParam;
            set
            {
                SetProperty(ref _tecParam, value);
            }
        }

        public AuditModule Module { get => AuditModule.Temp_controller; }

        public object Clone()
        {
            var clonedObject = ObjectCloner.DeepCopy(this);
            return clonedObject;
        }
    }
}
