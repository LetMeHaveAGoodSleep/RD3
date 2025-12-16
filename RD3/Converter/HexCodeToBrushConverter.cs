using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace RD3
{
    /// <summary>
    /// 方成 把HexCode转成Brush
    /// 
    /// </summary>
    public class HexCodeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentColor = ColorUtil.FromHexCode(value?.ToString());
            System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(currentColor.A, currentColor.R, currentColor.G, currentColor.B);
            SolidColorBrush brush = new SolidColorBrush(color);
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
