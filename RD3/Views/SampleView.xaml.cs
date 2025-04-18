using HandyControl.Controls;
using RD3.Shared;
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
    /// SampleView.xaml 的交互逻辑
    /// </summary>
    public partial class SampleView : UserControl
    {
        public SampleView()
        {
            InitializeComponent();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            Sample sample = dataGrid.SelectedItem as Sample;
            ((SampleViewModel)this.DataContext)?.EditCommand.Execute(sample);
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            Sample sample = dataGrid.SelectedItem as Sample;
            ((SampleViewModel)this.DataContext)?.DeleteCommand.Execute(sample);
        }

        private void TxtSearch_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            if (pagination.PageIndex != 1)
            {
                pagination.PageIndex = 1;
            }

            ((SampleViewModel)this.DataContext)?.SearchCommand.Execute(e);
        }
    }
}
