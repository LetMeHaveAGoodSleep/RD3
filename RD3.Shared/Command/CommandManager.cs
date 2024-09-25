using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace RD3.Shared
{
    public class CommandManager
    {
        private static volatile CommandManager _instance; // 使用volatile确保多线程环境下的可见性
        private static readonly object _lock = new object(); // 锁对象
        private static List<Command> _commands = new List<Command>();
        public List<Command> Commands { get { return _commands; } }

        private CommandManager()
        {
            LoadCommand();
        }

        public static CommandManager GetInstance()
        {
            if (_instance == null) // 第一次检查
            {
                lock (_lock) // 锁定临界区
                {
                    if (_instance == null) // 第二次检查
                    {
                        _instance = new CommandManager(); // 实例化
                    }
                }
            }
            return _instance;
        }

        void LoadCommand()
        {
            string jsonContent = AESEncryption.DecryptFile(FileConst.CommandPath);
            _commands = JsonConvert.DeserializeObject<List<Command>>(jsonContent);
        }

    }
}
