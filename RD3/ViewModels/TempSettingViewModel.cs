using ImTools;
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
using System.Windows;

namespace RD3.ViewModels
{
    public class TempSettingViewModel : BaseViewModel, IDialogAware
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

        public DelegateCommand ReadPIDCommand => new(() => 
        {
            CurrentDeviceParameter.TempParam.PIDParam = InstrumentSolution.GetInstance().CommandWrapper.GetTECPID(CurrentDeviceParameter.Name);
            HandyControl.Controls.MessageBox.Info("读取成功", "温馨提示");
        });

        public DelegateCommand SetPIDCommand => new(() => 
        {
            InstrumentSolution.GetInstance().CommandWrapper.SetTECPID(CurrentDeviceParameter.Name, CurrentDeviceParameter.TempParam.PIDParam);
            HandyControl.Controls.MessageBox.Info("写入成功", "温馨提示");
        });

        public DelegateCommand OpenCommand => new(() =>
        {
            if (BasicParam.TimeSeries.TimeSeriesItemCol.Count < 1 && BasicParam.ControlMode == ControlMode.TimeSeries)
            {
                HandyControl.Controls.MessageBox.Warning($"{CurrentDeviceParameter.Name}的温度时间序列为空", "温馨提示");
                return;
            }
            if (!CheckTimeSeries())
            {
                return;
            }
            if (HandyControl.Controls.MessageBox.Show($"是否开启温度控制？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            BasicParam.IsControling = true;
        });

        public DelegateCommand CloseCommand => new(() =>
        {
            if (HandyControl.Controls.MessageBox.Show($"是否停止温度控制？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            BasicParam.IsControling = false;
            BasicParam.TimeSeries.RunningInfo = string.Empty;
        });

        public TempSettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            BasicParam = CurrentDeviceParameter.TempParam;
        }

        public string Title => "温度设置";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return CheckTimeSeries();
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
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
                if (item.Value < BasicParam.LowerLimit || item.Value > BasicParam.UpperLimit)
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
