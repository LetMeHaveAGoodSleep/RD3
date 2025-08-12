using ImTools;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using RD3.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RD3.ViewModels
{
    public class PumpSettingViewModel : BaseViewModel, IDialogAware
    {
        private Dictionary<string, float> dicPumpSP = new Dictionary<string, float>();

        private PumpInfo _pumpInfo = new();
        public PumpInfo PumpInfo
        {
            get => _pumpInfo;
            set { SetProperty(ref _pumpInfo, value); }
        }

        private DeviceParameter _currentDeviceParameter;
        public DeviceParameter CurrentDeviceParameter
        {
            get { return _currentDeviceParameter; }
            set { SetProperty(ref _currentDeviceParameter, value); }
        }

        public DelegateCommand ChangePumpCommand => new(() =>
        {
            try
            {
                if (PumpInfo.Pump != PeristalticPump.None)
                {
                    int count = AnalysisSolution.GetInstance().PumpInfoCol.Count(t => t.Pump == PumpInfo.Pump);
                    if (count > 1)
                    {
                        HandyControl.Controls.MessageBox.Info($"已存在{EnumUtil.GetEnumDescription(PumpInfo.Pump)}泵", "温馨提示");
                        PumpInfo.Pump = PeristalticPump.None;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        });

        public DelegateCommand PumpClearCommand => new(() =>
        {
            if (HandyControl.Controls.MessageBox.Show($"确定清除泵{PumpInfo.PumpIndex}的累计量?", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            CommandWrapper.SetResetFlowCapacity(PumpInfo.DeviceID, ClearModule.Pump, PumpInfo.PumpIndex);
        });

        public DelegateCommand PumpControlCommand => new(() => 
        {
            //关闭泵
            if (PumpInfo.IsControling)
            {
                PumpInfo.IsControling = false;
                return;
            }
            if (PumpInfo.FlowRate_SP <= 0)
            {
                HandyControl.Controls.MessageBox.Warning("流速必须大于0", "温馨提示");
                return;
            }

            PumpInfo.FlowRate_SP = Math.Clamp(PumpInfo.FlowRate_SP, 0, Const.MaxPumpFlowRate);
            if (PumpInfo.Pump != PeristalticPump.FeedPump && PumpInfo.Pump != PeristalticPump.Feed2Pump)
            {
                if (PumpInfo.RunningTime_SP <= 0 && !PumpInfo.IsConstSpeed)
                {
                    HandyControl.Controls.MessageBox.Warning("运行时间必须大于0", "温馨提示");
                    return;
                }
                PumpInfo.IsControling = true;
                return;
            }
            PumpInfo.IsControling = true;
        });

        public DelegateCommand<DeviceParameter> AFSettingCommand => new((DeviceParameter device) =>
        {
            if (PumpInfo.FlowRate_SP <= 0)
            {
                HandyControl.Controls.MessageBox.Warning("流速必须大于0", "温馨提示");
                return;
            }
            if (PumpInfo.Cycle <= 0)
            {
                HandyControl.Controls.MessageBox.Warning("消泡周期必须大于0", "温馨提示");
                return;
            }
            if (PumpInfo.DutyCycle <= 0)
            {
                HandyControl.Controls.MessageBox.Warning("占空比必须大于0", "温馨提示");
                return;
            }
            DefoamingParam param = new DefoamingParam()
            {
                PumpNo = PumpInfo.PumpIndex,
                SensorEnable = PumpInfo.AutoDefoaming,
                Cycle = PumpInfo.Cycle,
                TimeRatio = PumpInfo.DutyCycle,
                FlowSpeed = PumpInfo.FlowRate_SP
            };
            InstrumentSolution.GetInstance().CommandWrapper.SetAutoDefoamingSetting(PumpInfo.DeviceID, param);
        });

        public DelegateCommand FeedStrategyCommand => new(() =>
        {
            DialogParameters keyValuePairs = new DialogParameters()
            {
                {"deviceID",PumpInfo.DeviceID },
                {nameof(PeristalticPump), PumpInfo.Pump}
            };

            DialogHostService.ShowOnce(nameof(FeedStrategyView), keyValuePairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });

        public PumpSettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            CurrentDeviceParameter = AnalysisSolution.GetInstance().ReactorCol[0];
        }

        public string Title => "蠕动泵设置";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            PumpInfo = parameters.GetValue<PumpInfo>(nameof(PumpInfo));
        }
    }
}
