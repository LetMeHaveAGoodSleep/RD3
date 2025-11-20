using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class PIDInfo : AuditParam
    {
        public PIDInfo()
        {
            _createTime = DateTime.Now;
            _maxSpeed = 100;
            _interval = 1;
        }

        private DateTime _createTime;
        public DateTime CreateTime
        {
            get => _createTime;
            set => SetProperty(ref _createTime, value);
        }

        private string _createUser;
        public string CreateUser
        {
            get => _createUser;
            set => SetProperty(ref _createUser, value);
        }

        private string _deviceId;
        public string DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }

        private float _startTime;
        public float StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        private float _endTime;
        public float EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        private float _p;
        [Description("比例P")]
        public float P
        {
            get => _p;
            set => SetPropertyWithAudit(ref _p, value);
        }

        private float _i;
        [Description("积分P")]
        public float I
        {
            get => _i;
            set => SetPropertyWithAudit(ref _i, value);
        }

        private float _d;
        [Description("微分P")]
        public float D
        {
            get => _d;
            set => SetPropertyWithAudit(ref _d, value);
        }

        private int _interval;
        [Description("采样间隔")]
        public int Interval
        {
            get => _interval;
            set => SetPropertyWithAudit(ref _interval, value);
        }

        private float _threshold;
        [Description("阈值")]
        public float Threshold
        {
            get => _threshold;
            set => SetPropertyWithAudit(ref _threshold, value);
        }

        private float _deadArea;
        [Description("死区")]
        public float DeadArea
        {
            get => _deadArea;
            set => SetPropertyWithAudit(ref _deadArea, value);
        }

        private float _maxSpeed;
        [Description("边界值")]
        public float MaxSpeed
        {
            get => _maxSpeed;
            set => SetPropertyWithAudit(ref _maxSpeed, value);
        }

        private string _executeType = "自动";
        public string ExecuteType
        {
            get => _executeType;
            set => SetPropertyWithAudit(ref _executeType, value);
        }

        private bool _used = false;
        public bool Used
        {
            get => _used;
            set => SetPropertyWithAudit(ref _used, value);
        }

        private string _pidName = "";
        public string PidName
        {
            get => _pidName;
            set => SetPropertyWithAudit(ref _pidName, value);
        }

        private PIDFactor _factor = PIDFactor.Unknown;
        public PIDFactor Factor
        {
            get => _factor;
            set
            {
                var newName = EnumUtil.GetEnumDescription(value);
                if (PidName != newName)
                {
                    PidName = newName; // 触发PidName的审计
                }
                SetPropertyWithAudit(ref _factor, value); // 触发Factor的审计
            }
        }

        public void SetExecuteType(string ex)
        {
            ExecuteType = ex; // 复用属性setter，触发审计
        }

        private string _connectedWith;
        public string ConnectedWith
        {
            get => _connectedWith;
            set => SetPropertyWithAudit(ref _connectedWith, value);
        } // 关联项

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            PIDInfo other = (PIDInfo)obj;
            return P == other.P && I == other.I && D == other.D && DeviceId == other.DeviceId
                && Interval == other.Interval && Threshold == other.Threshold
                && DeadArea == other.DeadArea && Factor == other.Factor;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(P, I, D, DeviceId, Interval, Threshold, DeadArea, Factor);
        }
    }
}
