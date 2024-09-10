using Prism.Events;
using RD3.Common.Events;
using RD3.ViewModels;
using RD3.Shared;
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
using RD3.Extensions;

namespace RD3.Views
{
    /// <summary>
    /// EditProjectView.xaml 的交互逻辑
    /// </summary>
    public partial class EditProjectView : UserControl
    {
        private readonly IEventAggregator _aggregator;
        public EditProjectView(IEventAggregator aggregator)
        {
            InitializeComponent();
            aggregator.ResgiterMessage((MessageModel model) =>
            {
                Project project = model.Model as Project;
            }, nameof(EditBatchViewModel));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
