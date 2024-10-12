using HandyControl.Data;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static MaterialDesignThemes.Wpf.Theme;

namespace RD3.ViewModels
{
    public class AuditViewModel : NavigationViewModel, IDialogAware
    {
        private bool _isAlarm = false;
        public bool IsAlarm
        {
            get { return _isAlarm; }
            set { SetProperty(ref _isAlarm, value); }
        }

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
            if (!IsAlarm)
            {
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
            }
            else
            {
                QueryAlarmRecord();
                AlarmRecords = new ObservableCollection<AlarmRecord>(DataList);
                if (string.IsNullOrEmpty(key))
                {
                    var collection = AlarmRecords.Where(t => t.Time <= endTime && t.Time >= startTime);
                    AlarmRecords = new ObservableCollection<AlarmRecord>(collection);
                }
                else
                {
                    var collection = AlarmRecords.Where(t => (t.Batch.Contains(key) || t.Description.Contains(key) || t.Reactor.Contains(key)
                    || t.Grade.ToString().Contains(key) || t.Value.Contains(key) || t.Description.Contains(key)) && t.Time <= endTime && t.Time >= startTime);
                    AlarmRecords = new ObservableCollection<AlarmRecord>(collection);
                }
            }
            PageUpdated(new FunctionEventArgs<int>(PageIndex));
        });
        public DelegateCommand ExportCommand => new(() =>
        {
            if (IsAlarm)
            {
                QueryAlarmRecord();
                var csv = new StringBuilder();
                var headers = new List<string>()
                {
                    Language.GetValue("Alarm Time").ToString(), Language.GetValue("Batch").ToString(),
                    Language.GetValue("Reactor").ToString(), Language.GetValue("Description").ToString(),
                    Language.GetValue("Grade").ToString(), Language.GetValue("Value").ToString()
                };
                csv.AppendLine(string.Join(",", headers));

                foreach (var record in DataList)
                {
                    var rowValues = new List<object>()
                {
                    record.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    record.Batch,
                    record.Reactor,
                    record.Description,
                    record.Grade,
                    record.Value
                };
                    csv.AppendLine(string.Join(",", rowValues));
                }
                string fileName = Language.GetValue("AlarmRecord").ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = fileName,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, csv.ToString());
                }
            }
            else
            {
                var csv = new StringBuilder();
                var headers = new List<string>()
                {
                    Language.GetValue("Occurrence Time").ToString(), Language.GetValue("Batch").ToString(),
                    Language.GetValue("Reactor").ToString(), Language.GetValue("Module").ToString(),
                    Language.GetValue("Operation").ToString(),
                    Language.GetValue("Description").ToString()
                };
                csv.AppendLine(string.Join(",", headers));

                foreach (var operation in Operations)
                {
                    var rowValues = new List<object>()
                {
                    operation.OccurrenceTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    operation.Batch,
                    operation.Reactor,
                    operation.Module,
                    operation.OperationStatement,
                    operation.Description
                };
                    csv.AppendLine(string.Join(",", rowValues));
                }
                string fileName = Language.GetValue("Audit").ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = fileName,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, csv.ToString());
                }
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
            QueryAlarmRecord();
        }

        void QueryAlarmRecord()
        {
            try
            {
                DataList.Clear();
                string[] lines = File.ReadAllLines(FileConst.AlarmHistoryPath);
                foreach (string line in lines)
                {
                    try
                    {
                        var temp = JsonConvert.DeserializeObject<AlarmRecord>(line);
                        DataList.Add(temp);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
            }
        }

        private void PageUpdated(FunctionEventArgs<int> info)
        {

            if (!IsAlarm)
            {
                PageCount = Operations.Count / DataCountPerPage + (Operations.Count % DataCountPerPage != 0 ? 1 : 0);
                var data = Operations.Skip((info.Info - 1) * DataCountPerPage).Take(DataCountPerPage);
                OperationCol = new ObservableCollection<Operation>(data);
            }
            else
            {
                PageCount = AlarmRecords.Count / DataCountPerPage + (AlarmRecords.Count % DataCountPerPage != 0 ? 1 : 0);
                var data = AlarmRecords.Skip((info.Info - 1) * DataCountPerPage).Take(DataCountPerPage);
                AlarmRecordCol = new ObservableCollection<AlarmRecord>(data);
            }
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
