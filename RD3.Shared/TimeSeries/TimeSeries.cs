using Newtonsoft.Json;
using Prism.Mvvm;
using RD3.Shared;
using RD3.Shared.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class TimeSeries : AuditParam
    {
        public TimeSeries()
        {
            ModuleName = "时间序列";
        }

        private string _runningInfo = string.Empty;
        [JsonIgnore]
        public string RunningInfo
        {
            get => _runningInfo;
            set
            {
                SetProperty(ref _runningInfo, value);
            }
        }

        private TimeType _timeType = TimeType.RelativeTime;
        [Description("时间参照类型")]
        public TimeType TimeType
        {
            get => _timeType;
            set
            {
                SetPropertyWithAudit(ref _timeType, value);
            }
        }

        private TimeUnit _timer = TimeUnit.Hour;
        [Description("时间单位")]
        public TimeUnit Timer
        {
            get => _timer;
            set
            {
                SetPropertyWithAudit(ref _timer, value);
            }
        }

        private ObservableCollection<TimeSeriesItem> _timeSeriesItemCol = [];
        public ObservableCollection<TimeSeriesItem> TimeSeriesItemCol
        {
            get { return _timeSeriesItemCol; }
            set { SetProperty(ref _timeSeriesItemCol, value); }
        }
    }

    public class TimeSeriesItem : BindableBase, ICloneable
    {
        private double _startTime;
        public double StartTime
        {
            get { return _startTime; }
            set
            {
                SetProperty(ref _startTime, value);
            }
        }

        private double _endTime;
        public double EndTime
        {
            get { return _endTime; }
            set
            {
                SetProperty(ref _endTime, value);
            }
        }

        private double _value;
        public double Value
        {
            get { return _value; }
            set
            {
                SetProperty(ref _value, value);
            }
        }

        //备注 add by hdb
        private string _remark;
        public string Remark
        {
            get { return _remark; }
            set
            {
                SetProperty(ref _remark, value);
            }
        }

        public object Clone()
        {
            var clonedObject = ObjectCloner.DeepCopy(this);
            return clonedObject;
        }
    }
}
