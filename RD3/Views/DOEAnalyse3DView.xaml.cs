using HelixToolkit.Wpf;
using ImTools;
using MaterialDesignThemes.Wpf;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using RD3.Common;
using RD3.Controls;
using RD3.Shared;
using RD3.ViewModels;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using SkiaSharp;
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
using static FParsec.ErrorMessage;

namespace RD3.Views
{
    /// <summary>
    /// DOEAnalyse2DView.xaml 的交互逻辑
    /// </summary>
    public partial class DOEAnalyse3DView : UserControl
    {
        Vector<double> vectorCoefficient = null;
        List<string> termExpressions = [];
        public DOEAnalyse3DView()
        {
            InitializeComponent();
            wpfPlot.Plot.XLabel("实验次数");
            wpfPlot.Plot.YLabel("响应值");
            wpfPlot.Plot.ShowLegend(Alignment.UpperLeft, ScottPlot.Orientation.Horizontal);
            wpfPlot.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericFixedInterval(1);
            wpfPlot.Plot.Font.Automatic();


            AnalysisSolution.GetInstance().EventPublisher.BeforeDOEResultAnalyse -= EventPublisher_BeforeDOEResultAnalyse;
            AnalysisSolution.GetInstance().EventPublisher.BeforeDOEResultAnalyse += EventPublisher_BeforeDOEResultAnalyse;
            this.Unloaded += DOEAnalyse3DView_Unloaded;
        }

        private void DOEAnalyse3DView_Unloaded(object sender, RoutedEventArgs e)
        {
            AnalysisSolution.GetInstance().EventPublisher.BeforeDOEResultAnalyse -= EventPublisher_BeforeDOEResultAnalyse;
        }

        private void EventPublisher_BeforeDOEResultAnalyse(object sender, System.Data.DataTable e)
        {
            try 
            {
                int order = (this.DataContext as DOEAnalyse3DViewModel).Order;
                List<double> xs = [];
                List<double> expDataList = [];
                Matrix<double> factorMatrix = DenseMatrix.OfArray(new double[e.Rows.Count, e.Columns.Count - 1]);
                List<double> responseList = [];
                List<double> polyList = [];
                for (int i = 0; i < e.Rows.Count; i++)
                {
                    xs.Add(i);
                    expDataList.Add(Convert.ToDouble(e.Rows[i]["Response"]));

                    for (int j = 0; j < e.Columns.Count - 1; j++)
                    {
                        factorMatrix[i, j] = Convert.ToDouble(e.Rows[i][j]);
                    }
                }

                // 创建响应向量
                var responseVector = Vector<double>.Build.Dense(expDataList.ToArray());
                //拟合
                var result = QuadraticSurfaceLM.FitQuadraticSurfaceLM(factorMatrix, responseVector);
                vectorCoefficient = result.Parameters;
                termExpressions = result.Terms;
                wpfPlot.Plot.Title(result.Formula.Replace("y", "z").Replace("x1","x").Replace("x2","y"), 12);
                (this.DataContext as DOEAnalyse3DViewModel).MSE = result.mse;
                (this.DataContext as DOEAnalyse3DViewModel).Terms = result.Terms;
                (this.DataContext as DOEAnalyse3DViewModel).FitCoefficient = result.Parameters;

                // 计算预测值
                Vector<double> predictedValues = Vector<double>.Build.Dense(factorMatrix.RowCount);
                for (int i = 0; i < factorMatrix.RowCount; i++)
                {
                    predictedValues[i] = QuadraticSurfaceLM.Predict(Vector<double>.Build.Dense(factorMatrix.Row(i).ToArray()), vectorCoefficient, termExpressions);
                }
                var polyScatter = wpfPlot.Plot.Add.Scatter(xs.ToArray(), predictedValues.ToArray(), ScottPlot.Colors.Red);
                polyScatter.LegendText = "多项式拟合";
                polyScatter.MarkerShape = MarkerShape.OpenCircle;
                polyScatter.MarkerSize = 2;


                var expmarkers = wpfPlot.Plot.Add.Markers(xs, expDataList, MarkerShape.HashTag, 10, ScottPlot.Colors.Green);
                expmarkers.LegendText = "实验数据";

                // 计算残差
                Vector<double> actualValues = Vector<double>.Build.Dense(expDataList.ToArray());
                Vector<double> residuals = predictedValues - actualValues;
                var residualMarkers = wpfPlot.Plot.Add.Markers(xs.ToArray(), residuals.ToArray(), MarkerShape.OpenCircle, 10, new ScottPlot.Color("#3C5488"));
                residualMarkers.LegendText = "残差";
                for (int i = 0; i < residuals.Count; i++)
                {
                    if (Math.Abs(residuals[i]) <= 0.3)
                    {
                        continue;
                    }
                    var line = wpfPlot.Plot.Add.Line(xs[i], 0, xs[i], residuals[i]);
                    line.Color = new ScottPlot.Color("#3C5488");
                }

                //预测值
                var vector = Vector<double>.Build.DenseOfArray(new double[] { xFactorNumericUpDown.Value, yFactorNumericUpDown.Value });
                var prediction = QuadraticSurfaceLM.Predict(vector, vectorCoefficient, termExpressions);
                (this.DataContext as DOEAnalyse3DViewModel).Prediction = prediction;

                DataTable datatable = e.Copy();
                //for (int i = 0; i < datatable.Rows.Count; i++)
                //{
                //    datatable.Rows[i][2] = predictedValues[i];
                //}

                Dictionary<string, (double, double)> dictionary = new Dictionary<string, (double, double)>();
                for (int i = 0; i < 3; i++)
                {
                    var temp = datatable.AsEnumerable().Select(r => Convert.ToDouble(r[i]));
                    dictionary.Add(datatable.Columns[i].ColumnName, (temp.Min(), temp.Max()));
                }

                helixSurfaceView.Generate3DView(vectorCoefficient, termExpressions, datatable, dictionary);

            }
            catch (Exception ex) 
            {
                HandyControl.Controls.MessageBox.Warning(ex.Message, "温馨提示");
            }

        }

        private void NumericUpDown_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (vectorCoefficient == null) return;
            int order = Convert.ToInt32(txtOrder.Text);
            var vector = Vector<double>.Build.DenseOfArray(new double[] { xFactorNumericUpDown.Value, yFactorNumericUpDown.Value });
            var prediction = QuadraticSurfaceLM.Predict(vector, vectorCoefficient, termExpressions);
            (this.DataContext as DOEAnalyse3DViewModel).Prediction = prediction;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (vectorCoefficient == null) return;
            int order = Convert.ToInt32(txtOrder.Text);
            var vector = Vector<double>.Build.DenseOfArray(new double[] { xFactorNumericUpDown.Value, yFactorNumericUpDown.Value });
            var prediction = QuadraticSurfaceLM.Predict(vector, vectorCoefficient, termExpressions);
            (this.DataContext as DOEAnalyse3DViewModel).Prediction = prediction;
        }
    }
}
