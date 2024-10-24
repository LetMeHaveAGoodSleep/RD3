﻿using RD3.Common;
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
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Windows;
using System.IO;
using RD3.Common.Events;
using System.Net.Http.Json;
using DryIoc;
using System.Windows.Threading;
using System.Xml.Linq;
using ImTools;
using ScottPlot;

namespace RD3.ViewModels
{
    public class IndexViewModel : NavigationViewModel
    {
        List<DeviceExperimentHistoryData> DeviceExperimentHistoryDatas = [];

        private DispatcherTimer experimentTimer;

        private TimeSpan elapsedTime;

        private string _formattedTime="00:00:00";

        public string FormattedTime
        {
            get => _formattedTime;
            set
            {
                SetProperty(ref _formattedTime, value);
            }
        }

        private bool _running;
        public bool Running
        {
            get { return _running; }
            set { SetProperty(ref _running, value); }
        }

        private bool _isPause;
        public bool IsPause
        {
            get { return _isPause; }
            set { SetProperty(ref _isPause, value); }
        }

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

        public Batch CurrentBatch { get; private set; }

        private DeviceParameter _currentDeviceParameter;
        public DeviceParameter CurrentDeviceParameter
        {
            get { return _currentDeviceParameter; }
            set { SetProperty(ref _currentDeviceParameter, value); }
        }

        private ObservableCollection<DeviceParameter> _deviceParameterCol = [];
        public ObservableCollection<DeviceParameter> DeviceParameterCol
        { 
            get { return _deviceParameterCol; } 
            set { SetProperty(ref _deviceParameterCol, value); } 
        }

        private readonly IDialogHostService dialog;

        public DelegateCommand SaveAsTemplateCommand => new(() => 
        {
            try
            {
                ProjectTemplate template = new ProjectTemplate();
                template.Temp = CurrentDeviceParameter.TempParam.Temp_PV;
                template.PH = CurrentDeviceParameter.PHParam.PH_PV;
                template.DO = CurrentDeviceParameter.DOParam.DO_PV;
                template.Agit = CurrentDeviceParameter.AgitParam.Agit_PV;
                template.Air = CurrentDeviceParameter.AirParam.Air_PV;
                template.CO2 = CurrentDeviceParameter.CO2Param.CO2_PV;
                template.O2 = CurrentDeviceParameter.O2Param.O2_PV;
                template.N2 = CurrentDeviceParameter.N2Param.N2_PV;
                template.Feed = CurrentDeviceParameter.FeedParam.Feed_PV;
                template.Base = CurrentDeviceParameter.BaseParam.Base_PV;
                template.Acid = CurrentDeviceParameter.AcidParam.Acid_PV;
                template.AF = CurrentDeviceParameter.AFParam.AF_PV;
                template.CreatDate = DateTime.Now;
                template.Creator = AppSession.CurrentUser.UserName;
                template.Name = "Template_" + Guid.NewGuid().ToString().Replace("-", "");
                template.UsageTime = 0;

                ProjectTemplateManager.GetInstance().AddTemplate(template);
                ProjectTemplateManager.GetInstance().Save();

                aggregator.SendMessage(Language.GetValue("模板保存成功").ToString(), nameof(IndexViewModel));
            }
            catch (FormatException ex)
            {
                // 处理解析失败的情况
                MessageBox.Show("解析浮点数失败: " + ex.Message);
            }
        });

        public DelegateCommand ImportTemplateCommand => new(() =>
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Temp Files (*.template)|*.template"
            };
            if (openFileDialog.ShowDialog() != true) return;
            try 
            {
                string jsonContent = AESEncryption.DecryptFile(openFileDialog.FileName);
                ProjectTemplate template = JsonConvert.DeserializeObject<ProjectTemplate>(jsonContent);
                CurrentDeviceParameter.TempParam.Temp_PV = template.Temp;
                CurrentDeviceParameter.PHParam.PH_PV = template.PH;
                CurrentDeviceParameter.DOParam.DO_PV = template.DO;
                CurrentDeviceParameter.AgitParam.Agit_PV = template.Agit;
                CurrentDeviceParameter.AirParam.Air_PV = template.Air;
                CurrentDeviceParameter.CO2Param.CO2_PV = template.CO2;
                CurrentDeviceParameter.O2Param.O2_PV = template.O2;
                CurrentDeviceParameter.N2Param.N2_PV = template.N2;
                CurrentDeviceParameter.FeedParam.Feed_PV = template.Feed;
                CurrentDeviceParameter.BaseParam.Base_PV = template.Base;
                CurrentDeviceParameter.AcidParam.Acid_PV = template.Acid;
                CurrentDeviceParameter.AFParam.AF_PV = template.AF;

                //TODO：下发指令，重新控制
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
        });

        public DelegateCommand<string> ExecuteCommand { get; private set; }

        public DelegateCommand EditParamCommand => new(() =>
        {
            DialogParameters pairs = new DialogParameters
            {
                { "DeviceParam", CurrentDeviceParameter }
            };
            dialog?.ShowDialog(nameof(EditExperimentParamView),pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }

                CurrentDeviceParameter = callback.Parameters.GetValue<DeviceParameter>("DeviceParam");
            });
        });

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

        public DelegateCommand StartCommand => new(() =>
        {
            Running = true;
            IsPause = false;
            foreach (var item in DeviceParameterCol)
            {
                item.WorkStatus = WorkStatus.Running;
            }
            InstrumentSolution.GetInstance().CommandWrapper.SetTemp(CurrentDeviceParameter.TempParam);
            InstrumentSolution.GetInstance().CommandWrapper.SetDO(CurrentDeviceParameter.DOParam);

            elapsedTime = TimeSpan.Zero;
            experimentTimer.Start();

            //Test Data
            int numberOfHours = 24;
            DateTime startDateTime = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            double[] dateDoubles = new double[numberOfHours];
            double startDouble = startDateTime.ToOADate(); // days since 1900
            double deltaDouble = 1.0 / 24.0; // an hour is 1/24 of a day
            for (int i = 0; i < numberOfHours; i++)
            {
                dateDoubles[i] = startDouble + i * deltaDouble;
            }
            foreach (var item in DeviceExperimentHistoryDatas)
            {
                foreach (var item2 in item.ExperimentHistoryDatas) 
                {
                    item2.Data.Add(dateDoubles, Generate.Sin(numberOfHours));
                }
            }

            aggregator.SendMessage("", "UpdatePlot", DeviceExperimentHistoryDatas);
        });

        public DelegateCommand PauseCommand => new(() => 
        {
            IsPause = true;
        });

        public DelegateCommand StopCommand => new(() =>
        {
            Running = false;//标志位重置
            IsPause = false;


            foreach (var item in DeviceParameterCol)
            {
                item.WorkStatus = WorkStatus.Idle;
            }

            experimentTimer.Stop();
        });

        public IndexViewModel(IContainerProvider provider,
            IDialogHostService dialog) : base(provider)
        {

            experimentTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            experimentTimer.Tick += ExperimentTimer_Tick;

            this.dialog = dialog;

            ObservableCollection<DeviceParameter> temp = [];
            for (int i = 1; i < 17; i++)
            {
                DeviceParameter device = new()
                {
                    Name = "G" + i.ToString().PadLeft(2, '0'),
                    SerialNumber = i,
                    Temp = 37 + i,
                    PH = 7.2f,
                    DO = 98,
                    Agit = 1000,
                    Base = 149.6f,
                    Acid = 30.0f,
                    AF = 103.3f,
                    Feed = 0.00f
                };
                device.TempParam.Temp_PV = 37.1f;
                device.PHParam.PH_PV = 7.2f;
                device.DOParam.DO_PV = 100f;
                device.AgitParam.Agit_PV = 2000f;
                temp.Add(device);


                DeviceExperimentHistoryData experimentHistoryData = new()
                {
                    DeviceName = "G" + i.ToString().PadLeft(2, '0')
                };
                DeviceExperimentHistoryDatas.Add(experimentHistoryData);
            }
            CurrentBatch = BatchManager.GetInstance().Batches.First();
            DeviceParameterCol = new ObservableCollection<DeviceParameter>(temp);
            CurrentDeviceParameter = DeviceParameterCol[0];

            aggregator.ResgiterMessage((MessageModel model) =>
            {
                ProjectTemplate template = model.Model as ProjectTemplate;
                CurrentDeviceParameter.TempParam.Temp_PV = template.Temp;
                CurrentDeviceParameter.PHParam.PH_PV = template.PH;
                CurrentDeviceParameter.DOParam.DO_PV = template.DO;
                CurrentDeviceParameter.AgitParam.Agit_PV = template.Agit;
                CurrentDeviceParameter.AirParam.Air_PV = template.Air;
                CurrentDeviceParameter.CO2Param.CO2_PV = template.CO2;
                CurrentDeviceParameter.O2Param.O2_PV = template.O2;
                CurrentDeviceParameter.N2Param.N2_PV = template.N2;
                CurrentDeviceParameter.FeedParam.Feed_PV = template.Feed;
                CurrentDeviceParameter.BaseParam.Base_PV = template.Base;
                CurrentDeviceParameter.AcidParam.Acid_PV = template.Acid;
                CurrentDeviceParameter.AFParam.AF_PV = template.AF;

                //TODO:下发指令
            }, nameof(ProjectTemplate));
        }

        private void ExperimentTimer_Tick(object sender, EventArgs e)
        {
            elapsedTime += TimeSpan.FromSeconds(1);
            FormattedTime = elapsedTime.ToString(@"hh\:mm\:ss");
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
