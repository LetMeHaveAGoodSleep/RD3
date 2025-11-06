using HelixToolkit.Wpf;
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

namespace RD3.Controls
{
    /// <summary>
    /// HelixSphereView.xaml 的交互逻辑
    /// </summary>
    public partial class HelixSphereView : UserControl
    {
        private ModelVisual3D _model;
        private SphereVisual3D[] _sphereVisual3DArray;
        private ModelVisual3D _surfaceModel; // 响应面

        public HelixSphereView()
        {
            InitializeComponent();
        }

        public void Generate3DView(DataTable data, Dictionary<string, (double, double)> dictionary, int length = 10, int divider = 5)
        {
            if (_model != null)
            {
                helixViewport3D.Children.Remove(_model);
            }
            List<(string colName, (double min, double max) limit)> source = [];
            foreach (KeyValuePair<string, (double, double)> item in dictionary)
            {
                source.Add((item.Key, item.Value));
            }
            _model = SurfaceBuilder.CreateScientificAxes(source, length, divider);
            helixViewport3D.Children.Add(_model);
            if (_sphereVisual3DArray != null)
            {
                foreach (var item in _sphereVisual3DArray)
                {
                    helixViewport3D.Children.Remove(item);
                }
            }
            List<SphereVisual3D> list = [];
            for (int i = 0; i < data.Rows.Count; i++)
            {
                var point = new Point3D(0, 0, 0);
                if (data.Columns.Count > 0)
                {
                    var (low, high) = dictionary[data.Columns[0].ColumnName];
                    point.X = Math.Abs(SurfaceBuilder.Normalization(low, high, Convert.ToDouble(data.Rows[i][0])) * 10 - 10);
                }
                if (data.Columns.Count > 1)
                {
                    var (low, high) = dictionary[data.Columns[1].ColumnName];
                    point.Y = Math.Abs(SurfaceBuilder.Normalization(low, high, Convert.ToDouble(data.Rows[i][1])) * 10 - 10);
                }
                if (data.Columns.Count > 2)
                {
                    var (low, high) = dictionary[data.Columns[2].ColumnName];
                    point.Z = SurfaceBuilder.Normalization(low, high, Convert.ToDouble(data.Rows[i][2])) * 10;
                }
                list.Add(SurfaceBuilder.CreateSphere(point));
            }
            _sphereVisual3DArray = list.ToArray();
            foreach (var item in _sphereVisual3DArray)
            {
                helixViewport3D.Children.Add(item);
            }
        }
    }
}
