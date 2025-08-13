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
        public DeviceParameter CurrentDeviceParameter
        {
            get { return AnalysisSolution.GetInstance().ReactorCol[0]; }
        }

        public DelegateCommand ReadPIDCommand => new(() => 
        {
            CurrentDeviceParameter.TempParam.PIDParam = InstrumentSolution.GetInstance().CommandWrapper.GetTECPID(CurrentDeviceParameter.Name);
            //HandyControl.Controls.MessageBox.Show("读取成功", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Information);
        });

        public DelegateCommand SetPIDCommand => new(() => 
        {
            InstrumentSolution.GetInstance().CommandWrapper.SetTECPID(CurrentDeviceParameter.Name, CurrentDeviceParameter.TempParam.PIDParam);
        });

        public TempSettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => "温度设置";

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
