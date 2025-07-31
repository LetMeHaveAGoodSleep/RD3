using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RD3
{
    public class DynamicFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !double.TryParse(value.ToString(), out var num))
                return value;

            return num % 1 == 0 ? num.ToString("F0") : num.ToString("F2");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value; // 实际值由ViewModel维护
        }
    }
}
