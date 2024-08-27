using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class MCUDebugViewModel : NavigationViewModel
    {

        //private float _temp
        public MCUDebugViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
        }
    }
}
