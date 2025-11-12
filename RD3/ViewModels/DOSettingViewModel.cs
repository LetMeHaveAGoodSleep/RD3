using HandyControl.Controls;
using ImTools;
using MathNet.Symbolics;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using RD3.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace RD3.ViewModels
{
    public class DOSettingViewModel : BaseViewModel, IDialogAware
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

        private DOAssParam _param = new DOAssParam();
        public DOAssParam Param
        {
            get { return _param; }
            set { SetProperty(ref _param, value); }
        }

        private ObservableCollection<DOAssParam> _paramCol = new ObservableCollection<DOAssParam>();
        public ObservableCollection<DOAssParam> ParamCol
        {
            get { return _paramCol; }
            set { SetProperty(ref _paramCol, value); }
        }

        private MidRangingParam _midRanging = new MidRangingParam();
        public MidRangingParam MidRanging
        {
            get { return _midRanging; }
            set { SetProperty(ref _midRanging, value); }
        }

        private ObservableCollection<MidRangingParam> _midRangingCol = new ObservableCollection<MidRangingParam>();
        public ObservableCollection<MidRangingParam> MidRangingCol
        {
            get { return _midRangingCol; }
            set { SetProperty(ref _midRangingCol, value); }
        }

        public DelegateCommand DeleteAllFactorCommand => new(() =>
        {
            Param.FactorCol.Clear();
            Param.FactorContent = "执行顺序：" + string.Join("-", Param.FactorCol);
        });

        public DelegateCommand AddCascadeCommand => new(() =>
        {
            if (Param.CascadeCol.Count > 0)
            {
                Param.CascadeCol.Add(new CascadeParam()
                {
                    AirFlowRate = Param.CascadeCol[Param.CascadeCol.Count - 1].AirFlowRate,
                    O2FlowRate = Param.CascadeCol[Param.CascadeCol.Count - 1].O2FlowRate,
                    Temp = Param.CascadeCol[Param.CascadeCol.Count - 1].Temp,
                    FeedFlowRate = Param.CascadeCol[Param.CascadeCol.Count - 1].FeedFlowRate,
                });
            }
            else
            {
                Param.CascadeCol.Add(new CascadeParam());
            }
        });

        public DelegateCommand<object> DeleteCascadeCommand => new((object o) =>
        {
            CascadeParam param = o as CascadeParam;
            Param.CascadeCol.Remove(param);
        });

        public DelegateCommand OKCommand => new(() =>
        {
            if (CurrentDeviceParameter.DOParam.ControlStrategy == DOControlStrategy.Midranging)
            {
                MidRangingParamManager.GetInstance().Save(MidRangingCol);
            }
            else
            {
                DOAssManager.GetInstance().Save(ParamCol);
            }

            DialogParameters Parameters = new DialogParameters { { nameof(DOAssParam), Param }, { nameof(MidRangingParam), MidRanging }, { "Flag", CurrentDeviceParameter.DOParam.ControlStrategy == DOControlStrategy.Midranging } };
            DialogResult dialogResult = new DialogResult(ButtonResult.OK, Parameters);
            RequestClose?.Invoke(dialogResult);
        });

        public DelegateCommand PIDSettingCommand => new(() =>
        {
            DialogHostService.ShowOnce(nameof(PadDOPIDView), callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });

        public DelegateCommand<DOControlFactor?> FactorUpCommand => new((DOControlFactor? factor) => 
        {
            int index = -1;
            if (!factor.HasValue) return;
            switch (CurrentDeviceParameter.DOParam.ControlStrategy)
            {
                case DOControlStrategy.Step:
                    index = Param.FactorCol.IndexOf((DOControlFactor)factor);
                    if (index < 1)
                    {
                        return;
                    }
                    Param.FactorCol.Remove((DOControlFactor)factor);
                    Param.FactorCol.Insert(index - 1, (DOControlFactor)factor);
                    break;
                case DOControlStrategy.Midranging:
                    index = MidRanging.FactorCol.IndexOf((DOControlFactor)factor);
                    if (index < 1)
                    {
                        return;
                    }
                    MidRanging.FactorCol.Remove((DOControlFactor)factor);
                    MidRanging.FactorCol.Insert(index - 1, (DOControlFactor)factor);
                    break;
            }
        });

        public DelegateCommand<DOControlFactor?> FactorDownCommand => new((DOControlFactor? factor) => 
        {
            int index = -1;
            if (!factor.HasValue) return;
            switch (CurrentDeviceParameter.DOParam.ControlStrategy)
            {
                case DOControlStrategy.Step:
                    index = Param.FactorCol.IndexOf((DOControlFactor)factor);
                    if (index == Param.FactorCol.Count - 1)
                    {
                        return;
                    }
                    Param.FactorCol.Remove((DOControlFactor)factor);
                    Param.FactorCol.Insert(index + 1, (DOControlFactor)factor);
                    break;
                case DOControlStrategy.Midranging:
                    Param.FactorCol.Insert(index + 1, (DOControlFactor)factor);
                    index = MidRanging.FactorCol.IndexOf((DOControlFactor)factor);
                    if (index == MidRanging.FactorCol.Count - 1)
                    {
                        return;
                    }
                    MidRanging.FactorCol.Remove((DOControlFactor)factor);
                    MidRanging.FactorCol.Insert(index + 1, (DOControlFactor)factor);
                    break;
            }
        });

        public DelegateCommand OpenCommand => new(() =>
        {
            if (BasicParam.TimeSeries.TimeSeriesItemCol.Count < 1 && BasicParam.ControlMode == ControlMode.TimeSeries)
            {
                HandyControl.Controls.MessageBox.Warning($"{CurrentDeviceParameter.Name}的溶氧时间序列为空", "温馨提示");
                return;
            }
            if (!CheckTimeSeries())
            {
                return;
            }
            if (HandyControl.Controls.MessageBox.Show($"是否开启溶氧控制？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            BasicParam.IsControling = true;
        });

        public DelegateCommand CloseCommand => new(() =>
        {
            if (HandyControl.Controls.MessageBox.Show($"是否停止溶氧控制？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            BasicParam.IsControling = false;
            BasicParam.TimeSeries.RunningInfo = string.Empty;
        });

        public DOSettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            BasicParam = CurrentDeviceParameter.DOParam;
        }

        public string Title { get; set; }

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return CheckTimeSeries();
        }

        public void OnDialogClosed()
        {
            if (CurrentDeviceParameter.DOParam.ControlStrategy == DOControlStrategy.Midranging)
            {
                MidRangingParamManager.GetInstance().Save(MidRangingCol);
            }
            else
            {
                DOAssManager.GetInstance().Save(ParamCol);
            }
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Title = "溶氧控制策略";

            ParamCol = DOAssManager.GetInstance().DOAssParamCol;
            Param = ParamCol.FindFirst(t => t.DeviceName == CurrentDeviceParameter.Name);
            if (Param == null)
            {
                Param = new();
            }

            var list = Param.FactorCol.Select(t => EnumUtil.GetEnumDescription(t)).ToList();

            Param.FactorContent = "执行顺序：" + string.Join("-", list);

            MidRangingCol = MidRangingParamManager.GetInstance().MidRangingParamCol;
            MidRanging = MidRangingCol.FindFirst(t => t.DeviceName == CurrentDeviceParameter.Name);
            if (MidRanging == null)
            {
                MidRanging = new();
            }

            var list1 = MidRanging.FactorCol.Select(t => EnumUtil.GetEnumDescription(t)).ToList();

            MidRanging.FactorContent = "执行顺序：" + string.Join("-", list1);
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
