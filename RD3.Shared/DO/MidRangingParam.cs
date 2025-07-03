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
    public class MidRangingParam : DOControlBaseParam
    {
        private int _agitHigh = 1200;
        public int AgitHigh
        {
            get => _agitHigh;
            set { SetProperty(ref _agitHigh, value); }
        }

        private float _initialAir = 0.5f;
        public float InitialAir
        {
            get => _initialAir;
            set { SetProperty(ref _initialAir, value); }
        }

        private float _airUpperLimit = 2f;
        public float AirUpperLimit
        {
            get => _airUpperLimit;
            set { SetProperty(ref _airUpperLimit, value); }
        }

        private bool _o2Associated = false;
        public bool O2Associated
        {
            get => _o2Associated;
            set { SetProperty(ref _o2Associated, value); }
        }

        private ObservableCollection<DOControlFactor> _factorCol = [];
        public ObservableCollection<DOControlFactor> FactorCol
        {
            get => _factorCol;
            set
            {
                SetProperty(ref _factorCol, value);
            }
        }

        private string _factorContent;
        [JsonIgnore]
        public string FactorContent
        {
            get { return _factorContent; }
            set { SetProperty(ref _factorContent, value); }
        }

        private object _reserve1;
        public object Reserve1
        {
            get => _reserve1;
            set { SetProperty(ref _reserve1, value); }
        }

        private object _reserve2;
        public object Reserve2
        {
            get => _reserve2;
            set { SetProperty(ref _reserve2, value); }
        }

        private object _reserve3;
        public object Reserve3
        {
            get => _reserve3;
            set { SetProperty(ref _reserve3, value); }
        }
    }
}
