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

namespace RD3.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        private readonly IDialogHostService dialogHostService;

        public MainView(IEventAggregator aggregator, IDialogHostService dialogHostService)
        {
            InitializeComponent();

            //注册提示消息
            aggregator.ResgiterMessage(arg =>
            {
                Snackbar.MessageQueue.Enqueue(arg);
            });

            //注册等待消息窗口
            aggregator.Resgiter(arg =>
            {
                DialogHost.IsOpen = arg.IsOpen;

                if (DialogHost.IsOpen)
                    DialogHost.DialogContent = new ProgressView();
            });

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

            //var menuRegister = new List<SubItem>();
            //menuRegister.Add(new SubItem("Customer"));
            //menuRegister.Add(new SubItem("Providers"));
            //menuRegister.Add(new SubItem("Employees"));
            //menuRegister.Add(new SubItem("Products"));
            //var item6 = new ItemMenu("Register", menuRegister, PackIconKind.Register);

            //var menuSchedule = new List<SubItem>();
            //menuSchedule.Add(new SubItem("Services"));
            //menuSchedule.Add(new SubItem("Meetings"));
            //var item1 = new ItemMenu("Appointments", menuSchedule, PackIconKind.Schedule);

            //var menuReports = new List<SubItem>();
            //menuReports.Add(new SubItem("Customers"));
            //menuReports.Add(new SubItem("Providers"));
            //menuReports.Add(new SubItem("Products"));
            //menuReports.Add(new SubItem("Stock"));
            //menuReports.Add(new SubItem("Sales"));
            //var item2 = new ItemMenu("Reports", menuReports, PackIconKind.FileReport);

            //var menuExpenses = new List<SubItem>();
            //menuExpenses.Add(new SubItem("Fixed"));
            //menuExpenses.Add(new SubItem("Variable"));
            //var item3 = new ItemMenu("Expenses", menuExpenses, PackIconKind.ShoppingBasket);

            //var menuFinancial = new List<SubItem>();
            //menuFinancial.Add(new SubItem("Cash flow"));
            //var item4 = new ItemMenu("Financial", menuFinancial, PackIconKind.ScaleBalance);

            //var item0 = new ItemMenu("Dashboard", new UserControl(), PackIconKind.ViewDashboard);

            //Menu.Children.Add(new UserControlMenuItem(item0));
            //Menu.Children.Add(new UserControlMenuItem(item6));
            //Menu.Children.Add(new UserControlMenuItem(item1));
            //Menu.Children.Add(new UserControlMenuItem(item2));
            //Menu.Children.Add(new UserControlMenuItem(item3));
            //Menu.Children.Add(new UserControlMenuItem(item4));
        }
    }
}
