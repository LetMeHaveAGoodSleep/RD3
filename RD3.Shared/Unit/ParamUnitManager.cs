using Microsoft.FSharp.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XZ.DB;

namespace RD3.Shared
{
    public class ParamUnitManager
    {
        public ObservableCollection<ParamUnit> Units = new ObservableCollection<ParamUnit>();
        private static volatile ParamUnitManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private ParamUnitManager()
        {
            LoadParameterUnit();
        }

        public static ParamUnitManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new ParamUnitManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private void LoadParameterUnit()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.ParamUnitPath);
            Units = JsonConvert.DeserializeObject<ObservableCollection<ParamUnit>>(jsonContent);
        }
    }
}
