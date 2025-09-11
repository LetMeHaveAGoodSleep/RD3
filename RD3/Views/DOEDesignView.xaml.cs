using HelixToolkit.Wpf;
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
        public DOEDesignView(IEventAggregator aggregator)
        {
            InitializeComponent();
        }

        public ModelVisual3D CreateScientificAxes(DataTable data, double length = 5, int divider = 5)
        {
            var group = new ModelVisual3D();

            double step = length / divider;

            // === 网格面 ===
            group.Children.Add(CreateXYGrid(length, step, 0));        // XY 面
            group.Children.Add(CreateXZGrid(length, step, 0));        // XZ 面
            group.Children.Add(CreateYZGrid(length, step, 0));        // YZ 面

            if (data.Columns.Count > 0)
            {
                // === X轴 ===
                group.Children.Add(new LinesVisual3D
                {
                    Color = Colors.Black,
                    Thickness = 1,
                    Points = new Point3DCollection
                {
                    new Point3D(length,length,0),
                    new Point3D(0,length,0)
                }
                });
                for (int i = 0; i <= divider; i++)
                {
                    double x = i * step;
                    group.Children.Add(new LinesVisual3D
                    {
                        Color = Colors.Black,
                        Points = new Point3DCollection {
                new Point3D(length-x,length,0),
                new Point3D(length-x,length-0.1,0)
            }
                    });

                    // === X轴标签 ===
                    group.Children.Add(new BillboardTextVisual3D
                    {
                        Text = x.ToString(),
                        Position = new Point3D(length - x, length + 0.5, 0),
                        Height = 0.5,
                        FontSize = 20,
                        Foreground = Brushes.Black,
                        Transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90),
                            new Point3D(length - x, length + 0.5, 0))  // 旋转中心设为文字位置
                    });
                }
                group.Children.Add(new BillboardTextVisual3D
                {
                    Text = data.Columns[0].ColumnName,
                    Position = new Point3D(length / 2, length + 1, 0),
                    Height = 0.4,
                    FontSize = 28,
                    Foreground = Brushes.Black,
                    Transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90),
                            new Point3D(length / 2, length + 1, 0))  // 旋转中心设为文字位置
                });
            }

            if (data.Columns.Count > 1)
            {
                // === Y轴 ===
                group.Children.Add(new LinesVisual3D
                {
                    Color = Colors.Black,
                    Thickness = 1,
                    Points = new Point3DCollection {
                new Point3D(length,length,0),
                new Point3D(length,0,0)
            }
                });
                for (int i = 0; i <= divider; i++)
                {
                    double y = i * step;
                    group.Children.Add(new LinesVisual3D
                    {
                        Color = Colors.Black,
                        Points = new Point3DCollection {
                    new Point3D(length,length-y,0),
                    new Point3D(length-0.1,length-y,0)
                }
                    });
                    // === Y轴标签 (竖排) ===
                    var text = new BillboardTextVisual3D
                    {
                        Text = y.ToString(),
                        //Position = new Point3D(length + 0.5, length - y, 0),
                        Position = new Point3D(0, 0, 0),
                        Height = 0.5,
                        FontSize = 20,
                        FontFamily = AppSession.FontFamily,
                        Foreground = Brushes.Black,
                        Transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90),
                            new Point3D(length + 0.5, length - y, 0))  // 旋转中心设为文字位置
                    };
                    group.Children.Add(text);
                }
                var yAxisText = new BillboardTextVisual3D
                {
                    Text = data.Columns[1].ColumnName,
                    FontSize = 28,
                    Position = new Point3D(length + 1.5, length / 2, 0),
                    Height = 0.4,
                    Foreground = Brushes.Black,
                    Transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90),
                            new Point3D(length + 1.5, length / 2, 0))  // 旋转中心设为文字位置
                };
                group.Children.Add(yAxisText);
            }

            if (data.Columns.Count > 2)
            {
                // === Z轴 ===
                group.Children.Add(new LinesVisual3D
                {
                    Color = Colors.Black,
                    Thickness = 1,
                    Points = new Point3DCollection {
                new Point3D(length,0,0),
                new Point3D(length,0,length)
            }
                });
                for (int i = 0; i <= divider; i++)
                {
                    double z = i * step;
                    group.Children.Add(new LinesVisual3D
                    {
                        Color = Colors.Black,
                        Points = new Point3DCollection {
                    new Point3D(length,0,z),
                    new Point3D(length-0.1,0,z)
                }
                    });
                    // === Z轴标签 (竖直) ===
                    group.Children.Add(new BillboardTextVisual3D
                    {
                        Text = z.ToString(),
                        Position = new Point3D(length + 0.5, 0, z),
                        Height = 0.5,
                        FontSize = 20,
                        Foreground = Brushes.Black,
                        Transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0),
                            new Point3D(length + 0.5, 0, z))
                    });
                }
                group.Children.Add(new BillboardTextVisual3D
                {
                    Text = data.Columns[2].ColumnName,
                    Position = new Point3D(length + 1, 0, length / 2),
                    Height = 0.4,
                    FontSize = 28,
                    Foreground = Brushes.Black,
                    Transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(0, 1, 0), 90),
                            new Point3D(length + 1, 0, length / 2))
                });
            }
            return group;
        }

        // 红色小球
        public SphereVisual3D CreateSphere(Point3D position, double radius = 0.2)
        {
            return new SphereVisual3D
            {
                Center = position,
                Radius = radius,
                Material = MaterialHelper.CreateMaterial(Colors.Red)
            };
        }

        public ModelVisual3D CreateXYGrid(double size, double step, double z = 0, double thickness = 1)
        {
            var grid = new LinesVisual3D
            {
                Thickness = thickness,
                Color = Colors.LightGray,
            };

            var pts = new Point3DCollection();

            // 竖线（平行Y）
            for (double x = 0; x <= size + 1e-6; x += step)
            {
                pts.Add(new Point3D(x, 0, z));
                pts.Add(new Point3D(x, size, z));
            }

            // 横线（平行X）
            for (double y = 0; y <= size + 1e-6; y += step)
            {
                pts.Add(new Point3D(0, y, z));
                pts.Add(new Point3D(size, y, z));
            }

            grid.Points = pts;

            var group = new ModelVisual3D();
            group.Children.Add(grid);
            return group;
        }

        public ModelVisual3D CreateXZGrid(double size, double step, double y = 0, double thickness = 1)
        {
            var grid = new LinesVisual3D { Thickness = thickness, Color = Colors.LightGray };
            var pts = new Point3DCollection();
            for (double x = 0; x <= size + 1e-6; x += step) { pts.Add(new Point3D(x, y, 0)); pts.Add(new Point3D(x, y, size)); }
            for (double z = 0; z <= size + 1e-6; z += step) { pts.Add(new Point3D(0, y, z)); pts.Add(new Point3D(size, y, z)); }
            grid.Points = pts; var g = new ModelVisual3D(); g.Children.Add(grid); return g;
        }

        public ModelVisual3D CreateYZGrid(double size, double step, double x = 0, double thickness = 1)
        {
            var grid = new LinesVisual3D { Thickness = thickness, Color = Colors.LightGray };
            var pts = new Point3DCollection();
            for (double y = 0; y <= size + 1e-6; y += step) { pts.Add(new Point3D(x, y, 0)); pts.Add(new Point3D(x, y, size)); }
            for (double z = 0; z <= size + 1e-6; z += step) { pts.Add(new Point3D(x, 0, z)); pts.Add(new Point3D(x, size, z)); }
            grid.Points = pts; var g = new ModelVisual3D(); g.Children.Add(grid); return g;
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
            helixViewport3D.Children.Add(CreateScientificAxes(data));
            helixViewport3D.Children.Add(CreateSphere(new Point3D(2, 3, 4)));
        }
    }
}
