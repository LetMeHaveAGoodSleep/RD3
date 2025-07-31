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
        private float _setPoint;
        /// <summary>
        /// 流量设定值
        /// </summary>
        public float SetPoint
        {
            get => _setPoint;
            set { SetProperty(ref _setPoint, value); }
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
                if (value > 0)
                {
                    IsRunning = true;
                }
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

        private bool _isRunning = false;
        public bool IsRunning
        {
            get => _isRunning;
            private set { SetProperty(ref _isRunning, value); }
        }

        public object Clone()
        {
            var clonedObject = ObjectCloner.DeepCopy(this);
            return clonedObject;
        }
    }
}
