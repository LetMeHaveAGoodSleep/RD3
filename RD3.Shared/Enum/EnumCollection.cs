using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public enum MidrangingPeriod
    {
        [Description("未达到转速高限")]
        DuringAgitHigh,
        [Description("未达到通气上限")]
        DuringAirUpperLimit,
        [Description("未达到氧气上限")]
        DuringO2UpperLimit,
        [Description("降温中")]
        DuringFallTemp,
        [Description("减少补料中")]
        DuringReduceFeed
    }


    public enum DOControlStrategy
    {
        [Description("阶梯通气")]
        Step,
        [Description("中位控制")]
        Midranging,
        [Description("周期")]
        Cycle
    }
    public enum PIDFactor
    {
        [Description("未知")]
        Unknown,
        [Description("DO_正向")]
        DO_Dircet,
        [Description("DO_反向")]
        DO_Reverse,
        [Description("PH_酸")]
        pH_Acid,
        [Description("PH_碱")]
        pH_Base,
        [Description("升温")]
        Temp_Rise,
        [Description("降温")]
        Temp_Fall,
        [Description("通气")]
        Air,
        [Description("氧气")]
        O2
    }

    public enum ControlMode
    {
        Enable,
        Associated,
        Disable,
        TimeSeries,
        Funtion,
        Cycle,
        Polynomial,
        Exponential
    }

    public enum PHControlMode
    {
        [Description("PID")]
        PID,
        [Description("动态缓冲")]
        Buffer,
        [Description("自适应调节")]
        Adaptive
    }

    public enum FeedControlMode
    {
        [Description("恒速")]
        ConstantSpeed = 0,
        [Description("多项式")]
        Polynomial,
        [Description("指数")]
        Exponential,
        [Description("时间序列")]
        TimeSeries,
        [Description("DO_stat(速度)")]
        DO_stat_Speed,
        [Description("pH_stat(速度)")]
        pH_stat_Speed,
        [Description("DO_stat(体积)")]
        DO_stat_Volume,
        [Description("pH_stat(体积)")]
        pH_stat_Volume,
        [Description("定量")]
        Quantitative,
        [Description("周期")]
        Cycle,
        [Description("探究")]
        Probe
    }

    public enum GasType
    {
        [Description("空气")]
        Air = 1,
        [Description("氧气")]
        O2,
        [Description("二氧化碳")]
        CO2,
        [Description("氮气")]
        N2
    }

    public enum WorkStatus
    {
        Initializing,
        Idle,
        Running,
        SystemError
    }

    public enum ReactorStatus
    {
        DisConnected,
        Connected,
        Simulated
    }

    public enum SensorType
    {
        pH = 0x01,
        DO,
        JarWeight,
        Bottle1Weight,
        Bottle2Weight,
        PT100,
        pHTemp,
        DOTemp,
        TempControlNTC1,
        TempControlNTC2,
        CoolingModuleNTC1,
        CoolingModuleNTC2,
        CoolingModuleNTC3,
    }

    public enum SensorCorrectMode
    {
        [Description("系数校准")]
        CoefficientCalibration = 1,
        [Description("偏置校准")]
        OffsetCalibration,
        [Description("两点校准_A")]
        TwoPointCalibration_A,
        [Description("两点校准_B")]
        TwoPointCalibration_B,
    }

    public enum SampleType
    {
        Metabolites
    }

    public enum SampleParam
    {
        pH,
        Density,
        Gas
    }

    public enum AlarmGrade
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    public enum ProjectStatus
    {
        Unstarted,
        Running,
        Complete,
        Unknown
    }

    public enum ExperimentParameter
    {
        DO,
        Air,
        Agit,
        PH,
        Temp
    }

    public enum OpenMode
    {
        View,
        Add,
        Edit
    }

    public enum AuditAction
    {
        [Description("Modify control mode")]
        ModifyControlMode,
        [Description("Modify set value")]
        ModifySetValue,
        [Description("Stop experiment")]
        StopExperiment,
        [Description("Start experiment")]
        StartExperiment,
        [Description("Pause experiment")]
        PauseExperiment
    }

    public enum AuditModule
    {
        Agitation,
        Acid_pump,
        Base_pump,
        Temp_controller,
        Air_inflow,
        Condensation
    }

    public enum UnScheduleAction
    {
        Sample,
        [Description("Add liquid")]
        Addliquid,
        Harvest,
        Inoculate
    }

    public enum RegistrationStatus
    {
        NoRegister,
        Success,
        Expired
    }

    public enum IndicatorType
    {
        Stop = 0,
        Start = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4,
    }

    public enum FeedTimer
    {
        Day = 86400,
        Hour = 3600,
        Minute = 60
    }

    public enum FeedFormulaParam
    {
        Air_output,
        CO2_output,
        O2_output,
        N2_output,
        Agit_output
    }

    public enum OpenType
    {
        Navigate,
        Dialog
    }

    public enum Factor
    {
        DO,
        PH,
        Temperature,
        AF_Flow,
        Agitation,
        Acid,
        Base,
        Feed,
        Air,
        O2,
        CO2,
        N2
    }

    public enum DOEResponse
    {
        [Description("Acid_Flow_PV")]
        Acid_Flow_PV,
        [Description("Base_Flow_PV")]
        Base_Flow_PV,
        [Description("AF_Flow_PV")]
        AF_Flow_PV,
        [Description("Agitation")]
        Agitation,
        [Description("Air_Flow_PV")]
        Air_Flow_PV,
        [Description("CO2_Flow_PV")]
        CO2_Flow_PV,
        [Description("O2_Flow_PV")]
        O2_Flow_PV,
        [Description("N2_Flow_PV")]
        N2_Flow_PV,
        [Description("PH_PV")]
        PH_PV,
    }

    public enum DOEAlpha
    {
        [Description("正交")]
        Orthogonal,
        [Description("可旋转性")]
        Rotatable,
        [Description("球形")]
        Spherical
    }

    public enum DOEFace
    {
        Faced,
        Inscribed,
        Circumscribed
    }

    public enum AlarmModule
    {
        MainBoard,
        Environment,
        Acid_Pump,
        Base_Pump,
        Gas,
        Flash,
        Camera,
        OD,
        Wifi,
        TEC,
        Agitating_Motor,
        PH_Sensor,
        Temp_Sensor,
        DO_Sensor,
        Tail_gas,
        SignalBoard,
        Weight,
        Condensation,
        NFC,
        Communication
    }

    public enum PeristalticPump
    {
        [Description("未设置")]
        None,
        [Description("酸")]
        AcidPump,
        [Description("碱")]
        BasePump,
        [Description("补料1")]
        FeedPump,
        [Description("消泡")]
        AFPump,
        [Description("补料2")]
        Feed2Pump
    }

    public enum PumpControlMode
    {
        Direct = 1,
        Reverse
    }

    public enum SwitchMode
    {
        Close,
        Open,
    }

    public enum MCUBoardType
    {
        MainBoard = 1,
        SignalBoard,
        TemperatureCcontrolBoard,
        CondensationBoard
    }

    public enum DebugMode
    {
        Stop,
        Direct,
        Reverse
    }
    public enum TwoPointCorrectParam
    {
        Null = 0,
        A,
        B,
        Complete = 0xff
    }

    public enum PeristalticCalibrationParam
    {
        //系数修改
        Coefficient = 0x01,
        //步骤一 写入蠕动泵转动时间，单位s(秒)
        Speed,
        //步骤一 写入蠕动泵转动时间，单位s(秒)
        RunTime,
        //0x02 : 步骤二 等待蠕动泵转动完成后，写入流出液体的体积，ml(毫升)
        Capacity,
        //0x05 : 步骤三 校准完成，写入配置
        Complete = 0x05
    }

    public enum ClearModule
    {
        Pump = 0x01,
        MFC,
        Weight,
        AllPump,
        AllMFC,
        AllWeight,
        All
    }

    public enum CalibrationModule
    {
        Temp,
        DO,
        PH,
        JarWeight,
        Bottle1Weight,
        Bottle2Weight
    }

    public enum MCUDownloadStatus
    {
        Idle,
        Downloading
    }
}
