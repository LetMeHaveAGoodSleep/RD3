using ImTools;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Ioc;
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

namespace RD3.ViewModels
{
    public class pHSettingViewModel : BaseViewModel, IDialogAware
    {
        public DeviceParameter CurrentDeviceParameter
        {
            get { return AnalysisSolution.GetInstance().ReactorCol[0]; }
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

        public pHSettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => "pH设置";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
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
    }
}
