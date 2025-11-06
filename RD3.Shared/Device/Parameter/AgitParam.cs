using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
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
        public new int SP
        {
            get { return _sp; }
            set { SetProperty(ref _sp, value); }
        }

        private int _lowerLimit = 0;
        /// <summary>
        /// 下限
        /// </summary>
        public new int LowerLimit
        {
            get { return _lowerLimit; }
            set { SetProperty(ref _lowerLimit, value); }
        }

        private int _upperLimit = 100;
        /// <summary>
        /// 上限
        /// </summary>
        public new  int UpperLimit
        {
            get { return _upperLimit; }
            set { SetProperty(ref _upperLimit, value); }
        }
        public AgitParam()
        {
            LowerLimit = 0;
            UpperLimit = Const.MaxAgit;
        }
    }
}
