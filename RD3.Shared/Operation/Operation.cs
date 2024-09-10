using log4net.Core;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class Operation : BindableBase
    {
        private string _batch;
        public string Batch
        {
            get { return _batch; }
            set { SetProperty(ref _batch, value); }
        }
        private string _reactor;
        public string Reactor
        {
            get { return _reactor; }
            set { SetProperty(ref _reactor, value); }
        }
        private string _module;
        public string Module
        {
            get { return _module; }
            set { SetProperty(ref _module, value); }
        }
        private string _operationStatement;
        public string OperationStatement
        {
            get { return _operationStatement; }
            set { SetProperty(ref _operationStatement, value); }
        }
        private string _description;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }
        public DateTime OccurrenceTime { get; set; }
    }
}
