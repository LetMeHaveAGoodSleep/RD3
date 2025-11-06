using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    /// <summary>
    /// 审计追踪信息基类
    /// </summary>
    public class AuditParam : BindableBase, ICloneable
    {
        private string _moduleName;
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName 
        { 
            get { return _moduleName; }
            set { SetProperty(ref _moduleName, value); }
        }

        private bool _isAuditing = false;
        /// <summary>
        /// 启用审计追踪与否
        /// </summary>
        public bool IsAuditing
        {
            get { return _isAuditing; }
            set { SetProperty(ref _isAuditing, value); }
        }

        private bool _isManualChange = true;
        /// <summary>
        /// 是否人为更改
        /// </summary>
        public bool IsManualChange
        {
            get { return _isManualChange; }
            set { SetProperty(ref _isManualChange, value); }
        }

        public object Clone()
        {
            var clonedObject = ObjectCloner.DeepCopy(this);
            return clonedObject;
        }
    }
}
