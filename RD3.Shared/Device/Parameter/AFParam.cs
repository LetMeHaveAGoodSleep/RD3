using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class AFParam : BasicParam
    {
        public AFParam()
        {
            LowerLimit = 0;
            UpperLimit = Const.MaxPumpFlowRate;
        }

        private bool _autoDefoaming = false;
        [Description("是否消泡")]
        public bool AutoDefoaming
        {
            get { return _autoDefoaming; }
            set { SetPropertyWithAudit(nameof(AutoDefoaming), ref _autoDefoaming, value); }
        }

        private int _cycle;
        [Description("消泡周期")]
        public int Cycle
        {
            get => _cycle;
            set { SetPropertyWithAudit(nameof(Cycle), ref _cycle, value); }
        }

        private float _dutyCycle;
        [Description("消泡占空比")]
        public float DutyCycle
        {
            get => _dutyCycle;
            set { SetPropertyWithAudit(nameof(DutyCycle), ref _dutyCycle, value); }
        }
    }
}
