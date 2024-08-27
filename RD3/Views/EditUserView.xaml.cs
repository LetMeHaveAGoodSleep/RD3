using HandyControl.Controls;
using Prism.Events;
using RD3.Extensions;
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
    /// EditUserView.xaml 的交互逻辑
    /// </summary>
    public partial class EditUserView : UserControl
    {
        private readonly IEventAggregator eventAggregator;
        public EditUserView(IEventAggregator aggregator)
        {
            InitializeComponent();
            eventAggregator = aggregator;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(passWord.Password) || string.IsNullOrWhiteSpace(confirmPwd.Password))
            {
                return;
            }
            ((EditUserViewModel)this.DataContext).User.Password = passWord.Password;
            ((EditUserViewModel)this.DataContext).ConfirmPwd = confirmPwd.Password;
            ((EditUserViewModel)this.DataContext).OKCommand.Execute();
        }
    }
}
