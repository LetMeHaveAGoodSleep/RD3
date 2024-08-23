using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class ClientConfig : BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _host;
        public string Host
        {
            get { return _host; }
            set { SetProperty(ref _host, value); }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { SetProperty(ref _port, value); }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
        }

        private bool _isServer;
        public bool IsServer
        {
            get { return _isServer; }
            set { SetProperty(ref _isServer, value); }
        }

        private int _reconnectDelay;
        public int ReconnectDelay
        {
            get { return _reconnectDelay; }
            set { SetProperty(ref _reconnectDelay, value); }
        }

        private int _retryTimes;
        public int RetryTimes
        {
            get { return _retryTimes; }
            set { SetProperty(ref _retryTimes, value); }
        }

        public string TypeName
        {
            get { return this.ToString(); }
        }
        public override string ToString()
        {
            return "TCP Client";
        }
    }
}
