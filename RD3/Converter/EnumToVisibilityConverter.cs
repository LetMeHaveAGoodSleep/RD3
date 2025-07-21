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

            var currentEnum = value.ToString();
            var enumStrings = parameter.ToString().Split(',').Select(s => s.Trim());

            return enumStrings.Contains(currentEnum) ? Visibility.Visible : Visibility.Collapsed;
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
