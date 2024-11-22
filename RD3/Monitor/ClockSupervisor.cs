using Newtonsoft.Json;
using Prism.Events;
using Prism.Ioc;
using RD3.Extensions;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RD3
{
    public class ClockSupervisor
    {
        private static volatile ClockSupervisor _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象

        private DispatcherTimer _monitorTimer;

        private ClockSupervisor()
        {
            StartMonitor();
        }

        public static ClockSupervisor GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new ClockSupervisor(); // 实例化
                    }
                }
            }
            return _instance;
        }

        void StartMonitor()
        {
            _monitorTimer = new DispatcherTimer();
            _monitorTimer = new DispatcherTimer();
            _monitorTimer.Interval = TimeSpan.FromSeconds(1);
            _monitorTimer.Tick += ((s, e) =>
            {
                var realTimeParam = InstrumentSolution.GetInstance().CommandWrapper.GetRealTime();
                var containerProvider = (System.Windows.Application.Current as App).Container;
                var eventAggregator = containerProvider.Resolve<IEventAggregator>();
                //实时信息传递
                eventAggregator.SendMessage("", nameof(ClockSupervisor), realTimeParam);
            });
            _monitorTimer.Start();
        }
    }
}
