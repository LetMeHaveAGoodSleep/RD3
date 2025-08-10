using ImTools;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Controller;
using RD3.Shared;
using RD3.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using XZ.SQLite;

namespace RD3.ViewModels
{
    public class PadMainViewModel : BaseViewModel, IConfigureService
    {
        private int _selectedMenuIndex = 0;
        public int SelectedMenuIndex
        {
            get => _selectedMenuIndex;
            set 
            {
                SetProperty(ref _selectedMenuIndex, value);
            }
        }

        private DeviceParameter _currentDeviceParameter;
        public DeviceParameter CurrentDeviceParameter
        {
            get { return _currentDeviceParameter; }
            set { SetProperty(ref _currentDeviceParameter, value); }
        }


        public DelegateCommand AgitSettingCommand => new(() =>
        {
            DialogHostService.ShowOnce(nameof(AgitSettingView), callback => { });
        });

        public DelegateCommand DOSettingCommand => new(() =>
        {
            DialogHostService.ShowOnce(nameof(DOSettingView), callback =>
            {
                if (callback.Result != ButtonResult.OK) return;
            });
        });

        public DelegateCommand pHSettingCommand => new(() =>
        {
            DialogHostService.ShowOnce(nameof(pHSettingView), callback => { });
        });

        public DelegateCommand TempSettingCommand => new(() =>
        {
            DialogHostService.ShowOnce(nameof(TempSettingView), callback => { });
        });

        public DelegateCommand ExitCommand => new(() =>
        {
            if (AppSession.CurrentUser.Type != Shared.UserType.Admin)
            {
                HandyControl.Controls.Growl.WarningGlobal("非管理员不可关闭软件！");
            }
            else
            {
                if (HandyControl.Controls.MessageBox.Show("确定退出本系统？", "温馨提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                    Environment.Exit(0);
                    return;
                }

            }

        });

        public PadMainViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {

        }

        public void Configure()
        {
            if (AnalysisSolution.GetInstance().ReactorCol.Count < 1)
            {
                AnalysisSolution.GetInstance().ReactorCol.Add(new DeviceParameter() { Name = "G01" });
            }
            CurrentDeviceParameter = AnalysisSolution.GetInstance().ReactorCol[0];

            //读取实时信息&实时保存泵和MFC的信息
            var worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                while (true)
                {
                    try
                    {
                        AnalysisSolution.GetInstance().SaveAllSetting();
                        AnalysisSolution.GetInstance().SavePumpMFCToOld(CurrentDeviceParameter.Name);

                        if (CurrentDeviceParameter == null)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        string deviceID = "G01";
                        foreach (var item in ClockSupervisor.realDatasDic.Keys)
                        {
                            deviceID = item;
                            break;
                        }
                        if (!ClockSupervisor.realDatasDic.ContainsKey(deviceID) || ClockSupervisor.realDatasDic[deviceID].Count < 1)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        int index = ClockSupervisor.realDatasDic[deviceID].Count - 1;
                        var realTimeParam = ClockSupervisor.realDatasDic[deviceID][index];
                        PropertyMapper.Map(realTimeParam, CurrentDeviceParameter);
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }

                }
            };
            worker.RunWorkerAsync();

            var back = new BackgroundWorker();
            back.DoWork += (s, e) =>
            {
                Thread.Sleep(300000);
                while (true)
                {
                    try
                    {
                        AnalysisSolution.GetInstance().SaveAllSetting();
                        AnalysisSolution.GetInstance().SavePumpMFCToOld(CurrentDeviceParameter.Name);
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        Thread.Sleep(50000);
                    }

                }
            };
            back.RunWorkerAsync();

            //温控
            var worker1 = new BackgroundWorker();
            worker1.DoWork += (s, e) =>
            {
                while (true)
                {
                    try
                    {
                        if (!CurrentDeviceParameter.TempParam.LastIsControling && CurrentDeviceParameter.TempParam.IsControling)
                        {
                            AnalysisSolution.GetInstance().TempController.StartWork();
                        }
                        else if (!CurrentDeviceParameter.TempParam.IsControling && CurrentDeviceParameter.TempParam.LastIsControling)
                        {
                            AnalysisSolution.GetInstance().TempController.StopWork();
                        }
                        CurrentDeviceParameter.TempParam.IsControling = CurrentDeviceParameter.TempParam.IsControling;
                    }
                    catch (Exception ex) { }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
            };
            worker1.RunWorkerAsync();

            //转速
            var worker2 = new BackgroundWorker();
            worker2.DoWork += (s, e) =>
            {
                while (true)
                {
                    try
                    {
                        if (!CurrentDeviceParameter.AgitParam.LastIsControling && CurrentDeviceParameter.AgitParam.IsControling)
                        {
                            AnalysisSolution.GetInstance().AgitController.StartWork();
                        }
                        else if (!CurrentDeviceParameter.AgitParam.IsControling && CurrentDeviceParameter.AgitParam.LastIsControling)
                        {
                            AnalysisSolution.GetInstance().AgitController.StopWork();
                        }
                        CurrentDeviceParameter.AgitParam.IsControling = CurrentDeviceParameter.AgitParam.IsControling;
                    }
                    catch (Exception ex) { }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
            };
            worker2.RunWorkerAsync();

            //溶氧
            var worker3 = new BackgroundWorker();
            worker3.DoWork += (s, e) =>
            {
                while (true)
                {
                    try
                    {
                        if (!CurrentDeviceParameter.DOParam.LastIsControling && CurrentDeviceParameter.DOParam.IsControling)
                        {
                            AnalysisSolution.GetInstance().DOController.StartWork();
                        }
                        else if (!CurrentDeviceParameter.DOParam.IsControling && CurrentDeviceParameter.DOParam.LastIsControling)
                        {
                            AnalysisSolution.GetInstance().DOController.StopWork();
                        }
                        CurrentDeviceParameter.DOParam.IsControling = CurrentDeviceParameter.DOParam.IsControling;
                    }
                    catch (Exception ex) { }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
            };
            worker3.RunWorkerAsync();

            //pH
            var worker4 = new BackgroundWorker();
            worker4.DoWork += (s, e) =>
            {
                while (true)
                {
                    try
                    {
                        if (!CurrentDeviceParameter.PHParam.LastIsControling && CurrentDeviceParameter.PHParam.IsControling)
                        {
                            AnalysisSolution.GetInstance().pHController.StartWork();
                        }
                        else if (!CurrentDeviceParameter.PHParam.IsControling && CurrentDeviceParameter.PHParam.LastIsControling)
                        {
                            AnalysisSolution.GetInstance().pHController.StopWork();
                        }
                        CurrentDeviceParameter.PHParam.IsControling = CurrentDeviceParameter.PHParam.IsControling;
                    }
                    catch (Exception ex) { }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
            };
            worker4.RunWorkerAsync();
        }
    }
}
