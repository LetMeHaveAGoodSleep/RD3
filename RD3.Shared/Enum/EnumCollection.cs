﻿using System;
using System.Collections.Generic;
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
}