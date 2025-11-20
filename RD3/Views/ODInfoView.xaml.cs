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
using Microsoft.ML.OnnxRuntime;
using RD3.Shared;
using RD3.ViewModels;

namespace RD3.Views
{
    /// <summary>
    /// peristalticpump.xaml 的交互逻辑
    /// </summary>
    public partial class ODInfoView : UserControl
    {
        public ODInfoView()
        {
            InitializeComponent();
        }
    }
}
