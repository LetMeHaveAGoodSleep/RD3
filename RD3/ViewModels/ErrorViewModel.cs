using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class ErrorViewModel : NavigationViewModel
    {
        public ErrorViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }
    }
}
