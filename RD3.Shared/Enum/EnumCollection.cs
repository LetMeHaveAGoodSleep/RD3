using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public enum TecControlMode
    {
        [Description("关闭")]
        Close,
        [Description("PID控制")]
        PIDControl,
        [Description("PWM控制")]
        PWMControl,
        [Description("PID参数自整定")]
        PIDSelfTuning,
        [Description("TEC测试")]
        TecTest,
        [Description("快速控制")]
        QuickControl = 0xf1,
    }

    public enum TimeInterval
    {
        [Description("秒")]
        Second = 1,

        [Description("分钟")]
        Minute = 60, // 1分钟 = 60秒

        [Description("小时")]
        Hour = 3600, // 1小时 = 60分钟 = 3600秒

        [Description("天")]
        Day = 86400 // 1天 = 24小时 = 86400秒
    }
    public enum KeyBoardType
    {
        [Description("全键盘")]
        Normal,
        [Description("纯数字键盘")]
        Number,
        [Description("数字+符号键盘")]
        NumberSymbol,
        [Description("字母键盘")]
        Letters,
        [Description("字母+数字键盘")]
        LettersNumber,
        [Description("字母+数字+功能键盘")]
        LettersNumbernoFunctionkeys
    }
    // 定义逻辑类型（默认为逻辑与）
    public enum LogicType 
    {
        [Description("与")]
        AND,
        [Description("或")]
        OR 
    }
    public enum WeightIndex
    {
        [Description("未设置")]
        Unset,
        [Description("称1")]
        Weight1 = 1,
        [Description("称2")]
        Weight2,
        [Description("称3")]
        Weight3,
        [Description("称4")]
        Weight4
    }

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
        [Description("中位级联")]
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
        Temp,
        [Description("转速")]
        Agit,
        [Description("泵1流速（ml/h）")]
        Pump1FlowRate,
        [Description("泵2流速（ml/h）")]
        Pump2FlowRate,
        [Description("泵3流速（ml/h）")]
        Pump3FlowRate,
        [Description("泵4流速（ml/h）")]
        Pump4FlowRate,
        [Description("泵5流速（ml/h）")]
        Pump5FlowRate,
        [Description("泵6流速（ml/h）")]
        Pump6FlowRate,
        [Description("泵1流量（mL）")]
        Pump1Flow,
        [Description("泵2流量（mL）")]
        Pump2Flow,
        [Description("泵3流量（mL）")]
        Pump3Flow,
        [Description("泵4流量（mL）")]
        Pump4Flow,
        [Description("泵5流量（mL）")]
        Pump5Flow,
        [Description("泵6流量（mL）")]
        Pump6Flow,
        [Description("MFC1流量（L/min）")]
        MFC1FlowRate,
        [Description("MFC2流量（L/min）")]
        MFC2FlowRate,
        [Description("MFC3流量（L/min）")]
        MFC3FlowRate,
        [Description("MFC4流量（L/min）")]
        MFC4FlowRate,
        [Description("MFC5流量（L/min）")]
        MFC5FlowRate,
    }

    public enum DOEResponse
    {
        [Description("溶氧")]
        DO,
        [Description("pH")]
        pH,
        [Description("温度")]
        Temp,
        [Description("转速")]
        Agit,
        [Description("泵1流速（ml/h）")]
        Pump1FlowRate,
        [Description("泵2流速（ml/h）")]
        Pump2FlowRate,
        [Description("泵3流速（ml/h）")]
        Pump3FlowRate,
        [Description("泵4流速（ml/h）")]
        Pump4FlowRate,
        [Description("泵5流速（ml/h）")]
        Pump5FlowRate,
        [Description("泵6流速（ml/h）")]
        Pump6FlowRate,
        [Description("泵1总量（mL）")]
        Pump1FlowCapacity,
        [Description("泵2总量（mL）")]
        Pump2FlowCapacity,
        [Description("泵3总量（mL）")]
        Pump3FlowCapacity,
        [Description("泵4总量（mL）")]
        Pump4FlowCapacity,
        [Description("泵5总量（mL）")]
        Pump5FlowCapacity,
        [Description("泵6总量（mL）")]
        Pump6FlowCapacity,
        [Description("MFC1流量（L/min）")]
        MFC1FlowRate,
        [Description("MFC2流量（L/min）")]
        MFC2FlowRate,
        [Description("MFC3流量（L/min）")]
        MFC3FlowRate,
        [Description("MFC4流量（L/min）")]
        MFC4FlowRate,
        [Description("MFC5流量（L/min）")]
        MFC5FlowRate,
        [Description("二氧化碳释放速率")]
        CER,
        [Description("摄氧速率")]
        OUR,
        [Description("呼吸熵")]
        RQ,
        [Description("体积氧传质系数")]
        KLA,
        [Description("EPC压力")]
        EPCPressure,
        [Description("X传感器")]
        XSensor,
        [Description("自定义1")]
        Diy1,
        [Description("自定义2")]
        Diy2,
    }

    public enum DOEDesignType
    {
        [Description("全因子设计")]
        FullFactorial,
        [Description("两水平因子设计")]
        TwoLevelFractionalFactorial,
        [Description("Plackett-Burman设计")]
        Plackett_Burman,
        [Description("Box-Behnken设计")]
        Box_Behnken,
        [Description("中心复合设计")]
        CentralComposite,
        [Description("拉丁超立方抽样设计")]
        LatinHypercube,
    }

    public enum ProbDistribution
    {
        [Description("无")]
        None,
        [Description("正态分布")]
        Normal,
        [Description("泊松分布")]
        Poisson,
        [Description("指数分布")]
        Exponential,
        [Description("贝塔分布")]
        Beta,
        [Description("伽马分布")]
        Gamma,
    }

    public enum Criterion
    {
        [Description("无")]
        None,
        [Description("样本点置于区间中心")]
        Center,
        [Description("最大化样本点间的最小距离，随机分布")]
        Maximin,
        [Description("最大化样本点间的最小距离，置于区间中心")]
        CenterMaximin,
        [Description("最小化因子间的最大相关系数")]
        Correlation,
    }
    

    public enum DOEAlpha
    {
        [Description("正交")]
        Orthogonal,
        [Description("可旋转性")]
        Rotatable,
        [Description("表面中心")]
        Faced
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
        [Description("正转")]
        Direct = 1,
        [Description("反转")]
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
