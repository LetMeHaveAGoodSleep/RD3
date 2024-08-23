using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class AlarmLogger
    {
        private static volatile AlarmLogger _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private readonly BlockingCollection<AlarmRecord> _queue = new BlockingCollection<AlarmRecord>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private AlarmLogger()
        {
            StartAutoSave();
        }

        public static AlarmLogger GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new AlarmLogger(); // 实例化
                    }
                }
            }
            return _instance;
        }
        private void StartAutoSave()
        {
            Task.Run(() => AutoSave(), _cancellationTokenSource.Token);
        }

        public void AddAlarm(AlarmRecord alarm)
        {
            _queue.Add(alarm);
        }

        private async Task AutoSave()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(0, _cancellationTokenSource.Token);

                try
                {
                    SaveAlarmsToFile();
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"Error saving alarms: {ex.Message}");
                }
            }
        }

        private void SaveAlarmsToFile()
        {
            using (StreamWriter writer = new StreamWriter(FileConst.AlarmHistoryPath, true))
            {
                while (_queue.TryTake(out AlarmRecord alarm))
                {
                    writer.WriteLine(alarm.ToString());
                }
            }
        }

        public void StopAutoSave()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
