using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class PHParam : BasicParam
    {
        private PHControlMode _pHControlMode = PHControlMode.PID;
        [Description("控制策略")]
        public PHControlMode PHControlMode
        {
            get { return _pHControlMode; }
            set { SetPropertyWithAudit(nameof(PHControlMode), ref _pHControlMode, value); }
        }

        private bool _acidAssociated = true;
        [Description("是否关联酸")]
        public bool AcidAssociated
        {
            get { return _acidAssociated; }
            set { SetPropertyWithAudit(nameof(AcidAssociated), ref _acidAssociated, value); }
        }

        private bool _baseAssociated = true;
        [Description("是否关联碱")]
        public bool BaseAssociated
        {
            get { return _baseAssociated; }
            set { SetPropertyWithAudit(nameof(BaseAssociated), ref _baseAssociated, value); }
        }

        private PIDInfo _acidPID;
        public PIDInfo AcidPID
        {
            get => _acidPID;
            set
            {
                SetProperty(ref _acidPID, value);
            }
        }

        private PIDInfo _basePID;
        public PIDInfo BasePID
        {
            get => _basePID;
            set
            {
                SetProperty(ref _basePID, value);
            }
        }
    }
}
