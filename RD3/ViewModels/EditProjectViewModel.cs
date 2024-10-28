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
    public class EditProjectViewModel : BaseViewModel, IDialogAware
    {
        private OpenMode _mode;

        private bool _enable;
        public bool Enable
        {
            get { return _enable; }
            set { SetProperty(ref _enable, value); }
        }
        private Project _project;
        public Project Project
        {
            get { return _project; }
            set { SetProperty(ref _project, value); }
        }

        public string Title => "";

        public event Action<IDialogResult> RequestClose;

        public DelegateCommand OKCommand => new(() =>
        {
            CheckContent();
            var collection = ProjectManager.GetInstance().Projects.Where(t => t.Name == Project.Name);
            if (collection.Count() > (int)_mode)
            {
                MessageBox.Show(Language.GetValue(string.Format("已存在名称‘{0}’", Project.Name)).ToString());
                return;
            }
            if (_mode == OpenMode.Add)
            {
                Project.Account = Project.Creator = AppSession.CurrentUser.UserName;
                Project.CreatDate = DateTime.Now;
            }
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        });

        private void CheckContent()
        {
            if (string.IsNullOrWhiteSpace(Project.Name))
            {
                MessageBox.Show(Language.GetValue("名字不能为空").ToString());
                return;
            }
            if (string.IsNullOrWhiteSpace(Project.Client))
            {
                MessageBox.Show(Language.GetValue("使用人不能为空").ToString());
                return;
            }
            if (string.IsNullOrWhiteSpace(Project.StartDate.ToString()))
            {
                MessageBox.Show(Language.GetValue("请选择开始日期").ToString());
                return;
            }
            if (string.IsNullOrWhiteSpace(Project.CloseDate.ToString()))
            {
                MessageBox.Show(Language.GetValue("请选择结束日期").ToString());
                return;
            }
        }

        public DelegateCommand CancelCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public EditProjectViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
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
            Project = parameters.GetValue<Project>("Project");
            _mode = parameters.GetValue<OpenMode>("Mode");
            Enable = !(_mode == OpenMode.View);
            aggregator.SendMessage("", nameof(EditProjectViewModel), Project);
        }
    }
}
