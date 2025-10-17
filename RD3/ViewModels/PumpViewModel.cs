using ImTools;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Controller;
using RD3.Shared;
using RD3.Views;
using SixLabors.ImageSharp.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RD3.ViewModels
{
    public class PumpViewModel : BaseViewModel
    {
        private PumpController _controller = new();
        public PumpController Controller
        {
            get => _controller;
            set { SetProperty(ref _controller, value); }
        }

        private PumpInfo _pumpInfo = new();
        public PumpInfo PumpInfo
        {
            get => _pumpInfo;
            set 
            { 
                SetProperty(ref _pumpInfo, value);
                Controller.PumpInfo = value;
            }
        }

        public DeviceParameter CurrentDeviceParameter
        {
            get { return AnalysisSolution.GetInstance().ReactorCol[0]; }
        }


        public DelegateCommand PumpSettingCommand => new(() =>
        {
            DialogParameters keyValuePairs = new DialogParameters()
            {
                {nameof(PumpInfo),PumpInfo }
            };
            DialogHostService.ShowOnce(nameof(PumpSettingView), keyValuePairs, callback =>
            {
                if (callback.Result != ButtonResult.OK) return;
            });
        });

        public DelegateCommand PumpClearCommand => new(() =>
        {
            if (HandyControl.Controls.MessageBox.Show($"确定清除泵{PumpInfo.PumpIndex}的累计量?", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            CommandWrapper.SetResetFlowCapacity(PumpInfo.DeviceID, ClearModule.Pump, PumpInfo.PumpIndex);
        });

        public PumpViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                while (true)
                {
                    if (PumpInfo.PumpIndex != -1)
                    {
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
                        PumpInfo.FlowRate = PumpInfo.FlowCapacity = PumpInfo.Weight = 0;

                        var type = realTimeParam.GetType();
                        var properties = type.GetProperties();
                        foreach (var item in properties.Where(t => t.CanRead && t.CanWrite))
                        {
                            if (item.Name == $"Pump{PumpInfo.PumpIndex}FlowRate")
                            {
                                object value = item.GetValue(realTimeParam);
                                PumpInfo.FlowRate = Convert.ToSingle(value);
                            }
                            else if (item.Name == $"Pump{PumpInfo.PumpIndex}FlowCapacity")
                            {
                                object value = item.GetValue(realTimeParam);
                                PumpInfo.FlowCapacity = Convert.ToSingle(value);
                            }
                            else if (item.Name == $"JarWeight")
                            {
                                if (PumpInfo.IsWeigh && PumpInfo.WeighIndex == WeightIndex.Weight1)
                                {
                                    object value = item.GetValue(realTimeParam);
                                    PumpInfo.Weight = Convert.ToSingle(value);
                                }
                            }
                            else if (item.Name == $"ReserveWeight")
                            {
                                if (PumpInfo.IsWeigh && PumpInfo.WeighIndex == WeightIndex.Weight2)
                                {
                                    object value = item.GetValue(realTimeParam);
                                    PumpInfo.Weight = Convert.ToSingle(value);
                                }
                            }
                            else if (item.Name == $"Bottle1Weight")
                            {
                                if (PumpInfo.IsWeigh && PumpInfo.WeighIndex == WeightIndex.Weight3)
                                {
                                    object value = item.GetValue(realTimeParam);
                                    PumpInfo.Weight = Convert.ToSingle(value);
                                }
                            }
                            else if (item.Name == $"Bottle2Weight")
                            {
                                if (PumpInfo.IsWeigh && PumpInfo.WeighIndex == WeightIndex.Weight4)
                                {
                                    object value = item.GetValue(realTimeParam);
                                    PumpInfo.Weight = Convert.ToSingle(value);
                                }
                            }
                        }
                    }
                    Thread.Sleep(1000);
                }
            };
            worker.RunWorkerAsync();

            BackgroundWorker backgroundWorker= new BackgroundWorker();
            backgroundWorker.DoWork += (s, e) => 
            {
                PumpInfo info = null;
                while (true)
                {
                    try
                    {
                        if (CurrentDeviceParameter.ReactorStatus == ReactorStatus.DisConnected)
                        {
                            continue;
                        }

                        if (info != null)
                        {
                            if (!info.IsControling && PumpInfo.IsControling)
                            {
                                PumpInfo.RunningTime = 0;
                            }
                            else if (PumpInfo.IsControling)
                            {
                                if (PumpInfo.RunningTime <= PumpInfo.RunningTime_SP * 60f - 1 || PumpInfo.IsConstSpeed)
                                {
                                    PumpInfo.RunningTime += 1;
                                }
                            }
                        }
                        info = PumpInfo.Clone() as PumpInfo;

                        if (Controller.PumpInfo != null)
                        {
                            if (Controller.PumpInfo.IsControling && !Controller.PumpInfo.LastIsControling)
                            {
                                Controller.StartWork();
                            }
                            else if (Controller.PumpInfo.LastIsControling && !Controller.PumpInfo.IsControling)
                            {
                                Controller.StopWork();
                            }
                            Controller.PumpInfo.IsControling = Controller.PumpInfo.IsControling;
                        }

                    }
                    catch (Exception ex) { }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
            };
            backgroundWorker.RunWorkerAsync();
        }
    }
}
