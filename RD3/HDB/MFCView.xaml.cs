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
using RD3.Shared;
using RD3.ViewModels;

namespace RD3.Views
{
    /// <summary>
    /// peristalticpump.xaml 的交互逻辑
    /// </summary>
    public partial class MFCView : UserControl
    {
        int mfcIndex = -1;

        public MFCView()
        {
            InitializeComponent();
        }

        public void ResumeMFCSetting(int index)
        {
            mfcIndex = index;
            var vm = this.DataContext as MFCViewModel;
            var mfcInfo = AnalysisSolution.GetInstance().MFCInfoCol.FindFirst(t => t.MFCIndex == index);
            vm.MFCInfo = mfcInfo;
        }
    }
}
