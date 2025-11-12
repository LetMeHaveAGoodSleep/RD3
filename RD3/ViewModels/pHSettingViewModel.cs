using ImTools;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using RD3.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace RD3.ViewModels
{
    public class pHSettingViewModel : BaseViewModel, IDialogAware
    {
        private BasicParam _basicParam;
        public BasicParam BasicParam
        {
            get => _basicParam;
            set { SetProperty(ref _basicParam, value); }
        }

        public DeviceParameter CurrentDeviceParameter
        {
            get { return AnalysisSolution.GetInstance().CurrentFermentor.Device; }
        }

        public DelegateCommand StrategySettingCommand => new(() =>
        {
            string windowName = string.Empty;
            switch (CurrentDeviceParameter.PHParam.PHControlMode)
            {
                case PHControlMode.PID:
                    windowName = nameof(PIDView);
                    break;
                case PHControlMode.Adaptive:
                    windowName = nameof(AdaptpHView);
                    break;
                case PHControlMode.Buffer:
                    windowName = nameof(PHControlView);
                    break;
            }
            DialogHostService.ShowOnce(windowName, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });

        public DelegateCommand OpenCommand => new(() =>
        {
            if (BasicParam.TimeSeries.TimeSeriesItemCol.Count < 1 && BasicParam.ControlMode == ControlMode.TimeSeries)
            {
                HandyControl.Controls.MessageBox.Warning($"{CurrentDeviceParameter.Name}的pH时间序列为空", "温馨提示");
                return;
            }
            if (!CheckTimeSeries())
            {
                return;
            }
            if (HandyControl.Controls.MessageBox.Show($"是否开启pH控制？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            BasicParam.IsControling = true;
        });

        public DelegateCommand CloseCommand => new(() =>
        {
            if (HandyControl.Controls.MessageBox.Show($"是否停止pH控制？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            BasicParam.IsControling = false;
            BasicParam.TimeSeries.RunningInfo = string.Empty;
        });

        public pHSettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService, IRegionManager regionManager) : base(containerProvider, dialogHostService)
        {
            BasicParam = CurrentDeviceParameter.PHParam;
        }

        public string Title => "pH设置";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return CheckTimeSeries();
        }

        public void OnDialogClosed()
        {
            var acidPID = PIDInfoManager.GetInstance().PIDInfos.FindFirst(t => t.deviceID == CurrentDeviceParameter.Name && t.Factor == PIDFactor.pH_Acid);
            if (acidPID == null)
            {
                PIDInfoManager.GetInstance().PIDInfos.Add(CurrentDeviceParameter.PHParam.AcidPID);
            }
            else
            {
                acidPID = CurrentDeviceParameter.PHParam.AcidPID;
            }

            var basePID = PIDInfoManager.GetInstance().PIDInfos.FindFirst(t => t.deviceID == CurrentDeviceParameter.Name && t.Factor == PIDFactor.pH_Base);
            if (basePID == null)
            {
                PIDInfoManager.GetInstance().PIDInfos.Add(CurrentDeviceParameter.PHParam.BasePID);
            }
            else
            {
                basePID = CurrentDeviceParameter.PHParam.BasePID;
            }
            PIDInfoManager.GetInstance().Save();

            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (CurrentDeviceParameter.PHParam.AcidPID == null)
            {
                var acidPID = PIDInfoManager.GetInstance().PIDInfos.FindFirst(t => t.deviceID == CurrentDeviceParameter.Name && t.Factor == PIDFactor.pH_Acid);
                if (acidPID == null)
                {
                    acidPID = new PIDInfo()
                    {
                        deviceID = CurrentDeviceParameter.Name,
                        Factor = PIDFactor.pH_Acid,
                        Interval = 1,
                        maxSpeed = Const.MaxPumpFlowRate,
                        Threshold = 100
                    };
                }
                CurrentDeviceParameter.PHParam.AcidPID = acidPID;
            }

            if(CurrentDeviceParameter.PHParam.BasePID == null)
            {
                var basePID = PIDInfoManager.GetInstance().PIDInfos.FindFirst(t => t.deviceID == CurrentDeviceParameter.Name && t.Factor == PIDFactor.pH_Base);
                if (basePID == null)
                {
                    basePID = new PIDInfo()
                    {
                        deviceID = CurrentDeviceParameter.Name,
                        Factor = PIDFactor.pH_Base,
                        Interval = 1,
                        maxSpeed = Const.MaxPumpFlowRate,
                        Threshold = 100
                    };
                }
                CurrentDeviceParameter.PHParam.BasePID = basePID;
            }
        }

        private bool CheckTimeSeries()
        {
            bool flag = true;
            var items = BasicParam.TimeSeries.TimeSeriesItemCol;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.StartTime >= item.EndTime)
                {
                    HandyControl.Controls.MessageBox.Warning($"时间序列第{i + 1}行：开始时间应该小于结束时间", "温馨提示");
                    flag = false;
                    break;
                }
                if (item.Value < BasicParam.LowerLimit|| item.Value > BasicParam.UpperLimit)
                {
                    HandyControl.Controls.MessageBox.Warning($"时间序列第{i + 1}行：目标值应该处于{BasicParam.LowerLimit}和{BasicParam.UpperLimit}之间", "温馨提示");
                    flag = false;
                    break;
                }
            }
            return flag;
        }
    }
}
