using DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.ViewModels;
using RD3.Views;
using System;
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
            return Container.Resolve<MainWindow>();
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

                Current.MainWindow.Show();
            });
        }

        protected override void OnInitialized()
        {
            var dialog = Container.Resolve<IDialogService>();
            dialog.ShowDialog("LoginView", callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    Environment.Exit(0);
                    return;
                }

                var service = App.Current.MainWindow.DataContext as IConfigureService;
                if (service != null)
                    service.Configure();
                base.OnInitialized();
            });
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {

            containerRegistry.Register<IDialogHostService, DialogHostService>();

            containerRegistry.RegisterDialog<LoginView, LoginViewModel>();


        }
    }
}
