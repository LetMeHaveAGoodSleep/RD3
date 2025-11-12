using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RD3
{
    public class NumericRangeRule : ValidationRule
    {
        public float Min { get; set; } = 0;
        public float Max { get; set; } = 100;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || !float.TryParse(value.ToString(), out float numericValue))
                return new ValidationResult(false, "必须输入数字");

            if (numericValue < Min || numericValue > Max)
                return new ValidationResult(false, $"值必须在 {Min} 和 {Max} 之间");

            return ValidationResult.ValidResult;
        }
    }
}
