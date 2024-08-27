using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using RD3.Common.Models;
using RD3.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    class CalibrateViewModel : NavigationViewModel
    {
        public CalibrateViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
        }
    }
}
