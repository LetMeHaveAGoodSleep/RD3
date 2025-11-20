using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RD3
{
    public class BoolToBoolStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool flag)
            {
                return flag ? Boolean.TrueString : Boolean.FalseString;
            }
            else
            {
                return Boolean.FalseString;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return false;
            }
            if (value.ToString().Equals(Boolean.FalseString, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            else if (value.ToString().Equals(Boolean.TrueString, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
