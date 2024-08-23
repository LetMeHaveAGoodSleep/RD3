using log4net.Core;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class AlarmRecord: BindableBase
    {
        private string _description;
        public string Description
        {
            get { return _description; }
            set { _description= value; }
        }

        private string _batch;
        public string Batch
        {
            get { return _batch; }
            set { _batch = value; }
        }

        private string _reactor;
        public string Reactor
        {
            get { return _reactor; }
            set { _reactor = value; }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private AlarmGrade _grade;
        public AlarmGrade Grade
        {
            get { return _grade; }
            set { _grade= value; }
        }

        private DateTime _time;
        public DateTime Time
        {
            get { return _time; }
            set { _time = value; }
        }

        public override string ToString()
        {
             return $"{Time.ToString("yyyy:MM:dd HH:mm:ss")}#{Batch}#{Reactor}#{Value}#{Grade.ToString()}#{Description}";
        }
    }
}
