using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace RD3
{
    public class MultiVisibilityToVisibilityConverter : IMultiValueConverter
    {
        // 定义逻辑类型（默认为逻辑与）
        public enum LogicType { AND, OR }
        public LogicType Logic { get; set; } = LogicType.AND;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return Visibility.Collapsed;

            // 检查所有值是否为Visibility类型
            var visibilities = values
                .Where(v => v is Visibility)
                .Cast<Visibility>()
                .ToArray();

            if (visibilities.Length != values.Length)
                return Visibility.Collapsed; // 存在非Visibility类型时默认隐藏

            // 根据逻辑类型计算结果
            bool result = (Logic == LogicType.AND);
            foreach (var visibility in visibilities)
            {
                bool isVisible = (visibility == Visibility.Visible);
                switch (Logic)
                {
                    case LogicType.AND:
                        result &= isVisible;
                        break;
                    case LogicType.OR:
                        result |= isVisible;
                        break;
                }
            }

            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // 通常不需要反向转换
        }
    }
}
