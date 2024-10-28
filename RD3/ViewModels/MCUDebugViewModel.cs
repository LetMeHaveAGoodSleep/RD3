using HandyControl.Controls;
using Prism.Commands;
using Prism.Ioc;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class MCUDebugViewModel : NavigationViewModel
    {
        public DelegateCommand<string> ReadCommand => new((string commandText) => 
        {
            int command = Convert.ToInt32(commandText);
            foreach (var item in CommunicationManager.GetInstance().TcpClients) 
            {
                if (!item.IsConnected) continue;
                byte[] bytes =
                [
                    (byte)((command >> 8) & 0xFF), // 低字节
                    (byte)(command & 0xFF),// 高字节    
                ];
                var command1 = CommandManager.GetInstance().Commands.Find(t => t.ID == command);
                List<byte> bytes2 = new List<byte> { 0x55 };
                item.SendData(bytes, bytes2.ToArray());
            }
        });


        public DelegateCommand<string> SetCommand => new((string commandText) =>
        {
            int command = Convert.ToInt32(commandText);
            var command1 = CommandManager.GetInstance().Commands.Find(t => t.ID == command);
            if (command1 == null)
            {
                MessageBox.Show("无此命令");
                return;
            }
            foreach (var item in CommunicationManager.GetInstance().TcpClients)
            {
                if (!item.IsConnected) continue;
                byte[] bytes =
                [
                    (byte)((command >> 8) & 0xFF), // 低字节
                    (byte)(command & 0xFF),// 高字节    
                ];
                List<byte> bytes2 = new List<byte> { 0x66 };
                var a = BitConverter.GetBytes(30f);
                var b = BitConverter.GetBytes(31f);
                var c = BitConverter.GetBytes(29f);
                Array.Reverse(a);
                Array.Reverse(b);
                Array.Reverse(c);
                bytes2.AddRange(b);
                bytes2.AddRange(b);
                bytes2.AddRange(c);
                item.SendData(bytes, bytes2.ToArray());
            }
        });

        public MCUDebugViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }
    }
}
