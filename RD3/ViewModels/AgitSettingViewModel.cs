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
    public class AgitSettingViewModel : BaseViewModel, IDialogAware
    {
        private DeviceParameter _currentDeviceParameter;
        public DeviceParameter CurrentDeviceParameter
        {
            get { return _currentDeviceParameter; }
            set { SetProperty(ref _currentDeviceParameter, value); }
        }

        private Dictionary<string, BackgroundWorker> dicAgitWorker = new Dictionary<string, BackgroundWorker>();
        private Dictionary<string, float> dicAgitSP = new Dictionary<string, float>();

        public DelegateCommand<DeviceParameter> AgitRunCommand => new((DeviceParameter device) =>
        {
            var currentDeviceParameter = CurrentDeviceParameter;
            if (device != null)
            {
                currentDeviceParameter = device;
            }

            if (dicAgitWorker.ContainsKey(currentDeviceParameter.Name) && dicAgitWorker[currentDeviceParameter.Name] != null && dicAgitWorker[currentDeviceParameter.Name].IsBusy)
            {
                dicAgitWorker[currentDeviceParameter.Name].CancelAsync();
                Thread.Sleep(100);
            }

            if (currentDeviceParameter.AgitParam.IsControling)
            {
                try
                {
                    dicAgitWorker[currentDeviceParameter.Name] = new BackgroundWorker();
                    dicAgitWorker[currentDeviceParameter.Name].WorkerSupportsCancellation = true;
                    dicAgitWorker[currentDeviceParameter.Name].WorkerReportsProgress = true;
                    dicAgitWorker[currentDeviceParameter.Name].DoWork += (s, e) =>
                    {
                        try
                        {
                            currentDeviceParameter.AgitParam.Agit_PV = currentDeviceParameter.AgitParam.Agit_PV >= Const.MaxAgit ? Const.MaxAgit : currentDeviceParameter.AgitParam.Agit_PV;
                            CommandWrapper.SetAgitSpeed(currentDeviceParameter.Name, currentDeviceParameter.AgitParam.Agit_PV);
                            dicAgitSP[currentDeviceParameter.Name] = currentDeviceParameter.AgitParam.Agit_PV;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Debug("设置转速异常" + ex.Message);
                        }

                        var deviceParameter = AnalysisSolution.GetInstance().ReactorCol.FindFirst(t => t.Name == currentDeviceParameter.Name);
                        BackgroundWorker worker = s as BackgroundWorker;
                        while (true)
                        {
                            if (worker.CancellationPending)
                            {
                                return;
                            }
                            try
                            {
                                if (dicAgitSP[deviceParameter.Name] != deviceParameter.AgitParam.Agit_PV)
                                {
                                    CommandWrapper.SetAgitSpeed(deviceParameter.Name, deviceParameter.AgitParam.Agit_PV);
                                    dicAgitSP[deviceParameter.Name] = deviceParameter.AgitParam.Agit_PV;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("转速控制失败" + ex.Message);
                            }
                            Thread.Sleep(1000);
                        }
                    };
                    dicAgitWorker[currentDeviceParameter.Name].RunWorkerCompleted += (s, e) =>
                    {
                        BackgroundWorker backgroundWorker = s as BackgroundWorker;
                        backgroundWorker.Dispose();
                        backgroundWorker = null;
                    };
                    dicAgitWorker[currentDeviceParameter.Name].RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("转速控制失败" + ex.Message);
                }
            }
            else
            {
                Task.Run(() =>
                {
                    try
                    {
                        CommandWrapper.SetAgitSpeed(currentDeviceParameter.Name, 0);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("转速控制失败" + ex.Message);
                    }
                });
            }
        });

        public AgitSettingViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            CurrentDeviceParameter = AnalysisSolution.GetInstance().ReactorCol[0];
        }

        public string Title => "转速设置";

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
