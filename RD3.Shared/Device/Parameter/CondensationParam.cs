using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class CondensationParam : BasicParam
    {
        private bool _enable = false;
        [Description("是否使能")]
        public bool Enable
        {
            get { return _enable; }
            set
            {
                SetPropertyWithAudit(nameof(Enable),ref _enable, value);
                _runningStr = value ? Boolean.TrueString : Boolean.FalseString;
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
