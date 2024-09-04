using ScottPlot.WPF;
using ScottPlot;
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
using ScottPlot.AxisPanels;
using Prism.Ioc;

namespace RD3.Views
{
    /// <summary>
    /// IndexView.xaml 的交互逻辑
    /// </summary>
    public partial class IndexView : UserControl
    {
        public IndexView(IContainerProvider containerProvider)
        {
            InitializeComponent();

            ILanguage language=containerProvider.Resolve<ILanguage>();

            // 创建 ScottPlot 的 Plot 对象
            var plt = wpfPlot.Plot;
            plt.Axes.Left.Label.Text = "Agit";
            plt.Legend.IsVisible = true;
            plt.ShowLegend(Edge.Top);


            DateTime[] dates = Generate.ConsecutiveDays(100);
            double[] ys = Generate.RandomWalk(100);
            var scatter = plt.Add.Scatter(dates, ys);
            scatter.LegendText = "1";
            plt.Axes.DateTimeTicksBottom();
            plt.Axes.Bottom.Label.Text = "Time";

            plt.RenderManager.RenderStarting += (s, e) =>
            {
                Tick[] ticks = plt.Axes.Bottom.TickGenerator.Ticks;
                for (int i = 0; i < ticks.Length; i++)
                {
                    DateTime dt = DateTime.FromOADate(ticks[i].Position);
                    string label = $"{dt:HH:mm:ss}\r\n{dt:yyyy/MM/dd}";
                    ticks[i] = new Tick(ticks[i].Position, label);
                }
            };

            // create a second axis and add it to the plot
            var yAxis2 = plt.Axes.AddLeftAxis();
            yAxis2.LabelText = "Air";

            var yAxis3 = plt.Axes.AddRightAxis();
            yAxis3.LabelText = "DO";

            var yAxis4 = plt.Axes.AddRightAxis();
            yAxis4.LabelText = "Temp";

            var yAxis5 = plt.Axes.AddRightAxis();
            yAxis5.LabelText = "pH";

            var sig1 = plt.Add.ScatterLine(dates,ScottPlot.Generate.Sin(51, mult: 0.01));
            sig1.LegendText = "2";
            sig1.Axes.XAxis = plt.Axes.Bottom; // standard X axis
            sig1.Axes.YAxis = plt.Axes.Left; // standard Y axis

            // add a new plottable and tell it to use the custom Y axis
            var sig2 = plt.Add.Signal(ScottPlot.Generate.Cos(51, mult: 100));
            sig2.Axes.XAxis = plt.Axes.Bottom; // standard X axis
            sig2.Axes.YAxis = yAxis2; // custom Y axis


            // add a new plottable and tell it to use the custom Y axis
            var sig3 = plt.Add.Signal(ScottPlot.Generate.Cos(51, mult: 10));
            sig3.Axes.XAxis = plt.Axes.Bottom; // standard X axis
            sig3.Axes.YAxis = yAxis3; // custom Y axis

            var sig4 = plt.Add.Signal(ScottPlot.Generate.Cos(51, mult: 50));
            sig4.Axes.XAxis = plt.Axes.Bottom; // standard X axis
            sig4.Axes.YAxis = yAxis4; // custom Y axis

            var sig5 = plt.Add.Signal(ScottPlot.Generate.Cos(51, mult: 8));
            sig5.Axes.XAxis = plt.Axes.Bottom; // standard X axis
            sig5.Axes.YAxis = yAxis5; // custom Y axis

            // 刷新图表以显示所有轴和曲线
            wpfPlot.Refresh();
        }
    }
}
