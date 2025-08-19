using HandyControl.Controls;
using Microsoft.VisualBasic;
using RD3.Common;
using RD3.Shared;
using RD3.ViewModels;
using ScottPlot.AxisPanels;
using ScottPlot.TickGenerators;
using ScottPlot.WPF;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using XZ.SQLite;
using ScottPlot.Plottables;
using Prism.Events;
using ImTools;
using OpenTK.Compute.OpenCL;

namespace RD3.Views
{
    /// <summary>
    /// PadMainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PadMainView : System.Windows.Window
    {
        Dictionary<string, (Crosshair, ScottPlot.Plottables.Marker, ScottPlot.Plottables.Text)> dicMarker = new Dictionary<string, (Crosshair, Marker, Text)>();

        Dictionary<string, (List<LeftAxis>, List<RightAxis>)> dicAxis = new Dictionary<string, (List<LeftAxis>, List<RightAxis>)>();

        private Dictionary<string, double[]> dicScale = new Dictionary<string, double[]>();

        Dictionary<string, Dictionary<string, ScottPlot.Color>> dicColor = new Dictionary<string, Dictionary<string, ScottPlot.Color>>();

        public PadMainView()
        {
            InitializeComponent();
            this.Closing += PadMainView_Closing;

            tabMenu.SelectionChanged += TabControl_SelectionChanged;

            FormattedTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += (s, e) => 
            {
                FormattedTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            timer.Start();

            RefershPumpMFC();

            //调试模式下，就不需要让窗口不可移动
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                this.WindowStyle = WindowStyle.None;
                SourceInitialized += (s, e) =>
                {
                    IntPtr hwnd = new WindowInteropHelper(this).Handle;
                    HwndSource.FromHwnd(hwnd)?.AddHook(WndProc);
                    ForceFullScreen();
                };
                btnDOSimulate.Visibility = btnpHSimulate.Visibility = Visibility.Collapsed;
            }

            foreach (KeyValuePair<string, string> item in GraphConfig.GetAllValue())
            {
                double min = Convert.ToDouble(VarConfig.GetValue(item.Key + "ScaleMin")?.ToString());
                double max = Convert.ToDouble(VarConfig.GetValue(item.Key + "ScaleMax")?.ToString());
                dicScale.Add(item.Key, [min, max]);
            }

            wpfPlot3.RenderTransform = new TransformGroup
            {
                Children = new TransformCollection {
        new ScaleTransform(), // 缩放
        new TranslateTransform() // 平移
    }
            };

            //wpfPlot3.ManipulationInertiaStarting += (s, e) => 
            //{
            //    WpfPlot wpfPlot = s as WpfPlot;
            //};

            wpfPlot3.ManipulationDelta += (s, e) => 
            {
                var deltaManipulation = e.DeltaManipulation;
                WpfPlot wpfPlot = s as WpfPlot;
                //wpfPlot.Plot.Axes.AutoScale();

                //FrameworkElement element = (FrameworkElement)e.Source;

                //var transform = (TransformGroup)element.RenderTransform;

                //Matrix matrix = (transform.Children[0] as MatrixTransform).Matrix;

                //var deltaManipulation = e.DeltaManipulation;

                //Point center = new Point(element.ActualWidth / 2, element.ActualHeight / 2);
                //center = matrix.Transform(center);

                //matrix.ScaleAt(deltaManipulation.Scale.X, deltaManipulation.Scale.Y, center.X, center.Y);

                //matrix.RotateAt(e.DeltaManipulation.Rotation, center.X, center.Y);

                //matrix.Translate(e.DeltaManipulation.Translation.X, e.DeltaManipulation.Translation.Y);

                //((MatrixTransform)element.RenderTransform).Matrix = matrix;
            };

            DateTime startDateTime = DateTime.Now;
            foreach (WpfPlot item in GridMain.FindVisualChildren<WpfPlot>())
            {
                var legendPanel = item.Plot.ShowLegend(Edge.Top);
                legendPanel.Padding = new PixelPadding(20);

                var bottomAxis = item.Plot.Axes.DateTimeTicksBottom();
                item.Plot.Axes.SetLimitsX(startDateTime.ToOADate(), startDateTime.AddMinutes(1).ToOADate(), item.Plot.Axes.Bottom);
                ((DateTimeAutomatic)bottomAxis.TickGenerator).LabelFormatter = new((DateTime dt) =>
                {
                    bool isMidnight = dt is { Hour: 0, Minute: 0, Second: 0 };
                    return isMidnight
                        ? DateOnly.FromDateTime(dt).ToString()
                        : TimeOnly.FromDateTime(dt).ToString("HH\\:mm\\:ss");
                });

                CreatePlotMark(item);

                item.TouchMove += Item_TouchMove;

                item.Plot.Add.Palette = new ScottPlot.Palettes.Normal();

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
                Dictionary<string, string> reverseDict = GraphConfig.GetAllValue();
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
                        var yAxis = dicAxis[item.Name].Item2[result - judge];
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
        }

        private void Item_TouchMove(object sender, TouchEventArgs e)
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
            TouchPoint mousePosition = e.GetTouchPoint((UIElement)sender);
            // 考虑 DPI 缩放
            Pixel mousePixel = new(mousePosition.Position.X, mousePosition.Position.Y);
            Dictionary<string, DataPoint> nearestPoints = new();
            foreach (var item in wpfPlot.Plot.PlottableList)
            {
                if (item.IsVisible && item is SignalXY signal)
                {
                    Coordinates mouseLocation = wpfPlot.Plot.GetCoordinates(mousePixel, wpfPlot.Plot.Axes.Bottom, signal.Axes.YAxis);
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
                    MyHighlightText.Location = new Coordinates(0, 0);
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
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_NCHITTEST = 0x0084;
            if (msg == WM_NCHITTEST)
            {
                // 始终返回非可拖动区域标识
                handled = true;
                return (IntPtr)1; // HTNOWHERE
            }
            return IntPtr.Zero;
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private void ForceFullScreen()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            // 移除WS_THICKFRAME和WS_MAXIMIZEBOX样式
            SetWindowLong(hwnd, -16, 0x10000000);
            // 禁用DWM动画
            int disableAnim = 1;
            DwmSetWindowAttribute(hwnd, 3, ref disableAnim, sizeof(int));
        }

        /// <summary>
        /// 图表轴等参数配置
        /// </summary>
        private void GraphSetting()
        {
            List<string> source = new List<string>() { "Plot3", "Plot4" };
            List<WrapPanel> wrapPanels = new List<WrapPanel>() { ChkGraphList3 };
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
              HandyControl.Controls.MessageBox.Show(string.Format("最多显示{0}条曲线", sum),"温馨提示");
                return;
            }
            if (array.Length < 2) return;
            Dictionary<string, string> dictionary = CustomGraphConfig.GetValue(array[0]) as Dictionary<string, string>;
            dictionary[array[1]] = ((bool)checkBox.IsChecked).ToString();
            CustomGraphConfig.SetValue(array[0], dictionary);
            foreach (var item in GridMain.FindVisualChildren<WpfPlot>().Where(t => t.Name.Contains(array[0])))
            {
                Dictionary<string, string> reverseDict = GraphConfig.Dictionary;
                if ((bool)checkBox.IsChecked)
                {
                    var leftCount = item.Plot.Axes.GetAxes().Where(t => t.GetType() == typeof(LeftAxis) && !t.IsVisible).Count();
                    var rightCount = item.Plot.Axes.GetAxes().Where(t => t.GetType() == typeof(RightAxis) && !t.IsVisible).Count();
                    var allCount = item.Plot.Axes.GetAxes().Where(t => t.GetType() == typeof(LeftAxis) || t.GetType() == typeof(RightAxis)).Count();
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
                                yAxis.Label.Text.Replace(reverseDict[array[1]] + "(" + GraphUnitConfig.GetValue(reverseDict[array[1]]) + ")", string.Empty);
                            }
                            else
                            {
                                yAxis.Label.Text.Replace(reverseDict[array[1]], string.Empty);
                            }
                        }
                    }
                }
                item?.Refresh();
            }
        }

        private void EventPublisher_DataProcessed(object sender, DeviceExperimentHistoryData graphDataSource)
        {
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    wpfPlot3.Plot.Remove<ScottPlot.Plottables.SignalXY>();
                    wpfPlot3.Refresh();
                    Dictionary<string, string> dictionary = CustomGraphConfig.GetValue(wpfPlot3.Name.Substring(wpfPlot3.Name.Length - 5, 5)) as Dictionary<string, string>;
                    foreach (var item2 in graphDataSource.ExperimentHistoryDatas)
                    {
                        Dictionary<string, string> reverseDict = GraphConfig.Dictionary;
                        var count = item2.Xs.Count <= item2.Ys.Count ? item2.Xs.Count : item2.Ys.Count;
                        dictionary.TryGetValue(item2.ParamerterName, out var str);
                        bool flag = Convert.ToBoolean(str);
                        if (!flag)
                        {
                            continue;
                        }
                        Dictionary<string, string> reverseDict1 = PumpMFCConfig.GetValue(AnalysisSolution.GetInstance().ReactorCol[0].Name);
                        for (int i = 0; i < count; i++)
                        {
                            try
                            {
                                string legendText = graphDataSource.DeviceName + "_" + item2.ParamerterName + "_" + (i + 1).ToString();

                                dicColor.TryGetValue(wpfPlot3.Name, out var keyValuePairs);
                                ScottPlot.Color color = ScottPlot.Color.FromHex("#000000");
                                string propertyName = PameterMapperConfig.GetValue(item2.ParamerterName)?.ToString();
                                var node1 = ParameterNodeManager.GetInstance().ParameterNodes.FindFirst(t => t.fieldName.ToUpper() == propertyName.ToUpper());
                                if (node1 != null)
                                {
                                    var array = node1.colorStr.Split(',');
                                    color = ScottPlot.Color.FromARGB(System.Drawing.Color.FromArgb(byte.Parse(array[0]), byte.Parse(array[1]), byte.Parse(array[2])).ToArgb());
                                }
                                var signal = wpfPlot3.Plot.Add.SignalXY(item2.Xs[i].ToArray(), item2.Ys[i].ToArray(), color);
                                signal.LegendText = legendText;
                                signal.IsVisible = flag;
                                signal.MarkerSize = (float)node1?.pointSize;
                                signal.LineWidth = (float)node1?.lineWidth;
                                if (flag)
                                {
                                    var yAxis = wpfPlot3.Plot.Axes.GetAxes().FindFirst(t => t.Label.Text.Contains(reverseDict[item2.ParamerterName]?.ToString()));
                                    if (yAxis != null)
                                    {
                                        signal.Axes.YAxis = (IYAxis)yAxis;
                                    }
                                    else
                                    {
                                        signal.Axes.YAxis = wpfPlot3.Plot.Axes.Left;
                                    }
                                }
                                else
                                {
                                    signal.Axes.YAxis = wpfPlot3.Plot.Axes.Left;
                                }
                                signal.Axes.XAxis = wpfPlot3.Plot.Axes.Bottom;
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                    wpfPlot3?.Refresh();
                }
                catch (Exception ex)
                {
                    LogHelper.Error("图表出错：" + ex.Message + "\r\n" + ex.StackTrace);
                }
            });
        }

        private void RefershPumpMFC()
        {
            pump1.ResumePumpSetting(1);
            pump2.ResumePumpSetting(2);
            pump3.ResumePumpSetting(3);
            pump4.ResumePumpSetting(4);
            pump5.ResumePumpSetting(5);
            pump6.ResumePumpSetting(6);

            mfc1.ResumeMFCSetting(1);
            mfc2.ResumeMFCSetting(2);
            mfc3.ResumeMFCSetting(3);
            mfc4.ResumeMFCSetting(4);
        }

        private void PadMainView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Windows.Controls.TabControl tabControl = sender as System.Windows.Controls.TabControl;
            if (e.AddedItems.Count < 1) return;

            if (e.AddedItems[0] is System.Windows.Controls.TabItem selectedItem)
            {
                if (selectedItem.Name == nameof(tabBatch))
                {
                    (batchView.DataContext as NewBatchViewModel).ReloadDataCommand.Execute();
                }

                if (selectedItem.Tag != null)
                {
                    var lastItem = e.RemovedItems[0] as System.Windows.Controls.TabItem;
                    lastItem.IsSelected = true;
                }

                if (selectedItem.Name == tabCurve.Name)
                {
                    AnalysisSolution.GetInstance().EventPublisher.DataProcessed -= EventPublisher_DataProcessed;
                    AnalysisSolution.GetInstance().EventPublisher.DataProcessed += EventPublisher_DataProcessed;
                }
                else
                {
                    AnalysisSolution.GetInstance().EventPublisher.DataProcessed -= EventPublisher_DataProcessed;
                }
                if (selectedItem.Name == tabAlarm.Name)
                {
                    ((AlarmRecordViewModel)alarmRecordView.DataContext).ReloadDataCommand.Execute();
                }
                else
                {
                    ((AlarmRecordViewModel)alarmRecordView.DataContext).CancelLoadCommand.Execute();
                }
                if (selectedItem.Name == tabAudit.Name)
                {
                    ((PadAuditViewModel)auditView.DataContext).ReloadDataCommand.Execute();
                }
                else
                {
                    ((PadAuditViewModel)auditView.DataContext).CancelLoadCommand.Execute();
                }
            }
        }
    }
}
