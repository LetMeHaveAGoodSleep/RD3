using ScottPlot.WPF;
using ScottPlot;
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
using RD3.Shared;
using RD3.ViewModels;
using Prism.Events;
using Prism.Ioc;
using RD3.Common;
using RD3.Extensions;
using Fpi.Communication.Manager;
using RD3.Common.Events;
using ScottPlot.AxisPanels;
using ScottPlot.Plottables;
using ImTools;
using HandyControl.Controls;
using System.Collections.ObjectModel;
using Fpi.Instruments;
using ScottPlot.TickGenerators;
using System.Windows.Controls.Primitives;
using Microsoft.FSharp.Core;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RD3.Views
{
    /// <summary>
    /// CalibrationView.xaml 的交互逻辑
    /// </summary>
    public partial class CalibrationView : UserControl
    {
        private SubscriptionToken token1 = null;

        private SubscriptionToken token2 = null;

        private SubscriptionToken token3 = null;


        IEventAggregator aggregator;

        //Dictionary<CalibrationModule, Dictionary<string, List<Coordinates>>> dictionary = new();

        Dictionary<CalibrationModule, string> dicUnit = new Dictionary<CalibrationModule, string>()
        {
             { CalibrationModule.Temp, "(℃)" },
             { CalibrationModule.PH, "" },
              { CalibrationModule.DO, "(%)" },
               { CalibrationModule.Bottle1Weight, "(g)" },
                { CalibrationModule.Bottle2Weight, "(g)" },
                { CalibrationModule.JarWeight, "(g)" },
        };

        Dictionary<CalibrationModule, string> dicAxisName = new Dictionary<CalibrationModule, string>()
        {
             { CalibrationModule.Temp, "温度" },
             { CalibrationModule.PH, "PH" },
              { CalibrationModule.DO, "溶氧" },
               { CalibrationModule.Bottle1Weight, "称重1" },
                { CalibrationModule.Bottle2Weight, "称重2" },
                { CalibrationModule.JarWeight, "罐体称重" },
        };

        private Dictionary<ExperimentParameter, double[]> dicScale = new Dictionary<ExperimentParameter, double[]>
        {
            { ExperimentParameter.Air, [0, 10] },
             { ExperimentParameter.DO, [0, 100] },
             { ExperimentParameter.Temp, [0, 200] },
             { ExperimentParameter.PH, [0, 13] },
            {ExperimentParameter.Agit,[0,2000] }
        };


        public CalibrationView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            aggregator = eventAggregator;
        }
    }
}
