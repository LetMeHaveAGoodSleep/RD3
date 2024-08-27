using RD3.Common;
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
    /// UserView.xaml 的交互逻辑
    /// </summary>
    public partial class UserView : UserControl
    {
        public UserView()
        {
            InitializeComponent();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            User user = dataGrid.SelectedItem as User;
            ((UserViewModel)this.DataContext)?.EditCommand.Execute(user);
        }
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            User user = dataGrid.SelectedItem as User;
            ((UserViewModel)this.DataContext)?.DeleteCommand.Execute(user);
        }
    }
}
