using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RD3
{
    // 支持取消的版本
    public class AutoShutdownService : IDisposable
    {
        private bool isExcute = false;

        private static volatile AutoShutdownService _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象

        private AutoShutdownService()
        {
        }

        public static AutoShutdownService GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new AutoShutdownService(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private readonly CancellationTokenSource _cts = new();

        public async Task StartShutdownTimerAsync()
        {
            if (!isExcute)
            {
                isExcute = true;
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), _cts.Token);
                    Shutdown();
                }
                catch (TaskCanceledException)
                {
                    // 计时被取消
                }
                finally 
                {
                    isExcute = false;
                }
            }
        }

        public void CancelShutdown()
        {
            _cts.Cancel();
        }

        private void Shutdown()
        {
            // 回到UI线程执行关闭
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.Application.Current.Shutdown();
                Environment.Exit(0);
            });
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
