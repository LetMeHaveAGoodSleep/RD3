using Newtonsoft.Json;
using Prism.Mvvm;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class MFCInfo : MFCSetting, ICloneable
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
        public bool IsControling
        {
            get { return _isControling; }
            set 
            {
                LastIsControling = _isControling;
                SetProperty(ref _isControling, value);
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
                    IsControling = true;
                }
            }
        }

        public object Clone()
        {
            var clonedObject = ObjectCloner.DeepCopy(this);
            return clonedObject;
        }
    }
}
