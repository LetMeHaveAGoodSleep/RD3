using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class AdaptpHViewModel : BaseViewModel, IDialogAware
    {

        private AdaptivepHParameter _adaptivepHParameter = new AdaptivepHParameter();
        public AdaptivepHParameter AdaptivepHParameter
        {
            get => _adaptivepHParameter;
            private set { SetProperty(ref _adaptivepHParameter, value); }
        }

        public string Title => "pH自适应调节参数";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
           
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            AdaptivepHParameter = parameters.GetValue<AdaptivepHParameter>(nameof(AdaptivepHParameter));
            if (AdaptivepHParameter == null && InstrumentSolution.GetInstance().CommunicationProtocol == 1)
            {
                AdaptivepHParameter = AnalysisSolution.GetInstance().CurrentFermentor.Device.AdaptivepHParameter;
            }
        }

        public AdaptpHViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }
    }
}
