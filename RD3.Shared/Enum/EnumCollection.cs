﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public enum ControlMode
    {
        Fixed,
        Associated,
        Free,
        Disable
    }

    public enum PHPump
    {
        Base,
        Acid
    }

    public enum DOPump
    {
        Agit,
        Air,
        Feed,
        N2,
        CO2
    }

    public enum GasType
    {
        Air,
        CO2,
        O2,
        N2
    }

    public enum WorkStatus
    {
        Idle,
        Running,
    }

    public enum SensorCorrectionMode
    {
        Auto,
        Manual
    }

    public enum SensorType
    {
        PH,
        DO,
        ORP,
        Temp,
        Weigh,
        MFC
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
        Agit, 
        Air,
        Temp,
        PH
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
        Air_inflow
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

    public enum FeedMethod
    {
        Unknown,
        Constant,
        Associate,
        Gradient,
        Function,
        Loop,
        MutilLinear
    }

    public enum IndicatorType
    {
        Stop = 0,
        Start = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4,
    }

    public enum FeedAssociateModule
    {
        DO,
        PH
    }

    public enum FeedAssociatePattern
    {
        Dose,
        Speed
    }

    public enum FeedAssociateThreshold
    {
        Lowerlimit,
        Upperlimit
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
}
