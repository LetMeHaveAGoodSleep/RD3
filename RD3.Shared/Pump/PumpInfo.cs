using Prism.Mvvm;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class PumpInfo : PumpSetting, ICloneable
    {
        private float _flowRate_SP;
        /// <summary>
        /// 流速设定值
        /// </summary>
        public float FlowRate_SP
        {
            get => _flowRate_SP;
            set { SetProperty(ref _flowRate_SP, value); }
        }

        private int _runningTime_SP;
        /// <summary>
        /// 运行时间设定值
        /// </summary>
        public int RunningTime_SP
        {
            get => _runningTime_SP;
            set { SetProperty(ref _runningTime_SP, value); }
        }


        private float _flowCapacity_SP;
        /// <summary>
        /// 总量设定值
        /// </summary>
        public float FlowCapacity_SP
        {
            get => _flowCapacity_SP;
            set { SetProperty(ref _flowCapacity_SP, value); }
        }

        private float _flowRate;
        /// <summary>
        /// 流速实际值
        /// </summary>
        [JsonIgnore]
        public float FlowRate
        {
            get => _flowRate;
            set
            {
                if (value > 0)
                {
                    IsRunning = true;
                }
                SetProperty(ref _flowRate, value);
            }
        }


        private int _runningTime;
        /// <summary>
        /// 运行时间实际值
        /// </summary>
        [JsonIgnore]
        public int RunningTime
        {
            get => _runningTime;
            set
            {
                SetProperty(ref _runningTime, value);
                if (value <= 0)
                {
                    FormattedTime = "00:00:00";
                }
                else
                {
                    TimeSpan ts = TimeSpan.FromSeconds(value);
                    FormattedTime = ts.ToString(@"hh\:mm\:ss");

                    if (value >= RunningTime_SP && !IsConstSpeed)
                    {
                        IsControling = false;
                    }
                }
            }
        }

        private string _formattedTime = "00:00:00";
        /// <summary>
        /// 运行时间实际值
        /// </summary>
        [JsonIgnore]
        public string FormattedTime
        {
            get => _formattedTime;
            set { SetProperty(ref _formattedTime, value); }
        }

        private float _pumpFlow = 0f;
        [JsonIgnore]
        public float PumpFlow
        {
            get { return _pumpFlow; }
            set { SetProperty(ref _pumpFlow, value); }
        }

        private float _flowCapacity;
        /// <summary>
        /// 总量
        /// </summary>
        [JsonIgnore]
        public float FlowCapacity
        {
            get => _flowCapacity;
            set
            {
                SetProperty(ref _flowCapacity, value);
            }
        }

        private float _weight;
        /// <summary>
        /// 重量
        /// </summary>
        [JsonIgnore]
        public float Weight
        {
            get => _weight;
            set
            {
                SetProperty(ref _weight, value);
            }
        }

        private bool _isConstSpeed = false;
        /// <summary>
        /// 是否恒速运行
        /// </summary>
        public bool IsConstSpeed
        {
            get => _isConstSpeed;
            set  
            {
                SetProperty(ref _isConstSpeed, value);
            }
        }

        private bool _isRunning = false;
        /// <summary>
        /// 通过流速来判断泵是否在转动
        /// </summary>
        [JsonIgnore]
        public bool IsRunning
        {
            get => _isRunning;
            private set { SetProperty(ref _isRunning, value); }
        }

        private bool _lastIsControling = false;
        [JsonIgnore]
        public bool LastIsControling
        {
            get { return _lastIsControling; }
            private set { SetProperty(ref _lastIsControling, value); }
        }

        private bool _isControling = false;
        /// <summary>
        /// 是否在控制泵
        /// </summary>
        [JsonIgnore]
        public bool IsControling
        {
            get => _isControling;
            set 
            {
                LastIsControling = _isControling;
                SetProperty(ref _isControling, value);
                if (!value)
                {
                    RunningTime = 0;
                }
            }
        }

        private bool _isControlled = false;
        /// <summary>
        /// 是否被其他控制，比如pH、补料
        /// </summary>
        [JsonIgnore]
        public bool IsControlled
        {
            get => _isControlled;
            set 
            {
                SetProperty(ref _isControlled, value);
                if (value)
                {
                    RunningTime_SP = Convert.ToInt32(Const.MaxPumpFlowCapacity);
                    IsControling = true;
                }
            }
        }

        private bool _autoDefoaming = false;
        public bool AutoDefoaming
        {
            get { return _autoDefoaming; }
            set { SetProperty(ref _autoDefoaming, value); }
        }

        private int _cycle;
        public int Cycle
        {
            get => _cycle;
            set { SetProperty(ref _cycle, value); }
        }

        private float _dutyCycle;
        public float DutyCycle
        {
            get => _dutyCycle;
            set { SetProperty(ref _dutyCycle, value); }
        }

        private FeedControlMode _feedMode = FeedControlMode.ConstantSpeed;
        public FeedControlMode FeedMode
        {
            get { return _feedMode; }
            set
            {
                SetProperty(ref _feedMode, value);
            }
        }

        public object Clone()
        {
            var clonedObject = ObjectCloner.DeepCopy(this);
            return clonedObject;
        }
    }
}
