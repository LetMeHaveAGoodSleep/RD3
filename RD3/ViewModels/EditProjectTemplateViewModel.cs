using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Extensions;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RD3.ViewModels
{
    public class EditProjectTemplateViewModel : NavigationViewModel, IDialogAware
    {
        private OpenMode _mode;

        private bool _enable;
        public bool Enable
        {
            get { return _enable; }
            set { SetProperty(ref _enable, value); }
        }
        private ProjectTemplate _template;
        public ProjectTemplate Template
        {
            get { return _template; }
            set { SetProperty(ref _template, value); }
        }

        public string Title => "";

        public event Action<IDialogResult> RequestClose;

        public DelegateCommand OKCommand => new(() =>
        {
            CheckContent();
            if(_mode == OpenMode.Add)
            {
                Template.Creator = AppSession.CurrentUser.UserName;
                Template.CreatDate = DateTime.Now;
            }
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        });

        private void CheckContent()
        {

            //if (string.IsNullOrWhiteSpace(Template.PH.ToString()))
            //{
            //    MessageBox.Show(Language.GetValue("名字不能为空").ToString());
            //    return;
            //}
        }

        public DelegateCommand CancelCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public EditProjectTemplateViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Template = parameters.GetValue<ProjectTemplate>("Template");
            _mode = parameters.GetValue<OpenMode>("Mode");
            Enable = !(_mode == OpenMode.View);
            aggregator.SendMessage("", nameof(EditProjectTemplateViewModel), Template);
        }
    }
}
