using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class MFCSettingViewModel : BaseViewModel, IDialogAware
    {
        private GasParam _gasParam = new();
        public GasParam GasParam
        {
            get { return _gasParam; }
            set { SetProperty(ref _gasParam, value); }
        }

        private DeviceParameter _currentDeviceParameter;
        public DeviceParameter CurrentDeviceParameter
        {
            get { return _currentDeviceParameter; }
            set { SetProperty(ref _currentDeviceParameter, value); }
        }

        private MFCInfo _mfcInfo = new();
        public MFCInfo MFCInfo
        {
            get => _mfcInfo;
            set 
            {
                SetProperty(ref _mfcInfo, value);
                if (value != null)
                {
                    switch (value.Gas)
                    {
                        case GasType.Air:
                            GasParam = CurrentDeviceParameter.AirParam;
                            break;
                        case GasType.CO2:
                            GasParam = CurrentDeviceParameter.CO2Param;
                            break;
                        case GasType.O2:
                            GasParam = CurrentDeviceParameter.O2Param;
                            break;
                        case GasType.N2:
                            GasParam = CurrentDeviceParameter.N2Param;
                            break;
                    }
                }
            }
        }

        public DelegateCommand MFCControlCommand => new(() =>
        {
            GasParam gasParam = new GasParam()
            {
                MFCNo = MFCInfo.MFCIndex,
                GasType = MFCInfo.Gas,
            };
            //关闭泵
            if (!MFCInfo.IsControling)
            {
                gasParam.FlowSpeed = 0;
                InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(MFCInfo.DeviceID, gasParam);
                return;
            }
            if (MFCInfo.FlowRate_SP < 0)
            {
                MFCInfo.IsControling = false;
                HandyControl.Controls.MessageBox.Warning("流量必须大于0", "温馨提示");
                return;
            }
            MFCInfo.FlowRate_SP = Math.Clamp(MFCInfo.FlowRate_SP, 0, 100);
            gasParam.FlowSpeed = MFCInfo.FlowRate_SP;
            InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(MFCInfo.DeviceID, gasParam);
        });

        public MFCSettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            CurrentDeviceParameter = AnalysisSolution.GetInstance().FermentorCol[0].Device;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                while (true)
                {
                    if (MFCInfo.MFCIndex != -1)
                    {
                        string deviceID = "G01";
                        foreach (var item in ClockSupervisor.realDatasDic.Keys)
                        {
                            deviceID = item;
                            break;
                        }
                        if (!ClockSupervisor.realDatasDic.ContainsKey(deviceID) || ClockSupervisor.realDatasDic[deviceID].Count < 1)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        int index = ClockSupervisor.realDatasDic[deviceID].Count - 1;
                        var realTimeParam = ClockSupervisor.realDatasDic[deviceID][index];

                        var type = realTimeParam.GetType();
                        var properties = type.GetProperties();
                        foreach (var item in properties.Where(t => t.CanRead && t.CanWrite))
                        {
                            if (item.Name == $"MFc{MFCInfo.MFCIndex}FlowRate")
                            {
                                object value = item.GetValue(realTimeParam);
                                MFCInfo.FlowRate = Convert.ToSingle(value);
                            }
                            else if (item.Name == $"Pump{MFCInfo.MFCIndex}FlowCapacity")
                            {
                                object value = item.GetValue(realTimeParam);
                                MFCInfo.FlowCapacity = Convert.ToSingle(value);
                            }
                        }
                    }
                    Thread.Sleep(1000);
                }
            };
            worker.RunWorkerAsync();
        }

        public string Title => "MFC设置";

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
            MFCInfo = parameters.GetValue<MFCInfo>(nameof(MFCInfo));
        }
    }
}
