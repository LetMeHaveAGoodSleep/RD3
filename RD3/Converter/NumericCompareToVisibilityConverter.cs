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
    class NumericCompareToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TypeCode typeCode = Type.GetTypeCode(value.GetType());
            TypeCode typeCode1 = Type.GetTypeCode(parameter.GetType());
            if (typeCode >= TypeCode.SByte && typeCode <= TypeCode.Decimal)
            {
                if (decimal.Parse(value.ToString()) <= decimal.Parse(parameter.ToString()))
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("该转换器不支持反向转换");
        }
    }
}
