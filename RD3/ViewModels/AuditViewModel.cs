using HandyControl.Data;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class AuditViewModel : NavigationViewModel, IDialogAware
    {
        private ObservableCollection<Operation> _operationCol = [];
        public ObservableCollection<Operation> OperationCol { get { return _operationCol; } set { SetProperty(ref _operationCol, value); } }

        private ObservableCollection<Operation> _operations = [];
        public ObservableCollection<Operation> Operations { get { return _operations; } set { SetProperty(ref _operations, value); } }

        private ObservableCollection<AlarmRecord> _alarmRecordCol = [];
        public ObservableCollection<AlarmRecord> AlarmRecordCol { get { return _alarmRecordCol; } set { SetProperty(ref _alarmRecordCol, value); } }

        private ObservableCollection<AlarmRecord> _dataList = [];
        public ObservableCollection<AlarmRecord> DataList { get { return _dataList; } set { SetProperty(ref _dataList, value); } }

        private ObservableCollection<AlarmRecord> _alarmRecords = new ObservableCollection<AlarmRecord>();
        public ObservableCollection<AlarmRecord> AlarmRecords { get { return _alarmRecords; } set { SetProperty(ref _alarmRecords, value); } }

        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand => new(PageUpdated);

        public DelegateCommand<Tuple<string, string, string>> FilterCommand => new((Tuple<string, string, string> tuple) =>
        {
            DateTime startTime = string.IsNullOrEmpty(tuple.Item1) ? DateTime.MinValue : Convert.ToDateTime(tuple.Item1);
            DateTime endTime = string.IsNullOrEmpty(tuple.Item2) ? DateTime.MaxValue : Convert.ToDateTime(tuple.Item2);
            var key = tuple.Item3;
            Operations = new ObservableCollection<Operation>(OperationManager.GetInstance().Operations);
            if (string.IsNullOrEmpty(key))
            {
                var collection = Operations.Where(t => t.OccurrenceTime <= endTime && t.OccurrenceTime >= startTime);
                Operations = new ObservableCollection<Operation>(collection);
            }
            else
            {
                var collection = Operations.Where(t => (t.Batch.Contains(key) || t.Description.Contains(key) || t.Reactor.Contains(key)
                || t.OperationStatement.Contains(key) || t.Description.Contains(key)) && t.OccurrenceTime <= endTime && t.OccurrenceTime >= startTime);
                Operations = new ObservableCollection<Operation>(collection);
            }
            PageCount = Operations.Count / DataCountPerPage + (Operations.Count % DataCountPerPage != 0 ? 1 : 0);
            if (PageIndex != 1)
            {
                PageIndex = 1;
            }
            else
            {
                var data = Operations.Take(DataCountPerPage);
                OperationCol = new ObservableCollection<Operation>(data);
            }
        });

        private int _pageCount;
        public int PageCount
        {
            get { return _pageCount; }
            set { SetProperty(ref _pageCount, value); }
        }
        private int _pageIndex;
        public int PageIndex
        {
            get { return _pageIndex; }
            set { SetProperty(ref _pageIndex, value); }
        }

        private int _dataCountPerPage = 10;
        public int DataCountPerPage
        {
            get { return _dataCountPerPage; }
            set { SetProperty(ref _dataCountPerPage, value); }
        }

        public AuditViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
            Operations = new ObservableCollection<Operation>(OperationManager.GetInstance().Operations);
            _pageIndex = 1;
            PageCount = Operations.Count / DataCountPerPage + (Operations.Count % DataCountPerPage != 0 ? 1 : 0);
            var data = Operations.Take(DataCountPerPage);
            OperationCol = new ObservableCollection<Operation>(data);
        }

        private void PageUpdated(FunctionEventArgs<int> info)
        {
            var data = Operations.Skip((info.Info - 1) * DataCountPerPage).Take(DataCountPerPage);
            OperationCol = new ObservableCollection<Operation>(data);
        }

        public string Title => AppSession.CompanyName;

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
