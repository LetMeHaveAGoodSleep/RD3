using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using MathNet.Numerics.LinearAlgebra;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MathNet.Numerics.LinearAlgebra.Double;

namespace RD3.Controls
{
    public static class SurfaceBuilder
    {
        private static readonly int _tickFontSize = 30;
        private static readonly int _axesFontSize = 40;
        private static readonly double _tickDistance = 1d;
        private static readonly double _axesDistance = 2d;

        // 曲面(可选等高线) + 科学坐标系，输出一个场景组
        public static Model3DGroup CreateSurfaceScene(
            DataTable table,
            double length = 10,
            int divider = 5)
        {
            // 1) 计算数据范围
            var xs = table.AsEnumerable().Select(r => Convert.ToDouble(r[0]));
            var ys = table.AsEnumerable().Select(r => Convert.ToDouble(r[1]));
            var zs = table.AsEnumerable().Select(r => Convert.ToDouble(r[2]));

            double xmin = xs.Min(), xmax = xs.Max();
            double ymin = ys.Min(), ymax = ys.Max();
            double zmin = zs.Min(), zmax = zs.Max();


            double dx = Math.Max(xmax - xmin, 1e-9);
            double dy = Math.Max(ymax - ymin, 1e-9);
            double dz = Math.Max(zmax - zmin, 1e-9);

            // 将数据坐标线性映射到 [0, length] 立方体
            double sx = length / dx;
            double sy = length / dy;
            double sz = length / dz;

            // 2) 构建曲面（或曲面+等高线）
            Model3D surfaceModel;
            surfaceModel = CreateSurface(table); // GeometryModel3D

            // 3) 应用“平移到原点 + 缩放到 length”的变换
            var t = new Transform3DGroup();
            t.Children.Add(new TranslateTransform3D(-xmin, -ymin, -zmin));
            t.Children.Add(new ScaleTransform3D(sx, sy, sz));
            surfaceModel.Transform = t;

            // 5) 组合为一个场景
            var scene = new Model3DGroup();
            scene.Children.Add(surfaceModel);
            return scene;
        }

        public static GeometryModel3D CreateSurface(DataTable table)
        {
            var xs = table.AsEnumerable().Select(r => Convert.ToDouble(r[0])).Distinct().OrderBy(v => v).ToArray();
            var ys = table.AsEnumerable().Select(r => Convert.ToDouble(r[1])).Distinct().OrderBy(v => v).ToArray();

            double zMin = table.AsEnumerable().Select(r => Convert.ToDouble(r[2])).Min();
            double zMax = table.AsEnumerable().Select(r => Convert.ToDouble(r[2])).Max();
            bool singleZ = Math.Abs(zMax - zMin) < 1e-12;

            var positions = new List<Point3D>();
            var texcoords = new List<Point>();
            var normals = new List<Vector3D>();
            var triIndices = new List<int>();

            for (int i = 0; i < xs.Length - 1; i++)
            {
                for (int j = 0; j < ys.Length - 1; j++)
                {
                    var p00 = new Point3D(xs[i], ys[j], GetZ(table, xs[i], ys[j]));
                    var p10 = new Point3D(xs[i + 1], ys[j], GetZ(table, xs[i + 1], ys[j]));
                    var p11 = new Point3D(xs[i + 1], ys[j + 1], GetZ(table, xs[i + 1], ys[j + 1]));
                    var p01 = new Point3D(xs[i], ys[j + 1], GetZ(table, xs[i], ys[j + 1]));

                    double t00 = singleZ ? 0.5 : (p00.Z - zMin) / (zMax - zMin);
                    double t10 = singleZ ? 0.5 : (p10.Z - zMin) / (zMax - zMin);
                    double t11 = singleZ ? 0.5 : (p11.Z - zMin) / (zMax - zMin);
                    double t01 = singleZ ? 0.5 : (p01.Z - zMin) / (zMax - zMin);

                    // Triangle 1: p00, p10, p11
                    AddTriangle(positions, texcoords, normals, triIndices,
                        p00, p10, p11,
                        new Point(0, t00), new Point(1, t10), new Point(1, t11));

                    // Triangle 2: p00, p11, p01
                    AddTriangle(positions, texcoords, normals, triIndices,
                        p00, p11, p01,
                        new Point(0, t00), new Point(1, t11), new Point(0, t01));
                }
            }

            var mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection(positions),
                TextureCoordinates = new PointCollection(texcoords),
                Normals = new Vector3DCollection(normals),
                TriangleIndices = new Int32Collection(triIndices)
            };

            // 彩虹渐变刷子
            var gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            gradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.0));
            gradient.GradientStops.Add(new GradientStop(Colors.Cyan, 0.25));
            gradient.GradientStops.Add(new GradientStop(Colors.Lime, 0.5));
            gradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.75));
            gradient.GradientStops.Add(new GradientStop(Colors.Red, 1.0));
            gradient.Freeze();

            var matGroup = new MaterialGroup();
            matGroup.Children.Add(new DiffuseMaterial(gradient));
            matGroup.Children.Add(new SpecularMaterial(new SolidColorBrush(Color.FromRgb(220, 220, 220)), 20.0));
            matGroup.Freeze();

            return new GeometryModel3D
            {
                Geometry = mesh,
                Material = matGroup,
                BackMaterial = matGroup
            };
        }

        private static void AddTriangle(
            List<Point3D> positions,
            List<Point> texcoords,
            List<Vector3D> normals,
            List<int> triIndices,
            Point3D a, Point3D b, Point3D c,
            Point ta, Point tb, Point tc)
        {
            int baseIndex = positions.Count;
            positions.Add(a); positions.Add(b); positions.Add(c);
            texcoords.Add(ta); texcoords.Add(tb); texcoords.Add(tc);

            var n = ComputeNormal(a, b, c);
            normals.Add(n); normals.Add(n); normals.Add(n);

            triIndices.Add(baseIndex);
            triIndices.Add(baseIndex + 1);
            triIndices.Add(baseIndex + 2);
        }

        private static Vector3D ComputeNormal(Point3D a, Point3D b, Point3D c)
        {
            var v1 = b - a;
            var v2 = c - a;
            var n = Vector3D.CrossProduct(v1, v2);
            if (n.Length < 1e-9) return new Vector3D(0, 0, 1);
            n.Normalize();
            return n;
        }

        private static double GetZ(DataTable table, double x, double y)
        {
            var row = table.AsEnumerable()
                .FirstOrDefault(r => Convert.ToDouble(r[0]) == x && Convert.ToDouble(r[1]) == y);
            return row != null ? Convert.ToDouble(row[2]) : 0.0;
        }

        public static bool IsInteger(double num)
        {
            // 使用一个极小的容忍值(epsilon)来抵消浮点数精度可能带来的影响
             double epsilon = 1e-10;
            double remainder = num % 1;
            // 考虑余数可能非常接近 0 或非常接近 1（对于负数）
            return Math.Abs(remainder) < epsilon || Math.Abs(remainder - 1) < epsilon;
        }

        public static ModelVisual3D CreateScientificAxes(List<(string colName, (double min, double max) limit)> source, double length = 10, int divider = 5)
        {
            var group = new ModelVisual3D();

            double step = length / divider;

            // === 网格面 ===
            group.Children.Add(CreateXYGrid(length, step, 0));        // XY 面
            group.Children.Add(CreateXZGrid(length, step, 0));        // XZ 面
            group.Children.Add(CreateYZGrid(length, step, 0));        // YZ 面

            if (source.Count > 0)
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
                var (colName, (min, max)) = source[0];
                for (int i = 0; i <= divider; i++)
                {
                    double x = i * step;
                    double value = min + i * (max - min) / divider;
                    group.Children.Add(new LinesVisual3D
                    {
                        Color = Colors.Black,
                        Points = new Point3DCollection {
                new Point3D(length-x,length,0),
                new Point3D(length-x,length-0.1,0)
            }
                    });
                    string format = IsInteger(value) ? "F0" : "F2";
                    // === X轴标签 ===
                    group.Children.Add(new TextVisual3D
                    {
                        Text = value.ToString(format),
                        Position = new Point3D(length - x, length + _tickDistance, 0),
                        Height = 0.6,
                        FontSize = _tickFontSize,
                        Foreground = Brushes.Black,
                        Transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90),
                            new Point3D(length - x, length + _tickDistance, 0))  // 旋转中心设为文字位置
                    });
                }
                group.Children.Add(new TextVisual3D
                {
                    Text = colName,
                    Position = new Point3D(length / 2, length + _axesDistance, 0),
                    Height = 1.0,
                    FontSize = _axesFontSize,
                    Foreground = Brushes.Black,
                    Transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90),
                            new Point3D(length / 2, length + _axesDistance, 0))  // 旋转中心设为文字位置
                });
            }

            if (source.Count > 1)
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
                var (colName, (min, max)) = source[1];
                for (int i = 0; i <= divider; i++)
                {
                    double y = i * step;
                    double value = min + i * (max - min) / divider;
                    group.Children.Add(new LinesVisual3D
                    {
                        Color = Colors.Black,
                        Points = new Point3DCollection {
                    new Point3D(length,length-y,0),
                    new Point3D(length-0.1,length-y,0)
                }
                    });
                    string format = IsInteger(value) ? "F0" : "F2";
                    // === Y轴标签 (竖排) ===
                    var text = new TextVisual3D
                    {
                        Text = value.ToString(format),
                        Position = new Point3D(length + _tickDistance, length - y, 0),
                        Height = 0.6,
                        FontSize = _tickFontSize,
                        Foreground = Brushes.Black,
                        Transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90),
                            new Point3D(length + _tickDistance, length - y, 0))  // 旋转中心设为文字位置
                    };
                    group.Children.Add(text);
                }
                group.Children.Add(new TextVisual3D
                {
                    Text = colName,
                    FontSize = _axesFontSize,
                    Position = new Point3D(length + _axesDistance * 2, length / 2, 0),
                    Height = 1,
                    Foreground = Brushes.Black,
                    Transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90),
                            new Point3D(length + _axesDistance * 1.5, length / 2, 0))  // 旋转中心设为文字位置
                });
            }

            if (source.Count > 2)
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
                var (colName, (min, max)) = source[2];
                var transform = new TranslateTransform3D() { OffsetX = _tickDistance * 0.5 };
                for (int i = 1; i <= divider; i++)
                {
                    double z = i * step;
                    double value = min + i * (max - min) / divider;
                    group.Children.Add(new LinesVisual3D
                    {
                        Color = Colors.Black,
                        Points = new Point3DCollection {
                    new Point3D(length,0,z),
                    new Point3D(length-0.1,0,z)
                }
                    });
                    string format = IsInteger(value) ? "F0" : "F2";
                    // === Z轴标签 (竖直) ===
                    group.Children.Add(new TextVisual3D
                    {
                        Text = value.ToString(format),
                        Position = new Point3D(length + _tickDistance, 0, z),
                        Height = 0.6,
                        FontSize = _tickFontSize,
                        Foreground = Brushes.Black,
                        Transform = transform
                    });
                }
                group.Children.Add(new TextVisual3D
                {
                    Text = colName,
                    Position = new Point3D(length + _axesDistance * 1.5, 0, length / 2),
                    Height = 1,
                    FontSize = _axesFontSize,
                    Foreground = Brushes.Black,
                    Transform = new RotateTransform3D(
                    new AxisAngleRotation3D(new Vector3D(0, 1, 0), 90),
                    new Point3D(length + _axesDistance * 1.5, 0, length / 2))
                });
            }
            return group;
        }

        public static SphereVisual3D CreateSphere(Point3D position, double radius = 0.3)
        {
            return new SphereVisual3D
            {
                Center = position,
                Radius = radius,
                Material = MaterialHelper.CreateMaterial(Colors.Red)
            };
        }

        public static ModelVisual3D CreateXYGrid(double size, double step, double z = 0, double thickness = 1)
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

        public static ModelVisual3D CreateXZGrid(double size, double step, double y = 0, double thickness = 1)
        {
            var grid = new LinesVisual3D { Thickness = thickness, Color = Colors.LightGray };
            var pts = new Point3DCollection();
            for (double x = 0; x <= size + 1e-6; x += step) { pts.Add(new Point3D(x, y, 0)); pts.Add(new Point3D(x, y, size)); }
            for (double z = 0; z <= size + 1e-6; z += step) { pts.Add(new Point3D(0, y, z)); pts.Add(new Point3D(size, y, z)); }
            grid.Points = pts; var g = new ModelVisual3D(); g.Children.Add(grid); return g;
        }

        public static ModelVisual3D CreateYZGrid(double size, double step, double x = 0, double thickness = 1)
        {
            var grid = new LinesVisual3D { Thickness = thickness, Color = Colors.LightGray };
            var pts = new Point3DCollection();
            for (double y = 0; y <= size + 1e-6; y += step) { pts.Add(new Point3D(x, y, 0)); pts.Add(new Point3D(x, y, size)); }
            for (double z = 0; z <= size + 1e-6; z += step) { pts.Add(new Point3D(x, 0, z)); pts.Add(new Point3D(x, size, z)); }
            grid.Points = pts; var g = new ModelVisual3D(); g.Children.Add(grid); return g;
        }

        /// <summary>
        /// 归一化值
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double Normalization(double min, double max, double value)
        {
            return (value - min) / (max - min);
        }
    }

    public static class ResponseSurfaceSceneBuilder
    {
        public static ModelVisual3D BuildSmoothSurfaceScene(
            Vector<double> parameters,
            List<string> terms,
           Dictionary<string, (double, double)> dictionary,
            int splitsPerAxis = 120,            // 越大越圆滑
            double length = 10.0,
            int divider = 5
           ) // 可选：固定 x3..xk 的物理值（长度=k-2）
        {
            if (parameters == null || terms == null || parameters.Count != terms.Count)
                throw new ArgumentException("parameters 与 terms 长度不一致。");
            int k = dictionary.Keys.Count;
            if (k < 2) throw new ArgumentException("至少需要两个因子。");

            // 解析多项式项
            var exps = ParseTermsToExponents(k, terms);

            // 轴信息（用于你给的 CreateScientificAxes）
            List<(string colName, (double min, double max) limit)> source = [];
            foreach (KeyValuePair<string, (double, double)> item in dictionary)
            {
                source.Add((item.Key, item.Value));
            }

            // 1) 坐标轴
            var scene = new ModelVisual3D();
            scene.Children.Add(SurfaceBuilder.CreateScientificAxes(source, length, divider));

            var ranges = DenseMatrix.OfArray(new double[k, 2]);
            for (int i = 0; i < source.Count; i++)
            {
                ranges[i, 0] = source[i].limit.min;
                ranges[i, 1] = source[i].limit.max;
            }
            // 2) 采样并生成圆滑曲面
            var model = BuildSurfaceModel(parameters, exps, ranges, splitsPerAxis, length);
            scene.Children.Add(model);
            return scene;
        }

        private static ModelVisual3D BuildSurfaceModel(
            Vector<double> parameters,
            List<int[]> exps,
            Matrix<double> ranges,
            int splitsPerAxis,
            double length)
        {
            int k = ranges.RowCount;

            // 固定其他维度为区间中点
            var xPhys = Vector<double>.Build.Dense(k, 0.0);
            for (int i = 2; i < k; i++)
                xPhys[i] = 0.5 * (ranges[i, 0] + ranges[i, 1]);

            // 先估计 z 范围（用于归一化与着色）W
            (double zmin1, double zmax1) = EstimateZRange(parameters, exps, ranges);
            double zmin = ranges[2, 0] <= zmin1 ? ranges[2, 0] : zmin1;
            double zmax = ranges[2, 1] >= zmin1 ? ranges[2, 1] : zmax1;
            double dz = Math.Max(zmax - zmin, 1e-12);

            // XY 归一化比例；Z 使用估计范围
            double sx = length / Math.Max(ranges[0, 1] - ranges[0, 0], 1e-12);
            double sy = length / Math.Max(ranges[1, 1] - ranges[1, 0], 1e-12);
            double sz = length / Math.Max(ranges[2, 1] - ranges[2, 0], 1e-12);

            int nx = Math.Max(2, splitsPerAxis);
            int ny = Math.Max(2, splitsPerAxis);

            var positions = new Point3DCollection();
            var texcoords = new PointCollection();
            var normals = new Vector3DCollection();
            var indices = new Int32Collection();

            for (int i = 0; i < nx - 1; i++)
            {
                double tx0 = (double)i / (nx - 1);
                double tx1 = (double)(i + 1) / (nx - 1);
                double x0 = ranges[0, 0] + (ranges[0, 1] - ranges[0, 0]) * tx0;
                double x1 = ranges[0, 0] + (ranges[0, 1] - ranges[0, 0]) * tx1;

                for (int j = 0; j < ny - 1; j++)
                {
                    double ty0 = (double)j / (ny - 1);
                    double ty1 = (double)(j + 1) / (ny - 1);
                    double y0 = ranges[1, 0] + (ranges[1, 1] - ranges[1, 0]) * ty0;
                    double y1 = ranges[1, 0] + (ranges[1, 1] - ranges[1, 0]) * ty1;

                    // 物理空间取值
                    xPhys[0] = x0; xPhys[1] = y0;
                    double z00p = EvaluatePolynomial(xPhys, parameters, exps);
                    xPhys[0] = x1; xPhys[1] = y0;
                    double z10p = EvaluatePolynomial(xPhys, parameters, exps);
                    xPhys[0] = x1; xPhys[1] = y1;
                    double z11p = EvaluatePolynomial(xPhys, parameters, exps);
                    xPhys[0] = x0; xPhys[1] = y1;
                    double z01p = EvaluatePolynomial(xPhys, parameters, exps);

                    // 颜色系数
                    double t00 = (z00p - zmin) / dz;
                    double t10 = (z10p - zmin) / dz;
                    double t11 = (z11p - zmin) / dz;
                    double t01 = (z01p - zmin) / dz;

                    // 坐标轴方向修正（与你的坐标轴一致：数值↑ → 世界 X/Y ↓）
                    var p00 = new Point3D(
                        length - (x0 - ranges[0, 0]) * sx,
                        length - (y0 - ranges[1, 0]) * sy,
                        (z00p - zmin) * sz);

                    var p10 = new Point3D(
                        length - (x1 - ranges[0, 0]) * sx,
                        length - (y0 - ranges[1, 0]) * sy,
                        (z10p - zmin) * sz);

                    var p11 = new Point3D(
                        length - (x1 - ranges[0, 0]) * sx,
                        length - (y1 - ranges[1, 0]) * sy,
                        (z11p - zmin) * sz);

                    var p01 = new Point3D(
                        length - (x0 - ranges[0, 0]) * sx,
                        length - (y1 - ranges[1, 0]) * sy,
                        (z01p - zmin) * sz);

                    // 三角形1
                    AddTriangle(positions, texcoords, normals, indices,
                        p00, p10, p11,
                        new Point(0, 1 - t00), new Point(1, 1 - t10), new Point(1, 1 - t11));

                    // 三角形2
                    AddTriangle(positions, texcoords, normals, indices,
                        p00, p11, p01,
                        new Point(0, 1 - t00), new Point(1, 1 - t11), new Point(0, 1 - t01));
                }
            }

            var mesh = new MeshGeometry3D
            {
                Positions = positions,
                TextureCoordinates = texcoords,
                Normals = normals,
                TriangleIndices = indices
            };

            var gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Colors.Red,    0.0),
            new GradientStop(Colors.Yellow, 0.25),
            new GradientStop(Colors.Lime,   0.5),
            new GradientStop(Colors.Cyan,   0.75),
            new GradientStop(Colors.Blue,   1.0)
        }
            };
            gradient.Freeze();

            var mat = new MaterialGroup();
            mat.Children.Add(new DiffuseMaterial(gradient));
            mat.Children.Add(new SpecularMaterial(new SolidColorBrush(Color.FromRgb(220, 220, 220)), 30.0));

            var gm = new GeometryModel3D { Geometry = mesh, Material = mat, BackMaterial = mat };
            return new ModelVisual3D { Content = gm };
        }

        private static void AddTriangle(
            Point3DCollection positions,
            PointCollection texcoords,
            Vector3DCollection normals,
            Int32Collection triIndices,
            Point3D a, Point3D b, Point3D c,
            Point ta, Point tb, Point tc)
        {
            int baseIndex = positions.Count;
            positions.Add(a); positions.Add(b); positions.Add(c);
            texcoords.Add(ta); texcoords.Add(tb); texcoords.Add(tc);
            var n = ComputeNormal(a, b, c);
            normals.Add(n); normals.Add(n); normals.Add(n);
            triIndices.Add(baseIndex); triIndices.Add(baseIndex + 1); triIndices.Add(baseIndex + 2);
        }

        private static Vector3D ComputeNormal(Point3D a, Point3D b, Point3D c)
        {
            var v1 = b - a;
            var v2 = c - a;
            var n = Vector3D.CrossProduct(v1, v2);
            if (n.Length < 1e-12) return new Vector3D(0, 0, 1);
            n.Normalize();
            return n;
        }

        // z 估计范围：对 x1/x2 做粗略采样，其它维取中点或固定值
        public static (double min, double max) EstimateZRange(Vector<double> parameters, List<int[]> exps, Matrix<double> ranges)
        {
            int k = ranges.RowCount;
            var x = Vector<double>.Build.Dense(k, 0.0);
            for (int i = 2; i < k; i++) x[i] = 0.5 * (ranges[i, 0] + ranges[i, 1]);

            double zmin = double.PositiveInfinity, zmax = double.NegativeInfinity;
            const int G = 40;
            for (int i = 0; i <= G; i++)
            {
                x[0] = ranges[0, 0] + (ranges[0, 1] - ranges[0, 0]) * i / G;
                for (int j = 0; j <= G; j++)
                {
                    x[1] = ranges[1, 0] + (ranges[1, 1] - ranges[1, 0]) * j / G;
                    double z = EvaluatePolynomial(x, parameters, exps);
                    if (z < zmin) zmin = z;
                    if (z > zmax) zmax = z;
                }
            }
            if (double.IsInfinity(zmin) || double.IsInfinity(zmax)) { zmin = 0; zmax = 1; }
            if (Math.Abs(zmax - zmin) < 1e-12) zmax = zmin + 1.0;
            return (zmin, zmax);
        }

        // 多项式求值：terms 已解析为 exps
        private static double EvaluatePolynomial(Vector<double> x, Vector<double> parameters, List<int[]> exps)
        {
            double result = 0.0;
            for (int i = 0; i < exps.Count; i++)
            {
                double term = 1.0;
                var a = exps[i];
                for (int j = 0; j < a.Length; j++)
                {
                    int p = a[j];
                    if (p == 0) continue;
                    term *= Math.Pow(x[j], p);
                }
                result += parameters[i] * term;
            }
            return result;
        }

        // 解析 "1", "x1", "x2^2", "x1x2" 等
        public static List<int[]> ParseTermsToExponents(int k, List<string> terms)
        {
            var list = new List<int[]>(terms.Count);
            foreach (var raw in terms)
            {
                var s = raw.Replace(" ", "");
                var a = new int[k];
                if (s == "1" || string.IsNullOrEmpty(s)) { list.Add(a); continue; }

                for (int i = 0; i < s.Length;)
                {
                    if (s[i] != 'x' && s[i] != 'X') { i++; continue; }
                    i++;
                    int idx = 0;
                    while (i < s.Length && char.IsDigit(s[i])) { idx = idx * 10 + (s[i] - '0'); i++; }
                    if (idx <= 0 || idx > k) throw new ArgumentException($"项 {raw} 索引越界 x{idx}");

                    int pow = 1;
                    if (i < s.Length && s[i] == '^')
                    {
                        i++;
                        int p = 0;
                        if (i >= s.Length || !char.IsDigit(s[i])) throw new ArgumentException($"项 {raw} 幂缺失");
                        while (i < s.Length && char.IsDigit(s[i])) { p = p * 10 + (s[i] - '0'); i++; }
                        pow = Math.Max(1, p);
                    }
                    a[idx - 1] += pow;
                }
                list.Add(a);
            }
            return list;
        }
    }
}

