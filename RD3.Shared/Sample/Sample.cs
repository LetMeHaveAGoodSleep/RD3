using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class Sample:BindableBase
    {
        private string _id;
        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

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

        private SampleType _type;
        public SampleType Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        public DateTime SampleTime { get; set; }

        private SampleParam _mainParam;
        public SampleParam MainParam
        {
            get { return _mainParam; }
            set { SetProperty(ref _mainParam, value); }
        }

        private double _value;
        public double Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public string Creator { get; set; }

        private string _remark;
        public string Remark
        {
            get { return _remark; }
            set { SetProperty(ref _remark, value); }
        }
    }
}
