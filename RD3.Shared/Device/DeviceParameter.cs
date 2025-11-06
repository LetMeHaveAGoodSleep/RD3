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
    public class DeviceParameter : RealTimeParam
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private bool _inExperimenting = false;
        /// <summary>
        /// 表示该反应器是否在批次实验中
        /// </summary>
        public bool InExperimenting
        {
            get { return _inExperimenting; }
            set { SetProperty(ref _inExperimenting, value); }
        }

        private int _batchID = -1;
        /// <summary>
        /// 批次号
        /// </summary>
        public int BatchID
        {
            get { return _batchID; }
            set 
            { 
                SetProperty(ref _batchID, value);
                if (value <= 0)
                {
                    ExperimentTime = 0;
                }
            }
        }

        private int _experimentTime;
        /// <summary>
        ///批次实验实际值
        /// </summary>
        public int ExperimentTime
        {
            get => _experimentTime;
            set
            {
                SetProperty(ref _experimentTime, value);
                if (value <= 0)
                {
                    FormattedTime = "00天00时00分00秒";
                }
                else
                {
                    TimeSpan ts = TimeSpan.FromSeconds(value);
                    FormattedTime = $"{ts.Days:00}天{ts.Hours:00}时{ts.Minutes:00}分{ts.Seconds:00}秒";
                }
            }
        }

        private string _formattedTime = "00天00时00分00秒";
        /// <summary>
        /// 运行时间实际值
        /// </summary>
        [JsonIgnore]
        public string FormattedTime
        {
            get => _formattedTime;
            set { SetProperty(ref _formattedTime, value); }
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


        private TempParam _tempParam = new() { IsAuditing = true };
        public TempParam TempParam
        {
            get { return _tempParam; }
            set { SetProperty(ref _tempParam, value); }
        }

        private PHParam _pHParam = new() { IsAuditing = true };
        public PHParam PHParam
        {
            get { return _pHParam; }
            set { SetProperty(ref _pHParam, value); }
        }

        private DOParam _dOParam = new() { IsAuditing = true };
        public DOParam DOParam
        {
            get { return _dOParam; }
            set { SetProperty(ref _dOParam, value); }
        }

        private AgitParam _agitParam = new() { IsAuditing = true };
        public AgitParam AgitParam
        {
            get { return _agitParam; }
            set { SetProperty(ref _agitParam, value); }
        }

        private BaseParam _baseParam = new() { IsAuditing = true };
        public BaseParam BaseParam
        {
            get { return _baseParam; }
            set { SetProperty(ref _baseParam, value); }
        }

        private AcidParam _acidParam = new() { IsAuditing = true };
        public AcidParam AcidParam
        {
            get { return _acidParam; }
            set { SetProperty(ref _acidParam, value); }
        }

        private AFParam _aFParam = new() { IsAuditing = true };
        public AFParam AFParam
        {
            get { return _aFParam; }
            set { SetProperty(ref _aFParam, value); }
        }

        private GasParam _airParam = new() { IsAuditing = true };
        public GasParam AirParam
        {
            get { return _airParam; }
            set { SetProperty(ref _airParam, value); }
        }

        private GasParam _cO2Param = new() { IsAuditing = true };
        public GasParam CO2Param
        {
            get { return _cO2Param; }
            set { SetProperty(ref _cO2Param, value); }
        }

        private GasParam _o2Param = new() { IsAuditing = true };
        public GasParam O2Param
        {
            get { return _o2Param; }
            set { SetProperty(ref _o2Param, value); }
        }

        private GasParam _n2Param = new() { IsAuditing = true };
        public GasParam N2Param
        {
            get { return _n2Param; }
            set { SetProperty(ref _n2Param, value); }
        }

        DefoamingSetting _defoamingSetting = new DefoamingSetting();
        public DefoamingSetting DefoamingSetting
        {
            get => _defoamingSetting;
            set { SetProperty(ref _defoamingSetting, value); }
        }

        FeedParam _feedParam1 = new FeedParam() { Index = 1, IsAuditing = true };
        public FeedParam FeedParam1
        {
            get => _feedParam1;
            set { SetProperty(ref _feedParam1, value); }
        }

        FeedParam _feedParam2 = new FeedParam() { Index = 2, IsAuditing = true };
        public FeedParam FeedParam2
        {
            get => _feedParam2;
            set { SetProperty(ref _feedParam2, value); }
        }

        private bool _doFilterEnable = false;
        /// <summary>
        /// DO值是否滤波
        /// </summary>
        public bool DOFilterEnable
        {
            get => _doFilterEnable;
            set { SetProperty(ref _doFilterEnable, value); }
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
        /// <summary>
        /// DO滤波的参数
        /// </summary>
        public float AgitSampleCycle
        {
            get => _agitSampleCycle;
            set 
            {
                SetProperty(ref _agitSampleCycle, value);
            }
        }

        private float _agitSampleFrequency = Convert.ToSingle(1.0f / (2 * Math.PI * Math.Max(1, 0.001)));
        /// <summary>
        /// DO滤波的频率
        /// </summary>
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
        /// <summary>
        /// DO滤波的时间常数
        /// </summary>
        public float AgitTimeConstant
        {
            get => _agitTimeConstant;
            private set { SetProperty(ref _agitTimeConstant, value); }
        }

        private AdaptivepHParameter _adaptivepHParameter = new AdaptivepHParameter();
        /// <summary>
        /// pH自适应调节参数
        /// </summary>
        public AdaptivepHParameter AdaptivepHParameter
        {
            get => _adaptivepHParameter;
            private set { SetProperty(ref _adaptivepHParameter, value); }
        }
    }
}
