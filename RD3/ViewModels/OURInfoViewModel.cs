using Prism.Commands;
using Prism.Ioc;
using RD3.Common;
using RD3.Shared;
using RD3.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RD3.ViewModels
{
    public class OURInfoViewModel : BaseViewModel
    {
        public DeviceParameter CurrentDeviceParameter
        {
            get => AnalysisSolution.GetInstance().CurrentFermentor.Device;
        }

        public OURInfoViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }
    }
}
