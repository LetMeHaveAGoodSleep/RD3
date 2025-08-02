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
using System.Data.Common;
using System.Data;
using System.Runtime.CompilerServices;
using static MaterialDesignThemes.Wpf.Theme;
using OpenTK.Graphics;
using Prism.Mvvm;
using DataGridCell = System.Windows.Controls.DataGridCell;
using ScottPlot.PathStrategies;
using ScottPlot.Hatches;
using System.Threading;
using System.Windows.Forms;

namespace RD3.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class DeviceNodesView : System.Windows.Controls.UserControl
    {
        private readonly IDialogHostService dialogHostService;
        readonly ILanguage language;
        public DeviceNodesView(IEventAggregator aggregator, IDialogHostService dialogHostService, IContainerProvider containerProvider)
        {
            InitializeComponent();

            language = containerProvider.Resolve<ILanguage>();

            System.Windows.Application.Current.Resources["AppFontFamily"] = AppSession.FontFamily;

            this.dialogHostService = dialogHostService;

            InitComBoxItes();
        }

        private void InitComBoxItes()
        {
            List<AxisType> axisTypes = new List<AxisType>();
            axisTypes.Add(new AxisType() { Value = 0, Name = "左侧坐标" });//左侧坐标，1-左侧新加坐标，2-右侧坐标，3右侧新加坐标
            axisTypes.Add(new AxisType() { Value = 1, Name = "左侧新加坐标" });
            axisTypes.Add(new AxisType() { Value = 2, Name = "右侧坐标" });
            axisTypes.Add(new AxisType() { Value = 3, Name = "右侧新加坐标" });
            colAxisType.ItemsSource = axisTypes;
            colAxisType.DisplayMemberPath = "Name";
            colAxisType.SelectedValuePath = "Value";

            List<string> unitList = new List<string>() {"", "℃",  "mL", "mL/h", "L/min", "%", "rpm","psi" };
            colUnit.ItemsSource = unitList;


            List<UsedType> usedType = new List<UsedType>();
            usedType.Add(new UsedType() { Value = 0, Name = "是" });
            usedType.Add(new UsedType() { Value = 1, Name = "否" });
            colUsed.ItemsSource = usedType;
            colUsed.DisplayMemberPath = "Name";
            colUsed.SelectedValuePath = "Value";

            Type type = typeof(RealTimeParam);
            List<PropertyInfo> fields = type.GetProperties().ToList().FindAll(c => c.CanRead && c.CanWrite && c.CanRead && c.PropertyType.IsValueType);
            colFieldName.ItemsSource = fields;
            colFieldName.DisplayMemberPath = "Name";
            colFieldName.SelectedValuePath = "Name";

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += (s, e) =>
            {
                try
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        while (true)
                        {
                            Thread.Sleep(100);
                            if (this.DataContext != null )
                            {
                                break;
                            }
                        }
                        foreach (var item in dataGrid.Items)
                        {
                            DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                            if (row != null)
                            {
                                var node = item as ParameterNode;
                                if (node == null|| string.IsNullOrEmpty(node.colorStr))
                                {
                                    continue;
                                }
                                string[] colorStr = node.colorStr.Split(',');
                                System.Windows.Media.Color c = System.Windows.Media.Color.FromArgb(255, byte.Parse(colorStr[0]), byte.Parse(colorStr[1]), byte.Parse(colorStr[2]));
                                SolidColorBrush b = new SolidColorBrush(c);
                                System.Windows.Controls.DataGridCell cell = dataGrid.Columns[dataGrid.Columns.Count - 2].GetCellContent(row)?.Parent as DataGridCell;
                                cell.Background = b;
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    string msg = $"{ex.Message}\r\n{ex.StackTrace}";
                    LogHelper.Error(msg);
                }
                finally
                {

                }

            };
            worker.RunWorkerAsync();

        }

        private void DataGridMain_OnLayoutUpdated(object sender, EventArgs e)
        {
            dataGrid.RowHeight = double.NaN;
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (e.Column.Header.ToString() == "类型")
            {
                //FeedGradientInfo info = dataGrid.SelectedCells[0].Item as FeedGradientInfo;
                //if (info != null)
                    //如果是颜色列，弹窗颜色对话框
                    ColorDialog colorDialog = new ColorDialog();
                //colorDialog.Color = Color.Red;
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                }
            }
        }
        private void ColorSelected_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedCells.Count > 0)
            {
                ParameterNode p = (ParameterNode)dataGrid.SelectedCells[0].Item;
                if (p != null)
                {
                    ColorDialog colorDialog = new ColorDialog();
                    string[] colors = p.colorStr?.Split(',');
                    if (colors == null || colors.Length < 3)
                    {
                        colors = new string[3] { "0", "0", "0" };
                    }
                    colorDialog.Color = Color.FromArgb(255, int.Parse(colors[0]), int.Parse(colors[1]), int.Parse(colors[2]));
                    if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        p.colorStr = $"{colorDialog.Color.R},{colorDialog.Color.G},{colorDialog.Color.B}";
                        var rowIndex = dataGrid.Items.IndexOf(p);
  

                        DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
                        if (row != null)
                        {
                            // 获取单元格（需引用 PresentationFramework.dll）  
                            System.Windows.Controls.DataGridCell cell = dataGrid.Columns[dataGrid.Columns.Count - 2].GetCellContent(row)?.Parent as DataGridCell;
                            if (cell != null)
                            {
                                string[] colorStr = p.colorStr.Split(',');
                                System.Windows.Media.Color c = System.Windows.Media.Color.FromArgb(255, byte.Parse(colorStr[0]), byte.Parse(colorStr[1]), byte.Parse(colorStr[2]));
                                SolidColorBrush b = new SolidColorBrush(c);
                                cell.Background = b;
                            }
                        }
                    }
                }
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }


}
