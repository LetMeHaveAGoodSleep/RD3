using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RD3.Shared
{
    public class BasicParam : AuditParam, IDataErrorInfo
    {
        private float _sp = 0f;
        /// <summary>
        /// 预设值
        /// </summary>
        [Description("预设值")]
        public float SP
        {
            get { return _sp; }
            set
            {
                float oldValue = _sp;
                // 先更新值（便于触发验证）
                if (SetPropertyWithAudit(ref _sp, value))
                {
                    // 验证失败时恢复旧值
                    if (!string.IsNullOrEmpty(this[nameof(SP)]))
                    {
                        SetPropertyWithAudit(ref _sp, oldValue);
                    }
                }
            }
        }

        private float _lowerLimit = 0;
        /// <summary>
        /// 下限
        /// </summary>
        [Description("下限")]
        public float LowerLimit
        {
            get { return _lowerLimit; }
            set 
            {
                if (SetPropertyWithAudit(ref _lowerLimit, value))
                {
                    ValidateSP(); // 上下限变动时验证SP
                }
            }
        }

        private float _upperLimit = 100;
        /// <summary>
        /// 上限
        /// </summary>
        public float UpperLimit
        {
            get { return _upperLimit; }
            set 
            {
                if (SetPropertyWithAudit(ref _upperLimit, value))
                {
                    ValidateSP(); // 上下限变动时验证SP
                }
            }
        }

        private ControlMode _controlMode = ControlMode.Constant;
        [Description("控制方式")]
        public ControlMode ControlMode
        {
            get { return _controlMode; }
            set { SetPropertyWithAudit(ref _controlMode, value); }
        }

        private TimeSeries _timeSeries = new TimeSeries();
        public TimeSeries TimeSeries
        {
            get => _timeSeries;
            set
            {
                SetProperty(ref _timeSeries, value);
            }
        }

        private bool _lastIsControling = false;
        [JsonIgnore]
        public bool LastIsControling
        {
            get { return _lastIsControling; }
            private set { SetProperty(ref _lastIsControling, value); }
        }

        private bool _isControling = false;
        [Description("是否开启控制")]
        public bool IsControling
        {
            get { return _isControling; }
            set
            {
                LastIsControling = _isControling;
                SetPropertyWithAudit(ref _isControling, value);
            }
        }
        /// <summary>
        /// 验证SP是否在范围内，超出则修正
        /// </summary>
        private void ValidateSP()
        {
            if (SP < LowerLimit)
            {
                SP = LowerLimit; // 自动修正为下限
            }
            else if (SP > UpperLimit)
            {
                SP = UpperLimit; // 自动修正为上限
            }
        }

        #region IDataErrorInfo 实现（核心验证逻辑）
        // 整体错误信息（暂不使用）
        public string Error => null;

        // 按属性名返回错误信息
        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(SP))
                {
                    // 验证SP是否在上下限范围内
                    if (SP < LowerLimit || SP > UpperLimit)
                    {
                        // 返回错误信息（将显示在TextBox上）
                        return $"必须在 [{LowerLimit} - {UpperLimit}] 范围内";
                    }
                }
                return null; // 无错误
            }
        }
        #endregion
    }
}
