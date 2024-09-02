using ImTools;
using Prism.Events;
using RD3.Common.Events;
using RD3.Extensions;
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
    /// EditBatchView.xaml 的交互逻辑
    /// </summary>
    public partial class EditBatchView : UserControl
    {
        private readonly IEventAggregator _aggregator;
        public EditBatchView(IEventAggregator aggregator)
        {
            InitializeComponent();
            _aggregator = aggregator;
            aggregator.ResgiterMessage((MessageModel model) => 
            {
                cmbReactor.SelectedItems.Clear();
                Batch batch = model.Model as Batch;
                var array = batch?.Reactor?.Split(',');
                if (array == null) return;
                foreach (var item in array)
                {
                    cmbReactor.SelectedItems.Add(item);
                } 
            }, nameof(EditBatchViewModel));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            List<string> list = new List<string>();
            foreach (var item in cmbReactor.SelectedItems)
            {
                list.Add(item.ToString());
            }
            ((EditBatchViewModel)this.DataContext).Batch.Reactor = string.Join(",", list);
            ((EditBatchViewModel)this.DataContext).OKCommand.Execute();
        }
    }
}
