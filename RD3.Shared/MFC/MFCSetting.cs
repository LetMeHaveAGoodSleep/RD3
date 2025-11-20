using Prism.Mvvm;
using RD3.Shared.Util;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace RD3.Shared
{
    public class MFCSetting : AuditParam
    {
        private string _deviceID = "G01";
        public string DeviceID
        {
            get => _deviceID;
            private set { SetProperty(ref _deviceID, value); }
        }

        private int _mfcIndex = -1;
        public int MFCIndex
        {
            get => _mfcIndex;
            set { SetProperty(ref _mfcIndex, value); }
        }

        private GasType _gas = GasType.Air;
        [Description("MFC用途")]
        public GasType Gas
        {
            get => _gas;
            set
            {
                GasName = EnumUtil.GetEnumDescription(value);
                SetPropertyWithAudit(ref _gas, value);
            }
        }

        private string _gasName = "未设置";
        public string GasName
        {
            get => _gasName;
            private set { SetProperty(ref _gasName, value); }
        }

        private bool _isExist = false;
        public bool IsExist
        {
            get => _isExist;
            set { SetProperty(ref _isExist, value); }
        }

        private bool _isEnable = false;
        [Description("是否使能")]
        public bool IsEnable
        {
            get => _isEnable;
            set { SetPropertyWithAudit(ref _isEnable, value); }
        }
    }
}
