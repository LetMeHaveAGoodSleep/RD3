using HandyControl.Controls;
using RD3.Shared;
using RD3.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// DOSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class DOSettingView : UserControl
    {
        public DOSettingView()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is DOSettingViewModel settingViewModel && tsView.DataContext is TimeSeriesViewModel timeSeriesViewModel)
            {
                timeSeriesViewModel.BasicParam = settingViewModel.BasicParam;
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;  // 从1开始递增
        }
    }
}
