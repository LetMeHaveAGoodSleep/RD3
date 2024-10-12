using ImTools;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
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
using System.Windows.Documents;

namespace RD3.ViewModels
{
    public class EditBatchViewModel : BaseViewModel,IDialogAware
    {
        private OpenMode _mode;

        private Project _project;
        public Project Project
        {
            get { return _project; }
            set { SetProperty(ref _project, value); }
        }

        private Batch _batch;
        public Batch Batch
        {
            get { return _batch; }
            set { SetProperty(ref _batch, value); }
        }

        private bool _enable;
        public bool Enable
        {
            get { return _enable; }
            set { SetProperty(ref _enable, value); }
        }

        private List<string> _reactorList = new List<string>();

        public List<string> ReactorList 
        { 
            get => _reactorList; 
            set => SetProperty(ref _reactorList, value); 
        }

        private ObservableCollection<Project> _projectList = new ObservableCollection<Project>();

        public ObservableCollection<Project> ProjectList
        {
            get => _projectList;
            set => SetProperty(ref _projectList, value);
        }

        public string Title => "";

        public event Action<IDialogResult> RequestClose;

        public DelegateCommand OKCommand => new(() =>
        {
            CheckContent();
            var collection = BatchManager.GetInstance().Batches.Where(t => t.Name == Batch.Name);
            if (collection.Count() > (int)_mode)
            {
                MessageBox.Show(Language.GetValue(string.Format("已存在名称‘{0}’", Batch.Name)).ToString());
                return;
            }
            Batch.Project = Project.Name;
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        });

        private void CheckContent()
        {
            if (string.IsNullOrWhiteSpace(Batch.Name))
            {
                MessageBox.Show(Language.GetValue("名字不能为空").ToString());
                return;
            }
            if (Project == null)
            {
                MessageBox.Show(Language.GetValue("项目不能为空").ToString());
                return;
            }
            if (string.IsNullOrWhiteSpace(Batch.Reactor))
            {
                MessageBox.Show(Language.GetValue("请选择仪器").ToString());
                return;
            }
            if (string.IsNullOrWhiteSpace(Batch.StartTime.ToString()))
            {
                MessageBox.Show(Language.GetValue("请选择开始时间").ToString());
                return;
            }
            if (string.IsNullOrWhiteSpace(Batch.EndTime.ToString()))
            {
                MessageBox.Show(Language.GetValue("请选择结束时间").ToString());
                return;
            }
        }

        public DelegateCommand CancelCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public EditBatchViewModel(IContainerProvider containerProvider) : base(containerProvider)
        {
            _reactorList = new List<string>(DeviceManager.GetInstance().Devices.Select(t => t.Name));
            _projectList = new ObservableCollection<Project>(ProjectManager.GetInstance().Projects);
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
            Batch = parameters.GetValue<Batch>("Batch");
            Project = ProjectManager.GetInstance().Projects.FindFirst(t => t.Name == Batch?.Project);
            _mode = parameters.GetValue<OpenMode>("Mode");
            Enable = !(_mode == OpenMode.View);
            aggregator.SendMessage("", nameof(EditBatchViewModel), Batch);
        }
    }
}
