using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class PHParam : BasicParam
    {
        private PHControlMode _pHControlMode = PHControlMode.PID;
        public PHControlMode PHControlMode
        {
            get { return _pHControlMode; }
            set { SetProperty(ref _pHControlMode, value); }
        }

        private bool _acidAssociated = true;
        public bool AcidAssociated
        {
            get { return _acidAssociated; }
            set { SetProperty(ref _acidAssociated, value); }
        }

        private bool _baseAssociated = true;
        public bool BaseAssociated
        {
            get { return _baseAssociated; }
            set { SetProperty(ref _baseAssociated, value); }
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
