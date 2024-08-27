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
    public class NavigationViewModel : BindableBase, INavigationAware
    {
        public readonly IContainerProvider containerProvider;
        public readonly IEventAggregator aggregator;
        public readonly ILanguage Language;

        public NavigationViewModel(IContainerProvider containerProvider)
        {
            this.containerProvider = containerProvider;
            this.aggregator = containerProvider.Resolve<IEventAggregator>();
            this.Language = containerProvider.Resolve<ILanguage>();
            Language.LoadResourceKey("zh_CN");//默认显示中文
        }

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
