using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class PIDInfo : BindableBase
    {
        public PIDInfo()
        {
            createTime = DateTime.Now;
            maxSpeed = 100;
            Interval = 1;
        }

        public DateTime createTime { get; set; }
        public string createUser { get; set; }
        public string deviceID { get; set; }
        public float startTime { get; set; }
        public float endTime { get; set; }
        public float P { get; set; }
        public float I { get; set; }
        public float D { get; set; }
        public int Interval { get; set; }

        public float Threshold { get; set; }

        public float deadArea { get; set; }

        public float maxSpeed { get; set; }

        private string _excuteType = "自动";
        public string excuteType
        {
            get => _excuteType;
            set
            {
                _excuteType = value;
                SetProperty(ref _excuteType, value);
            }
        }

        private bool _used = false;
        public bool Used
        {
            get => _used;
            set
            {
                SetProperty(ref _used, value);
            }
        }


        private string _pidName = "";
        public string PidName
        {
            get => _pidName;
            set
            {
                SetProperty(ref _pidName, value);
            }
        }

        private PIDFactor _fator = PIDFactor.Unknown;
        public PIDFactor Factor
        {
            get => _fator;
            set
            {
                PidName = EnumUtil.GetEnumDescription(value);
                SetProperty(ref _fator, value);
            }
        }

        public void SetExcuteType(string ex)
        {
            excuteType = ex;
        }

        public string connectedWith { get; set; }//关联项

        public override bool Equals(object obj)
        {
            // 检查null和类型是否匹配
            if (obj == null || GetType() != obj.GetType())
                return false;

            PIDInfo other = (PIDInfo)obj;
            return P == other.P && I == other.I && D == other.D && deviceID == other.deviceID && Interval == other.Interval && Threshold == other.Threshold
                && deadArea == other.deadArea && Factor == other.Factor;
        }

        // 必须同时重写GetHashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(P, I, D, deviceID, Interval, Threshold, deadArea, Factor);
        }

        /// <summary>
        /// 克隆参数
        /// </summary>
        /// <param name="source"></param>
        public void CloneArgs(PIDInfo soure)
        {
            this.Used = soure.Used;
            this.deadArea = soure.deadArea;
            this.D = soure.D;
            this.I = soure.I;
            this.P = soure.P;
            this.Interval = soure.Interval;
            this.Threshold = soure.Threshold;
            this.maxSpeed = soure.maxSpeed;
        }
    }
}
