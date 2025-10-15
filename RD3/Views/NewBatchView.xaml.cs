using HandyControl.Data;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
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
using XZ.SQLite;

namespace RD3.Views
{
    /// <summary>
    /// BatchView.xaml 的交互逻辑
    /// </summary>
    public partial class NewBatchView : UserControl
    {
        private readonly IDialogHostService dialogHostService;

        private readonly ILanguage Language;

        private readonly IEventAggregator aggregator;

        public NewBatchView()
        {
            InitializeComponent();
        }

        public NewBatchView(IContainerProvider containerProvider, IEventAggregator aggregator)
        {
            InitializeComponent();

            this.Language = containerProvider.Resolve<ILanguage>();
            this.aggregator = aggregator;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            ((NewBatchViewModel)this.DataContext)?.AddCommand.Execute();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            object o =  dataGrid1.SelectedItem;
            ((NewBatchViewModel)this.DataContext)?.EditCommand.Execute(o);
        }
        private void ButtonView_Click(object sender, RoutedEventArgs e)
        {
            object o =  dataGrid1.SelectedItem;
            List<RD3Batch> batches = new List<RD3Batch>();
            batches.Add(o as RD3Batch);
            ((NewBatchViewModel)this.DataContext)?.CompareCommand.Execute(batches);
        }
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid1.SelectedItems.Count < 1)
            {
                return;
            }
            if (HandyControl.Controls.MessageBox.Show("确定要删除当前批次吗？", "温馨提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            List<RD3Batch> list = dataGrid1.SelectedItems.Cast<RD3Batch>().ToList();
            ((NewBatchViewModel)this.DataContext)?.DeleteCommand.Execute(list);
        }


        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object o =  dataGrid1.SelectedItem;
            //((NewBatchViewModel)this.DataContext)?.ViewCommand.Execute(o);
        }


        private void TxtSearch_SearchStarted(object sender, HandyControl.Data.FunctionEventArgs<string> e)
        {
            if (pagination.PageIndex != 1)
            {
                pagination.PageIndex = 1;
            }
            //((NewBatchViewModel)this.DataContext)?.SearchCommand.Execute(e);
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            //(DataContext as NewBatchViewModel)?.ExportCommand.Execute(null);
        }

        private void ButtonUseTemplate_Click(object sender, RoutedEventArgs e)
        {
            aggregator.SendMessage("", dataGrid1.SelectedItem.GetType().Name, dataGrid1.SelectedItem);
            MessageBox.Show(Language.GetValue("设置成功").ToString());
        }

        /// <summary>
        /// 右键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu context = new ContextMenu();
            MenuItem item = new MenuItem() { Width = 100 };
            item.Header = "批次比较";
            item.Click += new RoutedEventHandler(compare_click);
            context.Items.Add(item);
            context.IsOpen = true;

            MenuItem dataOutPut = new MenuItem() { Width = 100 };
            dataOutPut.Header = "数据导出";
            dataOutPut.Click += new RoutedEventHandler(dataOutPut_click);
            context.Items.Add(dataOutPut);
            context.IsOpen = true;

            MenuItem offlineData = new MenuItem() { Width = 100 };
            offlineData.Header = "离线数据";
            offlineData.Click += new RoutedEventHandler(offLinedata_click);
            context.Items.Add(offlineData);
            context.IsOpen = true;
        }

        /// <summary>
        /// 右键-数据导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataOutPut_click(object sender, RoutedEventArgs e)
        {
            if (dataGrid1.SelectedItems.Count > 0)
            {
                List<RD3Batch> batches = new List<RD3Batch>();
                //1弹出选数据//2显示
                foreach (var item in dataGrid1.SelectedItems)
                {
                    batches.Add(item as RD3Batch);
                    break;
                }
            ((NewBatchViewModel)this.DataContext)?.OutputDataCommand.Execute(batches[0]);
            }
        }

        /// <summary>
        /// 导入离线数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void offLinedata_click(object sender, RoutedEventArgs e)
        {
            if (dataGrid1.SelectedItems.Count > 0)
            {
                List<RD3Batch> batches = new List<RD3Batch>();
                foreach (var item in dataGrid1.SelectedItems)
                {
                    batches.Add(item as RD3Batch);
                    break;
                }
            ((NewBatchViewModel)this.DataContext)?.OffLineDataCommand.Execute(batches[0]);
            }
        }

        /// <summary>
        /// 比较
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void compare_click(object sender, RoutedEventArgs e)
        {
            List<RD3Batch> batches = new List<RD3Batch>();
            //1弹出选数据//2显示
            foreach(var item in dataGrid1.SelectedItems)
            {
                batches.Add(item as RD3Batch);
            }
            ((NewBatchViewModel)this.DataContext)?.CompareCommand.Execute(batches);
        }

        public void Refresh()
        {
            ((NewBatchViewModel)this.DataContext)?.ReloadDataCommand.Execute();
        }
    }
}
