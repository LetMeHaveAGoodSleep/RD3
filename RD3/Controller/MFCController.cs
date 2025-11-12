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
    public class MFCController
    {
        private float _flowRateSP = -1;

        private BackgroundWorker _backgroundWorker;

        public DeviceParameter CurrentDeviceParameter
        {
            get { return AnalysisSolution.GetInstance().CurrentFermentor.Device; }
        }

        private MFCInfo _mfcInfo;
        public MFCInfo MFCInfo
        {
            get => _mfcInfo;
            set => _mfcInfo = value;
        }

        public MFCController()
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
                    GasParam gasParam = new GasParam()
                    {
                        MFCNo = MFCInfo.MFCIndex,
                        GasType = MFCInfo.Gas,
                        FlowSpeed = MFCInfo.FlowRate_SP
                    };
                    InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(MFCInfo.DeviceID, gasParam);
                    _flowRateSP = MFCInfo.FlowRate_SP;
                }
                catch (Exception ex)
                {
                    LogHelper.Debug($"MFC{MFCInfo.MFCIndex}设置流量出错" + ex.Message);
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
                        if (_flowRateSP != MFCInfo.FlowRate_SP)
                        {
                            GasParam gasParam = new GasParam()
                            {
                                MFCNo = MFCInfo.MFCIndex,
                                GasType = MFCInfo.Gas,
                                FlowSpeed = MFCInfo.FlowRate_SP
                            };
                            InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(MFCInfo.DeviceID, gasParam);
                            _flowRateSP = MFCInfo.FlowRate_SP;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug($"MFC{MFCInfo.MFCIndex}设置流量出错" + ex.Message);
                    }
                    Thread.Sleep(1000);
                }
            };
            _backgroundWorker.RunWorkerCompleted += (s, e) =>
            {
                BackgroundWorker backgroundWorker = s as BackgroundWorker;
                backgroundWorker.Dispose();
                backgroundWorker = null;

                GasParam gasParam = new GasParam()
                {
                    MFCNo = MFCInfo.MFCIndex,
                    GasType = MFCInfo.Gas,
                    FlowSpeed = 0
                };
                InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(MFCInfo.DeviceID, gasParam);
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

            GasParam gasParam = new GasParam()
            {
                MFCNo = MFCInfo.MFCIndex,
                GasType = MFCInfo.Gas,
                FlowSpeed = 0
            };
            InstrumentSolution.GetInstance().CommandWrapper.SetGasSpeed(MFCInfo.DeviceID, gasParam);
        }
    }
}
