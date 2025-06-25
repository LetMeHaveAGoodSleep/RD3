using DryIoc;
using Fpi.Util.Interfaces.Initialize;
using log4net;
using MaterialDesignThemes.Wpf;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using RD3.ViewModels;
using RD3.Views;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using XZ.SQLite;

namespace RD3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        static Mutex mutex;
        bool createdNew;

        protected override Window CreateShell()
        {
            return Container.Resolve<NewMainView>();
            //return Container.Resolve<MainView>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DeviceManager.GetInstance();

            string mutexName = "RD3";
            mutex = new Mutex(true, mutexName, out createdNew);
            if (createdNew)
            {
                FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
                DispatcherUnhandledException += App_DispatcherUnhandledException;
                mutex.ReleaseMutex();
                base.OnStartup(e);
            }
            else
            {
                MessageBox.Show("应用程序已经在运行！");
                Application.Current.Shutdown();
                Environment.Exit(0);
                return;
            }

        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogHelper.Error(e.Exception+"  "+ e.Exception.StackTrace);
            e.Handled = true;
        }

        public static void LoginOut(IContainerProvider containerProvider)
        {
            Current.MainWindow.Hide();

            var dialog = containerProvider.Resolve<IDialogService>();

            dialog.ShowDialog(nameof(LoginView), callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    Environment.Exit(0);
                    return;
                }
            });

            var service = App.Current.MainWindow.DataContext as IConfigureService;
            if (service != null)
                service.Configure();

            dialog.ShowDialog(nameof(SelfCheckView), callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    Environment.Exit(0);
                    return;
                }
            });

            Current.MainWindow.Show();
        }

        protected override void OnInitialized()
        {
            RD3SQLHelper.InitDB();
            PropertyInfo[] propertyInfos = typeof(RealTimeParam).GetProperties().Where(c => c.CanWrite && c.CanRead && (c.PropertyType == typeof(double) || c.PropertyType == typeof(float) || c.PropertyType == typeof(int) || c.PropertyType == typeof(string))).ToArray();
            RD3SQLHelper.CreateRealTimeParamTable1(propertyInfos);

            // 创建索引以提高查询效率
            SQLiteHelper.CreateIndex("realTimeParamTable1", "deviceID");
            SQLiteHelper.CreateIndex("realTimeParamTable1", "batchID");
            SQLiteHelper.CreateIndex("realTimeParamTable1", "dateTime");

            UserManager.GetInstance();
            var dialog = Container.Resolve<IDialogService>();
            dialog.ShowDialog(nameof(LoginView),callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    Environment.Exit(0);
                    return;
                }
            });

            dialog.ShowDialog(nameof(SelfCheckView), callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    Environment.Exit(0);
                    return;
                }
            });

            var service = App.Current.MainWindow.DataContext as IConfigureService;
            if (service != null)
                service.Configure();
            //初始化加载
            FunctionManager.GetInstance();
            //CommandManager.GetInstance();
            AlarmManager.GetInstance();
            //AlarmLogger.GetInstance();
            //DeviceManager.GetInstance();

            base.OnInitialized();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDialogHostService, DialogHostService>();
            containerRegistry.RegisterForNavigation<LoginView, LoginViewModel>();
            containerRegistry.RegisterForNavigation<AboutView>();
            containerRegistry.RegisterForNavigation<MsgView, MsgViewModel>();
            containerRegistry.RegisterDialog<UserView, UserViewModel>();
            containerRegistry.RegisterDialog<EditUserView, EditUserViewModel>();
            containerRegistry.RegisterForNavigation<SampleView, SampleViewModel>();
            containerRegistry.RegisterForNavigation<CalibrationView, CalibrateViewModel>(); 
            containerRegistry.RegisterDialog<SelfCheckView, SelfCheckViewModel>();
            containerRegistry.RegisterForNavigation<ErrorView, ErrorViewModel>();
            containerRegistry.RegisterInstance<ILanguage>(new XZLanguage());
            containerRegistry.RegisterInstance<IInitializationListener>(new InitializationListener()); 
            containerRegistry.RegisterForNavigation<MCUDebugView, MCUDebugViewModel>();
            containerRegistry.RegisterForNavigation<SettingView, SettingViewModel>();
            containerRegistry.RegisterDialog<RegisterView, RegisterViewModel>();
            containerRegistry.RegisterDialog<DOEMainView, DOEMainViewModel>();
            containerRegistry.RegisterDialog<DOEDesignView, DOEDesignViewModel>();
            containerRegistry.RegisterDialog<ChooseReactorView, ChooseReactorViewModel>();
            containerRegistry.RegisterDialog<AboutView, AboutViewModel>();
            containerRegistry.RegisterDialog<EditBatchIDView, EditBatchIDViewModel>();
            containerRegistry.RegisterDialog<CameraImageView, CameraImageViewModel>();
            containerRegistry.RegisterDialog<ReactorView, ReactorViewModel>();
            containerRegistry.RegisterSingleton<ReactorViewModel>();
            containerRegistry.RegisterDialog<TimeSeriesView, TimeSeriesViewModel>();
            containerRegistry.RegisterDialog<PumpMFCView, PumpMFCViewModel>();
            containerRegistry.RegisterDialog<CompareBatchView, CompareBatchViewModel>();
            containerRegistry.RegisterDialog<AddOffLineDataView, AddOffLineDataViewModel>();
            containerRegistry.RegisterDialog<AddOfflineDatasView, AddOffLineDatasViewModel>();//add by hdb 批量录入离线数据
            containerRegistry.RegisterDialog<NewBatchView, NewBatchViewModel>();
            containerRegistry.RegisterDialog<DOAssociateView, DOAssociateViewModel>();

            containerRegistry.RegisterDialog<PIDView, PIDViewModel>();
            containerRegistry.RegisterDialog<ParameterView, ParameterViewModel>();
            containerRegistry.RegisterDialog<ParameterSourceView, ParameterSourceViewModel>();
            containerRegistry.RegisterDialog<OffLineDatasView, OffLineDatasViewModel>();
            containerRegistry.RegisterDialog<NewFeedView, NewFeedViewModel>();
            containerRegistry.RegisterDialog<FeedGradientView, FeedGradientViewModel>();
            containerRegistry.RegisterDialog<AlarmThresholdView, AlarmThresholdViewModel>();
            containerRegistry.RegisterDialog<FeedStrategyView, FeedStrategyViewModel>();

            containerRegistry.RegisterDialog<OutputBatchDataView, OutputBatchDataViewModel>();//add by hdb 批量录入离线数据
            containerRegistry.RegisterDialog<DeviceNodesView, DeviceNodesViewModel>();

            containerRegistry.RegisterDialog<PHControlView, PHControlViewModel>();
            containerRegistry.RegisterDialog<DOTimeSeriesView, DOTimeSeriesViewModel>();

            containerRegistry.RegisterDialog<AuditView, AuditViewModel>();
            containerRegistry.RegisterDialog<WarnParamView, WarnParamViewModel>();
            containerRegistry.RegisterDialog<AddBatchInfoView, AddBatchInfoViewModel>();
            containerRegistry.RegisterDialog<DOControlStrategyView, DOControlStrategyViewModel>();
            containerRegistry.RegisterDialog<ProbView, ProbViewModel>();
            containerRegistry.RegisterDialog<AdaptpHView, AdaptpHViewModel>();
            //containerRegistry.RegisterDialogWindow<DialogWindowBase>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 关闭所有窗口
            foreach (Window window in Windows)
            {
                window.Close();
            }

            GC.WaitForPendingFinalizers();
            GC.Collect();

            base.OnExit(e);
        }
    }
}
