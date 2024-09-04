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
    /// BatchView.xaml 的交互逻辑
    /// </summary>
    public partial class BatchView : UserControl
    {
        public BatchView()
        {
            InitializeComponent();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            Batch batch = dataGrid.SelectedItem as Batch;
            ((BatchViewModel)this.DataContext)?.EditCommand.Execute(batch);
        }
        private void ButtonView_Click(object sender, RoutedEventArgs e)
        {
            Batch batch = dataGrid.SelectedItem as Batch;
            ((BatchViewModel)this.DataContext)?.ViewCommand.Execute(batch);
        }
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            Batch batch = dataGrid.SelectedItem as Batch;
            ((BatchViewModel)this.DataContext)?.DeleteCommand.Execute(batch);
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
    }
}
