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
using System.Runtime.InteropServices;
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
        EnhancedSqliteBackupService backupService;

        // 设置高性能模式（需调用Windows API）
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(uint esFlags);
        const uint ES_CONTINUOUS = 0x80000000;
        const uint ES_SYSTEM_REQUIRED = 0x00000001;



        protected override Window CreateShell()
        {
            var softwarePlatform = VarConfig.GetValue("SoftwarePlatform")?.ToString();
            Enum.TryParse(typeof(SoftwarePlatform), softwarePlatform, out var result);
            if (result == null)
            {
                result = SoftwarePlatform.Default;
            }
            switch ((SoftwarePlatform)result)
            {
                case SoftwarePlatform.WindowsPad:
                    VarConfig.SetValue("SoftwarePlatform", SoftwarePlatform.WindowsPad);
                    return Container.Resolve<PadMainView>();
                default:
                    VarConfig.SetValue("SoftwarePlatform", SoftwarePlatform.Default);
                    return Container.Resolve<NewMainView>();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DeviceManager.GetInstance();

            string mutexName = "RD3";
            mutex = new Mutex(true, mutexName, out createdNew);
            if (createdNew)
            {
                //使用CPU高性能模式
                SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED);

                FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
                DispatcherUnhandledException += App_DispatcherUnhandledException;
                mutex.ReleaseMutex();
                base.OnStartup(e);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("应用程序已经在运行！", "温馨提示", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning, System.Windows.Forms.MessageBoxDefaultButton.Button1, System.Windows.Forms.MessageBoxOptions.DefaultDesktopOnly | System.Windows.Forms.MessageBoxOptions.ServiceNotification);
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
            var softwarePlatform = VarConfig.GetValue("SoftwarePlatform")?.ToString();
            Enum.TryParse(typeof(SoftwarePlatform), softwarePlatform, out var result);
            if (result == null)
            {
                result = SoftwarePlatform.Default;
            }

            switch ((SoftwarePlatform)result)
            {
                case SoftwarePlatform.Default:
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
                    break;
            }
        }

        protected override void OnInitialized()
        {
            RD3SQLHelper.InitDB();
            PropertyInfo[] propertyInfos = typeof(RealTimeParam).GetProperties().Where(c => c.CanWrite && c.CanRead && (c.PropertyType == typeof(double) || c.PropertyType == typeof(float) || c.PropertyType == typeof(int) || c.PropertyType == typeof(string))).ToArray();
            RD3SQLHelper.CreateRealTimeParamTable1(propertyInfos);

            UserManager.GetInstance();
            var dialog = Container.Resolve<IDialogService>();

            backupService = new EnhancedSqliteBackupService(@"hisDatas\xzrd3.db", @"D:\DatabaseBackups");
            backupService.Start();

            var softwarePlatform = VarConfig.GetValue("SoftwarePlatform")?.ToString();
            Enum.TryParse(typeof(SoftwarePlatform), softwarePlatform, out var result);
            if (result == null)
            {
                result = SoftwarePlatform.Default;
            }

            switch ((SoftwarePlatform)result)
            {
                case SoftwarePlatform.Default:
                    dialog.ShowDialog(nameof(LoginView), callback =>
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
                    break;
                case SoftwarePlatform.WindowsPad:
                    break;
            }

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
            containerRegistry.RegisterForNavigation<PadMainView, PadMainViewModel>();
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

            backupService?.Dispose();

            SQLiteHelper.Close();

            base.OnExit(e);
        }
    }
}
