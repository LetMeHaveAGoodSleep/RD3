using ImTools;
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
    public class CondensationController
    {
        private float _sp = -1;

        private BackgroundWorker _backgroundWorker;

        private DeviceParameter _currentDeviceParameter;
        public DeviceParameter CurrentDeviceParameter
        {
            get => _currentDeviceParameter;
            private set => _currentDeviceParameter = value;
        }

        public CondensationController(DeviceParameter deviceParameter)
        {
            _currentDeviceParameter = deviceParameter;
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
                    CondensationParam param = new CondensationParam()
                    {
                        Enable = CurrentDeviceParameter.CondensationParam.Enable,
                        SP = _currentDeviceParameter.CondensationParam.SP
                    };
                    InstrumentSolution.GetInstance().CommandWrapper.SetCondensationControl(_currentDeviceParameter.Name, param);
                    _sp = _currentDeviceParameter.CondensationParam.SP;
                }
                catch (Exception ex)
                {
                    LogHelper.Debug($"设置冷凝温度出错" + ex.Message);
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
                        if (_sp != _currentDeviceParameter.CondensationParam.SP)
                        {
                            CondensationParam param = new CondensationParam()
                            {
                                Enable = CurrentDeviceParameter.CondensationParam.Enable,
                                SP = _currentDeviceParameter.CondensationParam.SP
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetCondensationControl(_currentDeviceParameter.Name, param);
                            _sp = _currentDeviceParameter.CondensationParam.SP;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug($"设置冷凝温度出错" + ex.Message);
                    }
                    Thread.Sleep(1000);
                }
            };
            _backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                BackgroundWorker backgroundWorker = s as BackgroundWorker;
                backgroundWorker.Dispose();
                backgroundWorker = null;

                CondensationParam param = new CondensationParam()
                {
                    Enable = CurrentDeviceParameter.CondensationParam.Enable,
                    SP = _currentDeviceParameter.CondensationParam.SP
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetCondensationControl(_currentDeviceParameter.Name, param);
            };
            _backgroundWorker.RunWorkerAsync();
        }

        public void StopWork(DeviceParameter CurrentDeviceParameter = null)
        {
            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                _backgroundWorker.CancelAsync();
                Thread.Sleep(100);
            }

            CondensationParam param = new CondensationParam()
            {
                Enable = CurrentDeviceParameter.CondensationParam.Enable,
                SP = 0
            };
            InstrumentSolution.GetInstance().CommandWrapper.SetCondensationControl(_currentDeviceParameter.Name, param);
        }
    }
}
