using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
using ImTools;
using Prism.Services.Dialogs;
using RD3.Shared;
using RD3.ViewModels;
using XZ.SQLite;

namespace RD3.Views
{
    /// <summary>
    /// peristalticpump.xaml 的交互逻辑
    /// </summary>
    public partial class PumpView : UserControl
    {
        int pumpIndex = -1;

        public PumpView()
        {
            InitializeComponent();

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (s, e) =>
            {
                while (true)
                {
                    string deviceID = "G01";
                    foreach (var item in ClockSupervisor.realDatasDic.Keys)
                    {
                        deviceID = item;
                        break;
                    }
                    if (!ClockSupervisor.realDatasDic.ContainsKey(deviceID) || ClockSupervisor.realDatasDic[deviceID].Count < 1)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    int index = ClockSupervisor.realDatasDic[deviceID].Count - 1;
                    var realTimeParam = ClockSupervisor.realDatasDic[deviceID][index];
                    PumpInfo pumpInfo = AnalysisSolution.GetInstance().PumpInfoCol.FindFirst(t => t.PumpIndex == pumpIndex);

                    float flowRate = 0;
                    Type type = realTimeParam.GetType();
                    PropertyInfo[] properties = type.GetProperties();
                    foreach (PropertyInfo prop in properties.Where(t => t.CanWrite && t.CanRead))
                    {
                        if (prop.Name == $"Pump{pumpIndex}FlowRate")
                        {
                            object value = prop.GetValue(realTimeParam);
                            flowRate = Convert.ToSingle(value);
                        }
                    }
                    Thread.Sleep(1000);
                }
            };
            backgroundWorker.RunWorkerAsync();
        }

        public void ResumePumpSetting(int index)
        {
            pumpIndex = index;
            var vm = this.DataContext as PumpViewModel;
            var pumpInfo = AnalysisSolution.GetInstance().PumpInfoCol.FindFirst(t => t.PumpIndex == index);
            vm.PumpInfo = pumpInfo;
        }
    }
}
