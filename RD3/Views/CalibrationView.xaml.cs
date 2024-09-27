using ScottPlot.WPF;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RD3.Views
{
    /// <summary>
    /// CalibrationView.xaml 的交互逻辑
    /// </summary>
    public partial class CalibrationView : UserControl
    {
        public CalibrationView()
        {
            InitializeComponent();

            ChkAll.Checked += ChkAll_CheckChanged;
            ChkAll.Unchecked += ChkAll_Unchecked;

            var parent = ChkAll.Parent;
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child == null) continue;
                var item = child as CheckBox;
                if (item == null) continue;
                item.Checked += CheckBox_CheckChanged;
                item.Unchecked += CheckBox_CheckChanged;
            }


            var plt = wpfPlot1.Plot;
            plt.Legend.IsVisible = true;
            plt.ShowLegend(Edge.Top);


            DateTime[] dates = Generate.ConsecutiveDays(100);
            double[] ys = Generate.RandomWalk(100);
            var scatter = plt.Add.Scatter(dates, ys);
            scatter.LegendText = "1";
            plt.Axes.DateTimeTicksBottom();
            plt.Axes.Bottom.Label.Text = "Time";
            plt.Axes.Left.Label.Text = "DO";
            wpfPlot1.Refresh();
        }

        private void ChkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            var parent = checkBox.Parent;
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child == null) continue;
                var item = child as CheckBox;
                if (item == null || object.ReferenceEquals(item, checkBox)) continue;
                item.IsChecked = false;
            }
        }

        private void ChkAll_CheckChanged(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            var parent = checkBox.Parent;
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++) 
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child == null) continue;
                var item = child as CheckBox;
                if(item == null||object.ReferenceEquals(item,checkBox)) continue;
                item.IsChecked = true;
            }
        }

        private void CheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            var parent = checkBox.Parent;
            var count = VisualTreeHelper.GetChildrenCount(parent);
            int checkedCount = 0;
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child == null) continue;
                var item = child as CheckBox;
                if (item == null) continue;
                if (item.Name == nameof(ChkAll)) continue;
                checkedCount += Convert.ToInt32(item.IsChecked);
            }
            if (checkedCount == count - 1)
            {
                ChkAll.IsChecked = true;
            }
            else if (checkedCount == 0)
            {
                ChkAll.IsChecked = false;
            }
            else 
            { 
                ChkAll.IsChecked = null; 
            }
        }
    }
}
