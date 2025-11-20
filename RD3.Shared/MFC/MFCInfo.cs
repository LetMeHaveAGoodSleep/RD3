using Newtonsoft.Json;
using Prism.Mvvm;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class MFCInfo : MFCSetting
    {
        private float _flowRate_SP;
        /// <summary>
        /// 流量设定值
        /// </summary>
        [Description("流量预设值")]
        public float FlowRate_SP
        {
            get => _flowRate_SP;
            set { SetPropertyWithAudit(ref _flowRate_SP, value); }
        }

        private float _flowRate;
        /// <summary>
        /// 流量实际值
        /// </summary>
        public float FlowRate
        {
            get => _flowRate;
            set
            {
                SetProperty(ref _flowRate, value);
                if (value > 0)
                {
                    _runningStr = value > 0 ? Boolean.TrueString : Boolean.FalseString;
                }
            }
        }

        private float _flowCapacity;
        /// <summary>
        /// 总量
        /// </summary>
        public float FlowCapacity
        {
            get => _flowCapacity;
            set
            {
                SetProperty(ref _flowCapacity, value);
            }
        }

        private bool _lastIsControling = false;
        public bool LastIsControling
        {
            get { return _lastIsControling; }
           private set { SetProperty(ref _lastIsControling, value); }
        }

        private bool _isControling = false;
        [Description("是否正在控制")]
        public bool IsControling
        {
            get { return _isControling; }
            set 
            {
                LastIsControling = _isControling;
                SetPropertyWithAudit(ref _isControling, value);
            }
        }

        private bool _isControlled = false;
        /// <summary>
        /// 是否被其他控制，比如溶氧
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
                    IsAuditing = false;
                    IsControling = true;
                }
            }
        }

        private string _runningStr = Boolean.FalseString;
        /// <summary>
        /// 通过流速来判断泵是否在转动
        /// </summary>
        [JsonIgnore]
        public string RunningStr
        {
            get => _runningStr;
            private set { SetProperty(ref _runningStr, value); }
        }
    }
}
