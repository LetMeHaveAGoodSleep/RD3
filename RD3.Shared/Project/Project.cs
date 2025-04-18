using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class Project : BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _account;
        public string Account
        {
            get { return _account; }
            set { SetProperty(ref _account, value); }
        }

        private string _client;
        public string Client
        {
            get { return _client; }
            set { SetProperty(ref _client, value); }
        }

        public DateTime StartDate { get; set; }

        public DateTime CloseDate { get; set; }

        public DateTime CreatDate { get; set; }

        public string Creator { get; set; }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        private ProjectStatus _status;
        public ProjectStatus Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        public string StatusName
        {
            get { return _status.ToString(); }
        }
    }
}
