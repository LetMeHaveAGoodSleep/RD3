using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace RD3.Views
{
    /// <summary>
    /// SettingView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingView : UserControl
    {
        public SettingView()
        {
            InitializeComponent();
        }

        private void TabControl_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            var control = tabControl.Template.FindName("headerPanel", tabControl) as TabPanel;
            control.Visibility = Visibility.Collapsed;
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string filePath = AppDomain.CurrentDomain.BaseDirectory + button?.Tag?.ToString();
            // 检查文件是否存在
            if (File.Exists(filePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true, // 确保使用默认程序打开
                    CreateNoWindow = true    // 不显示命令窗口
                });
            }
            else
            {
                MessageBox.Show("文件未找到: " + filePath);
            }
        }
    }
}
