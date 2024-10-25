using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using RD3.Common.Models;
using RD3.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RD3.ViewModels
{
    class CalibrateViewModel : NavigationViewModel
    {
        private bool _calibrating;
        public bool Calibrating
        {
            get { return _calibrating; }
            set { SetProperty(ref _calibrating, value); }
        }

        private DispatcherTimer calibrateTimer;

        private TimeSpan elapsedTime;

        private string _formattedTime = "00:00:00";

        public string FormattedTime
        {
            get => _formattedTime;
            set
            {
                SetProperty(ref _formattedTime, value);
            }
        }

        public CalibrateViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
            calibrateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            calibrateTimer.Tick += CalibrateTimer_Tick; ;
        }

        private void CalibrateTimer_Tick(object sender, EventArgs e)
        {
            elapsedTime += TimeSpan.FromSeconds(1);
            FormattedTime = elapsedTime.ToString(@"hh\:mm\:ss");
        }
    }
}
