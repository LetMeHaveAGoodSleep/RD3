using RD3.Shared;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RD3
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProjectStatus status)
            {
                switch (status)
                {
                    case ProjectStatus.Unstarted:
                        return new SolidColorBrush(System.Windows.Media.Colors.DarkOrange);
                        case ProjectStatus.Running:
                        return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xCA,0XF5,0XC8));
                    case ProjectStatus.Complete:
                        return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xB5, 0XD0, 0XFF));
                }
            }
            return new SolidColorBrush(System.Windows.Media.Colors.DarkOrange);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
