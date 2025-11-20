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
    public class CondensationSettingViewModel : BaseViewModel,IDialogAware
    {
        public DeviceParameter CurrentDeviceParameter
        {
            get => AnalysisSolution.GetInstance().CurrentFermentor.Device;
        }

        public DelegateCommand StartCommand => new(() => 
        {
            if (HandyControl.Controls.MessageBox.Show($"是否设定冷凝温度为【{CurrentDeviceParameter.CondensationParam.SP}】℃？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            CurrentDeviceParameter.CondensationParam.Enable = true;
            CurrentDeviceParameter.CondensationParam.IsControling = true;
        });

        public DelegateCommand StopCommand => new(() =>
        {
            CurrentDeviceParameter.CondensationParam.Enable = false;
            CurrentDeviceParameter.CondensationParam.IsControling = false;
        });

        public CondensationSettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => "冷凝控制";

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
