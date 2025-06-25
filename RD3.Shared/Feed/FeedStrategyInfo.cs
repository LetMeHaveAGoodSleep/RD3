using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class FeedStrategyInfo : BindableBase, ICloneable
    {
        private string _name = "unnamed";
        public string Name
        {
            get => _name;
            set { SetProperty(ref _name, value); }
        }

        private string _id;
        public string ID
        {
            get => _id;
            set { SetProperty(ref _id, value); }
        }

        private PeristalticPump _pump = PeristalticPump.FeedPump;
        public PeristalticPump Pump
        {
            get => _pump;
            set { SetProperty(ref _pump, value); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { SetProperty(ref _description, value); }
        }

        private bool _isEnabled = false;
        /// <summary>
        /// 指示是否是Stat
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set { SetProperty(ref _isEnabled, value); }
        }

        private bool _isStatEnabled = false;
        /// <summary>
        /// 指示是否是Stat
        /// </summary>
        public bool IsStatEnabled
        {
            get => _isStatEnabled;
            set { SetProperty(ref _isStatEnabled, value); }
        }

        private bool _isConstant = true;
        public bool IsConstant
        {
            get => _isConstant;
            set { SetProperty(ref _isConstant, value); }
        }

        private bool _isExp = false;
        public bool IsExp
        {
            get => _isExp;
            set { SetProperty(ref _isExp, value); }
        }

        private bool _isPolynomial = false;
        public bool IsPolynomial
        {
            get => _isPolynomial;
            set { SetProperty(ref _isPolynomial, value); }
        }

        //private bool _curvePlusStat = false;
        //public bool CurvePlusStat
        //{
        //    get => _curvePlusStat;
        //    set { SetProperty(ref _curvePlusStat, value); }
        //}

        private bool _statDisable = true;
        public bool StatDisable
        {
            get => _statDisable;
            set { SetProperty(ref _statDisable, value); }
        }

        private bool _curveDOStat = false;
        public bool CurveDOStat
        {
            get => _curveDOStat;
            set { SetProperty(ref _curveDOStat, value); }
        }

        private bool _curvepHStat = false;
        public bool CurvepHStat
        {
            get => _curvepHStat;
            set { SetProperty(ref _curvepHStat, value); }
        }

        private string _infoType;
        public string InfoType
        {
            get => _infoType;
            set 
            {
                switch (value)
                {
                    case "Polynomial":
                        IsStatEnabled = true;
                        IsEnabled = false;
                        IsPolynomial = true;
                        Description = $"y=a(t-Δt)^2+b(t-Δt)+c:参数a、b、c单位mL/h；t: 运行时间, 单位h;\r\n" +
                            $"Δt: 时间间隔, when t<Δt, y=c;\r\n 参数1为a；参数2为b；参数3为c;参数4为△t";
                        break;
                    case "Exponential":
                        IsStatEnabled = true;
                        IsEnabled = false;
                        IsExp = true;
                        Description = $"F1(t)=F1(0)*exp(μ*(t-△t)):μ: 生长速率, 1/h;t: 运行时间, h;\r\nΔt: 时间间隔, when t<Δt, 补料速度=F1(0);\r\n" +
                            $"参数1为F1(0)；参数2为μ；参数3为△t";
                        break;
                    case "DO_Feedback":
                        IsConstant = true;
                        StatDisable= true;

                        IsStatEnabled = false;
                        IsEnabled = true;
                        Description = "DO_stat(速度):如果溶氧<=X1，补料速度=Y1;如果溶氧>=X2，补料速度=Y2;单位mL/h\r\n否则保持原状态;\r\n参数1为X1；参数2为Y1；参数3为X2；参数4为Y2；";
                        break;
                    case "PH_Feedback":
                        IsConstant = true;
                        StatDisable = true;
                        IsStatEnabled = false;
                        IsEnabled = true;
                        Description = $"pH_stat(速度):如果pH<=X1,补料速度= Y1;如果pH>=X2,补料速度=Y2;单位mL/h\r\n否则保持原状态;\r\n参数1为X1；参数2为Y1；参数3为X2；参数4为Y2；";
                        break;
                    case "DO_Feedback_Total":
                        IsConstant = true;
                        StatDisable = true;
                        IsStatEnabled = false;
                        IsEnabled = true;
                        Description = "DO_stat(体积):如果溶氧<=X1，补料总量=Y1;如果溶氧>=X2，补料总量=Y2;单位mL\r\n否则保持原状态;\r\n参数1为X1；参数2为Y1；参数3为X2；参数4为Y2；";
                        break;
                    case "PH_Feedback_Total":
                        IsConstant = true;
                        StatDisable = true;
                        IsStatEnabled = false;
                        IsEnabled = true;
                        Description = "pH_stat(体积):如果pH<=X1,补料总量= Y1;如果pH>=X2,补料总量=Y2;单位mL\r\n否则保持原状态;\r\n参数1为X1；参数2为Y1；参数3为X2；参数4为Y2；";
                        break;
                    case "Constant":
                        IsConstant = true;
                        StatDisable = true;
                        IsStatEnabled = false;
                        IsEnabled = false;
                        IsConstant = true;
                        Description = "恒速补料:参数1为速度，单位mL/h";
                        break;
                    case "Quantitative":
                        IsConstant = true;
                        StatDisable = true;
                        IsStatEnabled = false;
                        IsEnabled = false;
                        IsConstant = true;
                        Description = "定量补料:参数1为体积，单位为mL;参数2为速度，单位为mL/h；";
                        break;
                    case "Cycle":
                        IsConstant = true;
                        StatDisable = true;
                        IsStatEnabled = false;
                        IsEnabled = false;
                        IsConstant = true;
                        Description = "周期补料:参数1为周期，单位为分钟；参数2为占空比，单位为%；\r\n参数3为补料速度，单位为mL/h；参数4为补料体积，单位为mL；";
                        break;
                }
                SetProperty(ref _infoType, value);
            }
        }

        private int _triggerInterval = default;
        public int TriggerInterval
        {
            get => _triggerInterval;
            set { SetProperty(ref _triggerInterval, value); }
        }

        private int _statInterval;
        public int StatInterval
        {
            get { return _statInterval; }
            set { SetProperty(ref _statInterval, value); }
        }

        private int _statCycle;
        public int StatCycle
        {
            get { return _statCycle; }
            set { SetProperty(ref _statCycle, value); }
        }

        private int _statSymbol = 0;
        /// <summary>
        /// 升降动作
        /// 0;小于 1;大于
        /// </summary>
        public int StatSymbol
        {
            get { return _statSymbol; }
            set { SetProperty(ref _statSymbol, value); }
        }

        private float _statValue;
        public float StatValue
        {
            get { return _statValue; }
            set { SetProperty(ref _statValue, value); }
        }

        private float _a;
        public float A
        {
            get => _a;
            set { SetProperty(ref _a, value); }
        }

        private string _b;
        public string B
        {
            get => _b;
            set { SetProperty(ref _b, value); }
        }

        private float _c;
        public float C
        {
            get => _c;
            set { SetProperty(ref _c, value); }
        }

        private string _d;
        public string D
        {
            get => _d;
            set { SetProperty(ref _d, value); }
        }

        public object Clone()
        {
            var clonedObject = ObjectCloner.DeepCopy(this);
            return clonedObject;
        }
    }
}
