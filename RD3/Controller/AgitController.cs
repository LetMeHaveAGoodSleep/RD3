using ImTools;
using Prism.Mvvm;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RD3.Controller
{
    public class AgitController
    {
        private int _agitSP = -1;

        private BackgroundWorker _backgroundWorker;

        public DeviceParameter CurrentDeviceParameter
        {
            get { return AnalysisSolution.GetInstance().ReactorCol[0]; }
        }

        public AgitController()
        {

        }

        public void StartWork()
        {
            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                return;
            }

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.DoWork += (s, e) =>
            {
                try
                {
                    CurrentDeviceParameter.AgitParam.Agit_PV = Math.Clamp(CurrentDeviceParameter.AgitParam.Agit_PV, CurrentDeviceParameter.AgitParam.LowerLimit, CurrentDeviceParameter.AgitParam.UpperLimit);
                    InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(CurrentDeviceParameter.Name, CurrentDeviceParameter.AgitParam.Agit_PV);
                    _agitSP = CurrentDeviceParameter.AgitParam.Agit_PV;
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("设置转速异常" + ex.Message);
                }
                BackgroundWorker worker = s as BackgroundWorker;
                while (true)
                {
                    if (worker.CancellationPending)
                    {
                        return;
                    }
                    try
                    {
                        if (_agitSP != CurrentDeviceParameter.AgitParam.Agit_PV || CurrentDeviceParameter.AgitParam.Agit_PV < CurrentDeviceParameter.AgitParam.LowerLimit || CurrentDeviceParameter.AgitParam.Agit_PV > CurrentDeviceParameter.AgitParam.UpperLimit)
                        {
                            CurrentDeviceParameter.AgitParam.Agit_PV = Math.Clamp(CurrentDeviceParameter.AgitParam.Agit_PV, CurrentDeviceParameter.AgitParam.LowerLimit, CurrentDeviceParameter.AgitParam.UpperLimit);
                            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(CurrentDeviceParameter.Name, CurrentDeviceParameter.AgitParam.Agit_PV);
                            _agitSP = CurrentDeviceParameter.AgitParam.Agit_PV;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("转速控制失败" + ex.Message);
                    }
                    Thread.Sleep(1000);
                }
            };
            _backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                BackgroundWorker backgroundWorker = s as BackgroundWorker;
                backgroundWorker.Dispose();
                backgroundWorker = null;
            };
            _backgroundWorker.RunWorkerAsync();
        }

        public void StopWork()
        {
            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                _backgroundWorker.CancelAsync();
                Thread.Sleep(100);
            }

            InstrumentSolution.GetInstance().CommandWrapper.SetAgitSpeed(CurrentDeviceParameter.Name, 0);
        }
    }
}
