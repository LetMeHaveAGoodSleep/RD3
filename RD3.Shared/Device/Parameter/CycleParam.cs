using Prism.Mvvm;
using RD3.Shared.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class CycleParam : AuditParam
    {
        private int _directInterval = 1;
        [Description("转速(+)时间间隔")]
        public int DirectInterval
        {
            get { return _directInterval; }
            set { SetPropertyWithAudit(nameof(DirectInterval), ref _directInterval, value); }
        }

        private int _reverseInterval = 1;
        [Description("转速(-)时间间隔")]
        public int ReverseInterval
        {
            get { return _reverseInterval; }
            set { SetPropertyWithAudit(nameof(ReverseInterval), ref _reverseInterval, value); }
        }

        private int _directStep = 1;
        [Description("转速步长(+)")]
        public int DirectStep
        {
            get { return _directStep; }
            set { SetPropertyWithAudit(nameof(DirectStep), ref _directStep, value); }
        }

        private int _reverseStep = 1;
        [Description("转速步长(-)")]
        public int ReverseStep
        {
            get { return _reverseStep; }
            set { SetPropertyWithAudit(nameof(ReverseStep), ref _reverseStep, value); }
        }

        private int _lowerLimit = 100;
        [Description("转速下限")]
        public int LowerLimit
        {
            get { return _lowerLimit; }
            set { SetPropertyWithAudit(nameof(LowerLimit), ref _lowerLimit, value); }
        }

        private int _upperLimit = Const.MaxAgit;
        [Description("转速上限")]
        public int UpperLimit
        {
            get { return _upperLimit; }
            set { SetPropertyWithAudit(nameof(UpperLimit), ref _upperLimit, value); }
        }
    }
}
