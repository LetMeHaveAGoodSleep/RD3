using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    /// <summary>
    /// 报表节点类
    /// </summary>
    public class ReportNode : BindableBase
    {
        #region 私有字段
        // 数据库存储字段
        private string _fieldName;

        // 表头显示名称
        private string _showName;

        // 数据类型（默认值：System.Single）
        private string _typeName = "System.Single";

        private bool _used; 
                            
        private int _width = 100;// 列宽（默认值：100）
        // 单位（默认值：空字符串）
        private string _unit = string.Empty;
        #endregion

        #region 公共属性
        /// <summary>
        /// 数据库存储字段名
        /// </summary>
        public string FieldName
        {
            get => _fieldName;
            set => SetProperty(ref _fieldName, value); // 简化单行set语法
        }

        /// <summary>
        /// 表头显示名称
        /// </summary>
        public string ShowName
        {
            get => _showName;
            set => SetProperty(ref _showName, value);
        }

        /// <summary>
        /// 数据类型（默认：System.Single）
        /// </summary>
        public string TypeName
        {
            get => _typeName;
            set => SetProperty(ref _typeName, value);
        }

        /// <summary>
        /// 是否启用该字段
        /// </summary>
        public bool Used
        {
            get => _used;
            set => SetProperty(ref _used, value);
        }

        /// <summary>
        /// 列宽（默认：100）
        /// </summary>
        public int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        /// <summary>
        /// 数值单位
        /// </summary>
        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }
        #endregion
    }
}
