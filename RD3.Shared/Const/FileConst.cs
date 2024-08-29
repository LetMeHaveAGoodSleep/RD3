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
        public static readonly string UserPath = ConfigDirectory + "\\User.json";
        public static readonly string ConstPath = ConfigDirectory + "\\Const.json";
        public static readonly string VarPath = ConfigDirectory + "\\Var.json";
        public static readonly string AlarmPath = ConfigDirectory + "\\Alarm.json";
        public static readonly string CommunicationPath = ConfigDirectory + "\\Communication.json";
        public static readonly string CommandPath = ConfigDirectory + "\\Command.json";
        public static readonly string DevicePath = ConfigDirectory + "\\Device.json";
        public static readonly string FunctionPath = ConfigDirectory + "\\Function.json";
        public static readonly string AlarmHistoryPath = AppDomain.CurrentDomain.BaseDirectory + "Alarm" + "\\AlarmHistory.log";
    }
}
