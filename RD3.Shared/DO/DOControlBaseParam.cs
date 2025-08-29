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
    public class DOControlBaseParam : BindableBase
    {
        private string _deviceName = "";
        public string DeviceName
        {
            get => _deviceName;
            set { SetProperty(ref _deviceName, value); }
        }

        private int _agitLowerLimit = 100;
        public int AgitLowerLimit
        {
            get => _agitLowerLimit;
            set { SetProperty(ref _agitLowerLimit, value); }
        }

        private int _gasWaitTime = 120;
        public int GasWaitTime
        {
            get => _gasWaitTime;
            set { SetProperty(ref _gasWaitTime, value); }
        }

        private int _agitUpperLimit = 1500;
        public int AgitUpperLimit
        {
            get => _agitUpperLimit;
            set { SetProperty(ref _agitUpperLimit, value); }
        }

        private int _unit = 1;
        /// <summary>
        /// 0:VVM 1:L/min
        /// </summary>
        public int Unit
        {
            get => _unit;
            set { SetProperty(ref _unit, value); }
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

        private bool _airEnable;
        public bool AirEnable
        {
            get { return _airEnable; }
            set
            {
                SetProperty(ref _airEnable, value);
                if (value)
                {
                    if (!FactorCol.Contains(DOControlFactor.Air))
                    {
                        FactorCol.Add(DOControlFactor.Air);
                    }
                }
                else
                {
                    while (FactorCol.Contains(DOControlFactor.Air))
                    {
                        FactorCol.Remove(DOControlFactor.Air);
                    }
                }
            }
        }

        private bool _o2Enable;
        public bool O2Enable
        {
            get { return _o2Enable; }
            set
            {
                SetProperty(ref _o2Enable, value);

                if (value)
                {
                    if (!FactorCol.Contains(DOControlFactor.O2))
                    {
                        FactorCol.Add(DOControlFactor.O2);
                    }
                }
                else
                {
                    while (FactorCol.Contains(DOControlFactor.O2))
                    {
                        FactorCol.Remove(DOControlFactor.O2);
                    }
                }
            }
        }

        private bool _tempEnable;
        public bool TempEnable
        {
            get { return _tempEnable; }
            set
            {
                SetProperty(ref _tempEnable, value);

                if (value)
                {
                    if (!FactorCol.Contains(DOControlFactor.Temp))
                    {
                        FactorCol.Add(DOControlFactor.Temp);
                    }
                }
                else
                {
                    while (FactorCol.Contains(DOControlFactor.Temp))
                    {
                        FactorCol.Remove(DOControlFactor.Temp);
                    }
                }
            }
        }

        private bool _feedEnable;
        public bool FeedEnable
        {
            get { return _feedEnable; }
            set
            {
                SetProperty(ref _feedEnable, value);

                if (value)
                {
                    if (!FactorCol.Contains(DOControlFactor.Feed))
                    {
                        FactorCol.Add(DOControlFactor.Feed);
                    }
                }
                else
                {
                    while (FactorCol.Contains(DOControlFactor.Feed))
                    {
                        FactorCol.Remove(DOControlFactor.Feed);
                    }
                }
            }
        }
    }
}
