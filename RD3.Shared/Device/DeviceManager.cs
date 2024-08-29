using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class DeviceManager
    {
        public ObservableCollection<Device> Devices = new ObservableCollection<Device>();
        private static volatile DeviceManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private DeviceManager()
        {
            LoadDevice();
        }

        public static DeviceManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new DeviceManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private void LoadDevice()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.DevicePath);
            Devices = JsonConvert.DeserializeObject<ObservableCollection<Device>>(jsonContent);
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(Devices);
            json = AESEncryption.Encrypt(json);
            File.Delete(FileConst.DevicePath);
            File.WriteAllText(FileConst.DevicePath, json);
        }
    }
}
