using Fpi.Communication.Commands.Config;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Extensions;
using RD3.Shared;
using RD3.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RD3.ViewModels
{
    public class TimeSeriesViewModel : BaseViewModel
    {
        private BasicParam _basicParam;
        public BasicParam BasicParam
        {
            get => _basicParam;
            set { SetProperty(ref _basicParam, value); }
        }

        public void Sort()
        {
            BasicParam.TimeSeries.TimeSeriesItemCol = new ObservableCollection<TimeSeriesItem>(
    BasicParam.TimeSeries.TimeSeriesItemCol.OrderBy(t => t.StartTime).ThenBy(t => t.EndTime));
        }

        public DelegateCommand CopyCommand => new(() =>
        {
            BasicParam.TimeSeries.TimeSeriesItemCol = new ObservableCollection<TimeSeriesItem>(BasicParam.TimeSeries.TimeSeriesItemCol.OrderBy(t => new { t.StartTime, t.EndTime }));
            if (BasicParam.TimeSeries.TimeSeriesItemCol.GroupBy(x => new { x.StartTime, x.EndTime }).Any(g => g.Count() > 1))
            {
                HandyControl.Controls.MessageBox.Warning("存在相同的时间项", "温馨提示");
                return;
            }
        });

        public DelegateCommand AddCommand => new(() =>
        {
            Sort();
            double startTime = 0;
            double endTime = 1;
            double value = BasicParam.LowerLimit;
            if (BasicParam.TimeSeries.TimeSeriesItemCol.Count > 0)
            {
                startTime = BasicParam.TimeSeries.TimeSeriesItemCol.Last().EndTime;
                endTime = startTime + 30;
                value = BasicParam.TimeSeries.TimeSeriesItemCol[BasicParam.TimeSeries.TimeSeriesItemCol.Count - 1].Value;
            }
            BasicParam.TimeSeries.TimeSeriesItemCol.Add(new TimeSeriesItem() { StartTime = startTime, EndTime = endTime, Value = value });
            Sort();
        });

        public DelegateCommand<object> InsertCommand => new((object o) =>
        {
            TimeSeriesItem item = o as TimeSeriesItem;
            TimeSeriesItem copy = item.Clone() as TimeSeriesItem;
            int index = 0;
            index = BasicParam.TimeSeries.TimeSeriesItemCol.IndexOf(item);
            BasicParam.TimeSeries.TimeSeriesItemCol.Insert(index, copy);
            Sort();
        });

        public DelegateCommand<object> DeleteCommand => new((object o) =>
        {
            TimeSeriesItem item = o as TimeSeriesItem;
            BasicParam.TimeSeries.TimeSeriesItemCol.Remove(item);
            Sort();
        });

        public TimeSeriesViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {

        }
    }
}
