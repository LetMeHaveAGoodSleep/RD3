using HandyControl.Tools.Extension;
using HandyControl.Tools;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HandyControl.Controls;
using RD3.Common;
using RD3.ViewModels;
using RD3.Shared;
using System.Data;

namespace RD3.Views
{
    /// <summary>
    /// DOEMainView.xaml 的交互逻辑
    /// </summary>
    public partial class DOEMainView : UserControl
    {
        private GridLength _columnDefinitionWidth;

        public DOEMainView()
        {
            InitializeComponent();
        }

        private void ListboxFactor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.RemovedItems.Count < 1 && e.AddedItems.Count < 1) return;
            //foreach (var item in e.RemovedItems)
            //{
            //    if (((DOEMainViewModel)DataContext).SelectedFactors.Contains((Factor)item))
            //    {
            //        ((DOEMainViewModel)DataContext).SelectedFactors.Remove((Factor)item);
            //    }
            //}

            //foreach (var item in e.AddedItems)
            //{
            //    if (!((DOEMainViewModel)DataContext).SelectedFactors.Contains((Factor)item))
            //    {
            //        ((DOEMainViewModel)DataContext).SelectedFactors.Add((Factor)item);
            //    }
            //}

            ((DOEMainViewModel)DataContext).DesignEnable = true;
        }

        private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // 通过 DataTable 索引器绑定，TwoWay + 实时写回
            if (e.Column is DataGridTextColumn textCol)
            {
                textCol.Binding = new Binding($"[{e.PropertyName}]")
                {
                    Mode = e.Column.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    ValidatesOnDataErrors = true,
                    NotifyOnSourceUpdated = true
                };
                textCol.IsReadOnly = e.Column.IsReadOnly;
            }
        }
    }
}
