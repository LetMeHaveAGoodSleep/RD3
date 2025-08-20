using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RD3
{
    public class GreaterThanConverter : IValueConverter
    {
        // 默认阈值（可通过Binding的ConverterParameter动态传入）
        public double Threshold { get; set; } = 0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 解析输入值和阈值
            double inputValue = System.Convert.ToDouble(value);
            double threshold = (parameter != null) ?
                              System.Convert.ToDouble(parameter) :
                              Threshold;

            // 比较并返回布尔结果
            return inputValue > threshold;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 单向绑定无需反向转换
            throw new NotImplementedException();
        }
    }
}
