using HandyControl.Data;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using RD3.Views;
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
        private bool _isTemplate = false;
        public bool IsTemplate
        {
            get { return _isTemplate; }
            set { SetProperty(ref _isTemplate, value); }
        }

        private readonly IDialogService dialogService;

        public string Title => AppSession.CompanyName;

        public event Action<IDialogResult> RequestClose;

        private Project CurrentProject;

        private ObservableCollection<Project> _projects = [];
        public ObservableCollection<Project> Projects { get { return _projects; } set { SetProperty(ref _projects, value); } }

        private ObservableCollection<Project> _projectCol = [];
        public ObservableCollection<Project> ProjectCol { get { return _projectCol; } set { SetProperty(ref _projectCol, value); } }

        private ProjectTemplate CurrentProjectTemplate;

        private ObservableCollection<ProjectTemplate> _projectTemplates = [];
        public ObservableCollection<ProjectTemplate> ProjectTemplates { get { return _projectTemplates; } set { SetProperty(ref _projectTemplates, value); } }

        private ObservableCollection<ProjectTemplate> _projectTemplate = [];
        public ObservableCollection<ProjectTemplate> ProjectTemplateCol { get { return _projectTemplates; } set { SetProperty(ref _projectTemplates, value); } }

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

        public DelegateCommand AddCommand => new(() =>
        {
            if (!_isTemplate)
            {
                Project project = new() { StartDate = DateTime.Now, CloseDate = DateTime.Now.AddDays(1) };
                DialogParameters pairs = new()
                {
                    { "Project", project },
                    {"Mode", OpenMode.Add  }
                };
                dialogService?.ShowDialog(nameof(EditProjectView), pairs, callback =>
                {
                    if (callback.Result != ButtonResult.OK)
                    {
                        return;
                    }
                    Projects.Add(project);
                    ProjectManager.GetInstance().Save(Projects);
                    PageUpdated(new FunctionEventArgs<int>(PageIndex));
                });
            }
            else
            {
                ProjectTemplate template = new() { UsageTime = 0 };
                DialogParameters pairs = new()
                {
                    { "Template", template },
                    {"Mode", OpenMode.Add  }
                };
                dialogService?.ShowDialog(nameof(EditTemplateView), pairs, callback =>
                {
                    if (callback.Result != ButtonResult.OK)
                    {
                        return;
                    }
                    ProjectTemplates.Add(template);
                    ProjectTemplateManager.GetInstance().Save(ProjectTemplates);
                    PageUpdated(new FunctionEventArgs<int>(PageIndex));
                });
            }
        });

        public DelegateCommand<object> EditCommand => new((object o) =>
        {
            if (o == null) return;
            if (o.GetType().Name == nameof(Project))
            {
                Project project = (Project)o;
                DialogParameters pairs = new DialogParameters
                {
                    { "Project", project },
                    { "Mode", OpenMode.Edit }
                };
                dialogService?.ShowDialog(nameof(EditProjectView), pairs, callback =>
                {
                    if (callback.Result != ButtonResult.OK)
                    {
                        return;
                    }
                    ProjectManager.GetInstance().Save(Projects);
                });
            }
            else if (o.GetType().Name == nameof(ProjectTemplate))
            {
                ProjectTemplate template = (ProjectTemplate)o;
                DialogParameters pairs = new DialogParameters
                {
                    { "Template", template },
                    { "Mode", OpenMode.Edit }
                };
                dialogService?.ShowDialog(nameof(EditTemplateView), pairs, callback =>
                {
                    if (callback.Result != ButtonResult.OK)
                    {
                        return;
                    }
                    ProjectTemplateManager.GetInstance().Save(ProjectTemplates);
                });
            }
        });

        public DelegateCommand<object> ViewCommand => new((object o) =>
        {

            if (o == null) return;
            if (o.GetType().Name == nameof(ProjectTemplate))
            {
                ProjectTemplate template = (ProjectTemplate)o;
                DialogParameters pairs = new DialogParameters
                {
                    { "Template", template },
                    { "Mode", OpenMode.View  }
                };
                dialogService?.ShowDialog(nameof(EditTemplateView), pairs, callback =>
                {
                    if (callback.Result != ButtonResult.OK)
                    {
                        return;
                    }
                });
            }
            else if (o.GetType().Name == nameof(Project))
            {
                Project project = (Project)o;
                DialogParameters pairs = new DialogParameters
                {
                    { "Project", project },
                    { "Mode", OpenMode.View }
                };
                dialogService?.ShowDialog(nameof(EditProjectView), pairs, callback =>
                {
                    if (callback.Result != ButtonResult.OK)
                    {
                        return;
                    }
                });
            }
        });

        public DelegateCommand<object> DeleteCommand => new((object o) =>
        {
            if (o == null) return;
            if (o.GetType().Name == nameof(Project))
            {
                Project project = (Project)o;
                ProjectCol.Remove(project);
                Projects.Remove(project);
                ProjectManager.GetInstance().Save(Projects);
            }
            else if (o.GetType().Name == nameof(ProjectTemplate))
            {
                ProjectTemplate template = (ProjectTemplate)o;
                ProjectTemplateCol.Remove(template);
                ProjectTemplates.Remove(template);
                ProjectTemplateManager.GetInstance().Save(ProjectTemplates);
            }
        });

        public DelegateCommand<FunctionEventArgs<string>> SearchCommand => new((FunctionEventArgs<string> e) =>
        {
            string key = e.Info;
            if (!IsTemplate)
            {
                if (string.IsNullOrEmpty(key))
                {
                    Projects = new ObservableCollection<Project>(ProjectManager.GetInstance().Projects);
                }
                else
                {
                    var collection = Projects.Where(t => t.Name.Contains(key) || t.Account.Contains(key)
                    || t.Client.Contains(key) || t.Creator.Contains(key) || t.Description.Contains(key));
                    Projects = new ObservableCollection<Project>(collection);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(key))
                {
                    ProjectTemplates = new ObservableCollection<ProjectTemplate>(ProjectTemplateManager.GetInstance().Templates);
                }
                else
                {
                    var collection = ProjectTemplates.Where(t => t.Name.Contains(key) || t.Creator.Contains(key)
                    || t.UsageTime.ToString().Contains(key));
                    ProjectTemplates = new ObservableCollection<ProjectTemplate>(collection);
                }
            }
            PageUpdated(new FunctionEventArgs<int>(PageIndex));
        });

        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand => new(PageUpdated);

        public ProjectViewModel(IContainerProvider containerProvider, IDialogService dialog, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            PageIndex = 1;
            dialogService = dialog;
            Projects = new ObservableCollection<Project>(ProjectManager.GetInstance().Projects);
            ProjectTemplates = new ObservableCollection<ProjectTemplate>(ProjectTemplateManager.GetInstance().Templates);
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
            if (!IsTemplate)
            {
                PageCount = Projects.Count / DataCountPerPage + (Projects.Count % DataCountPerPage != 0 ? 1 : 0);
                var data = Projects.Skip((info.Info - 1) * DataCountPerPage).Take(DataCountPerPage);
                ProjectCol = new ObservableCollection<Project>(data);
            }
            else 
            {
                PageCount = ProjectTemplates.Count / DataCountPerPage + (ProjectTemplates.Count % DataCountPerPage != 0 ? 1 : 0);
                var data = ProjectTemplates.Skip((info.Info - 1) * DataCountPerPage).Take(DataCountPerPage);
                ProjectTemplateCol = new ObservableCollection<ProjectTemplate>(data);
            }
        }
    }
}
