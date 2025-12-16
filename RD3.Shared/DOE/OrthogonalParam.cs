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

        private float _level1 = -1f;
        public float Level1
        {
            get { return _level1; }
            set { SetProperty(ref _level1, value); }
        }

        private float _level2 = -1f;
        public float Level2
        {
            get { return _level2; }
            set { SetProperty(ref _level2, value); }
        }

        private float _level3 = -1f;
        public float Level3
        {
            get { return _level3; }
            set { SetProperty(ref _level3, value); }
        }

        private float _level4 = -1f;
        public float Level4
        {
            get { return _level4; }
            set { SetProperty(ref _level4, value); }
        }

        private float _level5 = -1f;
        public float Level5
        {
            get { return _level5; }
            set { SetProperty(ref _level5, value); }
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
