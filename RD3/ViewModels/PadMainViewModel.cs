using Fpi.Communication.Manager;
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
using System.Drawing;
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
        private List<DeviceExperimentHistoryData> _graphDataSourceList = new List<DeviceExperimentHistoryData>();

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

        public DelegateCommand StartExperimentCommand => new(() =>
        {
            if (CurrentDeviceParameter.InExperimenting)
            {
                HandyControl.Controls.MessageBox.Show("实验正在进行中...", "温馨提示");
                return;
            }

            Tuple<string, string, string> tuple = Tuple.Create(string.Empty, string.Empty, string.Empty);
            List<string> list = new List<string>() { CurrentDeviceParameter.Name };
            DialogParameters dialogParameters = new DialogParameters()
            {
                { "DeviceName",string.Join("|",list)}
            };
            DialogHostService?.ShowOnce(nameof(AddBatchInfoView), dialogParameters, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }

                tuple = callback.Parameters.GetValue<Tuple<string, string, string>>(nameof(Batch));
                DateTime dt = DateTime.Now;
                RD3Batch rD3Batch = new RD3Batch()
                {
                    createTime = dt.ToString("yyyy-MM-dd HH:mm:ss"),
                    devieceID = CurrentDeviceParameter.Name,
                    statue = (int)RD3BatchStatue.Running,
                    startDateTime = dt.ToString("yyyy-MM-dd HH:mm:ss"),
                    createUser = AppSession.CurrentUser.UserName,
                    Strain = tuple.Item1,
                    Tester = tuple.Item2,
                    description = tuple.Item3
                };
                rD3Batch.ID = RD3SQLHelper.InsertBatch(rD3Batch);
                CurrentDeviceParameter.InExperimenting = true;
                CurrentDeviceParameter.BatchID = rD3Batch.ID;
                Task.Run(() =>
                {
                    string content = string.Format("批次开始，批次ID为{0}", rD3Batch.ID);
                    RD3SQLHelper.AddAuditRecord(CurrentDeviceParameter?.Name, CurrentDeviceParameter?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                });
            });

        });

        public DelegateCommand EndExperimentCommand => new(() =>
        {
            if (!CurrentDeviceParameter.InExperimenting)
            {
                HandyControl.Controls.MessageBox.Show("当前未进行实验...", "温馨提示");
                return;
            }

            if (HandyControl.Controls.MessageBox.Show("确定要结束当前批次吗？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            string content = string.Format("批次结束，批次ID为{0}", CurrentDeviceParameter.BatchID);
            RD3SQLHelper.EndBatch(CurrentDeviceParameter.BatchID, DateTime.Now);
            RD3SQLHelper.AddAuditRecord(CurrentDeviceParameter?.Name, CurrentDeviceParameter.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);

            CurrentDeviceParameter.InExperimenting = false;
            CurrentDeviceParameter.BatchID = -1;
        });

        public DelegateCommand<string> DODIYCommand => new((string content) =>
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox($"请输入溶氧自定义值:", "修改溶氧", "");
            if (float.TryParse(input, out float value))
            {
                AppSession.VirtualDO = value;
            }
        });

        public DelegateCommand<string> pHDIYCommand => new((string content) =>
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox($"请输入pH自定义值:", "修改pH", "");
            if (float.TryParse(input, out float value))
            {
                AppSession.VirtualpH = value;
            }
        });

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


        public DelegateCommand MinimizeCommand => new(() =>
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
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

            DeviceExperimentHistoryData data = new DeviceExperimentHistoryData()
            {
                DeviceName = CurrentDeviceParameter.Name
            };
            foreach (var item1 in GraphConfig.GetAllValue().Keys.ToList())
            {
                ExperimentHistoryData historyData = new ExperimentHistoryData()
                {
                    ParamerterName = item1
                };
                data.ExperimentHistoryDatas.Add(historyData);
            }
            _graphDataSourceList.Add(data);


            Thread thread = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    DeviceParameter lastDeviceParameter = null;
                    while (true)
                    {
                        try
                        {
                            if (lastDeviceParameter != null)
                            {
                                if (!lastDeviceParameter.InExperimenting && CurrentDeviceParameter.InExperimenting)
                                {
                                    CurrentDeviceParameter.ExperimentTime = 0;
                                }
                                else if (CurrentDeviceParameter.InExperimenting)
                                {
                                    CurrentDeviceParameter.ExperimentTime += 1;
                                }
                            }
                            lastDeviceParameter = CurrentDeviceParameter.Clone() as DeviceParameter;
                        }
                        catch (Exception ex) { }
                        finally
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
            }));
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Highest;
            thread.Start();

            //连接状态判断
            Thread thread1 = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    try
                    {
                        Pipe pipe = PortManager.GetInstance().FindSendPipe(CurrentDeviceParameter.Name);
                        if (pipe == null)
                        {
                            CurrentDeviceParameter.ReactorStatus = ReactorStatus.DisConnected;
                            Thread.Sleep(1000);
                            continue;
                        }
                        if (InstrumentSolution.GetInstance().IsSimulation)
                        {
                            CurrentDeviceParameter.ReactorStatus = ReactorStatus.Simulated;
                        }
                        else
                        {
                            CurrentDeviceParameter.ReactorStatus = pipe.Connected == true ? ReactorStatus.Connected : ReactorStatus.DisConnected;
                            if (CurrentDeviceParameter.ReactorStatus == ReactorStatus.DisConnected)
                            {
                                HandyControl.Controls.MessageBox.Show("连接异常", "温馨提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                                continue;
                            }
                        }
                    }
                    catch (Exception ex) { }
                    finally
                    {
                        Thread.Sleep(1000);
                    }

                }
            }));
            thread1.IsBackground = true;
            thread1.Priority = ThreadPriority.Lowest;
            thread1.Start();


            //读取实时信息
            Thread thread2 = new Thread(new ThreadStart(() =>
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


                        DeviceExperimentHistoryData dataSource = _graphDataSourceList.Find(t => t.DeviceName == CurrentDeviceParameter.Name);
                        foreach (var item2 in dataSource.ExperimentHistoryDatas)
                        {
                            if (item2.Xs.Count == 0)
                            {
                                item2.Xs.Add([]);
                            }
                            else
                            {
                                item2.Xs[0] = [];
                            }
                            if (item2.Ys.Count == 0)
                            {
                                item2.Ys.Add([]);
                            }
                            else
                            {
                                item2.Ys[0] = [];
                            }

                            var list = ClockSupervisor.realDatasDic[deviceID];
                            if (list.Count < 1) return;
                            var realTime = list[0];
                            double oaDate = realTime.SampleTime.ToOADate();//缓存第一个点时间
                            int cacheCount = ClockSupervisor.realData_time[deviceID].Count;
                            for (int i = 0; i < cacheCount; i++)
                            {
                                double time = ClockSupervisor.realData_time[deviceID][i];
                                if (time > oaDate)
                                {
                                    break;
                                }
                                item2.Xs[0].Add(time);

                                string propertyName = PameterMapperConfig.GetValue(item2.ParamerterName)?.ToString();
                                var realTimeParam1 = ClockSupervisor.realDatasDic_tenSecond[deviceID][i];
                                double value = realTimeParam1.GetPropertyValue<double>(propertyName);
                                item2.Ys[0].Add(value);
                            }

                            for (int j = 0; j < list.Count; j++)
                            {
                                item2.Xs[0].Add(list[j].SampleTime.ToOADate());
                                string propertyName = PameterMapperConfig.GetValue(item2.ParamerterName)?.ToString();
                                double value = list[j].GetPropertyValue<double>(propertyName);
                                item2.Ys[0].Add(value);
                            }
                        }
                        dataSource = _graphDataSourceList.Find(t => t.DeviceName == CurrentDeviceParameter.Name);
                        AnalysisSolution.GetInstance().EventPublisher.PublishRealTimeData(dataSource);
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }

                }
            }));
            thread2.IsBackground = true;
            thread2.Priority = ThreadPriority.Highest;
            thread2.Start();


            //保存泵和MFC的信息
            Thread thread3 = new Thread(new ThreadStart(() =>
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
            }));
            thread3.IsBackground = true;
            thread3.Priority = ThreadPriority.Lowest;
            thread3.Start();

            //温控
            Thread thread4 = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    try
                    {
                        if (CurrentDeviceParameter.ReactorStatus == ReactorStatus.DisConnected)
                        {
                            continue;
                        }

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
            }));
            thread4.IsBackground = true;
            thread4.Priority = ThreadPriority.Lowest;
            thread4.Start();


            //转速
            Thread thread5 = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    try
                    {
                        if (CurrentDeviceParameter.ReactorStatus == ReactorStatus.DisConnected)
                        {
                            continue;
                        }

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
            }));
            thread5.IsBackground = true;
            thread5.Priority = ThreadPriority.Lowest;
            thread5.Start();

            //溶氧
            Thread thread6 = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    try
                    {
                        if (CurrentDeviceParameter.ReactorStatus == ReactorStatus.DisConnected)
                        {
                            continue;
                        }

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
            }));
            thread6.IsBackground = true;
            thread6.Priority = ThreadPriority.Lowest;
            thread6.Start();

            //pH
            Thread thread7 = new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    try
                    {
                        if (CurrentDeviceParameter.ReactorStatus == ReactorStatus.DisConnected)
                        {
                            continue;
                        }

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
            }));
            thread7.IsBackground = true;
            thread7.Priority = ThreadPriority.Lowest;
            thread7.Start();

            //实时信息保存
            Thread thread8 = new Thread(new ThreadStart(() =>
            {
                List<object[]> list = [];
                Dictionary<string, DateTime> dictionary = new Dictionary<string, DateTime>();
                Dictionary<string, int> keyValuePairs = new Dictionary<string, int>();
                while (true)
                {
                    try
                    {
                        PropertyInfo[] propertyInfos = typeof(RealTimeParam).GetProperties().Where(c => c.CanWrite && c.CanRead && (c.PropertyType == typeof(double) || c.PropertyType == typeof(float) || c.PropertyType == typeof(int) || c.PropertyType == typeof(string))).ToArray();

                        int index = ClockSupervisor.realDatasDic[CurrentDeviceParameter.Name].Count - 1;
                        if (index < 0)
                        {
                            Thread.Sleep(1000);
                            continue;
                        } 
                        var realTimeParam = ClockSupervisor.realDatasDic[CurrentDeviceParameter.Name][index];
                        //方成 开机以后保存罐体初始重量
                        if (!AppSession.DicTankWeight.ContainsKey(CurrentDeviceParameter.Name))
                        {
                            AppSession.DicTankWeight[CurrentDeviceParameter.Name] = Tuple.Create(realTimeParam.JarWeight, realTimeParam.Pump1FlowCapacity, realTimeParam.Pump2FlowCapacity, realTimeParam.Pump3FlowCapacity, realTimeParam.Pump4FlowCapacity, realTimeParam.Pump5FlowCapacity, realTimeParam.Pump6FlowCapacity);
                        }

                        if (!dictionary.ContainsKey(CurrentDeviceParameter.Name))
                        {
                            var realTimeParams = ClockSupervisor.realDatasDic[CurrentDeviceParameter.Name];
                            for (int i = 0; i < realTimeParams.Count; i++)
                            {
                                var realTime = realTimeParams[i];
                                dictionary[CurrentDeviceParameter.Name] = realTime.SampleTime;
                                if ((string.IsNullOrWhiteSpace(realTime.ReactorName) || InstrumentSolution.GetInstance().IsSimulation) && CurrentDeviceParameter.BatchID < 1)
                                {
                                    continue;
                                }
                                if (!keyValuePairs.ContainsKey(CurrentDeviceParameter.Name))
                                {
                                    keyValuePairs[CurrentDeviceParameter.Name] = 1;
                                }
                                else
                                {
                                    keyValuePairs[CurrentDeviceParameter.Name] += 1;
                                }

                                List<object> values = new List<object>();
                                values.Add(CurrentDeviceParameter.Name);
                                values.Add(CurrentDeviceParameter.BatchID.ToString());
                                values.Add(realTime.SampleTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                foreach (var p in propertyInfos)
                                {
                                    object o = p.GetValue(realTime);
                                    values.Add(o);
                                }
                                list.Add(values.ToArray());
                            }
                        }
                        else
                        {
                            var realTimeParams = ClockSupervisor.realDatasDic[CurrentDeviceParameter.Name].FindAll(t => t.SampleTime > dictionary[CurrentDeviceParameter.Name]);
                            for (int i = 0; i < realTimeParams.Count; i++)
                            {
                                var realTime = realTimeParams[i];
                                dictionary[CurrentDeviceParameter.Name] = realTime.SampleTime;
                                if ((string.IsNullOrWhiteSpace(realTime.ReactorName) || InstrumentSolution.GetInstance().IsSimulation) && CurrentDeviceParameter.BatchID < 1)
                                {
                                    continue;
                                }
                                if (!keyValuePairs.ContainsKey(CurrentDeviceParameter.Name))
                                {
                                    keyValuePairs[CurrentDeviceParameter.Name] = 1;
                                }
                                else
                                {
                                    keyValuePairs[CurrentDeviceParameter.Name] += 1;
                                }
                                List<object> values = new List<object>();
                                values.Add(CurrentDeviceParameter.Name);
                                values.Add(CurrentDeviceParameter.BatchID.ToString());
                                values.Add(realTime.SampleTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                foreach (var p in propertyInfos)
                                {
                                    object o = p.GetValue(realTime);
                                    values.Add(o);
                                }
                                list.Add(values.ToArray());
                            }
                        }

                        if (keyValuePairs.Keys.Count < 1) continue;
                        var maxValue = keyValuePairs.OrderByDescending(x => x.Value).First().Value;
                        if (maxValue > 0 && maxValue >= 1)
                        {
                            RD3SQLHelper.BulkInsertRealTimeParam(list);

                            for (int i = 0; i < list.Count; i++)
                            {
                                list[i] = null;
                            }

                            list.Clear();
                            list = null;

                            keyValuePairs.Clear();
                            keyValuePairs = null;

                            list = [];
                            keyValuePairs = new Dictionary<string, int>();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("保存实时信息出错" + ex.Message + "\r\n" + ex.StackTrace);
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                    if (DateTime.Now.Second % 15 == 0)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
            }));
            thread8.IsBackground = true;
            thread8.Priority = ThreadPriority.Highest;
            thread8.Start();

            //审计追踪
            Thread thread9 = new Thread(new ThreadStart(() =>
            {
                var deviceParameter = CurrentDeviceParameter;
                var lastDevice = deviceParameter.Clone() as DeviceParameter;

                while (true)
                {
                    try
                    {
                        if (deviceParameter.BatchID != -1)
                        {
                            lastDevice = deviceParameter.Clone() as DeviceParameter;
                            Thread.Sleep(1000);
                            continue;
                        }

                        var device = CurrentDeviceParameter;

                        #region 溶氧 
                        if (deviceParameter.DOParam.ControlStrategy != lastDevice.DOParam.ControlStrategy)
                        {
                            Task.Run(() =>
                            {
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("DO:控制策略从{0}变更为", EnumUtil.GetEnumDescription(lastDevice.DOParam.ControlStrategy), EnumUtil.GetEnumDescription(deviceParameter.DOParam.ControlStrategy));
                                sb.AppendLine(content);
                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (deviceParameter.DOParam.DO_PV != lastDevice.DOParam.DO_PV)
                        {
                            Task.Run(() =>
                            {
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("DO预设值从{0}变更为{1}", lastDevice.DOParam.DO_PV, deviceParameter.DOParam.DO_PV);
                                sb.AppendLine(content);
                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (!deviceParameter.DOParam.IsControling && lastDevice.DOParam.IsControling)
                        {
                            Task.Run(() =>
                            {
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("DO控制模式从{0}变更为{1}", "自动", "手动");
                                sb.AppendLine(content);
                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (deviceParameter.DOParam.IsControling && !lastDevice.DOParam.IsControling)
                        {
                            Task.Run(() =>
                            {
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("DO控制模式从{0}变更为{1}", "手动", "自动");
                                sb.AppendLine(content);
                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (!deviceParameter.DOParam.IsControling)
                        {
                            if (deviceParameter.AgitParam.Agit_PV != lastDevice.AgitParam.Agit_PV)
                            {
                                Task.Run(() =>
                                {
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("转速预设值从{0}变更为{1}", lastDevice.AgitParam.Agit_PV, deviceParameter.AgitParam.Agit_PV);
                                    sb.AppendLine(content);
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (deviceParameter.AirParam.FlowSpeed != lastDevice.AirParam.FlowSpeed)
                            {
                                Task.Run(() =>
                                {
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("通气预设值从{0}变更为{1}", lastDevice.AirParam.FlowSpeed, deviceParameter.AirParam.FlowSpeed);
                                    sb.AppendLine(content);
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (deviceParameter.O2Param.FlowSpeed != lastDevice.O2Param.FlowSpeed)
                            {
                                Task.Run(() =>
                                {
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("氧气预设值从{0}变更为{1}", lastDevice.O2Param.FlowSpeed, deviceParameter.O2Param.FlowSpeed);
                                    sb.AppendLine(content);
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (!deviceParameter.AgitParam.IsControling && lastDevice.AgitParam.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("转速控制模式从{0}变更为{1}", "自动", "手动");
                                    sb.AppendLine(content);
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (deviceParameter.AgitParam.IsControling && !lastDevice.AgitParam.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("转速控制模式从{0}变更为{1}", "手动", "自动");
                                    sb.AppendLine(content);
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (!deviceParameter.AirParam.IsControling && lastDevice.AirParam.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("通气控制模式从{0}变更为{1}", "自动", "手动");
                                    sb.AppendLine(content);
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (deviceParameter.AirParam.IsControling && !lastDevice.AirParam.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("通气控制模式从{0}变更为{1}", "手动", "自动");
                                    sb.AppendLine(content);
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (deviceParameter.O2Param.IsControling && !lastDevice.O2Param.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("氧气控制模式从{0}变更为{1}", "手动", "自动");
                                    sb.AppendLine(content);
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (!deviceParameter.O2Param.IsControling && lastDevice.O2Param.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("氧气控制模式从{0}变更为{1}", "自动", "手动");
                                    sb.AppendLine(content);
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }
                        }
                        #endregion

                        #region PH

                        if (deviceParameter.PHParam.PH_PV != lastDevice.PHParam.PH_PV)
                        {
                            Task.Run(() =>
                            {
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("PH预设值从{0}变更为{1}", lastDevice.PHParam.PH_PV, deviceParameter.PHParam.PH_PV);
                                sb.AppendLine(content);
                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (!deviceParameter.PHParam.IsControling && lastDevice.PHParam.IsControling)
                        {
                            Task.Run(() =>
                            {
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("PH控制模式从{0}变更为{1}", "自动", "手动");
                                sb.AppendLine(content);
                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (deviceParameter.PHParam.IsControling && !lastDevice.PHParam.IsControling)
                        {
                            Task.Run(() =>
                            {
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("PH控制模式从{0}变更为{1}", "手动", "自动");
                                sb.AppendLine(content);
                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (!deviceParameter.PHParam.IsControling)
                        {
                            if (deviceParameter.AcidParam.Acid_PV != lastDevice.AcidParam.Acid_PV)
                            {
                                Task.Run(() =>
                                {
                                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                    string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("酸泵预设值从{0}变更为{1}", lastDevice.AcidParam.Acid_PV, deviceParameter.AcidParam.Acid_PV);
                                    sb.AppendLine(content);
    
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (deviceParameter.BaseParam.Base_PV != lastDevice.BaseParam.Base_PV)
                            {
                                Task.Run(() =>
                                {
                                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                    string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("碱泵预设值从{0}变更为{1}", lastDevice.BaseParam.Base_PV, deviceParameter.BaseParam.Base_PV);
                                    sb.AppendLine(content);
    
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (!deviceParameter.AcidParam.IsControling && lastDevice.AcidParam.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                    string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("酸泵控制模式从{0}变更为{1}", "自动", "手动");
                                    sb.AppendLine(content);
    
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (deviceParameter.AcidParam.IsControling && !lastDevice.AcidParam.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                    string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("酸泵控制模式从{0}变更为{1}", "手动", "自动");
                                    sb.AppendLine(content);
    
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (!deviceParameter.BaseParam.IsControling && lastDevice.BaseParam.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                    string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("碱泵控制模式从{0}变更为{1}", "手动", "自动");
                                    sb.AppendLine(content);
    
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (deviceParameter.BaseParam.IsControling && !lastDevice.BaseParam.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                    string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("碱泵控制模式从{0}变更为{1}", "手动", "自动");
                                    sb.AppendLine(content);
    
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }
                        }
                        #endregion

                        #region 温度
                        if (deviceParameter.TempParam.Temp_PV != lastDevice.TempParam.Temp_PV)
                        {
                            Task.Run(() =>
                            {
                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("温度预设值从{0}变更为{1}", lastDevice.TempParam.Temp_PV, deviceParameter.TempParam.Temp_PV);
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (!deviceParameter.TempParam.IsControling && lastDevice.TempParam.IsControling)
                        {
                            Task.Run(() =>
                            {
                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("温度控制模式从{0}变更为{1}", "打开", "关闭");
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (deviceParameter.TempParam.IsControling && !lastDevice.TempParam.IsControling)
                        {
                            Task.Run(() =>
                            {
                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("温度控制模式从{0}变更为{1}", "关闭", "打开");
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }
                        #endregion

                        #region 补料

                        //只有常数时候才记录预设值变更
                        if ((deviceParameter.FeedParam1.Feed_PV != lastDevice.FeedParam1.Feed_PV) && (deviceParameter.FeedParam1.FeedIndex == lastDevice.FeedParam1.Feed_PV && deviceParameter.FeedParam1.FeedIndex == 1))
                        {
                            Task.Run(() =>
                            {
                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("补料预设值从{0}变更为{1}", lastDevice.FeedParam1.Feed_PV, deviceParameter.FeedParam1.Feed_PV);
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (!deviceParameter.FeedParam1.IsControling && lastDevice.FeedParam1.IsControling)
                        {
                            Task.Run(() =>
                            {
                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("补料控制模式从{0}变更为{1}", "自动", "手动");
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (deviceParameter.FeedParam1.IsControling && !lastDevice.FeedParam1.IsControling)
                        {
                            Task.Run(() =>
                            {
                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("补料控制模式从{0}变更为{1}", "手动", "自动");
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (deviceParameter.FeedParam1.FeedIndex != lastDevice.FeedParam1.FeedIndex)
                        {
                            Task.Run(() =>
                            {
                                string newName = string.Empty;
                                string oldName = string.Empty;

                                #region 杂乱代码
                                if (deviceParameter.FeedParam1.FeedIndex == 0)
                                {
                                    newName = "常量";
                                }
                                else if (deviceParameter.FeedParam1.FeedIndex == 1)
                                {
                                    newName = "多项式";
                                }
                                else if (deviceParameter.FeedParam1.FeedIndex == 2)
                                {
                                    newName = "指数";
                                }
                                else if (deviceParameter.FeedParam1.FeedIndex == 3)
                                {
                                    newName = "时间序列";
                                }
                                else if (lastDevice.FeedParam1.FeedIndex == 4)
                                {
                                    newName = "DO_stat(流速)";
                                }
                                else if (lastDevice.FeedParam1.FeedIndex == 5)
                                {
                                    newName = "pH_stat(流速)";
                                }
                                else if (lastDevice.FeedParam1.FeedIndex == 6)
                                {
                                    newName = "DO_stat(总量)";
                                }
                                else if (lastDevice.FeedParam1.FeedIndex == 7)
                                {
                                    newName = "pH_stat(总量)";
                                }

                                if (lastDevice.FeedParam1.FeedIndex == 0)
                                {
                                    oldName = "常量";
                                }
                                else if (lastDevice.FeedParam1.FeedIndex == 1)
                                {
                                    oldName = "多项式";
                                }
                                else if (lastDevice.FeedParam1.FeedIndex == 2)
                                {
                                    oldName = "指数";
                                }
                                else if (lastDevice.FeedParam1.FeedIndex == 3)
                                {
                                    oldName = "时间序列";
                                }
                                else if (lastDevice.FeedParam1.FeedIndex == 4)
                                {
                                    oldName = "DO_stat(流速)";
                                }
                                else if (lastDevice.FeedParam1.FeedIndex == 5)
                                {
                                    oldName = "pH_stat(流速)";
                                }
                                else if (lastDevice.FeedParam1.FeedIndex == 6)
                                {
                                    oldName = "DO_stat(总量)";
                                }
                                else if (lastDevice.FeedParam1.FeedIndex == 7)
                                {
                                    oldName = "pH_stat(总量)";
                                }
                                #endregion

                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("补料模式从{0}变更为{1}", oldName, newName);
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }
                        #endregion

                        #region 消泡
                        if (deviceParameter.AFParam.AF_PV != lastDevice.AFParam.AF_PV)
                        {
                            Task.Run(() =>
                            {
                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("消泡预设值从{0}变更为{1}", lastDevice.AFParam.AF_PV, deviceParameter.AFParam.AF_PV);
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (!deviceParameter.AFParam.IsControling && lastDevice.AFParam.IsControling)
                        {
                            Task.Run(() =>
                            {
                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("消泡模式从{0}变更为{1}", "打开", "关闭");
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (deviceParameter.AFParam.IsControling && !lastDevice.AFParam.IsControling)
                        {
                            Task.Run(() =>
                            {
                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("消泡模式从{0}变更为{1}", "关闭", "打开");
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (deviceParameter.AFParam.AutoDefoaming && !lastDevice.AFParam.AutoDefoaming)
                        {
                            Task.Run(() =>
                            {
                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("自动消泡模式从{0}变更为{1}", "关闭", "打开");
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (!deviceParameter.AFParam.AutoDefoaming && lastDevice.AFParam.AutoDefoaming)
                        {
                            Task.Run(() =>
                            {
                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("自动消泡模式从{0}变更为{1}", "打开", "关闭");
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (deviceParameter.AFParam.Cycle != lastDevice.AFParam.Cycle)
                        {
                            Task.Run(() =>
                            {
                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("消泡周期从{0}变更为{1}", lastDevice.AFParam.Cycle, deviceParameter.AFParam.Cycle);
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        if (deviceParameter.AFParam.DutyCycle != lastDevice.AFParam.DutyCycle)
                        {
                            Task.Run(() =>
                            {
                                string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                string content = string.Format("消泡占空比从{0}变更为{1}", lastDevice.AFParam.DutyCycle, deviceParameter.AFParam.DutyCycle);
                                sb.AppendLine(content);

                                RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                            });
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug(string.Format("反应器{0}日志记录异常，异常信息：{1}", deviceParameter.Name, ex.Message));
                    }

                    lastDevice = deviceParameter.Clone() as DeviceParameter;
                    Thread.Sleep(1000);
                }
            }));
            thread9.IsBackground = true;
            thread9.Priority = ThreadPriority.Highest;
            thread9.Start();
        }
    }
}
