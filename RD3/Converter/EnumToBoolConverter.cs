using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RD3
{
    public class EnumToBoolConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            string targetValue = parameter.ToString();
            string sourceValue = value.ToString();

            return sourceValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
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
