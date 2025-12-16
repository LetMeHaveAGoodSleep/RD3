using DryIoc;
using Fpi.Util.Interfaces.Initialize;
using HandyControl.Data;
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using XZ.DB;
using XZ.SQLite;
using static Microsoft.FSharp.Core.ByRefKinds;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace RD3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        Window secondaryWindow;
        static Mutex mutex;
        bool createdNew;
        SoftwarePlatform platform = SoftwarePlatform.Default;

        // 设置高性能模式（需调用Windows API）
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(uint esFlags);
        const uint ES_CONTINUOUS = 0x80000000;
        const uint ES_SYSTEM_REQUIRED = 0x00000001;

        private void StartNewUIThread()
        {
            Thread newUIThread = new Thread(() =>
            {
                // 在新线程中创建窗口
                secondaryWindow = new PadLoadingView(); // 替换为你的实际Window类
                secondaryWindow.Closed += (d, k) =>
                {
                    // 当窗口关闭后马上结束消息循环
                    System.Windows.Threading.Dispatcher.ExitAllFrames();
                };
                secondaryWindow.Show();
                // 启动消息循环，这将保持窗口响应并阻塞在此处，直到Dispatcher被关闭
                System.Windows.Threading.Dispatcher.Run();
            });

            // 必须设置线程为STA模式
            newUIThread.SetApartmentState(ApartmentState.STA);
            newUIThread.IsBackground = true; // 可选：设置为后台线程，主线程关闭时它也会自动终止
            newUIThread.Start();

            System.Threading.Tasks.Task.Run(() => 
            {
                bool isAutoStart = TaskSchedulerAutoStartHelper.IsAutoStartEnabled();
                // 切换自启状态
                bool result = TaskSchedulerAutoStartHelper.SetAutoStart(!isAutoStart);
            });
        }

        protected override Window CreateShell()
        {
            StartNewUIThread();

            switch (platform)
            {
                case SoftwarePlatform.WindowsPad:
                    VarConfig.SetValue("SoftwarePlatform", SoftwarePlatform.WindowsPad);
                    return Container.Resolve<PadIndexView>();
                default:
                    VarConfig.SetValue("SoftwarePlatform", SoftwarePlatform.Default);
                    return Container.Resolve<NewMainView>();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            string exeDir = Path.GetDirectoryName(exePath);
            // 关键：设置当前工作目录
            Directory.SetCurrentDirectory(exeDir);

            DeviceManager.GetInstance();

            var softwarePlatform = VarConfig.GetValue("SoftwarePlatform")?.ToString();
            Enum.TryParse(typeof(SoftwarePlatform), softwarePlatform, out var result);
            if (result == null)
            {
                result = SoftwarePlatform.Default;
            }
            platform = (SoftwarePlatform)result;

            string mutexName = "RD3";
            switch (platform)
            {
                case SoftwarePlatform.Default:
                    mutexName = "RD3";
                    break;
                case SoftwarePlatform.WindowsPad:
                    User user = UserManager.GetInstance().Users?.ToList().Find(t => t.Type == UserType.Admin);
                    user.DevieceIDs = "G01";
                    if (user != null)
                    {
                        AppSession.CurrentUser = user;
                    }
                    mutexName = "RD3_Pad";
                    VarConfig.SetValue("CommunicationProtocol", 1);
                    break;
                case SoftwarePlatform.HighThroughput:
                    mutexName = "RD3_HT";
                    break;
            }

            mutex = new Mutex(true, mutexName, out createdNew);
            if (createdNew|| System.Diagnostics.Debugger.IsAttached)
            {
                try
                {
                    //使用CPU高性能模式
                    SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }

                FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
                DispatcherUnhandledException += App_DispatcherUnhandledException;
                if (createdNew)
                {
                    mutex.ReleaseMutex();
                }

                RD3SQLHelper.InitDB();
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
            SQLiteHelper.Close();

            AnalysisSolution.GetInstance().SaveReactorSetting();

            LogHelper.Fatal(e.Exception + "  " + e.Exception.StackTrace);
            e.Handled = true;

            HandyControl.Controls.MessageBox.Warning("程序出错，请重启软件", "温馨提示");
            Application.Current.Shutdown();
            Environment.Exit(0);
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
            UserManager.GetInstance();
            var dialog = Container.Resolve<IDialogService>();

            var softwarePlatform = VarConfig.GetValue("SoftwarePlatform")?.ToString();
            Enum.TryParse(typeof(SoftwarePlatform), softwarePlatform, out var result);
            if (result == null)
            {
                result = SoftwarePlatform.Default;
            }

            if (secondaryWindow != null)
            {
                secondaryWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    secondaryWindow.Close();
                }));
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
                    //dialog.ShowDialog(nameof(SelfCheckView), callback =>
                    //{
                    //    if (callback.Result != ButtonResult.OK)
                    //    {
                    //        Environment.Exit(0);
                    //        return;
                    //    }
                    //});
                    break;
            }

            var service = App.Current.MainWindow.DataContext as IConfigureService;
            if (service != null)
                service.Configure();
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
            containerRegistry.RegisterForNavigation<TimeSeriesView>();
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
            containerRegistry.RegisterDialog<ParameterNodeView, ParameterNodeViewModel>();

            containerRegistry.RegisterDialog<PHControlView, PHControlViewModel>();
            containerRegistry.RegisterDialog<DOTimeSeriesView, DOTimeSeriesViewModel>();

            containerRegistry.RegisterDialog<AuditView, AuditViewModel>();
            containerRegistry.RegisterDialog<WarnParamView, WarnParamViewModel>();
            containerRegistry.RegisterDialog<AddBatchInfoView, AddBatchInfoViewModel>();
            containerRegistry.RegisterDialog<DOControlStrategyView, DOControlStrategyViewModel>();
            containerRegistry.RegisterDialog<ProbView, ProbViewModel>();
            containerRegistry.RegisterDialog<AdaptpHView, AdaptpHViewModel>();
            containerRegistry.RegisterForNavigation<PadIndexView, PadMainViewModel>();
            containerRegistry.RegisterDialog<PumpSettingView, PumpSettingViewModel>();

            containerRegistry.RegisterDialog<AgitSettingView, AgitSettingViewModel>();
            containerRegistry.RegisterDialog<DOSettingView, DOSettingViewModel>();
            containerRegistry.RegisterDialog<MFCSettingView, MFCSettingViewModel>();
            containerRegistry.RegisterDialog<TempSettingView, TempSettingViewModel>();
            containerRegistry.RegisterDialog<pHSettingView, pHSettingViewModel>();
            containerRegistry.RegisterDialog<PadCalibrationView, PadCalibrationViewModel>();
            containerRegistry.RegisterDialog<PadConfigurationView, PadConfigurationViewModel>();
            containerRegistry.RegisterDialog<PadDOPIDView, PadDOPIDViewModel>();
            containerRegistry.RegisterDialog<ChooseDOEAnalyseMethodView, ChooseDOEAnalyseMethodViewModel>();
            containerRegistry.RegisterDialog<DOEAnalyse2DView, DOEAnalyse2DViewModel>();
            containerRegistry.RegisterDialog<ChooseDOE3DFactorView, ChooseDOE3DFactorViewModel>();
            containerRegistry.RegisterDialog<DOEAnalyse3DView, DOEAnalyse3DViewModel>();
            containerRegistry.RegisterDialog<DOEFactorView, DOEFactorViewModel>();
            //containerRegistry.RegisterDialogWindow<DialogWindowBase>();

            containerRegistry.RegisterForNavigation<CondensationView, CondensationViewModel>();
            containerRegistry.RegisterDialog<CondensationSettingView, CondensationSettingViewModel>();
            containerRegistry.RegisterForNavigation<EPCView, EPCViewModel>();
            containerRegistry.RegisterDialog<EPCSettingView, EPCSettingViewModel>();
            containerRegistry.RegisterDialog<RealTimeDataView, RealTimeDataViewModel>();
            containerRegistry.RegisterDialog<ChooseRTParamView, ChooseRTParamViewModel>();
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

            SQLiteHelper.Close();

            AnalysisSolution.GetInstance().SaveReactorSetting();

            base.OnExit(e);
        }
    }
}
