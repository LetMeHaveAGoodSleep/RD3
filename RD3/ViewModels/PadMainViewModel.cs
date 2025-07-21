using Prism.Ioc;
using RD3.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class PadMainViewModel : BaseViewModel
    {
        public PadMainViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }
    }
}
