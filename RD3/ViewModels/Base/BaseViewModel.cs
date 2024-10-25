using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using RD3.Common;
using RD3.Shared;
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
        public readonly ICommandWrapper CommandWrapper;

        public BaseViewModel(IContainerProvider containerProvider)
        {
            this.containerProvider = containerProvider;
            this.aggregator = containerProvider.Resolve<IEventAggregator>();
            this.Language = containerProvider.Resolve<ILanguage>();
            if (AppSession.LanguageName == Const.CHNLanguage)
            {
                Language.LoadResourceKey("zh_CN");//默认显示中文
            }
            else
            {
                Language.LoadResourceKey("en_US");
            }

            CommandWrapper = InstrumentSolution.GetInstance().CommandWrapper;
        }
    }
}
