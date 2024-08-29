using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class ControlViewModel : NavigationViewModel
    {
        public ControlViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
        }
    }
}
