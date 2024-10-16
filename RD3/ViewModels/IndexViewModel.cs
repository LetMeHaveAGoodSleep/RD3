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
using RD3.Views;

namespace RD3.ViewModels
{
    public class IndexViewModel : NavigationViewModel
    {
        private double _progress1 = 45;
        public double Progress1
        {
            get { return _progress1; }
            set { SetProperty(ref _progress1, value); }
        }

        private double _progress2 = 50;
        public double Progress2
        {
            get { return _progress2; }
            set { SetProperty(ref _progress2, value); }
        }

        private double _progress3 = 60;
        public double Progress3
        {
            get { return _progress3; }
            set { SetProperty(ref _progress3, value); }
        }

        private double _progress4 = 70;
        public double Progress4
        {
            get { return _progress4; }
            set { SetProperty(ref _progress4, value); }
        }
        private DeviceParameter _currentDeviceParameter;
      public DeviceParameter CurrentDeviceParameter
        {
            get { return _currentDeviceParameter; }
            set { SetProperty(ref _currentDeviceParameter, value); }
        }


        private ObservableCollection<DeviceParameter> _deviceParameterCol = [];
        public ObservableCollection<DeviceParameter> DeviceParameterCol { get { return _deviceParameterCol; } set { SetProperty(ref _deviceParameterCol, value); } }

        private readonly IDialogHostService dialog;
        private readonly IRegionManager regionManager;

        public DelegateCommand SaveAsTemplateCommand => new(() => 
        {
            // 创建一个 Random 对象
            Random random = new Random();
            // 生成一个介于 0 和 100 之间的随机数
            int randomNumber = random.Next(0, 101);
            Progress1 = randomNumber; 
        });

        public DelegateCommand<string> ExecuteCommand { get; private set; }
        public DelegateCommand<TaskBar> NavigateCommand { get; private set; }

        public DelegateCommand EditParamCommand => new(() =>
        dialog?.ShowDialog(nameof(EditExperimentParamView), callback =>
        {
            if (callback.Result != ButtonResult.OK)
            {
                return;
            }
        }));

        public DelegateCommand<string> UnScheduleCommand => new((string parameter) =>
        {
            Enum.TryParse(parameter, true, out UnScheduleAction parameter1);
            DialogParameters pairs = new DialogParameters()
            {
                { "ActionType", parameter1 }
            };
            dialog?.ShowDialog(nameof(UnscheduledView), pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                aggregator.SendMessage("", nameof(UnscheduledViewModel), callback.Parameters);
            });
        });

        public DelegateCommand<object> ChangeDeviceCommand => new((object o) => 
        {
            int.TryParse(o.ToString(), out int index);
            if (index < 0 || index > DeviceParameterCol.Count - 1) return;
            CurrentDeviceParameter = DeviceParameterCol[index]; 
        });

        public IndexViewModel(IContainerProvider provider,
            IDialogHostService dialog) : base(provider)
        {
            Title = $"你好，{AppSession.CurrentUser.UserName} {DateTime.Now.GetDateTimeFormats('D')[1]}";
            CreateTaskBars();
            this.regionManager = provider.Resolve<IRegionManager>();
            this.dialog = dialog;
            NavigateCommand = new DelegateCommand<TaskBar>(Navigate);

            ObservableCollection<DeviceParameter> temp = new ObservableCollection<DeviceParameter>();
            for (int i = 1; i < 17; i++)
            {
                DeviceParameter device = new DeviceParameter()
                {
                    Name = "G" + i.ToString().PadLeft(2, '0'),
                    Temp = 37 + i,
                    PH = 7.2f,
                    DO = 98,
                    Agit = 1000,
                    Base = 149.6f,
                    Acid = 30.0f,
                    AF = 103.3f,
                    Feed = 0.00f
                };
                device.TempParam.Temp_PV = "37.1";
                device.PHParam.PH_PV = "7.2";
                device.DOParam.DO_PV = "213-232";
                device.AgitParam.Agit_PV = "2000";
                temp.Add(device);
            }
            DeviceParameterCol = new ObservableCollection<DeviceParameter>(temp);
            CurrentDeviceParameter = DeviceParameterCol[0];
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
