using ImTools;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using SixLabors.ImageSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class PadConfigurationViewModel : BaseViewModel, IDialogAware
    {
        #region  属性
        private PumpInfo _pumpInfo1;
        public PumpInfo PumpInfo1
        {
            get => _pumpInfo1;
            set { SetProperty(ref _pumpInfo1, value); }
        }

        private PumpInfo _pumpInfo2;
        public PumpInfo PumpInfo2
        {
            get => _pumpInfo2;
            set { SetProperty(ref _pumpInfo2, value); }
        }

        private PumpInfo _pumpInfo3;
        public PumpInfo PumpInfo3
        {
            get => _pumpInfo3;
            set { SetProperty(ref _pumpInfo3, value); }
        }

        private PumpInfo _pumpInfo4;
        public PumpInfo PumpInfo4
        {
            get => _pumpInfo4;
            set { SetProperty(ref _pumpInfo4, value); }
        }

        private PumpInfo _pumpInfo5;
        public PumpInfo PumpInfo5
        {
            get => _pumpInfo5;
            set { SetProperty(ref _pumpInfo5, value); }
        }

        private PumpInfo _pumpInfo6;
        public PumpInfo PumpInfo6
        {
            get => _pumpInfo6;
            set { SetProperty(ref _pumpInfo6, value); }
        }

        private MFCInfo _mfcInfo1;
        public MFCInfo MFCInfo1
        {
            get => _mfcInfo1;
            set { SetProperty(ref _mfcInfo1, value); }
        }

        private MFCInfo _mfcInfo2;
        public MFCInfo MFCInfo2
        {
            get => _mfcInfo2;
            set { SetProperty(ref _mfcInfo2, value); }
        }

        private MFCInfo _mfcInfo3;
        public MFCInfo MFCInfo3
        {
            get => _mfcInfo3;
            set { SetProperty(ref _mfcInfo3, value); }
        }

        private MFCInfo _mfcInfo4;
        public MFCInfo MFCInfo4
        {
            get => _mfcInfo4;
            set { SetProperty(ref _mfcInfo4, value); }
        }

        #endregion


        public DelegateCommand<string> ChangePumpCommand => new((string str) =>
        {
            try
            {
                int.TryParse(str, out int index);
                PumpInfo pumpInfo = AnalysisSolution.GetInstance().PumpInfoCol.FindFirst(t => t.PumpIndex == index);
                if (pumpInfo == null) return;
                if (pumpInfo.Pump != PeristalticPump.None)
                {
                    int count = AnalysisSolution.GetInstance().PumpInfoCol.Count(t => t.Pump == pumpInfo.Pump);
                    if (count > 1)
                    {
                        HandyControl.Controls.MessageBox.Info($"已存在{EnumUtil.GetEnumDescription(pumpInfo.Pump)}泵", "温馨提示");
                        pumpInfo.Pump = PeristalticPump.None;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        });

        public DelegateCommand<string> ChangeMFCCommand => new((string str) =>
        {
            try
            {
                int.TryParse(str, out int index);
                MFCInfo mfcInfo = AnalysisSolution.GetInstance().MFCInfoCol.FindFirst(t => t.MFCIndex == index);
                if (mfcInfo == null) return;
                if (mfcInfo.Gas != GasType.Unset)
                {
                    int count = AnalysisSolution.GetInstance().MFCInfoCol.Count(t => t.Gas == mfcInfo.Gas);
                    if (count > 1)
                    {
                        HandyControl.Controls.MessageBox.Info($"已存在{EnumUtil.GetEnumDescription(mfcInfo.Gas)}MFC", "温馨提示");
                        mfcInfo.Gas = GasType.Unset;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        });

        public PadConfigurationViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            PumpInfo1 = AnalysisSolution.GetInstance().PumpInfoCol.FindFirst(t => t.PumpIndex == 1);
            PumpInfo2 = AnalysisSolution.GetInstance().PumpInfoCol.FindFirst(t => t.PumpIndex == 2);
            PumpInfo3 = AnalysisSolution.GetInstance().PumpInfoCol.FindFirst(t => t.PumpIndex == 3);
            PumpInfo4 = AnalysisSolution.GetInstance().PumpInfoCol.FindFirst(t => t.PumpIndex == 4);
            PumpInfo5 = AnalysisSolution.GetInstance().PumpInfoCol.FindFirst(t => t.PumpIndex == 5);
            PumpInfo6 = AnalysisSolution.GetInstance().PumpInfoCol.FindFirst(t => t.PumpIndex == 6);

            MFCInfo1 = AnalysisSolution.GetInstance().MFCInfoCol.FindFirst(t => t.MFCIndex == 1);
            MFCInfo2 = AnalysisSolution.GetInstance().MFCInfoCol.FindFirst(t => t.MFCIndex == 2);
            MFCInfo3 = AnalysisSolution.GetInstance().MFCInfoCol.FindFirst(t => t.MFCIndex == 3);
            MFCInfo4 = AnalysisSolution.GetInstance().MFCInfoCol.FindFirst(t => t.MFCIndex == 4);
        }

        public string Title => "设备配置";

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
           
        }
    }
}
