using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class FileConst
    {
        public static readonly string ConfigDirectory = AppDomain.CurrentDomain.BaseDirectory + "Config";
        public static readonly string DataDirectory = AppDomain.CurrentDomain.BaseDirectory + "Data";
        public static readonly string UserPath = DataDirectory + "\\User.json";
        public static readonly string ConstPath = ConfigDirectory + "\\Const.json";
        public static readonly string VarPath = ConfigDirectory + "\\Var.json";
        public static readonly string GraphPath = ConfigDirectory + "\\GraphSetting.json";
        public static readonly string GraphUnitPath = ConfigDirectory + "\\GraphUnit.json";
        public static readonly string CustomGraphPath = ConfigDirectory + "\\CustomGraphSetting.json";
        public static readonly string ReactorSettingPath = ConfigDirectory + "\\ReactorSetting.json";
        public static readonly string PumpMFCPath = ConfigDirectory + "\\PumpMFCSetting.json";
        public static readonly string AlarmPath = ConfigDirectory + "\\Alarm.json";
        public static readonly string CommunicationPath = ConfigDirectory + "\\Communication.json";
        public static readonly string CommandPath = ConfigDirectory + "\\Command.json";
        public static readonly string DevicePath = DataDirectory + "\\Device.json";
        public static readonly string FunctionPath = DataDirectory + "\\Function.json";
        public static readonly string BatchPath = DataDirectory + "\\Batch.json";
        public static readonly string ProjectPath = DataDirectory + "\\Project.json";
        public static readonly string ProjectTemplatePath = DataDirectory + "\\ProjectTemplate.json";
        public static readonly string SamplePath = DataDirectory + "\\Sample.json";
        public static readonly string OperationPath = DataDirectory + "\\Operation.json";
        public static readonly string AlarmHistoryDir = AppDomain.CurrentDomain.BaseDirectory + "Alarm\\";
        public static readonly string AlarmHistoryPath = AlarmHistoryDir + "AlarmHistory.log";
        public static readonly string DefoamingPath = DataDirectory + "\\Defoaming.json";//// 新添加-hdb
        public static readonly string DOAssParamPath = ConfigDirectory + "\\DOAssParam.json";
        public static readonly string MidRangingParamPath = ConfigDirectory + "\\MidRangingParam.json";
        public static readonly string ParameterSourcePath = ConfigDirectory + "\\ParameterSource.json";
        public static readonly string ParameterPath = ConfigDirectory + "\\Parameter.json";
        public static readonly string PidInfoPath = ConfigDirectory + "\\PidInfo.json";
        public static readonly string FeedInfoPath = DataDirectory + "\\FeedInfo.json";
        public static readonly string CurveColorPath = ConfigDirectory + "\\CurveColor.json";
        public static readonly string ParameterMapperConfigPath = ConfigDirectory + "\\ParameterMapperConfig.json";
        public static readonly string ParameterNodePath = AppDomain.CurrentDomain.BaseDirectory + "Data\\ParameterNode.json";
        public static readonly string DORTInfoPath = DataDirectory + "\\DORTInfo.json";
        public static readonly string ReactorParamDir = AppDomain.CurrentDomain.BaseDirectory + "PersonalSetting\\{0}";
        public static readonly string ReactorParamPath = ReactorParamDir + "\\ReactorParam.json";
        public static readonly string CameraAlarmPath = ConfigDirectory + "\\CameraAlarm.json";
        public static readonly string CameraSettingPath = ConfigDirectory + "\\CameraSetting.json";
        public static readonly string FeedStrategyPath = DataDirectory + "\\FeedStrategy.json";
        public static readonly string FeedGradientPath = DataDirectory + "\\FeedGradientInfo.json";
        public static readonly string ProbingPath = ConfigDirectory + "\\Probing.json";
        public static readonly string PumpInfoPath = ConfigDirectory + "\\PumpInfo.json";
        public static readonly string MFCInfoPath = ConfigDirectory + "\\MFCInfo.json";
        public static readonly string ReportNodesPath = AppDomain.CurrentDomain.BaseDirectory + "Data\\ReportNodes.json";
        public static readonly string ParamUnitPath = ConfigDirectory + "\\ParamUnit.json";
    }
}
