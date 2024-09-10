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
        public static readonly string AlarmPath = DataDirectory + "\\Alarm.json";
        public static readonly string CommunicationPath = ConfigDirectory + "\\Communication.json";
        public static readonly string CommandPath = ConfigDirectory + "\\Command.json";
        public static readonly string DevicePath = DataDirectory + "\\Device.json";
        public static readonly string FunctionPath = DataDirectory + "\\Function.json";
        public static readonly string BatchPath = DataDirectory + "\\Batch.json";
        public static readonly string ProjectPath = DataDirectory + "\\Project.json";
        public static readonly string SamplePath = DataDirectory + "\\Sample.json";
        public static readonly string OperationPath = DataDirectory + "\\Operation.json";
        public static readonly string AlarmHistoryPath = AppDomain.CurrentDomain.BaseDirectory + "Alarm" + "\\AlarmHistory.log";
    }
}
