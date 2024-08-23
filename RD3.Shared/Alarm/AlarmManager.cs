using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static log4net.Appender.FileAppender;

namespace RD3.Shared
{
    public class AlarmManager
    {
       public List<Alarm> Alarms { get; private set; } = new List<Alarm>();
        private static volatile AlarmManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象

        private AlarmManager()
        {
            LoadAlarm();
        }

        public static AlarmManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new AlarmManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        void LoadAlarm()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.AlarmPath);
            Alarms = JsonConvert.DeserializeObject<List<Alarm>>(jsonContent);
        }
    }
}
