using HandyControl.Data;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class AlarmViewModel : NavigationViewModel
    {
       private ObservableCollection<AlarmRecord> _alarmRecordCol = new ObservableCollection<AlarmRecord>();
        public ObservableCollection<AlarmRecord> AlarmRecordCol { get { return _alarmRecordCol; }set { SetProperty(ref _alarmRecordCol, value); } }

        private ObservableCollection<AlarmRecord> _dataList = new ObservableCollection<AlarmRecord>();
        public ObservableCollection<AlarmRecord> DataList { get { return _dataList; } set { SetProperty(ref _dataList, value); } }
        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand => new(PageUpdated);

        private int _pageCount;
        public int PageCount
        {
            get { return _pageCount; }
            set { SetProperty(ref _pageCount, value); }
        }
        private int _pageIndex = -1;
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
        

        public AlarmViewModel(IContainerProvider containerProvider):base(containerProvider) 
        {
            QueryAlarmRecord();
            PageCount = DataList.Count / DataCountPerPage + (DataList.Count % DataCountPerPage != 0 ? 1 : 0);
            var data = DataList.Take(DataCountPerPage);
            AlarmRecordCol = new ObservableCollection<AlarmRecord>(data);
            _pageIndex = 1;
        }

        void QueryAlarmRecord()
        {
            try
            {
                string[] lines = File.ReadAllLines(FileConst.AlarmHistoryPath);
                foreach (string line in lines) 
                {
                    try
                    {
                        DataList.Add(ProcessAlarm(line));
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
        AlarmRecord ProcessAlarm(string line)
        {
            AlarmRecord record = new AlarmRecord();
            string[] array = line.Split('#');
            record.Time = DateTime.ParseExact(array[0], "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);
            record.Batch = array[1];
            record.Reactor = array[2];
            record.Value = array[3];
            record.Grade = (AlarmGrade)Enum.Parse(typeof(AlarmGrade), array[4]);
            record.Description = array[5];
            return record;
        }
        private void PageUpdated(FunctionEventArgs<int> info)
        {
            var data = DataList.Skip((info.Info - 1) * DataCountPerPage).Take(DataCountPerPage);
            AlarmRecordCol = new ObservableCollection<AlarmRecord>(data);
        }
    }
}
