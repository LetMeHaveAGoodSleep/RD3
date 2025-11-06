using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class BasicParam : AuditParam
    {
        private float _sp = 0f;
        /// <summary>
        /// 预设值
        /// </summary>
        public float SP
        {
            get { return _sp; }
            set { SetProperty(ref _sp, value); }
        }

        private float _lowerLimit = 0;
        /// <summary>
        /// 下限
        /// </summary>
        public float LowerLimit
        {
            get { return _lowerLimit; }
            set { SetProperty(ref _lowerLimit, value); }
        }

        private float _upperLimit = 100;
        /// <summary>
        /// 上限
        /// </summary>
        public float UpperLimit
        {
            get { return _upperLimit; }
            set { SetProperty(ref _upperLimit, value); }
        }

        private ControlMode _controlMode = ControlMode.Constant;
        public ControlMode ControlMode
        {
            get { return _controlMode; }
            set { SetProperty(ref _controlMode, value); }
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
    }
}
