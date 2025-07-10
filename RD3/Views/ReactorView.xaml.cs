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
using System.Timers;
using RD3.ViewModels;
using SkiaSharp;
using RD3.Common.Events;
using Prism.Events;
using RD3.Extensions;
using Prism.Services.Dialogs;
using RD3.Shared;
using System.Diagnostics;
using ScottPlot.TickGenerators;
using System.CodeDom;
using RD3.Common;
using ScottPlot.Colormaps;
using System.Reflection;
using Fpi.Communication.Protocols;
using System.Collections;
using MathNet.Symbolics;
using System.Drawing.Imaging;
using Fpi.Communication.Buses;
using System.ComponentModel;
using System.Threading;
using XZ.SQLite;
using System.Xml.Linq;

namespace RD3.Views
{
    /// <summary>
    /// IndexView.xaml 的交互逻辑
    /// </summary>
    public partial class ReactorView : UserControl
    {
        private static readonly object _lock1 = new object();

        Dictionary<string, (Crosshair, ScottPlot.Plottables.Marker, ScottPlot.Plottables.Text)> dicMarker = new Dictionary<string, (Crosshair, Marker, Text)>();

        Dictionary<string, Dictionary<string, ScottPlot.Color>> dicColor = new Dictionary<string, Dictionary<string, ScottPlot.Color>>();

        Dictionary<string, (List<LeftAxis>, List<RightAxis>)> dicAxis = new Dictionary<string, (List<LeftAxis>, List<RightAxis>)>();

        private readonly IEventAggregator _aggregator;

        private Dictionary<string, double[]> dicScale = new Dictionary<string, double[]>();

        private bool isAutoScale = false;

        public ReactorView(IContainerProvider containerProvider, IEventAggregator aggregator, IDialogHostService dialogHostService)
        {
            InitializeComponent();

            _aggregator = aggregator;

            foreach (KeyValuePair<string, string> item in GraphConfig.GetAllValue())
            {
                double min = Convert.ToDouble(VarConfig.GetValue(item.Key + "ScaleMin")?.ToString());
                double max = Convert.ToDouble(VarConfig.GetValue(item.Key + "ScaleMax")?.ToString());
                dicScale.Add(item.Key, [min, max]);
            }

            //double[] dateDoubles = new double[60];
            DateTime startDateTime = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
            double startDouble = startDateTime.ToOADate(); // days since 1900
            //double deltaDouble = 1.0 / 60.0; // an hour is 1/24 of a day
            //for (int i = 0; i < 60; i++)
            //{
            //    dateDoubles[i] = startDouble + i * deltaDouble;
            //}

            foreach (WpfPlot item in GridMain.FindVisualChildren<WpfPlot>())
            {
                var legendPanel = item.Plot.ShowLegend(Edge.Top);
                legendPanel.Padding = new PixelPadding(20);

                var bottomAxis = item.Plot.Axes.DateTimeTicksBottom();
                item.Plot.Axes.SetLimitsX(startDouble, startDouble + 1, item.Plot.Axes.Bottom);
                ((DateTimeAutomatic)bottomAxis.TickGenerator).LabelFormatter = new((DateTime dt) =>
                {
                    bool isMidnight = dt is { Hour: 0, Minute: 0, Second: 0 };
                    return isMidnight
                        ? DateOnly.FromDateTime(dt).ToString()
                        : TimeOnly.FromDateTime(dt).ToString("HH\\:mm\\:ss");
                });

                CreatePlotMark(item);

                item.MouseMove += ((sender, e) => 
                {
                    WpfPlot wpfPlot = sender as WpfPlot;
                    wpfPlot.Cursor = Cursors.Arrow;
                    if (!dicMarker.TryGetValue(wpfPlot.Name, out var result))
                    {
                        return;
                    }
                    Crosshair MyCrosshair = result.Item1;
                    ScottPlot.Plottables.Marker MyHighlightMarker = result.Item2;
                    ScottPlot.Plottables.Text MyHighlightText = result.Item3;
                    MyCrosshair.IsVisible = MyHighlightMarker.IsVisible = MyHighlightText.IsVisible = false;
                    // 获取当前控件的 DPI 因子
                    Matrix transformToDevice = PresentationSource.FromVisual(wpfPlot).CompositionTarget.TransformToDevice;
                    double dpiXFactor = transformToDevice.M11;//水平
                    double dpiYFactor = transformToDevice.M22;//垂直
                    System.Windows.Point mousePosition = e.GetPosition((UIElement)sender);
                    // 考虑 DPI 缩放
                    //mousePosition = new System.Windows.Point(mousePosition.X * dpiXFactor, mousePosition.Y * dpiYFactor);

                    //var position = e.GetPosition(wpfPlot);
                    Pixel mousePixel = new(mousePosition.X, mousePosition.Y);
                    Dictionary<string, DataPoint> nearestPoints = new();
                    foreach (var item in wpfPlot.Plot.PlottableList)
                    {
                        if (item.IsVisible && item is SignalXY signal)
                        {
                            Coordinates mouseLocation = wpfPlot.Plot.GetCoordinates(mousePixel, wpfPlot.Plot.Axes.Bottom, signal.Axes.YAxis);
                            //由选中一个点改为选中一串点
                            //DataPoint nearestPoint = signal.Data.GetNearest(mouseLocation, wpfPlot.Plot.RenderManager.LastRender);
                            DataPoint nearestPoint = signal.Data.GetNearestX(mouseLocation, wpfPlot.Plot.RenderManager.LastRender);
                            nearestPoints.Add(signal.LegendText, nearestPoint);
                        }
                    }

                    bool pointSelected = false;
                    string signLabel = "";

                    StringBuilder sb = new StringBuilder();
                    foreach (var point in nearestPoints)
                    {
                        if (point.Value.IsReal)
                        {
                            if (!pointSelected)
                            {
                                sb.AppendLine($"Time:{DateTime.FromOADate(point.Value.X).ToString("yyyy-MM-dd HH:mm:ss")}");
                                signLabel = point.Key;
                                pointSelected = true;
                            }
                            sb.AppendLine($"{point.Key}:{point.Value.Y.ToString("F2")}");
                        }
                    }

                    if (pointSelected)
                    {
                        var scatter = wpfPlot.Plot.PlottableList.Find(c => c is SignalXY signal && signal.IsVisible && signal.LegendText == signLabel);
                        if (scatter != null)
                        {
                            SignalXY signal = (scatter as SignalXY);
                            DataPoint point = nearestPoints[signLabel];

                            MyCrosshair.IsVisible = true;
                            MyCrosshair.Position = point.Coordinates;
                            MyCrosshair.LineColor = signal.MarkerStyle.FillColor;
                            MyCrosshair.Axes.YAxis = signal.Axes.YAxis;
                            MyCrosshair.Axes.XAxis = signal.Axes.XAxis;

                            MyHighlightMarker.IsVisible = true;
                            MyHighlightMarker.Location = point.Coordinates;
                            MyHighlightMarker.MarkerStyle.LineColor = signal.MarkerStyle.FillColor;
                            MyHighlightMarker.Axes.YAxis = signal.Axes.YAxis;
                            MyHighlightMarker.Axes.XAxis = signal.Axes.XAxis;

                            MyHighlightText.IsVisible = true;
                            MyHighlightText.Location = point.Coordinates;
                            //DateTime datetime = DateTime.FromOADate(point.X);
                            //MyHighlightText.LabelText = $"{datetime.ToString("yyyy-MM-dd HH:mm:ss")}\r\n {signal.LegendText}:{point.Y:0.##}";
                            MyHighlightText.LabelText = sb.ToString();

                            MyHighlightText.LabelFontColor = signal.MarkerStyle.FillColor;
                            MyHighlightText.Axes.YAxis = signal.Axes.YAxis;
                            MyHighlightText.Axes.XAxis = signal.Axes.XAxis;

                            wpfPlot.Refresh();
                        }
                    }
                    if (!pointSelected && MyCrosshair.IsVisible)
                    {
                        MyCrosshair.IsVisible = false;
                        MyHighlightMarker.IsVisible = false;
                        MyHighlightText.IsVisible = false;
                        wpfPlot.Refresh();
                    }
                });

                item.Plot.Add.Palette = new ScottPlot.Palettes.Normal();
                //dicColor.Add(item.Name, new Dictionary<string, ScottPlot.Color>());
                item.MouseLeftButtonDown += ((s, e) =>
                {
                    WpfPlot wpfPlot = s as WpfPlot;
                    // 获取当前控件的 DPI 因子
                    Matrix transformToDevice = PresentationSource.FromVisual(wpfPlot).CompositionTarget.TransformToDevice;
                    double dpiXFactor = transformToDevice.M11;//水平
                    double dpiYFactor = transformToDevice.M22;//垂直
                    System.Windows.Point mousePosition = e.GetPosition((UIElement)s);
                    // 考虑 DPI 缩放
                    mousePosition = new System.Windows.Point(mousePosition.X * dpiXFactor, mousePosition.Y * dpiYFactor);
                    Pixel mousePixel = new(mousePosition.X, mousePosition.Y);
                    var mouseCoord = item.Plot.GetCoordinates(mousePixel);

                    // 判断是否点击在坐标轴顶点附近
                    var xAxis = item.Plot.Axes.Bottom;
                    var yAxis = item.Plot.Axes.Left;
                    bool isNearYMax = Math.Abs(mouseCoord.Y - yAxis.Range.Max) < yAxis.Range.Span * 0.05;
                    bool isNearYMin = Math.Abs(mouseCoord.Y - yAxis.Range.Min) < yAxis.Range.Span * 0.05;
                    bool isNearXMin = Math.Abs(mouseCoord.X - xAxis.Range.Min) < xAxis.Range.Span * 0.05;

                    // 弹出输入框修改轴范围
                    if ((isNearYMax || isNearYMin) && isNearXMin)
                    {
                        string axisType = "Y轴";
                        string limitType = isNearYMax == true ? "上限" : "下限";
                        string input = Microsoft.VisualBasic.Interaction.InputBox($"请输入新的{axisType}{limitType}:", "修改坐标轴范围", "");

                        if (double.TryParse(input, out double newValue))
                        {
                            if (isNearYMax) yAxis.Range.Max = newValue;
                            else if (isNearYMin) yAxis.Range.Min = newValue;

                            item.Refresh(); // 刷新图表
                        }
                    }
                    else
                    {
                        WrapPanel wrapPanel = VisualTreeHelperExtensions.FindVisualChildren<WrapPanel>(item.Parent).First();
                        wrapPanel.Visibility = wrapPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                        var res = VisualTreeHelperExtensions.FindVisualChildren<ComboBox>(item.Parent);
                        if (res != null && res.Count() > 0)
                        {
                            ComboBox comboBox = res.First();
                            comboBox.Visibility = wrapPanel.Visibility;
                        }
                        e.Handled = true;
                    }
                });

                dicAxis.Add(item.Name, (new List<LeftAxis>(), new List<RightAxis>()));

                item.Plot.Axes.Left.IsVisible = item.Plot.Axes.Right.IsVisible = false;
                item.Plot.Axes.Left.Label.FontSize = item.Plot.Axes.Right.Label.FontSize = 10;
                dicAxis[item.Name].Item1.Add((LeftAxis)item.Plot.Axes.Left);
                dicAxis[item.Name].Item2.Add((RightAxis)item.Plot.Axes.Right);
                for (int i = 0; i < 2; i++)
                {
                    var leftAxis = item.Plot.Axes.AddLeftAxis();
                    leftAxis.LabelPadding = 50;
                    leftAxis.EmptyLabelPadding = new PixelPadding(50);
                    leftAxis.IsVisible = false;
                    leftAxis.LabelFontSize = 10;
                    dicAxis[item.Name].Item1.Add(leftAxis);

                    var rightAxis = item.Plot.Axes.AddRightAxis();
                    rightAxis.LabelPadding = 50;
                    rightAxis.EmptyLabelPadding = new PixelPadding(50);
                    rightAxis.LabelFontSize = 10;
                    rightAxis.IsVisible = false;
                    dicAxis[item.Name].Item2.Add(rightAxis);
                }

                string plotName = item.Name.Substring(3, item.Name.Length - 3);
                Dictionary<string, string> dictionary = CustomGraphConfig.GetValue(plotName) as Dictionary<string, string>;
                Dictionary<string, string> reverseDict = GraphConfig.Dictionary;
                var collection = dictionary.Where(t => Convert.ToBoolean(t.Value));
                int index = 0;
                int axisCount = dicAxis[item.Name].Item1.Count + dicAxis[item.Name].Item2.Count;
                int judge = (int)Math.Ceiling((double)(axisCount / 2));
                foreach (KeyValuePair<string, string> item1 in collection)
                {
                    int result = index % axisCount;
                    if (result < judge)//左边坐标轴
                    {
                        var yAxis = dicAxis[item.Name].Item1[result];
                        if (!string.IsNullOrWhiteSpace(GraphUnitConfig.GetValue(reverseDict[item1.Key])?.ToString()))
                        {
                            yAxis.LabelText += reverseDict[item1.Key] + "(" + GraphUnitConfig.GetValue(reverseDict[item1.Key]) + ")";
                        }
                        else
                        {
                            yAxis.LabelText += reverseDict[item1.Key];
                        }
                        yAxis.LabelFontName = ScottPlot.Fonts.Detect(yAxis.LabelText);
                        yAxis.IsVisible = true;
                        dicScale.TryGetValue(item1.Key, out var scale);
                        double top = yAxis.Max >= scale[1] ? yAxis.Max : scale[1];
                        double bottom = yAxis.Min <= scale[0] ? yAxis.Min : scale[0];
                        item.Plot.Axes.SetLimitsY(bottom, top, yAxis);
                    }
                    else//右边坐标轴
                    {
                        var yAxis = dicAxis[item.Name].Item2[result- judge];
                        if (!string.IsNullOrWhiteSpace(GraphUnitConfig.GetValue(reverseDict[item1.Key])?.ToString()))
                        {
                            yAxis.LabelText += reverseDict[item1.Key] + "(" + GraphUnitConfig.GetValue(reverseDict[item1.Key]) + ")";
                        }
                        else
                        {
                            yAxis.LabelText += reverseDict[item1.Key];
                        }
                        yAxis.LabelFontName = ScottPlot.Fonts.Detect(yAxis.LabelText);
                        yAxis.IsVisible = true;
                        dicScale.TryGetValue(item1.Key, out var scale);
                        double top = yAxis.Max >= scale[1] ? yAxis.Max : scale[1];
                        double bottom = yAxis.Min <= scale[0] ? yAxis.Min : scale[0];
                        item.Plot.Axes.SetLimitsY(bottom, top, yAxis);
                    }
                    index++;
                }
            }

            GraphSetting();

            AnalysisSolution.GetInstance().EventPublisher.DataProcessed -= EventPublisher_DataProcessed;
            AnalysisSolution.GetInstance().EventPublisher.DataProcessed += EventPublisher_DataProcessed;
        }

        private void EventPublisher_DataProcessed(object sender, DeviceExperimentHistoryData graphDataSource)
        {
            Dispatcher.BeginInvoke(() =>
            {
                foreach (WpfPlot item in GridMain.FindVisualChildren<WpfPlot>())
                {
                    try
                    {
                        item.Plot.Remove<ScottPlot.Plottables.SignalXY>();
                        Dictionary<string, string> dictionary = CustomGraphConfig.GetValue(item.Name.Substring(item.Name.Length - 5, 5)) as Dictionary<string, string>;
                        foreach (var item2 in graphDataSource.ExperimentHistoryDatas)
                        {
                            Dictionary<string, string> reverseDict = GraphConfig.Dictionary;
                            var count = item2.Xs.Count <= item2.Ys.Count ? item2.Xs.Count : item2.Ys.Count;
                            dictionary.TryGetValue(item2.ParamerterName, out var str);
                            bool flag = Convert.ToBoolean(str);
                            if (!flag)//modify by hdb 不显示的曲线，不往图表中添加
                                continue;
                            Dictionary<string, string> reverseDict1 = PumpMFCConfig.GetValue((CmbDevice.SelectedItem as Device)?.Name);
                            for (int i = 0; i < count; i++)
                            {
                                try
                                {
                                    string legendText = graphDataSource.DeviceName + "_" + item2.ParamerterName + "_" + (i + 1).ToString();

                                    dicColor.TryGetValue(item.Name, out var keyValuePairs);
                                    ScottPlot.Color color = ScottPlot.Color.FromHex("#FFFFFF");
                                    string propertyName = PameterMapperConfig.GetValue(item2.ParamerterName)?.ToString();
                                    var node1 = ParameterNodeManager.GetInstance().ParameterNodes.FindFirst(t => t.fieldName.ToUpper() == propertyName.ToUpper());
                                    if (node1 != null)
                                    {
                                        var array = node1.colorStr.Split(',');
                                        color = ScottPlot.Color.FromARGB(System.Drawing.Color.FromArgb(byte.Parse(array[0]), byte.Parse(array[1]), byte.Parse(array[2])).ToArgb());
                                    }
                                    var signal = item.Plot.Add.SignalXY(item2.Xs[i].ToArray(), item2.Ys[i].ToArray(), color);
                                    signal.LegendText = legendText;
                                    signal.IsVisible = flag;
                                    signal.MarkerSize = (float)node1?.pointSize;
                                    signal.LineWidth = (float)node1?.lineWidth;
                                    if (flag)
                                    {
                                        var yAxis = item.Plot.Axes.GetAxes().FindFirst(t => t.Label.Text.Contains(reverseDict[item2.ParamerterName]?.ToString()));
                                        if (yAxis != null)
                                        {
                                            signal.Axes.YAxis = (IYAxis)yAxis;
                                        }
                                        else
                                        {
                                            signal.Axes.YAxis = item.Plot.Axes.Left;
                                        }
                                    }
                                    else
                                    {
                                        signal.Axes.YAxis = item.Plot.Axes.Left;
                                    }
                                    signal.Axes.XAxis = item.Plot.Axes.Bottom;
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                        item?.Refresh();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("详情界面出错：" + ex.Message + "\r\n" + ex.StackTrace);
                    }
                }
            });
        }

        /// <summary>
        /// 图表轴等参数配置
        /// </summary>
        private void GraphSetting()
        {
            List<string> source = new List<string>() { "Plot3", "Plot4" };
            List<WrapPanel> wrapPanels = new List<WrapPanel>() {ChkGraphList3, ChkGraphList4 };
            for (int i = 0; i < source.Count; i++)
            {
                string plotName = source[i];
                Dictionary<string, string> dictionary1 = CustomGraphConfig.GetValue(plotName) as Dictionary<string, string>;
                foreach (KeyValuePair<string, string> item in GraphConfig.GetAllValue())
                {
                    try
                    {
                        CheckBox checkBox = new CheckBox();
                        checkBox.Content = item.Value;
                        checkBox.Tag = plotName + "," + item.Key;
                        checkBox.Margin = new Thickness() { Left = 5, Right = 5, Top = 5, Bottom = 5 };
                        checkBox.IsChecked = false;
                        foreach (KeyValuePair<string, string> item1 in dictionary1.Where(t => Convert.ToBoolean(t.Value)))
                        {
                            if (item.Key.ToString().ToLower() == item1.Key.ToLower())
                            {
                                checkBox.IsChecked = true;
                                break;
                            }
                        }
                        checkBox.Checked += ChkGraph_Checked;
                        checkBox.Unchecked += ChkGraph_Checked;
                        wrapPanels[i].Children.Add(checkBox);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        private void CreatePlotMark(WpfPlot wpfPlot)
        {
            Crosshair MyCrosshair;
            ScottPlot.Plottables.Marker MyHighlightMarker;
            ScottPlot.Plottables.Text MyHighlightText;
            MyCrosshair = wpfPlot.Plot.Add.Crosshair(0, 0);
            MyCrosshair.IsVisible = false;
            //MyCrosshair.MarkerShape = MarkerShape.OpenCircle;
            MyCrosshair.MarkerSize = 15;
            MyCrosshair.HorizontalLine.IsVisible = false;

            MyHighlightMarker = wpfPlot.Plot.Add.Marker(0, 0);
            MyHighlightMarker.Shape = MarkerShape.None;
            MyHighlightMarker.Size = 17;
            MyHighlightMarker.LineWidth = 2;
            MyHighlightMarker.IsVisible = false;

            // Create a text label to place near the highlighted value
            MyHighlightText = wpfPlot.Plot.Add.Text("", 0, 0);
            MyHighlightText.LabelAlignment = Alignment.LowerLeft;
            MyHighlightText.LabelBold = true;
            MyHighlightText.OffsetX = 7;
            MyHighlightText.OffsetY = 0;
            MyHighlightText.IsVisible = false;

            dicMarker.Add(wpfPlot.Name, (MyCrosshair, MyHighlightMarker, MyHighlightText));
        }

        /// <summary>
        /// 一张图最多有10个Y轴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkGraph_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            var checkboxs = checkBox.Parent.FindVisualChildren<CheckBox>();
            var array = checkBox.Tag?.ToString().Split(",");
            int sum = array[0] == "Plot1" ? 5 : 9;
            int count = 0;
            foreach (var item in checkboxs)
            {
                if (item == checkBox)
                {
                    continue;
                }
                if ((bool)item.IsChecked)
                {
                    count += 1;
                }
            }
            if (count >= sum && (bool)checkBox.IsChecked)
            {
                checkBox.IsChecked = false;
                MessageBox.Show(string.Format("最多显示{0}条曲线", sum));
                return;
            }
            if (array.Length < 2) return;
            Dictionary<string, string> dictionary = CustomGraphConfig.GetValue(array[0]) as Dictionary<string, string>;
            dictionary[array[1]] = ((bool)checkBox.IsChecked).ToString();
            CustomGraphConfig.SetValue(array[0], dictionary);
            foreach (var item in GridMain.FindVisualChildren<WpfPlot>().Where(t=>t.Name.Contains(array[0])))
            {
                Dictionary<string, string> reverseDict = GraphConfig.Dictionary;
                if ((bool)checkBox.IsChecked)
                {
                    var leftCount = item.Plot.Axes.GetAxes().Where(t => t.GetType() == typeof(LeftAxis) && !t.IsVisible).Count();
                    var rightCount = item.Plot.Axes.GetAxes().Where(t => t.GetType() == typeof(RightAxis) && !t.IsVisible).Count();
                    var allCount = item.Plot.Axes.GetAxes().Where(t => t.GetType() == typeof(LeftAxis)|| t.GetType() == typeof(RightAxis)).Count();
                    var half = (int)Math.Ceiling((double)(allCount / 2));
                    var temp = count % allCount;
                    if (leftCount > 0)
                    {
                        foreach (LeftAxis item1 in item.Plot.Axes.GetAxes().Where(t => t.GetType() == typeof(LeftAxis) && !((LeftAxis)t).IsVisible))
                        {
                            item1.IsVisible = true;
                            if (!string.IsNullOrWhiteSpace(GraphUnitConfig.GetValue(reverseDict[array[1]])?.ToString()))
                            {
                                item1.LabelText += reverseDict[array[1]] + "(" + GraphUnitConfig.GetValue(reverseDict[array[1]]) + ")";
                            }
                            else
                            {
                                item1.LabelText += reverseDict[array[1]];
                            }
                            item1.LabelFontName = ScottPlot.Fonts.Detect(reverseDict[array[1]]);
                            dicScale.TryGetValue(array[1], out var scale);
                            item.Plot.Axes.SetLimitsY(scale[0], scale[1], (IYAxis)item1);
                            break;
                        }
                    }
                    else if (rightCount > 0)
                    {
                        foreach (RightAxis item1 in item.Plot.Axes.GetAxes().Where(t => t.GetType() == typeof(RightAxis) && !((RightAxis)t).IsVisible))
                        {
                            item1.IsVisible = true;
                            if (!string.IsNullOrWhiteSpace(GraphUnitConfig.GetValue(reverseDict[array[1]])?.ToString()))
                            {
                                item1.LabelText += reverseDict[array[1]] + "(" + GraphUnitConfig.GetValue(reverseDict[array[1]]) + ")";
                            }
                            else
                            {
                                item1.LabelText += reverseDict[array[1]]; ;
                            }
                            item1.LabelFontName = ScottPlot.Fonts.Detect(reverseDict[array[1]]);
                            dicScale.TryGetValue(array[1], out var scale);
                            item.Plot.Axes.SetLimitsY(scale[0], scale[1], (IYAxis)item1);
                            break;
                        }
                    }
                    else if (temp < half)
                    {
                        int index = 0;
                        foreach (LeftAxis item1 in item.Plot.Axes.GetAxes().Where(t => t.GetType() == typeof(LeftAxis)))
                        {
                            if (index != temp)
                            {
                                index++;
                                continue;
                            }
                            item1.IsVisible = true;
                            if (!string.IsNullOrWhiteSpace(GraphUnitConfig.GetValue(reverseDict[array[1]])?.ToString()))
                            {
                                item1.LabelText += reverseDict[array[1]] + "(" + GraphUnitConfig.GetValue(reverseDict[array[1]]) + ")";
                            }
                            else
                            {
                                item1.LabelText += reverseDict[array[1]];
                            }
                            item1.LabelFontName = ScottPlot.Fonts.Detect(reverseDict[array[1]]);
                            dicScale.TryGetValue(array[1], out var scale);
                            item.Plot.Axes.SetLimitsY(scale[0], scale[1], (IYAxis)item1);
                            break;
                        }
                    }
                    else if (temp >= half)
                    {
                        int index = 0;
                        foreach (LeftAxis item1 in item.Plot.Axes.GetAxes().Where(t => t.GetType() == typeof(RightAxis)))
                        {
                            if (index != temp)
                            {
                                index++;
                                continue;
                            }
                            item1.IsVisible = true;
                            if (!string.IsNullOrWhiteSpace(GraphUnitConfig.GetValue(reverseDict[array[1]])?.ToString()))
                            {
                                item1.LabelText += reverseDict[array[1]] + "(" + GraphUnitConfig.GetValue(reverseDict[array[1]]) + ")";
                            }
                            else
                            {
                                item1.LabelText += reverseDict[array[1]];
                            }
                            item1.LabelFontName = ScottPlot.Fonts.Detect(reverseDict[array[1]]);
                            dicScale.TryGetValue(array[1], out var scale);
                            item.Plot.Axes.SetLimitsY(scale[0], scale[1], (IYAxis)item1);
                            break;
                        }
                    }
                }
                else if ((bool)checkBox.IsChecked == false)
                {
                    foreach (var item1 in item.Plot.Axes.GetAxes().Where(t => t.Label.Text.Contains(checkBox.Content?.ToString())))
                    {
                        if (item1 is IYAxis yAxis)
                        {
                            yAxis.IsVisible = false;
                            yAxis.Label.Text = string.Empty;
                            if (!string.IsNullOrWhiteSpace(GraphUnitConfig.GetValue(reverseDict[array[1]])?.ToString()))
                            {
                                yAxis.Label.Text.Replace(reverseDict[array[1]] + "(" + GraphUnitConfig.GetValue(reverseDict[array[1]]) + ")",string.Empty);
                            }
                            else
                            {
                                yAxis.Label.Text.Replace(reverseDict[array[1]],string.Empty);
                            }
                        }
                    }
                }
                item?.Refresh();
            }
        }

        private void WpfPlot_MouseMove(object sender, MouseEventArgs e)
        {
            WpfPlot wpfPlot = sender as WpfPlot;
            wpfPlot.Cursor = Cursors.Arrow;
            if (!dicMarker.TryGetValue(wpfPlot.Name, out var result))
            {
                return;
            }
            Crosshair MyCrosshair = result.Item1;
            ScottPlot.Plottables.Marker MyHighlightMarker = result.Item2;
            ScottPlot.Plottables.Text MyHighlightText = result.Item3;
            MyCrosshair.IsVisible = MyHighlightMarker.IsVisible = MyHighlightText.IsVisible = false;
            // 获取当前控件的 DPI 因子
            Matrix transformToDevice = PresentationSource.FromVisual(wpfPlot).CompositionTarget.TransformToDevice;
            double dpiXFactor = transformToDevice.M11;//水平
            double dpiYFactor = transformToDevice.M22;//垂直
            System.Windows.Point mousePosition = e.GetPosition((UIElement)sender);
            // 考虑 DPI 缩放
            //mousePosition = new System.Windows.Point(mousePosition.X * dpiXFactor, mousePosition.Y * dpiYFactor);

            //var position = e.GetPosition(wpfPlot);
            Pixel mousePixel = new(mousePosition.X, mousePosition.Y);
            var scatterPlots = wpfPlot.Plot.GetPlottables().OfType<Scatter>().Where(t => t.IsVisible).ToList();
            var yAxisList = wpfPlot.Plot.Axes.GetAxes().OfType<IYAxis>().ToList();
            bool flag = false;
            for (int i = 0; i < yAxisList.Count; i++)
            {
                if (flag) break;
                Coordinates mouseLocation = wpfPlot.Plot.GetCoordinates(mousePixel, wpfPlot.Plot.Axes.Bottom, yAxisList[i]);
                List<Scatter> scatters = scatterPlots.FindAll(t => t.Axes.YAxis == yAxisList[i]);
                for (int j = 0; j < scatters.Count; j++)
                {
                    Scatter scatter = scatters[j];
                    DataPoint nearestPoint = scatter.Data.GetNearest(mouseLocation, wpfPlot.Plot.RenderManager.LastRender);
                    if (nearestPoint.IsReal)
                    {
                        wpfPlot.Cursor = Cursors.Hand;

                        MyCrosshair.IsVisible = true;
                        MyCrosshair.Position = nearestPoint.Coordinates;
                        MyCrosshair.LineColor = scatter.MarkerStyle.FillColor;
                        MyCrosshair.Axes.YAxis = scatter.Axes.YAxis;

                        MyHighlightMarker.IsVisible = true;
                        MyHighlightMarker.Location = nearestPoint.Coordinates;
                        MyHighlightMarker.MarkerStyle.LineColor = scatter.MarkerStyle.FillColor;
                        MyHighlightMarker.Axes.YAxis = scatter.Axes.YAxis;

                        MyHighlightText.IsVisible = true;
                        MyHighlightText.Location = nearestPoint.Coordinates;
                        MyHighlightText.Axes.YAxis = scatter.Axes.YAxis;
                        DateTime dt = DateTime.FromOADate(nearestPoint.X);
                        MyHighlightText.LabelText = $"X:{dt:HH:mm:ss} {dt:yyyy/MM/dd}\r\nY:{nearestPoint.Y:0.###}";
                        MyHighlightText.LabelFontColor = scatter.MarkerStyle.FillColor;
                        wpfPlot.Refresh();
                        flag = true;
                        break;
                    }
                    else
                    {
                        wpfPlot.Cursor = Cursors.Arrow;
                        wpfPlot.Refresh();
                    }
                }
            }
        }

        private void Condensation_Click(object sender, MouseButtonEventArgs e)
        {
            System.Drawing.Image image = sender as System.Drawing.Image;
            Enum.TryParse(typeof(SwitchMode), image.Tag?.ToString(), out var switchMode);
            //InstrumentSolution.GetInstance().CommandWrapper.SetFunctionControlMode(((ReactorViewModel)DataContext).CurrentDeviceParameter.Name, ControlObject.Condensation, (SwitchMode)switchMode);
        }

        ~ReactorView()
        {
            Dispatcher.BeginInvoke(() =>
            {
                foreach (var child in Grid1.Children.OfType<IDisposable>())
                {
                    child.Dispose(); // 调用 IDisposable.Dispose()
                }
                wpfPlot3 = null;
                wpfPlot4 = null;
            });
        }
    }
}
