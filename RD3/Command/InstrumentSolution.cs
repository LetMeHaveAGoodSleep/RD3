using Fpi.Instruments;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public InstrumentSolution()
        {
            GenerateInstrument();
        }

        private RealCommandWrapper _realCommandWrapper = new();
        private VirtualCommandWrapper _virtualCommandWrapper = new();

        private Real5LCommandWrapper _real5LCommandWrapper = new();
        private Virtual5LCommandWrapper _virtual5LCommandWrapper = new();
        public ICommandWrapper CommandWrapper
        {
            get
            {
                if (CommunicationProtocol == 0)
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
                else
                {
                    if (IsSimulation)
                    {
                        return _virtual5LCommandWrapper;
                    }
                    else
                    {
                        return _real5LCommandWrapper;
                    }
                }
            }
        }

        public Instrument[] Instruments
        {
            get;
            private set;
        }

        private void GenerateInstrument()
        {
            List<Instrument> list = new List<Instrument>();
            foreach (Instrument ins in InstrumentManager.GetInstance().instruments)
            {
                list.Add(ins);
            }
            Instruments = list.ToArray();
        }

        public bool IsSimulation
        {
            get { return Convert.ToBoolean(VarConfig.GetValue("IsSimulation")); }
        }

        public int CommunicationProtocol
        {
            get 
            {
                string temp = VarConfig.GetValue("CommunicationProtocol")?.ToString();
                if (!int.TryParse(temp, out var result))
                {
                    return 0;
                }
                else
                {
                    return result;
                }
            } 
        }
    }
}
