using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    [Table(nameof(ParameterNode))]
    public class ParameterNode : BindableBase
    {
        private int _id;
        /// <summary>
        /// 主键（自增）
        /// </summary>
        [Key] // 标记为主键
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 自增约束
        public int ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _displayName = "未命名参数";
        public string DisplayName
        {
            get => _displayName;
            set { SetProperty(ref _displayName, value); }
        }

        private string _fieldName;
        public string FieldName
        {
            set { SetProperty(ref _fieldName, value); }
            get => _fieldName;
        }

        private double _minValue = 0d;
        public double MinValue
        {
            get => _minValue;
            set { SetProperty(ref _minValue, value); }
        }

        private double _maxValue = 100d;
        public double MaxValue
        {
            get => _maxValue;
            set { SetProperty(ref _maxValue, value); }
        }

        private string _unit;
        public string Unit
        {
            get => _unit;
            set { SetProperty(ref _unit, value); }
        }

        private float _lineWidth = 1;
        public float LineWidth
        {
            get => _lineWidth;
            set { SetProperty(ref _lineWidth, value); }
        }

        private float _pointSize = 3;
        public float PointSize
        {
            get => _pointSize;
            set { SetProperty(ref _pointSize, value); }
        }

        private string _colorStr = "0,0,0";
        public string ColorStr
        {
            set { SetProperty(ref _colorStr, value); }
            get => _colorStr;
        }

        private string _colorHex = "#FF000000";
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ColorHex
        {
            get => _colorHex;
            set => SetProperty(ref _colorHex, value);
        }

        private bool _isUsed = true;
        public bool IsUsed
        {
            set { SetProperty(ref _isUsed, value); }
            get => _isUsed;
        }

        private bool _isResponse = true;
        public bool IsResponse
        {
            set { SetProperty(ref _isResponse, value); }
            get => _isResponse;
        }

        private bool _isDesign = true;
        public bool IsDesign
        {
            set { SetProperty(ref _isDesign, value); }
            get => _isDesign;
        }

        public override string ToString()
        {
            return _displayName;
        }
    }
}
