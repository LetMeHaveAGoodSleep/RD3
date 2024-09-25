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
    public class ProjectManager
    {
        public ObservableCollection<Project> Projects = new ObservableCollection<Project>();
        private static volatile ProjectManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private ProjectManager()
        {
            LoadProject();
        }

        public static ProjectManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new ProjectManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private void LoadProject()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.ProjectPath);
            Projects = JsonConvert.DeserializeObject<ObservableCollection<Project>>(jsonContent);
            foreach (var item in Projects)
            {
                if (item.StartDate > DateTime.Now)
                {
                    item.Status = ProjectStatus.Unstarted;
                }
                else if (item.StartDate <= DateTime.Now && item.CloseDate >= DateTime.Now)
                {
                    item.Status = ProjectStatus.Running;
                }
                else if (item.CloseDate < DateTime.Now)
                {
                    item.Status = ProjectStatus.Complete;
                }
                else
                {
                    item.Status = ProjectStatus.Unknown;
                }
            }
        }

        public void Save(ObservableCollection<Project> dataList = null)
        {
            if (dataList != null && !dataList.Equals(Projects))
            {
                Projects = dataList;
            }
            string json = JsonConvert.SerializeObject(dataList ?? Projects);
            json = AESEncryption.Encrypt(json);
            File.Delete(FileConst.ProjectPath);
            File.WriteAllText(FileConst.ProjectPath, json);
        }
    }
}
