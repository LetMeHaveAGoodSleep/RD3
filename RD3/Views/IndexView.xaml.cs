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

namespace RD3.Views
{
    /// <summary>
    /// IndexView.xaml 的交互逻辑
    /// </summary>
    public partial class IndexView : UserControl
    {
        List<DeviceExperimentHistoryData> DeviceExperimentHistoryDatas = [];

        private readonly DateTimeXAxis bottomAxis;
        private readonly LeftAxis yAxis2;
        private readonly LeftAxis yAxis3;
        private readonly RightAxis yAxis4;
        private readonly RightAxis yAxis5;

        Crosshair MyCrosshair;
        ScottPlot.Plottables.Marker MyHighlightMarker;
        ScottPlot.Plottables.Text MyHighlightText;
        private readonly IEventAggregator _aggregator;
        private readonly ILanguage language;

        List<DeviceExperimentHistoryData> dataSource = [];

        public IndexView(IContainerProvider containerProvider, IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            language = containerProvider.Resolve<ILanguage>();

            InitializeComponent();

            wpfPlot1.Plot.Legend.IsVisible = true;
            wpfPlot1.Plot.ShowLegend(Edge.Top);
            var xAxis = wpfPlot1.Plot.Axes.DateTimeTicksBottom();
            ((DateTimeAutomatic)xAxis.TickGenerator).LabelFormatter = CustomFormatter;
            wpfPlot1.Menu.Clear();
            //wpfPlot1.Interaction = null;
            wpfPlot1.MouseDoubleClick += (s, e) => //点击小图表跳转到曲线Tag
            {
                tabControl.SelectedItem = tabTrend;
            };


            var plt = wpfPlot.Plot;
            plt.Legend.IsVisible = true;
            plt.ShowLegend(Edge.Top);


            // create an array of DateTimes one hour apart
            int numberOfHours = 24;
            DateTime[] dateTimes = new DateTime[numberOfHours];
            DateTime startDateTime = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            TimeSpan deltaTimeSpan = TimeSpan.FromHours(1);
            for (int i = 0; i < numberOfHours; i++)
            {
                dateTimes[i] = startDateTime + i * deltaTimeSpan;
            }

            // create an array of doubles representing the same DateTimes one hour apart
            double[] dateDoubles = new double[numberOfHours];
            double startDouble = startDateTime.ToOADate(); // days since 1900
            double deltaDouble = 1.0 / 24.0; // an hour is 1/24 of a day
            for (int i = 0; i < numberOfHours; i++)
            {
                dateDoubles[i] = startDouble + i * deltaDouble;
            }

            bottomAxis = plt.Axes.DateTimeTicksBottom();
            plt.Axes.SetLimitsX(dateDoubles[0], dateDoubles[dateDoubles.Length - 1], bottomAxis);
            DateTimeAutomatic tickGen = (DateTimeAutomatic)bottomAxis.TickGenerator;
            tickGen.LabelFormatter = CustomFormatter;
            plt.Axes.Left.Label.Text = "DO";

            // now both arrays represent the same dates
            //var scatter1 = plt.Add.ScatterLine(dateTimes, Generate.Sin(numberOfHours));
            var scatter3 = plt.Add.Scatter(dateDoubles, Generate.Cos(numberOfHours));
            scatter3.Axes.YAxis = plt.Axes.Left;
            scatter3.Axes.XAxis = plt.Axes.Bottom;
            scatter3.LegendText = "1";

            // create a second axis and add it to the plot
            yAxis2 = plt.Axes.AddLeftAxis();
            yAxis2.LabelPadding = 100;
            yAxis2.LabelText = "Agit";
            yAxis2.EmptyLabelPadding = new PixelPadding(100);
            plt.Axes.SetLimitsY(0, 10000, yAxis2);


            yAxis3 = plt.Axes.AddLeftAxis();
            yAxis3.LabelText = "Air";
            plt.Axes.SetLimitsY(0, 100, yAxis3);

            yAxis4 = plt.Axes.AddRightAxis();
            yAxis4.LabelText = "pH";
            plt.Axes.SetLimitsY(0, 10, yAxis4);

            yAxis5 = plt.Axes.AddRightAxis();
            yAxis5.LabelText = "Temp";
            plt.Axes.SetLimitsY(0, 100,yAxis5);
            

            plt.Axes.SetLimitsX(dateDoubles[0], dateDoubles[dateDoubles.Length - 1], bottomAxis);

            CreatePlotMark();

            CustomMouseAction(wpfPlot);

            CustomContextMenu(wpfPlot);
            CustomContextMenu(wpfPlot1);

            foreach (RadioButton item in ButtonGroup.Items)
            {
                item.Checked += RadioButton_Checked;
            }

            foreach (RadioButton item in ButtonGroup1.Items)
            {
                item.Checked += RadioButtonParamOverview_Checked;
            }

            aggregator.ResgiterMessage((MessageModel model) =>
            {
                var parameter = model.Model as DialogParameters;
                double volume = parameter.GetValue<double>("Volume");
                DateTime time = parameter.GetValue<DateTime>("Time");
                List<string> devices = parameter.GetValue<List<string>>("Devices");
                UnScheduleAction type = parameter.GetValue<UnScheduleAction>("ActionType");

                DateTime[] dates = [time];
                double[] ys = [volume];
                foreach (string item in devices)
                {
                    var scatter = plt.Add.ScatterPoints(dates, ys);
                    scatter.LegendText = item + "_" + EnumUtil.GetEnumDescription(type) + "_" + time.ToString("HH-mm-ss");
                    scatter.Axes.XAxis = plt.Axes.Bottom; // standard X axis
                    scatter.Axes.YAxis = yAxis5; // custom Y axis
                }
                tabControl.SelectedItem = tabTrend;

            }, nameof(UnscheduledViewModel));

            //注册提示消息
            aggregator.ResgiterMessage(arg =>
            {
                LoginSnakeBar.MessageQueue.Enqueue(arg.Message);
            }, nameof(IndexViewModel));

            aggregator.ResgiterMessage(arg =>
            {
                wpfPlot.Plot.Clear();
                wpfPlot1.Plot.Clear();

                DeviceExperimentHistoryDatas = arg.Model as List<DeviceExperimentHistoryData>;
                foreach (var item in DeviceExperimentHistoryDatas)
                {
                    foreach (var item1 in item.ExperimentHistoryDatas)
                    {
                        var count = item1.Xs.Count <= item1.Ys.Count ? item1.Xs.Count : item1.Ys.Count;
                        for (int i = 0; i < count; i++)
                        {
                            var scatter = wpfPlot.Plot.Add.Scatter(item1.Xs[i], item1.Ys[i]);
                            foreach (var item2 in wpfPlot.Plot.Axes.GetAxes().Where(t => t.Label.Text == item1.ExperimentParameter.ToString()))
                            {
                                scatter.Axes.YAxis = (IYAxis)item2;
                                break;
                            }
                            scatter.Axes.XAxis = wpfPlot.Plot.Axes.Bottom;
                            scatter.LegendText = item.DeviceName + "_" + item1.ExperimentParameter.ToString() + "_" + (i + 1).ToString();

                            if (item1.ExperimentParameter == ((IndexViewModel)this.DataContext).CurrentExperimentParameter)
                            {
                                var scatter1 = wpfPlot1.Plot.Add.Scatter(item1.Xs[i], item1.Ys[i]);
                                //scatter1.Axes.XAxis = wpfPlot1.Plot.Axes.Bottom;
                            }
                        }
                    }
                }
                wpfPlot.Plot.Axes.AutoScale();
                wpfPlot.Refresh();
                wpfPlot1.Plot.Axes.AutoScale();
                wpfPlot1.Refresh();
            }, "UpdatePlot");

            //ComboBoxTime.SelectionChanged += ComboBoxTime_SelectionChanged;
            wpfPlot.MouseMove += WpfPlot_MouseMove;
            wpfPlot.MouseDoubleClick += WpfPlot_MouseDoubleClick;


            ChkAll.Checked += ChkAll_Checked;
            ChkAll.Unchecked += ChkAll_Unchecked;

            var parent = ChkAll.Parent;
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child == null) continue;
                var item = child as CheckBox;
                if (item == null) continue;
                item.Checked += CheckBox_CheckChanged;
                item.Unchecked += CheckBox_CheckChanged;
            }
        }

        private void ChkAll_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            var parent = checkBox.Parent;
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child == null) continue;
                var item = child as CheckBox;
                if (item == null || object.ReferenceEquals(item, checkBox)) continue;
                item.IsChecked = true;
            }
        }

        private void ChkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            var parent = checkBox.Parent;
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child == null) continue;
                var item = child as CheckBox;
                if (item == null || object.ReferenceEquals(item, checkBox)) continue;
                item.IsChecked = false;
            }
        }

        private void CheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            var parent = checkBox.Parent;
            var count = VisualTreeHelper.GetChildrenCount(parent);
            int checkedCount = 0;
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child == null) continue;
                var item = child as CheckBox;
                if (item == null) continue;
                if (item.Name == nameof(ChkAll)) continue;
                checkedCount += Convert.ToInt32(item.IsChecked);
            }
            if (checkedCount == count - 1)
            {
                ChkAll.IsChecked = true;
            }
            else if (checkedCount == 0)
            {
                ChkAll.IsChecked = false;
            }
            else
            {
                ChkAll.IsChecked = null;
            }
        }
        private List<string> GetCheckedDevice()
        {
            List<string> devices = new List<string>();
            var parent = ChkAll.Parent;
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child == null) continue;
                var item = child as CheckBox;
                if (item == null) continue;
                if (item.Name != nameof(ChkAll) && (bool)item.IsChecked)
                {
                    devices.Add(item.Content.ToString());
                }
            }
            return devices;
        }

        private void WpfPlot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            wpfPlot.Plot.Axes.AutoScale();
            wpfPlot.Refresh();
        }

        private void CustomMouseAction(WpfPlot wpfPlot)
        {
            ScottPlot.Control.InputBindings customInputBindings = new()
            {
                DragPanButton = ScottPlot.Control.MouseButton.Left,
                DragZoomRectangleButton = ScottPlot.Control.MouseButton.Left,
                //DragZoomButton = ScottPlot.Control.MouseButton.Right,
                ZoomInWheelDirection = ScottPlot.Control.MouseWheelDirection.Up,
                ZoomOutWheelDirection = ScottPlot.Control.MouseWheelDirection.Down,
                ClickContextMenuButton = ScottPlot.Control.MouseButton.Right,
                DoubleClickButton = ScottPlot.Control.MouseButton.Left
            };

            ScottPlot.Control.Interaction interaction = new(wpfPlot)
            {
                Inputs = customInputBindings,
            };

            wpfPlot.Interaction = interaction;
        }

        private void CustomContextMenu(WpfPlot wpfPlot)
        {
            // clear existing menu items
            wpfPlot.Menu.Clear();

            wpfPlot.Menu.Add("Add Text", (formsplot1) =>
            {
                var txt = formsplot1.Plot.Add.Text("Test", Generate.RandomLocation());
                txt.LabelFontSize = 10 + Generate.RandomInteger(20);
                txt.LabelFontColor = Generate.RandomColor(128);
                txt.LabelBold = true;
                formsplot1.Plot.Axes.AutoScale();
                formsplot1.Refresh();
            });

            //wpfPlot.Menu.AddSeparator();

            //wpfPlot.Menu.Add(language.GetValue("Reset").ToString(), (formsplot1) =>
            //{
            //    formsplot1.Reset();
            //    formsplot1.Refresh();
            //});

            //wpfPlot.Plot.Title("Custom Right-Click Menu");
            //wpfPlot.Refresh();
        }

        private void WpfPlot_MouseMove(object sender, MouseEventArgs e)
        {
            WpfPlot wpfPlot = sender as WpfPlot;
            MyCrosshair.IsVisible = MyHighlightText.IsVisible = MyHighlightMarker.IsVisible = false;
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
        }

        private void CreatePlotMark()
        {
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
        }

        static string CustomFormatter(DateTime dt)
        {
            bool isMidnight = dt is { Hour: 0, Minute: 0, Second: 0 };
            return isMidnight
                ? DateOnly.FromDateTime(dt).ToString()
                : TimeOnly.FromDateTime(dt).ToString();
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

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 1) return;
            ((IndexViewModel)this.DataContext)?.EditParamCommand.Execute();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            foreach (TabItem item in tabControl.Items)
            {
                if (radioButton.Tag?.ToString() == item.Name)
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        private void RadioButtonParamOverview_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            wpfPlot1.Plot.Clear();
            //TODO:曲线切换
            foreach (var item in DeviceExperimentHistoryDatas)
            {
                foreach (var item1 in item.ExperimentHistoryDatas.Where(t=>t.ExperimentParameter== ((IndexViewModel)this.DataContext).CurrentExperimentParameter))
                {
                    var count = item1.Xs.Count <= item1.Ys.Count ? item1.Xs.Count : item1.Ys.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var scatter = wpfPlot1.Plot.Add.Scatter(item1.Xs[i], item1.Ys[i]);
                    }
                }
            }
        }

        private void ComboBoxTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox?.SelectedIndex == -1) return;

        }

        private void BtnScale_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtMaxScale.Text) || string.IsNullOrEmpty(TxtMinScale.Text))
            {
                return;
            }
            if (!double.TryParse(TxtMinScale.Text, out double minScle) || !double.TryParse(TxtMaxScale.Text, out double maxScle))
            {
                MessageBox.Show(language.GetValue("Must be numeric").ToString());
                return;
            }
            switch (CmbYAxis.SelectedIndex)
            {
                case 0:
                    wpfPlot.Plot.Axes.SetLimitsY(minScle, maxScle, yAxis3);
                    wpfPlot.Refresh();
                    break;
                case 1:
                    wpfPlot.Plot.Axes.SetLimitsY(minScle, maxScle, yAxis2);
                    wpfPlot.Refresh();
                    break;
                case 2:
                    wpfPlot.Plot.Axes.SetLimitsY(minScle, maxScle, wpfPlot.Plot.Axes.Left);
                    wpfPlot.Refresh();
                    break;
                case 3:
                    wpfPlot.Plot.Axes.SetLimitsY(minScle, maxScle, yAxis4);
                    wpfPlot.Refresh();
                    break;
                case 4:
                    wpfPlot.Plot.Axes.SetLimitsY(minScle, maxScle, yAxis5);
                    wpfPlot.Refresh();
                    break;
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            var devices = GetCheckedDevice();
            if (devices.Count < 1)
            {
                MessageBox.Show("请选择仪器");
                tabControl.SelectedItem = tabTrend;
                BorderDevice.Focus();
                return;
            }

            ((IndexViewModel)DataContext).Devices = devices;
            ((IndexViewModel)DataContext)?.StartCommand.Execute();
        }
    }
}
