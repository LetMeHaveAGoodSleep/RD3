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
    public class EnumToVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            //string targetValue = parameter.ToString();
            Enum.TryParse(value.GetType(), parameter?.ToString(), true, out var targetValue);
            Enum.TryParse(value.GetType(), value?.ToString(), true, out var sourceValue);
            //string sourceValue = value.ToString();
            return Enum.Equals(targetValue, sourceValue) ? Visibility.Visible : Visibility.Collapsed;
            //return sourceValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object enumValue;
            try
            {
                enumValue = Enum.Parse(targetType, parameter.ToString());
                if (enumValue != null)
                {
                    return enumValue;
                }
                else
                {
                    var values = Enum.GetValues(targetType);
                    enumValue = values.GetValue(0);
                }
            }
            catch
            {
                var values = Enum.GetValues(targetType);
                enumValue = values.GetValue(0);
            }
            return enumValue;
        }
    }
}
