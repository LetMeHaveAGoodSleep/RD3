using ImTools;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
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
                if (value == 1)
                {
                    #region 批次数据
                    List<RD3Batch> batches = RD3SQLHelper.QueryBatch();
                    BatchCol = new ObservableCollection<RD3Batch>(batches);
                    #endregion
                }
            }
        }

        #region  标定
        BackgroundWorker worker;
        BackgroundWorker worker1;
        BackgroundWorker worker2;

        private SensorType _sensorType = SensorType.PT100;
        public SensorType SensorType
        {
            get => _sensorType;
            set
            {
                if (_sensorType != value)
                {
                    CaliPoint1 = 0;
                    CaliPoint2 = 0;
                }
                if (value == SensorType.DO)
                {
                    CaliPoint1 = 0;
                    CaliPoint2 = 100;
                }

                //如果系数校准和偏置校准且非DO校准，校准2不使能
                if (CalibrateIndex == 0 || CalibrateIndex == 1 && value != SensorType.DO)
                {
                    Calibrate2Enabled = false;
                    Calibrate1Enabled = true;
                }
                else if (CalibrateIndex == 1 && value == SensorType.DO)
                {
                    Calibrate1Enabled = false;
                    Calibrate2Enabled = true;
                }
                else
                {
                    Calibrate1Enabled = true;
                    Calibrate2Enabled = true;
                }
                SetProperty(ref _sensorType, value);
            }
        }

        private float _coefficient;
        public float Coefficient
        {
            get => _coefficient;
            set { SetProperty(ref _coefficient, value); }
        }

        private float _bias;
        public float Bias
        {
            get => _bias;
            set { SetProperty(ref _bias, value); }
        }

        private int _statusCode;
        public int StatusCode
        {
            get => _statusCode;
            set { SetProperty(ref _statusCode, value); }
        }

        private float _caliPoint1;

        public float CaliPoint1
        {
            get => _caliPoint1;
            set
            {
                SetProperty(ref _caliPoint1, value);
            }
        }

        private float _caliPoint2;

        public float CaliPoint2
        {
            get => _caliPoint2;
            set
            {
                SetProperty(ref _caliPoint2, value);
            }
        }

        private bool _calibrate1Enabled = true;

        public bool Calibrate1Enabled
        {
            get => _calibrate1Enabled;
            set
            {
                SetProperty(ref _calibrate1Enabled, value);
            }
        }

        private bool _calibrate2Enabled = true;

        public bool Calibrate2Enabled
        {
            get => _calibrate2Enabled;
            set
            {
                SetProperty(ref _calibrate2Enabled, value);
            }
        }

        private SensorCorrectMode _correctMode = SensorCorrectMode.TwoPointCalibration_A;
        public SensorCorrectMode CorrectMode
        {
            get => _correctMode;
            set { SetProperty(ref _correctMode, value); }
        }

        private int _calibrateIndex = 2;

        public int CalibrateIndex
        {
            get => _calibrateIndex;
            set
            {
                //如果系数校准和偏置校准且非DO校准，校准2不使能
                if (value == 0 || value == 1 && SensorType != SensorType.DO)
                {
                    Calibrate2Enabled = false;
                    Calibrate1Enabled = true;
                }
                else if (value == 1 && SensorType == SensorType.DO)
                {
                    Calibrate1Enabled = false;
                    Calibrate2Enabled = true;
                }
                else
                {
                    Calibrate1Enabled = true;
                    Calibrate2Enabled = true;
                }
                SetProperty(ref _calibrateIndex, value);
            }
        }

        private float _currentValue;

        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                SetProperty(ref _currentValue, value);
            }
        }

        private string _selectedReactor = string.Empty;

        public string SelectedReactor
        {
            get => _selectedReactor;
            set
            {
                var temp = _selectedReactor;
                SetProperty(ref _selectedReactor, value);

                if (temp != value)
                {
                    CaliPoint1 = 0;
                    CaliPoint2 = 0;

                    ResetPumpInfo();

                    UpdatePumpPurpose(value);
                }
            }
        }

        private PumpCorrectParam _pump1Param = new() { PumpIndex = 1 };
        public PumpCorrectParam Pump1Param
        {
            get => _pump1Param;
            set { SetProperty(ref _pump1Param, value); }
        }

        private PumpCorrectParam _pump2Param = new() { PumpIndex = 2 };
        public PumpCorrectParam Pump2Param
        {
            get => _pump2Param;
            set { SetProperty(ref _pump2Param, value); }
        }

        private PumpCorrectParam _pump3Param = new() { PumpIndex = 3 };
        public PumpCorrectParam Pump3Param
        {
            get => _pump3Param;
            set { SetProperty(ref _pump3Param, value); }
        }

        private PumpCorrectParam _pump4Param = new() { PumpIndex = 4 };
        public PumpCorrectParam Pump4Param
        {
            get => _pump4Param;
            set { SetProperty(ref _pump4Param, value); }
        }

        private PumpCorrectParam _pump5Param = new() { PumpIndex = 5 };
        public PumpCorrectParam Pump5Param
        {
            get => _pump5Param;
            set { SetProperty(ref _pump5Param, value); }
        }

        private PumpCorrectParam _pump6Param = new() { PumpIndex = 6 };
        public PumpCorrectParam Pump6Param
        {
            get => _pump6Param;
            set { SetProperty(ref _pump6Param, value); }
        }

        public DelegateCommand<string> CalibrateSensorCommand => new((string code) =>
        {
            if (code == "1")//校准点1
            {
                if (CalibrateIndex == 0)//系数校准
                {
                    SensorCorrectParam param = new SensorCorrectParam()
                    {
                        SensorType = SensorType,
                        CorrectMode = SensorCorrectMode.CoefficientCalibration,
                        CorrectValue = CaliPoint1
                    };
                    InstrumentSolution.GetInstance().CommandWrapper.SetSensorCorrect(SelectedReactor, param);
                }
                else if (CalibrateIndex == 1)//单点校准
                {
                    float calibrateValue = CaliPoint1 - CurrentValue + Bias;
                    SensorCorrectParam param = new SensorCorrectParam()
                    {
                        SensorType = SensorType,
                        CorrectMode = SensorCorrectMode.OffsetCalibration,
                        CorrectValue = calibrateValue
                    };
                    InstrumentSolution.GetInstance().CommandWrapper.SetSensorCorrect(SelectedReactor, param);
                }
                else if (CalibrateIndex == 2)//两点校准
                {
                    SensorCorrectParam param = new SensorCorrectParam()
                    {
                        SensorType = SensorType,
                        CorrectMode = SensorCorrectMode.TwoPointCalibration_A,
                        CorrectValue = CaliPoint1
                    };
                    InstrumentSolution.GetInstance().CommandWrapper.SetSensorCorrect(SelectedReactor, param);
                }
            }
            else if (code == "2") //校准点2
            {
                SensorCorrectParam param = new SensorCorrectParam()
                {
                    SensorType = SensorType,
                    CorrectMode = SensorCorrectMode.TwoPointCalibration_B,
                    CorrectValue = CaliPoint2
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetSensorCorrect(SelectedReactor, param);
            }
        });


        public DelegateCommand<string> CalibratePumpCommand => new((string strParam) =>
        {
            try
            {
                //[0]:功能码 [1]:泵编号
                var array = strParam.Split(',');
                Type type = this.GetType();
                PropertyInfo[] properties = type.GetProperties();
                PumpCorrectParam pumpCorrectParam = new();
                foreach (PropertyInfo prop in properties.Where(t => t.CanWrite && t.CanRead))
                {
                    object value = prop.GetValue(this);
                    if (prop.Name == $"Pump{array[1]}Param")
                    {
                        pumpCorrectParam = (PumpCorrectParam)value;
                        break;
                    }
                }

                int param = Convert.ToInt32(array[0]);
                if (param == 0)
                {
                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpCorrect(SelectedReactor, pumpCorrectParam.PumpIndex, 1, Pump1Param.Coefficient);
                }
                if (param == 1)
                {
                    //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedReactor, (ControlObject)result, SwitchMode.Open);
                    PeristalticPumpControlParam controlParam = new PeristalticPumpControlParam()
                    {
                        PumpNo = pumpCorrectParam.PumpIndex,
                        ControlMode = PumpControlMode.Direct,
                        FlowSpeed = pumpCorrectParam.Speed,
                        FlowCapacity = pumpCorrectParam.TheoreticalVolume
                    };
                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedReactor, controlParam);
                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpCorrect(SelectedReactor, pumpCorrectParam.PumpIndex, 2, pumpCorrectParam.Speed);
                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpCorrect(SelectedReactor, pumpCorrectParam.PumpIndex, 3, pumpCorrectParam.Time / 60);
                }
                else if (param == 3)
                {
                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpCorrect(SelectedReactor, pumpCorrectParam.PumpIndex, 4, pumpCorrectParam.RealVolume);
                }
                else if (param == 255)
                {
                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpCorrect(SelectedReactor, pumpCorrectParam.PumpIndex, 5, 0);
                }
                //add by hdb 写入泵对应关系
            }
            catch (Exception ex)
            {

            }
        });

        public DelegateCommand<object> ChangePumpCommand => new((object o) =>
        {
            try
            {
                SavePumpPurpose(SelectedReactor);
            }
            catch (Exception ex)
            {

            }
        });

        private void ResetPumpInfo()
        {
            for (int i = 1; i < 7; i++)
            {
                Type type = this.GetType();
                PropertyInfo[] properties = type.GetProperties();
                PumpCorrectParam pumpCorrectParam = new();
                foreach (PropertyInfo prop in properties.Where(t => t.CanWrite && t.CanRead))
                {
                    object value = prop.GetValue(this);
                    if (prop.Name == $"Pump{i}Param")
                    {
                        pumpCorrectParam = (PumpCorrectParam)value;
                        pumpCorrectParam.Coefficient = pumpCorrectParam.Speed = pumpCorrectParam.Time = pumpCorrectParam.Weigh = pumpCorrectParam.Density = 0;
                    }
                }
            }

        }

        private void UpdatePumpPurpose(string reactor)
        {
            var dictionary = PumpMFCConfig.GetValue(reactor);
            if (dictionary == null || dictionary.Keys.Count < 1)
            {
                return;
            }

            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 1; i < 7; i++)
            {
                foreach (PropertyInfo prop in properties.Where(t => t.CanWrite && t.CanRead))
                {
                    object value = prop.GetValue(this);
                    if (prop.Name == $"Pump{i}Param")
                    {
                        var pumpCorrectParam = (PumpCorrectParam)value;
                        if (!dictionary.ContainsKey($"Pump{i}"))
                        {
                            pumpCorrectParam.Pump = PeristalticPump.None;
                            break;
                        }
                        pumpCorrectParam.Pump = (PeristalticPump)Enum.Parse(typeof(PeristalticPump), dictionary[$"Pump{i}"]);
                    }
                }
            }

        }

        private void SavePumpPurpose(string reactor)
        {
            var dictionary = PumpMFCConfig.GetValue(reactor);
            if (dictionary == null || dictionary.Keys.Count < 1)
            {
                dictionary = new Dictionary<string, string>();
            }

            Type type = this.GetType();
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 1; i < 7; i++)
            {
                foreach (PropertyInfo prop in properties.Where(t => t.CanWrite && t.CanRead))
                {
                    object value = prop.GetValue(this);
                    if (prop.Name == $"Pump{i}Param")
                    {
                        var pumpCorrectParam = (PumpCorrectParam)value;
                        dictionary[$"Pump{i}"] = pumpCorrectParam.Pump.ToString();
                    }
                }
            }
            PumpMFCConfig.SetValue(SelectedReactor, dictionary);

        }
        #endregion

        #region 批次数据
        ObservableCollection<RD3Batch> _batchCol = new ObservableCollection<RD3Batch>();
        public ObservableCollection<RD3Batch> BatchCol
        {
            get { return _batchCol; }
            set { SetProperty(ref _batchCol, value); }
        }

        public DelegateCommand<object> DeleteBatchCommand => new((object o) =>
        {
            var list = o as List<RD3Batch>;
            foreach (var item in list)
            {
                try
                {
                    BatchCol.Remove(item);
                    RD3SQLHelper.DeleteBatch(item);
                }
                catch (Exception ex)
                { }

            }
        });

        public DelegateCommand<object> CompareBatchCommand => new((object o) =>
        {
            DialogParameters pairs = new DialogParameters
                {
                    { "Batches", o },
                };
            DialogHostService.ShowOnce(nameof(CompareBatchView), pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });

        /// <summary>
        /// 离线数据命令
        /// </summary>
        public DelegateCommand<object> OffLineDataCommand => new((object o) =>
        {
            DialogParameters pairs = new DialogParameters
                {
                    { "Batch", o },
                };
            DialogHostService.ShowOnce(nameof(OffLineDatasView), pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });

        /// <summary>
        /// 导出数据命令
        /// </summary>
        public DelegateCommand<object> OutputDataCommand => new((object o) =>
        {
            DialogParameters pairs = new DialogParameters
                {
                    { "Batch", o },
                };
            DialogHostService.ShowOnce(nameof(OutputBatchDataView), pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });
        #endregion

        private DeviceParameter _currentDeviceParameter;
        public DeviceParameter CurrentDeviceParameter
        {
            get { return _currentDeviceParameter; }
            set { SetProperty(ref _currentDeviceParameter, value); }
        }

        #region  溶氧相关
        private Dictionary<string, BackgroundWorker> dicDOWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, bool> dicDOStatus = new Dictionary<string, bool>();
       
        private Dictionary<string, int> dicDOAirIndex = new Dictionary<string, int>();
        private Dictionary<string, int> dicDOO2Index = new Dictionary<string, int>();
        private Dictionary<string, int> dicDOTempIndex = new Dictionary<string, int>();
        private Dictionary<string, int> dicDOFeedIndex = new Dictionary<string, int>();

        private Dictionary<string, int> dicDODelta = new Dictionary<string, int>();
        private Dictionary<string, QPIDController> dicDOPid = new Dictionary<string, QPIDController>();
        private Dictionary<string, QPIDController> dicDOAirPid = new Dictionary<string, QPIDController>();
        private Dictionary<string, QPIDController> dicDOO2Pid = new Dictionary<string, QPIDController>();

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
        private Dictionary<string, bool> dicPHStatus = new Dictionary<string, bool>();
        private Dictionary<string, float> dicPHDelta = new Dictionary<string, float>();
        private Dictionary<string, QPIDController> dicPHPid = new Dictionary<string, QPIDController>();
        private Dictionary<string, IntelligentPHController> dicPHController = new Dictionary<string, IntelligentPHController>();
        #endregion

        #region 温控相关
        private Dictionary<string, BackgroundWorker> dicTempTimeWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, BackgroundWorker> dicTempWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, float> dicTempSP = new Dictionary<string, float>();
        private Dictionary<string, BackgroundWorker> dicTempDOWorker = new Dictionary<string, BackgroundWorker>();
        #endregion

        #region 补料1相关
        private Dictionary<string, float> dicFeed1SP = new Dictionary<string, float>();
        #endregion

        public DelegateCommand DeviceSettingCommand => new(() => 
        {

        });

        public DelegateCommand<DeviceParameter> AgitRunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }

            if (dicAgitWorker.ContainsKey(currentDeviceParameter.Name) && dicAgitWorker[currentDeviceParameter.Name] != null && dicAgitWorker[currentDeviceParameter.Name].IsBusy)
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
                            currentDeviceParameter.AgitParam.Agit_PV = currentDeviceParameter.AgitParam.Agit_PV >= Const.MaxAgit ? Const.MaxAgit : currentDeviceParameter.AgitParam.Agit_PV;
                           CommandWrapper.SetAgitSpeed(currentDeviceParameter.Name, currentDeviceParameter.AgitParam.Agit_PV);
                            dicAgitSP[currentDeviceParameter.Name] = currentDeviceParameter.AgitParam.Agit_PV;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("设置转速异常" + ex.Message);
                        }

                        var deviceParameter = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        BackgroundWorker worker = s as BackgroundWorker;
                        while (true)
                        {
                            if (worker.CancellationPending)
                            {
                                return;
                            }
                            try
                            {
                                if (dicAgitSP[deviceParameter.Name] != deviceParameter.AgitParam.Agit_PV)
                                {
                                    CommandWrapper.SetAgitSpeed(deviceParameter.Name, deviceParameter.AgitParam.Agit_PV);
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
                        CommandWrapper.SetAgitSpeed(currentDeviceParameter.Name, 0);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("转速控制失败" + ex.Message);
                    }
                });
            }
        });

        public DelegateCommand<DeviceParameter> DORunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }

            if (dicDOWorker.ContainsKey(currentDeviceParameter.Name)&& dicDOWorker[currentDeviceParameter.Name] != null && dicDOWorker[currentDeviceParameter.Name].IsBusy)
            {
                dicDOWorker[currentDeviceParameter.Name].CancelAsync();
                while (dicDOStatus[currentDeviceParameter.Name])
                {
                    Thread.Sleep(100);
                }
            }

            //关闭控制
            if (!currentDeviceParameter.DOParam.IsControling)
            {
                return;
            }

            bool firstInitFeed = true;
            bool firstInitTemp = true;

            dicDOPid[currentDeviceParameter.Name].Reset();
            dicDOWorker[currentDeviceParameter.Name] = new BackgroundWorker();
            dicDOWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;      // 允许报告进度
            dicDOWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true; // 允许取消操作
            // 绑定事件
            dicDOWorker[currentDeviceParameter.Name].DoWork += ((s, e) =>
            {
                dicDOStatus[currentDeviceParameter.Name] = true;

                var deviceParameter = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == currentDeviceParameter.Name);

                deviceParameter.FeedSuspend = deviceParameter.IsDOLimit = deviceParameter.DORegulationLimit = false;
                deviceParameter.DOParam.InitialTemp = deviceParameter.TempParam.Temp_PV;
                e.Result = deviceParameter.Name;

                float maxGas = 0;//用于通气量的总和
                float initialGas = 0;

                int factorIndex = -1;//当前执行索引
                int lastFactorIndex = -1;//当前执行索引
                int lastDODelta = 0;//低通滤波的上个值
                var baseAgit = -1;//转速底值
                PIDInfo info = null;
                PIDInfo lastPid = null;

                int sleepCount = 1;

                while (AppSession.DOPause)
                {
                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                    {
                        dicDOStatus[currentDeviceParameter.Name] = false;
                        return;
                    }

                    Thread.Sleep(1000);
                }

                RealTimeParam realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                while (realTimeParam.DO < deviceParameter.DOParam.DO_PV && !deviceParameter.DOParam.IsDirect)
                {
                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                    {
                        dicDOStatus[currentDeviceParameter.Name] = false;
                        return;
                    }
                    while (AppSession.DOPause)
                    {
                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                        {
                            dicDOStatus[currentDeviceParameter.Name] = false;
                            return;
                        }
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(1000);
                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                }

                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                while (realTimeParam.DO > deviceParameter.DOParam.DO_PV && !deviceParameter.DOParam.IsReverse)
                {
                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                    {
                        dicDOStatus[currentDeviceParameter.Name] = false;
                        return;
                    }
                    while (AppSession.DOPause)
                    {
                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                        {
                            dicDOStatus[currentDeviceParameter.Name] = false;
                            return;
                        }
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(1000);
                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                }

                //mid-ranging控制
                if (deviceParameter.DOParam.ControlStrategy == DOControlStrategy.Midranging)
                {
                    info = null;
                    lastPid = null;
                    baseAgit = -1;
                    lastDODelta = 0;//低通滤波的上个值
                    factorIndex = -1;//当前执行索引
                    lastFactorIndex = -1;//当前执行索引

                    MidRangingParam param = MidRangingParamManager.GetInstance().MidRangingParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                    realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                    deviceParameter.AgitParam.IsControling = true;
                    deviceParameter.AgitParam.Agit_PV = realTimeParam.Agit >= param.AgitLowerLimit ? realTimeParam.Agit <= param.AgitUpperLimit ? realTimeParam.Agit : param.AgitUpperLimit : param.AgitLowerLimit;
                    AgitRunCommand.Execute(deviceParameter);

                    ObservableCollection<DOControlFactor> collection = [.. param.FactorCol];

                    if (collection.Count > 0 && (collection[0] == DOControlFactor.Air || collection[0] == DOControlFactor.O2))
                    {
                        if (param.Unit == 0)//VVM
                        {
                            initialGas = MathF.Round((float)(param.InitialAir * (realTimeParam.JarWeight - 2000) / 1000), 2);
                            maxGas = MathF.Round((float)(param.AirUpperLimit * (realTimeParam.JarWeight - 2000) / 1000), 2);
                        }
                        else if (param.Unit == 1)//L/min
                        {
                            initialGas = param.InitialAir;
                            maxGas = param.AirUpperLimit;
                        }

                        switch (collection[0])
                        {
                            case DOControlFactor.Air:
                                deviceParameter.AirParam.FlowSpeed = realTimeParam.AirFlowSpeed >= initialGas ? realTimeParam.AirFlowSpeed <= maxGas ? realTimeParam.AirFlowSpeed : maxGas : initialGas;
                                AirRunCommand.Execute(deviceParameter);
                                break;
                            case DOControlFactor.O2:
                                deviceParameter.O2Param.FlowSpeed = realTimeParam.O2FlowSpeed >= initialGas ? realTimeParam.O2FlowSpeed <= maxGas ? realTimeParam.O2FlowSpeed : maxGas : initialGas;
                                O2RunCommand.Execute(deviceParameter);
                                break;
                        }
                    }

                    sleepCount = 5;
                    while (sleepCount > 0)
                    {
                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                        {
                            dicDOStatus[currentDeviceParameter.Name] = false;
                            return;
                        }
                        while (AppSession.DOPause)
                        {
                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                dicDOStatus[currentDeviceParameter.Name] = false;
                                return;
                            }
                            Thread.Sleep(1000);
                        }
                        sleepCount--;
                        Thread.Sleep(1000);
                    }

                    ResetDOParam(deviceParameter);
                    while (true)
                    {
                        try
                        {
                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            while (realTimeParam.DO < deviceParameter.DOParam.DO_PV && !deviceParameter.DOParam.IsDirect)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        dicDOStatus[currentDeviceParameter.Name] = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                Thread.Sleep(1000);
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            }

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            while (realTimeParam.DO > deviceParameter.DOParam.DO_PV && !deviceParameter.DOParam.IsReverse)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        dicDOStatus[currentDeviceParameter.Name] = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                Thread.Sleep(1000);
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            }

                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                dicDOStatus[currentDeviceParameter.Name] = false;
                                return;
                            }

                            while (AppSession.DOPause)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }

                                Thread.Sleep(1000);
                            }

                            string result = File.ReadAllText(FileConst.PidInfoPath);
                            List<PIDInfo> pIDInfos = JsonConvert.DeserializeObject<List<PIDInfo>>(result);
                            if (pIDInfos == null)
                            {
                                MessageBox.Show("PID调控策略列表为空");
                                return;
                            }
                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (realTimeParam.DO <= deviceParameter.DOParam.DO_PV)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("DO_正向") && t.deviceID == deviceParameter.Name);
                            }
                            else if (realTimeParam.DO >= deviceParameter.DOParam.DO_PV)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("DO_反向") && t.deviceID == deviceParameter.Name);
                            }

                            if (info == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在DO的PID调控策略", deviceParameter.Name));
                                return;
                            }

                            if (baseAgit == -1)
                            {
                                baseAgit = realTimeParam.Agit;
                            }

                            //如果pid类型变了，pid系数清零 方成
                            //if (info != null && lastPid != null && info.PidName != lastPid.PidName)
                            if (info != null && lastPid != null && !info.Equals(lastPid))
                            {
                                if (info.PidName != lastPid.PidName)
                                {
                                    LogHelper.Debug(string.Format("反应器{2} DO调控：由{0}切换至{1}", lastPid.PidName, info.PidName, deviceParameter.Name));
                                    baseAgit = deviceParameter.AgitParam.Agit_PV;
                                }
                                LogHelper.Debug(string.Format("反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam.Agit, deviceParameter.AgitParam.Agit_PV, baseAgit));

                                ResetDOParam(deviceParameter);
                            }
                            lastPid = info;

                            param = MidRangingParamManager.GetInstance().MidRangingParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);

                            if (Math.Abs(realTimeParam.DO - deviceParameter.DOParam.DO_PV) <= info.deadArea)
                            {
                                baseAgit = deviceParameter.AgitParam.Agit_PV;
                                ResetDOParam(deviceParameter);

                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                while (count > 0)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        dicDOStatus[currentDeviceParameter.Name] = false;
                                        return;
                                    }
                                    while (AppSession.DOPause)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            dicDOStatus[currentDeviceParameter.Name] = false;
                                            return;
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    count--;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }

                            dicDOPid[deviceParameter.Name].SetParameters(kp: (float)info.P, ki: (float)info.I, kd: (float)info.D, integralThreshold: info.Threshold, interval: info.Interval);
                            dicDOPid[deviceParameter.Name].SetOutputLimits(-Math.Abs(info.maxSpeed), Math.Abs(info.maxSpeed));
                            dicDOPid[deviceParameter.Name].SetIntegralLimits(-2000, 2000);
                            dicDOPid[deviceParameter.Name].SetTarget(deviceParameter.DOParam.DO_PV);

                            LogHelper.Debug(string.Format("反应器{6} Mid-Ranging DO预设值：{0}，DO当前值：{1}，P：{2}，I：{3}，D：{4},采样时间：{5}", deviceParameter.DOParam.DO_PV, realTimeParam.DO, info.P, info.I, info.D, info.Interval, deviceParameter.Name));

                            float temp = dicDOPid[deviceParameter.Name].CalculatePositional_DO((float)realTimeParam.DO);
                            int tempAgit = Convert.ToInt32(baseAgit + temp);

                            dicDODelta[deviceParameter.Name] = tempAgit;
                            if (deviceParameter.DOFilterEnable)
                            {
                                //增加低通滤波 
                                var lowPassDelta = Convert.ToInt32(RCFilter.LowPass(tempAgit, lastDODelta, deviceParameter.AgitSampleCycle, deviceParameter.AgitSampleFrequency));
                                lastDODelta = lowPassDelta;
                                dicDODelta[deviceParameter.Name] = lowPassDelta;
                            }

                            LogHelper.Debug(string.Format("反应器{0} Mid-Ranging 转速底值：{1}，Delta：{2},原始值{3}，滤波值{4}", deviceParameter.Name, baseAgit, temp, tempAgit, dicDODelta[deviceParameter.Name]));

                            deviceParameter.AgitParam.Agit_PV = dicDODelta[deviceParameter.Name] >= param.AgitUpperLimit ? param.AgitUpperLimit : dicDODelta[deviceParameter.Name] <= param.AgitLowerLimit ? param.AgitLowerLimit : dicDODelta[deviceParameter.Name];
                            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(currentDeviceParameter.Name, deviceParameter.AgitParam.Agit_PV);

                            sleepCount = info.Interval <= 0 ? 1 : info.Interval;
                            while (sleepCount > 0)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        dicDOStatus[currentDeviceParameter.Name] = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                sleepCount--;
                                Thread.Sleep(1000);
                            }

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (Math.Abs(realTimeParam.DO - deviceParameter.DOParam.DO_PV) <= info.deadArea)
                            {
                                baseAgit = deviceParameter.AgitParam.Agit_PV;

                                ResetDOParam(deviceParameter);

                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                while (count > 0)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        dicDOStatus[currentDeviceParameter.Name] = false;
                                        return;
                                    }
                                    while (AppSession.DOPause)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            dicDOStatus[currentDeviceParameter.Name] = false;
                                            return;
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    count--;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }

                            if (deviceParameter.AgitParam.Agit_PV > param.AgitHigh && factorIndex == -1)
                            {
                                LogHelper.Debug($"到达设定转速高限:{param.AgitHigh}");
                                lastFactorIndex = factorIndex;
                                factorIndex = 0;
                            }

                            if (factorIndex < 0 || collection.Count <= factorIndex)//如果未达到高限或者没有其他执行参数，则一直循环
                            {
                                continue;
                            }

                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                dicDOStatus[currentDeviceParameter.Name] = false;
                                return;
                            }

                            while (AppSession.DOPause)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }

                            param = MidRangingParamManager.GetInstance().MidRangingParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                            PIDInfo info1 = null;
                            var previousElements = collection.Take(factorIndex);
                            bool isExistOtherGas = false;//在当前气体之前是否存在气体

                            switch (collection[factorIndex])
                            {
                                case DOControlFactor.Air:

                                    if (!deviceParameter.AirParam.IsControling)
                                    {
                                        deviceParameter.AirParam.IsControling = true;
                                        AirRunCommand.Execute(deviceParameter);
                                    }
                                    if (param.Unit == 0)//VVM
                                    {
                                        initialGas = MathF.Round((float)(param.InitialAir * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                        maxGas = MathF.Round((float)(param.AirUpperLimit * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                    }
                                    else if (param.Unit == 1)//L/min
                                    {
                                        initialGas = param.InitialAir;
                                        maxGas = param.AirUpperLimit;
                                    }

                                    info1 = pIDInfos.FindFirst(t => t.PidName.Contains("通气") && t.deviceID == deviceParameter.Name);
                                    if (info1 == null)
                                    {
                                        info1 = new PIDInfo() { P = 0.05f, I = 0.005f, D = 20, Threshold = 1000, maxSpeed = 1000 };
                                    }

                                    dicDOAirPid[deviceParameter.Name].Reset();
                                    dicDOAirPid[deviceParameter.Name].SetParameters(kp: (float)info1.P, ki: (float)info1.I, kd: (float)info1.D, integralThreshold: info1.Threshold, interval: info1.Interval);
                                    dicDOAirPid[deviceParameter.Name].SetOutputLimits(-Math.Abs(info1.maxSpeed), Math.Abs(info1.maxSpeed));
                                    dicDOAirPid[deviceParameter.Name].SetIntegralLimits(-2000, 2000);
                                    dicDOAirPid[deviceParameter.Name].SetTarget(dicDODelta[deviceParameter.Name]);
                                    float tempAir = dicDOAirPid[deviceParameter.Name].CalculateIncremental(param.AgitHigh);
                                    float airSpeed = deviceParameter.AirParam.FlowSpeed + tempAir;
                                    isExistOtherGas = previousElements.Where(t => t == DOControlFactor.O2).Count() > 0;
                                    float minAir = isExistOtherGas == true ? 0 : initialGas;
                                    if (airSpeed >= maxGas)
                                    {
                                        if (factorIndex < collection.Count - 1)//如果还有下一执行参数，则跳到下一个执行参数
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                    }
                                    else if (airSpeed <= minAir)
                                    {
                                        if (factorIndex - 1 > -1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }

                                    airSpeed = airSpeed >= maxGas ? maxGas : airSpeed < minAir ? minAir : MathF.Round(airSpeed, 2);
                                    deviceParameter.AirParam.FlowSpeed = airSpeed;
                                    if (isExistOtherGas)
                                    {
                                        deviceParameter.O2Param.IsControling = true;
                                        deviceParameter.O2Param.FlowSpeed = MathF.Round(maxGas - airSpeed, 2);
                                    }

                                    LogHelper.Debug(string.Format("反应器{0} Mid-Ranging 通气预设值：{1}，当前：{2}，delta：{3}", deviceParameter.Name, airSpeed, deviceParameter.AirParam.FlowSpeed, tempAir));
                                    sleepCount = info.Interval <= 1 ? 1 : info.Interval;
                                    while (sleepCount > 0)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            dicDOStatus[currentDeviceParameter.Name] = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                            {
                                                dicDOStatus[currentDeviceParameter.Name] = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case DOControlFactor.O2:
                                    if (!deviceParameter.O2Param.IsControling)
                                    {
                                        deviceParameter.O2Param.IsControling = true;
                                        O2RunCommand.Execute(deviceParameter);
                                    }
                                    if (param.Unit == 0)//VVM
                                    {
                                        initialGas = MathF.Round((float)(param.InitialAir * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                        maxGas = MathF.Round((float)(param.AirUpperLimit * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                    }
                                    else if (param.Unit == 1)//L/min
                                    {
                                        initialGas = param.InitialAir;
                                        maxGas = param.AirUpperLimit;
                                    }

                                    info1 = pIDInfos.FindFirst(t => t.PidName.Contains("氧气") && t.deviceID == deviceParameter.Name);
                                    if (info1 == null)
                                    {
                                        info1 = new PIDInfo() { P = 0.05f, I = 0.005f, D = 20, Threshold = 1000, maxSpeed = 1000 };
                                    }
                                    dicDOO2Pid[deviceParameter.Name].Reset();
                                    dicDOO2Pid[deviceParameter.Name].SetParameters(kp: (float)info1.P, ki: (float)info1.I, kd: (float)info1.D, integralThreshold: info1.Threshold, interval: info1.Interval);
                                    dicDOO2Pid[deviceParameter.Name].SetOutputLimits(-Math.Abs(info1.maxSpeed), Math.Abs(info1.maxSpeed));
                                    dicDOO2Pid[deviceParameter.Name].SetIntegralLimits(-2000, 2000);
                                    dicDOO2Pid[deviceParameter.Name].SetTarget(dicDODelta[deviceParameter.Name]);
                                    float tempO2 = dicDOO2Pid[deviceParameter.Name].CalculateIncremental((float)param.AgitHigh);
                                    float o2Speed = deviceParameter.O2Param.FlowSpeed + tempO2;
                                    isExistOtherGas = previousElements.Where(t => t == DOControlFactor.Air).Count() > 0;
                                    float minO2 = isExistOtherGas == true ? 0 : initialGas;
                                    if (o2Speed >= maxGas)
                                    {
                                        if (factorIndex < collection.Count - 1)//如果还有下一执行参数，则跳到下一个执行参数
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                    }
                                    else if (o2Speed <= minO2)
                                    {
                                        if (factorIndex - 1 > -1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }

                                    o2Speed = o2Speed >= maxGas ? maxGas : o2Speed < minO2 ? minO2 : MathF.Round(o2Speed, 2);
                                    deviceParameter.O2Param.FlowSpeed = o2Speed;
                                    if (isExistOtherGas)
                                    {
                                        deviceParameter.AirParam.IsControling = true;
                                        deviceParameter.AirParam.FlowSpeed = maxGas - o2Speed > 0 ? MathF.Round(maxGas - o2Speed, 2) : 0;
                                    }
                                    LogHelper.Debug(string.Format("反应器{0} Mid-Ranging 氧气预设值：{1}，底值：{2}，delta：{3}", deviceParameter.Name, o2Speed, deviceParameter.O2Param.FlowSpeed, tempO2));
                                    sleepCount = info.Interval <= 1 ? 1 : info.Interval;
                                    while (sleepCount > 0)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            dicDOStatus[currentDeviceParameter.Name] = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                            {
                                                dicDOStatus[currentDeviceParameter.Name] = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case DOControlFactor.Temp:
                                    deviceParameter.DORegulationLimit = false;
                                    if (!deviceParameter.TempParam.IsControling)
                                    {
                                        TempRunCommand.Execute(deviceParameter);
                                        Thread.Sleep(1000);
                                    }
                                    if (firstInitTemp)
                                    {
                                        deviceParameter.DOParam.InitialTemp = deviceParameter.TempParam.Temp_PV;
                                        firstInitTemp = false;
                                    }

                                    QPIDController pIDController = new QPIDController();
                                    info1 = pIDInfos.FindFirst(t => t.PidName.Contains("降温") && t.deviceID == deviceParameter.Name);
                                    if (info1 == null)
                                    {
                                        info1 = new PIDInfo() { P = 0.05f, I = 0.005f, D = 20, Threshold = 1000, maxSpeed = 1000 };
                                    }
                                    pIDController.SetParameters(kp: (float)info1.P, ki: (float)info1.I, kd: (float)info1.D, integralThreshold: info1.Threshold, interval: info1.Interval);
                                    pIDController.SetOutputLimits(-Math.Abs(info1.maxSpeed), Math.Abs(info1.maxSpeed));
                                    pIDController.SetIntegralLimits(-2000, 2000);
                                    pIDController.SetTarget(param.AgitHigh);
                                    float increment = pIDController.CalculateIncremental(dicDODelta[deviceParameter.Name]);
                                    float currentTemp = deviceParameter.TempParam.Temp_PV + increment;
                                    if (currentTemp <= deviceParameter.TempDOLowerLimit)
                                    {
                                        if (factorIndex < collection.Count - 1)//如果还有下一执行参数，则跳到下一个执行参数
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                    }
                                    else if (currentTemp >= deviceParameter.DOParam.InitialTemp)
                                    {
                                        if (factorIndex - 1 > -1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }
                                    currentTemp = currentTemp <= deviceParameter.TempDOLowerLimit ? deviceParameter.TempDOLowerLimit : currentTemp >= deviceParameter.DOParam.InitialTemp ? deviceParameter.DOParam.InitialTemp : currentTemp;
                                    deviceParameter.TempParam.Temp_PV = MathF.Round(currentTemp, 2);
                                    LogHelper.Debug(string.Format("反应器{0} 起始温度{1} 单次delta{2} 实际温度{3}", deviceParameter.Name, deviceParameter.DOParam.InitialTemp, increment, currentTemp));
                                    sleepCount = info1.Interval <= 0 ? 1 : info1.Interval;
                                    while (sleepCount > 0)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            dicDOStatus[currentDeviceParameter.Name] = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                            {
                                                dicDOStatus[currentDeviceParameter.Name] = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }

                                    if (deviceParameter.TempParam.Temp_PV <= deviceParameter.TempDOLowerLimit || deviceParameter.TempParam.Temp_PV >= deviceParameter.DOParam.InitialTemp)
                                    {
                                        while (true)
                                        {
                                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                            {
                                                dicDOStatus[currentDeviceParameter.Name] = false;
                                                return;
                                            }
                                            while (AppSession.DOPause)
                                            {
                                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                                {
                                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                                    return;
                                                }
                                                Thread.Sleep(1000);
                                            }
                                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                            if (Math.Abs(realTimeParam.Temp - deviceParameter.TempParam.Temp_PV) <= 0.2)
                                            {
                                                break;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    break;
                                case DOControlFactor.Feed:
                                    if (!deviceParameter.FeedParam1.IsControling)
                                    {
                                        if (factorIndex < collection.Count - 1 && lastFactorIndex <= factorIndex)//如果还有下一执行参数，则跳到下一个执行参数
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                        else if (factorIndex - 1 > -1 && lastFactorIndex >= factorIndex)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }

                                    if (firstInitFeed)
                                    {
                                        deviceParameter.FeedSuspend = true;
                                        deviceParameter.DOParam.InitialFeed = deviceParameter.FeedParam1.Feed_PV;
                                        firstInitFeed = false;
                                    }
                                    QPIDController controller = new QPIDController();
                                    info1 = pIDInfos.FindFirst(t => t.PidName.Contains("补料") && t.deviceID == deviceParameter.Name);
                                    if (info1 == null)
                                    {
                                        info1 = new PIDInfo() { P = 0.05f, I = 0.005f, D = 20, Threshold = 1000, maxSpeed = 1000 };
                                    }
                                    controller.SetParameters(kp: (float)info1.P, ki: (float)info1.I, kd: (float)info1.D, integralThreshold: info1.Threshold, interval: info1.Interval);
                                    controller.SetOutputLimits(-Math.Abs(info1.maxSpeed), Math.Abs(info1.maxSpeed));
                                    controller.SetIntegralLimits(-2000, 2000);
                                    controller.SetTarget(param.AgitHigh);
                                    float incrementFeed = controller.CalculateIncremental(dicDODelta[deviceParameter.Name]);
                                    float currentFeed = deviceParameter.FeedParam1.Feed_PV + incrementFeed;
                                    if (currentFeed <= deviceParameter.FeedDOLowerLimit)
                                    {
                                        if (factorIndex < collection.Count - 1)//如果还有下一执行参数，则跳到下一个执行参数
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                    }
                                    else if (currentFeed >= deviceParameter.DOParam.InitialFeed)
                                    {
                                        if (factorIndex - 1 > -1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }
                                    currentFeed = currentFeed <= deviceParameter.FeedDOLowerLimit ? deviceParameter.FeedDOLowerLimit : currentFeed >= deviceParameter.DOParam.InitialFeed ? deviceParameter.DOParam.InitialFeed : currentFeed;
                                    deviceParameter.FeedParam1.Feed_PV = MathF.Round(currentFeed, 2);
                                    PeristalticPump pump = PeristalticPump.FeedPump;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        var controlParam = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = (float)deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, controlParam);
                                        dicFeed1SP[deviceParameter.Name] = deviceParameter.FeedParam1.Feed_PV;
                                    }
                                    LogHelper.Debug(string.Format("反应器{0} 起始补料{1} 单次delta{2} 实际补料{3}", deviceParameter.Name, deviceParameter.DOParam.InitialFeed, incrementFeed, currentFeed));
                                    sleepCount = info1.Interval <= 0 ? 1 : info1.Interval;
                                    while (sleepCount > 0)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            dicDOStatus[currentDeviceParameter.Name] = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                            {
                                                dicDOStatus[currentDeviceParameter.Name] = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(string.Format("DO调整失败_Mid-Ranging，错误信息：{0}", ex.Message));
                            //LogHelper.Debug(string.Format("DO调整失败_Mid-Ranging，错误信息：{0}", ex.Message));
                            return;
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
                        realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                        while (realTimeParam.DO < deviceParameter.DOParam.DO_PV && !deviceParameter.DOParam.IsDirect)
                        {
                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                dicDOStatus[currentDeviceParameter.Name] = false;
                                return;
                            }
                            while (AppSession.DOPause)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);
                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                        }

                        realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                        while (realTimeParam.DO > deviceParameter.DOParam.DO_PV && !deviceParameter.DOParam.IsReverse)
                        {
                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                dicDOStatus[currentDeviceParameter.Name] = false;
                                return;
                            }
                            while (AppSession.DOPause)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }
                            Thread.Sleep(1000);
                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                        }

                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                        {
                            dicDOStatus[currentDeviceParameter.Name] = false;
                            return;
                        }

                        while (AppSession.DOPause)
                        {
                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                dicDOStatus[currentDeviceParameter.Name] = false;
                                return;
                            }

                            Thread.Sleep(1000);
                        }

                        realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
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
                            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(currentDeviceParameter.Name, deviceParameter.AgitParam.Agit_PV);

                            int count = deviceParameter.DOParam.AgitCycle.DirectInterval;
                            int index = 0;
                            while (index < count)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }

                                while (AppSession.DOPause)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        dicDOStatus[currentDeviceParameter.Name] = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
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
                            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(currentDeviceParameter.Name, deviceParameter.AgitParam.Agit_PV);

                            int count = deviceParameter.DOParam.AgitCycle.ReverseInterval;
                            int index = 0;
                            while (index < count)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        dicDOStatus[currentDeviceParameter.Name] = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                index += 1;
                                Thread.Sleep(1000);
                            }
                        }

                    }

                }
                //级联通气控制
                else if (deviceParameter.DOParam.ControlStrategy == DOControlStrategy.Step)
                {
                    e.Result = deviceParameter.Name;
                    DateTime startTime = DateTime.Now;
                    var sv = deviceParameter.DOParam.DO_PV;
                    dicDOFeedIndex[deviceParameter.Name] = dicDOTempIndex[deviceParameter.Name] = dicDOAirIndex[deviceParameter.Name] = dicDOO2Index[deviceParameter.Name] = 0;

                    info = null;
                    lastPid = null;
                    baseAgit = -1;
                    lastDODelta = 0;//低通滤波的上个值
                    factorIndex = 0;//当前执行索引
                    lastFactorIndex = -1;//当前执行索引

                    deviceParameter.AgitParam.IsControling = true;
                    AgitRunCommand.Execute(deviceParameter);

                    DOAssParam param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);

                    ObservableCollection<DOControlFactor> collection = [.. param.FactorCol];

                    if (collection.Count > 0)
                    {
                        if (collection.Contains(DOControlFactor.Air) && deviceParameter.AirParam.IsControling)
                        {
                            if (param.Unit == 0)//VVM
                            {
                                dicDOAirIndex[deviceParameter.Name] = param.AirCol.Select((value, index) => new { Value = value, Index = index }).FirstOrDefault(x =>
                                 (x.Index == 0 || MathF.Round(param.AirCol[x.Index - 1].StepValue * (realTimeParam.JarWeight - 2000) / 1000, 2) <= realTimeParam.AirFlowSpeed) &&
                                 (x.Index == param.AirCol.Count - 1 || MathF.Round(param.AirCol[x.Index + 1].StepValue * (realTimeParam.JarWeight - 2000) / 1000, 2) >= realTimeParam.AirFlowSpeed))?.Index ?? -1;
                            }
                            else if (param.Unit == 1)//L/min
                            {
                                dicDOAirIndex[deviceParameter.Name] = param.AirCol.Select((value, index) => new { Value = value, Index = index }).FirstOrDefault(x =>
                                 (x.Index == 0 || param.AirCol[x.Index - 1].StepValue <= realTimeParam.AirFlowSpeed) &&
                                 (x.Index == param.AirCol.Count - 1 || param.AirCol[x.Index + 1].StepValue >= realTimeParam.AirFlowSpeed))?.Index ?? -1;
                            }

                            LogHelper.Debug(string.Format("阶梯级联：通气档位为{0}", dicDOAirIndex[deviceParameter.Name] + 1));
                            AirRunCommand.Execute(deviceParameter);
                        }

                        if (collection.Contains(DOControlFactor.O2) && deviceParameter.O2Param.IsControling)
                        {
                            if (param.Unit == 0)//VVM
                            {
                                dicDOO2Index[deviceParameter.Name] = param.O2Col.Select((value, index) => new { Value = value, Index = index }).FirstOrDefault(x =>
                                 (x.Index == 0 || MathF.Round(param.O2Col[x.Index - 1].StepValue * (realTimeParam.JarWeight - 2000) / 1000, 2) <= realTimeParam.O2FlowSpeed) &&
                                 (x.Index == param.O2Col.Count - 1 || MathF.Round(param.O2Col[x.Index + 1].StepValue * (realTimeParam.JarWeight - 2000) / 1000, 2) >= realTimeParam.O2FlowSpeed))?.Index ?? -1;
                            }
                            else if (param.Unit == 1)//L/min
                            {
                                dicDOO2Index[deviceParameter.Name] = param.O2Col.Select((value, index) => new { Value = value, Index = index }).FirstOrDefault(x =>
                                 (x.Index == 0 || param.O2Col[x.Index - 1].StepValue <= realTimeParam.O2FlowSpeed) &&
                                 (x.Index == param.O2Col.Count - 1 || param.O2Col[x.Index + 1].StepValue >= realTimeParam.O2FlowSpeed))?.Index ?? -1;
                            }

                            LogHelper.Debug(string.Format("阶梯级联：氧气档位为{0}", dicDOO2Index[deviceParameter.Name] + 1));
                            O2RunCommand.Execute(deviceParameter);
                        }
                    }

                    sleepCount = 5;
                    while (sleepCount > 0)
                    {
                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                        {
                            dicDOStatus[currentDeviceParameter.Name] = false;
                            return;
                        }
                        while (AppSession.DOPause)
                        {
                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                dicDOStatus[currentDeviceParameter.Name] = false;
                                return;
                            }
                            Thread.Sleep(1000);
                        }
                        sleepCount--;
                        Thread.Sleep(1000);
                    }

                    ResetDOParam(deviceParameter);

                    while (true)
                    {
                        try
                        {
                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            while (realTimeParam.DO < deviceParameter.DOParam.DO_PV && !deviceParameter.DOParam.IsDirect)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        dicDOStatus[currentDeviceParameter.Name] = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                Thread.Sleep(1000);
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            }

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            while (realTimeParam.DO > deviceParameter.DOParam.DO_PV && !deviceParameter.DOParam.IsReverse)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        dicDOStatus[currentDeviceParameter.Name] = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                Thread.Sleep(1000);
                                realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            }

                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                dicDOStatus[currentDeviceParameter.Name] = false;
                                return;
                            }

                            while (AppSession.DOPause)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }

                                Thread.Sleep(1000);
                            }

                            string result = File.ReadAllText(FileConst.PidInfoPath);
                            List<PIDInfo> pIDInfos = JsonConvert.DeserializeObject<List<PIDInfo>>(result);
                            if (pIDInfos == null)
                            {
                                MessageBox.Show("PID调控策略列表为空");
                                return;
                            }

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (realTimeParam.DO <= deviceParameter.DOParam.DO_PV)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("DO_正向") && t.deviceID == deviceParameter.Name);
                            }
                            else if (realTimeParam.DO >= deviceParameter.DOParam.DO_PV)
                            {
                                info = pIDInfos.FindFirst(t => t.PidName.Contains("DO_反向") && t.deviceID == deviceParameter.Name);
                            }

                            if (info == null)
                            {
                                MessageBox.Show(string.Format("反应器{0}不存在DO的PID调控策略", deviceParameter.Name));
                                return;
                            }

                            if (baseAgit == -1)
                            {
                                baseAgit = realTimeParam.Agit;
                            }

                            //如果pid类型变了，pid系数清零 方成
                            //if (info != null && lastPid != null && info.PidName != lastPid.PidName)
                            if (info != null && lastPid != null && !info.Equals(lastPid))
                            {
                                if (info.PidName != lastPid.PidName)
                                {
                                    LogHelper.Debug(string.Format("反应器{2} DO调控：由{0}切换至{1}", lastPid.PidName, info.PidName, deviceParameter.Name));
                                    baseAgit = deviceParameter.AgitParam.Agit_PV;
                                }

                                ResetDOParam(deviceParameter);
                                LogHelper.Debug(string.Format("反应器{0} 当前转速{1} 预设转速{2} 转速底值设置为{3}", deviceParameter.Name, realTimeParam.Agit, deviceParameter.AgitParam.Agit_PV, baseAgit));
                            }
                            lastPid = info;

                            param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (Math.Abs(realTimeParam.DO - deviceParameter.DOParam.DO_PV) <= info.deadArea)
                            {
                                baseAgit = deviceParameter.AgitParam.Agit_PV;
                                ResetDOParam(deviceParameter);

                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                while (count > 0)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        dicDOStatus[currentDeviceParameter.Name] = false;
                                        return;
                                    }
                                    while (AppSession.DOPause)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            dicDOStatus[currentDeviceParameter.Name] = false;
                                            return;
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    count--;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }

                            dicDOPid[deviceParameter.Name].SetParameters(kp: (float)info.P, ki: (float)info.I, kd: (float)info.D, integralThreshold: info.Threshold, interval: info.Interval);
                            dicDOPid[deviceParameter.Name].SetOutputLimits(-Math.Abs(info.maxSpeed), Math.Abs(info.maxSpeed));
                            dicDOPid[deviceParameter.Name].SetIntegralLimits(-2000, 2000);
                            dicDOPid[deviceParameter.Name].SetTarget(deviceParameter.DOParam.DO_PV);

                            LogHelper.Debug(string.Format("反应器{6} 阶梯级联 DO预设值：{0}，DO当前值：{1}，P：{2}，I：{3}，D：{4},采样时间：{5}", deviceParameter.DOParam.DO_PV, realTimeParam.DO, info.P, info.I, info.D, info.Interval, deviceParameter.Name));

                            float temp = dicDOPid[deviceParameter.Name].CalculatePositional_DO((float)realTimeParam.DO);
                            float timeOffset = Convert.ToSingle((DateTime.Now - startTime).TotalMinutes);
                            temp = 1 * temp;//系数都默认为1
                            int tempAgit = Convert.ToInt32(baseAgit + temp);
                            dicDODelta[deviceParameter.Name] = tempAgit;
                            if (deviceParameter.DOFilterEnable)
                            {
                                //增加低通滤波 
                                var lowPassDelta = Convert.ToInt32(RCFilter.LowPass(tempAgit, lastDODelta, deviceParameter.AgitSampleCycle, deviceParameter.AgitSampleFrequency));
                                lastDODelta = lowPassDelta;
                                dicDODelta[deviceParameter.Name] = lowPassDelta;
                            }
                            LogHelper.Debug(string.Format("反应器{0} 阶梯级联 转速底值：{1}，Delta：{2},原始值{3}，滤波值{4}", deviceParameter.Name, baseAgit, temp, tempAgit, dicDODelta[deviceParameter.Name]));

                            deviceParameter.AgitParam.Agit_PV = dicDODelta[deviceParameter.Name] >= param.AgitUpperLimit ? param.AgitUpperLimit : dicDODelta[deviceParameter.Name] <= param.AgitLowerLimit ? param.AgitLowerLimit : dicDODelta[deviceParameter.Name];
                            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(currentDeviceParameter.Name, deviceParameter.AgitParam.Agit_PV);

                            sleepCount = info.Interval <= 1 ? 1 : info.Interval;
                            while (sleepCount > 0)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }
                                while (AppSession.DOPause)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        dicDOStatus[currentDeviceParameter.Name] = false;
                                        return;
                                    }
                                    Thread.Sleep(1000);
                                }
                                sleepCount--;
                                Thread.Sleep(1000);
                            }

                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                            if (Math.Abs(realTimeParam.DO - deviceParameter.DOParam.DO_PV) <= info.deadArea)
                            {
                                baseAgit = deviceParameter.AgitParam.Agit_PV;

                                ResetDOParam(deviceParameter);

                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                while (count > 0)
                                {
                                    if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        dicDOStatus[currentDeviceParameter.Name] = false;
                                        return;
                                    }
                                    while (AppSession.DOPause)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            dicDOStatus[currentDeviceParameter.Name] = false;
                                            return;
                                        }
                                        Thread.Sleep(1000);
                                    }
                                    count--;
                                    Thread.Sleep(1000);
                                }
                                continue;
                            }

                            if (factorIndex < 0 || collection.Count <= factorIndex)
                            {
                                continue;
                            }

                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                            {
                                dicDOStatus[currentDeviceParameter.Name] = false;
                                return;
                            }

                            while (AppSession.DOPause)
                            {
                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                {
                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                    return;
                                }
                                Thread.Sleep(1000);
                            }

                            param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                            var previousElements = collection.Take(factorIndex);
                            bool isExistOtherGas = false;//在当前气体之前是否存在气体
                            switch (collection[factorIndex])
                            {
                                case DOControlFactor.Air:

                                    if (!deviceParameter.AirParam.IsControling)
                                    {
                                        deviceParameter.AirParam.IsControling = true;
                                        AirRunCommand.Execute(deviceParameter);
                                    }

                                    //float airFlowSpeed1 = 0f;
                                    //if (param.Unit == 0)//VVM
                                    //{
                                    //    airFlowSpeed1 = MathF.Round((float)(param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                    //}
                                    //else if (param.Unit == 1)//L/min
                                    //{
                                    //    airFlowSpeed1 = param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue;
                                    //}
                                    //realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                    //if (Math.Abs(realTimeParam.AirFlowSpeed - airFlowSpeed1) > 0.1)
                                    //{
                                    //    deviceParameter.AirParam.FlowSpeed = airFlowSpeed1;
                                    //    Thread.Sleep(10000);
                                    //}

                                    if (param.Unit == 0)//VVM
                                    {
                                        initialGas = MathF.Round((float)(param.AirCol[0].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                        maxGas = MathF.Round((float)(param.AirCol[param.AirCol.Count - 1].StepValue * (deviceParameter.JarWeight - 2000) / 1000), 2);
                                    }
                                    else if (param.Unit == 1)//L/min
                                    {
                                        initialGas = param.AirCol[0].StepValue;
                                        maxGas = param.AirCol[param.AirCol.Count - 1].StepValue;
                                    }

                                    isExistOtherGas = previousElements.Where(t => t == DOControlFactor.O2).Count() > 0;

                                    if (dicDODelta[deviceParameter.Name] <= param.AgitLowerLimit)
                                    {
                                        if (dicDOAirIndex[deviceParameter.Name] > 0)//还存在上一阶梯
                                        {
                                            dicDOAirIndex[deviceParameter.Name] -= 1;

                                            float airFlowSpeed = 0f;
                                            if (param.Unit == 0)//VVM
                                            {
                                                airFlowSpeed = MathF.Round((float)(param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                            }
                                            else if (param.Unit == 1)//L/min
                                            {
                                                airFlowSpeed = param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue;
                                            }
                                            deviceParameter.AirParam.FlowSpeed = airFlowSpeed;

                                            if (isExistOtherGas)
                                            {
                                                if (dicDOO2Index[deviceParameter.Name] < param.O2Col.Count - 1)//加一档
                                                {
                                                    dicDOO2Index[deviceParameter.Name] += 1;

                                                    float o2FlowSpeed = 0f;
                                                    if (param.Unit == 0)//VVM
                                                    {
                                                        o2FlowSpeed = MathF.Round((float)(param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                                    }
                                                    else if (param.Unit == 1)//L/min
                                                    {
                                                        o2FlowSpeed = param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue;
                                                    }
                                                    deviceParameter.O2Param.FlowSpeed = o2FlowSpeed;
                                                    deviceParameter.O2Param.IsControling = true;
                                                    O2RunCommand.Execute(deviceParameter);
                                                }
                                            }
                                        }
                                        else if (factorIndex > 0)//非第一因子,所以不需要最低通气
                                        {
                                            if (isExistOtherGas)
                                            {
                                                deviceParameter.AirParam.FlowSpeed = 0;

                                                if (dicDOO2Index[deviceParameter.Name] < param.O2Col.Count - 1)//加一档
                                                {
                                                    dicDOO2Index[deviceParameter.Name] += 1;

                                                    float o2FlowSpeed = 0f;
                                                    if (param.Unit == 0)//VVM
                                                    {
                                                        o2FlowSpeed = MathF.Round((float)(param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                                    }
                                                    else if (param.Unit == 1)//L/min
                                                    {
                                                        o2FlowSpeed = param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue;
                                                    }
                                                    deviceParameter.O2Param.FlowSpeed = o2FlowSpeed;
                                                    deviceParameter.O2Param.IsControling = true;
                                                    O2RunCommand.Execute(deviceParameter);
                                                }
                                            }

                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }
                                    else if (dicDODelta[deviceParameter.Name] >= param.AgitUpperLimit)
                                    {
                                        if (dicDOAirIndex[deviceParameter.Name] < param.AirCol.Count - 1)//还存在下一阶梯
                                        {
                                            dicDOAirIndex[deviceParameter.Name] += 1;

                                            float airFlowSpeed = 0f;
                                            if (param.Unit == 0)//VVM
                                            {
                                                airFlowSpeed = MathF.Round((float)(param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                            }
                                            else if (param.Unit == 1)//L/min
                                            {
                                                airFlowSpeed = param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue;
                                            }
                                            deviceParameter.AirParam.FlowSpeed = airFlowSpeed;

                                            if (isExistOtherGas)
                                            {
                                                if (dicDOO2Index[deviceParameter.Name] > 0)
                                                {
                                                    dicDOO2Index[deviceParameter.Name] -= 1;

                                                    float o2FlowSpeed = 0f;
                                                    if (param.Unit == 0)//VVM
                                                    {
                                                        o2FlowSpeed = MathF.Round((float)(param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                                    }
                                                    else if (param.Unit == 1)//L/min
                                                    {
                                                        o2FlowSpeed = param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue;
                                                    }
                                                    deviceParameter.O2Param.FlowSpeed = o2FlowSpeed;
                                                    deviceParameter.O2Param.IsControling = true;
                                                    O2RunCommand.Execute(deviceParameter);
                                                }
                                                else
                                                {
                                                    deviceParameter.O2Param.FlowSpeed = 0;
                                                    deviceParameter.O2Param.IsControling = true;
                                                    O2RunCommand.Execute(deviceParameter);
                                                }
                                            }
                                        }
                                        else if (factorIndex < collection.Count - 1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                    }
                                    else
                                    {
                                        float airFlowSpeed = 0f;
                                        if (param.Unit == 0)//VVM
                                        {
                                            airFlowSpeed = MathF.Round((float)(param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                        }
                                        else if (param.Unit == 1)//L/min
                                        {
                                            airFlowSpeed = param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue;
                                        }
                                        realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (Math.Abs(realTimeParam.AirFlowSpeed - airFlowSpeed) > 0.05)
                                        {
                                            deviceParameter.AirParam.FlowSpeed = airFlowSpeed;
                                        }

                                        if (isExistOtherGas)
                                        {
                                            float o2FlowSpeed = 0f;
                                            if (param.Unit == 0)//VVM
                                            {
                                                o2FlowSpeed = MathF.Round((float)(param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                            }
                                            else if (param.Unit == 1)//L/min
                                            {
                                                o2FlowSpeed = param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue;
                                            }
                                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                            if (Math.Abs(realTimeParam.O2FlowSpeed - o2FlowSpeed) > 0.05)
                                            {
                                                deviceParameter.O2Param.FlowSpeed = o2FlowSpeed;

                                            }
                                        }
                                    }

                                    sleepCount = 1;
                                    while (sleepCount > 0)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            dicDOStatus[currentDeviceParameter.Name] = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                            {
                                                dicDOStatus[currentDeviceParameter.Name] = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case DOControlFactor.O2:
                                    if (!deviceParameter.O2Param.IsControling)
                                    {
                                        deviceParameter.O2Param.IsControling = true;
                                        O2RunCommand.Execute(deviceParameter);
                                    }

                                    if (param.Unit == 0)//VVM
                                    {
                                        initialGas = MathF.Round((float)(param.O2Col[0].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                        maxGas = MathF.Round((float)(param.O2Col[param.O2Col.Count - 1].StepValue * (deviceParameter.JarWeight - 2000) / 1000), 2);
                                    }
                                    else if (param.Unit == 1)//L/min
                                    {
                                        initialGas = param.O2Col[0].StepValue;
                                        maxGas = param.O2Col[param.O2Col.Count - 1].StepValue;
                                    }

                                    isExistOtherGas = previousElements.Where(t => t == DOControlFactor.Air).Count() > 0;

                                    if (dicDODelta[deviceParameter.Name] <= param.AgitLowerLimit)
                                    {
                                        if (dicDOO2Index[deviceParameter.Name] > 0)//还存在上一阶梯
                                        {
                                            dicDOO2Index[deviceParameter.Name] -= 1;

                                            float o2FlowSpeed = 0f;
                                            if (param.Unit == 0)//VVM
                                            {
                                                o2FlowSpeed = MathF.Round((float)(param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                            }
                                            else if (param.Unit == 1)//L/min
                                            {
                                                o2FlowSpeed = param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue;
                                            }
                                            deviceParameter.O2Param.FlowSpeed = o2FlowSpeed;

                                            if (isExistOtherGas)
                                            {
                                                if (dicDOAirIndex[deviceParameter.Name] < param.AirCol.Count - 1)//加一档
                                                {
                                                    dicDOAirIndex[deviceParameter.Name] += 1;

                                                    float airFlowSpeed = 0f;
                                                    if (param.Unit == 0)//VVM
                                                    {
                                                        airFlowSpeed = MathF.Round((float)(param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                                    }
                                                    else if (param.Unit == 1)//L/min
                                                    {
                                                        airFlowSpeed = param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue;
                                                    }
                                                    deviceParameter.AirParam.FlowSpeed = airFlowSpeed;
                                                    deviceParameter.AirParam.IsControling = true;
                                                    AirRunCommand.Execute(deviceParameter);
                                                }
                                            }
                                        }
                                        else if (factorIndex > 0)//非第一因子，不需要最低通气量
                                        {
                                            if (isExistOtherGas)
                                            {
                                                deviceParameter.O2Param.FlowSpeed = 0;

                                                if (dicDOAirIndex[deviceParameter.Name] < param.AirCol.Count - 1)//加一档
                                                {
                                                    dicDOAirIndex[deviceParameter.Name] += 1;

                                                    float airFlowSpeed = 0f;
                                                    if (param.Unit == 0)//VVM
                                                    {
                                                        airFlowSpeed = MathF.Round((float)(param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                                    }
                                                    else if (param.Unit == 1)//L/min
                                                    {
                                                        airFlowSpeed = param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue;
                                                    }
                                                    deviceParameter.AirParam.FlowSpeed = airFlowSpeed;
                                                    deviceParameter.AirParam.IsControling = true;
                                                    AirRunCommand.Execute(deviceParameter);
                                                }
                                            }

                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }
                                    else if (dicDODelta[deviceParameter.Name] >= param.AgitUpperLimit)
                                    {
                                        if (dicDOO2Index[deviceParameter.Name] < param.O2Col.Count - 1)//还存在下一阶梯
                                        {
                                            dicDOO2Index[deviceParameter.Name] += 1;

                                            float o2FlowSpeed = 0f;
                                            if (param.Unit == 0)//VVM
                                            {
                                                o2FlowSpeed = MathF.Round((float)(param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                            }
                                            else if (param.Unit == 1)//L/min
                                            {
                                                o2FlowSpeed = param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue;
                                            }
                                            deviceParameter.O2Param.FlowSpeed = o2FlowSpeed;

                                            if (isExistOtherGas)
                                            {
                                                if (dicDOAirIndex[deviceParameter.Name] > 0)
                                                {
                                                    dicDOAirIndex[deviceParameter.Name] -= 1;

                                                    float airFlowSpeed = 0f;
                                                    if (param.Unit == 0)//VVM
                                                    {
                                                        airFlowSpeed = MathF.Round((float)(param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                                    }
                                                    else if (param.Unit == 1)//L/min
                                                    {
                                                        airFlowSpeed = param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue;
                                                    }
                                                    deviceParameter.AirParam.FlowSpeed = airFlowSpeed;
                                                    deviceParameter.AirParam.IsControling = true;
                                                    AirRunCommand.Execute(deviceParameter);
                                                }
                                                else
                                                {
                                                    deviceParameter.AirParam.FlowSpeed = 0;
                                                    deviceParameter.AirParam.IsControling = true;
                                                    AirRunCommand.Execute(deviceParameter);
                                                }
                                            }
                                        }
                                        else if (factorIndex < collection.Count - 1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                    }
                                    else
                                    {
                                        float o2FlowSpeed = 0f;
                                        if (param.Unit == 0)//VVM
                                        {
                                            o2FlowSpeed = MathF.Round((float)(param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                        }
                                        else if (param.Unit == 1)//L/min
                                        {
                                            o2FlowSpeed = param.O2Col[dicDOO2Index[deviceParameter.Name]].StepValue;
                                        }
                                        realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                        if (Math.Abs(realTimeParam.O2FlowSpeed - o2FlowSpeed) > 0.1)
                                        {
                                            deviceParameter.O2Param.FlowSpeed = o2FlowSpeed;

                                        }

                                        if (isExistOtherGas)
                                        {
                                            float airFlowSpeed = 0f;
                                            if (param.Unit == 0)//VVM
                                            {
                                                airFlowSpeed = MathF.Round((float)(param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue * (realTimeParam.JarWeight - 2000) / 1000), 2);
                                            }
                                            else if (param.Unit == 1)//L/min
                                            {
                                                airFlowSpeed = param.AirCol[dicDOAirIndex[deviceParameter.Name]].StepValue;
                                            }
                                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                            if (Math.Abs(realTimeParam.AirFlowSpeed - airFlowSpeed) > 0.1)
                                            {
                                                deviceParameter.AirParam.FlowSpeed = airFlowSpeed;
                                            }
                                        }
                                    }

                                    sleepCount = 1;
                                    while (sleepCount > 0)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            dicDOStatus[currentDeviceParameter.Name] = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                            {
                                                dicDOStatus[currentDeviceParameter.Name] = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                                case DOControlFactor.Temp:
                                    deviceParameter.DORegulationLimit = false;
                                    if (!deviceParameter.TempParam.IsControling)
                                    {
                                        TempRunCommand.Execute(deviceParameter);
                                        Thread.Sleep(1000);
                                    }

                                    if (dicDODelta[deviceParameter.Name] <= param.AgitLowerLimit)
                                    {
                                        if (dicDOTempIndex[deviceParameter.Name] > 0)//还存在上一阶梯
                                        {
                                            dicDOTempIndex[deviceParameter.Name] -= 1;

                                            deviceParameter.TempParam.Temp_PV = param.TempCol[dicDOTempIndex[deviceParameter.Name]].StepValue;
                                        }
                                        else if (factorIndex > 0)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;

                                            deviceParameter.TempParam.Temp_PV = deviceParameter.DOParam.InitialTemp;
                                        }
                                    }
                                    else if (dicDODelta[deviceParameter.Name] >= param.AgitUpperLimit)
                                    {
                                        if (dicDOTempIndex[deviceParameter.Name] < param.TempCol.Count - 1)//还存在下一阶梯
                                        {
                                            dicDOTempIndex[deviceParameter.Name] += 1;

                                            deviceParameter.TempParam.Temp_PV = param.TempCol[dicDOTempIndex[deviceParameter.Name]].StepValue;
                                        }
                                        else if (factorIndex < collection.Count - 1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;

                                            deviceParameter.TempParam.Temp_PV = deviceParameter.DOParam.InitialTemp;
                                        }
                                    }
                                    else
                                    {
                                        deviceParameter.TempParam.Temp_PV = param.TempCol[dicDOTempIndex[deviceParameter.Name]].StepValue;
                                    }

                                    if (dicDOTempIndex[deviceParameter.Name] <= 0 || dicDOTempIndex[deviceParameter.Name] >= dicDOTempIndex.Count - 1)
                                    {
                                        while (true)
                                        {
                                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                            {
                                                dicDOStatus[currentDeviceParameter.Name] = false;
                                                return;
                                            }
                                            while (AppSession.DOPause)
                                            {
                                                if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                                {
                                                    dicDOStatus[currentDeviceParameter.Name] = false;
                                                    return;
                                                }
                                                Thread.Sleep(1000);
                                            }
                                            realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(deviceParameter.Name);
                                            if (Math.Abs(realTimeParam.Temp - deviceParameter.TempParam.Temp_PV) <= 0.2)
                                            {
                                                break;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    break;
                                case DOControlFactor.Feed:
                                    if (!deviceParameter.FeedParam1.IsControling)
                                    {
                                        if (factorIndex < collection.Count - 1 && lastFactorIndex <= factorIndex)//如果还有下一执行参数，则跳到下一个执行参数
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;
                                        }
                                        else if (factorIndex - 1 > -1 && lastFactorIndex >= factorIndex)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;
                                        }
                                    }

                                    if (firstInitFeed)
                                    {
                                        deviceParameter.FeedSuspend = true;
                                        deviceParameter.DOParam.InitialFeed = deviceParameter.FeedParam1.Feed_PV;
                                        firstInitFeed = false;
                                    }

                                    if (dicDODelta[deviceParameter.Name] <= param.AgitLowerLimit)
                                    {
                                        if (dicDOFeedIndex[deviceParameter.Name] > 0)//还存在上一阶梯
                                        {
                                            dicDOFeedIndex[deviceParameter.Name] -= 1;

                                            float coeff = param.FeedCol[dicDOFeedIndex[deviceParameter.Name]].StepValue;
                                            deviceParameter.FeedParam1.Feed_PV = MathF.Round(deviceParameter.DOParam.InitialFeed * coeff / 100, 2);
                                        }
                                        else if (factorIndex > 0)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex -= 1;

                                            deviceParameter.FeedParam1.Feed_PV = deviceParameter.DOParam.InitialFeed;
                                        }
                                    }
                                    else if (dicDODelta[deviceParameter.Name] >= param.AgitUpperLimit)
                                    {
                                        if (dicDOFeedIndex[deviceParameter.Name] < param.FeedCol.Count - 1)//还存在下一阶梯
                                        {
                                            dicDOFeedIndex[deviceParameter.Name] += 1;

                                            float coeff = param.FeedCol[dicDOFeedIndex[deviceParameter.Name]].StepValue;
                                            deviceParameter.FeedParam1.Feed_PV = MathF.Round(deviceParameter.DOParam.InitialFeed * coeff / 100, 2);
                                        }
                                        else if (factorIndex < collection.Count - 1)
                                        {
                                            lastFactorIndex = factorIndex;
                                            factorIndex += 1;

                                            deviceParameter.FeedParam1.Feed_PV = deviceParameter.DOParam.InitialFeed;
                                        }
                                    }
                                    else
                                    {
                                        float coeff = param.FeedCol[dicDOFeedIndex[deviceParameter.Name]].StepValue;
                                        deviceParameter.FeedParam1.Feed_PV = MathF.Round(deviceParameter.DOParam.InitialFeed * coeff / 100, 2);
                                    }

                                    PeristalticPump pump = PeristalticPump.FeedPump;
                                    int pumpNo = PumpMFCUtil.GetPumpIndex(deviceParameter.Name, pump);
                                    if (pumpNo >= 0)
                                    {
                                        var controlParam = new PeristalticPumpControlParam()
                                        {
                                            PumpNo = pumpNo,
                                            Pump = pump,
                                            ControlMode = PumpControlMode.Direct,
                                            FlowSpeed = deviceParameter.FeedParam1.Feed_PV,
                                            FlowCapacity = Const.MaxPumpFlowCapacity
                                        };
                                        InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(deviceParameter.Name, controlParam);
                                        dicFeed1SP[deviceParameter.Name] = deviceParameter.FeedParam1.Feed_PV;
                                    }
                                    LogHelper.Debug(string.Format("反应器{0} 起始补料{1} 实际补料{2}", deviceParameter.Name, deviceParameter.DOParam.InitialFeed, deviceParameter.FeedParam1.Feed_PV));

                                    sleepCount = 1;
                                    while (sleepCount > 0)
                                    {
                                        if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                        {
                                            dicDOStatus[currentDeviceParameter.Name] = false;
                                            return;
                                        }
                                        while (AppSession.DOPause)
                                        {
                                            if (dicDOWorker[deviceParameter.Name].CancellationPending)
                                            {
                                                dicDOStatus[currentDeviceParameter.Name] = false;
                                                return;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        sleepCount--;
                                        Thread.Sleep(1000);
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug(string.Format("DO调整失败：阶梯级联：错误信息：{0}", ex.Message));
                            Thread.Sleep(AppSession.Interval * 1000);
                        }
                    }
                }
            });
            dicDOWorker[currentDeviceParameter.Name].RunWorkerCompleted += ((s, e) =>
            {
                try
                {
                    var deviceParameter = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == e.Result?.ToString());
                    if (deviceParameter == null)
                    {
                        LogHelper.Debug(string.Format("溶氧控制：事件完成出错,未找到反应器{0}" + e.Result?.ToString()));
                        return;
                    }
                    deviceParameter.DOParam.IsControling = false;
                    deviceParameter.DORegulationLimit = false;
                    deviceParameter.FeedSuspend = false;

                    if (deviceParameter.TempParam.IsControling)
                    {
                        if (deviceParameter.DOParam.ControlStrategy == DOControlStrategy.Midranging)
                        {
                            var param = MidRangingParamManager.GetInstance().MidRangingParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                            if (param.FactorCol.Contains(DOControlFactor.Temp))
                            {
                                deviceParameter.TempParam.Temp_PV = deviceParameter.DOParam.InitialTemp;
                            }
                        }
                        else if (deviceParameter.DOParam.ControlStrategy == DOControlStrategy.Step)
                        {
                            var param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                            if (param.FactorCol.Contains(DOControlFactor.Temp))
                            {
                                deviceParameter.TempParam.Temp_PV = deviceParameter.DOParam.InitialTemp;
                            }
                        }
                    }

                    if (deviceParameter.FeedParam1.IsControling)
                    {
                        if (deviceParameter.DOParam.ControlStrategy == DOControlStrategy.Midranging)
                        {
                            var param = MidRangingParamManager.GetInstance().MidRangingParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                            if (param.FactorCol.Contains(DOControlFactor.Temp))
                            {
                                deviceParameter.FeedParam1.Feed_PV = deviceParameter.DOParam.InitialFeed;
                            }
                        }
                        else if (deviceParameter.DOParam.ControlStrategy == DOControlStrategy.Step)
                        {
                            var param = DOAssManager.GetInstance().DOAssParamCol.FindFirst(t => t.DeviceName == deviceParameter.Name);
                            if (param.FactorCol.Contains(DOControlFactor.Temp))
                            {
                                deviceParameter.FeedParam1.Feed_PV = deviceParameter.DOParam.InitialFeed;
                            }
                        }
                    }
                    BackgroundWorker backgroundWorker = s as BackgroundWorker;
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("溶氧控制：取消报错" + ex.Message);
                }
            });
            dicDOWorker[currentDeviceParameter.Name].RunWorkerAsync();
        });

        public DelegateCommand<DeviceParameter> PHRunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }
            //如果正在自动调控PH
            if (dicPHWorker.ContainsKey(currentDeviceParameter.Name) && dicPHWorker[currentDeviceParameter.Name] != null && dicPHWorker[currentDeviceParameter.Name].IsBusy)
            {
                dicPHWorker[currentDeviceParameter.Name].CancelAsync();
                while (dicPHStatus[currentDeviceParameter.Name])
                {
                    Thread.Sleep(100);
                }
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
                dicPHStatus[currentDeviceParameter.Name] = true;
                var deviceParameter = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
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
                            {
                                dicPHStatus[currentDeviceParameter.Name] = false;
                                return;
                            }

                            string result = File.ReadAllText(FileConst.PidInfoPath);
                            List<PIDInfo> pIDInfos = JsonConvert.DeserializeObject<List<PIDInfo>>(result);
                            if (pIDInfos == null)
                            {
                                MessageBox.Show("PID调控策略列表为空");
                                dicPHStatus[currentDeviceParameter.Name] = false;
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
                                dicPHStatus[currentDeviceParameter.Name] = false;
                                return;
                            }

                            //如果pid类型变了，pid系数清零 方成
                            if (info != null && lastPid != null && !info.Equals(lastPid))
                            {
                                dicPHPid[deviceParameter.Name].Reset();
                                if (info.PidName != lastPid.PidName)
                                {
                                    LogHelper.Debug(string.Format("PH调控：由{0}切换至{1}", lastPid.PidName, info.PidName));
                                    dicPHDelta[deviceParameter.Name] = 0;
                                }
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

                                int count = info.Interval <= 0 ? 1 : info.Interval;
                                int index = 0;
                                while (index < count)
                                {
                                    if (dicPHWorker[deviceParameter.Name].CancellationPending)
                                    {
                                        dicPHStatus[currentDeviceParameter.Name] = false;
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
                                {
                                    dicPHStatus[currentDeviceParameter.Name] = false;
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
                                dicPHStatus[currentDeviceParameter.Name] = false;
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
                                            {
                                                dicPHStatus[currentDeviceParameter.Name] = false;
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
                                        {
                                            dicPHStatus[currentDeviceParameter.Name] = false;
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
                            dicPHStatus[currentDeviceParameter.Name] = false;
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
                                                dicPHStatus[currentDeviceParameter.Name] = false;
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
                                                dicPHStatus[currentDeviceParameter.Name] = false;
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

                    var deviceParameter = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == e.Result?.ToString());
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

        public DelegateCommand<DeviceParameter> TempRunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }
            if (dicTempWorker.ContainsKey(currentDeviceParameter.Name) && dicTempWorker[currentDeviceParameter.Name] != null && dicTempWorker[currentDeviceParameter.Name].IsBusy)
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
                            InstrumentSolution.GetInstance().CommandWrapper.SetTempSetting(currentDeviceParameter.Name, currentDeviceParameter.TempParam);
                            dicTempSP[currentDeviceParameter.Name] = currentDeviceParameter.TempParam.Temp_PV;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("温控异常" + ex.Message);
                        }
                        var deviceParameter = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
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

                        var deviceParameter = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == e.Result?.ToString());
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

                        var deviceParameter = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
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

                        var deviceParameter = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
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

        public PadMainViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            //实时保存泵&MFC的信息
            var worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                while (true)
                {
                    if (CurrentDeviceParameter == null)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    AnalysisSolution.GetInstance().SaveAllSetting();
                    Thread.Sleep(1000);
                }
            };
            worker.RunWorkerAsync();

            //读取实时信息
            var worker1 = new BackgroundWorker();
            worker1.DoWork += (s, e) =>
            {
                while (true)
                {
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

                    Thread.Sleep(1000);
                }
            };
            worker1.RunWorkerAsync();
        }

        private void ResetDOParam(DeviceParameter deviceParameter)
        {
            deviceParameter.IsDOLimit = false;
            deviceParameter.DORegulationLimit = false;
            dicDOPid[deviceParameter.Name].Reset();
            dicDODelta[deviceParameter.Name] = 0;
            dicDOAirPid[deviceParameter.Name].Reset();
            dicDOO2Pid[deviceParameter.Name].Reset();
        }

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

        public void Configure()
        {
            if (AnalysisSolution.GetInstance().ReactorCol.Count < 1)
            {
                AnalysisSolution.GetInstance().ReactorCol.Add(new DeviceParameter() { Name = "G01" });
            }
            CurrentDeviceParameter = AnalysisSolution.GetInstance().ReactorCol[0];

            #region 标定
            foreach (var item in ClockSupervisor.realDatasDic.Keys)
            {
                SelectedReactor = item;
                break;
            }

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += (s, e) =>
            {
                BackgroundWorker worker = (BackgroundWorker)s;
                while (true)
                {
                    if (worker.CancellationPending)
                    {
                        return;
                    }
                    try
                    {
                        if (string.IsNullOrWhiteSpace(SelectedReactor))
                        {
                            CurrentValue = 0;
                            continue;
                        }
                        int index = ClockSupervisor.realDatasDic[SelectedReactor].Count - 1;
                        var realTimeParam = ClockSupervisor.realDatasDic[SelectedReactor][index];
                        switch (SensorType)
                        {
                            case SensorType.pH:
                                CurrentValue = (float)realTimeParam.PH;
                                break;
                            case SensorType.DO:
                                CurrentValue = (float)realTimeParam.DO;
                                break;
                            case SensorType.JarWeight:
                                CurrentValue = (float)realTimeParam.JarWeight;
                                break;
                            case SensorType.Bottle1Weight:
                                CurrentValue = (float)realTimeParam.Bottle1Weight;
                                break;
                            case SensorType.Bottle2Weight:
                                CurrentValue = (float)realTimeParam.Bottle2Weight;
                                break;
                            case SensorType.PT100:
                                CurrentValue = (float)realTimeParam.Temp;
                                break;
                            case SensorType.pHTemp:
                                CurrentValue = (float)realTimeParam.PHSensorTemp;
                                break;
                            case SensorType.DOTemp:
                                CurrentValue = (float)realTimeParam.DOSensorTemp;
                                break;
                            case SensorType.TempControlNTC1:
                                CurrentValue = (float)realTimeParam.HeatingBaseCoolingNTCTemp;
                                break;
                            case SensorType.TempControlNTC2:
                                CurrentValue = (float)realTimeParam.HeatingBaseHeatingNTCTemp;
                                break;
                            case SensorType.CoolingModuleNTC1:
                                CurrentValue = (float)realTimeParam.CoolingModuleCoolingNTCTemp;
                                break;
                            case SensorType.CoolingModuleNTC2:
                                CurrentValue = (float)realTimeParam.CoolingModuleHeatingNTCTemp;
                                break;
                            case SensorType.CoolingModuleNTC3:
                                CurrentValue = (float)realTimeParam.CoolingModuleRoomNTCTemp;
                                break;
                        }
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

            worker1 = new BackgroundWorker();
            worker1.WorkerReportsProgress = true;
            worker1.WorkerSupportsCancellation = true;
            worker1.DoWork += (s, e) =>
            {
                BackgroundWorker worker = (BackgroundWorker)s;
                while (true)
                {
                    if (worker.CancellationPending)
                    {
                        return;
                    }
                    try
                    {
                        if (string.IsNullOrWhiteSpace(SelectedReactor))
                        {
                            Coefficient = 0;
                            Bias = 0;
                            StatusCode = 0;
                            continue;
                        }
                        var param = InstrumentSolution.GetInstance().CommandWrapper.GetSensorCorrect(SelectedReactor, (byte)SensorType);
                        Coefficient = param.Coefficient;
                        Bias = param.Bias;
                        StatusCode = param.StatusCode;
                    }
                    catch (Exception ex)
                    {
                        Coefficient = 0;
                        Bias = 0;
                        StatusCode = 0;
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
            };
            worker1.RunWorkerAsync();

            worker2 = new BackgroundWorker();
            worker2.WorkerReportsProgress = true;
            worker2.WorkerSupportsCancellation = true;
            worker2.DoWork += (s, e) =>
            {
                BackgroundWorker worker = (BackgroundWorker)s;
                while (true)
                {
                    if (worker.CancellationPending)
                    {
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(SelectedReactor))
                    {
                        ResetPumpInfo();
                    }

                    try
                    {
                        var temp = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpCorrect(SelectedReactor, Pump1Param.PumpIndex);
                        Pump1Param.OldCoefficient = temp;
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        var temp = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpCorrect(SelectedReactor, Pump2Param.PumpIndex);
                        Pump2Param.OldCoefficient = temp;
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        var temp = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpCorrect(SelectedReactor, Pump3Param.PumpIndex);
                        Pump3Param.OldCoefficient = temp;
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        var temp = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpCorrect(SelectedReactor, Pump4Param.PumpIndex);
                        Pump4Param.OldCoefficient = temp;
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        var temp = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpCorrect(SelectedReactor, Pump5Param.PumpIndex);
                        Pump5Param.OldCoefficient = temp;
                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {
                        var temp = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpCorrect(SelectedReactor, Pump6Param.PumpIndex);
                        Pump6Param.OldCoefficient = temp;
                    }
                    catch (Exception ex)
                    {

                    }


                    Thread.Sleep(1000);
                }
            };
            worker2.RunWorkerAsync();

            ResetPumpInfo();
            #endregion
        }
    }
}
