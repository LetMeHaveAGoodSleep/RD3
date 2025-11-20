using HandyControl.Data;
using Prism.Events;
using Prism.Ioc;
using RD3.Extensions;
using RD3.Shared;
using RD3.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RD3.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class PIDView : UserControl
    {
        private readonly ILanguage Language;

        private readonly IEventAggregator aggregator;
        public PIDView(IContainerProvider containerProvider, IEventAggregator aggregator)
        {
            InitializeComponent();

            this.Language = containerProvider.Resolve<ILanguage>();
            this.aggregator = aggregator;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_excute_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button; 
            PIDInfo pid = dataGrid.CurrentCell.Item as PIDInfo;
            if (button.Content.ToString() == "开始")
            {
                button.Content = "结束";
                ((PIDViewModel)this.DataContext)?.ExecutePIDCommand.Execute(pid);//开始执行
            }
            else
            {
                button.Content = "开始";
                ((PIDViewModel)this.DataContext)?.StopPIDCommand.Execute(pid);//停止执行
            }
        }

        private void btnExcuteType_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            PIDInfo pid = dataGrid.CurrentCell.Item as PIDInfo;
            if (button.Content.ToString() == "手动")
            {
                pid.SetExecuteType("自动");
                button.Content = "自动";
            }
            else
            {
                pid.SetExecuteType("手动");
                button.Content = "手动";
            }
        }
    }
}
