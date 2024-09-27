using HandyControl.Data;
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
        public BatchView()
        {
            InitializeComponent();

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
                ((BatchViewModel)DataContext).IsTemplate = false;
                ButtonCompare.Visibility = ButtonCreate.Visibility = Visibility.Collapsed;
            }
            ((BatchViewModel)DataContext)?.PageUpdatedCommand.Execute(new FunctionEventArgs<int>(1));
        }
    }
}
