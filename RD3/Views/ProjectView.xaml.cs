using HandyControl.Data;
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
    public partial class ProjectView : UserControl
    {
        public ProjectView()
        {
            InitializeComponent();

            tgbHistory.Checked += ToggleButton_Checked;
            tgbTemplate.Checked += ToggleButton_Checked;
        }

        private void TabControl_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            var control = tabControl.Template.FindName("headerPanel", tabControl) as TabPanel;
            control.Visibility = Visibility.Collapsed;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if (toggleButton?.Name == nameof(tgbHistory))
            {
                tgbTemplate.IsChecked = !toggleButton.IsChecked;
                tabHistory.IsSelected = (bool)toggleButton.IsChecked;
                ((ProjectViewModel)DataContext).IsTemplate = tabTemplate.IsSelected = !(bool)toggleButton.IsChecked;
                ((ProjectViewModel)DataContext)?.PageUpdatedCommand.Execute(new FunctionEventArgs<int>(1));
                ButtonCreate.Visibility = (bool)toggleButton.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (toggleButton?.Name == nameof(tgbTemplate))
            {
                tgbHistory.IsChecked = !toggleButton.IsChecked;
                ((ProjectViewModel)DataContext).IsTemplate = tabTemplate.IsSelected = (bool)toggleButton.IsChecked;
                tabHistory.IsSelected = !(bool)toggleButton.IsChecked;
                ((ProjectViewModel)DataContext)?.PageUpdatedCommand.Execute(new FunctionEventArgs<int>(1));
                ButtonCreate.Visibility = (bool)toggleButton.IsChecked == true ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            ((ProjectViewModel)this.DataContext)?.AddCommand.Execute();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            object o = tabHistory.IsSelected ? dataGrid.SelectedItem : dataGrid1.SelectedItem;
            ((ProjectViewModel)this.DataContext)?.EditCommand.Execute(o);
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            object o = tabHistory.IsSelected ? dataGrid.SelectedItem : dataGrid1.SelectedItem;
            ((ProjectViewModel)this.DataContext)?.DeleteCommand.Execute(o);
        }

        private void ButtonView_Click(object sender, RoutedEventArgs e)
        {
            object o = tabHistory.IsSelected ? dataGrid.SelectedItem : dataGrid1.SelectedItem;
            ((ProjectViewModel)this.DataContext)?.ViewCommand.Execute(o);
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object o = tabHistory.IsSelected ? dataGrid.SelectedItem : dataGrid1.SelectedItem;
            ((ProjectViewModel)this.DataContext)?.ViewCommand.Execute(o);
        }

        private void TxtSearch_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            if (pagination.PageIndex != 1)
            {
                pagination.PageIndex = 1;
            }

            ((ProjectViewModel)this.DataContext)?.SearchCommand.Execute(e);
        }
    }
}
