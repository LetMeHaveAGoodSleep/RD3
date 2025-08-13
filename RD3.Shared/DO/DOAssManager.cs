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
    public class DOAssManager
    {
        public ObservableCollection<DOAssParam> DOAssParamCol = new ObservableCollection<DOAssParam>();
        private static volatile DOAssManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private DOAssManager()
        {
            LoadDOAssParam();
        }

        public static DOAssManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new DOAssManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private void LoadDOAssParam()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.DOAssParamPath);
            DOAssParamCol = JsonConvert.DeserializeObject<ObservableCollection<DOAssParam>>(jsonContent);
        }

        public void Save(ObservableCollection<DOAssParam> dataList = null)
        {
            if (dataList != null && !dataList.Equals(DOAssParamCol))
            {
                DOAssParamCol = dataList;
            }
            string json = JsonConvert.SerializeObject(dataList ?? DOAssParamCol);
            json = AESEncryption.Encrypt(json);

            string originalFile = FileConst.DOAssParamPath;
            string newFile = Path.Combine(FileConst.ConfigDirectory, Path.GetFileNameWithoutExtension(originalFile) + Guid.NewGuid().ToString("N") + Path.GetExtension(originalFile));
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
