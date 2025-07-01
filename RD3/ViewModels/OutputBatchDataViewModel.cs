using ImTools;
using Prism.Commands;
using Prism.Events;
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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using XZ.SQLite;

namespace RD3.ViewModels
{
    public class OutputBatchDataViewModel : BaseViewModel,IDialogAware
    {
        private readonly IDialogService _dialogService;

        public string Title => "批次数据导出";

        public event Action<IDialogResult> RequestClose;


        public DelegateCommand CancelCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public DelegateCommand<object> SaveDatasCommand => new((object o) =>
        {
            DataInfo dataInfo = (DataInfo)o;
            if (!string.IsNullOrEmpty(dataInfo.path))
            {
                //读取数据
                string dateTime = "dateTime like '%'";//"dateTime like '%:00'";
                if(dataInfo.second == 10)
                {
                    dateTime = "(dateTime like '%:00' or dateTime like '%:20' or dateTime like '%:30' or dateTime like '%:40' or dateTime like '%:50')";
                }
                if (dataInfo.second == 30)
                {
                    dateTime = "(dateTime like '%:00' or dateTime like '%:30')";
                }
                if (dataInfo.second == 60)
                {
                    dateTime = "dateTime like '%:00'";
                }
                if (dataInfo.second == 300)
                {
                    dateTime = "(dateTime like '%00:00' or dateTime like '%05:00' or dateTime like '%10:00' or dateTime like '%15:00' or dateTime like '%20:00' or dateTime like '%25:00' or dateTime like '%30:00'  or dateTime like '%35:00' or dateTime like '%40:00' or dateTime like '%45:00' or dateTime like '%50:00' or dateTime like '%55:00')";
                }

                string sql = $"select * from {RD3SQLHelper.realTimeParamTable1} where deviceID = '{currentBatch.devieceID}' and batchID = '{currentBatch.ID}' and {dateTime} order by ID desc";
                DataTable dt = SQLiteHelper.GetDatasToDataTable(sql);
                //写入数据
                StringBuilder sb = new StringBuilder();
                string title = "";
                for (int i = 1; i < dt.Columns.Count; i++)
                {
                    title += $"{dt.Columns[i].ColumnName},";
                }
                title = title.TrimEnd(',') ;
                title += "\r\n";
                sb.Append(title);

                foreach (DataRow dr in dt.Rows)
                {
                    string lineContent = "";

                    for (int i = 1; i < dt.Columns.Count; i++)
                    {
                        object obj = dr[i];
                        if(obj == DBNull.Value)
                        {
                            lineContent += ",";
                        }
                        else
                        {
                            lineContent += $"{obj.ToString()},";
                        }
                    }
                    lineContent = lineContent.TrimEnd(',');
                    lineContent += "\r\n";
                    sb.Append(lineContent);
                }
                File.WriteAllText(dataInfo.path,sb.ToString());
            }
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        });
        
        public OutputBatchDataViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            _dialogService = dialogHostService;
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }
        public RD3Batch currentBatch = null;
        public void OnDialogOpened(IDialogParameters parameters)
        {
            currentBatch = parameters.GetValue<RD3Batch>("Batch");
        }
    }
}
