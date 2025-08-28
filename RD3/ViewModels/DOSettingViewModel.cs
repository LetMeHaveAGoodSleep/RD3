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

namespace RD3.ViewModels
{
    public class DOSettingViewModel : BaseViewModel, IDialogAware
    {
        public DeviceParameter CurrentDeviceParameter
        {
            get { return AnalysisSolution.GetInstance().ReactorCol[0]; }
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

        public DelegateCommand<IList> ChangeFatorCommand => new((IList list) =>
        {
            switch (CurrentDeviceParameter.DOParam.ControlStrategy)
            {
                case DOControlStrategy.Step:
                    Param.FactorCol.Clear();
                    ObservableCollection<DOControlFactor> col = [];
                    for (int i = 0; i < list.Count; i++)
                    {
                        foreach (var item in EnumUtil.GetEnumDescriptions<DOControlFactor>())
                        {
                            if (item == (list[i] as TransferItem)?.Content?.ToString())
                            {
                                col.Add(EnumUtil.GetEnumByDescription<DOControlFactor>(item));
                            }
                        }
                    }
                    Param.FactorCol = col;

                    List<string> stringList = list.Cast<object>().Select(item => (item as TransferItem)?.Content?.ToString())
                    .Where(content => content != null).ToList();

                    Param.FactorContent = "执行顺序：" + string.Join("-", stringList);
                    break;
                case DOControlStrategy.Midranging:
                    MidRanging.FactorCol.Clear();
                    ObservableCollection<DOControlFactor> col1 = [];
                    for (int i = 0; i < list.Count; i++)
                    {
                        foreach (var item in EnumUtil.GetEnumDescriptions<DOControlFactor>())
                        {
                            if (item == (list[i] as TransferItem)?.Content?.ToString())
                            {
                                col1.Add(EnumUtil.GetEnumByDescription<DOControlFactor>(item));
                            }
                        }
                    }
                    MidRanging.FactorCol = col1;

                    stringList = list.Cast<object>().Select(item => (item as TransferItem)?.Content?.ToString())
                     .Where(content => content != null).ToList();

                    MidRanging.FactorContent = "执行顺序：" + string.Join("-", stringList);
                    break;
            }
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

        public DOSettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {

        }

        public string Title { get; set; }

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
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

            //if (Param.FactorCol.Contains(DOControlFactor.Air))
            //{
            //    Param.AirEnable = true;
            //}
            //else
            //{
            //    Param.AirEnable = false;
            //}

            //if (Param.FactorCol.Contains(DOControlFactor.O2))
            //{
            //    Param.O2Enable = true;
            //}
            //else
            //{
            //    Param.O2Enable = false;
            //}

            //if (Param.FactorCol.Contains(DOControlFactor.Temp))
            //{
            //    Param.TempEnable = true;
            //}
            //else
            //{
            //    Param.TempEnable = false;
            //}

            //if (Param.FactorCol.Contains(DOControlFactor.Feed))
            //{
            //    Param.FeedEnable = true;
            //}
            //else
            //{
            //    Param.FeedEnable = false;
            //}

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
    }
}
