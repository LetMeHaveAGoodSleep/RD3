using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class ChooseDOEAnalyseMethodViewModel : BaseViewModel,IDialogAware
    {
        private bool _is3DView = true;
        public bool Is3DView
        {
            get { return _is3DView; }
            set { SetProperty(ref _is3DView, value); }
        }

        public DelegateCommand OKCommand => new(() => 
        {
            DialogParameters keyValuePairs = new()
            {
                {nameof(Is3DView),Is3DView }
            };
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, keyValuePairs));
        });

        public DelegateCommand CancelCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public ChooseDOEAnalyseMethodViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => "选择分析方法";

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
        }
    }
}
