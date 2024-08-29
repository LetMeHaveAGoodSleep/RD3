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
    public class FunctionManager
    {
        public List<Function> Functions = new List<Function>();
        private static volatile FunctionManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private FunctionManager()
        {
            LoadFunction();
        }

        public static FunctionManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new FunctionManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private void LoadFunction()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.FunctionPath);
            Functions = JsonConvert.DeserializeObject<List<Function>>(jsonContent);
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(Functions);
            json = AESEncryption.Encrypt(json);
            File.Delete(FileConst.FunctionPath);
            File.WriteAllText(FileConst.FunctionPath, json);
        }
    }
}
