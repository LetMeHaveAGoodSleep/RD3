using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RD3
{
    public class NumberToSpecialStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNumeric = value.GetType().IsValueType && value.GetType().IsPrimitive;
            if (isNumeric)
            {
                float temp = float.Parse(value.ToString());
                if (temp == 0) return "-";
                else return temp.ToString();
            }
            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
