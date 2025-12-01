using Fpi.Communication.Ports.AsynPorts;
using HandyControl.Data;
using ImTools;
using Newtonsoft.Json;
using OpenTK.Windowing.Common;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Common.Events;
using RD3.Extensions;
using RD3.Shared;
using RD3.ViewModels;
using ScottPlot;
using ScottPlot.AxisPanels;
using ScottPlot.DataSources;
using ScottPlot.Plottables;
using ScottPlot.TickGenerators;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using XZ.SQLite;

namespace RD3.Views
{
    /// <summary>
    /// BatchView.xaml 的交互逻辑
    /// </summary>
    public partial class CompareBatchView : UserControl
    {
        private bool _plotNeedRefresh = true;

        DateTimeXAxis bottomAxis;
        TopAxis topAxis;

        private readonly IEventAggregator aggregator;

        List<RD3Batch> batches = null;
        Dictionary<string, List<double>> datas = new Dictionary<string, List<double>>();

        public CompareBatchView(IContainerProvider containerProvider, IEventAggregator aggregator)
        {
            InitializeComponent();
            this.aggregator = aggregator;

            InitAxis();
            CreatePlotMark();
            wpfPlot.Plot.ShowLegend(Edge.Top);

            bottomAxis = wpfPlot.Plot.Axes.DateTimeTicksBottom();
            topAxis = wpfPlot.Plot.Axes.AddTopAxis();

            DateTimeAutomatic tickGen = (DateTimeAutomatic)bottomAxis.TickGenerator;
            tickGen.LabelFormatter = NewMainView.CustomFormatter;//时间格式坐标格式
                                                                 //展示数据

            wpfPlot.PreviewMouseMove += (sender, e) =>
            {
                try
                {
                    WpfPlot wpfPlot = (WpfPlot)sender;
                    System.Windows.Point mousePosition = e.GetPosition((UIElement)sender);
                    Pixel mousePixel = new(mousePosition.X * wpfPlot.DisplayScale, mousePosition.Y * wpfPlot.DisplayScale);
                    Dictionary<string, DataPoint> nearestPoints = new();
                    foreach (var item in wpfPlot.Plot.PlottableList)
                    {
                        if (item.IsVisible && item is SignalXY signal)
                        {
                            Coordinates mouseLocation = wpfPlot.Plot.GetCoordinates(mousePixel, signal.Axes.XAxis, signal.Axes.YAxis);
                            DataPoint nearestPoint = signal.Data.GetNearestX(mouseLocation, wpfPlot.Plot.RenderManager.LastRender);
                            nearestPoints.Add(signal.LegendText, nearestPoint);
                        }
                    }
                    bool pointSelected = false;
                    string signLabel = "";

                    StringBuilder sb = new StringBuilder();
                    string line = string.Empty;
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
                        var signalXY = wpfPlot.Plot.PlottableList.Find(c => c is SignalXY signal && signal.IsVisible && signal.LegendText == signLabel);
                        if (signalXY != null)
                        {
                            SignalXY signal = (signalXY as SignalXY);
                            DataPoint point = nearestPoints[signLabel];
                            Coordinates coordinates = new Coordinates(signal.Axes.XAxis.Min, signal.Axes.YAxis.Min);


                            MyCrosshair.IsVisible = true;
                            MyCrosshair.Position = point.Coordinates;
                            MyCrosshair.LineColor = ScottPlot.Colors.Black;
                            MyCrosshair.Axes.YAxis = signal.Axes.YAxis;
                            MyCrosshair.Axes.XAxis = signal.Axes.XAxis;

                            //MyHighlightMarker.IsVisible = true;
                            //MyHighlightMarker.Location = point.Coordinates;
                            //MyHighlightMarker.MarkerStyle.LineColor = signal.MarkerStyle.FillColor;
                            //MyHighlightMarker.Axes.YAxis = signal.Axes.YAxis;
                            //MyHighlightMarker.Axes.XAxis = signal.Axes.XAxis;

                            MyHighlightText.IsVisible = true;
                            MyHighlightText.Location = coordinates;
                            MyHighlightText.LabelText = sb.ToString();

                            MyHighlightText.LabelFontColor = ScottPlot.Colors.Black;
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
                catch (Exception ex) { }
            };

            Thread thread = new Thread(() =>
            {
                while (AppSession.SelectedBatches == null || AppSession.SelectedBatches.Count == 0)
                {
                    Thread.Sleep(100);
                }
                RefreshPlot();

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    CmbTimeInterval.SelectionChanged -= CmbTimeInterval_SelectionChanged;
                    CmbTimeInterval.SelectionChanged += CmbTimeInterval_SelectionChanged;
                }));
            });
            thread.Priority = ThreadPriority.BelowNormal;
            thread.IsBackground = true;
            thread.Start();
        }

        private void RefreshPlot()
        {
            batches = AppSession.SelectedBatches;
            Dictionary<string, dataValue> deviceNodeDic = RD3SQLHelper.pNodeDic.Values.ToArray()[0];
            string timeInterval = ((int)AppSession.BatchTimeInterval).ToString();
            LogHelper.Debug($"时间间隔是：{timeInterval}");
            foreach (RD3Batch batch in batches)
            {
                try
                {
                    string sql = $"WITH FirstRecord AS (SELECT dateTime AS firstDateTime FROM {RD3SQLHelper.RTParamTable} WHERE deviceID = '{batch.devieceID}' AND batchID = '{batch.ID}' ORDER BY dateTime ASC LIMIT 1)" +
            $",TimeIntervals AS (SELECT r.*, CAST((strftime('%s', r.dateTime) - strftime('%s', fr.firstDateTime)) / {timeInterval} AS INTEGER) AS minuteInterval FROM {RD3SQLHelper.RTParamTable} r  CROSS JOIN FirstRecord fr  WHERE deviceID = '{batch.devieceID}' AND batchID = '{batch.ID}')" +
            $",SampledData AS (SELECT *,ROW_NUMBER() OVER (PARTITION BY minuteInterval ORDER BY dateTime ASC) AS rn FROM TimeIntervals) SELECT * FROM SampledData WHERE rn = 1 ORDER BY dateTime ASC;";
                    DataTable dt = SQLiteHelper.GetDatasToDataTable(sql);
                    if (dt.Rows.Count < 1) continue;

                    List<double> xList = new List<double>();

                    foreach (var item in deviceNodeDic.Values)
                    {
                        datas[item.name] = new List<double>();
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        DateTime.TryParse(row["dateTime"].ToString(), out var cdt);
                        xList.Add(cdt.ToOADate());
                        foreach (var item in deviceNodeDic.Values)
                        {
                            if (!dt.Columns.Contains(item.fieldName))
                                continue;
                            double v = 0;
                            object obj = row[item.fieldName];
                            if (obj != DBNull.Value)
                            {
                                double.TryParse(obj.ToString(), out v);
                            }
                            datas[item.name].Add(v);
                        }
                    }
                    double[] xValus = xList.ToArray();
                    Dispatcher.BeginInvoke(() =>
                    {
                        wpfPlot.Plot.PlottableList.RemoveAll(t => t is SignalXY);
                        int count = 1;
                        foreach (var item in datas)
                        {
                            try
                            {
                                var fieldName = string.Empty;
                                foreach (var item1 in deviceNodeDic.Values.Where(t => t.name.ToUpper() == item.Key.ToUpper()))
                                {
                                    fieldName = item1.fieldName;
                                }
                                ScottPlot.Color color = ScottPlot.Color.FromHex("#FFFFFF");
                                var node1 = ParameterNodeManager.GetInstance().ParameterNodes.FindFirst(t => t.fieldName == fieldName);
                                if (node1 != null)
                                {
                                    var array = node1.colorStr.Split(',');
                                    color = ScottPlot.Color.FromARGB(System.Drawing.Color.FromArgb(byte.Parse(array[0]), byte.Parse(array[1]), byte.Parse(array[2])).ToArgb());
                                }
                                var signal = wpfPlot.Plot.Add.SignalXY(xValus, item.Value.ToArray(), color);
                                signal.LegendText = $"{batch.ID}_{item.Key}";
                                signal.IsVisible = count < 6 ? true : false;
                                signal.MarkerSize = (float)node1?.pointSize;
                                signal.LineWidth = (float)node1?.lineWidth;
                                foreach (var axis in wpfPlot.Plot.Axes.GetAxes().ToList())
                                {
                                    if (axis is YAxisBase yaxis)
                                    {
                                        if (yaxis.LabelText.Contains(item.Key))
                                        {
                                            signal.Axes.YAxis = yaxis;
                                            break;
                                        }
                                    }
                                }
                                signal.Axes.XAxis = wpfPlot.Plot.Axes.Bottom;
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error($"批次查看界面：错误信息{ex.Message}");
                            }
                            finally 
                            {
                                count++;
                            }

                        }
                    });
                }
                catch (Exception ex)
                {
                }
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                wpfPlot.Plot.Title(string.Join(",", batches.Select(t => $"{t.devieceID}_{t.ID}")));
                wpfPlot.Plot.Axes.AutoScale();
                wpfPlot.Refresh();
            }));
        }

        Crosshair MyCrosshair;
        Marker MyHighlightMarker;
        Text MyHighlightText;
        private void CreatePlotMark()
        {
            MyCrosshair = wpfPlot.Plot.Add.Crosshair(0, 0);
            MyCrosshair.IsVisible = false;
            //MyCrosshair.MarkerShape = MarkerShape.None;
            MyCrosshair.MarkerSize = 15;
            MyCrosshair.HorizontalLine.IsVisible = false;

            MyHighlightMarker = wpfPlot.Plot.Add.Marker(0, 0);
            MyHighlightMarker.Shape = MarkerShape.None;
            MyHighlightMarker.Size = 17;
            MyHighlightMarker.LineWidth = 2;
            MyHighlightMarker.IsVisible = false;

            MyHighlightText = wpfPlot.Plot.Add.Text("", 0, 0);
            MyHighlightText.LabelAlignment = Alignment.LowerLeft;
            MyHighlightText.LabelBold = true;
            MyHighlightText.OffsetX = 7;
            MyHighlightText.OffsetY = 0;
            MyHighlightText.IsVisible = false;
        }


        /// <summary>
        /// 初始化控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitAxis()
        {
            Dictionary<string, dataValue> deviceNodeDic = RD3SQLHelper.pNodeDic.Values.ToArray()[0];
            //设置Y轴
            int count = 1;
            foreach (var node in deviceNodeDic.Values)//因子集合
            {
                switch (node.axisType)
                {
                    case 0://左轴
                        wpfPlot.Plot.Axes.Left.IsVisible = true;
                        wpfPlot.Plot.Axes.Left.Label.Text = string.IsNullOrEmpty(node.unit) ? node.name.ToString() : $"{node.name.ToString()}({node.unit})";
                        wpfPlot.Plot.Axes.Left.Max = node.maxValue;
                        wpfPlot.Plot.Axes.Left.Min = node.minValue;
                        (wpfPlot.Plot.Axes.Left as YAxisBase).LabelFontName = ScottPlot.Fonts.Detect((wpfPlot.Plot.Axes.Left as YAxisBase).LabelText);

                        break;
                    case 1://左新轴
                        YAxisBase newLeft = wpfPlot.Plot.Axes.AddLeftAxis();
                        SetAxisInfo(newLeft, node.name, node.unit, node.maxValue, node.minValue);
                        newLeft.IsVisible = count > 5 ? false : true;
                        break;
                    case 2://右轴
                        wpfPlot.Plot.Axes.Right.IsVisible = true;
                        wpfPlot.Plot.Axes.Right.Label.Text = string.IsNullOrEmpty(node.unit) ? node.name.ToString() : $"{node.name.ToString()}({node.unit})"; ;
                        wpfPlot.Plot.Axes.Right.Max = node.maxValue;
                        wpfPlot.Plot.Axes.Right.Min = node.minValue;
                        break;
                    case 3://右新轴
                        YAxisBase newRight = wpfPlot.Plot.Axes.AddRightAxis();
                        SetAxisInfo(newRight, node.name, node.unit, node.maxValue, node.minValue);
                        newRight.IsVisible = count > 5 ? false : true;
                        break;
                }

                CheckBox cb = new CheckBox() { Name = node.name, Content = node.name, Width = 200, Height = 30, Tag = node, IsChecked = count > 5 ? false : true };
                cb.Checked += Cb_Checked;
                cb.Unchecked += Cb_Checked;
                flagPanel.Children.Add(cb);

                count++;
            }


        }

        /// <summary>
        /// 曲线显示-不显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cb_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkBox = sender as System.Windows.Controls.CheckBox;
            bool isChecked = checkBox.IsChecked.HasValue && checkBox.IsChecked.Value;
            dataValue data = checkBox.Tag as dataValue;
            //方成 轴的显隐不跟checkbox挂钩
            var axes = wpfPlot?.Plot?.Axes?.GetAxes().ToList().Find(c => c.Label.Text.Contains(data.name));
            if (axes != null)
            {
                axes.IsVisible = isChecked;
            }

            var scatters = wpfPlot.Plot.PlottableList.ToList().FindAll(c => c is SignalXY && (c as SignalXY).LegendText.Contains(data.name));
            if (scatters != null)
            {
                foreach (var scatter in scatters)
                    scatter.IsVisible = isChecked;
            }
            wpfPlot.Refresh();
        }

        /// <summary>
        /// 初始化坐标轴
        /// </summary>
        /// <param name="label"></param>
        /// <param name="type">0-左，1-右</param>
        private void SetAxisInfo(YAxisBase yAxis, string label, string unit, double max, double min)
        {
            yAxis.LabelText = string.IsNullOrEmpty(unit) ? label.ToString() : $"{label.ToString()}({unit})";
            yAxis.EmptyLabelPadding = new PixelPadding(100);
            yAxis.LabelFontName = ScottPlot.Fonts.Detect(yAxis.LabelText);
            wpfPlot.Plot.Axes.SetLimitsY(min, max, yAxis);
        }

        /// <summary>
        /// 右键菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FlagPanel_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ContextMenu context = new ContextMenu();
            MenuItem item = new MenuItem() { Width = 100 };
            item.Header = "添加离线数据";
            item.Click += new RoutedEventHandler(inputOfflineData_Click);
            context.Items.Add(item);
            context.IsOpen = true;
        }

        /// <summary>
        /// 导入离线数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inputOfflineData_Click(object sender, RoutedEventArgs e)
        {
            ((CompareBatchViewModel)this.DataContext).AddOffLineDatasCommand.Execute();
        }

        private void CmbTimeInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Thread thread = new Thread(() =>
            {
                RefreshPlot();
            });
            thread.IsBackground = true;
            thread.Start();
        }
    }

    /// <summary>
    /// 数据
    /// </summary>
    public class lineDatas
    {
        public double[] xList;
        public Dictionary<string, double[]> yListDic;
    }
}
