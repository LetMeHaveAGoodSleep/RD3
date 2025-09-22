using ImTools;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using RD3.Common;
using RD3.Shared;
using RD3.ViewModels;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WPF;
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
using static SkiaSharp.HarfBuzz.SKShaper;

namespace RD3.Views
{
    /// <summary>
    /// DOEAnalyse2DView.xaml 的交互逻辑
    /// </summary>
    public partial class DOEAnalyse2DView : UserControl
    {
        Vector<double> vectorCoefficient = null;
        List<string> terms = null;
        public DOEAnalyse2DView()
        {
            InitializeComponent();
            wpfPlot.Plot.XLabel("实验次数");
            wpfPlot.Plot.YLabel("响应值");
            wpfPlot.Plot.ShowLegend(Alignment.UpperLeft, ScottPlot.Orientation.Horizontal);
            wpfPlot.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericFixedInterval(1);
            wpfPlot.Plot.Font.Automatic();


            AnalysisSolution.GetInstance().EventPublisher.BeforeDOEResultAnalyse -= EventPublisher_BeforeDOEResultAnalyse;
            AnalysisSolution.GetInstance().EventPublisher.BeforeDOEResultAnalyse += EventPublisher_BeforeDOEResultAnalyse;
            //wpfPlot.Plot.FigureBackground.Color = ScottPlot.Colors.Navy;
            //wpfPlot.Plot.DataBackground.Color = ScottPlot.Colors.Navy.Darken(0.1);
            //wpfPlot.Plot.Grid.MajorLineColor = ScottPlot.Colors.Navy.Lighten(0.1);

            //// some items have helper methods to configure multiple properties at once
            //wpfPlot.Plot.Axes.Color(ScottPlot.Colors.Navy.Lighten(0.8));
            this.Unloaded += DOEAnalyse2DView_Unloaded;
        }

        private void DOEAnalyse2DView_Unloaded(object sender, RoutedEventArgs e)
        {
            AnalysisSolution.GetInstance().EventPublisher.BeforeDOEResultAnalyse -= EventPublisher_BeforeDOEResultAnalyse;
        }

        private void EventPublisher_BeforeDOEResultAnalyse(object sender, System.Data.DataTable e)
        {
            try 
            {
                List<double> xs = [];
                List<double> expDataList = [];
                Matrix<double> factorMatrix = DenseMatrix.OfArray(new double[e.Rows.Count, e.Columns.Count - 3]);
                List<double> responseList = [];
                List<double> polyList = [];
                for (int i = 0; i < e.Rows.Count; i++)
                {
                    xs.Add(i);
                    expDataList.Add(Convert.ToDouble(e.Rows[i]["Response"]));

                    for (int j = 1; j < e.Columns.Count - 2; j++)
                    {
                        factorMatrix[i, j - 1] = Convert.ToDouble(e.Rows[i][j]);
                    }
                }

                var expmarkers = wpfPlot.Plot.Add.Markers(xs, expDataList, MarkerShape.HashTag, 10, ScottPlot.Colors.Green);
                expmarkers.LegendText = "实验数据";

                // 创建响应向量
                var responseVector = Vector<double>.Build.Dense(expDataList.ToArray());

                // 调用拟合方法
                var (coefficients, formula,mse,terms) = QuadraticSurfaceLM.FitQuadraticSurfaceLM(factorMatrix, responseVector, 2);
                vectorCoefficient = coefficients;
                this.terms = terms;
                (this.DataContext as DOEAnalyse2DViewModel).Terms = terms;
                (this.DataContext as DOEAnalyse2DViewModel).FitCoefficient = coefficients;
                wpfPlot.Plot.Title(formula, 12);

                // 计算预测值
                Vector<double> predictedValues = Vector<double>.Build.Dense(factorMatrix.RowCount);
                for (int i = 0; i < factorMatrix.RowCount; i++)
                {
                    predictedValues[i] = QuadraticSurfaceLM.Predict(Vector<double>.Build.Dense(factorMatrix.Row(i).ToArray()), coefficients, terms);
                }
                var polyScatter = wpfPlot.Plot.Add.Scatter(xs.ToArray(), predictedValues.ToArray(), ScottPlot.Colors.Red);
                polyScatter.LegendText = "多项式拟合";
                polyScatter.MarkerShape = MarkerShape.OpenCircle;
                polyScatter.MarkerSize = 2;

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

                //基准线
                double[] zeroArray = new double[xs.Count];
                var zeroScatter = wpfPlot.Plot.Add.Scatter(xs.ToArray(), zeroArray, new ScottPlot.Color("#00A087"));
                zeroScatter.LegendText = "基准线";
                zeroScatter.MarkerShape = MarkerShape.OpenCircle;
                zeroScatter.MarkerSize = 2;
                zeroScatter.LinePattern = LinePattern.DenselyDashed;

                //预测值
                var vector = Vector<double>.Build.Dense((this.DataContext as DOEAnalyse2DViewModel).Factor2DParams.Select(t => t.CurrentValue).ToArray());
                var prediction = QuadraticSurfaceLM.Predict(vector, vectorCoefficient, terms);
                wpfPlot.Plot.Remove(typeof(HorizontalLine));
                var horizontalLine = wpfPlot.Plot.Add.HorizontalLine(prediction, 1, ScottPlot.Colors.Purple);
                horizontalLine.LegendText = "预测值";

                horizontalLine.IsDraggable = false;
                horizontalLine.EnableAutoscale = false;
                (this.DataContext as DOEAnalyse2DViewModel).Prediction = prediction;
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Warning(ex.Message, "温馨提示");
            }
        }

        private void NumericUpDown_ValueChanged(object sender, HandyControl.Data.FunctionEventArgs<double> e)
        {
            if (vectorCoefficient == null) return;
            var vector = Vector<double>.Build.Dense((this.DataContext as DOEAnalyse2DViewModel).Factor2DParams.Select(t => t.CurrentValue).ToArray());
            var prediction = QuadraticSurfaceLM.Predict(vector, vectorCoefficient, terms); ;
            wpfPlot.Plot.Remove(typeof(HorizontalLine));
            var horizontalLine = wpfPlot.Plot.Add.HorizontalLine(prediction, 1, ScottPlot.Colors.Purple);
            horizontalLine.LegendText = "预测值";
            horizontalLine.IsDraggable = false;
            horizontalLine.EnableAutoscale = false;
            (this.DataContext as DOEAnalyse2DViewModel).Prediction = prediction;
            wpfPlot.Refresh();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (vectorCoefficient == null) return;
            var vector = Vector<double>.Build.Dense((this.DataContext as DOEAnalyse2DViewModel).Factor2DParams.Select(t => t.CurrentValue).ToArray());
            var prediction = QuadraticSurfaceLM.Predict(vector, vectorCoefficient, terms);
            wpfPlot.Plot.Remove(typeof(HorizontalLine));
            var horizontalLine = wpfPlot.Plot.Add.HorizontalLine(prediction, 1, ScottPlot.Colors.Purple);
            horizontalLine.LegendText = "预测值";
            horizontalLine.IsDraggable = false;
            horizontalLine.EnableAutoscale = false;
            (this.DataContext as DOEAnalyse2DViewModel).Prediction = prediction;
            wpfPlot.Refresh();
        }

        private void btnPredictMax_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in (this.DataContext as DOEAnalyse2DViewModel).Factor2DParams)
            {
                item.CurrentValue = item.Maximum;
            }
        }

        private void btnPredictMini_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in (this.DataContext as DOEAnalyse2DViewModel).Factor2DParams)
            {
                item.CurrentValue = item.Minimum;
            }
        }
    }
}
