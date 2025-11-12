using Newtonsoft.Json;
using RD3.Controller;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3
{
    public class Fermentor
    {
        public readonly DeviceParameter Device;

        [JsonIgnore]
        public AgitController AgitController { get; private set; }

        [JsonIgnore]
        public DOController DOController { get; private set; }

        [JsonIgnore]
        public TempController TempController { get; private set; }

        [JsonIgnore]
        public pHController pHController { get; private set; }

        private static volatile Fermentor _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private Fermentor(DeviceParameter device)
        {
            Device = device;

            AgitController = new();
            DOController = new();
            TempController = new();
            pHController = new();
        }

        

        public static Fermentor GetInstance(DeviceParameter device)
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new Fermentor(device); // 实例化
                    }
                }
            }
            return _instance;
        }
    }
}
