using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class DOParam : BasicParam
    {
        private float _initialFeed = 0f;
        public float InitialFeed
        {
            get => _initialFeed;
            set { SetProperty(ref _initialFeed, value); }
        }

        private float _initialTemp = 0f;
        public float InitialTemp
        {
            get => _initialTemp;
            set { SetProperty(ref _initialTemp, value); }
        }

        private bool _isCycle = true;
        public bool IsCycle
        {
            get { return _isCycle; }
            set { SetProperty(ref _isCycle, value); }
        }

        private bool _isDirect = true;
        public bool IsDirect
        {
            get { return _isDirect; }
            set { SetProperty(ref _isDirect, value); }
        }

        private bool _isReverse = true;
        public bool IsReverse
        {
            get { return _isReverse; }
            set { SetProperty(ref _isReverse, value); }
        }

        private CycleParam _agitCycle = new();
        public CycleParam AgitCycle
        {
            get { return _agitCycle; }
            set { SetProperty(ref _agitCycle, value); }
        }

        private DOControlStrategy _controlStrategy = DOControlStrategy.Step;
        public DOControlStrategy ControlStrategy
        {
            get { return _controlStrategy; }
            set
            {
                SetProperty(ref _controlStrategy, value);
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
