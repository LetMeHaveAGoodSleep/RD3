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
    public class BatchManager
    {
        public ObservableCollection<Batch> Batches = new ObservableCollection<Batch>();
        private static volatile BatchManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private BatchManager()
        {
            LoadBatch();
        }

        public static BatchManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new BatchManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private void LoadBatch()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.BatchPath);
            Batches = JsonConvert.DeserializeObject<ObservableCollection<Batch>>(jsonContent);
            Batches = new ObservableCollection<Batch>(Batches.OrderByDescending(t => t.StartTime));
        }

        public void Save(ObservableCollection<Batch> dataList = null)
        {
            if (dataList != null && !dataList.Equals(Batches))
            {
                Batches = dataList;
            }
            string json = JsonConvert.SerializeObject(dataList ?? Batches);
            json = AESEncryption.Encrypt(json);
            File.Delete(FileConst.BatchPath);
            File.WriteAllText(FileConst.BatchPath, json);
        }
    }
}
