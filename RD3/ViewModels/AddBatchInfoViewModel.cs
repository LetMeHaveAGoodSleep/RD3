using CustomApp;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XZ.SQLite;

namespace RD3.ViewModels
{
    public class AddBatchInfoViewModel : BaseViewModel, IDialogAware
    {
        private string _strain = "菌种信息";
        public string Strain
        {
            get { return _strain; }
            set { SetProperty(ref _strain, value); }
        }

        private string _tester = "测试人员";
        public string Tester
        {
            get { return _tester; }
            set { SetProperty(ref _tester, value); }
        }

        private string _remark = "备注";
        public string Remark
        {
            get { return _remark; }
            set { SetProperty(ref _remark, value); }
        }

        public DelegateCommand CloseCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public DelegateCommand OKCommand => new(() =>
        {
            DialogParameters keyValuePairs = new DialogParameters();
            keyValuePairs.Add(nameof(Batch), Tuple.Create(Strain, Tester, Remark));
            DialogResult dialogResult = new DialogResult(ButtonResult.OK, keyValuePairs);
            RequestClose?.Invoke(dialogResult);
        });

        private string _title;
        public string Title => _title;

        public AddBatchInfoViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            
        }

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        /// <summary>
        /// 打开窗口
        /// 带参数
        /// </summary>
        /// <param name="parameters"></param>
        public void OnDialogOpened(IDialogParameters parameters)
        {
            string deviceName = parameters.GetValue<string>("DeviceName");
            _title = $"{deviceName}-新建批次";
        }
    }
}
