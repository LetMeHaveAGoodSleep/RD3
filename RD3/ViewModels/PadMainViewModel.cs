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


        public PadMainViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {

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

            {
                BackgroundWorker worker = new BackgroundWorker();
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

                BackgroundWorker worker1 = new BackgroundWorker();
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

                BackgroundWorker worker2 = new BackgroundWorker();
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
            }

            #endregion


            {
                //读取实时信息&实时保存泵和MFC的信息
                var worker = new BackgroundWorker();
                worker.DoWork += (s, e) =>
                {
                    while (true)
                    {
                        try
                        {
                            AnalysisSolution.GetInstance().SaveAllSetting();


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
}
