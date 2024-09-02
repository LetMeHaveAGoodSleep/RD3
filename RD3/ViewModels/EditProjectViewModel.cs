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
    public class EditProjectViewModel : NavigationViewModel, IDialogAware
    {
        private string _mode;

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
            var collection = BatchManager.GetInstance().Batches.Where(t => t.Name == Project.Name);
            int count = _mode == "Add" ? 1 : 2;
            if (collection.Count() > count)
            {
                MessageBox.Show(Language.GetValue(string.Format("已存在名称‘{0}’", Project.Name)).ToString());
                return;
            }
            Project.Creator = AppSession.CurrentUser.UserName;
            Project.CreatDate = DateTime.Now;
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
                MessageBox.Show(Language.GetValue("请选择开始时间").ToString());
                return;
            }
            if (string.IsNullOrWhiteSpace(Project.CloseDate.ToString()))
            {
                MessageBox.Show(Language.GetValue("请选择结束时间").ToString());
                return;
            }
        }

        public DelegateCommand CancelCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public EditProjectViewModel(IContainerProvider containerProvider) : base(containerProvider)
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
            Project = parameters.GetValue<Project>("Batch");
            _mode = parameters.GetValue<string>("Mode");
            Enable = !(_mode == "View");
            aggregator.SendMessage("", nameof(EditProjectViewModel), Project);
        }
    }
}
