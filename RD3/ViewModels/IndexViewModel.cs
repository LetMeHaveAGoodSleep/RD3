using RD3.Common;
using RD3.Common.Models;
using RD3.Extensions;
using RD3.Shared.Dtos;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Windows.Media;
using RD3.Shared;

namespace RD3.ViewModels
{
    public class IndexViewModel : NavigationViewModel
    {
        private ObservableCollection<DeviceParameter> _devices = new ObservableCollection<DeviceParameter>();
        public ObservableCollection<DeviceParameter> Devices { get { return _devices; } set { SetProperty(ref _devices, value); } }

        //private AxesCollection _yAxisCollection;
        //public AxesCollection YAxisCollection { get => _yAxisCollection; }

        //private SeriesCollection _seriesCollection;
        //public SeriesCollection Series { get => _seriesCollection; }

        private readonly IDialogHostService dialog;
        private readonly IRegionManager regionManager;

        public DelegateCommand<string> ExecuteCommand { get; private set; }
        public DelegateCommand<TaskBar> NavigateCommand { get; private set; }

        public IndexViewModel(IContainerProvider provider,
            IDialogHostService dialog) : base(provider)
        {
            Title = $"你好，{AppSession.CurrentUser.UserName} {DateTime.Now.GetDateTimeFormats('D')[1].ToString()}";
            CreateTaskBars();
            this.regionManager = provider.Resolve<IRegionManager>();
            this.dialog = dialog;
            NavigateCommand = new DelegateCommand<TaskBar>(Navigate);

            //_yAxisCollection = new AxesCollection
            //{
            //    new Axis { Title = "Y-Axis 1", MinValue = 0, Position = AxisPosition.LeftBottom, Foreground = Brushes.Red },
            //    new Axis { Title = "Y-Axis 2", MinValue = 0, Position = AxisPosition.RightTop, Foreground = Brushes.Green },
            //    new Axis { Title = "Y-Axis 3", MinValue = 0,  Position = AxisPosition.RightTop, Foreground = Brushes.Blue }
            //};

            //GenerateTestSeries();

            ObservableCollection<DeviceParameter> temp = new ObservableCollection<DeviceParameter>();
            for (int i = 1; i < 9; i++)
            {
                DeviceParameter device = new DeviceParameter()
                {
                    Name = i.ToString().PadLeft(2, '0'),
                    Status = WorkStatus.Running,
                    Temp = 37,
                    PH = 7.2f,
                    DO = 98,
                    Agit = 1000,
                    Base = 149.6f,
                    Acid = 30.0f,
                    AF = 103.3f,
                    Feed = 0
                };
                temp.Add(device);
            }
            Devices = new ObservableCollection<DeviceParameter>(temp);
        }

        //private void GenerateTestSeries()
        //{
        //    LiveCharts.SeriesCollection testSeries = new LiveCharts.SeriesCollection();


        //    var dataPoints = ConvertPointListString("0,3.108 0.814,3.345 1.628,3.342 2.442,3.221 3.256,3.087 4.071,3.004 4.885,2.988 5.699,3.014 6.513,3.03 7.327,2.98 8.141,2.814 8.955,2.514 9.769,2.088 10.584,1.573 11.398,1.004 12.212,0.372");
        //    var ls1 = new LineSeries
        //    {
        //        Name = "Test_Series_1",
        //        Configuration = new CartesianMapper<DataPoint>()
        //                .X(dp => dp.X)
        //                .Y(dp => dp.Y),
        //        Title = "Test Series 1",
        //        Values = dataPoints,
        //        PointGeometry = null
        //    };

        //    var dataPoints2 = ConvertPointListString("0,1.359 0.814,1.584 1.628,2.177 2.442,2.737 3.256,3.164 4.071,3.518 4.885,3.858 5.699,4.216 6.513,4.574 7.327,4.863 8.141,5.01 8.955,4.969 9.769,4.721 10.584,4.308 11.398,3.784 12.212,3.15");
        //    var ls2 = new LineSeries
        //    {
        //        Name = "Test_Series_2",
        //        Configuration = new CartesianMapper<DataPoint>()
        //                .X(dp => dp.X)
        //                .Y(dp => dp.Y),
        //        Title = "Test Series 2",
        //        Values = dataPoints2,
        //        PointGeometry = null
        //    };

        //    var dataPoints3 = ConvertPointListString("0,0 0.814,27.099 1.628,39.408 2.442,45.311 3.256,50.089 4.071,54.802 4.885,59.645 5.699,64.222 6.513,68.022 7.327,70.766 8.141,72.09 8.955,71.418 9.769,68.126 10.584,60.939 11.398,47.694 12.212,22.742");
        //    var ls3 = new LineSeries
        //    {
        //        Name = "Test_Series_3",
        //        Configuration = new CartesianMapper<DataPoint>()
        //                .X(dp => dp.X)
        //                .Y(dp => dp.Y),
        //        Title = "Test Series 3",
        //        Values = dataPoints3,
        //        PointGeometry = null
        //    };

        //    var dataPoints4 = ConvertPointListString("0,0 0.84,0.027 1.68,0.109 2.52,0.245 3.36,0.435 4.2,0.68 5.04,0.979 5.88,1.333 6.72,1.741 7.56,2.203 8.4,2.72");
        //    var ls4 = new LineSeries
        //    {
        //        Name = "Test_Series_4",
        //        Configuration = new CartesianMapper<DataPoint>()
        //                .X(dp => dp.X)
        //                .Y(dp => dp.Y),
        //        Title = "Test Series 4",
        //        Values = dataPoints4,
        //        PointGeometry = null
        //    };

        //    var dataPoints5 = ConvertPointListString("0,2.72 8.4,2.72");
        //    var ls5 = new LineSeries
        //    {
        //        Name = "Test_Series_5",
        //        Configuration = new CartesianMapper<DataPoint>()
        //                .X(dp => dp.X)
        //                .Y(dp => dp.Y),
        //        Title = "Test Series 5",
        //        Values = dataPoints5,
        //        PointGeometry = null
        //    };

        //    var dataPoints6 = ConvertPointListString("8.4,0 8.4,2.72");
        //    var ls6 = new LineSeries
        //    {
        //        Name = "Test_Series_6",
        //        Configuration = new CartesianMapper<DataPoint>()
        //                .X(dp => dp.X)
        //                .Y(dp => dp.Y),
        //        Title = "Test Series 6",
        //        Values = dataPoints6,
        //        PointGeometry = null
        //    };

        //    testSeries.Add(ls1);
        //    testSeries.Add(ls2);
        //    testSeries.Add(ls3);
        //    testSeries.Add(ls4);
        //    testSeries.Add(ls5);
        //    testSeries.Add(ls6);

        //    _seriesCollection = testSeries;
        //}

        //private ChartValues<DataPoint> ConvertPointListString(string pointList, string delimiter = " ", string delimter2 = ",")
        //{
        //    ChartValues<DataPoint> ldp = new ChartValues<DataPoint>();

        //    char[] pointDelim = delimiter.ToCharArray();
        //    string[] points = pointList.Split(pointDelim);

        //    foreach (var point in points)
        //    {
        //        char[] coordDelim = delimter2.ToCharArray();
        //        string[] coord = point.Split(coordDelim);
        //        ldp.Add(new DataPoint(double.Parse(coord[0]), double.Parse(coord[1])));
        //    }
        //    return ldp;
        //}

        private void Navigate(TaskBar obj)
        {
            if (string.IsNullOrWhiteSpace(obj.Target)) return;

            NavigationParameters param = new NavigationParameters();

            if (obj.Title == "已完成")
            {
                param.Add("Value", 2);
            }
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(obj.Target, param);
        }

        #region 属性

        private string title;

        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<TaskBar> taskBars;

        public ObservableCollection<TaskBar> TaskBars
        {
            get { return taskBars; }
            set { taskBars = value; RaisePropertyChanged(); }
        }

        #endregion

        void CreateTaskBars()
        {
            TaskBars = new ObservableCollection<TaskBar>();
            TaskBars.Add(new TaskBar() { Icon = "ClockFast", Title = "汇总", Color = "#FF0CA0FF", Target = "ToDoView" });
            TaskBars.Add(new TaskBar() { Icon = "ClockCheckOutline", Title = "已完成", Color = "#FF1ECA3A", Target = "ToDoView" });
            TaskBars.Add(new TaskBar() { Icon = "ChartLineVariant", Title = "完成比例", Color = "#FF02C6DC", Target = "" });
            TaskBars.Add(new TaskBar() { Icon = "PlaylistStar", Title = "备忘录", Color = "#FFFFA000", Target = "MemoView" });
        }

        public override async void OnNavigatedTo(NavigationContext navigationContext)
        {
            //var summaryResult = await toDoService.SummaryAsync();
            //if (summaryResult.Status)
            //{
            //    Summary = summaryResult.Result;
            //    Refresh();
            //}
            base.OnNavigatedTo(navigationContext);
        }
    }
}
