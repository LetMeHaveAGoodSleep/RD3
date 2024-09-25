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
    public class ProjectTemplateManager
    {
        public ObservableCollection<ProjectTemplate> Templates = [];
        private static volatile ProjectTemplateManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new(); // 锁对象
        private ProjectTemplateManager()
        {
            LoadTemplate();
        }

        public static ProjectTemplateManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new ProjectTemplateManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private void LoadTemplate()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.ProjectTemplatePath);
            Templates = JsonConvert.DeserializeObject<ObservableCollection<ProjectTemplate>>(jsonContent);
        }

        public void Save(ObservableCollection<ProjectTemplate> dataList = null)
        {
            if (dataList != null && !dataList.Equals(Templates))
            {
                Templates = dataList;
            }
            string json = JsonConvert.SerializeObject(dataList ?? Templates);
            json = AESEncryption.Encrypt(json);
            File.Delete(FileConst.ProjectTemplatePath);
            File.WriteAllText(FileConst.ProjectTemplatePath, json);
        }
    }
}
