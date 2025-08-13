using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static log4net.Appender.FileAppender;

namespace RD3.Shared
{
    public class OperationManager
    {
        public List<Operation> Operations { get; private set; } = new();
        private static volatile OperationManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象

        private OperationManager()
        {
            LoadOperation();
        }

        public static OperationManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new OperationManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        void LoadOperation()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.OperationPath);
            Operations = JsonConvert.DeserializeObject<List<Operation>>(jsonContent);
        }

        public void AddOperation(Operation operation) 
        {
            Operations.Add(operation);
            Save();
        }

        public void AddOperation(IEnumerable<Operation> operationCol)
        {
            Operations.AddRange(operationCol);
            Save();
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(Operations);
            json = AESEncryption.Encrypt(json);

            string originalFile = FileConst.OperationPath;
            string newFile = Path.Combine(FileConst.DataDirectory, Path.GetFileNameWithoutExtension(originalFile) + Guid.NewGuid().ToString("N") + Path.GetExtension(originalFile));
            // 检查文件是否存在并重命名
            if (File.Exists(originalFile))
            {
                File.WriteAllText(newFile, json);
                File.Move(newFile, originalFile, true);
                File.Delete(newFile);
            }
            else
            {
                File.WriteAllText(originalFile, json);
            }
        }
    }
}
