using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class ChooseDOE3DFactorViewModel : BaseViewModel, IDialogAware
    {
        private ObservableCollection<string> _doeColunms = [];
        public ObservableCollection<string> DoeColunms
        {
            get => _doeColunms;
            set => SetProperty(ref _doeColunms, value);
        }

        private string _xParameter;
        public string XParameter
        {
            get => _xParameter;
            set => SetProperty(ref _xParameter, value);
        }

        private string _yParameter;
        public string YParameter
        {
            get => _yParameter;
            set => SetProperty(ref _yParameter, value);
        }

        private string _zParameter;
        public string ZParameter
        {
            get => _zParameter;
            set => SetProperty(ref _zParameter, value);
        }

        public DelegateCommand OKCommand => new(() =>
        {
            DialogParameters keyValuePairs = new()
            {
                {"AxesParams",new List<string>(){XParameter,YParameter,ZParameter }}
            };
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, keyValuePairs));
        });

        public DelegateCommand CancelCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public ChooseDOE3DFactorViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => "选择响应面展示因子";

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
            DoeColunms = parameters.GetValue<ObservableCollection<string>>(nameof(DoeColunms));
            XParameter = DoeColunms.Count > 0 ? DoeColunms[0] : "";
            YParameter = DoeColunms.Count > 1 ? DoeColunms[1] : "";
            ZParameter = DoeColunms.Count > 2 ? DoeColunms[DoeColunms.Count - 1] : "";
        }
    }
}
