using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{

    public enum MagneticBaseStatus
    {
        [Description("松开")]
        Release,
        [Description("吸合")]
        Engage,
        [Description("未设置")]
        Unset,
    }

    public enum SoftwarePlatform
    {
        [Description("1.5L发酵罐_电脑")]
        Default,
        [Description("5L发酵罐_Windows平板")]
        WindowsPad,
        [Description("500mL发酵罐_电脑")]
        HighThroughput
    }

    /// <summary>
    /// 转速是必需项，所以不在此列
    /// </summary>
    public enum DOControlFactor
    {
        [Description("空气")]
        Air,
        [Description("氧气")]
        O2,
        [Description("温度")]
        Temp,
        [Description("补料")]
        Feed
    }


    public enum DOControlStrategy
    {
        [Description("阶梯级联")]
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
        O2,
        [Description("补料")]
        Feed
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
        [Description("未设置")]
        Unset = 0,
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

    public enum SensorType_5L
    {
        pH = 0x01,
        DO,
        JarWeight,
        ReserveWeight,
        Bottle1Weight,
        Bottle2Weight,
        PT100,
        pHTemp,
        DOTemp,
        HeatBlanketNTC,
        RoomTempNTC
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
        [Description("溶氧")]
        DO,
        [Description("pH")]
        pH,
        [Description("温度")]
        Temperature,
        [Description("消泡速度")]
        AF_Flow,
        [Description("转速")]
        Agitation,
        [Description("酸泵速度")]
        Acid,
        [Description("碱速度")]
        Base,
        [Description("补料速度")]
        Feed,
        [Description("通气量")]
        Air,
        [Description("通氧量")]
        O2,
        [Description("通二氧化碳量")]
        CO2,
        [Description("通氮气量")]
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

    public enum DOEDesignType
    {
        [Description("全因子设计")]
        FullFactorial,
        [Description("两水平因子设计")]
       TwoLevelFractionalFactorial,
        [Description("Plackett-Burman")]
        Plackett_Burman,
        [Description("Box-Behnken")]
        Box_Behnken, 
        [Description("中心复合设计")]
        CentralComposite,
        [Description("拉丁超立方设计")]
        LatinHypercube, 
    }

    public enum DOEAlpha
    {
        [Description("正交")]
        Orthogonal,
        [Description("可旋转性")]
        Rotatable,
        //[Description("球形")]
        //Spherical
    }

    public enum DOEFace
    {
        [Description("面心型")]
        Faced,
        [Description("内切型")]
        Inscribed,
        [Description("外切型")]
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
        [Description("主板")]
        MainBoard = 1,
        [Description("信号板")]
        SignalBoard,
        [Description("温控板")]
        TemperatureControlBoard,
        [Description("冷凝板")]
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
