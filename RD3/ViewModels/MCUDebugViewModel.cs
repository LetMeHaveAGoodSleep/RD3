using Fpi.Instruments;
using HandyControl.Controls;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Common.Events;
using RD3.Extensions;
using RD3.Shared;
using RD3.Views;
using ScottPlot.Colormaps;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class MCUDebugViewModel : BaseViewModel,IDialogAware
    {
        private bool _pt100LoactionCheckEnable = true;
        public bool PT100LocationCheckEnable
        {
            get { return _pt100LoactionCheckEnable; }
            set { SetProperty(ref _pt100LoactionCheckEnable, value); }
        }

        private SubscriptionToken token = null;

        private DateTime _selectedTime = DateTime.Now;
        public DateTime SelectedTime
        {
            get { return _selectedTime; }
            set { SetProperty(ref _selectedTime, value); }
        }

        private int _runningHour = 0;
        public int RunningHour
        {
            get { return _runningHour; }
            set { SetProperty(ref _runningHour, value); }
        }

        private int _runningMinute = 0;
        public int RunningMinute
        {
            get { return _runningMinute; }
            set { SetProperty(ref _runningMinute, value); }
        }

        private int _runningSecond = 0;
        public int RunningSecond
        {
            get { return _runningSecond; }
            set { SetProperty(ref _runningSecond, value); }
        }

        private float delta = 0;

        private int delta1 = 0;

        QPIDController pid = new QPIDController();

        QPIDController pid1 = new QPIDController();

        private BackgroundWorker backgroundWorker = new BackgroundWorker();

        private BackgroundWorker backgroundWorker1 = new BackgroundWorker();

        private bool _doAuto = false;
        public bool DOAuto
        {
            get { return _doAuto; }
            set { SetProperty(ref _doAuto, value); }
        }

        private bool _phAuto = false;
        public bool PHAuto
        {
            get { return _phAuto; }
            set { SetProperty(ref _phAuto, value); }
        }

        private double _ph_SP = 0f;
        public double PH_SP
        {
            get { return _ph_SP; }
            set { SetProperty(ref _ph_SP, value); }
        }

        private double _kp = 0f;
        public double Kp
        {
            get { return _kp; }
            set { SetProperty(ref _kp, value); }
        }

        private double _ki = 0f;
        public double Ki
        {
            get { return _ki; }
            set { SetProperty(ref _ki, value); }
        }

        private double _kd = 0f;
        public double Kd
        {
            get { return _kd; }
            set { SetProperty(ref _kd, value); }
        }

        private double _threshold = 0.5f;
        public double Threshold
        {
            get { return _threshold; }
            set { SetProperty(ref _threshold, value); }
        }

        private double _outputLimit = 0f;
        public double OutputLimit
        {
            get { return _outputLimit; }
            set { SetProperty(ref _outputLimit, value); }
        }

        private double _integralLimit = 0f;
        public double IntegralLimit
        {
            get { return _integralLimit; }
            set { SetProperty(ref _integralLimit, value); }
        }

        private double _do_SP = 0f;
        public double DO_SP
        {
            get { return _do_SP; }
            set { SetProperty(ref _do_SP, value); }
        }

        private double _do_kp = 0f;
        public double DO_Kp
        {
            get { return _do_kp; }
            set { SetProperty(ref _do_kp, value); }
        }

        private double _do_ki = 0f;
        public double DO_Ki
        {
            get { return _do_ki; }
            set { SetProperty(ref _do_ki, value); }
        }

        private double _do_kd = 0f;
        public double DO_Kd
        {
            get { return _do_kd; }
            set { SetProperty(ref _do_kd, value); }
        }

        private double _do_threshold = 2f;
        public double DO_Threshold
        {
            get { return _do_threshold; }
            set { SetProperty(ref _do_threshold, value); }
        }

        private double _do_outputLimit = 0f;
        public double DO_OutputLimit
        {
            get { return _do_outputLimit; }
            set { SetProperty(ref _do_outputLimit, value); }
        }

        private double _do_outputLimit1 = 0f;
        public double DO_OutputLimit1
        {
            get { return _do_outputLimit1; }
            set { SetProperty(ref _do_outputLimit1, value); }
        }
        

        private double _do_integralLimit = 0f;
        public double DO_IntegralLimit
        {
            get { return _do_integralLimit; }
            set { SetProperty(ref _do_integralLimit, value); }
        }


        private TempParam _tempParam = new();
        public TempParam TempParam
        {
            get { return _tempParam; }
            set { SetProperty(ref _tempParam, value); }
        }

        private AgitParam _agitParam = new();
        public AgitParam AgitParam
        {
            get { return _agitParam; }
            set { SetProperty(ref _agitParam, value); }
        }

        private PeristalticPumpControlParam _acidParam = new() { Pump = PeristalticPump.AcidPump, PumpNo = 1 };
        public PeristalticPumpControlParam AcidParam
        {
            get { return _acidParam; }
            set { SetProperty(ref _acidParam, value); }
        }

        private PeristalticPumpControlParam _baseParam = new() { Pump = PeristalticPump.BasePump, PumpNo = 2 };
        public PeristalticPumpControlParam BaseParam
        {
            get { return _baseParam; }
            set { SetProperty(ref _baseParam, value); }
        }

        private PeristalticPumpControlParam _feedParam = new() { Pump = PeristalticPump.FeedPump, PumpNo = 3 };
        public PeristalticPumpControlParam FeedParam
        {
            get { return _feedParam; }
            set { SetProperty(ref _feedParam, value); }
        }

        private PeristalticPumpControlParam _aFParam = new() { Pump = PeristalticPump.AFPump, PumpNo = 4 };
        public PeristalticPumpControlParam AFParam
        {
            get { return _aFParam; }
            set { SetProperty(ref _aFParam, value); }
        }

        private PeristalticPumpControlParam _pump5Param = new() { Pump = PeristalticPump.FeedPump, PumpNo = 5 };
        public PeristalticPumpControlParam Pump5Param
        {
            get { return _pump5Param; }
            set { SetProperty(ref _pump5Param, value); }
        }

        private PeristalticPumpControlParam _pump6Param = new() { Pump = PeristalticPump.FeedPump, PumpNo = 6 };
        public PeristalticPumpControlParam Pump6Param
        {
            get { return _pump6Param; }
            set { SetProperty(ref _pump6Param, value); }
        }

        private DefoamingParam _defoamingParam = new();
        public DefoamingParam DefoamingParam
        {
            get { return _defoamingParam; }
            set { SetProperty(ref _defoamingParam, value); }
        }

        private GasParam _airParam = new() { GasType = GasType.Air, MFCNo = 1 };
        public GasParam AirParam
        {
            get { return _airParam; }
            set { SetProperty(ref _airParam, value); }
        }

        private GasParam _o2Param = new() { GasType = GasType.O2, MFCNo = 2 };
        public GasParam O2Param
        {
            get { return _o2Param; }
            set { SetProperty(ref _o2Param, value); }
        }

        private GasParam _cO2Param = new() { GasType = GasType.CO2, MFCNo = 3 };
        public GasParam CO2Param
        {
            get { return _cO2Param; }
            set { SetProperty(ref _cO2Param, value); }
        }

        private GasParam _n2Param = new() { GasType = GasType.N2, MFCNo = 4 };
        public GasParam N2Param
        {
            get { return _n2Param; }
            set { SetProperty(ref _n2Param, value); }
        }

        private GasParam _mfc5Param = new() { GasType = GasType.N2, MFCNo = 5 };
        public GasParam MFC5Param
        {
            get { return _mfc5Param; }
            set { SetProperty(ref _mfc5Param, value); }
        }

        private Dictionary<int, List<string>> keyValuePairs = new Dictionary<int, List<string>>()
        {
            {0,["希尔曼-模拟","希尔曼-数字", "微基-模拟", "微基-数字"]},
            {1,["希尔曼-数字","微基-数字","昇辉-数字"]}
        };

        private int _selectedSensorKindIndex = 0;
        public int SelectedSensorKindIndex
        {
            get { return _selectedSensorKindIndex; }
            set
            {
                if (_selectedSensorKindIndex != value && value != -1)
                {
                    _dataSource.Clear();
                    _dataSource.AddRange(keyValuePairs[value]);
                    SelectedSensorKindIndex1 = 0;
                }
                SetProperty(ref _selectedSensorKindIndex, value);
            }
        }

        private int _selectedSensorKindIndex1 = 0;
        public int SelectedSensorKindIndex1
        {
            get { return _selectedSensorKindIndex1; }
            set { SetProperty(ref _selectedSensorKindIndex1, value); }
        }

        private ObservableCollection<string> _dataSource = ["希尔曼-模拟", "希尔曼-数字", "微基-模拟", "微基-数字"];
        public ObservableCollection<string> DataSource
        {
            get { return _dataSource; }
            set { SetProperty(ref _dataSource, value); }
        }


        private ObservableCollection<string> _stirringMotorSource = [" ", "IDS-R", "智创"];
        public ObservableCollection<string> StirringMotorSource
        {
            get { return _stirringMotorSource; }
            set { SetProperty(ref _stirringMotorSource, value); }
        }

        private int _selectedStirringMotorType = 2;
        public int SelectedStirringMotorType
        {
            get { return _selectedStirringMotorType; }
            set { SetProperty(ref _selectedStirringMotorType, value); }
        }

        private MCUBoardType _selectedMCUBoardType = MCUBoardType.MainBoard;
        public MCUBoardType SelectedMCUBoardType
        {
            get { return _selectedMCUBoardType; }
            set { SetProperty(ref _selectedMCUBoardType, value); }
        }

        private string _softwareVersion;
        public string SoftwareVersion
        {
            get { return _softwareVersion; }
            set { SetProperty(ref _softwareVersion, value); }
        }

        private RealTimeParam _realTimeParam = new();
        public RealTimeParam RealTimeParam
        {
            get { return _realTimeParam; }
            set { SetProperty(ref _realTimeParam, value); }
        }

        private TECParam _tECParam = new();
        public TECParam TECParam
        {
            get { return _tECParam; }
            set { SetProperty(ref _tECParam, value); }
        }

        private DeviceParam _deviceParam = new();
        public DeviceParam DeviceParam
        {
            get { return _deviceParam; }
            set { SetProperty(ref _deviceParam, value); }
        }

        private int _selectedSensorIndex = -1;
        public int SelectedSensorIndex
        {
            get { return _selectedSensorIndex; }
            set 
            {
                if (_selectedSensorIndex != value)
                {
                    SensorCorrectParam = new();
                    SensorCorrectParam.SensorType = (SensorType)(value + 1);
                }
                SetProperty(ref _selectedSensorIndex, value); 
            }
        }

        private SensorCorrectParam _sensorCorrectParam = new();
        public SensorCorrectParam SensorCorrectParam
        {
            get { return _sensorCorrectParam; }
            set { SetProperty(ref _sensorCorrectParam, value); }
        }

        private PeristalticPumpCalibrationParam _peristalticPumpCalibrationParam = new();
        public PeristalticPumpCalibrationParam PeristalticPumpCalibrationParam
        {
            get { return _peristalticPumpCalibrationParam; }
            set { SetProperty(ref _peristalticPumpCalibrationParam, value); }
        }

        private ClearModule _selectedClearModule = ClearModule.Pump;
        public ClearModule SelectedClearModule
        {
            get { return _selectedClearModule; }
            set { SetProperty(ref _selectedClearModule, value); }
        }

        private int _clearModuleIndex = 1;
        public int ClearModuleIndex
        {
            get { return _clearModuleIndex; }
            set
            {
                SetProperty(ref _clearModuleIndex, value);
            }
        }

        public Instrument[] Instruments
        {
            get
            {
                List<Instrument> list = [];
                foreach (var item in InstrumentSolution.GetInstance().Instruments)
                {
                    if (AppSession.CurrentUser.DevieceIDs.Contains(item.id))
                    {
                        list.Add(item);
                    }
                }
                return list.ToArray();
            }
        }

        public Instrument SelectedInstrument
        {
            get
            {
                if (SelectedInstruments.Length > 0)
                {
                    return SelectedInstruments[0];
                }
                return null;
            }
        }

        private Instrument[] _selectedInstrument = [];
        public Instrument[] SelectedInstruments
        {
            get { return _selectedInstrument; }
            set { SetProperty(ref _selectedInstrument, value); }
        }


        private AlarmParam _alarmParam = new();

        public event Action<IDialogResult> RequestClose;

        public AlarmParam AlarmParam
        {
            get { return _alarmParam; }
            set { SetProperty(ref _alarmParam, value); }
        }

        private CondensationParam _condensationParam = new();
        public CondensationParam CondensationParam
        {
            get { return _condensationParam; }
            set { SetProperty(ref _condensationParam, value); }
        }

        private OffGasParam _offGasParam = new();
        public OffGasParam OffGasParam
        {
            get { return _offGasParam; }
            set { SetProperty(ref _offGasParam, value); }
        }

        private string _mcuDownloadStr;
        public string MCUDownloadStr
        {
            get { return _mcuDownloadStr; }
            set { SetProperty(ref _mcuDownloadStr, value); }
        }

        private DeviceParam _mcuDownloadParam = new();
        public DeviceParam MCUDownloadParam
        {
            get { return _mcuDownloadParam; }
            set { SetProperty(ref _mcuDownloadParam, value); }
        }

        private float _pressure = 0f;
        public float Pressure
        {
            get { return _pressure; }
            set { SetProperty(ref _pressure, value); }
        }

        private MagneticBaseStatus _selectedMagneticBaseStatus = MagneticBaseStatus.Unset;
        public MagneticBaseStatus SelectedMagneticBaseStatus
        {
            get { return _selectedMagneticBaseStatus; }
            set { SetProperty(ref _selectedMagneticBaseStatus, value); }
        }

        public int CommunicationProtocol
        {
            get
            {
                string temp = VarConfig.GetValue("CommunicationProtocol")?.ToString();
                if (!int.TryParse(temp, out var result))
                {
                    return 0;
                }
                else
                {
                    return result;
                }
            }
        }
        public DelegateCommand<string> ReadCommand => new(async (string commandText) =>
        {
            try
            {
                if (SelectedInstrument == null)
                {
                    await DialogExtensions.Info("温馨提示", "请选择设备");
                    return;
                }
                var array = commandText.Split(',');
                commandText = array[0];
                if (commandText == "1")
                {
                    TempParam = InstrumentSolution.GetInstance().CommandWrapper.GetTempSetting(SelectedInstrument?.id);
                }
                else if (commandText == "2")
                {
                    AgitParam.SP = InstrumentSolution.GetInstance().CommandWrapper.GetAgitSpeed(SelectedInstrument?.id);
                }
                else if (commandText == "3")
                {
                    int.TryParse(array[1], out var pumpNo);
                    if (pumpNo == 1)
                    {
                        AcidParam = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpControlParam(SelectedInstrument?.id, pumpNo);
                    }
                    else if (pumpNo == 2)
                    {
                        BaseParam = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpControlParam(SelectedInstrument?.id, pumpNo);
                    }
                    else if (pumpNo == 3)
                    {
                        FeedParam = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpControlParam(SelectedInstrument?.id, pumpNo);
                    }
                    else if (pumpNo == 4)
                    {
                        AFParam = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpControlParam(SelectedInstrument?.id, pumpNo);
                    }
                    else if (pumpNo == 5)
                    {
                        Pump5Param = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpControlParam(SelectedInstrument?.id, pumpNo);
                    }
                    else if (pumpNo == 6)
                    {
                        Pump6Param = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpControlParam(SelectedInstrument?.id, pumpNo);
                    }
                }
                else if (commandText == "4")
                {
                    DefoamingParam = InstrumentSolution.GetInstance().CommandWrapper.GetAutoDefoamingSetting(SelectedInstrument?.id);
                }
                else if (commandText == "5")
                {
                    //GasType gas = (GasType)Enum.Parse(typeof(GasType), array[1]);
                    int.TryParse(array[1], out var mfcNo);
                    if (mfcNo == 1)
                    {
                        AirParam = InstrumentSolution.GetInstance().CommandWrapper.GetGasSpeed(SelectedInstrument?.id, AirParam);
                    }
                    else if (mfcNo == 2)
                    {
                        O2Param = InstrumentSolution.GetInstance().CommandWrapper.GetGasSpeed(SelectedInstrument?.id, O2Param);
                    }
                    else if (mfcNo == 3)
                    {
                        CO2Param = InstrumentSolution.GetInstance().CommandWrapper.GetGasSpeed(SelectedInstrument?.id, CO2Param);
                    }
                    else if (mfcNo == 4)
                    {
                        N2Param = InstrumentSolution.GetInstance().CommandWrapper.GetGasSpeed(SelectedInstrument?.id, N2Param);
                    }
                    else if (mfcNo == 5)
                    {
                        MFC5Param = InstrumentSolution.GetInstance().CommandWrapper.GetGasSpeed(SelectedInstrument?.id, MFC5Param);
                    }
                }
                else if (commandText == "6")
                {
                    var tuple = InstrumentSolution.GetInstance().CommandWrapper.GetMCUSensorTypeSetting(SelectedInstrument?.id, SelectedSensorKindIndex + 1);
                    SelectedSensorKindIndex = tuple.Item1 - 1;
                    SelectedSensorKindIndex1 = tuple.Item2 - 1 < -1 ? -1 : tuple.Item2 - 1;
                }
                else if (commandText == "9")
                {
                    RealTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(SelectedInstrument?.id);
                }
                else if (commandText == "10")
                {
                    SoftwareVersion = InstrumentSolution.GetInstance().CommandWrapper.GetMCUVersion(SelectedInstrument?.id, (byte)SelectedMCUBoardType);
                }
                else if (commandText == "12")
                {
                    TECParam = InstrumentSolution.GetInstance().CommandWrapper.GetTECPID(SelectedInstrument?.id);
                }
                else if (commandText == "13")
                {
                    DeviceParam = InstrumentSolution.GetInstance().CommandWrapper.GetDeviceParam(SelectedInstrument?.id);
                }
                else if (commandText == "14")
                {
                    var param = InstrumentSolution.GetInstance().CommandWrapper.GetSensorCorrect(SelectedInstrument?.id, (byte)SensorCorrectParam.SensorType);
                    SensorCorrectParam.SensorType = param.SensorType;
                    SensorCorrectParam.Coefficient = param.Coefficient;
                    SensorCorrectParam.Bias= param.Bias;
                    SensorCorrectParam.StatusCode = param.StatusCode;
                }
                else if (commandText == "18")
                {
                    PeristalticPumpCalibrationParam.Coefficient = InstrumentSolution.GetInstance().CommandWrapper.GetPeristalticPumpCorrect(SelectedInstrument?.id, PeristalticPumpCalibrationParam.PumpNo);
                }
                else if (commandText == "22")
                {
                    AlarmParam = InstrumentSolution.GetInstance().CommandWrapper.GetSoundLightAlarm(SelectedInstrument?.id);
                }
                else if (commandText == "23")
                {
                    var result = InstrumentSolution.GetInstance().CommandWrapper.GetTimeSync(SelectedInstrument?.id);
                    SelectedTime = result.Item1;
                    RunningHour = result.Item2.Hours;
                    RunningMinute = result.Item2.Minutes;
                    RunningSecond = result.Item2.Seconds;
                }
                else if (commandText == "25")
                {
                    CondensationParam = InstrumentSolution.GetInstance().CommandWrapper.GetCondensationControl(SelectedInstrument?.id);
                }
                else if (commandText == "26")
                {
                    OffGasParam = InstrumentSolution.GetInstance().CommandWrapper.GetOffGas(SelectedInstrument?.id);
                }
                else if (commandText == "27")
                {
                    var downloadStatus = InstrumentSolution.GetInstance().CommandWrapper.GetMCUDownloadInfo(SelectedInstrument?.id);
                    MCUDownloadStr = downloadStatus.ToString();
                }
                else if (commandText == "29")
                {
                    MCUDownloadParam = InstrumentSolution.GetInstance().CommandWrapper.GetMCUDownloadAdress(SelectedInstrument?.id);
                }
                else if (commandText == "30")
                {
                    SelectedStirringMotorType = InstrumentSolution.GetInstance().CommandWrapper.GetStirringMotorType(SelectedInstrument?.id);
                }
                else if (commandText == "31")
                {
                    Pressure = InstrumentSolution.GetInstance().CommandWrapper.GetEPCPressure(SelectedInstrument?.id);
                }
                else if (commandText == "32")
                {
                    SelectedMagneticBaseStatus = (MagneticBaseStatus)InstrumentSolution.GetInstance().CommandWrapper.GetMagneticBase(SelectedInstrument?.id);
                }
                else if (commandText == "33")
                {
                    PT100LocationCheckEnable = InstrumentSolution.GetInstance().CommandWrapper.GetPT100LocationCheckSetting(SelectedInstrument?.id);
                }
            }
            catch (Exception ex)
            {
                await DialogExtensions.Info("异常", ex.Message);
            }

        });

        public DelegateCommand<string> SetCommand => new(async (string commandText) =>
        {
            try
            {
                if (SelectedInstrument == null)
                {
                    await DialogExtensions.Info("温馨提示", "请选择设备");
                    return;
                }
                var array = commandText.Split(',');
                commandText = array[0];
                if (commandText == "1")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetTempSetting(SelectedInstrument?.id, TempParam);
                        }
                        catch (Exception ex)
                        {
                            await DialogExtensions.Info("异常", SelectedInstrument?.id + "设置失败;/r/n" + ex.Message);
                        }
                    }
                }
                else if (commandText == "2")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(SelectedInstrument?.id, AgitParam.SP);
                        }
                        catch (Exception ex)
                        {
                            await DialogExtensions.Info("异常", SelectedInstrument?.id + "设置失败;/r/n" + ex.Message);
                        }
                    }
                }
                else if (commandText == "3")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {

                            int.TryParse(array[1], out var pumpNo);
                            if (pumpNo == 1)
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, AcidParam);
                            }
                            else if (pumpNo == 2)
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, BaseParam);
                            }
                            else if (pumpNo == 3)
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, FeedParam);
                            }
                            else if (pumpNo == 4)
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, AFParam);
                            }
                            else if (pumpNo == 5)
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, Pump5Param);
                            }
                            else if (pumpNo == 6)
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, Pump6Param);
                            }
                        }
                        catch (Exception ex)
                        {
                            await DialogExtensions.Info("异常", SelectedInstrument?.id + "设置失败;/r/n" + ex.Message);
                        }
                    }
                }
                else if (commandText == "4")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetAutoDefoamingSetting(SelectedInstrument?.id,DefoamingParam);
                        }
                        catch (Exception ex)
                        {
                            await DialogExtensions.Info("异常", SelectedInstrument?.id + "设置失败;/r/n" + ex.Message);
                        }
                    }
                }
                else if (commandText == "5")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            int.TryParse(array[1], out var mfcNo);
                            if (mfcNo == 1)
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(SelectedInstrument?.id, AirParam);
                            }
                            else if (mfcNo == 2)
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(SelectedInstrument?.id, O2Param);
                            }
                            else if (mfcNo == 3)
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(SelectedInstrument?.id, CO2Param);
                            }
                            else if (mfcNo == 4)
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(SelectedInstrument?.id, N2Param);
                            }
                            else if (mfcNo == 5)
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(SelectedInstrument?.id, MFC5Param);
                            }
                        }
                        catch (Exception ex)
                        {
                            await DialogExtensions.Info("异常", SelectedInstrument?.id + "设置失败;/r/n" + ex.Message);
                        }
                    }
                }
                else if (commandText == "6")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetMCUSensorTypeSetting(SelectedInstrument?.id, SelectedSensorKindIndex + 1, SelectedSensorKindIndex1 + 1);
                        }
                        catch (Exception ex)
                        {
                            await DialogExtensions.Info("异常", SelectedInstrument?.id + "设置失败;/r/n" + ex.Message);
                        }
                    }
                }
                else if (commandText == "12")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetTECPID(SelectedInstrument?.id, TECParam);
                        }
                        catch (Exception ex)
                        {
                            await DialogExtensions.Info("异常", SelectedInstrument?.id + "设置失败;/r/n" + ex.Message);
                        }
                    }
                }
                else if (commandText == "13")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetDeviceParam(SelectedInstrument?.id, DeviceParam);
                        }
                        catch (Exception ex)
                        {
                            await DialogExtensions.Info("异常", SelectedInstrument?.id + "设置失败;/r/n" + ex.Message);
                        }
                    }
                }
                else if (commandText == "14")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetSensorCorrect(SelectedInstrument?.id, SensorCorrectParam);
                        }
                        catch (Exception ex)
                        {
                            await DialogExtensions.Info("异常", SelectedInstrument?.id + "设置失败;/r/n" + ex.Message);
                        }
                    }
                }
                else if (commandText == "18")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpCorrect(SelectedInstrument?.id, PeristalticPumpCalibrationParam.PumpNo, (int)PeristalticPumpCalibrationParam.CalibrationParam, PeristalticPumpCalibrationParam.CalibrationValue);
                        }
                        catch (Exception ex)
                        {
                            await DialogExtensions.Info("异常", SelectedInstrument?.id + "设置失败;/r/n" + ex.Message);
                        }
                    }
                }
                else if (commandText == "21")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetResetFlowCapacity(SelectedInstrument?.id, SelectedClearModule, ClearModuleIndex);
                        }
                        catch (Exception ex) { }
                    }      
                }
                else if (commandText == "22")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetSoundLightAlarm(SelectedInstrument?.id, AlarmParam);
                        }
                        catch (Exception ex) { }
                    }
                    
                }
                else if (commandText == "23")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetTimeSync(SelectedInstrument?.id, SelectedTime, new TimeSpan(RunningHour, RunningMinute, RunningSecond));
                        }
                        catch (Exception ex) { }
                    }
                   
                }
                else if (commandText == "24")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            ScreenParam screenParam = new ScreenParam()
                            {
                                DO = (float)DO_SP,
                                DOAuto = DOAuto,
                                PH = (float)PH_SP,
                                PhAuto = PHAuto,
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetSettingSync(SelectedInstrument?.id, screenParam);
                        }
                        catch (Exception ex) { }
                    } 
                }
                else if (commandText == "25")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetCondensationControl(SelectedInstrument?.id,CondensationParam);
                        }
                        catch (Exception ex) { }
                    }
                }
                else if (commandText == "27")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            if (SelectedMCUBoardType == MCUBoardType.TemperatureControlBoard && InstrumentSolution.GetInstance().CommandWrapper.GetType() == typeof(Real5LCommandWrapper))
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetMCUDownloadInfo(SelectedInstrument?.id, 0x02);
                            }
                            else
                            {
                                InstrumentSolution.GetInstance().CommandWrapper.SetMCUDownloadInfo(SelectedInstrument?.id, (byte)SelectedMCUBoardType);
                            }
                        }
                        catch (Exception ex) { }
                    }
                }
                else if (commandText == "28")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetResetDefaultSetting(SelectedInstrument?.id, SelectedClearModule, ClearModuleIndex);
                        }
                        catch (Exception ex) { }
                    }
                }
                else if (commandText == "29")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetMCUDownloadAdress(SelectedInstrument?.id, MCUDownloadParam);
                        }
                        catch (Exception ex) { }
                    }
                }
                else if (commandText == "30")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetStirringMotorType(SelectedInstrument?.id, SelectedStirringMotorType);
                        }
                        catch (Exception ex) { }
                    }
                }
                else if (commandText == "31")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetEPCPressure(SelectedInstrument?.id, Pressure);
                        }
                        catch (Exception ex) { }
                    }
                }
                else if (commandText == "32")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetMagneticBase(SelectedInstrument?.id, (byte)SelectedMagneticBaseStatus);
                        }
                        catch (Exception ex) { }
                    }
                }
                else if (commandText == "33")
                {
                    foreach (var SelectedInstrument in SelectedInstruments)
                    {
                        try
                        {
                            InstrumentSolution.GetInstance().CommandWrapper.SetPT100LocationCheckSetting(SelectedInstrument?.id, PT100LocationCheckEnable);
                        }
                        catch (Exception ex) { }
                    }
                }

            }
            catch (Exception ex)
            {
                await DialogExtensions.Info("异常", ex.Message);
            }

        });

        public DelegateCommand PHPIDCommand => new(() => 
        {
            if (SelectedInstrument == null)
            {
                 DialogExtensions.Info("温馨提示", "请选择设备");
                return;
            }

            if (backgroundWorker != null && backgroundWorker.IsBusy)
            {
                delta = 0;
                backgroundWorker.CancelAsync();
                try 
                {
                    //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.BasePump, SwitchMode.Close);
                    PeristalticPumpControlParam param1 = new PeristalticPumpControlParam()
                    {
                        Pump = PeristalticPump.BasePump,
                        ControlMode = PumpControlMode.Direct,
                        FlowSpeed = 0,
                        FlowCapacity = 0
                    };
                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, param1);

                    //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.AcidPump, SwitchMode.Close);
                    PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                    {
                        Pump = PeristalticPump.AcidPump,
                        ControlMode = PumpControlMode.Direct,
                        FlowSpeed = 0,
                        FlowCapacity = 0
                    };
                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, param);
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("PHPID取消报错" + ex.Message);
                }

                return;
            }
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;      // 允许报告进度
            backgroundWorker.WorkerSupportsCancellation = true; // 允许取消操作
            // 绑定事件
            backgroundWorker.DoWork += ((s, e) =>
            {
                pid.SetParameters(kp: (float)Kp, ki: (float)Ki, kd: (float)Kd, integralThreshold: (float)Threshold);
                pid.SetOutputLimits(-(float)Math.Abs(OutputLimit), (float)Math.Abs(OutputLimit));
                pid.SetIntegralLimits(-(float)Math.Abs(IntegralLimit), (float)Math.Abs(IntegralLimit));
                pid.SetTarget((float)PH_SP);

                while (true)
                {
                    try
                    {
                        RealTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(SelectedInstrument?.id);
                        if (backgroundWorker.CancellationPending)
                        { // 检查取消请求
                            e.Cancel = true;
                            break;
                        }
                        // 增量式使用
                        delta += pid.CalculateIncremental((float)RealTimeParam.PH);
                        if (delta < 0)//酸泵
                        {
                            //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.BasePump, SwitchMode.Close);
                            PeristalticPumpControlParam param1 = new PeristalticPumpControlParam()
                            {
                                Pump = PeristalticPump.BasePump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = 0,
                                FlowCapacity = 0
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, param1);

                            //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.AcidPump, SwitchMode.Open);
                            PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                            {
                                Pump = PeristalticPump.AcidPump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = Math.Abs(delta),
                                FlowCapacity = 100
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, param);
                        }
                        else if (delta > 0)//碱泵
                        {
                            //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.AcidPump, SwitchMode.Close);
                            PeristalticPumpControlParam param1 = new PeristalticPumpControlParam()
                            {
                                Pump = PeristalticPump.AcidPump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = 0,
                                FlowCapacity = 0
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, param1);

                            //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.BasePump, SwitchMode.Open);
                            PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                            {
                                Pump = PeristalticPump.BasePump,
                                ControlMode = PumpControlMode.Direct,
                                FlowSpeed = Math.Abs(delta),
                                FlowCapacity = 100
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, param);
                        }
                        else
                        {
                            break;
                        }

                        Thread.Sleep(5000);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug(string.Format("PHPID调整失败，错误信息：{0}", ex.Message));
                        Thread.Sleep(5000);
                    }
                }
            });
            backgroundWorker.RunWorkerCompleted += ((s, e) => 
            {
                try
                {
                    backgroundWorker.Dispose();
                    backgroundWorker = null;

                    //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.BasePump, SwitchMode.Close);
                    PeristalticPumpControlParam param1 = new PeristalticPumpControlParam()
                    {
                        Pump = PeristalticPump.BasePump,
                        ControlMode = PumpControlMode.Direct,
                        FlowSpeed = 0,
                        FlowCapacity = 0
                    };
                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, param1);

                    //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.AcidPump, SwitchMode.Close);
                    PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                    {
                        Pump = PeristalticPump.AcidPump,
                        ControlMode = PumpControlMode.Direct,
                        FlowSpeed = 0,
                        FlowCapacity = 0
                    };
                    InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, param);
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("PHPID完成事件报错" + ex.Message);
                }

            });
            backgroundWorker.RunWorkerAsync();

            // 位置式使用 
            //float output = pid.CalculatePositional(currentValue);
        });

        public DelegateCommand StopPHPIDCommand => new(() =>
        {
            try
            {

                if (backgroundWorker != null && backgroundWorker.IsBusy)
                {
                    delta = 0;
                    backgroundWorker.CancelAsync();
                }

                //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.BasePump, SwitchMode.Close);
                PeristalticPumpControlParam param1 = new PeristalticPumpControlParam()
                {
                    Pump = PeristalticPump.BasePump,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = 0,
                    FlowCapacity = 0
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, param1);

                //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.AcidPump, SwitchMode.Close);
                PeristalticPumpControlParam param = new PeristalticPumpControlParam()
                {
                    Pump = PeristalticPump.AcidPump,
                    ControlMode = PumpControlMode.Direct,
                    FlowSpeed = 0,
                    FlowCapacity = 0
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetPeristalticPumpControlParam(SelectedInstrument?.id, param);
            }
            catch (Exception ex)
            {
                LogHelper.Debug("取消PHPID调控报错" + ex.Message);
            }

            return;

        });

        public DelegateCommand DOPIDCommand => new(() =>
        {
            if (SelectedInstrument == null)
            {
                DialogExtensions.Info("温馨提示", "请选择设备");
                return;
            }

            if (backgroundWorker1 != null && backgroundWorker1.IsBusy)
            {
                delta1 = 0;
                backgroundWorker1.CancelAsync();
                try
                {
                    //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.Agit, SwitchMode.Close);
                    InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(SelectedInstrument?.id, 0);
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("溶氧pid1取消报错" + ex.Message);
                }

                return;
            }
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.WorkerReportsProgress = true;      // 允许报告进度
            backgroundWorker1.WorkerSupportsCancellation = true; // 允许取消操作
            // 绑定事件
            backgroundWorker1.DoWork += ((s, e) =>
            {
                pid1.SetParameters(kp: (float)DO_Kp, ki: (float)DO_Ki, kd: (float)DO_Kd, integralThreshold: (float)DO_Threshold);
                pid1.SetOutputLimits(-9999, 9999);
                pid1.SetIntegralLimits(-(float)Math.Abs(DO_IntegralLimit), (float)Math.Abs(DO_IntegralLimit));
                pid1.SetTarget((float)DO_SP);

                //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.Agit, SwitchMode.Open);
                InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(SelectedInstrument?.id, 0);
                while (true)
                {
                    try
                    {
                        RealTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime(SelectedInstrument?.id);
                        if (backgroundWorker1.CancellationPending)
                        { // 检查取消请求
                            e.Cancel = true;
                            break;
                        }
                        // 增量式使用
                        float temp = pid1.CalculateIncremental_DO((float)RealTimeParam.DO);
                        LogHelper.Warn(string.Format("Delta:{0}", delta1));
                        delta1 = Convert.ToInt32(delta1 + temp);//四舍五入
                        if (delta1 >= DO_OutputLimit)
                        {
                            delta1 = Convert.ToInt32(DO_OutputLimit);
                        } else if(delta1 <= DO_OutputLimit1)
                        {
                            delta1 = Convert.ToInt32(DO_OutputLimit1);
                        }
                        InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(SelectedInstrument?.id, delta1);

                        Thread.Sleep(5000);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug(string.Format("溶氧pid调整失败，错误信息：{0}", ex.Message));
                        Thread.Sleep(5000);
                    }
                }
            });
            backgroundWorker1.RunWorkerCompleted += ((s, e) =>
            {
                try
                {
                    backgroundWorker1.Dispose();
                    backgroundWorker1 = null;

                    //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.Agit, SwitchMode.Close);
                    InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(SelectedInstrument?.id, 0);
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("溶氧pid完成事件报错" + ex.Message);
                }

            });
            backgroundWorker1.RunWorkerAsync();

            // 位置式使用 
            //float output = pid1.CalculatePositional(currentValue);
        });

        public DelegateCommand StopDOPIDCommand => new(() =>
        {
            try
            {

                if (backgroundWorker1 != null && backgroundWorker1.IsBusy)
                {
                    delta = 0;
                    backgroundWorker1.CancelAsync();
                }

                //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(SelectedInstrument?.id, ControlObject.Agit, SwitchMode.Close);
                InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(SelectedInstrument?.id, 0);
            }
            catch (Exception ex)
            {
                LogHelper.Debug("取消溶氧PID调控报错" + ex.Message);
            }

            return;

        });

        public string Title => "工程调试";

        public MCUDebugViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            SelectedInstruments = [];
            token?.Dispose();
            token = null;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            //token = aggregator.ResgiterMessage((MessageModel model) =>
            //   {
            //       //RealTimeParam realTimeParam = model.Model as RealTimeParam;
            //       //if (model.Message == SelectedInstrument?.id)
            //       //{
            //       //    RealTimeParam.Temp = realTimeParam.Temp;
            //       //    RealTimeParam.PH = realTimeParam.PH;
            //       //    RealTimeParam.Agit = realTimeParam.Agit;
            //       //    RealTimeParam.DO = realTimeParam.DO;
            //       //    RealTimeParam.AcidFlowCapacity = realTimeParam.AcidFlowCapacity;
            //       //    RealTimeParam.AcidFlowSpeed = realTimeParam.AcidFlowSpeed;
            //       //    RealTimeParam.BaseFlowCapacity = realTimeParam.BaseFlowCapacity;
            //       //    RealTimeParam.BaseFlowSpeed = realTimeParam.BaseFlowSpeed;
            //       //    RealTimeParam.WorkStatus = realTimeParam.WorkStatus;
            //       //    RealTimeParam.AFFlowCapacity = realTimeParam.AFFlowCapacity;
            //       //    RealTimeParam.AFFlowSpeed = realTimeParam.AFFlowSpeed;
            //       //    RealTimeParam.AirFlowSpeed = realTimeParam.AirFlowSpeed;
            //       //    RealTimeParam.AirFlowCapacity = realTimeParam.AirFlowCapacity;
            //       //    RealTimeParam.O2FlowSpeed = realTimeParam.O2FlowSpeed;
            //       //    RealTimeParam.O2FlowCapacity = realTimeParam.O2FlowCapacity;
            //       //    RealTimeParam.CO2FlowSpeed = realTimeParam.CO2FlowSpeed;
            //       //    RealTimeParam.CO2FlowCapacity = realTimeParam.CO2FlowCapacity;
            //       //    RealTimeParam.N2FlowSpeed = realTimeParam.N2FlowSpeed;
            //       //    RealTimeParam.N2FlowCapacity = realTimeParam.N2FlowCapacity;
            //       //    RealTimeParam.FeedFlowSpeed = realTimeParam.FeedFlowSpeed;
            //       //    RealTimeParam.FeedFlowSpeed = realTimeParam.FeedFlowSpeed;
            //       //    RealTimeParam.JarWeight = realTimeParam.JarWeight;
            //       //    RealTimeParam.Bottle1Weight = realTimeParam.Bottle1Weight;
            //       //    RealTimeParam.Bottle2Weight = realTimeParam.Bottle2Weight;
            //       //    RealTimeParam.HasFoam = realTimeParam.HasFoam;
            //       //    RealTimeParam.AlarmBytes = [.. realTimeParam.AlarmBytes];
            //       //}
            //       //if (SelectedInstrument != null)
            //       //{
            //       //    aggregator.SendMessage(model.Message, nameof(MCUDebugView), realTimeParam);
            //       //}
            //   }, nameof(ClockSupervisor));
        }
    }
}
