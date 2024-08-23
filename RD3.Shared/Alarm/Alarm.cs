using log4net.Core;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class Alarm : BindableBase
    {
        private string _id;
        public string ID
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
        private string _description;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        private AlarmGrade _grade;
        public AlarmGrade Grade
        {
            get { return _grade; }
            set { SetProperty(ref _grade, value); }
        }

        private int _repeat;
        public int Repeat
        {
            get { return _repeat; }
            set { SetProperty(ref _repeat, value); }
        }

        public override string ToString()
        {
             return $"{DateTime.Now.ToString("yyyy:MM:dd HH:mm:ss")} {Grade.ToString()} {Description}";
        }
    }

    public enum AlarmGrade
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }
}
