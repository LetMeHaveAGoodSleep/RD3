using Fpi.Util.WinApiUtil.CommDataType;
using HandyControl.Controls;
using HandyControl.Data;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using XZ.SQLite;

namespace RD3.ViewModels
{
    public class AlarmRecordViewModel : BaseViewModel
    {
        private DateTime _lastSearchTime;
        private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(300);
        private CancellationTokenSource _cancellationTokenSource;
        private string _condition = string.Empty;

        private ObservableCollection<AlarmRecord> _alarmRecordCol = new ObservableCollection<AlarmRecord>();
        public ObservableCollection<AlarmRecord> AlarmRecordCol { get { return _alarmRecordCol; } set { SetProperty(ref _alarmRecordCol, value); } }

        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand => new(PageUpdated);
        public DelegateCommand<FunctionEventArgs<string>> SearchCommand => new(async (FunctionEventArgs<string> e) =>
        {
            if (string.IsNullOrWhiteSpace(e.Info))
            {
                _condition = string.Empty;
            }
            else
            {
                _condition = $"where Code like '%{e.Info}%' or Source like '%{e.Info}%' or Module like '%{e.Info}%' or Grade like '%{e.Info}%' or Remark like '%{e.Info}%'" +
                $"or AlarmTime like '%{e.Info}%' or RemoveTime like '%{e.Info}%'";
            }
           await QueryAlarmRecordAsync();
        });

        private int _pageCount;
        public int PageCount
        {
            get { return _pageCount; }
            set { SetProperty(ref _pageCount, value); }
        }
        private int _pageIndex = 1;
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


        public AlarmRecordViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            QueryAlarmRecordAsync();
        }

        async Task QueryAlarmRecordAsync()
        {
            // 取消之前的搜索请求（如果有）
             _cancellationTokenSource?.Cancel();

            _cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            var now = DateTime.Now;
            _lastSearchTime = now;

            await Task.Delay(_debounceInterval);

            // 如果在等待期间有新的搜索请求，则跳过此次执行
            if ((DateTime.Now - _lastSearchTime) < _debounceInterval)
                return;

            try
            {
                await Task.Run(() =>
                {
                    DataTable dataTable = RD3SQLHelper.GetPaginationCount(nameof(AlarmRecord),_condition);
                    int dataCount = Convert.ToInt32(dataTable.Rows[0][0]);
                    PageCount = dataCount / DataCountPerPage + (dataCount % DataCountPerPage != 0 ? 1 : 0);
                    int startIndex = (PageIndex - 1) * DataCountPerPage + 1;
                    DataTable data = RD3SQLHelper.GetPaginationData(nameof(AlarmRecord), _condition, startIndex, DataCountPerPage);
                    var list = DataTableConverter.ConvertTo<AlarmRecord>(data);
                    AlarmRecordCol = [.. list];
                }, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // 搜索被取消，无需处理
            }
            catch (Exception ex)
            {
                // 处理其他异常
              HandyControl.Controls.MessageBox.Show($"搜索出错: {ex.Message}","温馨提示");
            }



        }

        private async void PageUpdated(FunctionEventArgs<int> info)
        {
            PageIndex = info.Info;
            await QueryAlarmRecordAsync();
        }
    }
}
