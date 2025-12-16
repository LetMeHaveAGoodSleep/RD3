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
    public class ParameterNodeManager
    {
        public ObservableCollection<ParameterNode> ParameterNodes = new ObservableCollection<ParameterNode>();
        private static volatile ParameterNodeManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private ParameterNodeManager()
        {
            LoadParameterNode();
        }

        public static ParameterNodeManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new ParameterNodeManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        private void LoadParameterNode()
        {
            //string jsonContent = AESEncryption.DecryptFile(FileConst.ParameterNodePath);
            var list = SqliteManager.QueryList<ParameterNode>();
            ParameterNodes = [..list];
        }

        public void Save(ObservableCollection<ParameterNode> dataList = null)
        {
            if (dataList != null && !dataList.Equals(ParameterNodes))
            {
                ParameterNodes = dataList;
            }

            foreach (ParameterNode node in ParameterNodes)
            {
                _ = node.ID switch
                {
                    < 1 => SqliteManager.Insert(node),
                    _ => SqliteManager.Update(node)
                };
            }
        }
    }
}
