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

namespace RD3.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        private readonly IDialogHostService dialogHostService;

        public MainView(IEventAggregator aggregator, IDialogHostService dialogHostService,IContainerProvider containerProvider)
        {
            InitializeComponent();

            //注册等待消息窗口
            aggregator.Resgiter(arg =>
            {
                DialogHost.IsOpen = arg.IsOpen;

                if (DialogHost.IsOpen)
                    DialogHost.DialogContent = new ProgressView();
            });

            aggregator.ResgiterMessage((MessageModel model) => 
            {
                ILanguage language = containerProvider.Resolve<ILanguage>();
                
                List<Function> functions = FunctionManager.GetInstance().Functions.Where(t => (uint)t.MinUserType <= AppSession.CurrentUser.Role)?.ToList();
                sideMenu.Items.Clear();
                foreach (Function function in functions) 
                {
                    SideMenuItem sideMenuItem = new SideMenuItem();
                    Image image = new Image();
                    image.Source = new BitmapImage(new Uri("Pack://application:,,," + string.Format("/Images/{0}.png",function.Name)));
                    image.Width = image.Height = 24;
                    image.Margin = new Thickness(0, -5, -15, 0);
                    sideMenuItem.Icon = image;
                    sideMenuItem.Header = language.GetValue(function.Name)?.ToString();
                    sideMenuItem.Command = ((MainViewModel)this.DataContext).SelectCmd;
                    sideMenuItem.CommandParameter = sideMenuItem.Header;
                    sideMenuItem.Margin = new Thickness(0, 0, 0, 10);
                    sideMenuItem.FontSize = 16;
                    sideMenu.Items.Add(sideMenuItem);
                }
                //foreach (SideMenuItem item in sideMenu.Items)
                //{
                //    int index = functions.FindIndex(t => language.GetValue(t.Name) == item.Header);
                //    if (index < 0)
                //    {
                //        item.Visibility = Visibility.Hidden;
                //    }
                //}
            }, nameof(MainViewModel));

            btnMin.Click += (s, e) => { this.WindowState = WindowState.Minimized; };
            btnMax.Click += (s, e) =>
            {
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Maximized;
            };
            btnClose.Click += async (s, e) =>
            {
                var dialogResult = await dialogHostService.Question("温馨提示", "确认退出系统?");
                if (dialogResult.Result != Prism.Services.Dialogs.ButtonResult.OK) return;
                this.Close();
            };
            this.MouseMove += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    this.DragMove();
            };
            this.dialogHostService = dialogHostService;
        }

        private void BtnUser_Click(object sender, RoutedEventArgs e)
        {
            Pop.IsOpen = true;
        }

    }
}
