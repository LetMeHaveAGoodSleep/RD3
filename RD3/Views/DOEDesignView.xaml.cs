using Prism.Events;
using RD3.Common.Events;
using RD3.Extensions;
using RD3.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// DOEDesignView.xaml 的交互逻辑
    /// </summary>
    public partial class DOEDesignView : UserControl
    {
        public DOEDesignView(IEventAggregator aggregator)
        {
            InitializeComponent();

            aggregator.ResgiterMessage((MessageModel model) =>
            {
                if (model.Model is string[] columns)
                {
                    //foreach (var column in columns)
                    //{
                    //    // 创建并添加文本列（DataGridTextColumn）
                    //    DataGridTextColumn textColumn = new DataGridTextColumn();
                    //    textColumn.Header = column;
                    //    textColumn.Width = 150;
                    //    textColumn.Binding = new System.Windows.Data.Binding(column);
                    //    DataGridResult.Columns.Add(textColumn);
                    //}
                }
                else if (model.Model is DataTable dataSource)
                {
                    //DataGridResult.ItemsSource = dataSource.DefaultView;
                }
            }, nameof(DOEDesignViewModel));
        }

        private void DataGridDesign_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)sender;
            // 获取鼠标点击位置对应的单元格信息
            var cellInfo = dataGrid.CurrentCell;
            if (cellInfo.IsValid)
            {
                // 设置当前单元格为编辑状态
                dataGrid.BeginEdit();
                // 获取当前单元格对应的编辑元素（通常是TextBox等）
                var element = dataGrid.Columns[cellInfo.Column.DisplayIndex].GetCellContent(cellInfo.Item) as UIElement;
                if (element != null)
                {
                    element.Focus();
                    // 判断编辑元素是否为TextBox，若是则选中全部文本
                    if (element is TextBox textBox)
                    {
                        textBox.SelectAll();
                    }
                    else if (element is ComboBox comboBox)
                    {
                        comboBox.IsDropDownOpen = true;
                    }
                    // 可以根据实际有更多类型的编辑元素继续添加相应逻辑
                }
            }
        }

        private void DataGridDesign_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Width= new DataGridLength(1, DataGridLengthUnitType.Auto);
        }
    }
}
