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
            base.OnStartup(e);
            DispatcherUnhandledException += App_DispatcherUnhandledException;
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
            UserManage.GetInstance();
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
            CommandManager.GetInstance();
            AlarmManager.GetInstance();
            AlarmLogger.GetInstance();
            //for (global::System.Int32 i = 0; i < 100; i++)
            //{
            //    AlarmRecord alarm = new AlarmRecord()
            //    {
            //        Time = DateTime.Now,
            //        Batch = "G0" + i.ToString(),
            //        Reactor = i.ToString(),
            //        Grade = AlarmGrade.Error,
            //        Description = " It's for Test" + i.ToString(),
            //        Value = i.ToString()
            //    };
            //    AlarmLogger.GetInstance().AddAlarm(alarm);
            //}

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
            containerRegistry.RegisterDialog<AddSampleLogView, SampleViewModel>();
            containerRegistry.RegisterForNavigation<ProjectView, ProjectViewModel>();
            containerRegistry.RegisterForNavigation<CalibrateView, CalibrateViewModel>();
            containerRegistry.RegisterDialog<SelfCheckView, SelfCheckViewModel>();
            containerRegistry.RegisterForNavigation<BatchView, BatchViewModel>();
            containerRegistry.RegisterForNavigation<ErrorView, ErrorViewModel>();
            containerRegistry.RegisterForNavigation<AlarmView, AlarmViewModel>();
            containerRegistry.RegisterDialog<CommunicationView, CommunicationViewModel>();
            containerRegistry.RegisterDialog<EditClientView, EditClientViewModel>();
            containerRegistry.RegisterInstance<ILanguage>(new XZLanguage());
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            
        }
    }
}
