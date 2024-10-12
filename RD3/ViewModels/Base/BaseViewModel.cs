using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class BaseViewModel : BindableBase
    {
        public readonly IContainerProvider containerProvider;
        public readonly IEventAggregator aggregator;
        public readonly ILanguage Language;

        public BaseViewModel(IContainerProvider containerProvider)
        {
            this.containerProvider = containerProvider;
            this.aggregator = containerProvider.Resolve<IEventAggregator>();
            this.Language = containerProvider.Resolve<ILanguage>();
            Language.LoadResourceKey("zh_CN");//默认显示英文
        }
    }
}
