using System;
using System.Globalization;
using System.Windows.Data;

namespace RD3
{
    public class StringToBoolConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 将字符串转换为布尔值，例如 "true" -> true, "false" -> false
            if (value is string strValue)
            {
                return bool.TryParse(strValue, out bool result) && result;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 可选：将布尔值转换回字符串
            return value?.ToString() ?? "false";
        }
    }
}
