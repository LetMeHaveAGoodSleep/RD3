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

        private ObservableCollection<DOControlFactor> _factorCol = [];
        public ObservableCollection<DOControlFactor> FactorCol
        {
            get => _factorCol;
            set
            {
                SetProperty(ref _factorCol, value);

                if (value.Contains(DOControlFactor.Air))
                {
                    AirEnable = true;
                }
                else
                {
                    AirEnable = false;
                }

                if (value.Contains(DOControlFactor.O2))
                {
                    O2Enable = true;
                }
                else
                {
                    O2Enable = false;
                }

                if (value.Contains(DOControlFactor.Temp))
                {
                    TempEnable = true;
                }
                else
                {
                    TempEnable = false;
                }

                if (value.Contains(DOControlFactor.Feed))
                {
                    FeedEnable = true;
                }
                else
                {
                    FeedEnable = false;
                }
            }
        }

        private string _factorContent;
        [JsonIgnore]
        public string FactorContent
        {
            get { return _factorContent; }
            set { SetProperty(ref _factorContent, value); }
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

        private bool _airEnable;
        public bool AirEnable
        {
            get { return _airEnable; }
            private set { SetProperty(ref _airEnable, value); }
        }

        private bool _o2Enable;
        public bool O2Enable
        {
            get { return _o2Enable; }
            private set { SetProperty(ref _o2Enable, value); }
        }

        private bool _tempEnable;
        public bool TempEnable
        {
            get { return _tempEnable; }
            private set { SetProperty(ref _tempEnable, value); }
        }

        private bool _feedEnable;
        public bool FeedEnable
        {
            get { return _feedEnable; }
           private set { SetProperty(ref _feedEnable, value); }
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
