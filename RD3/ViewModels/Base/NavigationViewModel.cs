using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using RD3.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class NavigationViewModel :BaseViewModel,INavigationAware, IRegionMemberLifetime
    {
        public NavigationViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
        }

        /// <summary>
        /// 导航离开后默认不销毁原界面
        /// </summary>
        public bool KeepAlive => true;

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        public void UpdateLoading(bool IsOpen)
        {
            aggregator.UpdateLoading(new Common.Events.UpdateModel()
            {
                IsOpen = IsOpen
            });
        }
    }
}
