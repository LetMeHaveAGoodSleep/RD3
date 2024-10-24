using System;

namespace RD3.Shared
{
    public class InstrumentSolution
    {
        private static volatile InstrumentSolution _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        public static InstrumentSolution GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new InstrumentSolution(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private RealCommandWrapper _realCommandWrapper = new();
        private VirtualCommandWrapper _virtualCommandWrapper = new();
        public ICommandWrapper CommandWrapper
        {
            get
            {
                if (IsSimulation)
                {
                    return _virtualCommandWrapper;
                }
                else
                {
                    return _realCommandWrapper;
                }
            }
        }

        public bool IsSimulation
        {
            get { return Convert.ToBoolean(VarConfig.GetValue("IsSimulation")); }
        }

    }
}
