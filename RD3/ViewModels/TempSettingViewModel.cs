using ImTools;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class TempSettingViewModel : BaseViewModel, IDialogAware
    {
        private DeviceParameter _currentDeviceParameter;
        public DeviceParameter CurrentDeviceParameter
        {
            get { return _currentDeviceParameter; }
            set { SetProperty(ref _currentDeviceParameter, value); }
        }

        #region 温控相关
        private Dictionary<string, BackgroundWorker> dicTempTimeWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, BackgroundWorker> dicTempWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, float> dicTempSP = new Dictionary<string, float>();
        private Dictionary<string, BackgroundWorker> dicTempDOWorker = new Dictionary<string, BackgroundWorker>();
        #endregion


        public DelegateCommand<DeviceParameter> TempRunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }
            if (dicTempWorker.ContainsKey(currentDeviceParameter.Name) && dicTempWorker[currentDeviceParameter.Name] != null && dicTempWorker[currentDeviceParameter.Name].IsBusy)
            {
                dicTempWorker[currentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            if (currentDeviceParameter.TempParam.IsControling)
            {
                try
                {
                    dicTempWorker[currentDeviceParameter.Name] = new BackgroundWorker();
                    dicTempWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true;
                    dicTempWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;
                    dicTempWorker[currentDeviceParameter.Name].DoWork += (s, e) =>
                    {
                        LogHelper.Debug($"反应器{currentDeviceParameter.Name}开始温控");
                        try
                        {
                            currentDeviceParameter.TempParam.IsEnable = true;
                            InstrumentSolution.GetInstance().CommandWrapper.SetTempSetting(currentDeviceParameter.Name, currentDeviceParameter.TempParam);
                            dicTempSP[currentDeviceParameter.Name] = currentDeviceParameter.TempParam.Temp_PV;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("温控异常" + ex.Message);
                        }
                        var deviceParameter = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        BackgroundWorker worker = s as BackgroundWorker;
                        while (true)
                        {
                            if (worker.CancellationPending)
                            {
                                e.Result = deviceParameter.Name;
                                return;
                            }
                            try
                            {
                                if (dicTempSP[deviceParameter.Name] != deviceParameter.TempParam.Temp_PV)
                                {
                                    deviceParameter.TempParam.IsEnable = true;
                                    InstrumentSolution.GetInstance().CommandWrapper.SetTempSetting(deviceParameter.Name, deviceParameter.TempParam);
                                    dicTempSP[deviceParameter.Name] = deviceParameter.TempParam.Temp_PV;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("温度控制失败" + ex.Message);
                            }
                            Thread.Sleep(1000);
                        }
                    };
                    dicTempWorker[currentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                    {
                        BackgroundWorker backgroundWorker = s as BackgroundWorker;
                        backgroundWorker.Dispose();
                        backgroundWorker = null;

                        var deviceParameter = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == e.Result?.ToString());
                        deviceParameter.TempParam.IsEnable = false;
                        InstrumentSolution.GetInstance().CommandWrapper.SetTempSetting(deviceParameter.Name, deviceParameter.TempParam);
                    };
                    dicTempWorker[currentDeviceParameter.Name].RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("温度控制失败" + ex.Message);
                }
            }
            else
            {
                Task.Run(() =>
                {
                    try
                    {
                        currentDeviceParameter.TempParam.IsEnable = false;
                        InstrumentSolution.GetInstance().CommandWrapper.SetTempSetting(currentDeviceParameter.Name, currentDeviceParameter.TempParam);
                    }
                    catch (Exception ex) { }
                });
            }
        });

        public TempSettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            CurrentDeviceParameter = AnalysisSolution.GetInstance().ReactorCol[0];
        }

        public string Title => "温度设置";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}
