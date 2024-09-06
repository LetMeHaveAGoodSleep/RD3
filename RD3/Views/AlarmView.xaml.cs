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
using HandyControl.Data;

namespace RD3.Views
{
    /// <summary>
    /// AlarmView.xaml 的交互逻辑
    /// </summary>
    public partial class AlarmView : UserControl
    {
        public AlarmView()
        {
            InitializeComponent();
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string, string> tuple=new Tuple<string, string, string>(dtpStart.Text,dtpEnd.Text, txtSearch.Text);
            ((AlarmViewModel)this.DataContext)?.FilterCommand.Execute(tuple);
        }

        private void TxtSearch_SearchStarted(object sender, FunctionEventArgs<string> e)
        {
            Tuple<string, string, string> tuple = new Tuple<string, string, string>(dtpStart.Text, dtpEnd.Text, e.Info);
            ((AlarmViewModel)this.DataContext)?.FilterCommand.Execute(tuple);
        }
    }
}
