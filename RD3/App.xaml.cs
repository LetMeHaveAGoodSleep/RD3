using DryIoc;
using log4net;
using MaterialDesignThemes.Wpf;
using Prism.DryIoc;
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
using System.Reflection;
using System.Windows;

namespace RD3
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainView>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            base.OnStartup(e); 
        }
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogHelper.Error(e.Exception);
            e.Handled = true;
        }

        public static void LoginOut(IContainerProvider containerProvider)
        {
            Current.MainWindow.Hide();

            var dialog = containerProvider.Resolve<IDialogService>();

            dialog.ShowDialog("LoginView", callback =>
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

            dialog.ShowDialog("SelfCheckView", callback =>
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
            UserManager.GetInstance();
            var dialog = Container.Resolve<IDialogService>();
            dialog.ShowDialog("LoginView", async callback =>
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

            dialog.ShowDialog("SelfCheckView", callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    Environment.Exit(0);
                    return;
                }
            });
            //初始化加载
            FunctionManager.GetInstance();
            CommandManager.GetInstance();
            AlarmManager.GetInstance();
            AlarmLogger.GetInstance();
            DeviceManager.GetInstance();
            CommunicationManager.GetInstance();
            base.OnInitialized();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IDialogHostService, DialogHostService>();
            containerRegistry.RegisterForNavigation<LoginView, LoginViewModel>();
            containerRegistry.RegisterForNavigation<AboutView>();
            containerRegistry.RegisterForNavigation<MsgView, MsgViewModel>();
            containerRegistry.RegisterForNavigation<IndexView, IndexViewModel>();
            containerRegistry.RegisterDialog<UserView, UserViewModel>();
            containerRegistry.RegisterDialog<EditUserView, EditUserViewModel>();
            containerRegistry.RegisterForNavigation<SampleView, SampleViewModel>();
            containerRegistry.RegisterDialog<EditSampleView, EditSampleViewModel>();
            containerRegistry.RegisterForNavigation<ProjectView, ProjectViewModel>();
            containerRegistry.RegisterDialog<EditProjectView, EditProjectViewModel>();
            containerRegistry.RegisterDialog<EditTemplateView, EditProjectTemplateViewModel>();
            containerRegistry.RegisterForNavigation<CalibrationView, CalibrateViewModel>(); 
            containerRegistry.RegisterForNavigation<CalibrateView, CalibrateViewModel>();
            containerRegistry.RegisterDialog<SelfCheckView, SelfCheckViewModel>();
            containerRegistry.RegisterForNavigation<BatchView, BatchViewModel>();
            containerRegistry.RegisterDialog<EditBatchView, EditBatchViewModel>();
            containerRegistry.RegisterForNavigation<ErrorView, ErrorViewModel>();
            containerRegistry.RegisterForNavigation<AlarmView, AlarmViewModel>();
            containerRegistry.RegisterForNavigation<OperationView, OperationViewModel>();
            containerRegistry.RegisterDialog<CommunicationView, CommunicationViewModel>();
            containerRegistry.RegisterDialog<EditClientView, EditClientViewModel>();
            containerRegistry.RegisterInstance<ILanguage>(new XZLanguage());
            containerRegistry.RegisterForNavigation<MCUDebugView, MCUDebugViewModel>();
            containerRegistry.RegisterForNavigation<ControlView, ControlViewModel>();
            containerRegistry.RegisterForNavigation<AddSampleView, AddSampleViewModel>();
            containerRegistry.RegisterDialog<EditExperimentParamView, EditExperimentParamViewModel>();
            containerRegistry.RegisterForNavigation<AuditView, AuditViewModel>();
            containerRegistry.RegisterForNavigation<SettingView, SettingViewModel>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            
        }
    }
}
