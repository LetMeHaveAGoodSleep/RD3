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
    public class DOAssParam : DOControlBaseParam
    {
        private int _interval = 10;
        public int Interval
        {
            get => _interval;
            set { SetProperty(ref _interval, value); }
        }

        private ObservableCollection<CascadeParam> _cascadeCol = [];
        public ObservableCollection<CascadeParam> CascadeCol
        {
            get => _cascadeCol;
            set
            {
                SetProperty(ref _cascadeCol, value);
            }
        }
    }

    public class CascadeParam : BindableBase
    {
        private float _airFlowRate;
        public float AirFlowRate
        {
            get => _airFlowRate;
            set { SetProperty(ref _airFlowRate, value); }
        }

        private float _o2FlowRate;
        public float O2FlowRate
        {
            get => _o2FlowRate;
            set { SetProperty(ref _o2FlowRate, value); }
        }

        private float _temp;
        public float Temp
        {
            get => _temp;
            set { SetProperty(ref _temp, value); }
        }

        private float _feedFlowRate;
        public float FeedFlowRate
        {
            get => _feedFlowRate;
            set { SetProperty(ref _feedFlowRate, value); }
        }
    }
}
