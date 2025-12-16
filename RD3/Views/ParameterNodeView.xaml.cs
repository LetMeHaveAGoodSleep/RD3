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
using ScottPlot.PathStrategies;
using ScottPlot.Hatches;
using System.Threading;
using System.Windows.Forms;

namespace RD3.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class ParameterNodeView : System.Windows.Controls.UserControl
    {
        public ParameterNodeView(IEventAggregator aggregator, IDialogHostService dialogHostService, IContainerProvider containerProvider)
        {
            InitializeComponent();
        }
    }


}
