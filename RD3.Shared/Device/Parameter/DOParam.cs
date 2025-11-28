using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class DOParam : BasicParam
    {
        private float _initialFeed = 0f;
        [Description("初始补料速度")]
        public float InitialFeed
        {
            get => _initialFeed;
            set { SetPropertyWithAudit(nameof(InitialFeed), ref _initialFeed, value); }
        }

        private float _initialTemp = 0f;
        [Description("初始温度")]
        public float InitialTemp
        {
            get => _initialTemp;
            set { SetPropertyWithAudit(nameof(InitialTemp), ref _initialTemp, value); }
        }

        private bool _isCycle = false;
        public bool IsCycle
        {
            get { return _isCycle; }
            set { SetProperty(ref _isCycle, value); }
        }

        private bool _isDirect = true;
        [Description("是否开启正向控制")]
        public bool IsDirect
        {
            get { return _isDirect; }
            set { SetPropertyWithAudit(nameof(IsDirect), ref _isDirect, value); }
        }

        private bool _isReverse = true;
        [Description("是否开启反向控制")]
        public bool IsReverse
        {
            get { return _isReverse; }
            set { SetPropertyWithAudit(nameof(IsReverse), ref _isReverse, value); }
        }

        private CycleParam _agitCycle = new() { ModuleName = "溶氧周期控制-转速参数" };
        public CycleParam AgitCycle
        {
            get { return _agitCycle; }
            set { SetProperty(ref _agitCycle, value); }
        }

        private DOControlStrategy _controlStrategy = DOControlStrategy.Step;
        [Description("控制策略")]
        public DOControlStrategy ControlStrategy
        {
            get { return _controlStrategy; }
            set
            {
                SetPropertyWithAudit(nameof(ControlStrategy), ref _controlStrategy, value);
            }
        }

        private PIDInfo _directPID;
        public PIDInfo DirectPID
        {
            get => _directPID;
            set
            {
                SetProperty(ref _directPID, value);
            }
        }

        private PIDInfo _reversePID;
        public PIDInfo ReversePID
        {
            get => _reversePID;
            set
            {
                SetProperty(ref _reversePID, value);
            }
        }
    }
}
