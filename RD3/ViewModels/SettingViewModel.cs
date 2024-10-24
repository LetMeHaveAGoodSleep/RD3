using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class SettingViewModel: NavigationViewModel
    {

        private bool _isSimulation;
        public bool IsSimulation
        {
            get => _isSimulation;
            set 
            { 
                SetProperty(ref _isSimulation, value);
                VarConfig.SetValue("IsSimulation", value);
            }
        }

        public SettingViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
            IsSimulation = Convert.ToBoolean(VarConfig.GetValue("IsSimulation"));
        }
    }
}
