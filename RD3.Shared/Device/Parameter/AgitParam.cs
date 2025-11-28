using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class AgitParam : BasicParam
    {
        private int _sp = 0;
        /// <summary>
        /// 预设值
        /// </summary>
        [Description("预设值")]
        public new int SP
        {
            get { return _sp; }
            set { SetPropertyWithAudit(nameof(SP), ref _sp, value); }
        }

        private int _lowerLimit = 0;
        /// <summary>
        /// 下限
        /// </summary>
        [Description("下限")]
        public new int LowerLimit
        {
            get { return _lowerLimit; }
            set { SetPropertyWithAudit(nameof(LowerLimit), ref _lowerLimit, value); }
        }

        private int _upperLimit = 100;
        /// <summary>
        /// 上限
        /// </summary>
        [Description("上限")]
        public new  int UpperLimit
        {
            get { return _upperLimit; }
            set { SetPropertyWithAudit(nameof(UpperLimit),ref _upperLimit, value); }
        }
        public AgitParam()
        {
            UpperLimit = Const.MaxAgit;
        }
    }
}
