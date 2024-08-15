using Prism.Events;
using RD3.Extensions;
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
using System.Windows.Shapes;

namespace RD3.Views
{
    /// <summary>
    /// SelfCheckView.xaml 的交互逻辑
    /// </summary>
    public partial class SelfCheckView : UserControl
    {
        public SelfCheckView()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            //textBox?.ScrollToEnd();
        }
    }
}
