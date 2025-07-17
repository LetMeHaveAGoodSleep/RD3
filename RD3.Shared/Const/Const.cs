using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public static class Const
    {
        public static readonly string CHNLanguage = "中文";
        public static readonly string ENGLanguage = "English";
        public static readonly double MaxTemp = 100;
        public static readonly double MaxPH = 14;
        public static readonly double MaxAgit = 1500;
        public static readonly double MaxDO = 100;
        public static readonly float PHPumpSpeed = 50;
        public static readonly float FeedPumpSpeed = 100;
        public static readonly string PercentUnit = "%";
        public static readonly string FlowRateUnit = "L/min";
        public static readonly string PumpUnit = "mL/h";
        public static readonly string AgitUnit = "rpm";
        public static readonly int NumericalPrecision = 2;
        public static readonly float MaxPumpFlowRate = 600;
        public static readonly float MaxPumpFlowCapacity = float.MaxValue;
    }
}
