using ImTools;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
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
                //var pumpSetting = AnalysisSolution.GetInstance().PumpCol.FindFirst(t => t.PumpIndex == PumpInfo.PumpIndex);
                //PropertyMapper.Map(PumpInfo, pumpSetting);
                //AnalysisSolution.GetInstance().SavePumpSetting();
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
                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                {
                    PumpNo = PumpInfo.PumpIndex,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = 0,
                    FlowCapacity = 0
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(PumpInfo.DeviceID, param);
                PumpInfo.IsControling = false;
                return;
            }
            if (PumpInfo.FlowRate_SP <= 0)
            {
                HandyControl.Controls.MessageBox.Warning("流速必须大于0", "温馨提示");
                return;
            }
            if (PumpInfo.RunningTime_SP <= 0)
            {
                HandyControl.Controls.MessageBox.Warning("运行时间必须大于0", "温馨提示");
                return;
            }

            PumpInfo.FlowRate_SP = PumpInfo.FlowRate_SP >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : PumpInfo.FlowRate_SP;
            if (PumpInfo.Pump != PeristalticPump.FeedPump && PumpInfo.Pump != PeristalticPump.Feed2Pump)
            {
                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                {
                    PumpNo = PumpInfo.PumpIndex,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = PumpInfo.FlowRate_SP >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : PumpInfo.FlowRate_SP,
                    FlowCapacity = PumpInfo.FlowRate_SP * PumpInfo.RunningTime_SP / 3600f
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(PumpInfo.DeviceID, param);
                PumpInfo.IsControling = true;
                return;
            }
        });

        public PumpSettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            CurrentDeviceParameter = AnalysisSolution.GetInstance().ReactorCol[0];
        }

        public string Title => "泵详情";

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
