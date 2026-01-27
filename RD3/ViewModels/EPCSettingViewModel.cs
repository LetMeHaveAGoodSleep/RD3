using Microsoft.VisualBasic;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RD3.ViewModels
{
    public class EPCSettingViewModel : BaseViewModel,IDialogAware
    {
        public DeviceParameter CurrentDeviceParameter
        {
            get => AnalysisSolution.GetInstance().CurrentFermentor.Device;
        }

        public DelegateCommand StartCommand => new(() =>
        {
            if (HandyControl.Controls.MessageBox.Show($"是否设定罐压为【{CurrentDeviceParameter.EPCParam.SP}】MPa？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            InstrumentSolution.GetInstance().CommandWrapper.SetEPCPressure(CurrentDeviceParameter.Name, CurrentDeviceParameter.EPCParam.SP);
            //CurrentDeviceParameter.EPCParam.Enable = true;
            //CurrentDeviceParameter.EPCParam.IsControling = true;
        });

        public DelegateCommand StopCommand => new(() =>
        {
            CurrentDeviceParameter.EPCParam.Enable = false;
            CurrentDeviceParameter.EPCParam.IsControling = false;
        });

        public EPCSettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => "罐压控制";

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
