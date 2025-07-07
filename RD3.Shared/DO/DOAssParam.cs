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
            }
        }

        private string _factorContent;
        [JsonIgnore]
        public string FactorContent
        {
            get { return _factorContent; }
            set { SetProperty(ref _factorContent, value); }
        }

        private ObservableCollection<CascadeParam> _airCol = [];
        public ObservableCollection<CascadeParam> AirCol
        {
            get => _airCol;
            set
            {
                SetProperty(ref _airCol, value);
            }
        }

        private ObservableCollection<CascadeParam> _o2Col = [];
        public ObservableCollection<CascadeParam> O2Col
        {
            get => _o2Col;
            set
            {
                SetProperty(ref _o2Col, value);
            }
        }

        private ObservableCollection<CascadeParam> _tempCol = [];
        public ObservableCollection<CascadeParam> TempCol
        {
            get => _tempCol;
            set
            {
                SetProperty(ref _tempCol, value);
            }
        }

        private ObservableCollection<CascadeParam> _feedCol = [];
        public ObservableCollection<CascadeParam> FeedCol
        {
            get => _feedCol;
            set
            {
                SetProperty(ref _feedCol, value);
            }
        }
    }


    public class CascadeParam : BindableBase
    {
        private float _stepValue;
        public float StepValue
        {
            get => _stepValue;
            set { SetProperty(ref _stepValue, value); }
        }
    }
}
