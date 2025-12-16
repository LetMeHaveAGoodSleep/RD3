using HelixToolkit.Wpf;
using log4net.Core;
using Prism.Events;
using RD3.Common;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RD3.Views
{
    /// <summary>
    /// DOEDesignView.xaml 的交互逻辑
    /// </summary>
    public partial class DOEDesignView : UserControl
    {
        private ModelVisual3D _model;
        private SphereVisual3D[] _sphereVisual3DArray;

        public DOEDesignView(IEventAggregator aggregator)
        {
            InitializeComponent();
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
            e.Column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as DOEDesignViewModel).GenerateCommand.Execute();
            var data = (this.DataContext as DOEDesignViewModel).DataSource.Copy();
            if (data.Columns.Count > 0)
            {
                data.Columns.RemoveAt(0); // 删除第一列
            }
            Dictionary<string, (double, double)> dictionary = new Dictionary<string, (double, double)>();
            int level = (this.DataContext as DOEDesignViewModel).Level;
            foreach (var item in (this.DataContext as DOEDesignViewModel).DesignCol)
            {
                // 收集当前 Level 范围内的所有值
                var levelValues = new List<double>();

                // 根据 Level 数值添加对应 LevelN 的值
                if (level >= 1 && double.TryParse(item.Level1.ToString(), out double l1))
                    levelValues.Add(l1);
                if (level >= 2 && double.TryParse(item.Level2.ToString(), out double l2))
                    levelValues.Add(l2);
                if (level >= 3 && double.TryParse(item.Level3.ToString(), out double l3))
                    levelValues.Add(l3);
                if (level >= 4 && double.TryParse(item.Level4.ToString(), out double l4))
                    levelValues.Add(l4);
                if (level >= 5 && double.TryParse(item.Level5.ToString(), out double l5))
                    levelValues.Add(l5);
                // 计算并存储最小/最大值
                if (levelValues.Any())
                {
                    dictionary.Add(item.Name, (levelValues.Min(), levelValues.Max()));
                }
            }
            helixSphereView.Generate3DView(data, dictionary);
        }
    }
}

