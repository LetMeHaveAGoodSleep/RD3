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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace RD3.ViewModels
{
    public class IndexViewModel : NavigationViewModel
    {
        public PlotModel PlotModel { get; set; }

        private readonly IDialogHostService dialog;
        private readonly IRegionManager regionManager;

        public DelegateCommand<string> ExecuteCommand { get; private set; }
        public DelegateCommand<TaskBar> NavigateCommand { get; private set; }

        public IndexViewModel(IContainerProvider provider,
            IDialogHostService dialog) : base(provider)
        {
            Title = $"你好，{AppSession.CurrentUser.UserName} {DateTime.Now.GetDateTimeFormats('D')[1].ToString()}";
            CreateTaskBars();
            ExecuteCommand = new DelegateCommand<string>(Execute);
            this.regionManager = provider.Resolve<IRegionManager>();
            this.dialog = dialog;
            NavigateCommand = new DelegateCommand<TaskBar>(Navigate);

            PlotModel = new PlotModel { Title = "Multiple Y-Axes Example" };

            // 添加第一个Y轴  
            var yAxis1 = new LinearAxis { Position = AxisPosition.Left, Title = "Y Axis 1" };
            PlotModel.Axes.Add(yAxis1);

            // 添加第二个Y轴  
            var yAxis2 = new LinearAxis { Position = AxisPosition.Right, Title = "Y Axis 2" };
            PlotModel.Axes.Add(yAxis2);

            var yAxis3 = new LinearAxis { Position = AxisPosition.Left, Title = "Y Axis 3" };
            PlotModel.Axes.Add(yAxis3);
            var series1 = new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)");
            series1.YAxisKey = yAxis1.Key;
            var series2 = new FunctionSeries(Math.Sin, -10, 10, 0.1, "sin(x)");
            series2.YAxisKey = yAxis2.Key;
            var series3 = new FunctionSeries(Math.Sin, -10, 10, 0.1, "sin(x)");
            series3.YAxisKey = yAxis3.Key;
            PlotModel.Series.Add(series1);
            PlotModel.Series.Add(series2);
            PlotModel.Series.Add(series3);
        }

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

        private SummaryDto summary;

        /// <summary>
        /// 首页统计
        /// </summary>
        public SummaryDto Summary
        {
            get { return summary; }
            set { summary = value; RaisePropertyChanged(); }
        }

        #endregion

        private void Execute(string obj)
        {
            switch (obj)
            {
                //case "新增待办": AddToDo(null); break;
                //case "新增备忘录": AddMemo(null); break;
            }
        }

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

        void Refresh()
        {
            TaskBars[0].Content = summary.Sum.ToString();
            TaskBars[1].Content = summary.CompletedCount.ToString();
            TaskBars[2].Content = summary.CompletedRatio;
            TaskBars[3].Content = summary.MemoeCount.ToString();
        }
    }
}
