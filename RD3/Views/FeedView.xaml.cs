using Prism.Events;
using RD3.Extensions;
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
using static MaterialDesignThemes.Wpf.Theme;

namespace RD3.Views
{
    /// <summary>
    /// FeedView.xaml 的交互逻辑
    /// </summary>
    public partial class FeedView : UserControl
    {
        public FeedView(IEventAggregator aggregator)
        {
            InitializeComponent();

            //注册提示消息
            aggregator.ResgiterMessage(arg =>
            {
                SnakeBar.MessageQueue.Enqueue(arg.Message);
            }, nameof(FeedViewModel));
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Image image = sender as Image;
            if (!(sender is Image image))
            {
                return;
            }
            foreach (TabItem item in TabControl.Items)
            {
                if (image.Tag?.ToString() == item.Name)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        private void ComboBoxTimer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtTimer1.Text = txtTimer2.Text = e.AddedItems[e.AddedItems.Count - 1]?.ToString();
        }
    }
}
