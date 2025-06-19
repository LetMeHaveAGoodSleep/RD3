using DryIoc;
using Fpi.Communication.Protocols;
using MaterialDesignThemes.Wpf;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RD3.Common
{
    /// <summary>
    /// 对话主机服务(自定义)
    /// </summary>
    public class DialogHostService : DialogService, IDialogHostService
    {
        private readonly IContainerExtension containerExtension;

        public DialogHostService(IContainerExtension containerExtension) : base(containerExtension)
        {
            this.containerExtension = containerExtension;
        }

        public void ShowOnce(string name, IDialogParameters parameters, Action<IDialogResult> callback, string windowName = "")
        {
            foreach (Window item in Application.Current.Windows)
            {
                if (item.Content == null) continue;
                string formName = item.Content?.ToString().Substring(item.Content.ToString().LastIndexOf(".") + 1);
                if (formName == name?.ToString())
                {
                    item.Activate();
                    item.WindowState = WindowState.Normal;
                    return;
                }
            }
            if (string.IsNullOrWhiteSpace(windowName))
            {
                base.Show(name, parameters, callback);
            }
            else
            {
                base.Show(name, parameters, callback,windowName);
            }
        }

        public async Task<IDialogResult> ShowDialog(string name, IDialogParameters parameters, string dialogHostName = "Root")
        {
            if (parameters == null)
                parameters = new DialogParameters();

            //从容器当中去除弹出窗口的实例
            var content = containerExtension.Resolve<object>(name);

            //验证实例的有效性 
            if (!(content is FrameworkElement dialogContent))
                throw new NullReferenceException("A dialog's content must be a FrameworkElement");

            if (dialogContent is FrameworkElement view && view.DataContext is null && ViewModelLocator.GetAutoWireViewModel(view) is null)
                ViewModelLocator.SetAutoWireViewModel(view, true);

            if (!(dialogContent.DataContext is IDialogHostAware viewModel))
                throw new NullReferenceException("A dialog's ViewModel must implement the IDialogAware interface");

            viewModel.DialogHostName = dialogHostName;

            DialogOpenedEventHandler eventHandler = (sender, eventArgs) =>
            {
                if (viewModel is IDialogHostAware aware)
                {
                    aware.OnDialogOpend(parameters);
                }
                eventArgs.Session.UpdateContent(content);
            };

            return (IDialogResult)await DialogHost.Show(dialogContent, viewModel.DialogHostName, eventHandler);
        }

        public void ShowOnce(string name, Action<IDialogResult> callback, string windowName = "")
        {
            foreach (Window item in Application.Current.Windows)
            {
                if (item.Content == null) continue;
                string formName = item.Content?.ToString().Substring(item.Content.ToString().LastIndexOf(".") + 1);
                if (formName == name?.ToString())
                {
                    item.WindowState = WindowState.Normal;
                    item.Show();
                    item.Activate();
                    return;
                }
            }

            var parameters = new DialogParameters();
            if (string.IsNullOrWhiteSpace(windowName))
            {
                base.Show(name, parameters, callback);
            }
            else
            {
                base.Show(name, parameters, callback, windowName);
            }
        }
    }
}
