using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class BatchViewModel : NavigationViewModel
    {
        public BatchViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
        }
    }
}
