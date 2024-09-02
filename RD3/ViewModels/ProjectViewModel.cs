using HandyControl.Data;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class ProjectViewModel : NavigationViewModel, IDialogAware
    {
        private readonly IDialogService dialogService;

        public string Title => AppSession.CompanyName;

        public event Action<IDialogResult> RequestClose;

        private Project CurrentProject;

        private ObservableCollection<Project> _projects = [];
        public ObservableCollection<Project> Projects { get { return _projects; } set { SetProperty(ref _projects, value); } }

        private ObservableCollection<Project> _ProjectCol = new ObservableCollection<Project>();
        public ObservableCollection<Project> ProjectCol { get { return _ProjectCol; } set { SetProperty(ref _ProjectCol, value); } }

        private int _pageCount = 10;
        public int PageCount
        {
            get { return _pageCount; }
            set { SetProperty(ref _pageCount, value); }
        }
        private int _pageIndex;
        public int PageIndex
        {
            get { return _pageIndex; }
            set { SetProperty(ref _pageIndex, value); }
        }

        private int _dataCountPerPage = 10;
        public int DataCountPerPage
        {
            get { return _dataCountPerPage; }
            set { SetProperty(ref _dataCountPerPage, value); }
        }

        public DelegateCommand CloseCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.OK)));

        public DelegateCommand AddProjectCommand => new(() =>
        {
            Project Project = new Project() {  CreatDate = DateTime.Now, StartDate = DateTime.Now,CloseDate=DateTime.Now };
            DialogParameters pairs = new DialogParameters
            {
                { "Project", Project },
                {"Mode", "Add"  }
            };
            dialogService?.ShowDialog("EditProjectView", pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                Projects.Add(Project);
                ProjectManager.GetInstance().Save(Projects);
                PageUpdated(new FunctionEventArgs<int>(PageIndex));
            });
        });

        public DelegateCommand<Project> EditCommand => new((Project Project) =>
        {
            DialogParameters pairs = new DialogParameters
            {
                { "Project", Project },
                {"Mode", "Edit"  }
            };
            dialogService?.ShowDialog("EditProjectView", pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                ProjectManager.GetInstance().Save(Projects);
            });
        });

        public DelegateCommand<Project> ViewCommand => new((Project Project) =>
        {
            DialogParameters pairs = new DialogParameters
            {
                { "Project", Project },
                {"Mode", "View"  }
            };
            dialogService?.ShowDialog("EditProjectView", pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });

        public DelegateCommand<Project> DeleteCommand => new((Project Project) =>
        {
            ProjectCol.Remove(Project);
            Projects.Remove(Project);
            ProjectManager.GetInstance().Save(Projects);
        });

        public DelegateCommand<FunctionEventArgs<string>> SearchCommand => new((FunctionEventArgs<string> e) =>
        {
            string key = e.Info;
            if (string.IsNullOrEmpty(key))
            {
                Projects = new ObservableCollection<Project>(ProjectManager.GetInstance().Projects);
            }
            else
            {
                var collection = Projects.Where(t => t.Name.Contains(key) ||  t.Account.Contains(key)
                || t.Client.Contains(key) || t.Creator.Contains(key) || t.Description.Contains(key));
                Projects = new ObservableCollection<Project>(collection);
            }
            if (PageIndex != 1)
            {
                PageIndex = 1;
            }
            else
            {
                var data = Projects.Take(DataCountPerPage);
                ProjectCol = new ObservableCollection<Project>(data);
            }
        });

        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand => new(PageUpdated);

        public ProjectViewModel(IContainerProvider containerProvider, IDialogService dialog) : base(containerProvider)
        {
            PageIndex = 1;
            dialogService = dialog;
            Projects = new ObservableCollection<Project>(ProjectManager.GetInstance().Projects);
            PageCount = Projects.Count / DataCountPerPage + (Projects.Count % DataCountPerPage != 0 ? 1 : 0);
            var data = Projects.Take(DataCountPerPage);
            ProjectCol = new ObservableCollection<Project>(data);
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

        }

        private void PageUpdated(FunctionEventArgs<int> info)
        {
            var data = Projects.Skip((info.Info - 1) * DataCountPerPage).Take(DataCountPerPage);
            ProjectCol = new ObservableCollection<Project>(data);
        }
    }
}
