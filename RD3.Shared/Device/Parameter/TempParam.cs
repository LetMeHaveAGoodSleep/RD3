using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class TempParam : BasicParam
    {
        private bool _isEnable = false;
        [Description("是否使能")]
        public bool IsEnable
        {
            get { return _isEnable; }
            set 
            {
                SetPropertyWithAudit(ref _isEnable, value);
                if (value)
                {
                    TecControlMode = TecControlMode.PIDControl;
                }
                else
                {
                    TecControlMode = TecControlMode.Close;
                }
            }
        }

        private TecControlMode _tecControlMode = TecControlMode.Close;
        [Description("控制模式")]
        public TecControlMode TecControlMode
        {
            get { return _tecControlMode; }
            set { SetPropertyWithAudit(ref _tecControlMode, value); }
        }

        private TECParam _tecParam;
        public TECParam PIDParam
        {
            get => _tecParam;
            set
            {
                SetProperty(ref _tecParam, value);
            }
        }
    }
}
