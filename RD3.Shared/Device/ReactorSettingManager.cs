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
    public class ReactorSettingManager
    {
        public ObservableCollection<ReactorSetting> ReactorSettings = new ObservableCollection<ReactorSetting>();
        private static volatile ReactorSettingManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private ReactorSettingManager()
        {
            LoadReactorSetting();
        }

        public static ReactorSettingManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new ReactorSettingManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private void LoadReactorSetting()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.ReactorSettingPath);
            ReactorSettings = JsonConvert.DeserializeObject<ObservableCollection<ReactorSetting>>(jsonContent);
            ReactorSettings = new ObservableCollection<ReactorSetting>(ReactorSettings.OrderByDescending(t => t.ReactorName));
        }

        public void Save(ObservableCollection<ReactorSetting> dataList = null)
        {
            if (dataList != null && !dataList.Equals(ReactorSettings))
            {
                ReactorSettings = dataList;
            }
            string json = JsonConvert.SerializeObject(dataList ?? ReactorSettings);
            json = AESEncryption.Encrypt(json);

            string originalFile = FileConst.CameraSettingPath;
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
