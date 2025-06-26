using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3
{
    public class RegistrationInfo : BindableBase
    {
        private DateTime _startTime = DateTime.MinValue;
        public DateTime StartTime
        {
            get => _startTime;
            set { SetProperty(ref _startTime, value); }
        }

        private DateTime _endTime = DateTime.MinValue;
        public DateTime EndTime
        {
            get => _endTime;
            set { SetProperty(ref _endTime, value); }
        }

        private DateTime _currentTime = DateTime.Now;
        public DateTime CurrentTime
        {
            get => _currentTime;
            set { SetProperty(ref _currentTime, value); }
        }
    }
}
