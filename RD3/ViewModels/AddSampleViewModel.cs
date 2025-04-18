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
    public class AddSampleViewModel : NavigationViewModel, IDialogAware
    {
        public AddSampleViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {

        }

        public string Title => throw new NotImplementedException();

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            throw new NotImplementedException();
        }

        public void OnDialogClosed()
        {
            throw new NotImplementedException();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            throw new NotImplementedException();
        }
    }
}
