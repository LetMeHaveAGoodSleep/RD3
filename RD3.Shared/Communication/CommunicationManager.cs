using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel;

namespace RD3.Shared
{
    public class CommunicationManager
    {
        public ObservableCollection<ClientConfig> Clients = new ObservableCollection<ClientConfig>();
        private static volatile CommunicationManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private static int timeOut = 3000;
        private List<TcpListener> tcpListeners = new List<TcpListener>();
        private List<CustomTcpClient> tcpClients = new List<CustomTcpClient>();

        private CommunicationManager()
        {
            LoadCommunication();

            // 创建客户端
            foreach (var client in Clients)
            {
                CustomTcpClient tcpClient = new CustomTcpClient(client.Host, client.Port,client.RetryTimes,client.ReconnectDelay);
                Task.Run(() => 
                {
                    tcpClient.Connect();
                });
                tcpClients.Add(tcpClient);
            }

        }

        public static CommunicationManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new CommunicationManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        void LoadCommunication()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.CommunicationPath);
            Clients = JsonConvert.DeserializeObject<ObservableCollection<ClientConfig>>(jsonContent);
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(Clients);
            File.Delete(FileConst.CommunicationPath);
            File.WriteAllText(FileConst.CommunicationPath, json);
        }
    }
}
