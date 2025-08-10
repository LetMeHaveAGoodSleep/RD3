using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RD3
{
    public class EnumToBoolConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string targetValue = parameter.ToString();
            var targetArray = targetValue.Split(',');
            string sourceValue = value.ToString();
            return Array.Exists(targetArray, s => s.ToLower() == sourceValue.ToLower());
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
