using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RD3.Shared
{
    public class BatchDevice
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private WorkStatus _status;
        public WorkStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private string _batch;
        public string Batch
        {
            get { return _batch; }
            set { _batch = value; }
        }
    }
}
