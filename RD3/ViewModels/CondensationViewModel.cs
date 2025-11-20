using Prism.Commands;
using Prism.Ioc;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RD3.ViewModels
{
    public class CondensationViewModel : BaseViewModel
    {
        public DeviceParameter CurrentDeviceParameter
        {
            get => AnalysisSolution.GetInstance().CurrentFermentor.Device;
        }

        public DelegateCommand<RoutedEventArgs> CondensationSettingCommand => new((RoutedEventArgs e) => 
        {
            e.Handled = true;
            DialogHostService.ShowOnce(nameof(Views.CondensationSettingView), null, callback =>
            {
                if (callback.Result != Prism.Services.Dialogs.ButtonResult.OK) return;
            });
        });

        public CondensationViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }
    }
}
