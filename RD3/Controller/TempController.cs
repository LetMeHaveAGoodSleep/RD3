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
    public class TempController
    {
        private float _tempSP = -1;

        private BackgroundWorker _backgroundWorker;

        public DeviceParameter CurrentDeviceParameter
        {
            get { return AnalysisSolution.GetInstance().ReactorCol[0]; }
        }

        public TempController()
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
                    CurrentDeviceParameter.TempParam.IsEnable = true;
                    CurrentDeviceParameter.TempParam.SP = Math.Clamp(CurrentDeviceParameter.TempParam.SP, CurrentDeviceParameter.TempParam.LowerLimit, CurrentDeviceParameter.TempParam.UpperLimit);
                    InstrumentSolution.GetInstance().CommandWrapper.SetTempSetting(CurrentDeviceParameter.Name, CurrentDeviceParameter.TempParam);
                    _tempSP = CurrentDeviceParameter.TempParam.SP;
                }
                catch (Exception ex)
                {
                    LogHelper.Debug("设置温控异常" + ex.Message);
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
                        if (_tempSP != CurrentDeviceParameter.TempParam.SP || CurrentDeviceParameter.TempParam.SP < CurrentDeviceParameter.TempParam.LowerLimit || CurrentDeviceParameter.TempParam.SP > CurrentDeviceParameter.TempParam.UpperLimit)
                        {
                            CurrentDeviceParameter.TempParam.IsEnable = true;
                            CurrentDeviceParameter.TempParam.SP = Math.Clamp(CurrentDeviceParameter.TempParam.SP, CurrentDeviceParameter.TempParam.LowerLimit, CurrentDeviceParameter.TempParam.UpperLimit);
                            InstrumentSolution.GetInstance().CommandWrapper.SetTempSetting(CurrentDeviceParameter.Name, CurrentDeviceParameter.TempParam);
                            _tempSP = CurrentDeviceParameter.AgitParam.SP;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("温控设置失败" + ex.Message);
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
            CurrentDeviceParameter.TempParam.IsEnable = false;
            InstrumentSolution.GetInstance().CommandWrapper.SetTempSetting(CurrentDeviceParameter.Name, CurrentDeviceParameter.TempParam);
        }
    }
}
