using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3
{
    public class OrthogonalParam : BindableBase
    {
        private string _name;
        public string Name
        { 
            get { return _name; } 
            set {SetProperty(ref _name,value); } 
        }

        private float _low;
        public float Low
        {
            get { return _low; }
            set { SetProperty(ref _low, value); }
        }

        private float _high;
        public float High
        {
            get { return _high; }
            set { SetProperty(ref _high, value); }
        }

        private ObservableCollection<FactorLevel> _factorLevels = [];
        public ObservableCollection<FactorLevel> FactorLevels
        {
            get => _factorLevels;
            set { SetProperty(ref _factorLevels, value); }
        }
    }

    public class FactorLevel : BindableBase
    {
        private float _levelValue;
        public float LevelValue
        {
            get { return _levelValue; }
            set { SetProperty(ref _levelValue, value); }
        }
    }
}
