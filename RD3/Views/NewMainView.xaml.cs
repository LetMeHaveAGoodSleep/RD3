using RD3.Common;
using RD3.Extensions;
using Prism.Events;
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
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System.Windows.Threading;
using HandyControl.Tools;
using RD3.ViewModels;
using RD3.Common.Events;
using HandyControl.Controls;
using RD3.Shared;
using Window = System.Windows.Window;
using System.Drawing;
using Image = System.Windows.Controls.Image;
using ImTools;
using Color = System.Drawing.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using FontFamily = System.Windows.Media.FontFamily;
using System.Windows.Markup;
using static MaterialDesignThemes.Wpf.Theme;
using ScottPlot;
using ScottPlot.WPF;
using ScottPlot.AxisPanels;
using System.Windows.Controls.Primitives;
using CustomApp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ScottPlot.Plottables;
using Fpi.Communication.Manager;
using System.Collections;
using MathNet.Symbolics;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.FSharp.Core;
using System.Reflection.Emit;
using ScottPlot.TickGenerators;
using ScottPlot.DataSources;
using ScottPlot.Colormaps;
using System.Reflection;
using System.Xml.Linq;
using System.IO;
using XZ.SQLite;
using DryIoc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Data;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.Intrinsics;
using Fpi.Util.WinApiUtil.CommDataType;

namespace RD3.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class NewMainView : GlowWindow
    {
        private SubscriptionToken token = null;
        private SubscriptionToken token1 = null;
        private readonly IDialogHostService dialogHostService;
        readonly ILanguage language;
        int pointCount = 7200;//modify by hdb 缓存2小时数据
        Crosshair MyCrosshair;
        Marker MyHighlightMarker;
        Text MyHighlightText;
        IEventAggregator eventAggregator;

        Dictionary<string, bool> nodeVisibleDic = new Dictionary<string, bool>();

        public NewMainView(IEventAggregator aggregator, IDialogHostService dialogHostService, IContainerProvider containerProvider)
        {
            InitializeComponent();

            eventAggregator = aggregator;

            language = containerProvider.Resolve<ILanguage>();

            Application.Current.Resources["AppFontFamily"] = AppSession.FontFamily;

            PropertyInfo[] propertyInfos = typeof(RealTimeParam).GetProperties().Where(c => c.CanWrite && c.CanRead && (c.PropertyType == typeof(double) || c.PropertyType == typeof(float) || c.PropertyType == typeof(int) || c.PropertyType == typeof(string))).ToArray();

            //设置Y轴
            foreach (var deviceNodeDic in RD3SQLHelper.pNodeDic.Values)//设备因子集合
            {
                int count = 1;
                foreach (var node in deviceNodeDic.Values)//因子集合
                {
                    switch (node.axisType)
                    {
                        case 0://左轴
                            wpfPlot.Plot.Axes.Left.IsVisible = true;
                            wpfPlot.Plot.Axes.Left.Label.Text = string.IsNullOrEmpty(node.unit) ? node.name.ToString() : $"{node.name.ToString()}({node.unit})";
                            (wpfPlot.Plot.Axes.Left as YAxisBase).LabelFontName = ScottPlot.Fonts.Detect((wpfPlot.Plot.Axes.Left as YAxisBase).LabelText);
                            wpfPlot.Plot.Axes.Left.Max = node.maxValue;
                            wpfPlot.Plot.Axes.Left.Min = node.minValue;

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
                    count++;
                }
                break;
            }

            //设置X轴
            // create an array of doubles representing the same DateTimes one hour apart
            double startDouble = DateTime.Now.AddHours(-1).ToOADate(); // days since 1900
            double endDouble = DateTime.Now.AddDays(2.1).ToOADate();
            DateTimeXAxis bottomAxis = wpfPlot.Plot.Axes.DateTimeTicksBottom();
            wpfPlot.Plot.Axes.SetLimitsX(startDouble, endDouble, bottomAxis);
            DateTimeAutomatic tickGen = (DateTimeAutomatic)bottomAxis.TickGenerator;
            tickGen.LabelFormatter = CustomFormatter;//时间格式坐标格式

            //wpfPlot.Plot.Font.Automatic();
            wpfPlot.Plot.ShowLegend(Edge.Top);
            wpfPlot.Refresh();

            InitControls();
            CreatePlotMark();

            wpfPlot.MouseMove += ((sender, e) =>
            {
                WpfPlot wpfPlot = (WpfPlot)sender;
                System.Windows.Point mousePosition = e.GetPosition((UIElement)sender);
                Pixel mousePixel = new(mousePosition.X, mousePosition.Y);
                Coordinates cd = wpfPlot.Plot.GetCoordinates(mousePixel, wpfPlot.Plot.Axes.Bottom);//, signal.Axes.YAxis);
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

            //注册等待消息窗口
            token = aggregator.Resgiter(arg =>
            {
                DialogHost.IsOpen = arg.IsOpen;

                if (DialogHost.IsOpen)
                    DialogHost.DialogContent = new ProgressView();
            });

            #region 更新数据新
            var model = this.DataContext as NewMainViewModel;
            List<double> xList = new List<double>();

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (s, e) =>
            {
                while (true)
                {
                    try
                    {
                        double x = DateTime.Now.ToOADate();//当前时间
                        Dispatcher.BeginInvoke(() =>
                        {
                            xList.Add(x);
                            while (xList.Count > ClockSupervisor.dataMaxCount)
                            {
                                xList.RemoveAt(0);
                            }
                            //添加缓存数据
                            //if (selectedDevice != null)//当前设备，刷新图表
                            //{
                            wpfPlot.Plot.Remove<ScottPlot.Plottables.SignalXY>();
                            var col = ((NewMainViewModel)this.DataContext).ReactorCol;
                            if (col == null || col.Count < 1) return;
                            foreach (var device in col.Where(t => t.CurveDisplay))
                            {
                                var nodeDic = RD3SQLHelper.pNodeDic[device.Name];

                                //添加曲线
                                double firstTime = xList[0];
                                List<double> xVs = new List<double>();
                                Dictionary<string, List<double>> yVsDic = new Dictionary<string, List<double>>();
                                foreach (var node in nodeDic.Values)//添加数据
                                {
                                    yVsDic.Add(node.name, new List<double>());
                                    if (!nodeVisibleDic.ContainsKey(node.name))
                                    {
                                        nodeVisibleDic.Add(node.name, nodeVisibleDic.Count > 4 ? false : true);
                                    }
                                }

                                int cacheCount = ClockSupervisor.realData_time[device.Name].Count;
                                for (int i = 0; i < cacheCount; i++)
                                {
                                    double time = ClockSupervisor.realData_time[device.Name][i];
                                    if (time > firstTime)
                                    {
                                        break;
                                    }
                                    xVs.Add(time);
                                    var realTimeParam = ClockSupervisor.realDatasDic_tenSecond[device.Name][i];
                                    foreach (var node in nodeDic.Values)//添加数据
                                    {
                                        PropertyInfo pi = realTimeParam.GetType().GetProperty(node.fieldName);
                                        if (pi != null && pi.CanRead)
                                        {
                                            yVsDic[node.name].Add(Convert.ToDouble(pi.GetValue(realTimeParam)));
                                        }
                                    }
                                }

                                int yCount = ClockSupervisor.realDatasDic[device.Name].Count;
                                int count = Math.Min(xList.Count, yCount);

                                for (int i = 0; i < count; i++)
                                {
                                    xVs.Add(xList[i]);
                                    var realTimeParam = ClockSupervisor.realDatasDic[device.Name][i];
                                    foreach (var node in nodeDic.Values)//添加数据
                                    {
                                        PropertyInfo pi = realTimeParam.GetType().GetProperty(node.fieldName);
                                        if (pi != null && pi.CanRead)
                                        {
                                            yVsDic[node.name].Add(Convert.ToDouble(pi.GetValue(realTimeParam)));
                                        }
                                    }
                                }

                                foreach (var node in nodeDic.Values)//添加数据
                                {
                                    bool visible = nodeVisibleDic[node.name];

                                    if (!visible)
                                        continue;
                                    string lineLabel = $"{device.Name}_{node.name}";

                                    ScottPlot.Color color = ScottPlot.Color.FromHex("#FFFFFF");
                                    var node1 = ParameterNodeManager.GetInstance().ParameterNodes.FindFirst(t => t.fieldName == node.fieldName);
                                    if (node1 != null)
                                    {
                                        var array = node1.colorStr.Split(',');
                                        color = ScottPlot.Color.FromARGB(Color.FromArgb(byte.Parse(array[0]), byte.Parse(array[1]), byte.Parse(array[2])).ToArgb());
                                    }
                                    SignalXY signal = wpfPlot.Plot.Add.SignalXY(xVs.ToArray(), yVsDic[node.name].ToArray(), color);
                                    signal.LegendText = lineLabel;
                                    YAxisBase yAxis = wpfPlot.Plot.Axes.GetAxes().ToList().Find(c => c.Label.Text.Contains(node.name)) as YAxisBase;
                                    signal.Axes.YAxis = yAxis == null ? wpfPlot.Plot.Axes.Left : yAxis;
                                    signal.Axes.XAxis = wpfPlot.Plot.Axes.Bottom;
                                    signal.MarkerSize = (float)node1?.pointSize;
                                    signal.LineWidth = (float)node1?.lineWidth;
                                }
                                wpfPlot?.Refresh();
                            }

                            //}
                        });
                    }
                    catch (Exception ex)
                    {
                        string msg = $"{ex.Message}\r\n{ex.StackTrace}";
                        LogHelper.Error(msg);
                    }
                    finally
                    {
                        Thread.Sleep(AppSession.Interval * 1000);
                        if (DateTime.Now.Second % 15 == 0)
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                    }
                }
            };
            worker.RunWorkerAsync();
            #endregion

            btnMin.Click += (s, e) => { this.WindowState = WindowState.Minimized; };
            btnMax.Click += (s, e) =>
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    PackIconWindowState.Kind = PackIconKind.WindowMaximize;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    PackIconWindowState.Kind = PackIconKind.WindowRestore;
                }
            };
            btnClose.Click += async (s, e) =>
            {
                var dialogResult = await dialogHostService.Question("温馨提示", "确认退出系统?");
                if (dialogResult.Result != ButtonResult.OK) return;
                Application.Current.Shutdown();
                Environment.Exit(0);
            };

            this.dialogHostService = dialogHostService;

            //this.WindowState = WindowState.Maximized;

            this.MouseMove += (s, e) =>
            {
                Window window = s as Window;
                
                if (e.LeftButton == MouseButtonState.Pressed && window.WindowState != WindowState.Maximized)
                {
                    System.Windows.Point point = e.GetPosition(this);
                    if (point.Y < 40)
                    {
                        this.Cursor = Cursors.Hand;
                        this.DragMove();
                    }
                    
                }
                else
                {
                    this.Cursor = Cursors.Arrow;
                }
            };
            this.MouseLeftButtonDown += (s, e) => 
            {
                Window window = s as Window;
                if (e.ClickCount > 1)
                {
                    window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                }
            };


            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += (s, e) =>
            {
                while (true)
                {
                    try
                    {
                        App.Current.Dispatcher.BeginInvoke(() =>
                        {
                            var selectedItem = dataGrid1.SelectedItem as AuditRecord;
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.AppendLine("select * from Audit");
                            ObservableCollection<AuditRecord> auditRecords = dataGrid1.ItemsSource as ObservableCollection<AuditRecord>;
                            if (auditRecords != null && auditRecords.Count > 0)
                            {
                                stringBuilder.AppendLine($"where DateTime > '{auditRecords[0].Datetime}'");
                            }
                            stringBuilder.AppendLine("order by DateTime desc");
                            DataTable dt = SQLiteHelper.GetDatasToDataTable(stringBuilder.ToString());
                            var addRecords = new ObservableCollection<AuditRecord>();
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                AuditRecord record = new AuditRecord();
                                record.BatchID = dt.Rows[i]["BatchID"].ToString();
                                record.Datetime = dt.Rows[i]["Datetime"].ToString();
                                record.DeviceID = dt.Rows[i]["DeviceID"].ToString();
                                record.Remark = dt.Rows[i]["Remark"].ToString();
                                addRecords.Add(record);
                            }
                            if (auditRecords != null && auditRecords.Count > 0)
                            {
                                foreach (AuditRecord record in auditRecords) 
                                {
                                    addRecords.Add(record);
                                }
                            }
                            dataGrid1.ItemsSource = addRecords;
                            if (selectedItem != null)
                            {
                                dataGrid1.ScrollIntoView(selectedItem);
                                dataGrid1.SelectedItem = selectedItem;
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug("获取操作记录失败" + ex.Message);
                    }
                    finally
                    {
                        Thread.Sleep(AppSession.Interval * 1000);
                    }
                }
            };
            backgroundWorker.RunWorkerAsync();

        }
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
        /// 获取数据
        /// </summary>
        /// <param name="nodeDic"></param>
        /// <returns></returns>
        private string GetValueStr(double x, Dictionary<string, dataValue> nodeDic, RealTimeParam realTimeParam)
        {
            string result = $"{x.ToString()},";
            foreach (var item in nodeDic.Values)
            {
                PropertyInfo pi = realTimeParam.GetType().GetProperty(item.fieldName);
                double value = 0;
                if (pi != null && pi.CanRead)
                {
                    value = (double)pi.GetValue(realTimeParam);
                }
                result += string.Format("{0},", value.ToString("F3"));
            }
            result = result.TrimEnd(',');
            result += "\r\n";
            return result;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="nodeDic"></param>
        /// <returns></returns>
        private string GetValueStr(double x,Dictionary<string, dataValue> nodeDic)
        {
            string result = $"{x.ToString()},";
            foreach(var item in nodeDic.Values)
            {
                result += string.Format("{0},",item.value.ToString("F3"));
            }
            result = result.TrimEnd(',');
            result += "\r\n";
            return result;
        }
        /// <summary>
        /// 添加新线条
        /// </summary>
        /// <param name="lineLabel"></param>
        /// <param name="x"></param>
        /// <param name="node"></param>
        private void AddNewLine(string lineLabel,double x,dataValue node)
        {
            var scatter = wpfPlot.Plot.Add.Scatter(x, node.value, ScottPlot.Color.FromColor(GetColor(node)));
            scatter.LegendText = lineLabel;
            scatter.IsVisible = GetLineIsVisible(lineLabel);
            YAxisBase yAxis = wpfPlot.Plot.Axes.GetAxes().ToList().Find(c => c.Label.Text.Contains(node.name)) as YAxisBase; 
            scatter.Axes.YAxis = yAxis == null? wpfPlot.Plot.Axes.Left:yAxis;
            scatter.Axes.XAxis = wpfPlot.Plot.Axes.Bottom;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="lineLabel"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void UpdateLineDatas(string lineLabel,double x, dataValue node)
        {
            for (int i = 0; i < wpfPlot.Plot.PlottableList.Count; i++)
            {
                if (wpfPlot.Plot.PlottableList[i] is Scatter scatter)
                {
                    if (lineLabel != scatter.LegendText)
                        continue;
                    var souceDatas = scatter.Data.GetScatterPoints().ToList();
                    souceDatas.Add(new Coordinates() {X = x,Y = node.value });
                    while(souceDatas.Count>  pointCount)//modify by hdb 缓存2小时数据
                    {
                        souceDatas.RemoveAt(0);
                    }
                    ScatterSourceCoordinatesList sscl = new ScatterSourceCoordinatesList(souceDatas);
                    wpfPlot.Plot.PlottableList[i] = GetScatter(scatter, sscl);
                }
            }
            GC.Collect();
        }
        /// <summary>
        /// 重新生成曲线
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sscl"></param>
        /// <returns></returns>
        private Scatter GetScatter(Scatter source, ScatterSourceCoordinatesList sscl)
        {
            Scatter target = new Scatter(sscl);
            PropertyInfo[] pis = target.GetType().GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                if (pi.CanWrite && pi.CanRead)
                {
                    object value = pi.GetValue(source);
                    pi.SetValue(target, value);
                }
            }
            return target;
        }

        /// <summary>
        /// 是否显示线条
        /// </summary>
        /// <param name="lineLabel"></param>
        /// <returns></returns>
        private bool GetLineIsVisible(string lineLabel)
        {
            //var selectedDevice = devieceGrid.SelectedItem as RD3Device;
            if(selectedDevice == null)
            {
                return false;
            }
            return lineLabel.Contains(selectedDevice.Name);
        }
        /// <summary>
        /// 获取颜色
        /// 可以做成缓存
        /// </summary>
        /// <param name="dv"></param>
        /// <returns></returns>
        private Color GetColor(dataValue dv)
        {
            //string[] rgb = dv.colorStr.Split(',');
            //return Color.FromArgb(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]));
            var node = ParameterNodeManager.GetInstance().ParameterNodes.FindFirst(t => t.ename == dv.ename);
            if (node != null)
            {
                var array = node.colorStr.Split(',');
                return Color.FromArgb(byte.Parse(array[0]), byte.Parse(array[1]), byte.Parse(array[2]));
            }
            else
            {
                string[] rgb = dv.colorStr.Split(',');
                return Color.FromArgb(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]));
            }
        }
        /// <summary>
        /// 反射获取值
        /// </summary>
        /// <param name="realTimeParam"></param>
        /// <param name="dv"></param>
        private void GetLineValue(RealTimeParam realTimeParam, Dictionary<string, dataValue> nodeDic)
        {
            foreach (var dv in nodeDic.Values)
            {
                PropertyInfo pi = realTimeParam.GetType().GetProperty(dv.fieldName);
                if (pi != null && pi.CanRead)
                {
                    dv.value = (double)pi.GetValue(realTimeParam);
                }
            }
        }
        /// <summary>
        /// 找到对应线条
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        private Scatter getScatterByLabel(string label)
        {
            foreach (var item in wpfPlot.Plot.PlottableList)
            {
                if (item is Scatter scatter && scatter.LegendText == label)
                    return scatter;
            }
            return null;
        }

        RD3Device selectedDevice = null;
        /// <summary>
        /// 初始化控件
        /// </summary>
        private void InitControls()
        {
            devieceGrid.HeadersVisibility = DataGridHeadersVisibility.Column;
            DevieceDetial.HeadersVisibility = DataGridHeadersVisibility.Column;

            DO.Checked += cb_checkChanged;
            Agit.Checked += cb_checkChanged;
            Air.Checked += cb_checkChanged;
            pH.Checked += cb_checkChanged;
            Temp.Checked += cb_checkChanged;

            AcidPumpFlowRate.Checked += cb_checkChanged;
            BasePumpFlowRate.Checked += cb_checkChanged;
            AFPumpFlowRate.Checked += cb_checkChanged;
            FeedPumpFlowRate.Checked += cb_checkChanged;

            AcidPumpFlow.Checked += cb_checkChanged;
            BasePumpFlow.Checked += cb_checkChanged;
            FeedPumpFlow.Checked += cb_checkChanged;
            AFPumpFlow.Checked += cb_checkChanged;
        }
       public static string CustomFormatter(DateTime dt)
        {
            bool isMidnight = dt is { Hour: 0, Minute: 0, Second: 0 };
            return isMidnight ? DateOnly.FromDateTime(dt).ToString() : TimeOnly.FromDateTime(dt).ToString("HH\\:mm\\:ss");
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
        /// 图标菜单选中/图标菜单未选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cb_checkChanged(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkBox = sender as System.Windows.Controls.CheckBox;
            if (nodeVisibleDic.ContainsKey(checkBox.Name))
            {
                nodeVisibleDic[checkBox.Name] = checkBox.IsChecked.HasValue && checkBox.IsChecked.Value;
            }
            else
            {
                nodeVisibleDic.Add(checkBox.Name, checkBox.IsChecked.HasValue && checkBox.IsChecked.Value);
            }
            var axes = wpfPlot?.Plot?.Axes?.GetAxes();
            foreach (var axis in axes)
            {
                if (axis.Label.Text.Contains(checkBox.Name))
                {
                    axis.IsVisible = checkBox.IsChecked.HasValue && checkBox.IsChecked.Value;
                }
            }

            //var selectedItem = devieceGrid.SelectedItem as RD3Device;

            //foreach (var item in wpfPlot.Plot.PlottableList)
            //{
            //    Scatter scatter = item as Scatter;
            //    if (scatter.LegendText.Contains(checkBox.Name))
            //    {
            //        scatter.IsVisible = checkBox.IsChecked.HasValue && checkBox.IsChecked.Value; ;
            //    }
            //    if (!scatter.LegendText.Contains(selectedItem.Name))
            //    {
            //        scatter.IsVisible = false;
            //    }
            //}
            //wpfPlot.Refresh();
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.Cursor = Cursors.Hand;
                this.DragMove();
            }
            else
            {
                this.Cursor= Cursors.Arrow;
            }
        }


        /// <summary>
        /// 设备选中，更新设备详情
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 获取当前选中的行
           selectedDevice = devieceGrid.SelectedItem as RD3Device;
            updateDevieceDetail(selectedDevice);

            //wpfPlot.Plot.Title(selectedDevice?.Name);
            //updatechart(selectedDevice);
        }
        /// <summary>
        /// 更新图表数据
        /// </summary>
        /// <param name="selectedItem"></param>
        private void updatechart(RD3Device device)
        {
            System.Windows.Controls.CheckBox[] boxes = new System.Windows.Controls.CheckBox[] { DO, Air, Agit, Temp, pH, AcidPumpFlowRate, BasePumpFlowRate, AFPumpFlowRate, FeedPumpFlowRate, AcidPumpFlow, BasePumpFlow, FeedPumpFlow, AFPumpFlow };
            //显示当前设备数据//隐藏其他设备数据
            string deviceId = device.Name;
            foreach (var item in wpfPlot.Plot.PlottableList)
            {
                Scatter scatter = item as Scatter;
                scatter.IsVisible = scatter.LegendText.Contains(deviceId);
                foreach(System.Windows.Controls.CheckBox box in boxes)
                {
                    if (scatter.LegendText.Contains(box.Name)&&box.IsChecked.HasValue && !box.IsChecked.Value)
                    {
                        scatter.IsVisible = false;
                    }
                }
            }
            wpfPlot.Refresh();
        }
        

        /// <summary>
        /// 更新设备详情
        /// </summary>
        /// <param name="device"></param>
        private void updateDevieceDetail(RD3Device device)
        {
            if (device == null)
            { return; }
            DevieceDetial.Columns[1].Header = device.Name;
            ((NewMainViewModel)this.DataContext)?.UpdateDeviece(device);//上下文
        }

        /// <summary>
        /// 点击菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mItem = sender as MenuItem;
            string[] array = mItem.Tag?.ToString().Split(",");
            string viewName = array[InstrumentSolution.GetInstance().CommunicationProtocol];
            dialogHostService?.ShowOnce(viewName, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        }

        /// <summary>
        /// 设备右键，运行批次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void devieceGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu context = new ContextMenu();
            MenuItem item = new MenuItem() { Width = 100};
            item.Header = "开始批次";
            item.Click += new RoutedEventHandler(doWork_Click);
            context.Items.Add(item);
            context.IsOpen = true;

            item = new MenuItem() { Width = 100 };
            item.Header = "结束批次";
            item.Click += new RoutedEventHandler(stopWork_Click);
            context.Items.Add(item);
            context.IsOpen = true;


            //0430 元素驱动 方成
            //item = new MenuItem() { Width = 100 };
            //item.Header = "离线数据";
            //item.Click += new RoutedEventHandler(offLine_Click);
            //context.Items.Add(item);
            //context.IsOpen = true;

            item = new MenuItem() { Width = 100 };
            item.Header = "设备详情";
            item.Command = ((NewMainViewModel)DataContext).ReactorCommand;
            context.Items.Add(item);
            context.IsOpen = true;
        }


        /// <summary>
        /// 添加离线数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void offLine_Click(object sender, RoutedEventArgs e)
        {
            var selectedDevice = devieceGrid.SelectedItem as RD3Device;
            if (selectedDevice == null)
            {
                return ;
            }
            OffLineSampleDatas offLineData = new OffLineSampleDatas()
            {
                BatchID = selectedDevice.BatchID.ToString(),
                CollectTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Collecter = AppSession.CurrentUser.UserName,
                SampleName = "",
            };
            IDialogParameters parameters  = new DialogParameters
            {
                { "OffLineData", offLineData },
            };
            //弹窗，输入离线数据
            dialogHostService?.ShowOnce(nameof(AddOffLineDataView), parameters, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                //保存到相应批次
                string fileNme = creatFile(selectedDevice, "offline.json",true);
                string result = JsonConvert.SerializeObject(offLineData);
                File.AppendAllText(fileNme, $"{result}\r\n");
                //显示到图表

                double x = DateTime.Parse(offLineData.CollectTime).ToOADate();
                foreach (var node in RD3SQLHelper.offLineNodeDic.Values)//添加数据
                {
                    string lineLabel = $"{selectedDevice.Name}_{node.name}";
                    var line = getScatterByLabel(lineLabel);
                    if (line == null)
                    {//新增
                        AddNewLine(lineLabel, x, node);
                    }
                    else//更新
                    {
                        UpdateLineDatas(lineLabel, x, node);
                    }
                }

            });
        }

        /// <summary>
        /// 下罐操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopWork_Click(object sender, RoutedEventArgs e)
        {
            //停止保存数据标识
            foreach (var item in devieceGrid.SelectedItems)
            {
                RD3Device device = item as RD3Device;
                if (device.Status == "Availble")
                {
                    continue;
                }

                Task.Run(() =>
                {
                    string batchID = device.BatchID.ToString();
                    string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name + "\\" + batchID;
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    string fileNme = dir + "\\Audit.txt";
                    StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", device?.Name, batchID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                    string content = string.Format("批次结束，批次ID为{0}", batchID);
                    sb.AppendLine(content);
                    File.AppendAllText(fileNme, sb.ToString());
                    RD3SQLHelper.AddAuditRecord(device?.Name, batchID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                });

                updateBatchInfo(device);//更新批次信息=

                device.Status = "Availble";
                device.BatchStatus = "Complete";
                device.BatchID = 0;//将批次ID置零，表示空闲状态
                updateDevieceDetail(device);
            }
        }

        /// <summary>
        /// 运行批次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void doWork_Click(object sender, RoutedEventArgs e)
        {
            Tuple<string, string, string> tuple = Tuple.Create(string.Empty, string.Empty, string.Empty);
            List<string> list = new List<string>();
            foreach (var item in devieceGrid.SelectedItems)
            {
                RD3Device device = item as RD3Device;
                list.Add(device.Name);
            }
            DialogParameters dialogParameters = new DialogParameters()
            {
                { "DeviceName",string.Join("|",list)}
            };
            dialogHostService?.ShowOnce(nameof(AddBatchInfoView), dialogParameters,callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                tuple = callback.Parameters.GetValue<Tuple<string, string, string>>(nameof(Batch));

                AppSession.RunningDevices = [];

                foreach (var item in devieceGrid.SelectedItems)
                {
                    RD3Device device = item as RD3Device;
                    if (device.Status == "Running")
                    {
                        continue;
                    }
                    device.Status = "Running";
                    device.BatchStatus = "Running";
                    DateTime dt = DateTime.Now;
                    RD3Batch rD3Batch = new RD3Batch()
                    {
                        createTime = dt.ToString("yyyy-MM-dd HH:mm:ss"),
                        devieceID = device.Name,
                        statue = (int)RD3BatchStatue.Running,
                        startDateTime = dt.ToString("yyyy-MM-dd HH:mm:ss"),
                        createUser = AppSession.CurrentUser.UserName,
                        Strain = tuple.Item1,
                        Tester = tuple.Item2,
                        description = tuple.Item3
                    };
                    rD3Batch.ID = RD3SQLHelper.InsertBatch(rD3Batch);
                    Task.Run(() => 
                    {
                        string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name + "\\" + device.BatchID;
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        string fileNme = dir + "\\Audit.txt";
                        StringBuilder sb = new StringBuilder(string.Format("反应器{0} 批次ID{1} 时间{2} ", device?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")));
                        string content = string.Format("批次开始，批次ID为{0}", device.BatchID);
                        sb.AppendLine(content);
                        File.AppendAllText(fileNme, sb.ToString());
                        RD3SQLHelper.AddAuditRecord(device?.Name, device?.BatchID.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), content);
                    });
                    rD3Batch.idRemark = rD3Batch.ID.ToString();
                    device.BatchID = rD3Batch.ID;
                    updateDevieceDetail(device);
                    string dir = creatFile(device, "online.csv");
                    saveBatchInfo(dir, rD3Batch);

                    AppSession.RunningDevices.Add(device);
                }
            });
        }
        /// <summary>
        /// 保存批次信息
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="rD3Batch"></param>
        private void saveBatchInfo(string dir, RD3Batch rD3Batch)
        {
            string fileName = $"{dir}\\batch.txt";
            string result = JsonConvert.SerializeObject(rD3Batch);
            File.WriteAllText(fileName, result);
        }
        /// <summary>
        /// 更新批次信息
        /// </summary>
        /// <param name="device"></param>
        private void updateBatchInfo(RD3Device device)
        {
            string fileName = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name + "\\" + $"{device.BatchID}\\batch.txt";
            string result = File.ReadAllText(fileName);
            RD3Batch batch = CustomApp.JsonHelper.StringToObject<RD3Batch>(result);
            batch.endDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            batch.statue = (long)RD3BatchStatue.Complete;
            File.WriteAllText(fileName, JsonConvert.SerializeObject(batch));

            RD3SQLHelper.UpdataBatch(batch);
        }
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="device"></param>
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="device"></param>
        private string creatFile(RD3Device device, string fileName, bool isOffLine = false)
        {
            //新建文件夹
            string dir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData\\" + device.Name;
            Directory.CreateDirectory(dir);
            dir = dir + "\\" + device.BatchID;
            Directory.CreateDirectory(dir);
            string fileNme = dir + "\\" + fileName;
            if (!isOffLine)//在线数据
            {
                File.AppendAllText(fileNme, GetColumns(RD3SQLHelper.pNodeDic[device.Name]));
                return dir;
            }
            else//离线数据
            {
                return fileNme;
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="nodeDic"></param>
        /// <returns></returns>
        private string GetColumns(Dictionary<string, dataValue> nodeDic)
        {
            string result = "dateTime,";
            foreach (var item in nodeDic.Values)
            {
                result += string.Format("{0},", item.name);
            }
            result = result.TrimEnd(',');
            result += "\r\n";
            return result;
        }
        ~NewMainView()
        {
            token.Dispose();
            token = null;

            token1.Dispose();
            token1 = null;
        }
    }
}
