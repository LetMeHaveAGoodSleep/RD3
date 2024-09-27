using RD3.ViewModels;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace RD3.Views
{
    /// <summary>
    /// AuditView.xaml 的交互逻辑
    /// </summary>
    public partial class AuditView : UserControl
    {
        public AuditView()
        {
            InitializeComponent();

            foreach (RadioButton item in ButtonGroup.Items)
            {
                item.Checked += RadioButton_Checked;
            }
        }

        private void TxtSearch_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            Tuple<string, string, string> tuple = new Tuple<string, string, string>(dtpStart.Text, dtpEnd.Text, e.Info);
            ((OperationViewModel)this.DataContext)?.FilterCommand.Execute(tuple);
        }

        private void TabControl_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            var control = tabControl.Template.FindName("headerPanel", tabControl) as TabPanel;
            control.Visibility = Visibility.Collapsed;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            foreach (TabItem item in tabControl.Items)
            {
                if (radioButton.Tag?.ToString() == item.Name)
                {
                    item.IsSelected = true;
                    break;
                }    
            }
        }
    }
}
