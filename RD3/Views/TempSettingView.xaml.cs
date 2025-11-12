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
    /// TempSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class TempSettingView : UserControl
    {
        public TempSettingView()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is TempSettingViewModel settingViewModel && tsView.DataContext is TimeSeriesViewModel timeSeriesViewModel)
            {
                timeSeriesViewModel.BasicParam = settingViewModel.BasicParam;
            }
        }
    }
}
