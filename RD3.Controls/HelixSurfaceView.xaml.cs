using HelixToolkit.Wpf;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using RD3.Shared;
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
using static System.Formats.Asn1.AsnWriter;

namespace RD3.Controls
{
    /// <summary>
    /// HelixSphereView.xaml 的交互逻辑
    /// </summary>
    public partial class HelixSurfaceView : UserControl
    {
        private ModelVisual3D _axesModel;
        private SphereVisual3D[] _sphereVisual3DArray;
        private ModelVisual3D _surfaceModel; // 响应面

        public HelixSurfaceView()
        {
            InitializeComponent();
        }

        public void Generate3DView(Vector<double> parameters, List<string> terms, DataTable data, Dictionary<string, (double, double)> dictionary, int length = 10, int divider = 5)
        {
            if (_axesModel != null)
            {
                helixViewport3D.Children.Remove(_axesModel);
            }
            List<(string colName, (double min, double max) limit)> source = [];
            foreach (KeyValuePair<string, (double, double)> item in dictionary)
            {
                source.Add((item.Key, item.Value));
            }
            var exps = ResponseSurfaceSceneBuilder.ParseTermsToExponents(dictionary.Keys.Count, terms);
            var ranges = DenseMatrix.OfArray(new double[dictionary.Keys.Count, 2]);
            for (int i = 0; i < source.Count; i++)
            {
                ranges[i, 0] = source[i].limit.min;
                ranges[i, 1] = source[i].limit.max;
            }
            (double zmin1, double zmax1) = ResponseSurfaceSceneBuilder.EstimateZRange(parameters, exps, ranges);
            ranges[2, 0] = ranges[2, 0] <= zmin1 ? ranges[2, 0] : zmin1;
            ranges[2, 1] = ranges[2, 1] >= zmin1 ? ranges[2, 1] : zmax1;

            _axesModel = SurfaceBuilder.CreateScientificAxes(source, length, divider);
            helixViewport3D.Children.Add(_axesModel);
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
                    point.X = Math.Abs(SurfaceBuilder.Normalization(ranges[0,0], ranges[0, 1], Convert.ToDouble(data.Rows[i][0])) * 10 - 10);
                }
                if (data.Columns.Count > 1)
                {
                    point.Y = Math.Abs(SurfaceBuilder.Normalization(ranges[1, 0], ranges[1, 1], Convert.ToDouble(data.Rows[i][1])) * 10 - 10);
                }
                if (data.Columns.Count > 2)
                {
                    point.Z = SurfaceBuilder.Normalization(ranges[2, 0], ranges[2, 1], Convert.ToDouble(data.Rows[i][2])) * 10;
                }
                list.Add(SurfaceBuilder.CreateSphere(point));
            }
            _sphereVisual3DArray = list.ToArray();
            foreach (var item in _sphereVisual3DArray)
            {
                helixViewport3D.Children.Add(item);
            }

            if (_surfaceModel != null)
            {
                helixViewport3D.Children.Remove(_surfaceModel);
            }

            // 3) 构建圆滑场景（坐标轴 + 曲面）
            _surfaceModel = ResponseSurfaceSceneBuilder.BuildSmoothSurfaceScene(parameters, terms, dictionary,splitsPerAxis: 160, length: 10, divider: 5);
            helixViewport3D.Children.Add(_surfaceModel);
        }
    }
}
