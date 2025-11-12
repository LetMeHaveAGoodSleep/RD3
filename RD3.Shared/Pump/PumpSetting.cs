using Newtonsoft.Json;
using Prism.Mvvm;
using System;

namespace RD3.Shared
{
    public class PumpSetting : AuditParam
    {
        private string _deviceID = "G01";
        public string DeviceID
        {
            get => _deviceID;
            private set { SetProperty(ref _deviceID, value); }
        }

        private int _pumpIndex = -1;
        public int PumpIndex
        {
            get => _pumpIndex;
            set { SetProperty(ref _pumpIndex, value); }
        }

        private PeristalticPump _pump = PeristalticPump.None;
        public PeristalticPump Pump
        {
            get => _pump;
            set
            {
                PumpName = EnumUtil.GetEnumDescription(value);
                SetProperty(ref _pump, value);
            }
        }

        private string _pumpName = "未设置";
        [JsonIgnore]
        public string PumpName
        {
            get => _pumpName;
            private set { SetProperty(ref _pumpName, value); }
        }

        private bool _isExist = false;
        public bool IsExist
        {
            get => _isExist;
            set { SetProperty(ref _isExist, value); }
        }

        private bool _isWeigh = true;
        public bool IsWeigh
        {
            get => _isWeigh;
            set { SetProperty(ref _isWeigh, value); }
        }

        private WeightIndex _weighIndex =  WeightIndex.Unset;
        public WeightIndex WeighIndex
        {
            get => _weighIndex;
            set { SetProperty(ref _weighIndex, value); }
        }

        private bool _isEnable = false;
        public bool IsEnable
        {
            get => _isEnable;
            set { SetProperty(ref _isEnable, value); }
        }
    }
}
