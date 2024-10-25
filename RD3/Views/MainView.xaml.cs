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
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace RD3.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        private readonly IDialogHostService dialogHostService;
        readonly ILanguage language;

        public MainView(IEventAggregator aggregator, IDialogHostService dialogHostService, IContainerProvider containerProvider)
        {
            InitializeComponent();

            language = containerProvider.Resolve<ILanguage>();

            //注册等待消息窗口
            aggregator.Resgiter(arg =>
            {
                DialogHost.IsOpen = arg.IsOpen;

                if (DialogHost.IsOpen)
                    DialogHost.DialogContent = new ProgressView();
            });

            aggregator.ResgiterMessage((MessageModel model) =>
            {
                //获取无权限的功能列表
                List<Function> functions = FunctionManager.GetInstance().Functions.Where(t => (uint)t.MinUserType > AppSession.CurrentUser.Role)?.ToList();
                foreach (SideMenuItem item in sideMenu.Items)
                {
                    item.Visibility = Visibility.Visible;
                }
                //sideMenu.Items.Clear();
                foreach (Function function in functions)
                {
                    SideMenuItem sideMenuItem = sideMenu.Items.FindFirst(t => ((SideMenuItem)t).Tag?.ToString() == function.Name) as SideMenuItem;
                    if (sideMenuItem == null) continue;
                    sideMenuItem.Visibility = Visibility.Collapsed;
                }
            }, nameof(MainViewModel));

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
                if (dialogResult.Result != Prism.Services.Dialogs.ButtonResult.OK) return;
                this.Close();
            };
            this.dialogHostService = dialogHostService;
        }

        private void BtnUser_Click(object sender, RoutedEventArgs e)
        {
            Pop.IsOpen = true;
        }

        private void SideMenuItem_MouseLeave(object sender, MouseEventArgs e)
        {
            //SideMenuItem sideMenuItem = (SideMenuItem)sender;
            //if (sideMenuItem == null || sideMenuItem.IsSelected) return;
            //var bitmap = new BitmapImage(new Uri("Pack://application:,,," + string.Format("/Images/SideMenu/{0}.png", sideMenuItem.Tag?.ToString())));
            //if (bitmap != null)
            //{
            //    Image image = new()
            //    {

            //        Source = bitmap
            //    };
            //    image.Width = image.Height = 24;
            //    sideMenuItem.Icon = image;
            //}
            //sideMenuItem.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3d959a"));
        }

        private void SideMenuItem_MouseEnter(object sender, MouseEventArgs e)
        {
            //SideMenuItem sideMenuItem = (SideMenuItem)sender;
            //if (sideMenuItem == null || sideMenuItem.IsSelected) return;
            //var bitmap = new BitmapImage(new Uri("Pack://application:,,," + string.Format("/Images/SideMenu/{0}_Primary.png", sideMenuItem.Tag?.ToString())));
            //if (bitmap != null)
            //{
            //    Image image = new()
            //    {

            //        Source = bitmap
            //    };
            //    image.Width = image.Height = 24;
            //    sideMenuItem.Icon = image;
            //}
            //foreach (var item in sideMenu.Items)
            //{
            //    SideMenuItem menuItem = item as SideMenuItem;
            //    if (menuItem == sideMenuItem) continue;
            //    menuItem.Background = brush;
            //}
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
