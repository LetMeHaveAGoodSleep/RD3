using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RD3.Shared
{
    public class Batch : BindableBase
    {

        private string _id;
        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _reactor;
        public string Reactor
        {
            get { return _reactor; }
            set { SetProperty(ref _reactor, value); }
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Creator { get; set; }

        private string _project;
        public string Project
        {
            get { return _project; }
            set { SetProperty(ref _project, value); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        private string _isFavorite;
        public string IsFavorite
        {
            get { return _isFavorite; }
            set { SetProperty(ref _isFavorite, value); }
        }
    }
}
