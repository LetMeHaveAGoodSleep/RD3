using ImTools;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class PadDOPIDViewModel : BaseViewModel, IDialogAware
    {
        public DeviceParameter CurrentDeviceParameter
        {
            get { return AnalysisSolution.GetInstance().CurrentFermentor.Device; }
        }

        public PadDOPIDViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {

        }

        public string Title => "溶氧PID";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            var dircetPID = PIDInfoManager.GetInstance().PIDInfos.FindFirst(t => t.deviceID == CurrentDeviceParameter.Name && t.Factor == PIDFactor.DO_Dircet);
            if (dircetPID == null)
            {
                PIDInfoManager.GetInstance().PIDInfos.Add(CurrentDeviceParameter.DOParam.DirectPID);
            }
            else
            {
                dircetPID = CurrentDeviceParameter.DOParam.DirectPID;
            }

            var reversePID = PIDInfoManager.GetInstance().PIDInfos.FindFirst(t => t.deviceID == CurrentDeviceParameter.Name && t.Factor == PIDFactor.DO_Reverse);
            if (reversePID == null)
            {
                PIDInfoManager.GetInstance().PIDInfos.Add(CurrentDeviceParameter.DOParam.ReversePID);
            }
            else
            {
                reversePID = CurrentDeviceParameter.DOParam.ReversePID;
            }
            PIDInfoManager.GetInstance().Save();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (CurrentDeviceParameter.DOParam.DirectPID == null)
            {
                var dircetPID = PIDInfoManager.GetInstance().PIDInfos.FindFirst(t => t.deviceID == CurrentDeviceParameter.Name && t.Factor == PIDFactor.DO_Dircet);
                if (dircetPID == null)
                {
                    dircetPID = new PIDInfo()
                    {
                        deviceID = CurrentDeviceParameter.Name,
                        Factor = PIDFactor.DO_Dircet,
                        Interval = 1,
                        maxSpeed = Const.MaxAgit,
                        Threshold = 100
                    };
                }
                CurrentDeviceParameter.DOParam.DirectPID = dircetPID;
            }

            if (CurrentDeviceParameter.DOParam.ReversePID == null)
            {
                var reversePID = PIDInfoManager.GetInstance().PIDInfos.FindFirst(t => t.deviceID == CurrentDeviceParameter.Name && t.Factor == PIDFactor.DO_Reverse);
                if (reversePID == null)
                {
                    reversePID = new PIDInfo()
                    {
                        deviceID = CurrentDeviceParameter.Name,
                        Factor = PIDFactor.DO_Reverse,
                        Interval = 1,
                        maxSpeed = Const.MaxAgit,
                        Threshold = 100
                    };
                }
                CurrentDeviceParameter.DOParam.ReversePID = reversePID;
            }
        }
    }
}
