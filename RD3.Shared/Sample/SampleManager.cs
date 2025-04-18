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
    public class SampleManager
    {
        public ObservableCollection<Sample> Samples = new ObservableCollection<Sample>();
        private static volatile SampleManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private SampleManager()
        {
            LoadSample();
        }

        public static SampleManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new SampleManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private void LoadSample()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.SamplePath);
            Samples = JsonConvert.DeserializeObject<ObservableCollection<Sample>>(jsonContent);
        }

        public void Save(ObservableCollection<Sample> dataList = null)
        {
            if (dataList != null && !dataList.Equals(Samples))
            {
                Samples = dataList;
            }
            string json = JsonConvert.SerializeObject(dataList ?? Samples);
            json = AESEncryption.Encrypt(json);
            File.Delete(FileConst.SamplePath);
            File.WriteAllText(FileConst.SamplePath, json);
        }
    }
}
