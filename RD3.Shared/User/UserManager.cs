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
    public class UserManager
    {
        public ObservableCollection<User> Users = new ObservableCollection<User>();
        private static volatile UserManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private UserManager() 
        {
            LoadUser();
        }

        public static UserManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new UserManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private void LoadUser()
        {
            if (!Directory.Exists(FileConst.ConfigDirectory))
            {
                Directory.CreateDirectory(FileConst.ConfigDirectory);
            }
            if (!File.Exists(FileConst.UserPath))
            {
                CreateJsonFile(FileConst.UserPath);
            }
            ReadJsonFile(FileConst.UserPath);
        }

        private void CreateJsonFile(string filePath)
        {
            User user = new User
            {
                UserName = "Admin",
                Password = "123456",
                Createtime = DateTime.Now,
                Creator = "SysAdmin",
                Type = UserType.Admin,
                TypeName = UserType.Admin.ToString()
            };
            Users.Add(user);
            Save();
        }

        private void ReadJsonFile(string filePath)
        {
            string jsonContent = AESEncryption.DecryptFile(filePath);
            Users = JsonConvert.DeserializeObject<ObservableCollection<User>>(jsonContent);
        }

        public void Save(ObservableCollection<User> dataList = null)
        {
            if (dataList != null && !dataList.Equals(Users))
            {
                Users = dataList;
            }
            string json = JsonConvert.SerializeObject(dataList ?? Users);
            json = AESEncryption.Encrypt(json);
            File.Delete(FileConst.UserPath);
            File.WriteAllText(FileConst.UserPath, json);
        }
    }
}
