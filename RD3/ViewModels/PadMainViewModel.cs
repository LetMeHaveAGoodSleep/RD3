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

            string content = string.Format("批次结束，批次ID为{0}", CurrentDeviceParameter.BatchID);
            RD3SQLHelper.EndBatch(CurrentDeviceParameter.BatchID, DateTime.Now);
            RD3SQLHelper.AddAuditRecord(CurrentDeviceParameter?.Name, CurrentDeviceParameter.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);

            CurrentDeviceParameter.InExperimenting = false;
            CurrentDeviceParameter.BatchID = -1;
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
            };
            worker.RunWorkerAsync();

            //保存泵和MFC的信息
            var backWorker = new BackgroundWorker();
            backWorker.DoWork += (s, e) =>
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
            backWorker.RunWorkerAsync();

            //温控
            var worker1 = new BackgroundWorker();
            worker1.DoWork += (s, e) =>
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
            };
            worker4.RunWorkerAsync();


            var back = new BackgroundWorker();
            back.DoWork += (s, e) =>
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
            };
            back.RunWorkerAsync();
        }
    }
}
