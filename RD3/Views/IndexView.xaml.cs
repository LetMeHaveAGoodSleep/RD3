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
using System.Windows.Controls.Primitives;
using ScottPlot.Plottables;
using ImTools;

namespace RD3.Views
{
    /// <summary>
    /// IndexView.xaml 的交互逻辑
    /// </summary>
    public partial class IndexView : UserControl
    {
        Crosshair MyCrosshair;
        ScottPlot.Plottables.Marker MyHighlightMarker;
        ScottPlot.Plottables.Text MyHighlightText;

        public IndexView(IContainerProvider containerProvider)
        {
            ILanguage language = containerProvider.Resolve<ILanguage>();

            InitializeComponent();

            tgbDetails.Checked += ToggleButton_Checked;
            tgbTrends.Checked += ToggleButton_Checked;
            tgbOverview.Checked += ToggleButton_Checked;

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

            var sig1 = plt.Add.Scatter(dates, ScottPlot.Generate.Sin(51, mult: 0.01));
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

            plt.Axes.SetLimitsY(-10, 100, yAxis4);

            MyCrosshair = wpfPlot.Plot.Add.Crosshair(0, 0);
            MyCrosshair.IsVisible = false;
            MyCrosshair.MarkerShape = MarkerShape.OpenCircle;
            MyCrosshair.MarkerSize = 15;

            MyHighlightMarker = wpfPlot.Plot.Add.Marker(0, 0);
            MyHighlightMarker.Shape = MarkerShape.OpenCircle;
            MyHighlightMarker.Size = 17;
            MyHighlightMarker.LineWidth = 2;

            // Create a text label to place near the highlighted value
            MyHighlightText = wpfPlot.Plot.Add.Text("", 0, 0);
            MyHighlightText.LabelAlignment = Alignment.LowerLeft;
            MyHighlightText.LabelBold = true;
            MyHighlightText.OffsetX = 7;
            MyHighlightText.OffsetY = -15;

            wpfPlot.MouseMove += (sender, e) =>
            {
                WpfPlot wpfPlot = sender as WpfPlot;
                var position = e.GetPosition(wpfPlot);
                Pixel mousePixel = new(position.X, position.Y);
                Coordinates mouseLocation = wpfPlot.Plot.GetCoordinates(mousePixel);
                var curveList = wpfPlot.Plot.GetPlottables();
                var scatterPlots = curveList.OfType<Scatter>().ToList();
                Dictionary<int, DataPoint> nearestPoints = new();
                for (int i = 0; i < scatterPlots.Count; i++)
                {
                    DataPoint nearestPoint = scatterPlots[i].Data.GetNearest(mouseLocation, wpfPlot.Plot.LastRender);
                    nearestPoints.Add(i, nearestPoint);
                }

                bool pointSelected = false;
                int scatterIndex = -1;
                double smallestDistance = double.MaxValue;
                for (int i = 0; i < nearestPoints.Count; i++)
                {
                    if (nearestPoints[i].IsReal)
                    {
                        // calculate the distance of the point to the mouse
                        double distance = nearestPoints[i].Coordinates.Distance(mouseLocation);
                        if (distance < smallestDistance)
                        {
                            // store the index
                            scatterIndex = i;
                            pointSelected = true;
                            // update the smallest distance
                            smallestDistance = distance;
                        }
                    }
                }

                if (pointSelected)
                {
                    ScottPlot.Plottables.Scatter scatter = scatterPlots[scatterIndex];
                    DataPoint point = nearestPoints[scatterIndex];

                    MyCrosshair.IsVisible = true;
                    MyCrosshair.Position = point.Coordinates;
                    MyCrosshair.LineColor = scatter.MarkerStyle.FillColor;

                    MyHighlightMarker.IsVisible = true;
                    MyHighlightMarker.Location = point.Coordinates;
                    MyHighlightMarker.MarkerStyle.LineColor = scatter.MarkerStyle.FillColor;

                    MyHighlightText.IsVisible = true;
                    MyHighlightText.Location = point.Coordinates;
                    DateTime dt = DateTime.FromOADate(point.X);
                    string label = $"{dt:HH:mm:ss}\r\n{dt:yyyy/MM/dd}";
                    MyHighlightText.LabelText = $"X:{dt:HH:mm:ss} {dt:yyyy/MM/dd}\r\nY:{point.Y:0.###}";
                    MyHighlightText.LabelFontColor = scatter.MarkerStyle.FillColor;

                    wpfPlot.Refresh();

                }

                // hide the crosshair, marker and text when no point is selected
                if (!pointSelected && MyCrosshair.IsVisible)
                {
                    MyHighlightText.IsVisible = MyHighlightMarker.IsVisible = MyCrosshair.IsVisible = false;
                    wpfPlot.Refresh();
                }
            };

            wpfPlot.MouseDoubleClick += (sender, e) =>
            {
                WpfPlot wpfPlot = sender as WpfPlot;
                var position = e.GetPosition(wpfPlot);
                var yAxes = wpfPlot.Plot.GetAxis(new Pixel(position.X, position.Y));
                if (yAxes.Edge != Edge.Left && yAxes.Edge != Edge.Right)
                {
                    return;
                }

                // 弹出对话框，让用户输入新的 Y 轴范围
                //var inputY = Prompt.ShowDialog($"Enter Y-axis {yAxis.Id} range (min,max):", $"Edit Y-axis {yAxis.Id}");
                //if (inputY != null)
                //{
                //    var range = inputY.Split(',');
                //    if (range.Length == 2)
                //    {
                //        double yMin = double.Parse(range[0]);
                //        double yMax = double.Parse(range[1]);
                //        yAxes.Min = yMin;
                //        yAxes.Max = yMax;
                //        wpfPlot.Refresh();
                //    }
                //}
            };
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if (toggleButton?.Name == "tgbDetails")
            {
                tgbOverview.IsChecked = tgbTrends.IsChecked = !toggleButton.IsChecked;
                tabDetails.IsSelected = (bool)toggleButton.IsChecked;
                tabTrends.IsSelected = tabOverview.IsSelected = !(bool)toggleButton.IsChecked;
            }
            else if (toggleButton?.Name == "tgbTrends")
            {
                tgbOverview.IsChecked = tgbDetails.IsChecked = !toggleButton.IsChecked;
                tabTrends.IsSelected = (bool)toggleButton.IsChecked;
                tabDetails.IsSelected = tabOverview.IsSelected = !(bool)toggleButton.IsChecked;
            }
            else if (toggleButton?.Name == "tgbOverview")
            {
                tgbDetails.IsChecked = tgbTrends.IsChecked = !toggleButton.IsChecked;
                tabOverview.IsSelected = (bool)toggleButton.IsChecked;
                tabTrends.IsSelected = tabDetails.IsSelected = !(bool)toggleButton.IsChecked;
            }
        }

        private void TabControl_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            var control = tabControl.Template.FindName("headerPanel", tabControl) as TabPanel;
            control.Visibility = Visibility.Collapsed;
        }

        private void ToggleButtonGraphParam_CheckChanged(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            var axis = wpfPlot?.Plot?.Axes?.GetAxes().FirstOrDefault(t => t.Label.Text == toggleButton.Tag?.ToString());
            if (axis == null)
            {
                return;
            }
            axis.IsVisible = (bool)toggleButton.IsChecked;
            wpfPlot.Refresh();
        }

        private void ToggleButtonGraphSetting_CheckChanged(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;

            if ((bool)toggleButton.IsChecked)
            {
                wpfPlot?.Plot?.ShowGrid();
                wpfPlot?.Refresh();
            }
            else
            {
                wpfPlot?.Plot?.HideGrid();
                wpfPlot?.Refresh();
            }
        }
    }
}
