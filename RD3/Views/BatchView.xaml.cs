using HandyControl.Data;
using Prism.Events;
using Prism.Ioc;
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
    /// BatchView.xaml 的交互逻辑
    /// </summary>
    public partial class BatchView : UserControl
    {
        private readonly ILanguage Language;

        private readonly IEventAggregator aggregator;
        public BatchView(IContainerProvider containerProvider, IEventAggregator aggregator)
        {
            InitializeComponent();

            this.Language = containerProvider.Resolve<ILanguage>();
            this.aggregator = aggregator;

            foreach (RadioButton item in ButtonGroup.Items)
            {
                item.Checked += RadioButton_Checked;
            }
            tabControl.SelectionChanged += TabControl_SelectionChanged;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            ((BatchViewModel)this.DataContext)?.AddCommand.Execute();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            object o = tabHistory.IsSelected == true ? dataGrid.SelectedItem : dataGrid1.SelectedItem;
            ((BatchViewModel)this.DataContext)?.EditCommand.Execute(o);
        }
        private void ButtonView_Click(object sender, RoutedEventArgs e)
        {
            object o = tabHistory.IsSelected == true ? dataGrid.SelectedItem : dataGrid1.SelectedItem;
            ((BatchViewModel)this.DataContext)?.ViewCommand.Execute(o);
        }
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            object o = tabHistory.IsSelected == true ? dataGrid.SelectedItem : dataGrid1.SelectedItem;
            ((BatchViewModel)this.DataContext)?.DeleteCommand.Execute(o);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Batch batch = dataGrid.SelectedItem as Batch;
            if (button?.Name == "BtnFavorite")
            {
                batch.IsFavorite = false;
                BatchManager.GetInstance().Save();
            }
            else if (button?.Name == "BtnNoFavorite")
            {
                batch.IsFavorite = true;
                BatchManager.GetInstance().Save();
            }
        }

        private void TabControl_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            var control = tabControl.Template.FindName("headerPanel", tabControl) as TabPanel;
            control.Visibility = Visibility.Collapsed;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object o = tabHistory.IsSelected == true ? dataGrid.SelectedItem : dataGrid1.SelectedItem;
            ((BatchViewModel)this.DataContext)?.ViewCommand.Execute(o);
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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null) return;
            TabItem tabItem = e.AddedItems[e.AddedItems.Count - 1] as TabItem;
            if (tabItem == tabHistory)
            {
                ((BatchViewModel)DataContext).IsTemplate = false;
                ButtonCompare.Visibility = ButtonCreate.Visibility = Visibility.Visible;
            }
            else
            {
                ((BatchViewModel)DataContext).IsTemplate = true;
                ButtonCompare.Visibility = ButtonCreate.Visibility = Visibility.Collapsed;
            }
            ((BatchViewModel)DataContext)?.SearchCommand.Execute(new FunctionEventArgs<string>(TxtSearch.Text));
        }

        private void TxtSearch_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            if (pagination.PageIndex != 1)
            {
                pagination.PageIndex = 1;
            }

            ((BatchViewModel)this.DataContext)?.SearchCommand.Execute(e);
        }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl.SelectedItem == tabTemplate)
            {
                if (dataGrid1.SelectedItem == null)
                {
                    MessageBox.Show(Language.GetValue("请选择模板").ToString());
                    return;
                }
                (DataContext as BatchViewModel)?.ExportCommand.Execute(dataGrid1.SelectedItem);
            }
            else
            {
                (DataContext as BatchViewModel)?.ExportCommand.Execute(null);
            }
        }

        private void ButtonUseTemplate_Click(object sender, RoutedEventArgs e)
        {
            aggregator.SendMessage("", dataGrid1.SelectedItem.GetType().Name, dataGrid1.SelectedItem);
            MessageBox.Show(Language.GetValue("设置成功").ToString());
        }
    }
}
