using HelixToolkit.Wpf;
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

            //Create3DScatterPlot();
        }

        private void Create3DScatterPlot()
        {
            // 1. 准备示例数据（替换为您的实际数据）
            var dataPoints = new List<Point3D>
        {
            // 格式：new Point3D(pH_SP, DO_SP, S_ALK)
            new Point3D(7.0, 5.2, 3.0),  // 示例数据点1
            new Point3D(6.4, 4.8, 2.5),  // 示例数据点2
            new Point3D(6.0, 5.0, 2.8),  // 示例数据点3
            // 添加更多数据点...
        };

            // 2. 创建3D视口
            var viewport = new HelixViewport3D
            {
                ZoomExtentsWhenLoaded = true,
                Background = Brushes.White
            };

            // 3. 添加默认光源
            viewport.Children.Add(new DefaultLights());

            // 4. 添加红色小球（散点）
            foreach (var point in dataPoints)
            {
                var sphere = new SphereVisual3D
                {
                    Center = point,
                    Radius = 0.2,  // 控制小球大小
                    Material = MaterialHelper.CreateMaterial(Colors.Red),
                    BackMaterial = MaterialHelper.CreateMaterial(Colors.Red)
                };
                viewport.Children.Add(sphere);
            }

            // 5. 设置坐标轴
            // X轴 (pH_SP)
            var xAxis = new ArrowVisual3D
            {
                Point1 = new Point3D(0, 0, 0),
                Point2 = new Point3D(8, 0, 0),  // 根据数据范围调整
                Diameter = 0.05,
                Fill = Brushes.Black
            };
            viewport.Children.Add(xAxis);
            viewport.Children.Add(new TextVisual3D
            {
                Text = "pH_SP",
                Position = new Point3D(8.5, 0, 0),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold
            });

            // Y轴 (DO_SP)
            var yAxis = new ArrowVisual3D
            {
                Point1 = new Point3D(0, 0, 0),
                Point2 = new Point3D(0, 6, 0),  // 根据数据范围调整
                Diameter = 0.05,
                Fill = Brushes.Black
            };
            viewport.Children.Add(yAxis);
            viewport.Children.Add(new TextVisual3D
            {
                Text = "DO_SP",
                Position = new Point3D(0, 6.5, 0),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold
            });

            // Z轴 (S_ALK)
            var zAxis = new ArrowVisual3D
            {
                Point1 = new Point3D(0, 0, 0),
                Point2 = new Point3D(0, 0, 4),  // 根据数据范围调整
                Diameter = 0.05,
                Fill = Brushes.Black
            };
            viewport.Children.Add(zAxis);
            viewport.Children.Add(new TextVisual3D
            {
                Text = "S_ALK",
                Position = new Point3D(0, 0, 4.5),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold
            });

            // 6. 添加刻度标签（X轴示例）
            for (double x = 0; x <= 8; x += 1)
            {
                viewport.Children.Add(new TextVisual3D
                {
                    Text = x.ToString("F1"),
                    Position = new Point3D(x, -0.3, -0.3),
                    Foreground = Brushes.Black,
                    Height = 0.2
                });
            }

            // 7. 添加图例（使用不同颜色表示不同pH值）
            var legendPoints = new Dictionary<string, Color>
        {
            { "7.0", Colors.Red },
            { "6.4", Colors.DarkRed },
            { "6.0", Colors.OrangeRed },
            // 添加更多图例项...
        };

            double legendY = 7.0;
            foreach (var item in legendPoints)
            {
                viewport.Children.Add(new TextVisual3D
                {
                    Text = item.Key,
                    Position = new Point3D(9, legendY, 0),
                    Foreground = new SolidColorBrush(item.Value),
                    FontWeight = FontWeights.Bold,
                    Height = 0.3
                });
                legendY -= 0.5;
            }

            // 8. 添加标题
            viewport.Children.Add(new BillboardTextVisual3D
            {
                Text = "蜘蛛网蛋白的pH值与SP值关系图",
                Position = new Point3D(4, 8, 0),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            });

            // 9. 将视口添加到窗口
            Content = viewport;
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
    }
}
