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
    public class TimeSeriesViewModel : BaseViewModel, INavigationAware
    {
        private ExperimentParameter _experimentParameter = ExperimentParameter.DO;
        private string _runningInfo = string.Empty;
        public string RunningInfo
        {
            get => _runningInfo;
            set
            {
                SetProperty(ref _runningInfo, value);
            }
        }

        private TimeSeriesParameter _timeSeriesParameter;

        private BackgroundWorker backgroundWorker = new BackgroundWorker();

        private TimeSeries _timeSeries = new TimeSeries();
        public TimeSeries TimeSeries
        {
            get => _timeSeries;
            set
            {
                SetProperty(ref _timeSeries, value);
            }
        }

        public DelegateCommand SaveCommand => new(() => 
        {
            TimeSeries.TimeSeriesItemCol = new ObservableCollection<TimeSeriesItem>(TimeSeries.TimeSeriesItemCol.OrderBy(t => new { t.StartTime, t.EndTime }));
        });
        public DelegateCommand RefershCommand => new(() =>
        {
            TimeSeries.TimeSeriesItemCol = new ObservableCollection<TimeSeriesItem>(TimeSeries.TimeSeriesItemCol.OrderBy(t => new { t.StartTime, t.EndTime }));
            aggregator.SendMessage("", nameof(TimeSeriesView), TimeSeries.TimeSeriesItemCol);
        });

        public DelegateCommand CopyCommand => new(() => 
        {
            TimeSeries.TimeSeriesItemCol = new ObservableCollection<TimeSeriesItem>(TimeSeries.TimeSeriesItemCol.OrderBy(t => new { t.StartTime, t.EndTime }));
            if (TimeSeries.TimeSeriesItemCol.GroupBy(x => new { x.StartTime, x.EndTime }).Any(g => g.Count() > 1))
            {
                MessageBox.Show("存在相同的时间项", "温馨提示");
                return ;
            }
            AnalysisSolution.GetInstance().EventPublisher.PublishTimeSeries((_experimentParameter, TimeSeries));
        });

        public DelegateCommand AddCommand => new(() =>
        {
            RefershCommand.Execute();
            double startTime = 0;
            double endTime = 1;
            double value = 1;
            if (TimeSeries.TimeSeriesItemCol.Count > 0)
            {
                startTime = TimeSeries.TimeSeriesItemCol.Last().EndTime;
                endTime = startTime + 30;
                value = TimeSeries.TimeSeriesItemCol[TimeSeries.TimeSeriesItemCol.Count - 1].Value;
            }
            TimeSeries.TimeSeriesItemCol.Add(new TimeSeriesItem() { StartTime = startTime, EndTime = endTime, Value = value });
        });

        public DelegateCommand<object> InsertCommand => new((object o) =>
        {
            TimeSeriesItem item = o as TimeSeriesItem;
            double startTime = 0;
            double endTime = 1;
            double value = 1;
            if (TimeSeries.TimeSeriesItemCol.Count > 0)
            {
                startTime = TimeSeries.TimeSeriesItemCol.Last().EndTime;
                endTime = startTime + 30;
                value = TimeSeries.TimeSeriesItemCol[TimeSeries.TimeSeriesItemCol.Count - 1].Value;
            }
            TimeSeries.TimeSeriesItemCol.Add(new TimeSeriesItem() { StartTime = startTime, EndTime = endTime, Value = value });
        });

        public DelegateCommand<object> DeleteCommand => new((object o) =>
        {
            TimeSeriesItem item = o as TimeSeriesItem;
            TimeSeries.TimeSeriesItemCol.Remove(item);
            RefershCommand.Execute();
        });
        public DelegateCommand DeleteAllCommand => new(() =>
        {
            TimeSeries.TimeSeriesItemCol.Clear();
        });

        public TimeSeriesViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }
    }
}
