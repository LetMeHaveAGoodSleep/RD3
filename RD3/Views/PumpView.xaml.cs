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
