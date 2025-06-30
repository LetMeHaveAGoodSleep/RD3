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
using System.Reflection.Metadata;
using MathNet.Symbolics;
using Fpi.Communication.Manager;
using System.Threading;
using Fpi.Instruments;
using System.Collections;
using Newtonsoft.Json.Linq;
using ScottPlot.Colormaps;
using RD3.Shared.Util;
using CustomApp;
using System.Drawing;
using XZ.OnnxRun;
using System.Timers;
using System.ComponentModel;
using Fpi.Communication.Protocols;
using SixLabors.ImageSharp.Drawing;
using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;
using ScottPlot.Hatches;
using System.Windows.Controls;
using MathNet.Numerics.Distributions;
using System.Text.RegularExpressions;
using ScottPlot.Plottables;
using System.Runtime.CompilerServices;
using XZ.SQLite;
using System.Windows.Documents;
using static System.Runtime.InteropServices.JavaScript.JSType;
using OpenTK.Graphics.OpenGL;
using Fpi.Util.WinApiUtil.CommDataType;
using System.Dynamic;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using System.Reflection;
using static Microsoft.FSharp.Core.ByRefKinds;
using System.Security.Cryptography;
using Fpi.Communication.Commands.Config;

namespace RD3.ViewModels
{
    [RegionMemberLifetime(KeepAlive = true)]
    public class ReactorViewModel : BaseViewModel, IDialogAware
    {
        private ObservableCollection<DOTimeSeries> _doTs = [];
        public ObservableCollection<DOTimeSeries> DOTS { get { return _doTs; } set { SetProperty(ref _doTs, value); } }

        bool isWindowOpen = false;

        int pointCount = 7200;//缓存2小时

        private Device _selectedDevice;
        public Device SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                SetProperty(ref _selectedDevice, value);
            }
        }

        private List<DeviceExperimentHistoryData> GraphDataSourceList = new List<DeviceExperimentHistoryData>();

        private BackgroundWorker changeDevieWorker = new BackgroundWorker();

        private BackgroundWorker reconnectWorker = new BackgroundWorker();

        #region 溶氧相关
        private Dictionary<string, BackgroundWorker> dicDOTimeWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, BackgroundWorker> dicDOWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, BackgroundWorker> dicDOTempWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, int> dicDODelta = new Dictionary<string, int>();
        private Dictionary<string, int> dicAirIndex = new Dictionary<string, int>();
        private Dictionary<string, int> dicO2Index = new Dictionary<string, int>();
        private Dictionary<string, QPIDController> dicDOPid = new Dictionary<string, QPIDController>();
        private Dictionary<string, QPIDController> dicDOAgitPid = new Dictionary<string, QPIDController>();
        private Dictionary<string, float> dicAgitPid = new Dictionary<string, float>();

        private Dictionary<string, BackgroundWorker> dicAgitWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, float> dicAgitSP = new Dictionary<string, float>();

        private Dictionary<string, BackgroundWorker> dicAirWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, float> dicAirSP = new Dictionary<string, float>();

        private Dictionary<string, BackgroundWorker> dicO2Worker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, float> dicO2SP = new Dictionary<string, float>();
        #endregion

        #region PH相关
        private Dictionary<string, BackgroundWorker> dicPHTimeWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, BackgroundWorker> dicPHWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, float> dicPHDelta = new Dictionary<string, float>();
        private Dictionary<string, QPIDController> dicPHPid = new Dictionary<string, QPIDController>();
        private Dictionary<string, IntelligentPHController> dicPHController = new Dictionary<string, IntelligentPHController>();

        private Dictionary<string, BackgroundWorker> dicAcidWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, float> dicAcidSP = new Dictionary<string, float>();

        private Dictionary<string, BackgroundWorker> dicBaseWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, float> dicBaseSP = new Dictionary<string, float>();
        #endregion

        #region 温控相关
        private Dictionary<string, BackgroundWorker> dicTempTimeWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, BackgroundWorker> dicTempWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, float> dicTempSP = new Dictionary<string, float>();
        private Dictionary<string, BackgroundWorker> dicTempDOWorker = new Dictionary<string, BackgroundWorker>();
        //private BackgroundWorker tempTimeWorker = new BackgroundWorker();
        //private BackgroundWorker tempWorker = new BackgroundWorker();
        #endregion

        #region 消泡相关
        private Dictionary<string, BackgroundWorker> dicAFWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, float> dicAFSP = new Dictionary<string, float>();
        #endregion

        private BackgroundWorker afCycleWorker = new BackgroundWorker();

        #region 补料1相关
        private Dictionary<string, BackgroundWorker> dicFeed1TimeWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, BackgroundWorker> dicFeed1Worker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, float> dicFeed1SP = new Dictionary<string, float>();

        private Dictionary<string, ProbingController> dicFeed1Probe = new Dictionary<string, ProbingController>();
        #endregion


        #region 补料2相关
        private Dictionary<string, BackgroundWorker> dicFeed2TimeWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, BackgroundWorker> dicFeed2Worker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, float> dicFeed2SP = new Dictionary<string, float>();
        private Dictionary<string, ProbingController> dicFeed2Probe = new Dictionary<string, ProbingController>();
        #endregion

        private Dictionary<string, BackgroundWorker> dicAuditWorker = new Dictionary<string, BackgroundWorker>();

        private SwitchMode _condensationEnable = SwitchMode.Close;
        public SwitchMode CondensationEnable
        {
            get => _condensationEnable;
            set { SetProperty(ref _condensationEnable, value); }
        }

        private ObservableCollection<Device> _reactorCol;
        public ObservableCollection<Device> ReactorCol
        {
            get
            {
                _reactorCol = new ObservableCollection<Device>();
                //设备和当前用户关联--hdb
                foreach (Device item in DeviceManager.GetInstance().Devices)
                {
                    if (!AppSession.CurrentUser.UserDevieceIDs.Contains(item.Name))
                    {
                        continue;
                    }
                    _reactorCol.Add(item);
                }
                return _reactorCol;
            }
        }

        private string _formattedTime = "00:00:00";

        public string FormattedTime
        {
            get => _formattedTime;
            set
            {
                SetProperty(ref _formattedTime, value);
            }
        }

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

        public event Action<IDialogResult> RequestClose;


        private List<BackgroundWorker> workers = [];

        public string Title => "设备详情";

        public DelegateCommand ReleaseCommand => new(() =>
        {
            try
            {
                if (MessageBox.Show("确认关闭选择反应器的运行？", "温馨提示", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }

                Task.Run(() =>
                {
                    CurrentDeviceParameter.WorkStatus = WorkStatus.Idle;

                    Type type = this.GetType();

                    foreach (KeyValuePair<string, BackgroundWorker> item in dicDOWorker)
                    {
                        if (item.Value != null && item.Value.IsBusy && item.Key == CurrentDeviceParameter.Name)
                        {
                            item.Value.CancelAsync();
                        }
                    }

                    foreach (KeyValuePair<string, BackgroundWorker> item in dicPHWorker)
                    {
                        if (item.Value != null && item.Value.IsBusy && item.Key == CurrentDeviceParameter.Name)
                        {
                            item.Value.CancelAsync();
                        }
                    }

                    foreach (KeyValuePair<string, BackgroundWorker> item in dicFeed1Worker)
                    {
                        if (item.Value != null && item.Value.IsBusy && item.Key == CurrentDeviceParameter.Name)
                        {
                            item.Value.CancelAsync();
                        }
                    }

                    foreach (KeyValuePair<string, BackgroundWorker> item in dicFeed2Worker)
                    {
                        if (item.Value != null && item.Value.IsBusy && item.Key == CurrentDeviceParameter.Name)
                        {
                            item.Value.CancelAsync();
                        }
                    }

                    foreach (KeyValuePair<string, ProbingController> item in dicFeed1Probe)
                    {
                        if (item.Value != null && item.Key == CurrentDeviceParameter.Name)
                        {
                            item.Value.StopCtrl();
                        }
                    }

                    foreach (KeyValuePair<string, ProbingController> item in dicFeed2Probe)
                    {
                        if (item.Value != null && item.Key == CurrentDeviceParameter.Name)
                        {
                            item.Value.StopCtrl();
                        }
                    }

                    foreach (KeyValuePair<string, BackgroundWorker> item in dicDOTimeWorker)
                    {
                        if (item.Value != null && item.Value.IsBusy && item.Key == CurrentDeviceParameter.Name)
                        {
                            item.Value.CancelAsync();
                        }
                    }

                    foreach (KeyValuePair<string, BackgroundWorker> item in dicPHTimeWorker)
                    {
                        if (item.Value != null && item.Value.IsBusy && item.Key == CurrentDeviceParameter.Name)
                        {
                            item.Value.CancelAsync();
                        }
                    }

                    foreach (KeyValuePair<string, BackgroundWorker> item in dicTempTimeWorker)
                    {
                        if (item.Value != null && item.Value.IsBusy && item.Key == CurrentDeviceParameter.Name)
                        {
                            item.Value.CancelAsync();
                        }
                    }

                    foreach (KeyValuePair<string, BackgroundWorker> item in dicFeed1TimeWorker)
                    {
                        if (item.Value != null && item.Value.IsBusy && item.Key == CurrentDeviceParameter.Name)
                        {
                            item.Value.CancelAsync();
                        }
                    }

                    foreach (KeyValuePair<string, BackgroundWorker> item in dicFeed2TimeWorker)
                    {
                        if (item.Value != null && item.Value.IsBusy && item.Key == CurrentDeviceParameter.Name)
                        {
                            item.Value.CancelAsync();
                        }
                    }


                    #region 溶氧
                    CurrentDeviceParameter.AgitParam.Agit_PV = 0;
                    CurrentDeviceParameter.AirParam.FlowSpeed = 0;
                    CurrentDeviceParameter.O2Param.FlowSpeed = 0;
                    CurrentDeviceParameter.DOParam.IsControling = false;
                    CurrentDeviceParameter.AgitParam.IsControling = false;
                    CurrentDeviceParameter.AirParam.IsControling = false;
                    CurrentDeviceParameter.O2Param.IsControling = false;
                    Task.Run(() =>
                    {
                        CommandWrapper.SetAgitSpeed(CurrentDeviceParameter.Name, CurrentDeviceParameter.AgitParam.Agit_PV);
                        int mfcNo = PumpMFCUtil.GetMFCIndex(CurrentDeviceParameter.Name, GasType.Air);
                        if (mfcNo >= 0)
                        {
                            GasParam air = new GasParam()
                            {
                                MFCNo = mfcNo,
                                FlowSpeed = CurrentDeviceParameter.AirParam.FlowSpeed,
                                GasType = GasType.Air
                            };
                            CommandWrapper.SetGasSpeed(CurrentDeviceParameter.Name, air);

                        }

                        int mfcNo1 = PumpMFCUtil.GetMFCIndex(CurrentDeviceParameter.Name, GasType.O2);
                        if (mfcNo1 >= 0)
                        {
                            GasParam O2 = new GasParam()
                            {
                                MFCNo = mfcNo1,
                                FlowSpeed = CurrentDeviceParameter.AirParam.FlowSpeed,
                                GasType = GasType.O2
                            };
                            CommandWrapper.SetGasSpeed(CurrentDeviceParameter.Name, O2);
                        }
                    });
                    #endregion

                    #region PH
                    CurrentDeviceParameter.PHParam.IsControling = false;
                    CurrentDeviceParameter.BaseParam.Base_PV = 0;
                    CurrentDeviceParameter.BaseParam.IsControling = false;
                    CurrentDeviceParameter.AcidParam.Acid_PV = 0;
                    CurrentDeviceParameter.AcidParam.IsControling = false;
                    Task.Run(() =>
                    {
                        int pumpNo = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, PeristalticPump.BasePump);
                        if (pumpNo >= 0)
                        {
                            PeristalticPumpControlParam param1 = new PeristalticPumpControlParam()
                            {
                                PumpNo = pumpNo,
                                Pump = PeristalticPump.BasePump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = CurrentDeviceParameter.BaseParam.Base_PV,
                                FlowCapacity = 0
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(CurrentDeviceParameter.Name, param1);
                        }

                        int pumpNo1 = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, PeristalticPump.AcidPump);
                        if (pumpNo1 >= 0)
                        {
                            PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                            {
                                PumpNo = pumpNo1,
                                Pump = PeristalticPump.AcidPump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = CurrentDeviceParameter.AcidParam.Acid_PV,
                                FlowCapacity = 0
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(CurrentDeviceParameter.Name, param);
                        }
                    });
                    #endregion

                    #region 补料
                    CurrentDeviceParameter.FeedParam1.IsControling = false;
                    CurrentDeviceParameter.FeedParam1.Feed_PV = 0;
                    Task.Run(() =>
                    {
                        int pumpNo = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, PeristalticPump.FeedPump);
                        if (pumpNo >= 0)
                        {
                            PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                            {
                                PumpNo = pumpNo,
                                Pump = PeristalticPump.FeedPump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = CurrentDeviceParameter.FeedParam1.Feed_PV,
                                FlowCapacity = 0
                            };
                            CommandWrapper.SetPeristalticPumpControlParam(CurrentDeviceParameter.Name, param);
                        }
                    });
                    #endregion

                    #region 消泡
                    CurrentDeviceParameter.AFParam.AutoDefoaming = false;
                    CurrentDeviceParameter.AFParam.IsControling = false;
                    CurrentDeviceParameter.AFParam.AF_PV = 0;
                    Task.Run(() =>
                    {
                        int pumpNo = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, PeristalticPump.AFPump);
                        if (pumpNo >= 0)
                        {
                            PeristalticPumpControlParam param1 = new PeristalticPumpControlParam()
                            {
                                PumpNo = pumpNo,
                                Pump = PeristalticPump.AFPump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = CurrentDeviceParameter.AFParam.AF_PV,
                                FlowCapacity = 0
                            };
                            CommandWrapper.SetPeristalticPumpControlParam(CurrentDeviceParameter.Name, param1);
                        }
                    });
                    #endregion
                    CurrentDeviceParameter.TempParam.IsControling = false;
                });
            }
            catch (Exception ex)
            {

            }

        });

        public DelegateCommand<object> ChangeDeviceCommand => new((object o) =>
        {
            //CurrentDeviceParameter = DeviceParameterCol.FindFirst(t => t.Name == SelectedDevice.Name);

            //LastDevice = CurrentDeviceParameter.Name;
            //string newName = (o as Device).Name.ToString();
            //CurrentDeviceParameter = DeviceParameterCol.FindFirst(t => t.Name == newName);
            //DeviceExperimentHistoryData dataSource = GraphDataSourceList.Find(t => t.DeviceName == CurrentDeviceParameter.Name);
            //aggregator.SendMessage(CurrentDeviceParameter.Name, nameof(ReactorView), dataSource);

            //if (changeDevieWorker != null && changeDevieWorker.IsBusy)
            //{
            //    changeDevieWorker.CancelAsync();
            //    return;
            //}
            //changeDevieWorker = new BackgroundWorker();
            //changeDevieWorker.WorkerReportsProgress = true;      // 允许报告进度
            //changeDevieWorker.WorkerSupportsCancellation = true; // 允许取消操作
            //// 绑定事件
            //changeDevieWorker.DoWork += ((s, e) =>
            //{
            //    try
            //    {
            //        var worker = (BackgroundWorker)s;
            //        if (worker.CancellationPending)
            //        { // 检查取消请求
            //            e.Cancel = true;
            //            return;
            //        }
            //        PeristalticPump[] pumps = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpSetting(newName);
            //        GasType[] mfcs = InstrumentSolution.GetInstance().CommandWrapper.GetMFCSetting(newName);
            //        CurrentDeviceParameter.PumpMFCSetting.MFC1 = mfcs[0];
            //        CurrentDeviceParameter.PumpMFCSetting.MFC2 = mfcs[1];
            //        CurrentDeviceParameter.PumpMFCSetting.Pump1 = pumps[0];
            //        CurrentDeviceParameter.PumpMFCSetting.Pump2 = pumps[1];
            //        CurrentDeviceParameter.PumpMFCSetting.Pump3 = pumps[2];
            //        CurrentDeviceParameter.PumpMFCSetting.Pump4 = pumps[3];
            //    }
            //    catch (Exception ex)
            //    {
            //        LogHelper.Debug(string.Format("切换设备时与下位机通讯异常，从{0}切换到{1}，错误信息：{2}", LastDevice, newName, ex.Message));
            //    }
            //});
            //changeDevieWorker.RunWorkerCompleted += ((s, e) => { changeDevieWorker.Dispose(); changeDevieWorker = null; });
            //changeDevieWorker.RunWorkerAsync();
        });

        public DelegateCommand<string> CondensateCommand => new((string str) =>
        {
            Enum.TryParse(typeof(SwitchMode), str, out var result);
            //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(CurrentDeviceParameter.Name, ControlObject.Condensation, (SwitchMode)result);
            CondensationEnable = (SwitchMode)result;
        });

        public DelegateCommand<DeviceParameter> DOSPCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }
            else
            {
                currentDeviceParameter.DOParam.ControlMode = currentDeviceParameter.DOParam.ControlMode == ControlMode.TimeSeries ? ControlMode.Enable : ControlMode.TimeSeries;
            }

            switch (currentDeviceParameter.DOParam.ControlMode)
            {
                case ControlMode.Enable:
                    if (dicDOTimeWorker[currentDeviceParameter.Name] != null && dicDOTimeWorker[currentDeviceParameter.Name].IsBusy)
                    {
                        dicDOTimeWorker[currentDeviceParameter.Name].CancelAsync();
                        return;
                    }
                    break;
                case ControlMode.TimeSeries:
                    if (dicDOTimeWorker[currentDeviceParameter.Name] != null && dicDOTimeWorker[currentDeviceParameter.Name].IsBusy)
                    {
                        if (dicDOTimeWorker[currentDeviceParameter.Name] != null && dicDOTimeWorker[currentDeviceParameter.Name].IsBusy)
                        {
                            currentDeviceParameter.DOParam.ControlMode = ControlMode.Enable;
                            dicDOTimeWorker[currentDeviceParameter.Name].CancelAsync();
                            return;
                        }
                        return;
                    }
                    if (currentDeviceParameter.DOParam.TimeSeries == null || currentDeviceParameter.DOParam.TimeSeries.TimeSeriesItemCol == null || currentDeviceParameter.DOParam.TimeSeries.TimeSeriesItemCol.Count < 1)
                    {
                        if (dicDOTimeWorker[currentDeviceParameter.Name] != null && dicDOTimeWorker[currentDeviceParameter.Name].IsBusy)
                        {
                            currentDeviceParameter.DOParam.ControlMode = ControlMode.Enable;
                            dicDOTimeWorker[currentDeviceParameter.Name].CancelAsync();
                            return;
                        }
                        return;
                    }

                    dicDOTimeWorker[currentDeviceParameter.Name] = new BackgroundWorker();
                    dicDOTimeWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;      // 允许报告进度
                    dicDOTimeWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true; // 允许取消操作

                    dicDOTimeWorker[currentDeviceParameter.Name].DoWork += ((s, e) =>
                    {
                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        DateTime beginTime = DateTime.Now;//开始时间
                        while (true)
                        {
                            double timeOffset = (DateTime.Now - beginTime).TotalMinutes;//时间差
                            try
                            {
                                var worker = (BackgroundWorker)s;
                                if (worker.CancellationPending)
                                {
                                    e.Result = deviceParameter.Name;
                                    return;
                                }
                                foreach (var item in deviceParameter.DOParam.TimeSeries.TimeSeriesItemCol)
                                {
                                    if (worker.CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }
                                    float time = 0;
                                    switch (deviceParameter.DOParam.TimeSeries.Timer)
                                    {
                                        case FeedTimer.Minute:
                                            time = Convert.ToSingle(item.Time);
                                            break;
                                        case FeedTimer.Hour:
                                            time = Convert.ToSingle(item.Time * 60);
                                            break;
                                        case FeedTimer.Day:
                                            time = Convert.ToSingle(item.Time * 60 * 24);
                                            break;
                                    }
                                    if (timeOffset <= time)
                                    {
                                        deviceParameter.DOParam.DO_PV = (float)item.Value;
                                        break;
                                    }
                                }
                                Thread.Sleep(1000);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Debug(string.Format("PH时间序列调整失败，错误信息：{0}", ex.Message));
                                Thread.Sleep(10000);
                            }
                        }
                    });
                    dicDOTimeWorker[currentDeviceParameter.Name].RunWorkerCompleted += ((s, e) =>
                    {
                        BackgroundWorker backgroundWorker = (BackgroundWorker)s;
                        backgroundWorker.Dispose();
                        backgroundWorker = null;

                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        deviceParameter.DOParam.ControlMode = ControlMode.Enable;
                    });
                    dicDOTimeWorker[currentDeviceParameter.Name].RunWorkerAsync();
                    break;
            }
        });

        public DelegateCommand DoAssociateCommand => new(() =>
        {
            DialogParameters keyValuePairs = new DialogParameters()
            {
                {nameof(DeviceParameter),CurrentDeviceParameter }
            };
            DialogHostService.ShowOnce(nameof(DOControlStrategyView), keyValuePairs, callback =>
            {
                if (callback.Result != ButtonResult.OK) return;
                var assParam = callback.Parameters.GetValue<DOAssParam>(nameof(DOAssParam));
                var midRangingParam = callback.Parameters.GetValue<MidRangingParam>(nameof(MidRangingParam));
            });
        });

        public DelegateCommand<DeviceParameter> PHSPCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }
            else
            {
                currentDeviceParameter.PHParam.ControlMode = currentDeviceParameter.PHParam.ControlMode == ControlMode.TimeSeries ? ControlMode.Enable : ControlMode.TimeSeries;
            }

            switch (currentDeviceParameter.PHParam.ControlMode)
            {
                case ControlMode.Enable:
                    if (dicPHTimeWorker[currentDeviceParameter.Name] != null && dicPHTimeWorker[currentDeviceParameter.Name].IsBusy)
                    {
                        dicPHTimeWorker[currentDeviceParameter.Name].CancelAsync();
                        return;
                    }
                    break;
                case ControlMode.TimeSeries:
                    if (dicPHTimeWorker[currentDeviceParameter.Name] != null && dicPHTimeWorker[currentDeviceParameter.Name].IsBusy)
                    {
                        if (dicPHTimeWorker[currentDeviceParameter.Name] != null && dicPHTimeWorker[currentDeviceParameter.Name].IsBusy)
                        {
                            currentDeviceParameter.PHParam.ControlMode = ControlMode.Enable;
                            dicPHTimeWorker[currentDeviceParameter.Name].CancelAsync();
                            return;
                        }
                        return;
                    }
                    if (currentDeviceParameter.PHParam.TimeSeries == null || currentDeviceParameter.PHParam.TimeSeries.TimeSeriesItemCol == null || currentDeviceParameter.PHParam.TimeSeries.TimeSeriesItemCol.Count < 1)
                    {
                        if (dicPHTimeWorker[currentDeviceParameter.Name] != null && dicPHTimeWorker[currentDeviceParameter.Name].IsBusy)
                        {
                            currentDeviceParameter.PHParam.ControlMode = ControlMode.Enable;
                            dicPHTimeWorker[currentDeviceParameter.Name].CancelAsync();
                            return;
                        }
                        return;
                    }

                    dicPHTimeWorker[currentDeviceParameter.Name] = new BackgroundWorker();
                    dicPHTimeWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;      // 允许报告进度
                    dicPHTimeWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true; // 允许取消操作

                    dicPHTimeWorker[currentDeviceParameter.Name].DoWork += ((s, e) =>
                    {
                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        DateTime beginTime = DateTime.Now;//开始时间
                        while (true)
                        {
                            double timeOffset = (DateTime.Now - beginTime).TotalMinutes;//时间差
                            try
                            {
                                var worker = (BackgroundWorker)s;
                                if (worker.CancellationPending)
                                {
                                    e.Result = deviceParameter.Name;
                                    return;
                                }
                                foreach (var item in deviceParameter.PHParam.TimeSeries.TimeSeriesItemCol)
                                {
                                    if (worker.CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }
                                    float time = 0;
                                    switch (deviceParameter.PHParam.TimeSeries.Timer)
                                    {
                                        case FeedTimer.Minute:
                                            time = Convert.ToSingle(item.Time);
                                            break;
                                        case FeedTimer.Hour:
                                            time = Convert.ToSingle(item.Time * 60);
                                            break;
                                        case FeedTimer.Day:
                                            time = Convert.ToSingle(item.Time * 60 * 24);
                                            break;
                                    }
                                    if (timeOffset <= time)
                                    {
                                        deviceParameter.PHParam.PH_PV = (float)item.Value;
                                        break;
                                    }
                                }
                                Thread.Sleep(1000);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Debug(string.Format("PH时间序列调整失败，错误信息：{0}", ex.Message));
                                Thread.Sleep(10000);
                            }
                        }
                    });
                    dicPHTimeWorker[currentDeviceParameter.Name].RunWorkerCompleted += ((s, e) =>
                    {
                        BackgroundWorker backgroundWorker = (BackgroundWorker)s;
                        backgroundWorker.Dispose();
                        backgroundWorker = null;

                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        deviceParameter.PHParam.ControlMode = ControlMode.Enable;
                    });
                    dicPHTimeWorker[currentDeviceParameter.Name].RunWorkerAsync();
                    break;
            }
        });

        public DelegateCommand<DeviceParameter> TempSPCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }
            else
            {
                currentDeviceParameter.TempParam.ControlMode = currentDeviceParameter.TempParam.ControlMode == ControlMode.TimeSeries ? ControlMode.Enable : ControlMode.TimeSeries;
            }
            switch (currentDeviceParameter.TempParam.ControlMode)
            {
                case ControlMode.Enable:
                    if (dicTempTimeWorker[currentDeviceParameter.Name] != null && dicTempTimeWorker[currentDeviceParameter.Name].IsBusy)
                    {
                        dicTempTimeWorker[currentDeviceParameter.Name].CancelAsync();
                        return;
                    }
                    break;
                case ControlMode.TimeSeries:
                    if (dicTempTimeWorker[currentDeviceParameter.Name] != null && dicTempTimeWorker[currentDeviceParameter.Name].IsBusy)
                    {
                        if (dicTempTimeWorker[currentDeviceParameter.Name] != null && dicTempTimeWorker[currentDeviceParameter.Name].IsBusy)
                        {
                            currentDeviceParameter.TempParam.ControlMode = ControlMode.Enable;
                            dicTempTimeWorker[currentDeviceParameter.Name].CancelAsync();
                            return;
                        }
                        return;
                    }
                    if (currentDeviceParameter.TempParam.TimeSeries == null || currentDeviceParameter.TempParam.TimeSeries.TimeSeriesItemCol == null || currentDeviceParameter.TempParam.TimeSeries.TimeSeriesItemCol.Count < 1)
                    {
                        if (dicTempTimeWorker[currentDeviceParameter.Name] != null && dicTempTimeWorker[currentDeviceParameter.Name].IsBusy)
                        {
                            currentDeviceParameter.TempParam.ControlMode = ControlMode.Enable;
                            dicTempTimeWorker[currentDeviceParameter.Name].CancelAsync();
                            return;
                        }
                        return;
                    }

                    dicTempTimeWorker[currentDeviceParameter.Name] = new BackgroundWorker();
                    dicTempTimeWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;      // 允许报告进度
                    dicTempTimeWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true; // 允许取消操作

                    dicTempTimeWorker[currentDeviceParameter.Name].DoWork += ((s, e) =>
                    {
                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        DateTime beginTime = DateTime.Now;//开始时间
                        while (true)
                        {
                            double timeOffset = (DateTime.Now - beginTime).TotalMinutes;//时间差
                            try
                            {
                                var worker = (BackgroundWorker)s;
                                if (worker.CancellationPending)
                                {
                                    e.Result = deviceParameter.Name;
                                    return;
                                }
                                foreach (var item in deviceParameter.TempParam.TimeSeries.TimeSeriesItemCol)
                                {
                                    if (worker.CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }
                                    float time = 0;
                                    switch (deviceParameter.TempParam.TimeSeries.Timer)
                                    {
                                        case FeedTimer.Minute:
                                            time = Convert.ToSingle(item.Time);
                                            break;
                                        case FeedTimer.Hour:
                                            time = Convert.ToSingle(item.Time * 60);
                                            break;
                                        case FeedTimer.Day:
                                            time = Convert.ToSingle(item.Time * 60 * 24);
                                            break;
                                    }
                                    if (timeOffset <= time)
                                    {
                                        deviceParameter.TempParam.Temp_PV = (float)item.Value;
                                        break;
                                    }
                                }
                                Thread.Sleep(1000);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Debug(string.Format("温度时间序列调整失败，错误信息：{0}", ex.Message));
                                Thread.Sleep(10000);
                            }
                        }
                    });
                    dicTempTimeWorker[currentDeviceParameter.Name].RunWorkerCompleted += ((s, e) =>
                    {
                        BackgroundWorker backgroundWorker = (BackgroundWorker)s;
                        backgroundWorker.Dispose();
                        backgroundWorker = null;

                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        deviceParameter.TempParam.ControlMode = ControlMode.Enable;
                    });
                    dicTempTimeWorker[currentDeviceParameter.Name].RunWorkerAsync();
                    break;
            }
        });

        /// <summary>
        /// 获取DO修正系数
        /// </summary>
        /// <param name="timeOffset"></param>
        /// <returns></returns>
        private float GetDoK(float timeOffset)
        {
            float k = 1;
            List<DOTimeSeries> pIDInfos = new List<DOTimeSeries>();
            if (File.Exists(FileConst.DORTInfoPath))
            {
                string result = File.ReadAllText(FileConst.DORTInfoPath);
                pIDInfos = CustomApp.JsonHelper.StringToObject<List<DOTimeSeries>>(result);
            }
            DOTS = new ObservableCollection<DOTimeSeries>(pIDInfos);
            foreach (var item in DOTS)
            {
                if (timeOffset >= item.startTime && timeOffset < item.endTime)
                {
                    k = item.expectation / 100.0f;
                    break;
                }
            }
            return k;
        }
        public DelegateCommand<DeviceParameter> DORunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }

            if (dicDOWorker[currentDeviceParameter.Name] != null && dicDOWorker[currentDeviceParameter.Name].IsBusy)
            {
                dicDOWorker[currentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            if (dicDOTempWorker[currentDeviceParameter.Name] != null && dicDOTempWorker[currentDeviceParameter.Name].IsBusy)
            {
                dicDOTempWorker[currentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            //关闭控制
            if (!currentDeviceParameter.DOParam.IsControling)
            {
                return;
            }

            float agitHigh = 0;

            dicDOPid[currentDeviceParameter.Name].Reset();
            dicDOWorker[currentDeviceParameter.Name] = new BackgroundWorker();
            dicDOWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;      // 允许报告进度
            dicDOWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true; // 允许取消操作
            // 绑定事件
            dicDOWorker[currentDeviceParameter.Name].DoWork += ((s, e) =>
            {
                var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);

                deviceParameter.IsDOLimit = deviceParameter.DORegulationLimit = false;
                e.Result = deviceParameter.Name;

                float maxMFCFlow = 0;//用于通气量的总和
                //mid-ranging控制 只有启用了mid-ranging且目标DO大于当前DO
                if (deviceParameter.DOParam.ControlStrategy == DOControlStrategy.Midranging)
                {
                    var agit = -1;
                    var airFlow = 0f;
                    var o2Flow = 0f;

                    MidRangingParam param = MidRangingParamManager.GetInstance().MidRangingParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                    RealTimeParam realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                    deviceParameter.AgitParam.IsControling = deviceParameter.AirParam.IsControling = true;

                    float initialAir = 0;
                    if (param.Unit == 0)//VVM
                    {
                        initialAir = MathF.Round((float)(param.InitialAir * (realTimeParam.JarWeight - 2000) / 1000), 2);
                    }
                    else if (param.Unit == 1)//L/min
                    {
                        initialAir = param.InitialAir;
                    }
                    //deviceParameter.AirParam.FlowSpeed = realTimeParam.AirFlowSpeed < initialAir ? initialAir : realTimeParam.AirFlowSpeed;
                    //deviceParameter.AgitParam.Agit_PV = realTimeParam.Agit < param.AgitLowerLimit ? param.AgitLowerLimit : realTimeParam.Agit;
                    deviceParameter.AirParam.FlowSpeed = initialAir;
                    deviceParameter.AgitParam.Agit_PV = param.AgitLowerLimit;

                    AgitRunCommand.Execute(deviceParameter);
                    AirRunCommand.Execute(deviceParameter);

                    if (param.O2Associated)
                    {
                        //刚开始不通氧气
                        //float initialO2 = 0;
                        //if (param.Unit == 0)//VVM
                        //{
                        //    initialO2 = MathF.Round((float)(param.InitialO2 * (realTimeParam.JarWeight - 2000) / 1000), 2);
                        //}
                        //else if (param.Unit == 1)//L/min
                        //{
                        //    initialO2 = param.InitialO2;
                        //}
                        //deviceParameter.O2Param.FlowSpeed = realTimeParam.O2FlowSpeed < initialO2 ? initialO2 : realTimeParam.O2FlowSpeed;
                        //deviceParameter.O2Param.IsControling = true;
                        //O2RunCommand.Execute(deviceParameter);
                    }

                    agit = deviceParameter.AgitParam.Agit_PV;
                    airFlow = deviceParameter.AirParam.FlowSpeed;
                    o2Flow = deviceParameter.O2Param.FlowSpeed;

                    Thread.Sleep(1000);

                    while (deviceParameter.DO > deviceParameter.DOParam.DO_PV)
                    {
                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                        {
                            e.Result = deviceParameter.Name;
                            return;
                        }
                        Thread.Sleep(1000);
                    }

                    deviceParameter.IsDOLimit = false;
                    dicDOPid[deviceParameter.Name].Reset();
                    dicDODelta[deviceParameter.Name] = 0;

                    dicDOAgitPid[deviceParameter.Name].Reset();
                    PIDInfo info = null;

                    while (true)
                    {
                        try
                        {
                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                e.Result = deviceParameter.Name;
                                return;
                            }

                            while (deviceParameter.DO > deviceParameter.DOParam.DO_PV)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    e.Result = deviceParameter.Name;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }

                            param = MidRangingParamManager.GetInstance().MidRangingParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                            string result = File.ReadAllText(FileConst.PidInfoPath);
                            List<PIDInfo> pIDInfos = JsonConvert.DeserializeObject<List<PIDInfo>>(result);
                            if (pIDInfos == null)
                            {
                                MessageBox.Show("PID调控策略列表为空");
                                e.Result = deviceParameter.Name;
                                return;
                            }
                            info = pIDInfos.FindFirst(t => t.PidName.Contains("DO_正向") && t.deviceID == deviceParameter.Name);

                            if (info == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在DO的PID调控策略", deviceParameter.Name));

                                e.Result = deviceParameter.Name;
                                return;
                            }

                            RealTimeParam realTimeParam1 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (realTimeParam1.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam1.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)
                            {
                                agit = deviceParameter.AgitParam.Agit_PV;
                                airFlow = deviceParameter.AirParam.FlowSpeed;
                                o2Flow = deviceParameter.O2Param.FlowSpeed;

                                deviceParameter.IsDOLimit = false;
                                dicDOAgitPid[deviceParameter.Name].Reset();
                                dicDOPid[deviceParameter.Name].Reset();
                                dicDODelta[deviceParameter.Name] = 0;
                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                int index1 = 0;
                                while (index1 < count)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    index1 += 1;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }

                            dicDOPid[deviceParameter.Name].SetParameters(kp: (float)info.P, ki: (float)info.I, kd: (float)info.D, integralThreshold: info.Threshold, interval: info.Interval);
                            dicDOPid[deviceParameter.Name].SetOutputLimits(-Math.Abs(info.maxSpeed), Math.Abs(info.maxSpeed));
                            dicDOPid[deviceParameter.Name].SetIntegralLimits(-2000, 2000);
                            dicDOPid[deviceParameter.Name].SetTarget(deviceParameter.DOParam.DO_PV);

                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                e.Result = deviceParameter.Name;
                                return;
                            }

                            LogHelper.Debug(string.Format("反应器{6} Mid-Ranging DO预设值：{0}，DO当前值：{1}，P：{2}，I：{3}，D：{4},采样时间：{5}", deviceParameter.DOParam.DO_PV, realTimeParam1.DO, info.P, info.I, info.D, info.Interval, deviceParameter.Name));

                            float temp = dicDOPid[deviceParameter.Name].CalculatePositional_DO((float)realTimeParam1.DO);

                            int tempAgit = Convert.ToInt32(agit + temp);
                            LogHelper.Debug(string.Format("反应器{0} Mid-Ranging 转速底值：{1}，Delta：{2}", deviceParameter.Name, agit, temp));
                            if (tempAgit >= param.AgitUpperLimit)
                            {
                                tempAgit = param.AgitUpperLimit;
                            }
                            else if (tempAgit <= param.AgitLowerLimit)
                            {
                                tempAgit = param.AgitLowerLimit;
                            }
                            deviceParameter.AgitParam.Agit_PV = tempAgit;
                            Thread.Sleep(3000);

                            realTimeParam1 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (realTimeParam1.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam1.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)
                            {
                                agit = deviceParameter.AgitParam.Agit_PV;
                                airFlow = deviceParameter.AirParam.FlowSpeed;
                                o2Flow = deviceParameter.O2Param.FlowSpeed;

                                deviceParameter.IsDOLimit = false;
                                dicDOAgitPid[deviceParameter.Name].Reset();
                                dicDOPid[deviceParameter.Name].Reset();
                                dicDODelta[deviceParameter.Name] = 0;
                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                int index1 = 0;
                                while (index1 < count)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    index1 += 1;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }
                            if (deviceParameter.AgitParam.Agit_PV >= param.AgitHigh)//到达转速高限
                            {
                                if (deviceParameter.AirParam.FlowSpeed >= param.AirUpperLimit)//通气已经到达极限
                                {
                                    if (realTimeParam1.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam1.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)
                                    {
                                        agit = deviceParameter.AgitParam.Agit_PV;
                                        airFlow = deviceParameter.AirParam.FlowSpeed;
                                        o2Flow = deviceParameter.O2Param.FlowSpeed;

                                        deviceParameter.IsDOLimit = false;
                                        dicDOAgitPid[deviceParameter.Name].Reset();
                                        dicDOPid[deviceParameter.Name].Reset();
                                        dicDODelta[deviceParameter.Name] = 0;
                                        int count = info.Interval <= 0 ? 1 : info.Interval;
                                        int index1 = 0;
                                        while (index1 < count)
                                        {
                                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                            {
                                                e.Result = deviceParameter.Name;
                                                return;
                                            }

                                            index1 += 1;
                                            Thread.Sleep(1000);
                                        }
                                        continue;
                                    }

                                    if (param.O2Associated)
                                    {
                                        break;//跳到氧气
                                    }
                                    else
                                    {
                                        deviceParameter.IsDOLimit = true;
                                    }
                                }
                                else
                                {
                                    result = File.ReadAllText(FileConst.PidInfoPath);
                                    pIDInfos = JsonConvert.DeserializeObject<List<PIDInfo>>(result);
                                    if (pIDInfos == null)
                                    {
                                        MessageBox.Show("PID调控策略列表为空");
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }
                                    PIDInfo info1 = pIDInfos.FindFirst(t => t.PidName.Contains("通气") && t.deviceID == deviceParameter.Name);

                                    if (info1 == null)
                                    {
                                        MessageBox.Show(string.Format("反应器{0}不存在DO的PID调控策略", deviceParameter.Name));
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    realTimeParam1 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                    dicDOAgitPid[deviceParameter.Name].Reset();
                                    dicDOAgitPid[deviceParameter.Name].SetParameters(kp: (float)info1.P, ki: (float)info1.I, kd: (float)info1.D, integralThreshold: info1.Threshold, interval: info1.Interval);
                                    dicDOAgitPid[deviceParameter.Name].SetOutputLimits(-Math.Abs(info1.maxSpeed), Math.Abs(info1.maxSpeed));
                                    dicDOAgitPid[deviceParameter.Name].SetIntegralLimits(-2000, 2000);
                                    dicDOAgitPid[deviceParameter.Name].SetTarget(realTimeParam1.Agit);

                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }
                                    float tempAir = dicDOAgitPid[deviceParameter.Name].CalculateIncremental(param.AgitHigh);
                                    float airSpeed = realTimeParam1.AirFlowSpeed + tempAir;
                                    if (airSpeed >= param.AirUpperLimit)
                                    {
                                        airSpeed = param.AirUpperLimit;
                                    }
                                    else if (airSpeed <= param.InitialAir)
                                    {
                                        airSpeed = param.InitialAir;
                                    }
                                    deviceParameter.AirParam.FlowSpeed = airSpeed;
                                    LogHelper.Debug(string.Format("反应器{0} Mid-Ranging 通气预设值：{1}，底值：{2}，delta：{3}", deviceParameter.Name, airSpeed, airFlow, tempAir));
                                    int count = info.Interval <= 60 ? 1 : info.Interval;
                                    int index1 = 0;
                                    while (index1 < count)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            e.Result = deviceParameter.Name;
                                            return;
                                        }

                                        index1 += 1;
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                            realTimeParam1 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (realTimeParam1.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam1.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)
                            {
                                agit = deviceParameter.AgitParam.Agit_PV;
                                airFlow = deviceParameter.AirParam.FlowSpeed;
                                o2Flow = deviceParameter.O2Param.FlowSpeed;

                                deviceParameter.IsDOLimit = false;
                                dicDOAgitPid[deviceParameter.Name].Reset();
                                dicDOPid[deviceParameter.Name].Reset();
                                dicDODelta[deviceParameter.Name] = 0;
                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                int index1 = 0;
                                while (index1 < count)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    index1 += 1;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("DO调整失败_Mid-Ranging，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }

                    while (param.O2Associated)
                    {
                        try
                        {
                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                e.Result = deviceParameter.Name;
                                return;
                            }

                            while (deviceParameter.DO > deviceParameter.DOParam.DO_PV)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    e.Result = deviceParameter.Name;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }

                            param = MidRangingParamManager.GetInstance().MidRangingParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                            string result = File.ReadAllText(FileConst.PidInfoPath);
                            List<PIDInfo> pIDInfos = JsonConvert.DeserializeObject<List<PIDInfo>>(result);
                            if (pIDInfos == null)
                            {
                                MessageBox.Show("PID调控策略列表为空");
                                e.Result = deviceParameter.Name;
                                return;
                            }
                            info = pIDInfos.FindFirst(t => t.PidName.Contains("氧气") && t.deviceID == deviceParameter.Name);

                            if (info == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在DO的PID调控策略", deviceParameter.Name));

                                e.Result = deviceParameter.Name;
                                return;
                            }

                            RealTimeParam realTimeParam1 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (realTimeParam1.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam1.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)
                            {
                                agit = deviceParameter.AgitParam.Agit_PV;
                                airFlow = deviceParameter.AirParam.FlowSpeed;
                                o2Flow = deviceParameter.O2Param.FlowSpeed;

                                deviceParameter.IsDOLimit = false;
                                dicDOAgitPid[deviceParameter.Name].Reset();
                                dicDOPid[deviceParameter.Name].Reset();
                                dicDODelta[deviceParameter.Name] = 0;
                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                int index1 = 0;
                                while (index1 < count)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    index1 += 1;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }

                            dicDOPid[deviceParameter.Name].SetParameters(kp: (float)info.P, ki: (float)info.I, kd: (float)info.D, integralThreshold: info.Threshold, interval: info.Interval);
                            dicDOPid[deviceParameter.Name].SetOutputLimits(-Math.Abs(info.maxSpeed), Math.Abs(info.maxSpeed));
                            dicDOPid[deviceParameter.Name].SetIntegralLimits(-2000, 2000);
                            dicDOPid[deviceParameter.Name].SetTarget(deviceParameter.DOParam.DO_PV);

                            LogHelper.Debug(string.Format("反应器{6} Mid-Ranging DO预设值：{0}，DO当前值：{1}，P：{2}，I：{3}，D：{4},采样时间：{5}", deviceParameter.DOParam.DO_PV, realTimeParam1.DO, info.P, info.I, info.D, info.Interval, deviceParameter.Name));

                            float temp = dicDOPid[deviceParameter.Name].CalculatePositional_DO((float)realTimeParam1.DO);
                            deviceParameter.AgitParam.Agit_PV = Convert.ToInt32(agit + temp);
                            if (deviceParameter.AgitParam.Agit_PV >= param.AgitUpperLimit)
                            {
                                deviceParameter.AgitParam.Agit_PV = param.AgitUpperLimit;
                            }
                            else if (deviceParameter.AgitParam.Agit_PV <= param.AgitLowerLimit)
                            {
                                deviceParameter.AgitParam.Agit_PV = param.AgitLowerLimit;
                            }
                            Thread.Sleep(1000);

                            realTimeParam1 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (realTimeParam1.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam1.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)
                            {
                                agit = deviceParameter.AgitParam.Agit_PV;
                                airFlow = deviceParameter.AirParam.FlowSpeed;
                                o2Flow = deviceParameter.O2Param.FlowSpeed;

                                deviceParameter.IsDOLimit = false;
                                dicDOAgitPid[deviceParameter.Name].Reset();
                                dicDOPid[deviceParameter.Name].Reset();
                                dicDODelta[deviceParameter.Name] = 0;
                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                int index1 = 0;
                                while (index1 < count)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    index1 += 1;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }
                            if (deviceParameter.AgitParam.Agit_PV >= param.AgitHigh)//到达转速高限
                            {
                                if (deviceParameter.O2Param.FlowSpeed >= param.O2UpperLimit)//氧气已经到达极限
                                {
                                    if (realTimeParam1.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam1.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)
                                    {
                                        agit = deviceParameter.AgitParam.Agit_PV;
                                        airFlow = deviceParameter.AirParam.FlowSpeed;
                                        o2Flow = deviceParameter.O2Param.FlowSpeed;

                                        deviceParameter.IsDOLimit = false;
                                        dicDOAgitPid[deviceParameter.Name].Reset();
                                        dicDOPid[deviceParameter.Name].Reset();
                                        dicDODelta[deviceParameter.Name] = 0;
                                        int count = info.Interval <= 0 ? 1 : info.Interval;
                                        int index1 = 0;
                                        while (index1 < count)
                                        {
                                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                            {
                                                e.Result = deviceParameter.Name;
                                                return;
                                            }

                                            index1 += 1;
                                            Thread.Sleep(1000);
                                        }
                                        continue;
                                    }
                                    if (!deviceParameter.TempDOAssociated)
                                    {
                                        if (deviceParameter.FeedDOAssociated)
                                        {
                                            deviceParameter.DORegulationLimit = true;
                                        }
                                    }
                                    deviceParameter.IsDOLimit = true;

                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }
                                }
                                else
                                {
                                    result = File.ReadAllText(FileConst.PidInfoPath);
                                    pIDInfos = JsonConvert.DeserializeObject<List<PIDInfo>>(result);
                                    if (pIDInfos == null)
                                    {
                                        MessageBox.Show("PID调控策略列表为空");
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }
                                    PIDInfo info1 = pIDInfos.FindFirst(t => t.PidName.Contains("通气") && t.deviceID == deviceParameter.Name);

                                    if (info1 == null)
                                    {
                                        MessageBox.Show(string.Format("反应器{0}不存在通气的PID调控策略", deviceParameter.Name));
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    dicDOAgitPid[deviceParameter.Name].SetParameters(kp: (float)info1.P, ki: (float)info1.I, kd: (float)info1.D, integralThreshold: info1.Threshold, interval: info1.Interval);
                                    dicDOAgitPid[deviceParameter.Name].SetOutputLimits(-Math.Abs(info1.maxSpeed), Math.Abs(info1.maxSpeed));
                                    dicDOAgitPid[deviceParameter.Name].SetIntegralLimits(-2000, 2000);
                                    dicDOAgitPid[deviceParameter.Name].SetTarget(deviceParameter.Agit);

                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    float tempO2 = dicDOAgitPid[deviceParameter.Name].CalculatePositional((float)param.AgitHigh);
                                    float o2Speed = o2Flow + tempO2;
                                    if (o2Speed >= param.O2UpperLimit)
                                    {
                                        o2Speed = param.O2UpperLimit;
                                    }
                                    else if (o2Speed <= param.InitialO2)
                                    {
                                        o2Speed = param.InitialO2;
                                    }
                                    deviceParameter.O2Param.FlowSpeed = o2Speed;
                                    if (deviceParameter.AirParam.FlowSpeed > tempO2)
                                    {
                                        deviceParameter.AirParam.FlowSpeed -= tempO2;
                                    }
                                    else
                                    {
                                        deviceParameter.AirParam.FlowSpeed = 0;
                                    }
                                    LogHelper.Debug(string.Format("反应器{0} Mid-Ranging 氧气预设值：{1}，底值：{2}，delta：{3}", deviceParameter.Name, o2Speed, airFlow, tempO2));
                                    int count = info.Interval <= 60 ? 1 : info.Interval;
                                    int index1 = 0;
                                    while (index1 < count)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            e.Result = deviceParameter.Name;
                                            return;
                                        }

                                        index1 += 1;
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                            realTimeParam1 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (realTimeParam1.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam1.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)
                            {
                                agit = deviceParameter.AgitParam.Agit_PV;
                                airFlow = deviceParameter.AirParam.FlowSpeed;
                                o2Flow = deviceParameter.O2Param.FlowSpeed;

                                deviceParameter.IsDOLimit = false;
                                dicDOAgitPid[deviceParameter.Name].Reset();
                                dicDOPid[deviceParameter.Name].Reset();
                                dicDODelta[deviceParameter.Name] = 0;
                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                int index1 = 0;
                                while (index1 < count)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    index1 += 1;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("DO调整失败_Mid-Ranging，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                }
                //周期
                else if (deviceParameter.DOParam.ControlStrategy == DOControlStrategy.Cycle)
                {
                    deviceParameter.AgitParam.IsControling = true;
                    AgitRunCommand.Execute(deviceParameter);
                    while (true)
                    {
                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                        {
                            return;
                        }
                        RealTimeParam realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                        if (realTimeParam.DO < deviceParameter.DOParam.DO_PV)
                        {
                            deviceParameter.AgitParam.Agit_PV += deviceParameter.DOParam.AgitCycle.DirectStep;
                            if (deviceParameter.AgitParam.Agit_PV < deviceParameter.DOParam.AgitCycle.LowerLimit)
                            {
                                deviceParameter.AgitParam.Agit_PV = deviceParameter.DOParam.AgitCycle.LowerLimit;
                            }
                            else if (deviceParameter.AgitParam.Agit_PV > deviceParameter.DOParam.AgitCycle.UpperLimit)
                            {
                                deviceParameter.AgitParam.Agit_PV = deviceParameter.DOParam.AgitCycle.UpperLimit;
                            }

                            int count = deviceParameter.DOParam.AgitCycle.DirectInterval;
                            int index = 0;
                            while (index < count)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    return;
                                }
                                index += 1;
                                Thread.Sleep(1000);
                            }
                        }
                        else if (realTimeParam.DO > deviceParameter.DOParam.DO_PV)
                        {
                            deviceParameter.AgitParam.Agit_PV -= deviceParameter.DOParam.AgitCycle.ReverseStep;

                            if (deviceParameter.AgitParam.Agit_PV < deviceParameter.DOParam.AgitCycle.LowerLimit)
                            {
                                deviceParameter.AgitParam.Agit_PV = deviceParameter.DOParam.AgitCycle.LowerLimit;
                            }
                            else if (deviceParameter.AgitParam.Agit_PV > deviceParameter.DOParam.AgitCycle.UpperLimit)
                            {
                                deviceParameter.AgitParam.Agit_PV = deviceParameter.DOParam.AgitCycle.UpperLimit;
                            }

                            int count = deviceParameter.DOParam.AgitCycle.ReverseInterval;
                            int index = 0;
                            while (index < count)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    return;
                                }
                                index += 1;
                                Thread.Sleep(1000);
                            }
                        }

                    }

                }
                //级联通气控制
                else
                {
                    DateTime startTime = DateTime.Now;
                    var sv = deviceParameter.DOParam.DO_PV;
                    int lastAgit = 0;
                    var agit = -1;

                    PIDInfo info = null;
                    PIDInfo lastPid = null;

                    DOAssParam param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);

                    deviceParameter.AgitParam.IsControling = deviceParameter.AirParam.IsControling = true;
                    AgitRunCommand.Execute(deviceParameter);
                    AirRunCommand.Execute(deviceParameter);
                    if (param.O2Col.Count > 1)
                    {
                        deviceParameter.O2Param.IsControling = true;
                        O2RunCommand.Execute(deviceParameter);
                    }

                    RealTimeParam realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);

                    while (realTimeParam.DO < deviceParameter.DOParam.DO_PV && !deviceParameter.DOParam.IsDirect)
                    {
                        realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                        {
                            e.Result = deviceParameter.Name;
                            return;
                        }
                        Thread.Sleep(1000);
                    }

                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                    while (realTimeParam.DO > deviceParameter.DOParam.DO_PV && !deviceParameter.DOParam.IsReverse)
                    {
                        realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                        {
                            e.Result = deviceParameter.Name;
                            return;
                        }
                        Thread.Sleep(1000);
                    }

                    if (param.Unit == 0)//VVM
                    {
                        maxMFCFlow = MathF.Round((float)(param.AirCol[param.AirCol.Count - 1].VVM * (deviceParameter.JarWeight - 2000) / 1000), 2);
                    }
                    else if (param.Unit == 1)//L/min
                    {
                        maxMFCFlow = param.AirCol[param.AirCol.Count - 1].VVM;
                    }
                    if (dicAirIndex[deviceParameter.Name] > param.AirCol.Count - 1 && param.AirCol.Count > 0)
                    {
                        LogHelper.Debug(string.Format("上次通气档位为{0}，当前档位总共{1}档，所以跳至{2}档", dicAirIndex[deviceParameter.Name] + 1, param.AirCol.Count, param.AirCol.Count));
                        dicAirIndex[deviceParameter.Name] = param.AirCol.Count - 1;
                    }

                    if (dicO2Index[deviceParameter.Name] > param.O2Col.Count - 1 && param.O2Col.Count > 0)
                    {
                        LogHelper.Debug(string.Format("上次氧气档位为{0}，当前档位总共{1}档，所以跳至{2}档", dicO2Index[deviceParameter.Name] + 1, param.O2Col.Count, param.O2Col.Count));
                        dicO2Index[deviceParameter.Name] = param.AirCol.Count - 1;
                    }

                    while (dicAirIndex[deviceParameter.Name] < param.AirCol.Count)
                    {
                        try
                        {
                            param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                e.Result = deviceParameter.Name;
                                return;
                            }
                            string result = File.ReadAllText(FileConst.PidInfoPath);
                            List<PIDInfo> pIDInfos = JsonConvert.DeserializeObject<List<PIDInfo>>(result);
                            if (pIDInfos == null)
                            {
                                MessageBox.Show("PID调控策略列表为空");
                                e.Result = deviceParameter.Name;
                                return;
                            }
                            RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (realTime.DO <= deviceParameter.DOParam.DO_PV)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("DO_正向") && t.deviceID == deviceParameter.Name);
                            }
                            else if (realTime.DO >= deviceParameter.DOParam.DO_PV)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("DO_反向") && t.deviceID == deviceParameter.Name);
                            }

                            if (info == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在DO的PID调控策略", deviceParameter.Name));
                                e.Result = deviceParameter.Name;
                                return;
                            }

                            //如果pid类型变了，pid系数清零 方成
                            if (info != null && lastPid != null && info.PidName != lastPid.PidName)
                            {
                                LogHelper.Debug(string.Format("反应器{2} DO调控：由{0}切换至{1}", lastPid.PidName, info.PidName, deviceParameter.Name));
                                if (agit == -1)
                                {
                                    agit = realTime.Agit;
                                }
                                else
                                {
                                    agit = deviceParameter.AgitParam.Agit_PV;
                                }
                                LogHelper.Debug(string.Format("反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTime.Agit, deviceParameter.AgitParam.Agit_PV, agit));
                                deviceParameter.IsDOLimit = false;
                                dicDOPid[deviceParameter.Name].Reset();
                                dicDODelta[deviceParameter.Name] = 0;
                            }
                            lastPid = info;

                            realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (realTime.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTime.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)
                            {
                                LogHelper.Debug(string.Format("溶氧进入死区，反应器{0}，上次转速为{1}，设置为{2}", deviceParameter.Name, agit, lastAgit, realTime.Agit));
                                if (agit == -1)
                                {
                                    agit = realTime.Agit;
                                }
                                else
                                {
                                    agit = deviceParameter.AgitParam.Agit_PV;
                                }
                                LogHelper.Debug(string.Format("反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTime.Agit, deviceParameter.AgitParam.Agit_PV, agit));
                                deviceParameter.IsDOLimit = false;
                                dicDOPid[deviceParameter.Name].Reset();
                                dicDODelta[deviceParameter.Name] = 0;

                                {
                                    int count = info.Interval <= 0 ? 1 : info.Interval;
                                    int index1 = 0;
                                    while (index1 < count)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            e.Result = deviceParameter.Name;
                                            return;
                                        }

                                        index1 += 1;
                                        Thread.Sleep(1000);
                                    }
                                }
                                continue;
                            }

                            realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            while (realTime.DO < deviceParameter.DOParam.DO_PV - info.deadArea && !deviceParameter.DOParam.IsDirect)
                            {
                                realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    e.Result = deviceParameter.Name;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }

                            realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            while (realTime.DO > deviceParameter.DOParam.DO_PV + info.deadArea && !deviceParameter.DOParam.IsReverse)
                            {
                                realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    e.Result = deviceParameter.Name;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }

                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                e.Result = deviceParameter.Name;
                                return;
                            }

                            dicDOPid[deviceParameter.Name].SetParameters(kp: (float)info.P, ki: (float)info.I, kd: (float)info.D, integralThreshold: info.Threshold, interval: info.Interval);
                            dicDOPid[deviceParameter.Name].SetOutputLimits(-Math.Abs(info.maxSpeed), Math.Abs(info.maxSpeed));
                            dicDOPid[deviceParameter.Name].SetIntegralLimits(-2000, 2000);
                            dicDOPid[deviceParameter.Name].SetTarget(deviceParameter.DOParam.DO_PV);

                            LogHelper.Debug(string.Format("反应器{6},DO预设值：{0}，DO当前值：{4}，P：{1}，I：{2}，D：{3},采样时间：{5}", deviceParameter.DOParam.DO_PV, info.P, info.I, info.D, deviceParameter.DO, info.Interval, deviceParameter.Name));

                            RealTimeParam realTimeParam1 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (agit == -1 || sv != deviceParameter.DOParam.DO_PV)
                            {
                                LogHelper.Debug(string.Format("反应器{3},给转速设置底值：{0}，溶氧上次设定值：{1}，当前设定值：{2}", realTimeParam1.Agit, sv, deviceParameter.DOParam.DO_PV, deviceParameter.Name));
                                agit = Convert.ToInt32(realTimeParam1.Agit);
                                sv = deviceParameter.DOParam.DO_PV;
                            }

                            //每次循环都检查当前通气量是否更新 方成
                            param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                            if (param != null)
                            {
                                agitHigh = param.AgitUpperLimit;

                                var doair = param.AirCol[dicAirIndex[deviceParameter.Name]];
                                if (doair != null)
                                {
                                    if (param.Unit == 0)//VVM
                                    {
                                        deviceParameter.AirParam.FlowSpeed = MathF.Round((float)(doair.VVM * (realTimeParam1.JarWeight - 2000) / 1000), 2);
                                    }
                                    else if (param.Unit == 1)//L/min
                                    {
                                        deviceParameter.AirParam.FlowSpeed = doair.VVM;
                                    }

                                    if (realTimeParam1.AirFlowSpeed != deviceParameter.AirParam.FlowSpeed)
                                    {
                                        Thread.Sleep(1000);//等待通气改变
                                    }
                                }

                                if (param.O2Col.Count > 0 && dicO2Index[deviceParameter.Name] <= param.O2Col.Count - 1)
                                {
                                    var doO2 = param.O2Col[dicO2Index[deviceParameter.Name]];
                                    if (doO2 != null)
                                    {
                                        if (param.Unit == 0)//VVM
                                        {
                                            deviceParameter.O2Param.FlowSpeed = MathF.Round((float)(doO2.VVM * (realTimeParam1.JarWeight - 2000) / 1000), 2);
                                        }
                                        else if (param.Unit == 1)//L/min
                                        {
                                            deviceParameter.O2Param.FlowSpeed = doO2.VVM;
                                        }

                                        if (realTimeParam1.O2FlowSpeed != deviceParameter.O2Param.FlowSpeed)
                                        {
                                            Thread.Sleep(1000);//等待通气改变
                                        }
                                    }
                                }
                            }

                            // 位置式PID计算
                            float temp = dicDOPid[deviceParameter.Name].CalculatePositional_DO((float)realTimeParam1.DO);
                            float k = GetDoK((float)(DateTime.Now - startTime).TotalMinutes);
                            temp = temp * k;
                            temp = (float)(temp + agit);//加上次值
                            dicDODelta[deviceParameter.Name] = Convert.ToInt32(temp);//由增量式变更为位置式
                            LogHelper.Debug(string.Format("反应器{1},DO总Delta:{0},单次Delta:{2},K值：{3},上次转速:{4}", dicDODelta[deviceParameter.Name], deviceParameter.Name, temp, k, agit));
                            //没勾选反向，当PID算出来的值小于上一次的值时，当次值等于上一次值
                            if (!deviceParameter.DOParam.IsReverse && dicDODelta[deviceParameter.Name] < lastAgit)
                            {
                                LogHelper.Debug(string.Format("反应器{2}，没勾选反向，当前值：{0}，上次值：{1}", dicDODelta[deviceParameter.Name], lastAgit, deviceParameter.Name));
                                dicDODelta[deviceParameter.Name] = lastAgit;
                            }
                            //没勾选正向，当PID算出来的值大于上一次的值时，当次值等于上一次值
                            if (!deviceParameter.DOParam.IsDirect && dicDODelta[deviceParameter.Name] > lastAgit)
                            {
                                LogHelper.Debug(string.Format("反应器{2}，没勾选正向，当前值：{0}，上次值：{1}", dicDODelta[deviceParameter.Name], lastAgit, deviceParameter.Name));
                                dicDODelta[deviceParameter.Name] = lastAgit;
                            }

                            if (dicDODelta[deviceParameter.Name] >= param.AgitUpperLimit)
                            {
                                dicDODelta[deviceParameter.Name] = param.AgitUpperLimit;
                            }
                            else if (dicDODelta[deviceParameter.Name] <= param.AgitLowerLimit)
                            {
                                dicDODelta[deviceParameter.Name] = param.AgitLowerLimit;
                            }

                            lastAgit = dicDODelta[deviceParameter.Name];
                            deviceParameter.AgitParam.Agit_PV = dicDODelta[deviceParameter.Name];
                            Thread.Sleep(1000);//等待转速设置下去

                            //InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(deviceParameter.Name, dicDODelta[deviceParameter.Name]);

                            {
                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                int index1 = 0;
                                while (index1 < count)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    index1 += 1;
                                    Thread.Sleep(1000);
                                }
                            }

                            RealTimeParam realTimeParam2 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (realTimeParam2.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam2.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)//DO调整完毕
                            {
                                if (agit == -1)
                                {
                                    agit = realTimeParam2.Agit;
                                }
                                else
                                {
                                    agit = deviceParameter.AgitParam.Agit_PV;
                                }
                                LogHelper.Debug(string.Format("溶氧进入死区，反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam2.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                deviceParameter.IsDOLimit = false;
                                dicDOPid[deviceParameter.Name].Reset();
                                dicDODelta[deviceParameter.Name] = 0;

                                //溶氧已调整完毕，不调整参数

                                {
                                    int count = info.Interval <= 0 ? 1 : info.Interval;
                                    int index1 = 0;
                                    while (index1 < count)
                                    {
                                        realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (realTimeParam.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)//DO调整完毕
                                        {
                                            if (agit == -1)
                                            {
                                                agit = realTimeParam.Agit;
                                            }
                                            else
                                            {
                                                agit = deviceParameter.AgitParam.Agit_PV;
                                            }
                                            LogHelper.Debug(string.Format("溶氧进入死区，反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                            deviceParameter.IsDOLimit = false;
                                            dicDOPid[deviceParameter.Name].Reset();
                                            dicDODelta[deviceParameter.Name] = 0;
                                            break;
                                        }

                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            e.Result = deviceParameter.Name;
                                            return;
                                        }

                                        index1 += 1;
                                        Thread.Sleep(1000);
                                    }
                                }
                                continue;
                            }

                            if (dicDODelta[deviceParameter.Name] == param.AgitUpperLimit)//达到上限
                            {
                                int count = 60;
                                int index1 = 0;
                                while (index1 < count)
                                {
                                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                    if (realTimeParam.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)//DO调整完毕
                                    {
                                        if (agit == -1)
                                        {
                                            agit = realTimeParam.Agit;
                                        }
                                        else
                                        {
                                            agit = deviceParameter.AgitParam.Agit_PV;
                                        }
                                        LogHelper.Debug(string.Format("溶氧进入死区，反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                        deviceParameter.IsDOLimit = false;
                                        dicDOPid[deviceParameter.Name].Reset();
                                        dicDODelta[deviceParameter.Name] = 0;
                                        //溶氧已调整完毕，不调整参数
                                        break;
                                    }

                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    index1 += 1;
                                    Thread.Sleep(1000);
                                }

                                RealTimeParam realTimeParam3 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                if (realTimeParam3.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam3.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)//DO调整完毕
                                {
                                    if (agit == -1)
                                    {
                                        agit = realTimeParam3.Agit;
                                    }
                                    else
                                    {
                                        agit = deviceParameter.AgitParam.Agit_PV;
                                    }
                                    LogHelper.Debug(string.Format("溶氧进入死区，反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam3.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                    deviceParameter.IsDOLimit = false;
                                    dicDOPid[deviceParameter.Name].Reset();
                                    dicDODelta[deviceParameter.Name] = 0;
                                    //溶氧已调整完毕，不调整参数
                                    continue;
                                }

                                if (realTimeParam3.DO < deviceParameter.DOParam.DO_PV - info.deadArea)
                                {//溶氧低于设定值，需要调整到下一档通气
                                    if (dicAirIndex[deviceParameter.Name] < param.AirCol.Count - 1)//存在下一阶段
                                    {
                                        dicAirIndex[deviceParameter.Name] += 1;
                                        continue;
                                    }
                                    if (param.O2Col.Count > 0)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        if (!deviceParameter.TempDOAssociated)
                                        {
                                            if (deviceParameter.FeedDOAssociated)
                                            {
                                                deviceParameter.DORegulationLimit = true;
                                            }
                                        }
                                        deviceParameter.IsDOLimit = true;
                                    }
                                }
                            }
                            else if (dicDODelta[deviceParameter.Name] == param.AgitLowerLimit)//达到下限
                            {
                                //当到达上下限时，等待60秒
                                int count = 60;
                                int index1 = 0;
                                while (index1 < count)
                                {
                                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                    if (realTimeParam.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)//DO调整完毕
                                    {
                                        if (agit == -1)
                                        {
                                            agit = realTimeParam.Agit;
                                        }
                                        else
                                        {
                                            agit = deviceParameter.AgitParam.Agit_PV;
                                        }
                                        LogHelper.Debug(string.Format("溶氧进入死区，反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                        deviceParameter.IsDOLimit = false;
                                        dicDOPid[deviceParameter.Name].Reset();
                                        dicDODelta[deviceParameter.Name] = 0;
                                        //溶氧已调整完毕，不调整参数
                                        break;
                                    }

                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    index1 += 1;
                                    Thread.Sleep(1000);
                                }

                                RealTimeParam realTimeParam3 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                if (realTimeParam3.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam3.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)//DO调整完毕
                                {
                                    if (agit == -1)
                                    {
                                        agit = realTimeParam3.Agit;
                                    }
                                    else
                                    {
                                        agit = deviceParameter.AgitParam.Agit_PV;
                                    }
                                    LogHelper.Debug(string.Format("溶氧进入死区，反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam3.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                    deviceParameter.IsDOLimit = false;
                                    dicDOPid[deviceParameter.Name].Reset();
                                    dicDODelta[deviceParameter.Name] = 0;
                                    //溶氧已调整完毕，不调整参数
                                    continue;
                                }

                                if (realTimeParam3.DO > deviceParameter.DOParam.DO_PV + info.deadArea)
                                {
                                    if (dicAirIndex[deviceParameter.Name] > 0)//退回上一阶段
                                    {
                                        //dicDODelta[deviceParameter.Name] = 0;
                                        dicAirIndex[deviceParameter.Name] -= 1;
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("DO调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }

                    while (dicO2Index[deviceParameter.Name] < param.O2Col.Count && param.O2Col.Count > 0)
                    {
                        try
                        {
                            param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                e.Result = deviceParameter.Name;
                                return;
                            }

                            RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            string result = File.ReadAllText(FileConst.PidInfoPath);
                            List<PIDInfo> pIDInfos = JsonConvert.DeserializeObject<List<PIDInfo>>(result);
                            if (pIDInfos == null)
                            {
                                MessageBox.Show("PID调控策略列表为空");
                                e.Result = deviceParameter.Name;
                                return;
                            }
                            if (realTime.DO <= deviceParameter.DOParam.DO_PV)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("DO_正向") && t.deviceID == deviceParameter.Name);
                            }
                            else if (realTime.DO >= deviceParameter.DOParam.DO_PV)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("DO_反向") && t.deviceID == deviceParameter.Name);
                            }

                            if (info == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在DO的PID调控策略", deviceParameter.Name));
                                e.Result = deviceParameter.Name;
                                return;
                            }

                            //如果pid类型变了，pid系数清零 方成
                            if (info != null && lastPid != null && info.PidName != lastPid.PidName)
                            {
                                LogHelper.Debug(string.Format("反应器{2} DO调控：由{0}切换至{1}", lastPid.PidName, info.PidName, deviceParameter.Name));
                                if (agit == -1)
                                {
                                    agit = realTime.Agit;
                                }
                                else
                                {
                                    agit = deviceParameter.AgitParam.Agit_PV;
                                }
                                LogHelper.Debug(string.Format("反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTime.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                deviceParameter.IsDOLimit = false;
                                dicDOPid[deviceParameter.Name].Reset();
                                dicDODelta[deviceParameter.Name] = 0;
                            }
                            lastPid = info;

                            if (realTime.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTime.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)
                            {
                                LogHelper.Debug(string.Format("溶氧进入死区，反应器{0}，上次转速为{1}，设置为{2}", deviceParameter.Name, agit, lastAgit, realTime.Agit));
                                if (agit == -1)
                                {
                                    agit = realTime.Agit;
                                }
                                else
                                {
                                    agit = deviceParameter.AgitParam.Agit_PV;
                                }
                                LogHelper.Debug(string.Format("反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTime.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                deviceParameter.IsDOLimit = false;
                                dicDOPid[deviceParameter.Name].Reset();
                                dicDODelta[deviceParameter.Name] = 0;

                                {
                                    int count = info.Interval <= 0 ? 1 : info.Interval;
                                    int index1 = 0;
                                    while (index1 < count)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            e.Result = deviceParameter.Name;
                                            return;
                                        }

                                        index1 += 1;
                                        Thread.Sleep(1000);
                                    }
                                }

                                continue;
                            }

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            while (realTimeParam.DO < deviceParameter.DOParam.DO_PV - info.deadArea && !deviceParameter.DOParam.IsDirect)
                            {
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    e.Result = deviceParameter.Name;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            while (realTimeParam.DO > deviceParameter.DOParam.DO_PV + info.deadArea && !deviceParameter.DOParam.IsReverse)
                            {
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    e.Result = deviceParameter.Name;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }

                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                e.Result = deviceParameter.Name;
                                return;
                            }

                            dicDOPid[deviceParameter.Name].SetParameters(kp: (float)info.P, ki: (float)info.I, kd: (float)info.D, integralThreshold: info.Threshold, interval: info.Interval);
                            dicDOPid[deviceParameter.Name].SetOutputLimits(-Math.Abs(info.maxSpeed), Math.Abs(info.maxSpeed));
                            dicDOPid[deviceParameter.Name].SetIntegralLimits(-2000, 2000);
                            dicDOPid[deviceParameter.Name].SetTarget(deviceParameter.DOParam.DO_PV);

                            LogHelper.Debug(string.Format("反应器{6},DO预设值：{0}，DO当前值：{4}，P：{1}，I：{2}，D：{3},采样时间：{5}", deviceParameter.DOParam.DO_PV, info.P, info.I, info.D, deviceParameter.DO, info.Interval, deviceParameter.Name));

                            RealTimeParam realTimeParam1 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (agit == -1 || sv != deviceParameter.DOParam.DO_PV)
                            {
                                LogHelper.Debug(string.Format("反应器{3},给转速设置底值：{0}，溶氧上次设定值：{1}，当前设定值：{2}", realTimeParam1.Agit, sv, deviceParameter.DOParam.DO_PV, deviceParameter.Name));
                                agit = Convert.ToInt32(realTimeParam1.Agit);
                                sv = deviceParameter.DOParam.DO_PV;
                            }

                            //每次循环都检查当前通气量是否更新 方成
                            param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                            if (param != null)
                            {
                                agitHigh = param.AgitUpperLimit;

                                var doair = param.AirCol[dicAirIndex[deviceParameter.Name]];
                                if (doair != null)
                                {
                                    if (param.Unit == 0)//VVM
                                    {
                                        deviceParameter.AirParam.FlowSpeed = MathF.Round((float)(doair.VVM * (realTimeParam1.JarWeight - 2000) / 1000), 2);
                                    }
                                    else if (param.Unit == 1)//L/min
                                    {
                                        deviceParameter.AirParam.FlowSpeed = doair.VVM;
                                    }

                                    if (realTimeParam1.AirFlowSpeed != deviceParameter.AirParam.FlowSpeed)
                                    {
                                        Thread.Sleep(1000);//等待通气改变
                                    }
                                }

                                var doO2 = param.O2Col[dicO2Index[deviceParameter.Name]];
                                if (doO2 != null)
                                {
                                    if (param.Unit == 0)//VVM
                                    {
                                        deviceParameter.O2Param.FlowSpeed = MathF.Round((float)(doO2.VVM * (realTimeParam1.JarWeight - 2000) / 1000), 2);
                                    }
                                    else if (param.Unit == 1)//L/min
                                    {
                                        deviceParameter.O2Param.FlowSpeed = doO2.VVM;
                                    }

                                    if (realTimeParam1.O2FlowSpeed != deviceParameter.O2Param.FlowSpeed)
                                    {
                                        Thread.Sleep(1000);//等待通气改变
                                    }
                                }
                            }

                            // 位置式PID计算
                            float temp = dicDOPid[deviceParameter.Name].CalculatePositional_DO((float)realTimeParam1.DO);
                            float k = GetDoK((float)(DateTime.Now - startTime).TotalMinutes);
                            LogHelper.Debug(string.Format("反应器{1},DO总Delta:{0},单次Delta:{2},K值：{3},上次转速:{4}", dicDODelta[deviceParameter.Name], deviceParameter.Name, temp, k, agit));
                            temp = temp * k;
                            temp = (float)(temp + agit);//加上次值
                            dicDODelta[deviceParameter.Name] = Convert.ToInt32(temp);//由增量式变更为位置式
                                                                                     //没勾选反向，当PID算出来的值小于上一次的值时，当次值等于上一次值
                            if (!deviceParameter.DOParam.IsReverse && dicDODelta[deviceParameter.Name] < lastAgit)
                            {
                                LogHelper.Debug(string.Format("反应器{2}，没勾选反向，当前值：{0}，上次值：{1}", dicDODelta[deviceParameter.Name], lastAgit, deviceParameter.Name));
                                dicDODelta[deviceParameter.Name] = lastAgit;
                            }
                            //没勾选正向，当PID算出来的值大于上一次的值时，当次值等于上一次值
                            if (!deviceParameter.DOParam.IsDirect && dicDODelta[deviceParameter.Name] > lastAgit)
                            {
                                LogHelper.Debug(string.Format("反应器{2}，没勾选正向，当前值：{0}，上次值：{1}", dicDODelta[deviceParameter.Name], lastAgit, deviceParameter.Name));
                                dicDODelta[deviceParameter.Name] = lastAgit;
                            }

                            if (dicDODelta[deviceParameter.Name] >= param.AgitUpperLimit)
                            {
                                dicDODelta[deviceParameter.Name] = param.AgitUpperLimit;
                            }
                            else if (dicDODelta[deviceParameter.Name] <= param.AgitLowerLimit)
                            {
                                dicDODelta[deviceParameter.Name] = param.AgitLowerLimit;
                            }

                            lastAgit = dicDODelta[deviceParameter.Name];
                            deviceParameter.AgitParam.Agit_PV = dicDODelta[deviceParameter.Name];
                            Thread.Sleep(1000);//等待转速设置下去
                            {
                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                int index1 = 0;
                                while (index1 < count)
                                {
                                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                    if (realTimeParam.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)//DO调整完毕
                                    {
                                        if (agit == -1)
                                        {
                                            agit = realTimeParam.Agit;
                                        }
                                        else
                                        {
                                            agit = deviceParameter.AgitParam.Agit_PV;
                                        }
                                        LogHelper.Debug(string.Format("溶氧进入死区，反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                        deviceParameter.IsDOLimit = false;
                                        dicDOPid[deviceParameter.Name].Reset();
                                        dicDODelta[deviceParameter.Name] = 0;
                                        //溶氧已调整完毕，不调整参数
                                        break;
                                    }

                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    index1 += 1;
                                    Thread.Sleep(1000);
                                }
                            }

                            RealTimeParam realTimeParam2 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (realTimeParam2.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam2.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)//DO调整完毕
                            {
                                if (agit == -1)
                                {
                                    agit = realTimeParam2.Agit;
                                }
                                else
                                {
                                    agit = deviceParameter.AgitParam.Agit_PV;
                                }
                                LogHelper.Debug(string.Format("溶氧进入死区，反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam2.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                deviceParameter.IsDOLimit = false;
                                dicDOPid[deviceParameter.Name].Reset();
                                dicDODelta[deviceParameter.Name] = 0;

                                //溶氧已调整完毕，不调整参数
                                {
                                    int count = info.Interval <= 0 ? 1 : info.Interval;
                                    int index1 = 0;
                                    while (index1 < count)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            e.Result = deviceParameter.Name;
                                            return;
                                        }

                                        index1 += 1;
                                        Thread.Sleep(1000);
                                    }
                                }
                                continue;
                            }

                            if (dicDODelta[deviceParameter.Name] == param.AgitUpperLimit)//达到上限
                            {
                                int count = 60;
                                int index1 = 0;
                                while (index1 < count)
                                {
                                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                    if (realTimeParam.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)//DO调整完毕
                                    {
                                        if (agit == -1)
                                        {
                                            agit = realTimeParam.Agit;
                                        }
                                        else
                                        {
                                            agit = deviceParameter.AgitParam.Agit_PV;
                                        }
                                        LogHelper.Debug(string.Format("溶氧进入死区，反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                        deviceParameter.IsDOLimit = false;
                                        dicDOPid[deviceParameter.Name].Reset();
                                        dicDODelta[deviceParameter.Name] = 0;
                                        //溶氧已调整完毕，不调整参数
                                        break;
                                    }

                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    index1 += 1;
                                    Thread.Sleep(1000);
                                }

                                RealTimeParam realTimeParam3 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                if (realTimeParam3.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam3.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)//DO调整完毕
                                {
                                    if (agit == -1)
                                    {
                                        agit = realTimeParam3.Agit;
                                    }
                                    else
                                    {
                                        agit = deviceParameter.AgitParam.Agit_PV;
                                    }
                                    LogHelper.Debug(string.Format("溶氧进入死区，反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam3.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                    deviceParameter.IsDOLimit = false;
                                    dicDOPid[deviceParameter.Name].Reset();
                                    dicDODelta[deviceParameter.Name] = 0;
                                    //溶氧已调整完毕，不调整参数
                                    continue;
                                }

                                if (realTimeParam3.DO < deviceParameter.DOParam.DO_PV - info.deadArea)
                                {//溶氧低于设定值，需要调整到下一档通气
                                    if (dicO2Index[deviceParameter.Name] < param.O2Col.Count - 1)//存在下一阶段
                                    {
                                        dicO2Index[deviceParameter.Name] += 1;

                                        if (dicAirIndex[deviceParameter.Name] >= 1)
                                        {
                                            dicAirIndex[deviceParameter.Name] -= 1;
                                        }
                                        continue;
                                    }

                                    if (!deviceParameter.TempDOAssociated)
                                    {
                                        if (deviceParameter.FeedDOAssociated)
                                        {
                                            deviceParameter.DORegulationLimit = true;
                                        }
                                    }

                                    deviceParameter.IsDOLimit = true;
                                }
                            }
                            else if (dicDODelta[deviceParameter.Name] == param.AgitLowerLimit)//达到下限
                            {
                                //当到达上下限时，等待60秒
                                int count = 60;
                                int index1 = 0;
                                while (index1 < count)
                                {
                                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                    if (realTimeParam.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)//DO调整完毕
                                    {
                                        if (agit == -1)
                                        {
                                            agit = realTimeParam.Agit;
                                        }
                                        else
                                        {
                                            agit = deviceParameter.AgitParam.Agit_PV;
                                        }
                                        LogHelper.Debug(string.Format("溶氧进入死区，反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                        deviceParameter.IsDOLimit = false;
                                        dicDOPid[deviceParameter.Name].Reset();
                                        dicDODelta[deviceParameter.Name] = 0;
                                        //溶氧已调整完毕，不调整参数
                                        break;
                                    }

                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    index1 += 1;
                                    Thread.Sleep(1000);
                                }

                                RealTimeParam realTimeParam3 = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                if (realTimeParam3.DO >= deviceParameter.DOParam.DO_PV - info.deadArea && realTimeParam3.DO <= deviceParameter.DOParam.DO_PV + info.deadArea)//DO调整完毕
                                {
                                    if (agit == -1)
                                    {
                                        agit = realTimeParam3.Agit;
                                    }
                                    else
                                    {
                                        agit = deviceParameter.AgitParam.Agit_PV;
                                    }
                                    LogHelper.Debug(string.Format("溶氧进入死区，反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam3.Agit, deviceParameter.AgitParam.Agit_PV, agit));

                                    deviceParameter.IsDOLimit = false;
                                    dicDOPid[deviceParameter.Name].Reset();
                                    dicDODelta[deviceParameter.Name] = 0;
                                    //溶氧已调整完毕，不调整参数
                                    continue;
                                }

                                if (realTimeParam3.DO > deviceParameter.DOParam.DO_PV + info.deadArea)
                                {
                                    if (dicAirIndex[deviceParameter.Name] > 0)//退回上一阶段
                                    {
                                        dicAirIndex[deviceParameter.Name] -= 1;

                                        if (dicAirIndex[deviceParameter.Name] < param.AirCol.Count - 1)
                                        {
                                            dicAirIndex[deviceParameter.Name] += 1;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("DO调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                }
            });
            dicDOWorker[currentDeviceParameter.Name].RunWorkerCompleted += ((s, e) =>
            {
                try
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter == null)
                    {
                        LogHelper.Debug(string.Format("DOPID事件完成出错,未找到反应器{0}" + e.Result?.ToString()));
                        return;
                    }
                    deviceParameter.DOParam.IsControling = false;
                    deviceParameter.DORegulationLimit = false;
                    //deviceParameter.AirParam.IsControling = deviceParameter.AgitParam.IsControling = false;

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("详情界面：溶氧pid取消报错" + ex.Message);
                }
            });
            dicDOWorker[currentDeviceParameter.Name].RunWorkerAsync();

            dicDOTempWorker[currentDeviceParameter.Name] = new BackgroundWorker();
            dicDOTempWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;
            dicDOTempWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true;
            dicDOTempWorker[currentDeviceParameter.Name].DoWork += (s, e) =>
            {
                var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                e.Result = deviceParameter.Name;
                while (true)
                {
                    Thread.Sleep(5000);

                    if (dicDOTempWorker[deviceParameter.Name].CancellationPending)
                    {
                        return;
                    }
                    while (deviceParameter.TempDOAssociated)
                    {
                        if (dicDOTempWorker[deviceParameter.Name].CancellationPending)
                        {
                            return;
                        }

                        float initialTemp = deviceParameter.TempParam.Temp_PV;
                        PIDInfo pIDInfo = null;
                        QPIDController pIDController = new QPIDController();

                        while (deviceParameter.IsDOLimit && deviceParameter.TempParam.IsControling)
                        {
                            if (dicDOTempWorker[deviceParameter.Name].CancellationPending)
                            {
                                return;
                            }

                            string result = File.ReadAllText(FileConst.PidInfoPath);
                            List<PIDInfo> pIDInfos = JsonConvert.DeserializeObject<List<PIDInfo>>(result);
                            if (pIDInfos == null)
                            {
                                MessageBox.Show("PID调控策略列表为空");
                                return;
                            }

                            pIDInfo = pIDInfos.FindFirst(t => t.PidName.Contains("降温") && t.deviceID == deviceParameter.Name);
                            if (pIDInfo == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在温控的PID调控策略", deviceParameter.Name));
                                return;
                            }
                            RealTimeParam realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            pIDController.SetParameters(kp: (float)pIDInfo.P, ki: (float)pIDInfo.I, kd: (float)pIDInfo.D, integralThreshold: pIDInfo.Threshold, interval: pIDInfo.Interval);
                            pIDController.SetOutputLimits(-Math.Abs(pIDInfo.maxSpeed), Math.Abs(pIDInfo.maxSpeed));
                            pIDController.SetIntegralLimits(-2000, 2000);
                            pIDController.SetTarget(agitHigh);

                            float pos = pIDController.CalculateIncremental(realTimeParam.Agit);
                            float currentTemp = deviceParameter.TempParam.Temp_PV + pos;
                            currentTemp = currentTemp <= deviceParameter.TempDOLowerLimit ? deviceParameter.TempDOLowerLimit : currentTemp;
                            deviceParameter.TempParam.Temp_PV = MathF.Round(currentTemp, 2);
                            LogHelper.Debug(string.Format("反应器{0} 起始温度{1} 单次delta{2} 实际温度{3}", deviceParameter.Name, initialTemp, pos, currentTemp));


                            int count = pIDInfo.Interval <= 0 ? 1 : pIDInfo.Interval;
                            int index1 = 0;
                            while (index1 < count)
                            {
                                if (dicDOTempWorker[deviceParameter.Name].CancellationPending)
                                {
                                    e.Result = deviceParameter.Name;
                                    return;
                                }

                                index1 += 1;
                                Thread.Sleep(1000);
                            }
                        }
                        pIDController.Reset();
                    }
                }
            };
            dicDOTempWorker[currentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
            {
                var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);

                BackgroundWorker backgroundWorker = s as BackgroundWorker;
                backgroundWorker.Dispose();
                backgroundWorker = null;
            };
            dicDOTempWorker[currentDeviceParameter.Name].RunWorkerAsync();
        });

        public DelegateCommand DOOPCommand => new(() =>
        {
            DialogHostService.ShowOnce(nameof(DOTimeSeriesView), callBack =>
            {

            });
        });

        public DelegateCommand<DeviceParameter> AgitRunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }

            if (dicAgitWorker[currentDeviceParameter.Name] != null && dicAgitWorker[currentDeviceParameter.Name].IsBusy)
            {
                dicAgitWorker[currentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            if (currentDeviceParameter.AgitParam.IsControling)
            {
                try
                {
                    dicAgitWorker[currentDeviceParameter.Name] = new BackgroundWorker();
                    dicAgitWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true;
                    dicAgitWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;
                    dicAgitWorker[currentDeviceParameter.Name].DoWork += (s, e) =>
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(currentDeviceParameter.Name, currentDeviceParameter.AgitParam.Agit_PV);
                            dicAgitSP[currentDeviceParameter.Name] = currentDeviceParameter.AgitParam.Agit_PV;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("设置转速异常" + ex.Message);
                        }

                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        BackgroundWorker worker = s as BackgroundWorker;
                        while (true)
                        {
                            if (worker.CancellationPending)
                            {
                                //e.Cancel = true;
                                return;
                            }
                            try
                            {
                                if (dicAgitSP[deviceParameter.Name] != deviceParameter.AgitParam.Agit_PV)
                                {
                                    InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(deviceParameter.Name, deviceParameter.AgitParam.Agit_PV);
                                    dicAgitSP[deviceParameter.Name] = deviceParameter.AgitParam.Agit_PV;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("转速控制失败" + ex.Message);
                            }
                            Thread.Sleep(1000);
                        }
                    };
                    dicAgitWorker[currentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                    {
                        BackgroundWorker backgroundWorker = s as BackgroundWorker;
                        backgroundWorker.Dispose();
                        backgroundWorker = null;
                    };
                    dicAgitWorker[currentDeviceParameter.Name].RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("转速控制失败" + ex.Message);
                }
            }
            else
            {
                Task.Run(() =>
                {
                    try
                    {
                        InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(currentDeviceParameter.Name, 0);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("转速控制失败" + ex.Message);
                    }
                });
            }
        });

        public DelegateCommand<DeviceParameter> AirRunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }

            if (dicAirWorker[currentDeviceParameter.Name] != null && dicAirWorker[currentDeviceParameter.Name].IsBusy)
            {
                dicAirWorker[currentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            if (currentDeviceParameter.AirParam.IsControling)
            {
                try
                {
                    dicAirWorker[currentDeviceParameter.Name] = new BackgroundWorker();
                    dicAirWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true;
                    dicAirWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;
                    dicAirWorker[currentDeviceParameter.Name].DoWork += (s, e) =>
                    {
                        try
                        {
                            int mfcNo = PumpMFCUtil.GetMFCIndex(currentDeviceParameter.Name, GasType.Air);
                            if (mfcNo >= 0)
                            {
                                GasParam gasParam = new GasParam()
                                {
                                    MFCNo = mfcNo,
                                    GasType = GasType.Air,
                                    FlowSpeed = currentDeviceParameter.AirParam.FlowSpeed
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(currentDeviceParameter.Name, gasParam);
                                dicAirSP[currentDeviceParameter.Name] = currentDeviceParameter.AirParam.FlowSpeed;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("设置空气异常" + ex.Message);
                        }

                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        BackgroundWorker worker = s as BackgroundWorker;
                        while (true)
                        {
                            if (worker.CancellationPending)
                            {
                                //e.Cancel = true;
                                return;
                            }
                            try
                            {
                                if (dicAirSP[deviceParameter.Name] != deviceParameter.AirParam.FlowSpeed)
                                {
                                    int mfcNo = PumpMFCUtil.GetMFCIndex(deviceParameter.Name, GasType.Air);
                                    if (mfcNo >= 0)
                                    {
                                        GasParam gasParam = new GasParam()
                                        {
                                            MFCNo = mfcNo,
                                            GasType = GasType.Air,
                                            FlowSpeed = deviceParameter.AirParam.FlowSpeed
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(deviceParameter.Name, gasParam);
                                        dicAirSP[deviceParameter.Name] = deviceParameter.AirParam.FlowSpeed;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("通气控制失败" + ex.Message);
                            }
                            Thread.Sleep(1000);
                        }
                    };
                    dicAirWorker[currentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                    {
                        BackgroundWorker backgroundWorker = s as BackgroundWorker;
                        backgroundWorker.Dispose();
                        backgroundWorker = null;
                    };
                    dicAirWorker[currentDeviceParameter.Name].RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("通气控制失败" + ex.Message);
                }
            }
            else
            {
                Task.Run(() =>
                {
                    try
                    {
                        int mfcNo = PumpMFCUtil.GetMFCIndex(currentDeviceParameter.Name, GasType.Air);
                        if (mfcNo >= 0)
                        {
                            GasParam gasParam = new GasParam()
                            {
                                MFCNo = mfcNo,
                                GasType = GasType.Air,
                                FlowSpeed = 0
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(currentDeviceParameter.Name, gasParam);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("通气控制失败" + ex.Message);
                    }
                });
            }
        });

        public DelegateCommand<DeviceParameter> O2RunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }

            if (dicO2Worker[currentDeviceParameter.Name] != null && dicO2Worker[currentDeviceParameter.Name].IsBusy)
            {
                dicO2Worker[currentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            if (currentDeviceParameter.O2Param.IsControling)
            {
                try
                {
                    dicO2Worker[currentDeviceParameter.Name] = new BackgroundWorker();
                    dicO2Worker[currentDeviceParameter.Name].WorkerSupportsCancellation = true;
                    dicO2Worker[currentDeviceParameter.Name].WorkerReportsProgress = true;
                    dicO2Worker[currentDeviceParameter.Name].DoWork += (s, e) =>
                    {
                        try
                        {
                            int mfcNo = PumpMFCUtil.GetMFCIndex(currentDeviceParameter.Name, GasType.O2);
                            if (mfcNo >= 0)
                            {
                                GasParam gasParam = new GasParam()
                                {
                                    MFCNo = mfcNo,
                                    GasType = GasType.O2,
                                    FlowSpeed = currentDeviceParameter.O2Param.FlowSpeed
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(currentDeviceParameter.Name, gasParam);
                                dicO2SP[currentDeviceParameter.Name] = currentDeviceParameter.O2Param.FlowSpeed;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("设置氧气异常" + ex.Message);
                        }

                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        BackgroundWorker worker = s as BackgroundWorker;
                        while (true)
                        {
                            if (worker.CancellationPending)
                            {
                                //e.Cancel = true;
                                return;
                            }
                            try
                            {
                                if (dicO2SP[deviceParameter.Name] != deviceParameter.O2Param.FlowSpeed)
                                {
                                    int mfcNo = PumpMFCUtil.GetMFCIndex(deviceParameter.Name, GasType.O2);
                                    if (mfcNo >= 0)
                                    {
                                        GasParam gasParam = new GasParam()
                                        {
                                            MFCNo = mfcNo,
                                            GasType = GasType.O2,
                                            FlowSpeed = deviceParameter.O2Param.FlowSpeed
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(deviceParameter.Name, gasParam);
                                        dicO2SP[deviceParameter.Name] = deviceParameter.O2Param.FlowSpeed;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("氧气控制失败" + ex.Message);
                            }
                            Thread.Sleep(1000);
                        }
                    };
                    dicO2Worker[currentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                    {
                        BackgroundWorker backgroundWorker = s as BackgroundWorker;
                        backgroundWorker.Dispose();
                        backgroundWorker = null;
                    };
                    dicO2Worker[currentDeviceParameter.Name].RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("氧气控制失败" + ex.Message);
                }
            }
            else
            {
                if (dicO2Worker[currentDeviceParameter.Name] != null && dicO2Worker[currentDeviceParameter.Name].IsBusy)
                {
                    dicO2Worker[currentDeviceParameter.Name].CancelAsync();
                }
                Task.Run(() =>
                {
                    try
                    {
                        int mfcNo = PumpMFCUtil.GetMFCIndex(currentDeviceParameter.Name, GasType.O2);
                        if (mfcNo >= 0)
                        {
                            GasParam gasParam = new GasParam()
                            {
                                MFCNo = mfcNo,
                                GasType = GasType.O2,
                                FlowSpeed = 0
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(currentDeviceParameter.Name, gasParam);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("氧气控制失败" + ex.Message);
                    }
                });
            }
        });

        /// <summary>
        /// 加碱
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="pump">单位是L</param>
        /// <param name="BaseAssociated"></param>
        private void AddBase(DeviceParameter deviceParameter, double pump, bool BaseAssociated)
        {
            deviceParameter.AcidParam.Acid_PV = 0;
            deviceParameter.BaseParam.Base_PV = 0;
            PeristalticPumpControlParam param1 = new PeristalticPumpControlParam();

            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.AcidPump);
            if (pumpNo >= 0)
            {
                param1 = new PeristalticPumpControlParam()
                {
                    PumpNo = pumpNo,
                    Pump = PeristalticPump.AcidPump,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = 0,
                    FlowCapacity = 0
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param1);
            }

            param1.Pump = PeristalticPump.BasePump;
            int pumpNo1 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.BasePump);
            if (pumpNo1 >= 0)
            {
                param1.PumpNo = pumpNo1;
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param1);
            }
            if (BaseAssociated)
            {
                if (double.IsPositiveInfinity(pump) || double.IsNegativeInfinity(pump))
                {
                    pump = 0;
                }
                var volume = (float)pump * 1000;
                var hour = 120.0f / 3600.0f;
                var flowRate = MathF.Round(volume / hour, Const.NumericalPrecision);
                flowRate = flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : flowRate;
                deviceParameter.BaseParam.Base_PV = flowRate;
                int pumpNo2 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.BasePump);
                if (pumpNo2 >= 0)
                {
                    PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                    {
                        PumpNo = pumpNo2,
                        Pump = PeristalticPump.BasePump,
                        ControlMode = PumpControlMode.Direct,
                        FlowSpeed = deviceParameter.BaseParam.Base_PV,
                        FlowCapacity = volume
                    };
                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                }
            }
        }

        /// <summary>
        /// 加酸
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="pump"></param>
        /// <param name="BaseAssociated"></param>
        private void AddAcid(DeviceParameter deviceParameter, double pump, bool AcidAssociated)
        {
            deviceParameter.AcidParam.Acid_PV = 0;
            deviceParameter.BaseParam.Base_PV = 0;
            PeristalticPumpControlParam param1 = new PeristalticPumpControlParam();

            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.BasePump);
            if (pumpNo >= 0)
            {
                param1 = new PeristalticPumpControlParam()
                {
                    PumpNo = pumpNo,
                    Pump = PeristalticPump.BasePump,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = 0,
                    FlowCapacity = 0
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param1);
            }
            param1.Pump = PeristalticPump.AcidPump;
            int pumpNo1 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.AcidPump);
            if (pumpNo1 >= 0)
            {
                param1.PumpNo = pumpNo1;
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param1);
            }

            if (AcidAssociated)
            {
                if (double.IsPositiveInfinity(pump) || double.IsNegativeInfinity(pump))
                {
                    pump = 0;
                }
                var volume = (float)pump * 1000;
                var hour = 120.0f / 3600.0f;
                var flowRate = MathF.Round(volume / hour, Const.NumericalPrecision);
                flowRate = flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : flowRate;
                deviceParameter.AcidParam.Acid_PV = flowRate;

                int pumpNo2 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.AcidPump);
                if (pumpNo2 >= 0)
                {
                    PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                    {
                        PumpNo = pumpNo2,
                        Pump = PeristalticPump.AcidPump,
                        ControlMode = PumpControlMode.Direct,
                        FlowSpeed = flowRate,
                        FlowCapacity = volume
                    };
                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                }
            }
        }

        /// <summary>
        /// 判断ph是否稳定
        /// </summary>
        /// <param name="ph1"></param>
        /// <param name="ph2"></param>
        /// <returns></returns>
        private bool phIsSteady(double ph1, double ph2)
        {
            double offset = Math.Abs(ph1 - ph2);
            return offset < 0.2;
        }

        public DelegateCommand<DeviceParameter> PHRunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }
            //如果正在自动调控PH
            if (dicPHWorker[currentDeviceParameter.Name] != null && dicPHWorker[currentDeviceParameter.Name].IsBusy)
            {
                dicPHWorker[currentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            //关闭控制
            if (!currentDeviceParameter.PHParam.IsControling)
            {
                dicPHDelta[currentDeviceParameter.Name] = 0;
                Task.Run(() =>
                {
                    try
                    {
                        currentDeviceParameter.AcidParam.Acid_PV = 0;
                        currentDeviceParameter.AcidParam.IsControling = false;
                        int pumpNo = PumpMFCUtil.GetPumpIndex(currentDeviceParameter.Name, PeristalticPump.AcidPump);
                        if (pumpNo >= 0)
                        {
                            PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                            {
                                PumpNo = pumpNo,
                                Pump = PeristalticPump.AcidPump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = currentDeviceParameter.AcidParam.Acid_PV,
                                FlowCapacity = 0
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(currentDeviceParameter.Name, param);
                        }

                        currentDeviceParameter.BaseParam.Base_PV = 0;
                        currentDeviceParameter.BaseParam.IsControling = false;
                        int pumpNo1 = PumpMFCUtil.GetPumpIndex(currentDeviceParameter.Name, PeristalticPump.BasePump);
                        if (pumpNo1 >= 0)
                        {
                            PeristalticPumpControlParam param1 = new PeristalticPumpControlParam()
                            {
                                PumpNo = pumpNo1,
                                Pump = PeristalticPump.BasePump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = currentDeviceParameter.BaseParam.Base_PV,
                                FlowCapacity = 0
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(currentDeviceParameter.Name, param1);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug("PHPID取消报错" + ex.Message);
                    }
                });
                return;
            }

            dicPHPid[currentDeviceParameter.Name].Reset();
            dicPHDelta[currentDeviceParameter.Name] = 0;


            dicPHWorker[currentDeviceParameter.Name] = new BackgroundWorker();
            dicPHWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;      // 允许报告进度
            dicPHWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true; // 允许取消操作
            // 绑定事件
            dicPHWorker[currentDeviceParameter.Name].DoWork += ((s, e) =>
            {
                var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                e.Result = deviceParameter.Name;
                if (deviceParameter.PHParam.PHControlMode == PHControlMode.PID)
                {
                    //dicPHPid[currentDeviceParameter.Name].SetTarget(currentDeviceParameter.PHParam.PH_PV);//PH的预设值可能会自动控制途中更改 方成
                    deviceParameter.BaseParam.IsControling = deviceParameter.AcidParam.IsControling = true;
                    PIDInfo info = null;
                    PIDInfo lastPid = null;
                    while (true)
                    {
                        try
                        {
                            if (dicPHWorker[deviceParameter.Name].CancellationPending)
                            { // 检查取消请求
                                //e.Cancel = true;
                                e.Result = deviceParameter.Name;
                                return;
                            }

                            string result = File.ReadAllText(FileConst.PidInfoPath);
                            List<PIDInfo> pIDInfos = JsonConvert.DeserializeObject<List<PIDInfo>>(result);
                            if (pIDInfos == null)
                            {
                                MessageBox.Show("PID调控策略列表为空");
                                //e.Cancel = true;
                                e.Result = deviceParameter.Name;
                                return;
                            }

                            RealTimeParam realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);

                            if (realTimeParam.PH >= deviceParameter.PHParam.PH_PV)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("PH_酸") && t.deviceID == deviceParameter.Name);
                            }
                            else if (realTimeParam.PH <= deviceParameter.PHParam.PH_PV)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("PH_碱") && t.deviceID == deviceParameter.Name);
                            }
                            if (info == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在PH的PID调控策略", deviceParameter.Name));
                                //e.Cancel = true;
                                e.Result = deviceParameter.Name;
                                return;
                            }

                            //如果pid类型变了，pid系数清零 方成
                            if (info != null && lastPid != null && info.PidName != lastPid.PidName)
                            {
                                LogHelper.Debug(string.Format("PH调控：由{0}切换至{1}", lastPid.PidName, info.PidName));
                                dicPHPid[deviceParameter.Name].Reset();
                                dicPHDelta[deviceParameter.Name] = 0;
                            }

                            lastPid = info;

                            dicPHPid[deviceParameter.Name].SetParameters(kp: (float)info.P, ki: (float)info.I, kd: (float)info.D, integralThreshold: info.Threshold);
                            dicPHPid[deviceParameter.Name].SetOutputLimits(-Math.Abs(info.maxSpeed), Math.Abs(info.maxSpeed));
                            dicPHPid[deviceParameter.Name].SetIntegralLimits(-20, 20);
                            dicPHPid[deviceParameter.Name].SetTarget(deviceParameter.PHParam.PH_PV);

                            LogHelper.Debug(string.Format("反应器{5},PH预设值：{0}，PH当前值：{4}，P：{1}，I：{2}，D：{3}", deviceParameter.PHParam.PH_PV, info.P, info.I, info.D, realTimeParam.PH, deviceParameter.Name));
                            if (realTimeParam.PH >= deviceParameter.PHParam.PH_PV - info.deadArea && realTimeParam.PH <= deviceParameter.PHParam.PH_PV + info.deadArea)
                            {
                                deviceParameter.AcidParam.Acid_PV = 0;

                                int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.AcidPump);
                                if (pumpNo >= 0)
                                {
                                    PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = PeristalticPump.AcidPump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = deviceParameter.AcidParam.Acid_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                }

                                deviceParameter.BaseParam.Base_PV = 0;
                                int pumpNo1 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.BasePump);
                                if (pumpNo1 >= 0)
                                {
                                    PeristalticPumpControlParam param1 = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo1,
                                        Pump = PeristalticPump.BasePump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = deviceParameter.BaseParam.Base_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param1);
                                }

                                //进入死区后，重置Pid的积分系数和误差数组
                                dicPHPid[deviceParameter.Name].Reset();
                                dicPHDelta[deviceParameter.Name] = 0;

                                //Thread.Sleep(info.Interval * 1000);

                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                int index = 0;
                                while (index < count)
                                {
                                    if (dicPHWorker[deviceParameter.Name].CancellationPending)
                                    { // 检查取消请求
                                        //e.Cancel = true;
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }

                                    index += 1;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }
                            // 增量式使用
                            float temp = dicPHPid[deviceParameter.Name].CalculateIncremental((float)realTimeParam.PH);
                            dicPHDelta[deviceParameter.Name] += temp;
                            LogHelper.Debug(string.Format("{1} PH 总Delta:{0} 单次Delta:{2}", dicPHDelta[deviceParameter.Name], deviceParameter.Name, temp));
                            if (dicPHDelta[deviceParameter.Name] < 0)//酸泵
                            {
                                deviceParameter.BaseParam.Base_PV = 0;
                                int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.BasePump);
                                if (pumpNo >= 0)
                                {
                                    PeristalticPumpControlParam param1 = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = PeristalticPump.BasePump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = deviceParameter.BaseParam.Base_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param1);
                                }

                                if (deviceParameter.PHParam.AcidAssociated)
                                {
                                    deviceParameter.AcidParam.Acid_PV = Math.Abs(dicPHDelta[deviceParameter.Name]) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : Math.Abs(dicPHDelta[deviceParameter.Name]);
                                    int pumpNo1 = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, PeristalticPump.AcidPump);
                                    if (pumpNo1 >= 0)
                                    {
                                        PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo1,
                                            Pump = PeristalticPump.AcidPump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.AcidParam.Acid_PV,
                                            FlowCapacity = 100
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                    }
                                }
                            }
                            else if (dicPHDelta[deviceParameter.Name] > 0)//碱泵
                            {
                                deviceParameter.AcidParam.Acid_PV = 0;
                                int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.AcidPump);
                                if (pumpNo >= 0)
                                {
                                    PeristalticPumpControlParam param1 = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = PeristalticPump.AcidPump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = deviceParameter.AcidParam.Acid_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param1);
                                }

                                if (deviceParameter.PHParam.BaseAssociated)
                                {
                                    int pumpNo1 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.BasePump);
                                    if (pumpNo1 >= 0)
                                    {
                                        deviceParameter.BaseParam.Base_PV = Math.Abs(dicPHDelta[deviceParameter.Name]) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : Math.Abs(dicPHDelta[deviceParameter.Name]);
                                        PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo1,
                                            Pump = PeristalticPump.BasePump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.BaseParam.Base_PV,
                                            FlowCapacity = 100
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                    }
                                }
                            }

                            int count1 = info.Interval <= 0 ? 1 : info.Interval;
                            int index1 = 0;
                            while (index1 < count1)
                            {
                                if (dicPHWorker[deviceParameter.Name].CancellationPending)
                                { // 检查取消请求
                                    //e.Cancel = true;
                                    e.Result = deviceParameter.Name;
                                    return;
                                }

                                index1 += 1;
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("PHPID调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                }
                else if (deviceParameter.PHParam.PHControlMode == PHControlMode.Buffer)
                {
                    try
                    {

                        while (true)
                        {
                            if (dicPHWorker[deviceParameter.Name].CancellationPending)
                            {
                                e.Result = deviceParameter.Name;
                                //e.Cancel = true;
                                return;
                            }

                            pHControlUtils.InitpHinfo();
                            var phInfo = pHControlUtils.phInfo;
                            string deviceId = deviceParameter.Name;
                            double aslope = phInfo.aslope;//酸
                            double bslope = phInfo.bslope;//碱
                            double dead_zone = 0.1;
                            double workpH = phInfo.ph0;//
                            int index = ClockSupervisor.realDatasDic[deviceId].Count - 2;
                            double lastpH = ClockSupervisor.realDatasDic[deviceId][index].PH;
                            index = ClockSupervisor.realDatasDic[deviceId].Count - 1;
                            double currentpH = ClockSupervisor.realDatasDic[deviceId][index].PH;

                            bool steady = phIsSteady(currentpH, lastpH);//判断是否稳定

                            if (steady)//稳定
                            {
                                //double offsetpH = currentpH - workpH;
                                double offsetpHAbs = Math.Abs(currentpH - workpH);

                                if (offsetpHAbs < dead_zone)//是否在死区外，是否满足控制策略如酸调、碱调)
                                {
                                    Thread.Sleep(1000);
                                    lastpH = currentpH;
                                    index = ClockSupervisor.realDatasDic[deviceId].Count - 1;
                                    currentpH = ClockSupervisor.realDatasDic[deviceId][index].PH;//当前ph
                                    continue;
                                }

                                if (offsetpHAbs < 1) //在buffer区，
                                {
                                    double slope = aslope;
                                    if (currentpH < workpH)
                                    {//加碱
                                        slope = aslope;
                                    }
                                    else
                                    {//加酸
                                        slope = bslope;
                                    }
                                    double newAslope = slope;
                                    //double newAslope = slope;//
                                    bool needControl = offsetpHAbs > dead_zone;
                                    if (needControl)//在死区外
                                    {
                                        double lastph = currentpH;
                                        //1 打入改变0.1pH(根据上述slope计算)的试剂量
                                        double pump = 0.1 / slope;//单位L-打入
                                        if (currentpH < workpH)// 判断加酸还是加碱
                                        {
                                            //加碱
                                            pump = 0.1 / bslope;//单位L-打入// 打入改变0.1pH(根据上述slope计算)的试剂量
                                            LogHelper.Debug(string.Format("bslope:{0};pump:{1}", bslope, pump));

                                            AddBase(deviceParameter, pump, deviceParameter.PHParam.BaseAssociated);
                                        }
                                        else//加酸
                                        {
                                            pump = 0.1 / aslope;//单位L-打入// 打入改变0.1pH(根据上述slope计算)的试剂量

                                            LogHelper.Debug(string.Format("aslope:{0};pump:{1}", aslope, pump));
                                            AddAcid(deviceParameter, pump, deviceParameter.PHParam.AcidAssociated);
                                        }

                                        while (true)
                                        {
                                            int index1 = 0;
                                            int time = (int)(60 * phInfo.T90);//等待3分钟，判断是否不在死区内，
                                            while (time > 0)
                                            {
                                                if (dicPHWorker[deviceParameter.Name].CancellationPending)
                                                { // 检查取消请求
                                                    e.Result = deviceParameter.Name;
                                                    //e.Cancel = true;
                                                    return;
                                                }

                                                Thread.Sleep(1000);
                                                lastpH = currentpH;
                                                index = ClockSupervisor.realDatasDic[deviceId].Count - 1;
                                                currentpH = ClockSupervisor.realDatasDic[deviceId][index].PH;//当前ph
                                                time--;
                                                index1 += 1;
                                                if (index1 >= 120)
                                                {
                                                    deviceParameter.BaseParam.Base_PV = 0;
                                                    deviceParameter.AcidParam.Acid_PV = 0;
                                                }
                                            }
                                            double offset = currentpH - workpH;
                                            if (Math.Abs(offset) > dead_zone)//如不在死区内，修正slope=上述slope*实际pH改变/0.1
                                            {
                                                newAslope = newAslope * Math.Abs(lastph - currentpH) / 0.1;
                                            }
                                            else//在死区内，跳出
                                            {
                                                break;
                                            }

                                            offsetpHAbs = currentpH - workpH;
                                            pump = 0.5 * offsetpHAbs / newAslope; //单位L - 打入
                                            if (currentpH < workpH)// 判断加酸还是加碱
                                            {
                                                LogHelper.Debug(string.Format("newAslope:{0};pump:{1}", newAslope, pump));
                                                //加碱
                                                AddBase(deviceParameter, pump, deviceParameter.PHParam.BaseAssociated);
                                            }
                                            else//加酸
                                            {
                                                LogHelper.Debug(string.Format("newAslope:{0};pump:{1}", newAslope, pump));
                                                AddAcid(deviceParameter, pump, deviceParameter.PHParam.AcidAssociated);
                                            }
                                            lastph = currentpH;
                                        }
                                    }
                                }
                                else//不在buffer区
                                {
                                    while (true)
                                    {
                                        double pump = 0.1 / aslope;
                                        if (currentpH < workpH)// 判断加酸还是加碱
                                        {
                                            //加碱
                                            pump = 0.1 / bslope;//单位L-打入// 打入改变0.1pH(根据上述slope计算)的试剂量

                                            LogHelper.Debug(string.Format("aslope:{0};pump:{1}", aslope, pump));

                                            AddBase(deviceParameter, pump, deviceParameter.PHParam.BaseAssociated);
                                        }
                                        else//加酸
                                        {
                                            pump = 0.1 / aslope;//单位L-打入// 打入改变0.1pH(根据上述slope计算)的试剂量

                                            LogHelper.Debug(string.Format("aslope:{0};pump:{1}", aslope, pump));

                                            AddAcid(deviceParameter, pump, deviceParameter.PHParam.AcidAssociated);
                                        }

                                        //等待3分钟
                                        int time = (int)(60 * phInfo.T90);
                                        int index1 = 0;
                                        while (time > 0)
                                        {
                                            if (dicPHWorker[deviceParameter.Name].CancellationPending)
                                            { // 检查取消请求
                                                e.Result = deviceParameter.Name;
                                                //e.Cancel = true;
                                                return;
                                            }

                                            Thread.Sleep(1000);
                                            lastpH = currentpH;
                                            index = ClockSupervisor.realDatasDic[deviceId].Count - 1;
                                            currentpH = ClockSupervisor.realDatasDic[deviceId][index].PH;
                                            time--;

                                            index1 += 1;
                                            if (index1 >= 120)
                                            {
                                                deviceParameter.BaseParam.Base_PV = 0;
                                                deviceParameter.AcidParam.Acid_PV = 0;
                                            }
                                        }
                                        if (Math.Abs(currentpH - workpH) < 1)
                                        {
                                            break;
                                        }

                                    }
                                }
                            }
                            else
                            {
                                int count = 0;
                                while (!steady)
                                {
                                    int time = (int)(60 * phInfo.T90);
                                    int index1 = 0;
                                    while (time > 0)//等一分钟
                                    {
                                        if (dicPHWorker[deviceParameter.Name].CancellationPending)
                                        { // 检查取消请求
                                            //e.Cancel = true;
                                            e.Result = deviceParameter.Name;
                                            return;
                                        }

                                        Thread.Sleep(1000);
                                        lastpH = currentpH;
                                        index = ClockSupervisor.realDatasDic[deviceId].Count - 1;
                                        currentpH = ClockSupervisor.realDatasDic[deviceId][index].PH;
                                        time--;

                                        index1 += 1;
                                        if (index1 >= 120)
                                        {
                                            deviceParameter.BaseParam.Base_PV = 0;
                                            deviceParameter.AcidParam.Acid_PV = 0;
                                        }
                                    }
                                    steady = phIsSteady(lastpH, currentpH);//判断是否稳定
                                    lastpH = currentpH;
                                    index = ClockSupervisor.realDatasDic[deviceId].Count - 1;
                                    currentpH = ClockSupervisor.realDatasDic[deviceId][index].PH;
                                    count++;
                                    if (count > 5)//提示报警
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if (deviceParameter.PHParam.PHControlMode == PHControlMode.Adaptive)
                {
                    PeristalticPumpControlParam param = new PeristalticPumpControlParam();

                    dicPHController[deviceParameter.Name] = new IntelligentPHController()
                    {
                        TargetPH = deviceParameter.PHParam.PH_PV
                    };

                    while (true)
                    {
                        if (dicPHWorker[deviceParameter.Name].CancellationPending)
                        {
                            return;
                        }
                        dicPHController[deviceParameter.Name].TargetPH = deviceParameter.PHParam.PH_PV;
                        PropertyMapper.Map(deviceParameter.AdaptivepHParameter, dicPHController[deviceParameter.Name]);

                        var realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                        var (isAlkali, volume) = dicPHController[deviceParameter.Name].CalculateDosing(currentPH: realTimeParam.PH, currentRPM: realTimeParam.Agit, currentVolume_L: realTimeParam.JarWeight / 1000);
                        if (volume > 0)
                        {
                            if (isAlkali)//加碱
                            {
                                deviceParameter.AcidParam.Acid_PV = 0;
                                int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.AcidPump);
                                if (pumpNo >= 0)
                                {
                                    param = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = PeristalticPump.AcidPump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = deviceParameter.AcidParam.Acid_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                }

                                if (deviceParameter.PHParam.BaseAssociated)
                                {
                                    pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.BasePump);
                                    if (pumpNo >= 0)
                                    {
                                        deviceParameter.BaseParam.Base_PV = AppSession.DefaultPumpFlowRate;
                                        param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = PeristalticPump.BasePump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.BaseParam.Base_PV,
                                            FlowCapacity = (float)volume
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                        LogHelper.Debug(string.Format("反应器{0},PH预设值：{1}，PH当前值：{2}，体积：{3}", deviceParameter.Name, deviceParameter.PHParam.PH_PV, realTimeParam.PH, volume));

                                        int count = Convert.ToInt32(Math.Ceiling(volume * 3600 / deviceParameter.BaseParam.Base_PV));
                                        while (count > 0 && !InstrumentSolution.GetInstance().IsSimulation)
                                        {
                                            if (dicPHWorker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                            count--;
                                        }
                                    }
                                }
                            }
                            else//加酸
                            {
                                deviceParameter.BaseParam.Base_PV = 0;
                                int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.BasePump);
                                if (pumpNo >= 0)
                                {
                                    param = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = PeristalticPump.BasePump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = deviceParameter.BaseParam.Base_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                }

                                if (deviceParameter.PHParam.AcidAssociated)
                                {
                                    deviceParameter.AcidParam.Acid_PV = AppSession.DefaultPumpFlowRate;
                                    pumpNo = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, PeristalticPump.AcidPump);
                                    if (pumpNo >= 0)
                                    {
                                        param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = PeristalticPump.AcidPump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.AcidParam.Acid_PV,
                                            FlowCapacity = (float)volume
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                        LogHelper.Debug(string.Format("反应器{0},PH预设值：{1}，PH当前值：{2}，体积：{3}", deviceParameter.Name, deviceParameter.PHParam.PH_PV, realTimeParam.PH, volume));

                                        int count = Convert.ToInt32(Math.Ceiling(volume * 3600 / deviceParameter.AcidParam.Acid_PV));
                                        while (count > 0 && !InstrumentSolution.GetInstance().IsSimulation)
                                        {
                                            if (dicPHWorker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                            count--;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            deviceParameter.AcidParam.Acid_PV = 0;
                            deviceParameter.AcidParam.IsControling = false;
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.AcidPump);
                            if (pumpNo >= 0)
                            {
                                param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = PeristalticPump.AcidPump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = deviceParameter.AcidParam.Acid_PV,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                            }

                            deviceParameter.BaseParam.Base_PV = 0;
                            deviceParameter.BaseParam.IsControling = false;
                            pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.BasePump);
                            if (pumpNo >= 0)
                            {
                                param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = PeristalticPump.BasePump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = deviceParameter.BaseParam.Base_PV,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                            }

                            Thread.Sleep(1000);
                        }
                    }
                }
            });
            dicPHWorker[currentDeviceParameter.Name].RunWorkerCompleted += ((s, e) =>
            {
                try
                {
                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;

                    //var dictionary = dicPHWorker.ToDictionary(x => x.Value, x => x.Key);
                    //dictionary.TryGetValue(backgroundWorker, out string deviceName);
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter == null)
                    {
                        LogHelper.Debug(string.Format("PHPID事件完成出错,未找到反应器{0}" + e.Result?.ToString()));
                        return;
                    }

                    deviceParameter.BaseParam.Base_PV = 0;
                    deviceParameter.BaseParam.IsControling = false;

                    deviceParameter.AcidParam.Acid_PV = 0;
                    deviceParameter.AcidParam.IsControling = false;
                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.BasePump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param1 = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = PeristalticPump.BasePump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = deviceParameter.BaseParam.Base_PV,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param1);
                            }

                            int pumpNo1 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.AcidPump);
                            if (pumpNo1 >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo1,
                                    Pump = PeristalticPump.AcidPump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = deviceParameter.AcidParam.Acid_PV,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("PHPID事件完成,是否取消：{0},错误：{1}，信息：{2}" + e.Cancelled, e.Error, ex.Message));
                        }
                    });

                }
                catch (Exception ex)
                {
                    LogHelper.Debug("PHPID完成事件报错" + ex.Message);
                }

            });
            dicPHWorker[currentDeviceParameter.Name].RunWorkerAsync();
        });

        public DelegateCommand<DeviceParameter> AcidRunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }

            if (dicAcidWorker[currentDeviceParameter.Name] != null && dicAcidWorker[currentDeviceParameter.Name].IsBusy)
            {
                dicAcidWorker[currentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            if (currentDeviceParameter.AcidParam.IsControling)
            {
                try
                {
                    dicAcidWorker[currentDeviceParameter.Name] = new BackgroundWorker();
                    dicAcidWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true;
                    dicAcidWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;
                    dicAcidWorker[currentDeviceParameter.Name].DoWork += (s, e) =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(currentDeviceParameter.Name, PeristalticPump.AcidPump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = PeristalticPump.AcidPump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = (float)currentDeviceParameter.AcidParam.Acid_PV >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)currentDeviceParameter.AcidParam.Acid_PV,
                                    FlowCapacity = 100
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(currentDeviceParameter.Name, param);
                                dicAcidSP[currentDeviceParameter.Name] = currentDeviceParameter.AcidParam.Acid_PV;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("设置酸泵异常" + ex.Message);
                        }
                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        BackgroundWorker worker = s as BackgroundWorker;
                        while (true)
                        {
                            if (worker.CancellationPending)
                            {
                                //e.Cancel = true;
                                return;
                            }
                            try
                            {
                                if (dicAcidSP[deviceParameter.Name] != deviceParameter.AcidParam.Acid_PV)
                                {
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.AcidPump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = PeristalticPump.AcidPump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.AcidParam.Acid_PV >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)deviceParameter.AcidParam.Acid_PV,
                                            FlowCapacity = 100
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                        dicAcidSP[deviceParameter.Name] = deviceParameter.AcidParam.Acid_PV;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("酸泵控制失败" + ex.Message);
                            }
                            Thread.Sleep(1000);
                        }
                    };
                    dicAcidWorker[currentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                    {
                        BackgroundWorker backgroundWorker = s as BackgroundWorker;
                        backgroundWorker.Dispose();
                        backgroundWorker = null;
                    };
                    dicAcidWorker[currentDeviceParameter.Name].RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("酸泵控制失败" + ex.Message);
                }
            }
            else
            {
                Task.Run(() =>
                {
                    try
                    {
                        int pumpNo = PumpMFCUtil.GetPumpIndex(currentDeviceParameter.Name, PeristalticPump.AcidPump);
                        if (pumpNo >= 0)
                        {
                            PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                            {
                                PumpNo = pumpNo,
                                Pump = PeristalticPump.AcidPump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = 0,
                                FlowCapacity = 0
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(currentDeviceParameter.Name, param);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("酸泵控制失败" + ex.Message);
                    }
                });
            }
        });

        public DelegateCommand<DeviceParameter> BaseRunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }

            if (dicBaseWorker[currentDeviceParameter.Name] != null && dicBaseWorker[currentDeviceParameter.Name].IsBusy)
            {
                dicBaseWorker[currentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            if (currentDeviceParameter.BaseParam.IsControling)
            {
                try
                {
                    int pumpNo = PumpMFCUtil.GetPumpIndex(currentDeviceParameter.Name, PeristalticPump.BasePump);
                    if (pumpNo >= 0)
                    {
                        PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                        {
                            PumpNo = pumpNo,
                            Pump = PeristalticPump.BasePump,
                            ControlMode = PumpControlMode.Direct,
                            FlowSpeed = (float)currentDeviceParameter.BaseParam.Base_PV >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)currentDeviceParameter.BaseParam.Base_PV,
                            FlowCapacity = 100
                        };
                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(currentDeviceParameter.Name, param);
                        dicBaseSP[currentDeviceParameter.Name] = currentDeviceParameter.BaseParam.Base_PV;
                    }

                    dicBaseWorker[currentDeviceParameter.Name] = new BackgroundWorker();
                    dicBaseWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true;
                    dicBaseWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;
                    dicBaseWorker[currentDeviceParameter.Name].DoWork += (s, e) =>
                    {
                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        BackgroundWorker worker = s as BackgroundWorker;
                        while (true)
                        {
                            if (worker.CancellationPending)
                            {
                                //e.Cancel = true;
                                return;
                            }
                            try
                            {
                                if (dicBaseSP[deviceParameter.Name] != deviceParameter.BaseParam.Base_PV)
                                {
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.BasePump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = PeristalticPump.BasePump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.BaseParam.Base_PV >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)deviceParameter.BaseParam.Base_PV,
                                            FlowCapacity = 100
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                        dicBaseSP[deviceParameter.Name] = deviceParameter.BaseParam.Base_PV;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("碱泵控制失败" + ex.Message);
                            }
                            Thread.Sleep(1000);
                        }
                    };
                    dicBaseWorker[currentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                    {
                        BackgroundWorker backgroundWorker = s as BackgroundWorker;
                        backgroundWorker.Dispose();
                        backgroundWorker = null;
                    };
                    dicBaseWorker[currentDeviceParameter.Name].RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("碱泵控制失败" + ex.Message);
                }
            }
            else
            {
                Task.Run(() =>
                {
                    try
                    {
                        int pumpNo = PumpMFCUtil.GetPumpIndex(currentDeviceParameter.Name, PeristalticPump.BasePump);
                        if (pumpNo >= 0)
                        {
                            PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                            {
                                PumpNo = pumpNo,
                                Pump = PeristalticPump.BasePump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = 0,
                                FlowCapacity = 0
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(currentDeviceParameter.Name, param);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("碱泵控制失败" + ex.Message);
                    }
                });

            }
        });

        public DelegateCommand AdaptpHCommand => new(() => 
        {
            DialogParameters keyValuePairs = new DialogParameters() { { nameof(AdaptivepHParameter), CurrentDeviceParameter.AdaptivepHParameter } };
            DialogHostService.Show(nameof(AdaptpHView), keyValuePairs, callback => 
            {

            });
        });

        public DelegateCommand<DeviceParameter> TempRunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }
            if (dicTempWorker[currentDeviceParameter.Name] != null && dicTempWorker[currentDeviceParameter.Name].IsBusy)
            {
                dicTempWorker[currentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            if (currentDeviceParameter.TempParam.IsControling)
            {
                try
                {
                    dicTempWorker[currentDeviceParameter.Name] = new BackgroundWorker();
                    dicTempWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true;
                    dicTempWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;
                    dicTempWorker[currentDeviceParameter.Name].DoWork += (s, e) =>
                    {
                        LogHelper.Debug($"反应器{currentDeviceParameter.Name}开始温控");
                        try
                        {
                            currentDeviceParameter.TempParam.IsEnable = true;
                            //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(currentDeviceParameter.Name, ControlObject.Temperature, SwitchMode.Open);
                            InstrumentSolution.GetInstance().CommandWrapper.SetTempSetting(currentDeviceParameter.Name, currentDeviceParameter.TempParam);
                            dicTempSP[currentDeviceParameter.Name] = currentDeviceParameter.TempParam.Temp_PV;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("温控异常" + ex.Message);
                        }
                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        BackgroundWorker worker = s as BackgroundWorker;
                        while (true)
                        {
                            if (worker.CancellationPending)
                            {
                                e.Result = deviceParameter.Name;
                                return;
                            }
                            try
                            {
                                if (dicTempSP[deviceParameter.Name] != deviceParameter.TempParam.Temp_PV)
                                {
                                    deviceParameter.TempParam.IsEnable = true;
                                    InstrumentSolution.GetInstance().CommandWrapper.SetTempSetting(deviceParameter.Name, deviceParameter.TempParam);
                                    dicTempSP[deviceParameter.Name] = deviceParameter.TempParam.Temp_PV;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("温度控制失败" + ex.Message);
                            }
                            Thread.Sleep(1000);
                        }
                    };
                    dicTempWorker[currentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                    {
                        BackgroundWorker backgroundWorker = s as BackgroundWorker;
                        backgroundWorker.Dispose();
                        backgroundWorker = null;

                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                        deviceParameter.TempParam.IsEnable = false;
                        InstrumentSolution.GetInstance().CommandWrapper.SetTempSetting(deviceParameter.Name, deviceParameter.TempParam);
                    };
                    dicTempWorker[currentDeviceParameter.Name].RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("温度控制失败" + ex.Message);
                }
            }
            else
            {
                Task.Run(() =>
                {
                    try
                    {
                        currentDeviceParameter.TempParam.IsEnable = false;
                        InstrumentSolution.GetInstance().CommandWrapper.SetTempSetting(currentDeviceParameter.Name, currentDeviceParameter.TempParam);
                    }
                    catch (Exception ex) { }
                });
            }
        });

        public DelegateCommand Feed1RunCommand => new(() =>
        {
            PeristalticPump pump = PeristalticPump.FeedPump;

            if (dicFeed1Worker[CurrentDeviceParameter.Name] != null && dicFeed1Worker[CurrentDeviceParameter.Name].IsBusy)
            {
                dicFeed1Worker[CurrentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            if (dicFeed1Probe[CurrentDeviceParameter.Name] != null)
            {
                dicFeed1Probe[CurrentDeviceParameter.Name].StopCtrl();
                Thread.Sleep(100);
            }

            //关闭控制
            if (!CurrentDeviceParameter.FeedParam1.IsControling)
            {
                Task.Run(() =>
                {
                    try
                    {
                        int pumpNo = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, pump);
                        if (pumpNo >= 0)
                        {
                            PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                            {
                                PumpNo = pumpNo,
                                Pump = pump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = 0,
                                FlowCapacity = 0
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(CurrentDeviceParameter.Name, param);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug("详情界面：补料1取消报错" + ex.Message);
                    }
                });
                return;
            }

            var feedMode = CurrentDeviceParameter.FeedParam1.FeedMode;
            //恒速
            if (feedMode == FeedControlMode.ConstantSpeed)
            {
                dicFeed1Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                    FeedStrategyInfo f = null;
                    PeristalticPumpControlParam param = new PeristalticPumpControlParam();
                    if (allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos) && feedGradientInfos != null)
                    {
                        f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "Constant".ToUpper() && t.Pump == pump);
                        if (f != null && f.A > 0)
                        {
                            CurrentDeviceParameter.FeedParam1.Feed_PV = f.A;
                        }
                    }

                    int pumpNo = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, pump);
                    if (pumpNo >= 0)
                    {
                        param = new PeristalticPumpControlParam()
                        {
                            PumpNo = pumpNo,
                            Pump = pump,
                            ControlMode = PumpControlMode.Direct,
                            FlowSpeed = (float)CurrentDeviceParameter.FeedParam1.Feed_PV >= AppSession.DefaultPumpFlowRate ? AppSession.DefaultPumpFlowRate : (float)CurrentDeviceParameter.FeedParam1.Feed_PV,
                            FlowCapacity = Const.MaxPumpFlowCapacity
                        };
                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(CurrentDeviceParameter.Name, param);
                        dicFeed1SP[CurrentDeviceParameter.Name] = CurrentDeviceParameter.FeedParam1.Feed_PV;
                    }

                    double totalSecond = 0;
                    while (true)
                    {
                        try
                        {
                            var worker = (BackgroundWorker)s;
                            if (worker.CancellationPending)
                            {
                                return;
                            }
                            f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "Constant".ToUpper() && t.Pump == pump);

                            if (dicFeed1SP[deviceParameter.Name] != deviceParameter.FeedParam1.Feed_PV)
                            {
                                pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                if (pumpNo >= 0)
                                {
                                    param = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = dicFeed1SP[CurrentDeviceParameter.Name] >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : dicFeed1SP[CurrentDeviceParameter.Name],
                                        FlowCapacity = Const.MaxPumpFlowCapacity
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                }

                                dicFeed1SP[CurrentDeviceParameter.Name] = deviceParameter.FeedParam1.Feed_PV;
                            }

                            Thread.Sleep(1000);

                            if (f.StatDisable)
                            {
                                totalSecond = 0;
                            }
                            else
                            {
                                totalSecond += 1;
                            }

                            if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                            {
                                pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                if (pumpNo >= 0)
                                {
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                    param = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = deviceParameter.FeedParam1.Feed_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                }

                                if (f.CurveDOStat)
                                {
                                    while (true)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (f.StatSymbol == 0)//小于
                                        {
                                            if (realTime.DO < f.StatValue)
                                            {
                                                pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                                if (pumpNo >= 0)
                                                {
                                                    deviceParameter.FeedParam1.Feed_PV = dicFeed1SP[CurrentDeviceParameter.Name];
                                                    param = new PeristalticPumpControlParam()
                                                    {
                                                        PumpNo = pumpNo,
                                                        Pump = pump,
                                                        ControlMode = PumpControlMode.Direct,
                                                        FlowSpeed = dicFeed1SP[CurrentDeviceParameter.Name] >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : dicFeed1SP[CurrentDeviceParameter.Name],
                                                        FlowCapacity = Const.MaxPumpFlowCapacity
                                                    };
                                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                                }
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        else if (f.StatSymbol == 1)//大于
                                        {
                                            if (realTime.DO > f.StatValue)
                                            {
                                                pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                                if (pumpNo >= 0)
                                                {
                                                    deviceParameter.FeedParam1.Feed_PV = dicFeed1SP[CurrentDeviceParameter.Name];
                                                    param = new PeristalticPumpControlParam()
                                                    {
                                                        PumpNo = pumpNo,
                                                        Pump = pump,
                                                        ControlMode = PumpControlMode.Direct,
                                                        FlowSpeed = dicFeed1SP[CurrentDeviceParameter.Name] >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : dicFeed1SP[CurrentDeviceParameter.Name],
                                                        FlowCapacity = Const.MaxPumpFlowCapacity
                                                    };
                                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                                }
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                    }
                                }
                                else if (f.CurvepHStat)
                                {
                                    while (true)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (f.StatSymbol == 0)//小于
                                        {
                                            if (realTime.PH < f.StatValue)
                                            {
                                                pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                                if (pumpNo >= 0)
                                                {
                                                    deviceParameter.FeedParam1.Feed_PV = dicFeed1SP[CurrentDeviceParameter.Name];
                                                    param = new PeristalticPumpControlParam()
                                                    {
                                                        PumpNo = pumpNo,
                                                        Pump = pump,
                                                        ControlMode = PumpControlMode.Direct,
                                                        FlowSpeed = dicFeed1SP[CurrentDeviceParameter.Name] >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : dicFeed1SP[CurrentDeviceParameter.Name],
                                                        FlowCapacity = Const.MaxPumpFlowCapacity
                                                    };
                                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                                }
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        else if (f.StatSymbol == 1)//大于
                                        {
                                            if (realTime.PH > f.StatValue)
                                            {
                                                pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                                if (pumpNo >= 0)
                                                {
                                                    deviceParameter.FeedParam1.Feed_PV = dicFeed1SP[CurrentDeviceParameter.Name];
                                                    param = new PeristalticPumpControlParam()
                                                    {
                                                        PumpNo = pumpNo,
                                                        Pump = pump,
                                                        ControlMode = PumpControlMode.Direct,
                                                        FlowSpeed = dicFeed1SP[CurrentDeviceParameter.Name] >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : dicFeed1SP[CurrentDeviceParameter.Name],
                                                        FlowCapacity = Const.MaxPumpFlowCapacity
                                                    };
                                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                                }
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料常数调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam1.IsControling = false;
                        //如果是常量，补料预设值不重置
                        //deviceParameter.FeedParam1.Feed_PV = 0;  
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料指数完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //多项式
            else if (feedMode == FeedControlMode.Polynomial)
            {
                dicFeed1Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    PeristalticPumpControlParam param = new PeristalticPumpControlParam();
                    var worker = (BackgroundWorker)s;

                    double totalSecond = 0;
                    double secondCount = 0;
                    while (true)
                    {
                        try
                        {

                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            bool flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }

                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "Polynomial".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在多项式策略", deviceParameter.Name));
                                return;
                            }

                            double interval = secondCount / 3600;
                            double a = f.A;//20
                            double b = double.Parse(f.B);//0.7
                            double c = f.C;//40
                            double deltaT = double.Parse(f.D);
                            double diff = interval - deltaT > 0 ? interval - deltaT : 0;
                            double feed = Math.Round(a * Math.Pow(diff, 2) + b * diff + c, 2);

                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            deviceParameter.FeedParam1.Feed_PV = (float)feed >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)feed;
                            if (pumpNo >= 0)
                            {
                                param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                            }

                            int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                            while (count > 0)
                            {
                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                {
                                    return;
                                }

                                count--;
                                Thread.Sleep(1000);
                            }

                            secondCount += 1;

                            if (f.StatDisable)
                            {
                                totalSecond = 0;
                            }
                            else
                            {
                                totalSecond += 1;
                            }

                            if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                            {
                                if (f.CurveDOStat || f.CurvepHStat)
                                {
                                    pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                        param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = 0
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                    }
                                }

                                if (f.CurveDOStat)
                                {
                                    while (true)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (f.StatSymbol == 0)//小于
                                        {
                                            if (realTime.DO < f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        else if (f.StatSymbol == 1)//大于
                                        {
                                            if (realTime.DO > f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                    }
                                }
                                else if (f.CurvepHStat)
                                {
                                    int pauseSecond = 0;

                                    while (true)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (f.StatSymbol == 0)//小于
                                        {
                                            if (realTime.PH < f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        else if (f.StatSymbol == 1)//大于
                                        {
                                            if (realTime.PH > f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                        pauseSecond += 1;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料多项式调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam1.IsControling = false;
                        deviceParameter.FeedParam1.Feed_PV = 0;
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料指数完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //指数
            else if (feedMode == FeedControlMode.Exponential)
            {
                dicFeed1Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var worker = (BackgroundWorker)s;
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    PeristalticPumpControlParam param = new PeristalticPumpControlParam();
                    double totalSecond = 0;
                    double secondCount = 0;
                    while (true)
                    {
                        try
                        {
                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            bool flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }
                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "Exponential".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在指数策略", deviceParameter.Name));
                                return;
                            }

                            double interval = secondCount / 3600;
                            double f1 = f.A;//20
                            double μ = double.Parse(f.B);//0.7
                            double deltaT = f.C;//Δt
                            double diff = interval - deltaT > 0 ? interval - deltaT : 0;
                            double feed = Math.Round(f1 * Math.Exp(μ * (diff)), 2);

                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                deviceParameter.FeedParam1.Feed_PV = (float)feed >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)feed;
                                param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                            }

                            int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                            while (count > 0)
                            {
                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                {
                                    return;
                                }

                                count--;
                                Thread.Sleep(1000);
                            }
                            secondCount += 1;

                            if (f.StatDisable)
                            {
                                totalSecond = 0;
                            }
                            else
                            {
                                totalSecond += 1;
                            }

                            if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                            {
                                if (f.CurveDOStat || f.CurvepHStat)
                                {
                                    pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                        param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = 0
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                    }
                                }

                                if (f.CurveDOStat)
                                {
                                    while (true)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (f.StatSymbol == 0)//小于
                                        {
                                            if (realTime.DO < f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        else if (f.StatSymbol == 1)//大于
                                        {
                                            if (realTime.DO > f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                    }
                                }
                                else if (f.CurvepHStat)
                                {
                                    int pauseSecond = 0;

                                    while (true)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (f.StatSymbol == 0)//小于
                                        {
                                            if (realTime.PH < f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        else if (f.StatSymbol == 1)//大于
                                        {
                                            if (realTime.PH > f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                        pauseSecond += 1;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料指数调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam1.IsControling = false;
                        deviceParameter.FeedParam1.Feed_PV = 0;
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料指数完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //时间序列
            else if (feedMode == FeedControlMode.TimeSeries)
            {
                dicFeed1Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    BackgroundWorker worker = s as BackgroundWorker;

                    bool QuantitativeFinish = false;//指示时间序列中定量补是否已经完成

                    var allDeviceInfos = FeedGradientManager.GetInstance().FeedGradientCol;
                    bool flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                    if (!flag || feedGradientInfos == null)
                    {
                        MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                        return;
                    }
                    DateTime beginTime;
                    beginTime = DateTime.Now;//开始时间
                    double endTime = feedGradientInfos[feedGradientInfos.Count - 1].EndTime;
                    double timeOffset = Math.Round((DateTime.Now - beginTime).TotalMinutes, 2);//时间差

                    double totalSecond = 0;//用于stat的停顿计时

                    double statTotalSeconds = 0;//用于stat多项式|指数的时间计算

                    double calcTotalSeconds = 0;

                    string lastInfoType = "";

                    while (timeOffset < endTime)
                    {
                        if (worker.CancellationPending)
                        {
                            return;
                        }

                        allDeviceInfos = FeedGradientManager.GetInstance().FeedGradientCol;
                        flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out feedGradientInfos);
                        if (!flag || feedGradientInfos == null)
                        {
                            MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                            return;
                        }

                        //找到当前时间点需要执行哪段
                        //var f = feedGradientInfos.FindFirst(c => c.BeginTime <= timeOffset && c.EndTime > timeOffset);
                        FeedGradientInfo f = null;
                        PeristalticPumpControlParam param = new PeristalticPumpControlParam();

                        foreach (var feedGradientInfo in feedGradientInfos)
                        {
                            if (feedGradientInfo.BeginTime <= timeOffset && feedGradientInfo.EndTime > timeOffset)
                            {
                                f = feedGradientInfo;
                                break;
                            }
                        }
                        if (f != null)
                        {
                            if (lastInfoType != f.InfoType)
                            {
                                totalSecond = 0;
                                calcTotalSeconds = 0;
                                statTotalSeconds = 0;
                            }

                            switch (f.InfoType)
                            {
                                case "Constant":
                                    deviceParameter.FeedParam1.Feed_PV = f.A >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : f.A;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                    }

                                    int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }

                                    if (f.StatDisable)
                                    {
                                        totalSecond = 0;
                                    }
                                    else
                                    {
                                        totalSecond += 1;
                                    }

                                    if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                                    {
                                        pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo >= 0)
                                        {
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                            param = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = deviceParameter.FeedParam1.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                        }

                                        if (f.CurveDOStat)
                                        {
                                            while (true)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }
                                                RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                if (f.StatSymbol == 0)//小于
                                                {
                                                    if (realTime.DO < f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (f.StatSymbol == 1)//大于
                                                {
                                                    if (realTime.DO > f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                Thread.Sleep(1000);
                                            }
                                        }
                                        else if (f.CurvepHStat)
                                        {
                                            while (true)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }
                                                RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                if (f.StatSymbol == 0)//小于
                                                {
                                                    if (realTime.PH < f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (f.StatSymbol == 1)//大于
                                                {
                                                    if (realTime.PH > f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                Thread.Sleep(1000);
                                            }
                                        }
                                    }
                                    break;
                                case "Polynomial"://多项式，执行业务逻辑
                                    double a = f.A;//20
                                    double b = double.Parse(f.B);//0.7
                                    double c = f.C;//40
                                    double calcTimeOffset = Math.Round(calcTotalSeconds / 3600, 2);
                                    double feed = Math.Round(a * Math.Pow(calcTimeOffset, 2) + b * calcTimeOffset + c, 2);

                                    deviceParameter.FeedParam1.Feed_PV = (float)feed >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)feed;
                                    int pumpNo1 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo1 >= 0)
                                    {
                                        param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo1,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                    }

                                    count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }

                                    calcTotalSeconds += 1;

                                    if (f.StatDisable)
                                    {
                                        totalSecond = 0;
                                    }
                                    else
                                    {
                                        totalSecond += 1;
                                    }

                                    if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                                    {
                                        pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo >= 0)
                                        {
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                            param = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = deviceParameter.FeedParam1.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                        }

                                        if (f.CurveDOStat)
                                        {
                                            while (true)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }
                                                RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                if (f.StatSymbol == 0)//小于
                                                {
                                                    if (realTime.DO < f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (f.StatSymbol == 1)//大于
                                                {
                                                    if (realTime.DO > f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                Thread.Sleep(1000);
                                            }
                                        }
                                        else if (f.CurvepHStat)
                                        {
                                            while (true)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }
                                                RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                if (f.StatSymbol == 0)//小于
                                                {
                                                    if (realTime.PH < f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (f.StatSymbol == 1)//大于
                                                {
                                                    if (realTime.PH > f.StatValue)
                                                    {

                                                        break;
                                                    }
                                                }
                                                Thread.Sleep(1000);
                                            }
                                        }
                                    }
                                    break;
                                case "Exponential"://指数，执行业务逻辑
                                    double f1 = f.A;//20
                                    double μ = double.Parse(f.B);//0.7
                                    double calcTimeOffset1 = Math.Round(calcTotalSeconds / 3600, 2);
                                    double feed1 = Math.Round(f1 * Math.Exp(μ * calcTimeOffset1), 2);

                                    deviceParameter.FeedParam1.Feed_PV = (float)feed1 >= AppSession.DefaultPumpFlowRate ? AppSession.DefaultPumpFlowRate : (float)feed1;
                                    int pumpNo2 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo2 >= 0)
                                    {
                                        param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo2,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                    }

                                    count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }

                                    calcTotalSeconds += 1;

                                    if (f.StatDisable)
                                    {
                                        totalSecond = 0;
                                    }
                                    else
                                    {
                                        totalSecond += 1;
                                    }

                                    if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                                    {
                                        pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo >= 0)
                                        {
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                            param = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = deviceParameter.FeedParam1.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                        }

                                        if (f.CurveDOStat)
                                        {
                                            while (true)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }
                                                RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                if (f.StatSymbol == 0)//小于
                                                {
                                                    if (realTime.DO < f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (f.StatSymbol == 1)//大于
                                                {
                                                    if (realTime.DO > f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                Thread.Sleep(1000);
                                            }
                                        }
                                        else if (f.CurvepHStat)
                                        {
                                            while (true)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }
                                                RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                if (f.StatSymbol == 0)//小于
                                                {
                                                    if (realTime.PH < f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (f.StatSymbol == 1)//大于
                                                {
                                                    if (realTime.PH > f.StatValue)
                                                    {

                                                        break;
                                                    }
                                                }
                                                Thread.Sleep(1000);
                                            }
                                        }
                                    }
                                    break;
                                case "DO_Feedback"://DO-stat(速度)
                                    if (deviceParameter.DO <= f.A)
                                    {
                                        if (f.IsConstant)
                                        {
                                            deviceParameter.FeedParam1.Feed_PV = float.Parse(f.B) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.B);
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                param = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                            }
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                            deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                param = new PeristalticPumpControlParam()
                                                {
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                            }
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                            deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                param = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                            }
                                        }

                                        statTotalSeconds += 1;

                                        count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count > 0)
                                        {
                                            if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else if (deviceParameter.DO >= f.C)
                                    {
                                        if (f.IsConstant)
                                        {
                                            deviceParameter.FeedParam1.Feed_PV = float.Parse(f.D) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.D);
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                param = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                            }
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                            deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                param = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                            }
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                            deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }

                                        statTotalSeconds += 1;

                                        count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count > 0)
                                        {
                                            if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                        int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo3 >= 0)
                                        {
                                            PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo3,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case "PH_Feedback"://根据PH反馈控制，执行业务逻辑
                                    if (deviceParameter.PH <= f.A)
                                    {
                                        if (f.IsConstant)
                                        {
                                            deviceParameter.FeedParam1.Feed_PV = float.Parse(f.B) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.B);
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                            deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                            deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }

                                        statTotalSeconds += 1;

                                        count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count > 0)
                                        {
                                            if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else if (deviceParameter.PH >= f.C)
                                    {
                                        if (f.IsConstant)
                                        {
                                            deviceParameter.FeedParam1.Feed_PV = float.Parse(f.D) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.D);
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                            deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                            deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }

                                        statTotalSeconds += 1;

                                        count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count > 0)
                                        {
                                            if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                        int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo9 >= 0)
                                        {
                                            PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo9,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                        }
                                    }
                                    break;
                                case "DO_Feedback_Total":
                                    if (deviceParameter.DO <= f.A)
                                    {
                                        if (f.IsConstant)
                                        {
                                            float flow = float.Parse(f.B);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = 0;
                                            }
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam1.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = 0;
                                            }
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam1.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = 0;
                                            }
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam1.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                        }

                                        statTotalSeconds += 1;

                                        int count5 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count5 > 0)
                                        {
                                            if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count5--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else if (deviceParameter.DO >= f.C)
                                    {
                                        if (f.IsConstant)
                                        {
                                            float flow = float.Parse(f.D);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam1.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam1.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam1.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                        }

                                        int count5 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count5 > 0)
                                        {
                                            if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count5--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                        int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo9 >= 0)
                                        {
                                            PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo9,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case "PH_Feedback_Total"://根据PH反馈控制，执行业务逻辑
                                    if (deviceParameter.PH <= f.A)
                                    {
                                        if (f.IsConstant)
                                        {
                                            float flow = float.Parse(f.B);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam1.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam1.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam1.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                        }

                                        statTotalSeconds += 1;

                                        int count5 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count5 > 0)
                                        {
                                            if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count5--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else if (deviceParameter.PH >= f.C)
                                    {
                                        if (f.IsConstant)
                                        {
                                            float flow = float.Parse(f.D);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam1.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam1.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam1.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam1.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                        }

                                        statTotalSeconds += 1;

                                        int count5 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count5 > 0)
                                        {
                                            if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count5--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                        int pumpNo8 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo8 >= 0)
                                        {
                                            PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo8,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case "Quantitative":
                                    if (f.A <= 0 || QuantitativeFinish)
                                    {
                                        continue;
                                    }
                                    float.TryParse(f.B, out var b1);
                                    int pumpNo10 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo10 >= 0)
                                    {
                                        PeristalticPumpControlParam param10 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo10,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = b1,
                                            FlowCapacity = f.A
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param10);
                                    }
                                    QuantitativeFinish = true;

                                    int count10 = Convert.ToInt32(Math.Ceiling(f.A / b1 * 3600));
                                    int index10 = 0;
                                    while (index10 < count10)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }
                                        index10 += 1;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case "Cycle":
                                    try
                                    {
                                        DateTime startTime = DateTime.Now;
                                        double totalMinutes = 0;

                                        if (f.A <= 0 || !float.TryParse(f.B, out var paramB) || paramB <= 0 || f.C <= 0 || !float.TryParse(f.D, out var paramD) || paramD <= 0)
                                        {
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                            continue;
                                        }
                                        int pumpNo11 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo11 < 0)
                                        {
                                            deviceParameter.FeedParam1.Feed_PV = 0;
                                            continue;
                                        }
                                        deviceParameter.FeedParam1.Feed_PV = f.C;
                                        if (pumpNo11 >= 0)
                                        {
                                            PeristalticPumpControlParam param11 = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo11,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = deviceParameter.FeedParam1.Feed_PV,
                                                FlowCapacity = paramD
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param11);
                                        }

                                        int count11 = Convert.ToInt32(Math.Ceiling(paramD / deviceParameter.FeedParam1.Feed_PV * 3600));
                                        while (count11 > 0)
                                        {
                                            if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                e.Result = deviceParameter.Name;
                                                return;
                                            }
                                            count11--;
                                            Thread.Sleep(1000);
                                        }
                                        totalMinutes = (DateTime.Now - startTime).TotalMinutes;

                                        deviceParameter.FeedParam1.Feed_PV = 0;

                                        double diff = f.A - totalMinutes;
                                        if (diff > 0)
                                        {
                                            int count12 = Convert.ToInt32(diff * 60);
                                            int index12 = 0;
                                            while (index12 < count12)
                                            {
                                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }
                                                index12 += 1;
                                                Thread.Sleep(1000);
                                            }
                                        }

                                        int count7 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        int index7 = 0;
                                        while (index7 < count7)
                                        {
                                            if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            index7 += 1;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogHelper.Debug(string.Format("周期补料调整失败，错误信息：{0}", ex.Message));
                                        Thread.Sleep(AppSession.Interval * 1000);
                                    }
                                    break;
                                default:
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                    int pumpNo7 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo7 >= 0)
                                    {
                                        PeristalticPumpControlParam param7 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo7,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = 0
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param7);
                                    }
                                    Thread.Sleep(1000);
                                    break;
                            }

                            lastInfoType = f.InfoType;
                        }

                        endTime = feedGradientInfos[feedGradientInfos.Count - 1].EndTime;
                        timeOffset = Math.Round((DateTime.Now - beginTime).TotalMinutes, 2);
                    }
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam1.IsControling = false;
                        deviceParameter.FeedParam1.Feed_PV = 0;
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料指数完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //DO反馈 速度
            else if (feedMode == FeedControlMode.DO_stat_Speed)
            {
                dicFeed1Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    double totalSeconds = 0;
                    var worker = (BackgroundWorker)s;
                    while (true)
                    {
                        try
                        {
                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            var flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }
                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "DO_Feedback".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在DO_stat(速度)策略", deviceParameter.Name));
                                return;
                            }

                            if (deviceParameter.DO <= f.A)
                            {
                                if (f.IsConstant)
                                {
                                    deviceParameter.FeedParam1.Feed_PV = float.Parse(f.B) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.B);
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                    deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                    deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }

                                totalSeconds += 1;

                                int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count > 0)
                                {
                                    if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }

                                    count--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else if (deviceParameter.DO >= f.C)
                            {
                                if (f.IsConstant)
                                {
                                    deviceParameter.FeedParam1.Feed_PV = float.Parse(f.D) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.D);
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                    deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                    deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }

                                totalSeconds += 1;

                                int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count > 0)
                                {
                                    if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }

                                    count--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                deviceParameter.FeedParam1.Feed_PV = 0;
                                int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                if (pumpNo >= 0)
                                {
                                    PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                }
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料DO反馈调整失败，错误信息：{0}", ex.Message));
                        }
                    }
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam1.IsControling = false;
                        deviceParameter.FeedParam1.Feed_PV = 0;
                    }

                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料Do反馈完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //PH反馈 速度
            else if (feedMode == FeedControlMode.pH_stat_Speed)
            {
                dicFeed1Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    double totalSeconds = 0;
                    while (true)
                    {
                        try
                        {
                            var worker = (BackgroundWorker)s;
                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            var flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }
                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "PH_Feedback".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在PH反馈策略", deviceParameter.Name));
                                return;
                            }

                            if (deviceParameter.PH <= f.A)
                            {
                                if (f.IsConstant)
                                {
                                    deviceParameter.FeedParam1.Feed_PV = float.Parse(f.B) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.B);
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                    deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                    deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }

                                totalSeconds += 1;

                                int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count > 0)
                                {
                                    if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }
                                    count--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else if (deviceParameter.PH >= f.C)
                            {
                                if (f.IsConstant)
                                {
                                    deviceParameter.FeedParam1.Feed_PV = float.Parse(f.D) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.D);
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                    deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                    deviceParameter.FeedParam1.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }

                                totalSeconds += 1;

                                int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count > 0)
                                {
                                    if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }
                                    count--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                deviceParameter.FeedParam1.Feed_PV = 0;
                                int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                if (pumpNo >= 0)
                                {
                                    PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                }
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料PH反馈调整失败，错误信息：{0}", ex.Message));
                        }
                        finally
                        {
                            Thread.Sleep(1000);
                        }
                    }
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam1.IsControling = false;
                        deviceParameter.FeedParam1.Feed_PV = 0;
                    }

                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料PH反馈完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //DO反馈 总量
            else if (feedMode == FeedControlMode.DO_stat_Volume)
            {
                dicFeed1Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    double totalSeconds = 0;
                    while (true)
                    {
                        try
                        {
                            var worker = (BackgroundWorker)s;
                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            var flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }
                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "DO_Feedback_Total".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在DO反馈总量策略", deviceParameter.Name));
                                return;
                            }

                            if (deviceParameter.DO <= f.A)
                            {
                                if (f.IsConstant)
                                {
                                    float flow = float.Parse(f.B);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam1.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam1.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? temp : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam1.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                        totalSeconds += 1;
                                    }
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                }

                                totalSeconds += 1;

                                int count1 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count1 > 0)
                                {
                                    if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }

                                    count1--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else if (deviceParameter.DO >= f.C)
                            {
                                if (f.IsConstant)
                                {
                                    float flow = float.Parse(f.D);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam1.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam1.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam1.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                }

                                totalSeconds += 1;

                                int count1 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count1 > 0)
                                {
                                    if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }

                                    count1--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                deviceParameter.FeedParam1.Feed_PV = 0;
                                int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                if (pumpNo >= 0)
                                {
                                    PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                }
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料DO反馈总量调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }

                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam1.IsControling = false;
                        deviceParameter.FeedParam1.Feed_PV = 0;
                    }

                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料Do反馈总量完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //PH反馈 总量
            else if (feedMode == FeedControlMode.pH_stat_Volume)
            {
                dicFeed1Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    double totalSeconds = 0;
                    var worker = (BackgroundWorker)s;
                    while (true)
                    {
                        try
                        {
                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            var flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }
                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "PH_Feedback_Total".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在PH反馈总量策略", deviceParameter.Name));
                                return;
                            }

                            if (deviceParameter.PH <= f.A)
                            {
                                if (f.IsConstant)
                                {
                                    float flow = float.Parse(f.B);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam1.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam1.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                        totalSeconds += 1;
                                    }
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam1.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                }

                                totalSeconds += 1;

                                int count1 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count1 > 0)
                                {
                                    if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }

                                    count1--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else if (deviceParameter.PH >= f.C)
                            {
                                if (f.IsConstant)
                                {
                                    float flow = float.Parse(f.D);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam1.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                        totalSeconds += 1;
                                    }
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam1.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam1.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam1.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam1.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam1.Feed_PV = 0;
                                }

                                totalSeconds += 1;

                                int count1 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count1 > 0)
                                {
                                    if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }

                                    count1--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                deviceParameter.FeedParam1.Feed_PV = 0;
                                int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                if (pumpNo >= 0)
                                {
                                    PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                }
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料PH反馈调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam1.IsControling = false;
                        deviceParameter.FeedParam1.Feed_PV = 0;
                    }

                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料PH反馈完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //定量
            else if (feedMode == FeedControlMode.Quantitative)
            {
                dicFeed1Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    if (deviceParameter.FeedParam1.Feed_Total <= 0)
                    {
                        return;
                    }

                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                    if (pumpNo >= 0)
                    {
                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                        {
                            PumpNo = pumpNo,
                            Pump = pump,
                            ControlMode = PumpControlMode.Direct,
                            FlowSpeed = deviceParameter.FeedParam1.Feed_PV,
                            FlowCapacity = deviceParameter.FeedParam1.Feed_Total
                        };
                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                    }

                    int count2 = 0;
                    if (deviceParameter.FeedParam1.Feed_PV > 0)
                    {
                        count2 = Convert.ToInt32(Math.Ceiling(deviceParameter.FeedParam1.Feed_Total / deviceParameter.FeedParam1.Feed_PV * 3600));
                    }
                    int index2 = 0;
                    while (index2 < count2)
                    {
                        if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                        {
                            return;
                        }
                        index2 += 1;
                        Thread.Sleep(1000);
                    }
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam1.IsControling = false;
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料定量完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //周期
            else if (feedMode == FeedControlMode.Cycle)
            {
                dicFeed1Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed1Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    var worker = (BackgroundWorker)s;
                    while (true)
                    {
                        try
                        {
                            DateTime startTime = DateTime.Now;
                            double totalMinutes = 0;

                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            var flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }
                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "Cycle".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在周期补料策略", deviceParameter.Name));
                                return;
                            }
                            if (f.A <= 0 || !float.TryParse(f.B, out var b) || b <= 0 || f.C <= 0 || !float.TryParse(f.D, out var d) || d <= 0)
                            {
                                deviceParameter.FeedParam1.Feed_PV = 0;
                                continue;
                            }
                            int pumpNo11 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo11 < 0)
                            {
                                deviceParameter.FeedParam1.Feed_PV = 0;
                                continue;
                            }
                            deviceParameter.FeedParam1.Feed_PV = f.C;
                            if (pumpNo11 >= 0)
                            {
                                PeristalticPumpControlParam param11 = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo11,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = deviceParameter.FeedParam1.Feed_PV,
                                    FlowCapacity = d
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param11);
                            }

                            int count11 = 1;
                            if (deviceParameter.FeedParam1.Feed_PV > 0)
                            {
                                count11 = Convert.ToInt32(Math.Ceiling(d / deviceParameter.FeedParam1.Feed_PV * 3600));
                            }
                            while (count11 > 0)
                            {
                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                {
                                    e.Result = deviceParameter.Name;
                                    return;
                                }
                                count11--;
                                Thread.Sleep(1000);
                            }
                            totalMinutes = (DateTime.Now - startTime).TotalMinutes;
                            double diff = f.A - totalMinutes;
                            deviceParameter.FeedParam1.Feed_PV = 0;
                            if (diff > 0)
                            {
                                int count12 = Convert.ToInt32(diff * 60);
                                while (count12>0)
                                {
                                    if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }
                                    count12--;
                                    Thread.Sleep(1000);
                                }
                            }

                            int count2 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                            while (count2 > 0)
                            {
                                if (dicFeed1Worker[deviceParameter.Name].CancellationPending)
                                {
                                    return;
                                }

                                count2--;
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("周期补料调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam1.IsControling = false;
                        deviceParameter.FeedParam1.Feed_PV = 0;
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料周期完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed1Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }

            else if (feedMode == FeedControlMode.Probe)
            {
                var param = ProbingParameterManager.GetInstance().ProbeCol.FindFirst(t => t.DeviceID == CurrentDeviceParameter.Name);
                dicFeed1Probe[CurrentDeviceParameter.Name].InitProbParam(param);
                dicFeed1Probe[CurrentDeviceParameter.Name].StartCtrl();
            }
        });

        public DelegateCommand Feed2RunCommand => new(() =>
        {
            PeristalticPump pump = PeristalticPump.Feed2Pump;

            if (dicFeed2Worker[CurrentDeviceParameter.Name] != null && dicFeed2Worker[CurrentDeviceParameter.Name].IsBusy)
            {
                dicFeed2Worker[CurrentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            if (dicFeed2Probe[CurrentDeviceParameter.Name] != null)
            {
                dicFeed2Probe[CurrentDeviceParameter.Name].StopCtrl();
                Thread.Sleep(100);
            }

            //关闭控制
            if (!CurrentDeviceParameter.FeedParam2.IsControling)
            {
                Task.Run(() =>
                {
                    try
                    {
                        int pumpNo = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, pump);
                        if (pumpNo >= 0)
                        {
                            PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                            {
                                PumpNo = pumpNo,
                                Pump = pump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = 0,
                                FlowCapacity = 0
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(CurrentDeviceParameter.Name, param);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug("详情界面：补料2取消报错" + ex.Message);
                    }
                });
                return;
            }

            var feedMode = CurrentDeviceParameter.FeedParam2.FeedMode;
            //恒速
            if (feedMode == FeedControlMode.ConstantSpeed)
            {
                dicFeed2Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                    FeedStrategyInfo f = null;
                    PeristalticPumpControlParam param = new PeristalticPumpControlParam();
                    if (allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos) && feedGradientInfos != null)
                    {
                        f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "Constant".ToUpper() && t.Pump == pump);
                        if (f != null && f.A > 0)
                        {
                            CurrentDeviceParameter.FeedParam2.Feed_PV = f.A;
                        }
                    }

                    int pumpNo = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, pump);
                    if (pumpNo >= 0)
                    {
                        param = new PeristalticPumpControlParam()
                        {
                            PumpNo = pumpNo,
                            Pump = pump,
                            ControlMode = PumpControlMode.Direct,
                            FlowSpeed = (float)CurrentDeviceParameter.FeedParam2.Feed_PV >= AppSession.DefaultPumpFlowRate ? AppSession.DefaultPumpFlowRate : (float)CurrentDeviceParameter.FeedParam2.Feed_PV,
                            FlowCapacity = Const.MaxPumpFlowCapacity
                        };
                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(CurrentDeviceParameter.Name, param);
                        dicFeed2SP[CurrentDeviceParameter.Name] = CurrentDeviceParameter.FeedParam2.Feed_PV;
                    }

                    double totalSecond = 0;
                    while (true)
                    {
                        try
                        {
                            var worker = (BackgroundWorker)s;
                            if (worker.CancellationPending)
                            {
                                return;
                            }
                            f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "Constant".ToUpper() && t.Pump == pump);

                            if (dicFeed2SP[deviceParameter.Name] != deviceParameter.FeedParam2.Feed_PV)
                            {
                                pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                if (pumpNo >= 0)
                                {
                                    param = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = dicFeed2SP[CurrentDeviceParameter.Name] >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : dicFeed2SP[CurrentDeviceParameter.Name],
                                        FlowCapacity = Const.MaxPumpFlowCapacity
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                }

                                dicFeed2SP[CurrentDeviceParameter.Name] = deviceParameter.FeedParam2.Feed_PV;
                            }

                            Thread.Sleep(1000);

                            if (f.StatDisable)
                            {
                                totalSecond = 0;
                            }
                            else
                            {
                                totalSecond += 1;
                            }

                            if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                            {
                                pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                if (pumpNo >= 0)
                                {
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                    param = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = deviceParameter.FeedParam2.Feed_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                }

                                if (f.CurveDOStat)
                                {
                                    while (true)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (f.StatSymbol == 0)//小于
                                        {
                                            if (realTime.DO < f.StatValue)
                                            {
                                                pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                                if (pumpNo >= 0)
                                                {
                                                    deviceParameter.FeedParam2.Feed_PV = dicFeed2SP[CurrentDeviceParameter.Name];
                                                    param = new PeristalticPumpControlParam()
                                                    {
                                                        PumpNo = pumpNo,
                                                        Pump = pump,
                                                        ControlMode = PumpControlMode.Direct,
                                                        FlowSpeed = dicFeed2SP[CurrentDeviceParameter.Name] >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : dicFeed2SP[CurrentDeviceParameter.Name],
                                                        FlowCapacity = Const.MaxPumpFlowCapacity
                                                    };
                                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                                }
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        else if (f.StatSymbol == 1)//大于
                                        {
                                            if (realTime.DO > f.StatValue)
                                            {
                                                pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                                if (pumpNo >= 0)
                                                {
                                                    deviceParameter.FeedParam2.Feed_PV = dicFeed2SP[CurrentDeviceParameter.Name];
                                                    param = new PeristalticPumpControlParam()
                                                    {
                                                        PumpNo = pumpNo,
                                                        Pump = pump,
                                                        ControlMode = PumpControlMode.Direct,
                                                        FlowSpeed = dicFeed2SP[CurrentDeviceParameter.Name] >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : dicFeed2SP[CurrentDeviceParameter.Name],
                                                        FlowCapacity = Const.MaxPumpFlowCapacity
                                                    };
                                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                                }
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                    }
                                }
                                else if (f.CurvepHStat)
                                {
                                    while (true)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (f.StatSymbol == 0)//小于
                                        {
                                            if (realTime.PH < f.StatValue)
                                            {
                                                pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                                if (pumpNo >= 0)
                                                {
                                                    deviceParameter.FeedParam2.Feed_PV = dicFeed2SP[CurrentDeviceParameter.Name];
                                                    param = new PeristalticPumpControlParam()
                                                    {
                                                        PumpNo = pumpNo,
                                                        Pump = pump,
                                                        ControlMode = PumpControlMode.Direct,
                                                        FlowSpeed = dicFeed2SP[CurrentDeviceParameter.Name] >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : dicFeed2SP[CurrentDeviceParameter.Name],
                                                        FlowCapacity = Const.MaxPumpFlowCapacity
                                                    };
                                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                                }
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        else if (f.StatSymbol == 1)//大于
                                        {
                                            if (realTime.PH > f.StatValue)
                                            {
                                                pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                                if (pumpNo >= 0)
                                                {
                                                    deviceParameter.FeedParam2.Feed_PV = dicFeed2SP[CurrentDeviceParameter.Name];
                                                    param = new PeristalticPumpControlParam()
                                                    {
                                                        PumpNo = pumpNo,
                                                        Pump = pump,
                                                        ControlMode = PumpControlMode.Direct,
                                                        FlowSpeed = dicFeed2SP[CurrentDeviceParameter.Name] >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : dicFeed2SP[CurrentDeviceParameter.Name],
                                                        FlowCapacity = Const.MaxPumpFlowCapacity
                                                    };
                                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                                }
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料常数调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam2.IsControling = false;
                        //如果是常量，补料预设值不重置
                        //deviceParameter.FeedParam2.Feed_PV = 0;  
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料指数完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //多项式
            else if (feedMode == FeedControlMode.Polynomial)
            {
                dicFeed2Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    PeristalticPumpControlParam param = new PeristalticPumpControlParam();
                    var worker = (BackgroundWorker)s;

                    double totalSecond = 0;
                    double secondCount = 0;
                    while (true)
                    {
                        try
                        {
                            
                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            bool flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }

                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "Polynomial".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在多项式策略", deviceParameter.Name));
                                return;
                            }

                            double interval = secondCount / 3600;
                            double a = f.A;//20
                            double b = double.Parse(f.B);//0.7
                            double c = f.C;//40
                            double deltaT = double.Parse(f.D);
                            double diff = interval - deltaT > 0 ? interval - deltaT : 0;
                            double feed = Math.Round(a * Math.Pow(diff, 2) + b * diff + c, 2);

                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            deviceParameter.FeedParam2.Feed_PV = (float)feed >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)feed;
                            if (pumpNo >= 0)
                            {
                                param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                            }

                            int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                            while (count > 0)
                            {
                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                {
                                    return;
                                }

                                count--;
                                Thread.Sleep(1000);
                            }

                            secondCount += 1;

                            if (f.StatDisable)
                            {
                                totalSecond = 0;
                            }
                            else
                            {
                                totalSecond += 1;
                            }

                            if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                            {
                                if (f.CurveDOStat || f.CurvepHStat)
                                {
                                    pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                        param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = 0
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                    }
                                }

                                if (f.CurveDOStat)
                                {
                                    while (true)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (f.StatSymbol == 0)//小于
                                        {
                                            if (realTime.DO < f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        else if (f.StatSymbol == 1)//大于
                                        {
                                            if (realTime.DO > f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                    }
                                }
                                else if (f.CurvepHStat)
                                {
                                    int pauseSecond = 0;

                                    while (true)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (f.StatSymbol == 0)//小于
                                        {
                                            if (realTime.PH < f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        else if (f.StatSymbol == 1)//大于
                                        {
                                            if (realTime.PH > f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                        pauseSecond += 1;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料多项式调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam2.IsControling = false;
                        deviceParameter.FeedParam2.Feed_PV = 0;
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料指数完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //指数
            else if (feedMode == FeedControlMode.Exponential)
            {
                dicFeed2Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var worker = (BackgroundWorker)s;
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    PeristalticPumpControlParam param = new PeristalticPumpControlParam();
                    double totalSecond = 0;
                    double secondCount = 0;
                    while (true)
                    {
                        try
                        {
                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            bool flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }
                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "Exponential".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在指数策略", deviceParameter.Name));
                                return;
                            }

                            double interval = secondCount / 3600;
                            double f1 = f.A;//20
                            double μ = double.Parse(f.B);//0.7
                            double deltaT = f.C;//Δt
                            double diff = interval - deltaT > 0 ? interval - deltaT : 0;
                            double feed = Math.Round(f1 * Math.Exp(μ * (diff)), 2);

                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                deviceParameter.FeedParam2.Feed_PV = (float)feed >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)feed;
                                param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                            }

                            int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                            while (count > 0)
                            {
                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                {
                                    return;
                                }

                                count--;
                                Thread.Sleep(1000);
                            }
                            secondCount += 1;

                            if (f.StatDisable)
                            {
                                totalSecond = 0;
                            }
                            else
                            {
                                totalSecond += 1;
                            }

                            if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                            {
                                if (f.CurveDOStat || f.CurvepHStat)
                                {
                                    pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                        param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = 0
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                    }
                                }

                                if (f.CurveDOStat)
                                {
                                    while (true)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (f.StatSymbol == 0)//小于
                                        {
                                            if (realTime.DO < f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        else if (f.StatSymbol == 1)//大于
                                        {
                                            if (realTime.DO > f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                    }
                                }
                                else if (f.CurvepHStat)
                                {
                                    int pauseSecond = 0;

                                    while (true)
                                    {
                                        if (worker.CancellationPending)
                                        {
                                            return;
                                        }
                                        RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (f.StatSymbol == 0)//小于
                                        {
                                            if (realTime.PH < f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        else if (f.StatSymbol == 1)//大于
                                        {
                                            if (realTime.PH > f.StatValue)
                                            {
                                                totalSecond = 0;
                                                break;
                                            }
                                        }
                                        Thread.Sleep(1000);
                                        pauseSecond += 1;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料指数调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam2.IsControling = false;
                        deviceParameter.FeedParam2.Feed_PV = 0;
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料指数完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //时间序列
            else if (feedMode == FeedControlMode.TimeSeries)
            {
                dicFeed2Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    BackgroundWorker worker = s as BackgroundWorker;

                    bool QuantitativeFinish = false;//指示时间序列中定量补是否已经完成

                    var allDeviceInfos = FeedGradientManager.GetInstance().FeedGradientCol;
                    bool flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                    if (!flag || feedGradientInfos == null)
                    {
                        MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                        return;
                    }
                    DateTime beginTime;
                    beginTime = DateTime.Now;//开始时间
                    double endTime = feedGradientInfos[feedGradientInfos.Count - 1].EndTime;
                    double timeOffset = Math.Round((DateTime.Now - beginTime).TotalMinutes, 2);//时间差

                    double totalSecond = 0;//用于stat的停顿计时

                    double statTotalSeconds = 0;//用于stat多项式|指数的时间计算

                    double calcTotalSeconds = 0;

                    string lastInfoType = "";

                    while (timeOffset < endTime)
                    {
                        if (worker.CancellationPending)
                        {
                            return;
                        }

                        allDeviceInfos = FeedGradientManager.GetInstance().FeedGradientCol;
                        flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out feedGradientInfos);
                        if (!flag || feedGradientInfos == null)
                        {
                            MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                            return;
                        }

                        //找到当前时间点需要执行哪段
                        //var f = feedGradientInfos.FindFirst(c => c.BeginTime <= timeOffset && c.EndTime > timeOffset);
                        FeedGradientInfo f = null;
                        PeristalticPumpControlParam param = new PeristalticPumpControlParam();

                        foreach (var feedGradientInfo in feedGradientInfos)
                        {
                            if (feedGradientInfo.BeginTime <= timeOffset && feedGradientInfo.EndTime > timeOffset)
                            {
                                f = feedGradientInfo;
                                break;
                            }
                        }
                        if (f != null)
                        {
                            if (lastInfoType != f.InfoType)
                            {
                                totalSecond = 0;
                                calcTotalSeconds = 0;
                                statTotalSeconds = 0;
                            }

                            switch (f.InfoType)
                            {
                                case "Constant":
                                    deviceParameter.FeedParam2.Feed_PV = f.A >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : f.A;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                    }

                                    int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }

                                    if (f.StatDisable)
                                    {
                                        totalSecond = 0;
                                    }
                                    else
                                    {
                                        totalSecond += 1;
                                    }

                                    if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                                    {
                                        pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo >= 0)
                                        {
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                            param = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = deviceParameter.FeedParam2.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                        }

                                        if (f.CurveDOStat)
                                        {
                                            while (true)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }
                                                RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                if (f.StatSymbol == 0)//小于
                                                {
                                                    if (realTime.DO < f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (f.StatSymbol == 1)//大于
                                                {
                                                    if (realTime.DO > f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                Thread.Sleep(1000);
                                            }
                                        }
                                        else if (f.CurvepHStat)
                                        {
                                            while (true)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }
                                                RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                if (f.StatSymbol == 0)//小于
                                                {
                                                    if (realTime.PH < f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (f.StatSymbol == 1)//大于
                                                {
                                                    if (realTime.PH > f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                Thread.Sleep(1000);
                                            }
                                        }
                                    }
                                    break;
                                case "Polynomial"://多项式，执行业务逻辑
                                    double a = f.A;//20
                                    double b = double.Parse(f.B);//0.7
                                    double c = f.C;//40
                                    double calcTimeOffset = Math.Round(calcTotalSeconds / 3600, 2);
                                    double feed = Math.Round(a * Math.Pow(calcTimeOffset, 2) + b * calcTimeOffset + c, 2);

                                    deviceParameter.FeedParam2.Feed_PV = (float)feed >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)feed;
                                    int pumpNo1 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo1 >= 0)
                                    {
                                        param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo1,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                    }

                                    count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }

                                    calcTotalSeconds += 1;

                                    if (f.StatDisable)
                                    {
                                        totalSecond = 0;
                                    }
                                    else
                                    {
                                        totalSecond += 1;
                                    }

                                    if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                                    {
                                        pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo >= 0)
                                        {
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                            param = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = deviceParameter.FeedParam2.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                        }

                                        if (f.CurveDOStat)
                                        {
                                            while (true)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }
                                                RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                if (f.StatSymbol == 0)//小于
                                                {
                                                    if (realTime.DO < f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (f.StatSymbol == 1)//大于
                                                {
                                                    if (realTime.DO > f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                Thread.Sleep(1000);
                                            }
                                        }
                                        else if (f.CurvepHStat)
                                        {
                                            while (true)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }
                                                RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                if (f.StatSymbol == 0)//小于
                                                {
                                                    if (realTime.PH < f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (f.StatSymbol == 1)//大于
                                                {
                                                    if (realTime.PH > f.StatValue)
                                                    {

                                                        break;
                                                    }
                                                }
                                                Thread.Sleep(1000);
                                            }
                                        }
                                    }
                                    break;
                                case "Exponential"://指数，执行业务逻辑
                                    double f1 = f.A;//20
                                    double μ = double.Parse(f.B);//0.7
                                    double calcTimeOffset1 = Math.Round(calcTotalSeconds / 3600, 2);
                                    double feed1 = Math.Round(f1 * Math.Exp(μ * calcTimeOffset1), 2);

                                    deviceParameter.FeedParam2.Feed_PV = (float)feed1 >= AppSession.DefaultPumpFlowRate ? AppSession.DefaultPumpFlowRate : (float)feed1;
                                    int pumpNo2 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo2 >= 0)
                                    {
                                        param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo2,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                    }

                                    count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }

                                    calcTotalSeconds += 1;

                                    if (f.StatDisable)
                                    {
                                        totalSecond = 0;
                                    }
                                    else
                                    {
                                        totalSecond += 1;
                                    }

                                    if (totalSecond > 0 && f.StatInterval > 0 && totalSecond % f.StatInterval == 0)
                                    {
                                        pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo >= 0)
                                        {
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                            param = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = deviceParameter.FeedParam2.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                        }

                                        if (f.CurveDOStat)
                                        {
                                            while (true)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }
                                                RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                if (f.StatSymbol == 0)//小于
                                                {
                                                    if (realTime.DO < f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (f.StatSymbol == 1)//大于
                                                {
                                                    if (realTime.DO > f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                Thread.Sleep(1000);
                                            }
                                        }
                                        else if (f.CurvepHStat)
                                        {
                                            while (true)
                                            {
                                                if (worker.CancellationPending)
                                                {
                                                    return;
                                                }
                                                RealTimeParam realTime = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                                if (f.StatSymbol == 0)//小于
                                                {
                                                    if (realTime.PH < f.StatValue)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else if (f.StatSymbol == 1)//大于
                                                {
                                                    if (realTime.PH > f.StatValue)
                                                    {

                                                        break;
                                                    }
                                                }
                                                Thread.Sleep(1000);
                                            }
                                        }
                                    }
                                    break;
                                case "DO_Feedback"://DO-stat(速度)
                                    if (deviceParameter.DO <= f.A)
                                    {
                                        if (f.IsConstant)
                                        {
                                            deviceParameter.FeedParam2.Feed_PV = float.Parse(f.B) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.B);
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                param = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                            }
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                            deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                param = new PeristalticPumpControlParam()
                                                {
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                            }
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                            deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                param = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                            }
                                        }

                                        statTotalSeconds += 1;

                                        count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count > 0)
                                        {
                                            if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else if (deviceParameter.DO >= f.C)
                                    {
                                        if (f.IsConstant)
                                        {
                                            deviceParameter.FeedParam2.Feed_PV = float.Parse(f.D) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.D);
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                param = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                            }
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                            deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                param = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                            }
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                            deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }

                                        statTotalSeconds += 1;

                                        count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count > 0)
                                        {
                                            if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                        int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo3 >= 0)
                                        {
                                            PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo3,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case "PH_Feedback"://根据PH反馈控制，执行业务逻辑
                                    if (deviceParameter.PH <= f.A)
                                    {
                                        if (f.IsConstant)
                                        {
                                            deviceParameter.FeedParam2.Feed_PV = float.Parse(f.B) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.B);
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                            deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                            deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }

                                        statTotalSeconds += 1;

                                        count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count > 0)
                                        {
                                            if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else if (deviceParameter.PH >= f.C)
                                    {
                                        if (f.IsConstant)
                                        {
                                            deviceParameter.FeedParam2.Feed_PV = float.Parse(f.D) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.D);
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                            deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                            deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = Const.MaxPumpFlowCapacity
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }
                                        }

                                        statTotalSeconds += 1;

                                        count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count > 0)
                                        {
                                            if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                        int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo9 >= 0)
                                        {
                                            PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo9,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                        }
                                    }
                                    break;
                                case "DO_Feedback_Total":
                                    if (deviceParameter.DO <= f.A)
                                    {
                                        if (f.IsConstant)
                                        {
                                            float flow = float.Parse(f.B);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = 0;
                                            }
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam2.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = 0;
                                            }
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam2.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = 0;
                                            }
                                            int pumpNo3 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo3 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo3,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam2.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                        }

                                        statTotalSeconds += 1;

                                        int count5 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count5 > 0)
                                        {
                                            if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count5--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else if (deviceParameter.DO >= f.C)
                                    {
                                        if (f.IsConstant)
                                        {
                                            float flow = float.Parse(f.D);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam2.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam2.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam2.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                        }

                                        int count5 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count5 > 0)
                                        {
                                            if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count5--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                        int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo9 >= 0)
                                        {
                                            PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo9,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case "PH_Feedback_Total"://根据PH反馈控制，执行业务逻辑
                                    if (deviceParameter.PH <= f.A)
                                    {
                                        if (f.IsConstant)
                                        {
                                            float flow = float.Parse(f.B);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam2.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam2.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam2.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                        }

                                        statTotalSeconds += 1;

                                        int count5 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count5 > 0)
                                        {
                                            if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count5--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else if (deviceParameter.PH >= f.C)
                                    {
                                        if (f.IsConstant)
                                        {
                                            float flow = float.Parse(f.D);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam2.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                        }
                                        else if (f.IsPolynomial)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramD > 0 ? offset - paramD : 0;
                                            double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam2.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                        }
                                        else if (f.IsExp)
                                        {
                                            double paramA, paramB, paramC, paramD;
                                            var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                            paramA = array[0];
                                            paramB = array[1];
                                            paramC = array[2];
                                            paramD = array[3];
                                            double offset = statTotalSeconds / 3600;
                                            double diff = offset - paramC > 0 ? offset - paramC : 0;
                                            double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                            if (flow > 0)
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                            }
                                            else
                                            {
                                                deviceParameter.FeedParam2.Feed_PV = 0;
                                            }
                                            int pumpNo9 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                            if (pumpNo9 >= 0)
                                            {
                                                PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                                {
                                                    PumpNo = pumpNo9,
                                                    Pump = pump,
                                                    ControlMode = PumpControlMode.Direct,
                                                    FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                    FlowCapacity = (float)flow
                                                };
                                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                            }

                                            int temp = 1;
                                            if (deviceParameter.FeedParam2.Feed_PV > 0)
                                            {
                                                temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                            }
                                            bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                            int count2 = flag2 == true ? 1 : temp;
                                            while (count2 > 0)
                                            {
                                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }

                                                count2--;
                                                Thread.Sleep(1000);
                                            }
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                        }

                                        statTotalSeconds += 1;

                                        int count5 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        while (count5 > 0)
                                        {
                                            if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            count5--;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                        int pumpNo8 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo8 >= 0)
                                        {
                                            PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo8,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                                FlowCapacity = 0
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case "Quantitative":
                                    if (f.A <= 0 || QuantitativeFinish)
                                    {
                                        continue;
                                    }
                                    float.TryParse(f.B, out var b1);
                                    int pumpNo10 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo10 >= 0)
                                    {
                                        PeristalticPumpControlParam param10 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo10,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = b1,
                                            FlowCapacity = f.A
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param10);
                                    }
                                    QuantitativeFinish = true;

                                    int count10 = Convert.ToInt32(Math.Ceiling(f.A / b1 * 3600));
                                    int index10 = 0;
                                    while (index10 < count10)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }
                                        index10 += 1;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case "Cycle":
                                    try
                                    {
                                        DateTime startTime = DateTime.Now;
                                        double totalMinutes = 0;

                                        if (f.A <= 0 || !float.TryParse(f.B, out var paramB) || paramB <= 0 || f.C <= 0 || !float.TryParse(f.D, out var paramD) || paramD <= 0)
                                        {
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                            continue;
                                        }
                                        int pumpNo11 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                        if (pumpNo11 < 0)
                                        {
                                            deviceParameter.FeedParam2.Feed_PV = 0;
                                            continue;
                                        }
                                        deviceParameter.FeedParam2.Feed_PV = f.C;
                                        if (pumpNo11 >= 0)
                                        {
                                            PeristalticPumpControlParam param11 = new PeristalticPumpControlParam()
                                            {
                                                PumpNo = pumpNo11,
                                                Pump = pump,
                                                ControlMode = PumpControlMode.Direct,
                                                FlowSpeed = deviceParameter.FeedParam2.Feed_PV,
                                                FlowCapacity = paramD
                                            };
                                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param11);
                                        }

                                        int count11 = Convert.ToInt32(Math.Ceiling(paramD / deviceParameter.FeedParam2.Feed_PV * 3600));
                                        while (count11 > 0)
                                        {
                                            if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                e.Result = deviceParameter.Name;
                                                return;
                                            }
                                            count11--;
                                            Thread.Sleep(1000);
                                        }
                                        totalMinutes = (DateTime.Now - startTime).TotalMinutes;

                                        deviceParameter.FeedParam2.Feed_PV = 0;

                                        double diff = f.A - totalMinutes;
                                        if (diff > 0)
                                        {
                                            int count12 = Convert.ToInt32(diff * 60);
                                            int index12 = 0;
                                            while (index12 < count12)
                                            {
                                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                                {
                                                    return;
                                                }
                                                index12 += 1;
                                                Thread.Sleep(1000);
                                            }
                                        }

                                        int count7 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                        int index7 = 0;
                                        while (index7 < count7)
                                        {
                                            if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                            {
                                                return;
                                            }

                                            index7 += 1;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogHelper.Debug(string.Format("周期补料调整失败，错误信息：{0}", ex.Message));
                                        Thread.Sleep(AppSession.Interval * 1000);
                                    }
                                    break;
                                default:
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                    int pumpNo7 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo7 >= 0)
                                    {
                                        PeristalticPumpControlParam param7 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo7,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = 0
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param7);
                                    }
                                    Thread.Sleep(1000);
                                    break;
                            }

                            lastInfoType = f.InfoType;
                        }

                        endTime = feedGradientInfos[feedGradientInfos.Count - 1].EndTime;
                        timeOffset = Math.Round((DateTime.Now - beginTime).TotalMinutes, 2);
                    }
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam2.IsControling = false;
                        deviceParameter.FeedParam2.Feed_PV = 0;
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料指数完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //DO反馈 速度
            else if (feedMode == FeedControlMode.DO_stat_Speed)
            {
                dicFeed2Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    double totalSeconds = 0;
                    var worker = (BackgroundWorker)s;
                    while (true)
                    {
                        try
                        {
                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            var flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }
                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "DO_Feedback".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在DO_stat(速度)策略", deviceParameter.Name));
                                return;
                            }

                            if (deviceParameter.DO <= f.A)
                            {
                                if (f.IsConstant)
                                {
                                    deviceParameter.FeedParam2.Feed_PV = float.Parse(f.B) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.B);
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                    deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                    deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }

                                totalSeconds += 1;

                                int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count > 0)
                                {
                                    if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }

                                    count--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else if (deviceParameter.DO >= f.C)
                            {
                                if (f.IsConstant)
                                {
                                    deviceParameter.FeedParam2.Feed_PV = float.Parse(f.D) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.D);
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                    deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                    deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }

                                totalSeconds += 1;

                                int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count > 0)
                                {
                                    if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }

                                    count--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                deviceParameter.FeedParam2.Feed_PV = 0;
                                int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                if (pumpNo >= 0)
                                {
                                    PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                }
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料DO反馈调整失败，错误信息：{0}", ex.Message));
                        }
                    }
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam2.IsControling = false;
                        deviceParameter.FeedParam2.Feed_PV = 0;
                    }

                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料Do反馈完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //PH反馈 速度
            else if (feedMode == FeedControlMode.pH_stat_Speed)
            {
                dicFeed2Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    double totalSeconds = 0;
                    while (true)
                    {
                        try
                        {
                            var worker = (BackgroundWorker)s;
                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            var flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }
                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "PH_Feedback".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在PH反馈策略", deviceParameter.Name));
                                return;
                            }

                            if (deviceParameter.PH <= f.A)
                            {
                                if (f.IsConstant)
                                {
                                    deviceParameter.FeedParam2.Feed_PV = float.Parse(f.B) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.B);
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                    deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                    deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }

                                totalSeconds += 1;

                                int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count > 0)
                                {
                                    if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }
                                    count--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else if (deviceParameter.PH >= f.C)
                            {
                                if (f.IsConstant)
                                {
                                    deviceParameter.FeedParam2.Feed_PV = float.Parse(f.D) >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : float.Parse(f.D);
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flowRate = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);

                                    deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flowRate = Math.Round(paramA * Math.Exp(paramB * diff), 2);

                                    deviceParameter.FeedParam2.Feed_PV = (float)flowRate >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)flowRate;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }
                                }

                                totalSeconds += 1;

                                int count = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count > 0)
                                {
                                    if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }
                                    count--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                deviceParameter.FeedParam2.Feed_PV = 0;
                                int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                if (pumpNo >= 0)
                                {
                                    PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                }
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料PH反馈调整失败，错误信息：{0}", ex.Message));
                        }
                        finally
                        {
                            Thread.Sleep(1000);
                        }
                    }
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam2.IsControling = false;
                        deviceParameter.FeedParam2.Feed_PV = 0;
                    }

                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料PH反馈完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //DO反馈 总量
            else if (feedMode == FeedControlMode.DO_stat_Volume)
            {
                dicFeed2Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    double totalSeconds = 0;
                    while (true)
                    {
                        try
                        {
                            var worker = (BackgroundWorker)s;
                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            var flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }
                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "DO_Feedback_Total".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在DO反馈总量策略", deviceParameter.Name));
                                return;
                            }

                            if (deviceParameter.DO <= f.A)
                            {
                                if (f.IsConstant)
                                {
                                    float flow = float.Parse(f.B);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam2.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam2.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam2.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                        totalSeconds += 1;
                                    }
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                }

                                totalSeconds += 1;

                                int count1 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count1 > 0)
                                {
                                    if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }

                                    count1--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else if (deviceParameter.DO >= f.C)
                            {
                                if (f.IsConstant)
                                {
                                    float flow = float.Parse(f.D);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam2.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam2.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam2.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                }

                                totalSeconds += 1;

                                int count1 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count1 > 0)
                                {
                                    if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }

                                    count1--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                deviceParameter.FeedParam2.Feed_PV = 0;
                                int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                if (pumpNo >= 0)
                                {
                                    PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                }
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料DO反馈总量调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }

                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam2.IsControling = false;
                        deviceParameter.FeedParam2.Feed_PV = 0;
                    }

                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料Do反馈总量完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //PH反馈 总量
            else if (feedMode == FeedControlMode.pH_stat_Volume)
            {
                dicFeed2Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    double totalSeconds = 0;
                    var worker = (BackgroundWorker)s;
                    while (true)
                    {
                        try
                        {
                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            var flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }
                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "PH_Feedback_Total".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在PH反馈总量策略", deviceParameter.Name));
                                return;
                            }

                            if (deviceParameter.PH <= f.A)
                            {
                                if (f.IsConstant)
                                {
                                    float flow = float.Parse(f.B);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam2.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam2.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                        totalSeconds += 1;
                                    }
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.B.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam2.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                }

                                totalSeconds += 1;

                                int count1 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count1 > 0)
                                {
                                    if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }

                                    count1--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else if (deviceParameter.PH >= f.C)
                            {
                                if (f.IsConstant)
                                {
                                    float flow = float.Parse(f.D);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam2.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                        totalSeconds += 1;
                                    }
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                }
                                else if (f.IsPolynomial)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramD > 0 ? offset - paramD : 0;
                                    double flow = Math.Round(paramA * Math.Pow(diff, 2) + paramB * diff + paramC, 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam2.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                }
                                else if (f.IsExp)
                                {
                                    double paramA, paramB, paramC, paramD;
                                    var array = f.D.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray();
                                    paramA = array[0];
                                    paramB = array[1];
                                    paramC = array[2];
                                    paramD = array[3];
                                    double offset = totalSeconds / 3600;
                                    double diff = offset - paramC > 0 ? offset - paramC : 0;
                                    double flow = Math.Round(paramA * Math.Exp(paramB * diff), 2);
                                    if (flow > 0)
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = AppSession.DefaultPumpFlowRate;
                                    }
                                    else
                                    {
                                        deviceParameter.FeedParam2.Feed_PV = 0;
                                    }
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                            FlowCapacity = (float)flow
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                                    }

                                    int temp = 1;
                                    if (deviceParameter.FeedParam2.Feed_PV > 0)
                                    {
                                        temp = Convert.ToInt32(Math.Ceiling(flow / deviceParameter.FeedParam2.Feed_PV * 3600));
                                    }
                                    bool flag2 = Convert.ToBoolean(VarConfig.GetValue("IsSimulation")?.ToString());
                                    int count = flag2 == true ? 1 : temp;
                                    while (count > 0)
                                    {
                                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                        {
                                            return;
                                        }

                                        count--;
                                        Thread.Sleep(1000);
                                    }
                                    deviceParameter.FeedParam2.Feed_PV = 0;
                                }

                                totalSeconds += 1;

                                int count1 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                                while (count1 > 0)
                                {
                                    if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        return;
                                    }

                                    count1--;
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                            {
                                deviceParameter.FeedParam2.Feed_PV = 0;
                                int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                if (pumpNo >= 0)
                                {
                                    PeristalticPumpControlParam param8 = new PeristalticPumpControlParam()
                                    {
                                        PumpNo = pumpNo,
                                        Pump = pump,
                                        ControlMode = PumpControlMode.Direct,
                                        FlowSpeed = (float)deviceParameter.FeedParam2.Feed_PV,
                                        FlowCapacity = 0
                                    };
                                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param8);
                                }
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("补料PH反馈调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam2.IsControling = false;
                        deviceParameter.FeedParam2.Feed_PV = 0;
                    }

                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料PH反馈完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //定量
            else if (feedMode == FeedControlMode.Quantitative)
            {
                dicFeed2Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    if (deviceParameter.FeedParam2.Feed_Total <= 0)
                    {
                        return;
                    }

                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                    if (pumpNo >= 0)
                    {
                        PeristalticPumpControlParam param4 = new PeristalticPumpControlParam()
                        {
                            PumpNo = pumpNo,
                            Pump = pump,
                            ControlMode = PumpControlMode.Direct,
                            FlowSpeed = deviceParameter.FeedParam2.Feed_PV,
                            FlowCapacity = deviceParameter.FeedParam2.Feed_Total
                        };
                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param4);
                    }

                    int count2 = 0;
                    if (deviceParameter.FeedParam2.Feed_PV > 0)
                    {
                        count2 = Convert.ToInt32(Math.Ceiling(deviceParameter.FeedParam2.Feed_Total / deviceParameter.FeedParam2.Feed_PV * 3600));
                    }
                    int index2 = 0;
                    while (index2 < count2)
                    {
                        if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                        {
                            return;
                        }
                        index2 += 1;
                        Thread.Sleep(1000);
                    }
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam2.IsControling = false;
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料定量完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }
            //周期
            else if (feedMode == FeedControlMode.Cycle)
            {
                dicFeed2Worker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                dicFeed2Worker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                    e.Result = deviceParameter.Name;
                    var worker = (BackgroundWorker)s;
                    while (true)
                    {
                        try
                        {
                            DateTime startTime = DateTime.Now;
                            double totalMinutes = 0;

                            if (worker.CancellationPending)
                            {
                                return;
                            }

                            var allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
                            var flag = allDeviceInfos.TryGetValue(deviceParameter.Name, out var feedGradientInfos);
                            if (!flag || feedGradientInfos == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在补料策略", deviceParameter.Name));
                                return;
                            }
                            var f = feedGradientInfos.FindFirst(t => t.InfoType.ToUpper() == "Cycle".ToUpper() && t.Pump == pump);
                            if (f == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在周期补料策略", deviceParameter.Name));
                                return;
                            }
                            if (f.A <= 0 || !float.TryParse(f.B, out var b) || b <= 0 || f.C <= 0 || !float.TryParse(f.D, out var d) || d <= 0)
                            {
                                deviceParameter.FeedParam2.Feed_PV = 0;
                                continue;
                            }
                            int pumpNo11 = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo11 < 0)
                            {
                                deviceParameter.FeedParam2.Feed_PV = 0;
                                continue;
                            }
                            deviceParameter.FeedParam2.Feed_PV = f.C;
                            if (pumpNo11 >= 0)
                            {
                                PeristalticPumpControlParam param11 = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo11,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = deviceParameter.FeedParam2.Feed_PV,
                                    FlowCapacity = d
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param11);
                            }

                            int count11 = 1;
                            if (deviceParameter.FeedParam2.Feed_PV > 0)
                            {
                                count11 = Convert.ToInt32(Math.Ceiling(d / deviceParameter.FeedParam2.Feed_PV * 3600));
                            }
                            while (count11 > 0)
                            {
                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                {
                                    e.Result = deviceParameter.Name;
                                    return;
                                }
                                count11--;
                                Thread.Sleep(1000);
                            }
                            totalMinutes = (DateTime.Now - startTime).TotalMinutes;
                            double diff = f.A - totalMinutes;
                            deviceParameter.FeedParam2.Feed_PV = 0;
                            if (diff > 0)
                            {
                                int count12 = Convert.ToInt32(diff * 60);
                                while (count12 > 0)
                                {
                                    if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                    {
                                        e.Result = deviceParameter.Name;
                                        return;
                                    }
                                    count12--;
                                    Thread.Sleep(1000);
                                }
                            }

                            int count2 = f.TriggerInterval < 1 ? 1 : f.TriggerInterval / 1;
                            while (count2 > 0)
                            {
                                if (dicFeed2Worker[deviceParameter.Name].CancellationPending)
                                {
                                    return;
                                }

                                count2--;
                                Thread.Sleep(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("周期补料调整失败，错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter != null)
                    {
                        deviceParameter.FeedParam2.IsControling = false;
                        deviceParameter.FeedParam2.Feed_PV = 0;
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                            if (pumpNo >= 0)
                            {
                                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                {
                                    PumpNo = pumpNo,
                                    Pump = pump,
                                    ControlMode = PumpControlMode.Direct,
                                    FlowSpeed = 0,
                                    FlowCapacity = 0
                                };
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(e.Result?.ToString(), param);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("Feed1补料周期完成事件出错");
                            return;
                        }
                    });

                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                };
                dicFeed2Worker[CurrentDeviceParameter.Name].RunWorkerAsync();
            }

            else if (feedMode == FeedControlMode.Probe)
            {
                var param = ProbingParameterManager.GetInstance().ProbeCol.FindFirst(t => t.DeviceID == CurrentDeviceParameter.Name);
                dicFeed2Probe[CurrentDeviceParameter.Name].InitProbParam(param);
                dicFeed2Probe[CurrentDeviceParameter.Name].StartCtrl();
            }
        });

        public DelegateCommand AFRunCommand => new(() =>
        {
            if (dicAFWorker[CurrentDeviceParameter.Name] != null && dicAFWorker[CurrentDeviceParameter.Name].IsBusy)
            {
                dicAFWorker[CurrentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            if (CurrentDeviceParameter.AFParam.IsControling)
            {
                try
                {
                    int pumpNo = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, PeristalticPump.AFPump);
                    if (pumpNo >= 0)
                    {
                        PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                        {
                            PumpNo = pumpNo,
                            Pump = PeristalticPump.AFPump,
                            ControlMode = PumpControlMode.Direct,
                            FlowSpeed = (float)CurrentDeviceParameter.AFParam.AF_PV >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)CurrentDeviceParameter.AFParam.AF_PV,
                            FlowCapacity = 1000
                        };
                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(CurrentDeviceParameter.Name, param);
                        dicAFSP[CurrentDeviceParameter.Name] = CurrentDeviceParameter.AFParam.AF_PV;
                    }

                    dicAFWorker[CurrentDeviceParameter.Name] = new BackgroundWorker();
                    dicAFWorker[CurrentDeviceParameter.Name].WorkerSupportsCancellation = true;
                    dicAFWorker[CurrentDeviceParameter.Name].WorkerReportsProgress = true;
                    dicAFWorker[CurrentDeviceParameter.Name].DoWork += (s, e) =>
                    {
                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                        BackgroundWorker worker = s as BackgroundWorker;
                        while (true)
                        {
                            if (worker.CancellationPending)
                            {
                                e.Result = deviceParameter.Name;
                                return;
                            }
                            try
                            {
                                if (dicAFSP[deviceParameter.Name] != deviceParameter.AFParam.AF_PV)
                                {
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, PeristalticPump.AFPump);
                                    if (pumpNo >= 0)
                                    {
                                        PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = PeristalticPump.AFPump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.AFParam.AF_PV >= Const.MaxPumpFlowRate ? Const.MaxPumpFlowRate : (float)deviceParameter.AFParam.AF_PV,
                                            FlowCapacity = 100
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, param);
                                        dicAFSP[deviceParameter.Name] = deviceParameter.AFParam.AF_PV;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("消泡泵控制失败" + ex.Message);
                            }
                            Thread.Sleep(1000);
                        }
                    };
                    dicAFWorker[CurrentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                    {
                        var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == e.Result?.ToString());
                        dicAFWorker[deviceParameter?.Name].Dispose();
                        dicAFWorker[deviceParameter?.Name] = null;
                    };
                    dicAFWorker[CurrentDeviceParameter.Name].RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("消泡泵控制失败" + ex.Message);
                }
            }
            else
            {
                try
                {
                    int pumpNo = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, PeristalticPump.AFPump);
                    if (pumpNo >= 0)
                    {
                        PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                        {
                            PumpNo = pumpNo,
                            ControlMode = PumpControlMode.Direct,
                            FlowSpeed = 0,
                            FlowCapacity = 0
                        };
                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(CurrentDeviceParameter.Name, param);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error("消泡泵控制失败" + ex.Message);
                }
            }
        });

        public DelegateCommand<DeviceParameter> AFSettingCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }
            int pumpNo = PumpMFCUtil.GetPumpIndex(currentDeviceParameter.Name, PeristalticPump.AFPump);
            if (pumpNo >= 0)
            {
                DefoamingParam param = new DefoamingParam()
                {
                    PumpNo = pumpNo,
                    SensorEnable = currentDeviceParameter.AFParam.AutoDefoaming,
                    Cycle = Convert.ToInt32(CurrentDeviceParameter.AFParam.Cycle),
                    TimeRatio = currentDeviceParameter.AFParam.DutyCycle,
                    FlowSpeed = currentDeviceParameter.AFParam.AF_PV
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetAutoDefoamingSetting(currentDeviceParameter.Name, param);
            }
        });

        public DelegateCommand<string> TimeSeriesCommand => new((string param) =>
        {
            Enum.TryParse(typeof(ExperimentParameter), param, out var result);
            DialogParameters keyValuePairs = new DialogParameters();
            switch ((ExperimentParameter)result)
            {
                case ExperimentParameter.DO://溶氧
                    var doTimeWorker = dicDOTimeWorker[CurrentDeviceParameter.Name];
                    keyValuePairs = new DialogParameters()
            {

                {nameof(TimeSeries), CurrentDeviceParameter.DOParam.TimeSeries},
                       {nameof(ExperimentParameter),ExperimentParameter.DO },
                        {"Name",CurrentDeviceParameter.Name }
            };
                    DialogHostService.ShowOnce(nameof(TimeSeriesView), keyValuePairs, callback =>
                    {
                        if (callback.Result != ButtonResult.OK)
                        {
                            return;
                        }
                        CurrentDeviceParameter.DOParam.TimeSeries = callback.Parameters.GetValue<TimeSeries>(nameof(TimeSeries)).Clone() as TimeSeries;
                    });
                    break;
                case ExperimentParameter.PH://PH
                    keyValuePairs = new DialogParameters()
            {

                {nameof(TimeSeries), CurrentDeviceParameter.PHParam.TimeSeries},
                 {nameof(ExperimentParameter),ExperimentParameter.PH },
                        {"Name",CurrentDeviceParameter.Name }
            };
                    DialogHostService.ShowOnce(nameof(TimeSeriesView), keyValuePairs, callback =>
                    {
                        if (callback.Result != ButtonResult.OK)
                        {
                            return;
                        }

                        CurrentDeviceParameter.PHParam.TimeSeries = callback.Parameters.GetValue<TimeSeries>(nameof(TimeSeries)).Clone() as TimeSeries;
                    });
                    break;
                case ExperimentParameter.Temp://温度
                    keyValuePairs = new DialogParameters()
            {

                {nameof(TimeSeries), CurrentDeviceParameter.TempParam.TimeSeries},
                 {nameof(ExperimentParameter),ExperimentParameter.Temp },
                        {"Name",CurrentDeviceParameter.Name }
            };
                    DialogHostService.ShowOnce(nameof(TimeSeriesView), keyValuePairs, callback =>
                    {
                        if (callback.Result != ButtonResult.OK)
                        {
                            return;
                        }
                        CurrentDeviceParameter.TempParam.TimeSeries = callback.Parameters.GetValue<TimeSeries>(nameof(TimeSeries)).Clone() as TimeSeries;
                    });
                    break;
            }
        });

        public DelegateCommand FeedCycleCommand => new(() =>
        {
            DialogParameters keyValuePairs = new DialogParameters()
            {
                {nameof(DeviceParameter),CurrentDeviceParameter.Name }
            };

            DialogHostService.ShowOnce(nameof(FeedCycleView), keyValuePairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });

        public DelegateCommand<string> FeedSeriesCommand => new((string feedPump) =>
        {
            DialogParameters keyValuePairs = new DialogParameters()
            {
                {"deviceID",CurrentDeviceParameter.Name },
                 {nameof(PeristalticPump), feedPump}
            };

            DialogHostService.ShowOnce(nameof(FeedGradientView), keyValuePairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });

        public DelegateCommand PIDCommand => new(() =>
        {
            DialogHostService.ShowOnce(nameof(PIDView), callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });

        public DelegateCommand<string> FeedStrategyCommand => new((string feedPump) =>
        {
            DialogParameters keyValuePairs = new DialogParameters()
            {
                {"deviceID",CurrentDeviceParameter.Name },
                {nameof(PeristalticPump), feedPump}
            };

            DialogHostService.ShowOnce(nameof(FeedStrategyView), keyValuePairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });

        public DelegateCommand<string> ResetFlowCommand => new((string str) =>
        {
            var array = str.Split(",");
            if (!Enum.TryParse(typeof(PeristalticPump), array[0], true, out var pump))
            {
                return;
            }
            int index = 1;
            if (array.Length > 1)
            {
                int.TryParse(array[1], out index);
            }
            int pumpIndex = PumpMFCUtil.GetPumpIndex(CurrentDeviceParameter.Name, (PeristalticPump)pump, index);
            if (pumpIndex >= 0)
            {
                CommandWrapper.SetResetFlowCapacity(CurrentDeviceParameter.Name, ClearModule.Pump, pumpIndex);
            }
        });

        public DelegateCommand CopyCommand => new(() =>
        {
            DialogHostService.ShowOnce(nameof(ChooseReactorView), callback =>
            {
                if (callback.Result != ButtonResult.OK) return;
                var list = callback.Parameters.GetValue<List<Device>>("Reactors");
                var copyDevice = CurrentDeviceParameter.Clone() as DeviceParameter;
                foreach (var item in list)
                {
                    DeviceParameter deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == item.Name);
                    if (deviceParameter == null || deviceParameter.Name == CurrentDeviceParameter.Name)
                    {
                        continue;
                    }
                    deviceParameter.DOParam = copyDevice.DOParam.Clone() as DOParam;
                    deviceParameter.AgitParam = copyDevice.AgitParam.Clone() as AgitParam;
                    deviceParameter.AirParam = copyDevice.AirParam.Clone() as GasParam;
                    deviceParameter.O2Param = copyDevice.O2Param.Clone() as GasParam;

                    deviceParameter.PHParam = copyDevice.PHParam.Clone() as PHParam;
                    deviceParameter.AcidParam = copyDevice.AcidParam.Clone() as AcidParam;
                    deviceParameter.BaseParam = copyDevice.BaseParam.Clone() as BaseParam;

                    deviceParameter.TempParam = copyDevice.TempParam.Clone() as TempParam;

                    deviceParameter.FeedParam1 = copyDevice.FeedParam1.Clone() as FeedParam;
                    deviceParameter.FeedParam2 = copyDevice.FeedParam2.Clone() as FeedParam;

                    deviceParameter.AFParam = copyDevice.AFParam.Clone() as AFParam;

                    ControlReactor(deviceParameter);
                }
            });
        });

        public DelegateCommand ProbeCommand => new(() =>
        {
            DialogHostService.ShowOnce(nameof(ProbView), callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });

        public DelegateCommand<string> TestCommand => new((string content) =>
        {
            var array = content.Split(",");
            if (array[0] == "1")
            {
                AppSession.VirtualDO = float.Parse(array[1]);
            }
            else
            {
                AppSession.VirtualpH = float.Parse(array[1]);
            }
        });



        public ReactorViewModel(IContainerProvider provider,
             IDialogHostService dialogHostService) : base(provider, dialogHostService)
        {
            List<DOTimeSeries> pIDInfos = new List<DOTimeSeries>();
            if (File.Exists(FileConst.DORTInfoPath))
            {
                string result = File.ReadAllText(FileConst.DORTInfoPath);
                pIDInfos = CustomApp.JsonHelper.StringToObject<List<DOTimeSeries>>(result);
            }
            DOTS = new ObservableCollection<DOTimeSeries>(pIDInfos);

            pHControlUtils.InitpHinfo();

            System.Timers.Timer experimentTimer = new System.Timers.Timer
            {
                Interval = 500
            };
            experimentTimer.Elapsed += ExperimentTimer_Elapsed;
            //elapsedTime = TimeSpan.Zero;
            experimentTimer.AutoReset = true;
            experimentTimer.Start();

            SelectedDevice = ReactorCol[0];

            //方成  HMI界面初始也加载所有反应器，以便切换
            for (int i = 0; i < ReactorCol.Count; i++)
            {
                Device reactor = ReactorCol[i];
                DeviceParameter device = null;
                if (AnalysisSolution.GetInstance().ReactorCol != null && AnalysisSolution.GetInstance().ReactorCol.Count > 1)
                {
                    device = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == reactor.Name);
                }
                if (device == null)
                {
                    device = new()
                    {
                        Name = reactor.Name,
                        SerialNumber = i + 1
                    };
                }
                DeviceParameterCol.Add(device);

                dicDOTimeWorker.Add(reactor.Name, new BackgroundWorker());
                dicDOWorker.Add(reactor.Name, new BackgroundWorker());
                dicDODelta.Add(reactor.Name, 0);
                dicDOPid.Add(reactor.Name, new QPIDController());
                dicDOAgitPid.Add(reactor.Name, new QPIDController());
                dicDOTempWorker.Add(reactor.Name, new BackgroundWorker());

                dicAgitWorker.Add(reactor.Name, new BackgroundWorker());
                dicAgitSP.Add(reactor.Name, 0f);

                dicAirIndex.Add(reactor.Name, 0);
                dicAirWorker.Add(reactor.Name, new BackgroundWorker());
                dicAirSP.Add(reactor.Name, 0f);

                dicO2Index.Add(reactor.Name, 0);
                dicO2Worker.Add(reactor.Name, new BackgroundWorker());
                dicO2SP.Add(reactor.Name, 0f);

                dicPHTimeWorker.Add(reactor.Name, new BackgroundWorker());
                dicPHWorker.Add(reactor.Name, new BackgroundWorker());
                dicPHDelta.Add(reactor.Name, 0f);
                dicPHPid.Add(reactor.Name, new QPIDController());

                dicAcidWorker.Add(reactor.Name, new BackgroundWorker());
                dicBaseWorker.Add(reactor.Name, new BackgroundWorker());

                dicTempTimeWorker.Add(reactor.Name, new BackgroundWorker());
                dicTempWorker.Add(reactor.Name, new BackgroundWorker());
                dicTempDOWorker.Add(reactor.Name, new BackgroundWorker());

                dicFeed1TimeWorker.Add(reactor.Name, new BackgroundWorker());
                dicFeed1Worker.Add(reactor.Name, new BackgroundWorker());

                dicFeed2TimeWorker.Add(reactor.Name, new BackgroundWorker());
                dicFeed2Worker.Add(reactor.Name, new BackgroundWorker());

                dicAuditWorker.Add(reactor.Name, new BackgroundWorker());

                dicAFWorker.Add(reactor.Name, new BackgroundWorker());

                dicFeed1Probe.Add(reactor.Name, new ProbingController());
                dicFeed2Probe.Add(reactor.Name, new ProbingController());
            }

            CurrentDeviceParameter = DeviceParameterCol[0];

            //方成  HMI界面初始也加载所有反应器的图表数据
            foreach (var item in ReactorCol)
            {
                DeviceExperimentHistoryData data = new DeviceExperimentHistoryData()
                {
                    DeviceName = item.Name
                };
                foreach (var item1 in GraphConfig.GetAllValue().Keys.ToList())
                {

                    ExperimentHistoryData historyData = new ExperimentHistoryData()
                    {
                        //ParamerterName = GraphConfig.GetValue(item1)?.ToString()
                        ParamerterName = item1
                    };
                    data.ExperimentHistoryDatas.Add(historyData);
                }
                GraphDataSourceList.Add(data);
            }

            //获取实时信息
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (s, e) =>
            {
                while (true)
                {
                    try
                    {
                        if (CurrentDeviceParameter == null)
                            continue;

                        if (CurrentDeviceParameter.Name != SelectedDevice.Name)
                        {
                            CurrentDeviceParameter = DeviceParameterCol.FindFirst(t => t.Name == SelectedDevice.Name);
                        }

                        string deviceID = CurrentDeviceParameter.Name;
                        int index = ClockSupervisor.realDatasDic[deviceID].Count - 1;
                        var realTimeParam = ClockSupervisor.realDatasDic[deviceID][index];

                        PropertyMapper.Map(realTimeParam, CurrentDeviceParameter);

                        DeviceExperimentHistoryData dataSource = GraphDataSourceList.Find(t => t.DeviceName == CurrentDeviceParameter.Name);
                        //var time = DateTime.Now;
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

                        foreach (var deviceParameter in DeviceParameterCol)//缓存数据
                        {
                            if (deviceParameter.Name == CurrentDeviceParameter.Name)
                                continue;

                            index = ClockSupervisor.realDatasDic[deviceParameter.Name].Count - 1;
                            realTimeParam = ClockSupervisor.realDatasDic[deviceParameter.Name][index];

                            PropertyMapper.Map(realTimeParam, deviceParameter);

                            //deviceParameter.Temp = realTimeParam.Temp;
                            //deviceParameter.PH = realTimeParam.PH;
                            //deviceParameter.Agit = realTimeParam.Agit;
                            //deviceParameter.DO = realTimeParam.DO;
                            //deviceParameter.WorkStatus = realTimeParam.WorkStatus;
                            //deviceParameter.JarWeight = realTimeParam.JarWeight;
                            //deviceParameter.Bottle1Weight = realTimeParam.Bottle1Weight;
                            //deviceParameter.Bottle2Weight = realTimeParam.Bottle2Weight;
                            //deviceParameter.HasFoam = realTimeParam.HasFoam;
                            //deviceParameter.AlarmBytes = [.. realTimeParam.AlarmBytes];

                            //deviceParameter.Pump1FlowSpeed = realTimeParam.Pump1FlowRate;
                            //deviceParameter.Pump1FlowCapacity = realTimeParam.Pump1FlowCapacity;
                            //deviceParameter.Pump2FlowSpeed = realTimeParam.Pump2FlowRate;
                            //deviceParameter.Pump2FlowCapacity = realTimeParam.Pump2FlowCapacity;
                            //deviceParameter.Pump3FlowSpeed = realTimeParam.Pump3FlowRate;
                            //deviceParameter.Pump3FlowCapacity = realTimeParam.Pump3FlowCapacity;
                            //deviceParameter.Pump4FlowSpeed = realTimeParam.Pump4FlowRate;
                            //deviceParameter.Pump4FlowCapacity = realTimeParam.Pump4FlowCapacity;
                            //deviceParameter.Pump5FlowSpeed = realTimeParam.Pump5FlowRate;
                            //deviceParameter.Pump5FlowCapacity = realTimeParam.Pump5FlowCapacity;
                            //deviceParameter.Pump6FlowSpeed = realTimeParam.Pump6FlowRate;
                            //deviceParameter.Pump6FlowCapacity = realTimeParam.Pump6FlowCapacity;

                            //deviceParameter.AcidFlowSpeed = realTimeParam.AcidFlowSpeed;
                            //deviceParameter.AcidFlowCapacity = realTimeParam.AcidFlowCapacity;
                            //deviceParameter.BaseFlowSpeed = realTimeParam.BaseFlowSpeed;
                            //deviceParameter.BaseFlowCapacity = realTimeParam.BaseFlowCapacity;
                            //deviceParameter.FeedFlowSpeed = realTimeParam.FeedFlowSpeed;
                            //deviceParameter.FeedFlowCapacity = realTimeParam.FeedFlowCapacity;
                            //deviceParameter.AFFlowSpeed = realTimeParam.AFFlowSpeed;
                            //deviceParameter.AFFlowCapacity = realTimeParam.AFFlowCapacity;

                            //deviceParameter.MFC1FlowSpeed = realTimeParam.MFC1FlowRate;
                            //deviceParameter.MFC1FlowCapacity = realTimeParam.MFC1FlowCapacity;
                            //deviceParameter.MFC2FlowSpeed = realTimeParam.MFC2FlowRate;
                            //deviceParameter.MFC2FlowCapacity = realTimeParam.MFC2FlowCapacity;
                            //deviceParameter.MFC3FlowSpeed = realTimeParam.MFC3FlowRate;
                            //deviceParameter.MFC3FlowCapacity = realTimeParam.MFC3FlowCapacity;
                            //deviceParameter.MFC4FlowSpeed = realTimeParam.MFC4FlowRate;
                            //deviceParameter.MFC4FlowCapacity = realTimeParam.MFC4FlowCapacity;
                            //deviceParameter.MFC5FlowSpeed = realTimeParam.MFC5FlowRate;
                            //deviceParameter.MFC5FlowCapacity = realTimeParam.MFC5FlowCapacity;

                            //deviceParameter.AirFlowSpeed = realTimeParam.AirFlowSpeed;
                            //deviceParameter.AirFlowCapacity = realTimeParam.AirFlowCapacity;
                            //deviceParameter.O2FlowSpeed = realTimeParam.O2FlowSpeed;
                            //deviceParameter.O2FlowCapacity = realTimeParam.O2FlowCapacity;
                            //deviceParameter.CO2FlowSpeed = realTimeParam.CO2FlowSpeed;
                            //deviceParameter.CO2FlowCapacity = realTimeParam.CO2FlowCapacity;
                            //deviceParameter.N2FlowSpeed = realTimeParam.N2FlowSpeed;
                            //deviceParameter.N2FlowCapacity = realTimeParam.N2FlowCapacity;
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = $"{ex.Message}\r\n{ex.StackTrace}";
                        LogHelper.Error(msg);
                    }
                    finally
                    {
                        Thread.Sleep(AppSession.Interval * 1000);
                    }
                }
            };
            worker.RunWorkerAsync();

            //获取泵和MFC的配置&给前端发送图表数据
            BackgroundWorker background = new BackgroundWorker();
            background.DoWork += (s, e) =>
            {
                Type type = CurrentDeviceParameter.PumpMFCSetting.GetType();
                PropertyInfo[] properties = type.GetProperties();

                while (true)
                {
                    if (isWindowOpen)
                    {
                        DeviceExperimentHistoryData dataSource = GraphDataSourceList.Find(t => t.DeviceName == CurrentDeviceParameter.Name);
                        AnalysisSolution.GetInstance().EventPublisher.PublishRealTimeData(dataSource);
                    }

                    try
                    {
                        var dictionary = PumpMFCConfig.GetValue(CurrentDeviceParameter.Name);
                        if (dictionary != null)
                        {

                            for (int i = 1; i < 7; i++)
                            {
                                foreach (PropertyInfo prop in properties.Where(t => t.CanWrite && t.CanRead))
                                {
                                    if (prop.Name == $"Pump{i}")
                                    {
                                        var temp = dictionary[$"Pump{i}"];
                                        if (Enum.TryParse(typeof(PeristalticPump), temp, out var result))
                                        {
                                            prop.SetValue(CurrentDeviceParameter.PumpMFCSetting, (PeristalticPump)result);
                                        }
                                    }
                                }
                            }

                            for (int i = 1; i < 5; i++)
                            {
                                foreach (PropertyInfo prop in properties.Where(t => t.CanWrite && t.CanRead))
                                {
                                    if (prop.Name == $"MFC{i}")
                                    {
                                        var temp = dictionary[$"MFC{i}"];
                                        if (Enum.TryParse(typeof(GasType), temp, out var result))
                                        {
                                            prop.SetValue(CurrentDeviceParameter.PumpMFCSetting, (GasType)result);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    Thread.Sleep(1000);

                    if (DateTime.Now.Second % 15 == 0)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
            };
            background.RunWorkerAsync();

            #region 获取当前设备连接状态
            reconnectWorker = new BackgroundWorker();
            reconnectWorker.WorkerReportsProgress = true;
            reconnectWorker.WorkerSupportsCancellation = true;
            reconnectWorker.DoWork += ((sender, e) =>
            {
                var worker = sender as BackgroundWorker;
                while (true)
                {
                    while (CurrentDeviceParameter != null)
                    {
                        try
                        {
                            if (worker.CancellationPending)
                            {
                                //e.Cancel = true;
                                return;
                            }
                            Pipe pipe = PortManager.GetInstance().FindSendPipe(CurrentDeviceParameter.Name);
                            Device device = DeviceManager.GetInstance().Devices.FindFirst(t => t.Name == CurrentDeviceParameter.Name);
                            if (pipe == null)
                            {
                                device.ReactorStatus = CurrentDeviceParameter.ReactorStatus = ReactorStatus.DisConnected;
                            }
                            else
                            {
                                pipe.autoConnect = pipe.autoReConnect = true;
                                CurrentDeviceParameter.Ip = (pipe.Bus as Fpi.Communication.Buses.TcpClientSocket)?.InstanceName.Substring(0, Convert.ToInt32((pipe.Bus as Fpi.Communication.Buses.TcpClientSocket)?.InstanceName.IndexOf(":")));
                                if (InstrumentSolution.GetInstance().IsSimulation)
                                {
                                    device.ReactorStatus = CurrentDeviceParameter.ReactorStatus = ReactorStatus.Simulated;
                                }
                                else
                                {
                                    device.ReactorStatus = CurrentDeviceParameter.ReactorStatus = pipe.Connected == true ? ReactorStatus.Connected : ReactorStatus.DisConnected;
                                    //if (!pipe.Connected)
                                    //{
                                    //    Task.Run(() => { pipe.Open(); });
                                    //}
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("轮询反应器连接状态" + ex.Message);
                        }
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(1000);
                }
            });
            reconnectWorker.RunWorkerAsync();
            #endregion

            AnalysisSolution.GetInstance().EventPublisher.TimeSeriesSended -= EventPublisher_TimeSeriesSended;
            AnalysisSolution.GetInstance().EventPublisher.TimeSeriesSended += EventPublisher_TimeSeriesSended;

            #region 记录操作日志
            foreach (var item in dicAuditWorker.Keys)
            {
                dicAuditWorker[item] = new BackgroundWorker();
                dicAuditWorker[item].WorkerSupportsCancellation = true;
                dicAuditWorker[item].WorkerReportsProgress = true;
                dicAuditWorker[item].DoWork += (s, e) =>
                {
                    var deviceParameter = DeviceParameterCol.FindFirst(t => t.Name == item);

                    var lastDevice = deviceParameter.Clone() as DeviceParameter;
                    BackgroundWorker backgroundWorker = s as BackgroundWorker;

                    while (true)
                    {
                        if (backgroundWorker.CancellationPending)
                        {
                            //e.Cancel = true;
                            e.Result = deviceParameter.Name;
                            return;
                        }
                        try
                        {
                            RD3Device device = AppSession.RunningDevices.Find(t => t.Name == deviceParameter.Name && t.Status == "Running");
                            if (device == null)
                            {
                                lastDevice = deviceParameter.Clone() as DeviceParameter;
                                Thread.Sleep(1000);
                                continue;
                            }

                            #region 溶氧 
                            if (deviceParameter.DOParam.ControlStrategy != lastDevice.DOParam.ControlStrategy)
                            {
                                Task.Run(() =>
                                {
                                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                    string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("DO:控制策略从{0}变更为", EnumUtil.GetEnumDescription(lastDevice.DOParam.ControlStrategy), EnumUtil.GetEnumDescription(deviceParameter.DOParam.ControlStrategy));
                                    sb.AppendLine(content);
                                    File.AppendAllText(fileNme, sb.ToString());
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (deviceParameter.DOParam.DO_PV != lastDevice.DOParam.DO_PV)
                            {
                                Task.Run(() =>
                                {
                                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                    string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("DO预设值从{0}变更为{1}", lastDevice.DOParam.DO_PV, deviceParameter.DOParam.DO_PV);
                                    sb.AppendLine(content);
                                    File.AppendAllText(fileNme, sb.ToString());
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (!deviceParameter.DOParam.IsControling && lastDevice.DOParam.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                    string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("DO控制模式从{0}变更为{1}", "自动", "手动");
                                    sb.AppendLine(content);
                                    File.AppendAllText(fileNme, sb.ToString());
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (deviceParameter.DOParam.IsControling && !lastDevice.DOParam.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                    string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("DO控制模式从{0}变更为{1}", "手动", "自动");
                                    sb.AppendLine(content);
                                    File.AppendAllText(fileNme, sb.ToString());
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (!deviceParameter.DOParam.IsControling)
                            {
                                if (deviceParameter.AgitParam.Agit_PV != lastDevice.AgitParam.Agit_PV)
                                {
                                    Task.Run(() =>
                                    {
                                        string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                        string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                        StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                        string content = string.Format("转速预设值从{0}变更为{1}", lastDevice.AgitParam.Agit_PV, deviceParameter.AgitParam.Agit_PV);
                                        sb.AppendLine(content);
                                        File.AppendAllText(fileNme, sb.ToString());
                                        RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                    });
                                }

                                if (deviceParameter.AirParam.FlowSpeed != lastDevice.AirParam.FlowSpeed)
                                {
                                    Task.Run(() =>
                                    {
                                        string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                        string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                        StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                        string content = string.Format("通气预设值从{0}变更为{1}", lastDevice.AirParam.FlowSpeed, deviceParameter.AirParam.FlowSpeed);
                                        sb.AppendLine(content);
                                        File.AppendAllText(fileNme, sb.ToString());
                                        RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                    });
                                }

                                if (deviceParameter.O2Param.FlowSpeed != lastDevice.O2Param.FlowSpeed)
                                {
                                    Task.Run(() =>
                                    {
                                        string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                        string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                        StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                        string content = string.Format("氧气预设值从{0}变更为{1}", lastDevice.O2Param.FlowSpeed, deviceParameter.O2Param.FlowSpeed);
                                        sb.AppendLine(content);
                                        File.AppendAllText(fileNme, sb.ToString());
                                        RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                    });
                                }

                                if (!deviceParameter.AgitParam.IsControling && lastDevice.AgitParam.IsControling)
                                {
                                    Task.Run(() =>
                                    {
                                        string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                        string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                        StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                        string content = string.Format("转速控制模式从{0}变更为{1}", "自动", "手动");
                                        sb.AppendLine(content);
                                        File.AppendAllText(fileNme, sb.ToString());
                                        RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                    });
                                }

                                if (deviceParameter.AgitParam.IsControling && !lastDevice.AgitParam.IsControling)
                                {
                                    Task.Run(() =>
                                    {
                                        string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                        string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                        StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                        string content = string.Format("转速控制模式从{0}变更为{1}", "手动", "自动");
                                        sb.AppendLine(content);
                                        File.AppendAllText(fileNme, sb.ToString());
                                        RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                    });
                                }

                                if (!deviceParameter.AirParam.IsControling && lastDevice.AirParam.IsControling)
                                {
                                    Task.Run(() =>
                                    {
                                        string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                        string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                        StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                        string content = string.Format("通气控制模式从{0}变更为{1}", "自动", "手动");
                                        sb.AppendLine(content);
                                        File.AppendAllText(fileNme, sb.ToString());
                                        RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                    });
                                }

                                if (deviceParameter.AirParam.IsControling && !lastDevice.AirParam.IsControling)
                                {
                                    Task.Run(() =>
                                    {
                                        string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                        string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                        StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                        string content = string.Format("通气控制模式从{0}变更为{1}", "手动", "自动");
                                        sb.AppendLine(content);
                                        File.AppendAllText(fileNme, sb.ToString());
                                        RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                    });
                                }

                                if (deviceParameter.O2Param.IsControling && !lastDevice.O2Param.IsControling)
                                {
                                    Task.Run(() =>
                                    {
                                        string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                        string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                        StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                        string content = string.Format("氧气控制模式从{0}变更为{1}", "手动", "自动");
                                        sb.AppendLine(content);
                                        File.AppendAllText(fileNme, sb.ToString());
                                        RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                    });
                                }

                                if (!deviceParameter.O2Param.IsControling && lastDevice.O2Param.IsControling)
                                {
                                    Task.Run(() =>
                                    {
                                        string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                        string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                        StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                        string content = string.Format("氧气控制模式从{0}变更为{1}", "自动", "手动");
                                        sb.AppendLine(content);
                                        File.AppendAllText(fileNme, sb.ToString());
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
                                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                    string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("PH预设值从{0}变更为{1}", lastDevice.PHParam.PH_PV, deviceParameter.PHParam.PH_PV);
                                    sb.AppendLine(content);
                                    File.AppendAllText(fileNme, sb.ToString());
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (!deviceParameter.PHParam.IsControling && lastDevice.PHParam.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                    string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("PH控制模式从{0}变更为{1}", "自动", "手动");
                                    sb.AppendLine(content);
                                    File.AppendAllText(fileNme, sb.ToString());
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            if (deviceParameter.PHParam.IsControling && !lastDevice.PHParam.IsControling)
                            {
                                Task.Run(() =>
                                {
                                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
                                    string fileNme = dir + "\\" + device.BatchID + "\\Audit.txt";
                                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                                    string content = string.Format("PH控制模式从{0}变更为{1}", "手动", "自动");
                                    sb.AppendLine(content);
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                        File.AppendAllText(fileNme, sb.ToString());
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
                                        File.AppendAllText(fileNme, sb.ToString());
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
                                        File.AppendAllText(fileNme, sb.ToString());
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
                                        File.AppendAllText(fileNme, sb.ToString());
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
                                        File.AppendAllText(fileNme, sb.ToString());
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
                                        File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
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
                                    File.AppendAllText(fileNme, sb.ToString());
                                    RD3SQLHelper.AddAuditRecord(deviceParameter?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                                });
                            }

                            #endregion
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("反应器{0}日志记录异常，异常信息：{1}", e.Result?.ToString(), ex.Message));
                        }

                        lastDevice = deviceParameter.Clone() as DeviceParameter;
                        Thread.Sleep(1000);
                    }
                };
                dicAuditWorker[item].RunWorkerCompleted += (s, e) => { };
                dicAuditWorker[item].RunWorkerAsync();
            }
            #endregion

            #region 配置同步&实验参数保存
            var backWorker = new BackgroundWorker();
            backWorker.WorkerReportsProgress = true;
            backWorker.WorkerSupportsCancellation = true;
            backWorker.DoWork += ((sender, e) =>
            {
                var worker = sender as BackgroundWorker;
                while (true)
                {
                    foreach (var item in DeviceParameterCol)
                    {
                        try
                        {
                            Pipe pipe = PortManager.GetInstance().FindSendPipe(item.Name);
                            if (pipe == null || !pipe.Connected)
                            {
                                continue;
                            }
                            ScreenParam screenParam = new ScreenParam()
                            {
                                PhAuto = item.PHParam.IsControling,
                                PH = item.PHParam.PH_PV,
                                DOAuto = item.DOParam.IsControling,
                                DO = item.DOParam.DO_PV
                            };
                            CommandWrapper.SetSettingSync(item.Name, screenParam);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("反应器{0}同步失败，", item.Name) + ex.Message);
                        }
                    }

                    Task.Run(() =>
                    {
                        AnalysisSolution.GetInstance().Save(DeviceParameterCol);//保存设置
                    });
                    Thread.Sleep(1000);
                }
            });
            backWorker.RunWorkerAsync();
            #endregion

            #region 参数预警
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += ((sender, e) =>
            {
                var worker = sender as BackgroundWorker;
                while (true)
                {
                    Thread.Sleep(5000);

                    bool.TryParse(VarConfig.GetValue("ThresholdAlarm")?.ToString(), out var flag);
                    if (!flag) continue;
                    StringBuilder sb = new StringBuilder();
                    List<string> deviceList = [];
                    foreach (var item in DeviceParameterCol)
                    {
                        try
                        {
                            Pipe pipe = PortManager.GetInstance().FindSendPipe(item.Name);
                            if ((pipe == null || !pipe.Connected) && !InstrumentSolution.GetInstance().IsSimulation)
                            {
                                continue;
                            }

                            if (AppSession.DicTankWeight.ContainsKey(item.Name))
                            {
                                var tuple = AppSession.DicTankWeight[item.Name];
                                double pumpTotalThreshold = 20;
                                double.TryParse(VarConfig.GetValue($"PumpTotalThreshold")?.ToString(), out pumpTotalThreshold);
                                double tankDifference = item.JarWeight - tuple.Item1;
                                double pumpDifference = (item.Pump1FlowCapacity - tuple.Item2) + (item.Pump2FlowCapacity - tuple.Item3) + (item.Pump3FlowCapacity - tuple.Item4) + (item.Pump4FlowCapacity - tuple.Item5);
                                if (Math.Abs(tankDifference - pumpDifference) >= pumpTotalThreshold)
                                {
                                    if (!deviceList.Contains(item.Name))
                                    {
                                        deviceList.Add(item.Name);
                                    }

                                    sb.AppendLine(string.Format("反应器{2}：罐体初始重量：{4}g，变化量为{0}g，泵的累积量为{1}mL，超过阈值：{3}g！", tankDifference, pumpDifference, item.Name, pumpTotalThreshold, tuple.Item1));
                                }
                            }

                            if (double.TryParse(VarConfig.GetValue("TempLowerLimit")?.ToString(), out var tempLowerLimit))
                            {
                                if (item.Temp < tempLowerLimit)
                                {
                                    if (!deviceList.Contains(item.Name))
                                    {
                                        deviceList.Add(item.Name);
                                    }

                                    sb.AppendLine(string.Format("反应器{2}：温度预警下限为{0}℃，当前为{1}℃！", tempLowerLimit, item.Temp, item.Name));
                                    if (item.TempParam.IsControling)
                                    {
                                        TempRunCommand.Execute(item);
                                    }
                                }
                            }
                            if (double.TryParse(VarConfig.GetValue("TempUpperLimit")?.ToString(), out var tempUpperLimit))
                            {
                                if (item.Temp > tempUpperLimit)
                                {
                                    if (!deviceList.Contains(item.Name))
                                    {
                                        deviceList.Add(item.Name);
                                    }

                                    sb.AppendLine(string.Format("反应器{2}：温度预警上限为{0}℃，当前为{1}℃！", tempUpperLimit, item.Temp, item.Name));

                                    if (item.TempParam.IsControling)
                                    {
                                        TempRunCommand.Execute(item);
                                    }
                                }
                            }

                            if (double.TryParse(VarConfig.GetValue("DOLowerLimit")?.ToString(), out var doLowerLimit))
                            {
                                if (item.DO < doLowerLimit)
                                {
                                    if (!deviceList.Contains(item.Name))
                                    {
                                        deviceList.Add(item.Name);
                                    }

                                    sb.AppendLine(string.Format("反应器{2}：溶氧预警下限为{0}%，当前溶氧为{1}%！", doLowerLimit, item.DO, item.Name));
                                }
                            }
                            if (double.TryParse(VarConfig.GetValue("DOUpperLimit")?.ToString(), out var doUpperLimit))
                            {
                                if (item.DO > doUpperLimit)
                                {
                                    if (!deviceList.Contains(item.Name))
                                    {
                                        deviceList.Add(item.Name);
                                    }

                                    sb.AppendLine(string.Format("反应器{2}：溶氧预警上限为{0}%，当前溶氧为{1}%！", doUpperLimit, item.DO, item.Name));
                                }
                            }

                            if (double.TryParse(VarConfig.GetValue("PHLowerLimit")?.ToString(), out var phLowerLimit))
                            {
                                if (item.PH < phLowerLimit)
                                {
                                    if (!deviceList.Contains(item.Name))
                                    {
                                        deviceList.Add(item.Name);
                                    }

                                    sb.AppendLine(string.Format("反应器{2}：pH预警下限为{0}，当前pH为{1}！", phLowerLimit, item.PH, item.Name));
                                }
                            }
                            if (double.TryParse(VarConfig.GetValue("PHUpperLimit")?.ToString(), out var phUpperLimit))
                            {
                                if (item.PH > phUpperLimit)
                                {
                                    if (!deviceList.Contains(item.Name))
                                    {
                                        deviceList.Add(item.Name);
                                    }

                                    sb.AppendLine(string.Format("反应器{2}：pH预警上限为{0}，当前pH为{1}！", phUpperLimit, item.Temp, item.Name));
                                }
                            }

                            Type type = item.GetType();
                            PropertyInfo[] properties = type.GetProperties();
                            for (int i = 1; i < 7; i++)
                            {
                                PropertyInfo propertyInfo = properties.FindFirst(t => t.CanWrite && t.CanRead && t.Name == $"Pump{i}FlowCapacity");
                                if (propertyInfo != null)
                                {
                                    object value = propertyInfo.GetValue(item);
                                    if (value != null)
                                    {
                                        double flowCapacity = Convert.ToDouble(value);
                                        double pumpTotal = 250;
                                        double.TryParse(VarConfig.GetValue($"Pump{i}Total")?.ToString(), out pumpTotal);
                                        double pumpThreshold = 50;
                                        double.TryParse(VarConfig.GetValue($"Pump{i}Threshold")?.ToString(), out pumpThreshold);
                                        if (pumpTotal - flowCapacity <= pumpThreshold)
                                        {
                                            if (!deviceList.Contains(item.Name))
                                            {
                                                deviceList.Add(item.Name);
                                            }

                                            sb.AppendLine(string.Format("反应器{4}：泵{0}总量为{1}mL，当前累积量为{3}mL,已到达警报阈值：{2}mL！", i, pumpTotal, pumpThreshold, flowCapacity, item.Name));
                                        }
                                    }
                                }
                            }
                            if (double.TryParse(VarConfig.GetValue("AFThreshold")?.ToString(), out var afThreshold))
                            {
                                if (item.AFFlowCapacity >= afThreshold)
                                {
                                    if (!deviceList.Contains(item.Name))
                                    {
                                        deviceList.Add(item.Name);
                                    }

                                    sb.AppendLine(string.Format("反应器{2}：消泡剂累积量阈值为{0}mL，当前消泡累积量为{1}mL！", afThreshold, item.AFFlowCapacity, item.Name));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("反应器{0}参数预警失败，", item.Name) + ex.Message);
                        }
                    }
                    if (sb.Length > 0)
                    {
                        Task.Run(() =>
                        {
                            AlarmParam alarmParam = new AlarmParam() { BuzzerEnable = SwitchMode.Open, RedLightEnable = SwitchMode.Open };
                            foreach (var item in deviceList)
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetSoundLightAlarm(item, alarmParam);
                            }
                        });
                        MessageBox.Show(sb.ToString(), "温馨提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    deviceList.Clear();
                }
            });
            backgroundWorker.RunWorkerAsync();
            #endregion

            //关闭前的控制状态恢复,等待仪器连接上再恢复
            var worker1  = new BackgroundWorker();
            worker1.DoWork += (s, e) => 
            {
                Dictionary<string, bool> keyValuePairs = new Dictionary<string, bool>();
                foreach (var item in DeviceParameterCol)
                {
                    keyValuePairs.Add(item.Name, false);
                }
                while (InstrumentSolution.GetInstance().IsSimulation)
                {
                    Thread.Sleep(1000);
                }

                while (keyValuePairs.Values.Count(t => t) < DeviceParameterCol.Count)
                {
                    foreach (var item in DeviceParameterCol)
                    {
                        if (!keyValuePairs[item.Name] && item.ReactorStatus == ReactorStatus.Connected)
                        {
                            ControlReactor(item);
                            keyValuePairs[item.Name] = true;
                        }
                    }
                    Thread.Sleep(1000);
                }
            };
            worker1.RunWorkerAsync();
        }

        private void EventPublisher_TimeSeriesSended(object sender, (ExperimentParameter, TimeSeries) e)
        {
            switch (e.Item1)
            {
                case ExperimentParameter.DO:
                    foreach (var item in DeviceParameterCol)
                    {
                        item.DOParam.TimeSeries = e.Item2.Clone() as TimeSeries;
                    }
                    break;
                case ExperimentParameter.PH:
                    foreach (var item in DeviceParameterCol)
                    {
                        item.PHParam.TimeSeries = e.Item2.Clone() as TimeSeries;
                    }
                    break;
                case ExperimentParameter.Temp:
                    foreach (var item in DeviceParameterCol)
                    {
                        item.TempParam.TimeSeries = e.Item2.Clone() as TimeSeries;
                    }
                    break;
            }
        }

        /// <summary>
        /// 控制反应器的各项指标
        /// </summary>
        /// <param name="deviceParameter"></param>
        private void ControlReactor(DeviceParameter deviceParameter)
        {
            if (deviceParameter.DOParam.IsControling)
            {
                DORunCommand.Execute(deviceParameter);
            }
            else
            {
                if (deviceParameter.AirParam.IsControling)
                {
                    AirRunCommand.Execute(deviceParameter);
                }
                if (deviceParameter.AgitParam.IsControling)
                {
                    AgitRunCommand.Execute(deviceParameter);
                }
                if (deviceParameter.O2Param.IsControling)
                {
                    O2RunCommand.Execute(deviceParameter);
                }
            }
            if (deviceParameter.TempParam.IsControling)
            {
                TempRunCommand.Execute(deviceParameter);
            }
            if (deviceParameter.PHParam.IsControling)
            {
                PHRunCommand.Execute(deviceParameter);
            }
            else
            {
                if (deviceParameter.AcidParam.IsControling)
                {
                    AcidRunCommand.Execute(deviceParameter);
                }
                if (deviceParameter.BaseParam.IsControling)
                {
                    BaseRunCommand.Execute(deviceParameter);
                }
            }

            if (deviceParameter.AFParam.AutoDefoaming)
            {
                AFSettingCommand.Execute(deviceParameter);
            }

            if (deviceParameter.DOParam.ControlMode == ControlMode.TimeSeries)
            {
                DOSPCommand.Execute(deviceParameter);
            }

            if (deviceParameter.PHParam.ControlMode == ControlMode.TimeSeries)
            {
                PHSPCommand.Execute(deviceParameter);
            }

            if (deviceParameter.TempParam.ControlMode == ControlMode.TimeSeries)
            {
                TempSPCommand.Execute(deviceParameter);
            }
        }

        private void ExperimentTimer_Elapsed(object sender, EventArgs e)
        {
            if (AppSession.RunningTimeSpan.Days >= 1)
            {
                // 格式：XX天XX时XX分XX秒
                FormattedTime = $"{AppSession.RunningTimeSpan.Days:00}天{AppSession.RunningTimeSpan.Hours:00}时{AppSession.RunningTimeSpan.Minutes:00}分{AppSession.RunningTimeSpan.Seconds:00}秒";
            }
            else
            {
                // 格式：XX时XX分XX秒
                FormattedTime = $"{AppSession.RunningTimeSpan.Hours:00}时{AppSession.RunningTimeSpan.Minutes:00}分{AppSession.RunningTimeSpan.Seconds:00}秒";
            }
            //FormattedTime = elapsedTime.ToString(@"hh\:mm\:ss");
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            isWindowOpen = false;
        }
        //方成
        //bool runFlag = true;
        public void OnDialogOpened(IDialogParameters parameters)
        {
            //方成 
            var deviceParameter = parameters.GetValue<DeviceParameter>(nameof(DeviceParameter)) as DeviceParameter;

            SelectedDevice = ReactorCol.FindFirst(t => t.Name == deviceParameter.Name);
            CurrentDeviceParameter = DeviceParameterCol.FindFirst(t => t.Name == deviceParameter.Name);

            isWindowOpen = true;

            //DeviceExperimentHistoryData dataSource = GraphDataSourceList.Find(t => t.DeviceName == CurrentDeviceParameter.Name);
            //aggregator.SendMessage(CurrentDeviceParameter.Name, nameof(ReactorView), dataSource);
        }
    }
}
