using HandyControl.Data;
using ImTools;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Extensions;
using RD3.Shared;
using RD3.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XZ.SQLite;

namespace RD3.ViewModels
{
    public class CompareBatchViewModel : BaseViewModel, IDialogAware
    {
        private TimeInterval _selectedTimeInterval = TimeInterval.Second;
        public TimeInterval SelectedTimeInterval
        {
            get=> _selectedTimeInterval;
            set 
            {
                SetProperty(ref _selectedTimeInterval, value);
                AppSession.BatchTimeInterval = value;
            }
        }

        public List<RD3Batch> batches;

        private readonly IDialogService dialogService;

        public string Title => "批次比较";

        public event Action<IDialogResult> RequestClose;

        public DelegateCommand CloseCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.OK)));

        public DelegateCommand<object> ExportCommand => new((object o) =>
        {
           
        });

        /// <summary>
        /// 增加离线数据
        /// </summary>
        public DelegateCommand AddOffLineDatasCommand => new(() =>
        {
            DialogHostService.ShowOnce(nameof(AddOfflineDatasView), callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        }
        );

        public CompareBatchViewModel(IContainerProvider containerProvider, IDialogService dialog, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            dialogService = dialog;
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            AppSession.SelectedBatches = [];
            AppSession.BatchTimeInterval = TimeInterval.Second;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            batches = parameters.GetValue<List<RD3Batch>>("Batches");
            double maxDay = 0;
            foreach (RD3Batch batch in batches)
            {
                try
                {
                    
                    string createTime = SQLiteHelper.GetValue(RD3SQLHelper.BatchTable, "createTime", $"ID ={batch.ID}")?.ToString();
                    string endDateTime = SQLiteHelper.GetValue(RD3SQLHelper.BatchTable, "endDateTime", $"ID ={batch.ID}")?.ToString();
                    DateTime.TryParse(createTime, out var time1);
                    if (!DateTime.TryParse(endDateTime, out var time2))
                    {
                        time2 = DateTime.Now;
                    }
                    var totalDay = (time2 - time1).TotalDays;
                    maxDay = totalDay > maxDay ? totalDay : maxDay;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            if (maxDay > 100)
            {
                SelectedTimeInterval = TimeInterval.Hour;
            }
            else if (maxDay > 1)
            {
                SelectedTimeInterval = TimeInterval.Minute;
            }
            AppSession.SelectedBatches = batches;
        }
    }
}
