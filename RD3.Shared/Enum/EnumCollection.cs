using System;
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
        Free
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
        PH,
        Acid,
        Base,
        Feed,
        AF
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
        [Description("Modify sontrol value")]
        ModifySetValue,
        [Description("Stop experiment")]
        StopExperiment,
        [Description("Start experiment")]
        StartExperiment,
        [Description("Pause experiment")]
        PauseExperiment
    }
}
