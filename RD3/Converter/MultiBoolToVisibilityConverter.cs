using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RD3
{
    public class MultiBoolToVisibilityConverter : IMultiValueConverter
    {
        public LogicType Logic { get; set; } = LogicType.AND;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return Visibility.Collapsed;

            // 检查所有值是否为布尔类型
            bool result = true;
            foreach (var value in values)
            {
                if (!(value is bool))
                    return Visibility.Collapsed;

                bool currentValue = (bool)value;
                switch (Logic)
                {
                    case LogicType.AND:
                        result &= currentValue;
                        break;
                    case LogicType.OR:
                        result |= currentValue;
                        break;
                }
            }

            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // 通常不需要反向转换
        }
    }
}
