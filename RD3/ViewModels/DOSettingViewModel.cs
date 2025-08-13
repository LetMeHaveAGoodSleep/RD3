using Fpi.Communication.Commands.Config;
using HandyControl.Controls;
using ImTools;
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
using System.Text;
using System.Threading.Tasks;

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


        public DelegateCommand AddAirCommand => new(() => 
        {
            Param.AirCol.Add(new CascadeParam() { StepValue= Param.AirCol[Param.AirCol.Count - 1].StepValue });
        });

        public DelegateCommand<object> DeleteAirCommand => new((object o) =>
        {
            CascadeParam param = o as CascadeParam;
            Param.AirCol.Remove(param);
        });

        public DelegateCommand AddO2Command => new(() =>
        {
            Param.O2Col.Add(new CascadeParam()
            {
                StepValue = Param.O2Col.Count > 0 ? Param.O2Col[Param.O2Col.Count - 1].StepValue : 0.1f
            });
        });

        public DelegateCommand<object> DeleteO2Command => new((object o) =>
        {
            CascadeParam param = o as CascadeParam;
            Param.O2Col.Remove(param);
        });

        public DelegateCommand AddTempCommand => new(() =>
        {
            Param.TempCol.Add(new CascadeParam()
            {
                StepValue = Param.TempCol.Count > 0 ? Param.TempCol[Param.TempCol.Count - 1].StepValue : 37f
            });
        });

        public DelegateCommand<object> DeleteTempCommand => new((object o) =>
        {
            CascadeParam param = o as CascadeParam;
            Param.TempCol.Remove(param);
        });

        public DelegateCommand AddFeedCommand => new(() =>
        {
            Param.FeedCol.Add(new CascadeParam()
            {
                StepValue = Param.FeedCol.Count > 0 ? Param.FeedCol[Param.FeedCol.Count - 1].StepValue : 80f
            });
        });

        public DelegateCommand<object> DeleteFeedCommand => new((object o) =>
        {
            CascadeParam param = o as CascadeParam;
            Param.FeedCol.Remove(param);
        });


        public DelegateCommand OKCommand => new(() =>
        {
            if (CurrentDeviceParameter.DOParam.ControlStrategy == DOControlStrategy.Midranging)
            {
                MidRangingParamManager.GetInstance().Save(MidRangingCol);
            }
            else
            {
                Param.AirCol = new ObservableCollection<CascadeParam>(Param.AirCol.OrderBy(t => t.StepValue));
                DOAssManager.GetInstance().Save(ParamCol);
            }

            DialogParameters Parameters = new DialogParameters { { nameof(DOAssParam), Param }, { nameof(MidRangingParam),MidRanging },{"Flag", CurrentDeviceParameter.DOParam.ControlStrategy == DOControlStrategy.Midranging } };
            DialogResult dialogResult = new DialogResult(ButtonResult.OK, Parameters);
            RequestClose?.Invoke(dialogResult);
        });

        public DelegateCommand<IList> ChangeFatorCommand => new((IList list) => 
        {
            switch (CurrentDeviceParameter.DOParam.ControlStrategy)
            {
                case DOControlStrategy.Step:
                    Param.FactorCol.Clear();

                    for (int i = 0; i < list.Count; i++)
                    {
                        foreach (var item in EnumUtil.GetEnumDescriptions<DOControlFactor>())
                        {
                            if (item == (list[i] as TransferItem)?.Content?.ToString())
                            {
                                Param.FactorCol.Add(EnumUtil.GetEnumByDescription<DOControlFactor>(item));
                            }
                        }
                    }

                    List<string> stringList = list.Cast<object>().Select(item => (item as TransferItem)?.Content?.ToString())
                    .Where(content => content != null).ToList();

                    Param.FactorContent = "执行顺序：" + string.Join("-", stringList);
                    break;
                case DOControlStrategy.Midranging:
                    MidRanging.FactorCol.Clear();

                    for (int i = 0; i < list.Count; i++)
                    {
                        foreach (var item in EnumUtil.GetEnumDescriptions<DOControlFactor>())
                        {
                            if (item == (list[i] as TransferItem)?.Content?.ToString())
                            {
                                MidRanging.FactorCol.Add(EnumUtil.GetEnumByDescription<DOControlFactor>(item));
                            }
                        }
                    }

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
                Param.AirCol = new ObservableCollection<CascadeParam>(Param.AirCol.OrderBy(t => t.StepValue));
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

            Param.FactorContent= "执行顺序：" + string.Join("-", list);

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
