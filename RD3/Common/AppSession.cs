using MathNet.Symbolics;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using XZ.SQLite;

namespace RD3.Common
{
    public static class AppSession
    {
        public static readonly XmlLanguage ChineseLanguage = XmlLanguage.GetLanguage("zh-cn");

        public static readonly XmlLanguage EnglishLanguage = XmlLanguage.GetLanguage("en-us");
        public static User CurrentUser { get; set; }

        public static string CompanyName { get { return VarConfig.GetValue("Company")?.ToString(); } }

        public static string LanguageName { get; set; } = Const.CHNLanguage;

        public static RD3Batch CurrentBatch { get; set; }

        public static List<RD3Batch> SelectedBatches { get; set; }

        public static TimeInterval BatchTimeInterval { get; set; } = TimeInterval.Second;

        public static DateTime ResgistrationTime { get; set; }

        public static int Interval
        {
            get
            {
                string temp = VarConfig.GetValue("TimeInterval").ToString();
                if (int.TryParse(temp, out var interval))
                {
                    return interval;
                }
                else
                {
                    VarConfig.SetValue("TimeInterval", 5);
                    return 5;
                }
            }
        }

        public static float DefaultPumpFlowRate
        {
            get
            {
                string temp = VarConfig.GetValue("DefaultPumpFlowRate")?.ToString();
                if (float.TryParse(temp, out var pumpFlowRate))
                {
                    return pumpFlowRate;
                }
                else
                {
                    VarConfig.SetValue("DefaultPumpFlowRate", 100);
                    return 100;
                }
            }
        }
        
        public static FontFamily FontFamily
        {
            get
            {
                var fontFamily = VarConfig.GetValue("FontFamily")?.ToString();
                var selectedFontFamily = FontFamilies.Find(t => t.ToString() == fontFamily);
                if (selectedFontFamily == null)
                {
                    return FontFamilies[0];
                }
                else
                {
                    return selectedFontFamily;
                }
            }
        }

        public static List<FontFamily> FontFamilies
        {
            get
            {
                List<FontFamily> source = [];
                source.AddRange(Fonts.SystemFontFamilies);
                source.Sort((x, y) => string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal));
                return source;
            }
        }

        public static bool AdjustPHEnable = false;

        public static bool LogOpen
        {
            get => Convert.ToBoolean(VarConfig.GetValue("LogOpen")?.ToString());
        }

        public static List<RD3Device> RunningDevices = [];

        public static bool IsBatchRunning
        {
            get => RunningDevices.Count > 0;
        }

        public static List<string> ShownFormList
        {
            get;
            set;
        } = [];

        /// <summary>
        /// 当前反应器相机的句柄
        /// </summary>
        public static IntPtr CameraHandle = IntPtr.Zero;

        /// <summary>
        /// 存储软件打开后的罐子重量以及泵的累积量
        /// </summary>
        public static Dictionary<string, Tuple<float, float, float, float, float,float, float>> DicTankWeight = new Dictionary<string, Tuple<float, float, float, float, float, float, float>>();

        public static TimeSpan RunningTimeSpan { get; set; }= TimeSpan.Zero;

        public static float VirtualDO = 0;

        public static float VirtualpH = 0;

        public static bool DOPause = false;
    }
}
