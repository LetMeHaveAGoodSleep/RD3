using RD3.ViewModels;
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
    /// UnscheduledView.xaml 的交互逻辑
    /// </summary>
    public partial class UnscheduledView : UserControl
    {
        public UnscheduledView()
        {
            InitializeComponent();

            ChkAll.Checked += ChkAll_Checked;
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
        }

        private void ChkAll_Checked(object sender, RoutedEventArgs e)
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
                item.IsChecked = true;
            }
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
        private List<string> GetCheckedDevice()
        {
            List<string> devices = new List<string>();
            var parent = ChkAll.Parent;
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child == null) continue;
                var item = child as CheckBox;
                if (item == null) continue;
                if (item.Name != nameof(ChkAll) && (bool)item.IsChecked)
                {
                    devices.Add(item.Content.ToString());
                }
            }
            return devices;
        }

        private void BtnClearAll_Click(object sender, RoutedEventArgs e)
        {
            if((bool)ChkAll.IsChecked) ChkAll.IsChecked = false;
            TxtVloume.Text = string.Empty;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(TxtVloume.Text, out double temp))
            {
                MessageBox.Show("请输入数值");
                return;
            }
            if (temp <= 0)
            {
                MessageBox.Show("请输入大于0的数值");
                return;
            }
            ((UnscheduledViewModel)DataContext).Devices = GetCheckedDevice();
            ((UnscheduledViewModel)DataContext)?.OKCommand.Execute();
        }
    }
}
