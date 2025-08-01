using Fpi.Util;
using MathNet.Symbolics;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RD3.ViewModels
{
    public class SettingViewModel: BaseViewModel,IDialogAware
    {

        private bool _isFilterWave = Convert.ToBoolean(VarConfig.GetValue("IsFilterWave")?.ToString());
        public bool IsFilterWave
        {
            get => _isFilterWave;
            set 
            {
                if (_isFilterWave != value)
                {
                    VarConfig.SetValue("IsFilterWave", value);
                }
                SetProperty(ref _isFilterWave, value);
            }
        }

        private bool _isSimulation = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
        public bool IsSimulation
        {
            get => _isSimulation;
            set
            {
                if (_isSimulation != value)
                {
                    VarConfig.SetValue("IsSimulation", value);
                }
                SetProperty(ref _isSimulation, value);
            }
        }

        private bool _logOpen;
        public bool LogOpen
        {
            get => _logOpen;
            set
            {
                Fpi.Util.LogHelper.LogOpen = value;
                VarConfig.SetValue("LogOpen", value);

                SetProperty(ref _logOpen, value);
            }
        }

        private List<string> _intervalSource = ["1", "5", "30", "60", "120", "300", "600", "1800", "3600"];
        public List<string> IntervalSource
        {
            get => _intervalSource;
            set
            {
                SetProperty(ref _intervalSource, value);
            }
        }

        private string _timeInterval = VarConfig.GetValue("TimeInterval")?.ToString();
        public string TimeInterval
        {
            get => _timeInterval;
            set
            {
                if (_timeInterval != value)
                {
                    VarConfig.SetValue("TimeInterval", value);

                    ClockSupervisor.GetInstance().MonitorTimer.Interval = Convert.ToDouble(value) * 1000;
                }
                SetProperty(ref _timeInterval, value);
            }
        }

        private List<FontFamily> _fontSource = AppSession.FontFamilies;
        public List<FontFamily> FontSource
        {
            get => _fontSource;
            set
            {
                SetProperty(ref _fontSource, value);
            }
        }

        private FontFamily _fontFamily = AppSession.FontFamily;

        public event Action<IDialogResult> RequestClose;

        public FontFamily FontFamily
        {
            get => _fontFamily;
            set
            {
                if (_fontFamily != value)
                {
                    VarConfig.SetValue("FontFamily", value.ToString());
                    Application.Current.Resources["AppFontFamily"] = value;
                }
                SetProperty(ref _fontFamily, value);
            }
        }

        public int CommunicationProtocol
        {
            get
            {
                string temp = VarConfig.GetValue("CommunicationProtocol")?.ToString();
                if (!int.TryParse(temp, out var result))
                {
                    return 0;
                }
                else
                {
                    return result;
                }
            }
            set
            {
                int temp = -1;
                VarConfig.SetValue("CommunicationProtocol", value);
                SetProperty(ref temp, value);
            }
        }

        private string _defaultPumpFlowRate = VarConfig.GetValue("DefaultPumpFlowRate")?.ToString();
        public string DefaultPumpFlowRate
        {
            get => _defaultPumpFlowRate;
            set
            {
                if (_defaultPumpFlowRate != value)
                {
                    VarConfig.SetValue("DefaultPumpFlowRate", value);
                }
                SetProperty(ref _defaultPumpFlowRate, value);
            }
        }

        public string Title => "系统设置";

        public SettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            IsSimulation = Convert.ToBoolean(VarConfig.GetValue("IsSimulation"));
            LogOpen = Convert.ToBoolean(VarConfig.GetValue("LogOpen")?.ToString());
            IsFilterWave = Convert.ToBoolean(VarConfig.GetValue("IsFilterWave")?.ToString());
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            
        }
    }
}
