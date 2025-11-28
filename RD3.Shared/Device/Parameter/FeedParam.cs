using MathNet.Symbolics;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class FeedParam : BasicParam
    {
        private float _feed_Total = 0f;
        [Description("补料预设量")]
        public float Feed_Total
        {
            get { return _feed_Total; }
            set { SetPropertyWithAudit(nameof(Feed_Total), ref _feed_Total, value); }
        }

        private string _feedName = $"补料1(mL/h)";
        public string FeedName
        {
            get => _feedName;
            set {  SetProperty(ref _feedName, value);}
        }

        private string _feedTotalName = $"补料1(mL)";
        public string FeedTotalName
        {
            get => _feedTotalName;
            set { SetProperty(ref _feedTotalName, value); }
        }

        private int _index = 1;
        public int Index
        {
            get => _index;
            set 
            {
                FeedName = $"补料{value}(mL/h)";
                FeedTotalName = $"补料{value}(mL)";
                SetProperty(ref _index, value);
            }
        }

        private int _feedIndex = 0;
        /// <summary>
        ///  常量
        ///  多项式
        ///  指数
        ///  时间序列
        ///  关联DO
        ///  关联PH
        ///  定量
        ///  周期
        /// </summary>
        [Description("补料策略")]
        public int FeedIndex
        {
            get { return _feedIndex; }
            set
            {
                if (value == 8)
                {
                    IsTotal = true;
                }
                else
                {
                    IsTotal = false;
                }
                SetPropertyWithAudit(nameof(FeedIndex), ref _feedIndex, value);
            }
        }

        private FeedControlMode _feedMode = FeedControlMode.ConstantSpeed;
        [Description("补料策略")]
        public FeedControlMode FeedMode
        {
            get { return _feedMode; }
            set
            {
                SetPropertyWithAudit(nameof(FeedMode), ref _feedMode, value);
            }
        }

        private bool _isTotal = false;
        [Description("是否定量补料")]
        public bool IsTotal
        {
            get { return _isTotal; }
            set { SetPropertyWithAudit(nameof(IsTotal), ref _isTotal, value); }
        }
    }
}
