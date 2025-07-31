using ImTools;
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
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public DelegateCommand DeviceSettingCommand => new(() => 
        {

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
                    AnalysisSolution.GetInstance().SavePumpSetting();
                    AnalysisSolution.GetInstance().SaveMFCSetting();
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
                        var param = InstrumentSolution.GetInstance().CommandWrapper.GetSensorCorrect(SelectedReactor, SensorType);
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
