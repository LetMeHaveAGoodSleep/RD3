using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// ChooseRTParamView.xaml 的交互逻辑
    /// </summary>
    public partial class ChooseRTParamView : UserControl
    {
        public ChooseRTParamView()
        {
            InitializeComponent();

            InitCtrl();
        }

        public void InitCtrl()
        {
            Type type = typeof(RealTimeParam);
            List<PropertyInfo> fields = type.GetProperties().ToList().FindAll(c => c.CanRead && c.CanWrite && c.CanRead);
            colFieldName.ItemsSource = fields;
            colFieldName.DisplayMemberPath = "Name";
            colFieldName.SelectedValuePath = "Name";
        }
    }
}
