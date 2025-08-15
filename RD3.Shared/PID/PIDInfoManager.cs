using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static log4net.Appender.FileAppender;

namespace RD3.Shared
{
    public class PIDInfoManager
    {
       public ObservableCollection<PIDInfo> PIDInfos { get; private set; } = [];
        private static volatile PIDInfoManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象

        private PIDInfoManager()
        {
            LoadPIDInfo();
        }

        public static PIDInfoManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new PIDInfoManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        void LoadPIDInfo()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.PidInfoPath);
            PIDInfos = JsonConvert.DeserializeObject<ObservableCollection<PIDInfo>>(jsonContent);
            var array = EnumUtil.GetEnumDescriptions<PIDFactor>();
            foreach (PIDInfo pidInfo in PIDInfos)
            {
                var factor = EnumUtil.GetEnumByDescription<PIDFactor>(pidInfo.PidName);
                pidInfo.Factor = factor;
            }
        }

        public void Save(ObservableCollection<PIDInfo> dataList = null)
        {
            if (dataList != null && !dataList.Equals(PIDInfos))
            {
                PIDInfos = dataList;
            }
            string json = JsonConvert.SerializeObject(dataList ?? PIDInfos);
            json = AESEncryption.Encrypt(json);

            string originalFile = FileConst.PidInfoPath;
            string newFile = Path.Combine(FileConst.ConfigDirectory, Path.GetFileNameWithoutExtension(originalFile) + Guid.NewGuid().ToString("N") + Path.GetExtension(originalFile));
            // 检查文件是否存在并重命名
            if (File.Exists(originalFile))
            {
                File.WriteAllText(newFile,json);
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
