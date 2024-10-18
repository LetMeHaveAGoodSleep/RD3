using HandyControl.Data;
using Microsoft.Win32;
using Newtonsoft.Json;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class BatchViewModel : NavigationViewModel
    {
        private bool _isTemplate = false;
        public bool IsTemplate
        {
            get { return _isTemplate; }
            set { SetProperty(ref _isTemplate, value); }
        }

        private ObservableCollection<ProjectTemplate> _projectTemplates = [];
        public ObservableCollection<ProjectTemplate> ProjectTemplates { get { return _projectTemplates; } set { SetProperty(ref _projectTemplates, value); } }

        private ObservableCollection<ProjectTemplate> _projectTemplate = [];
        public ObservableCollection<ProjectTemplate> ProjectTemplateCol { get { return _projectTemplates; } set { SetProperty(ref _projectTemplates, value); } }

        private readonly IDialogService dialogService;

        public string Title => AppSession.CompanyName;

        public event Action<IDialogResult> RequestClose;

        private Batch CurrentBatch;

        private ObservableCollection<Batch> _batches = new ObservableCollection<Batch>();
        public ObservableCollection<Batch> Batches { get { return _batches; } set { SetProperty(ref _batches, value); } }

        private ObservableCollection<Batch> _batchCol = new ObservableCollection<Batch>();
        public ObservableCollection<Batch> BatchCol { get { return _batchCol; } set { SetProperty(ref _batchCol, value); } }

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
            if (!IsTemplate)
            {
                string Id = DateTime.Now.ToString("yyyyMMdd");
                int index = 1;
                var result = BatchManager.GetInstance().Batches.ToList().FindLast(t => t.Id.StartsWith(Id));
                try
                {
                    index = Convert.ToInt32(result?.Id.Substring(8, 2)) + 1;
                }
                catch (Exception ex)
                {

                }
                Id += index.ToString().PadLeft(2, '0');
                Batch batch = new Batch() { Id = Id, EndTime = DateTime.Now.AddDays(1), StartTime = DateTime.Now, Status = "Add" };
                DialogParameters pairs = new DialogParameters
            {
                { "Batch", batch },
                {"Mode", OpenMode.Add  }
            };
                dialogService?.ShowDialog(nameof(EditBatchView), pairs, callback =>
                {
                    if (callback.Result != ButtonResult.OK)
                    {
                        return;
                    }
                    Batches.Add(batch);
                    BatchManager.GetInstance().Save(Batches);
                    PageIndex = 1;
                    PageUpdated(new FunctionEventArgs<int>(1));
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
                    PageIndex = 1;
                    PageUpdated(new FunctionEventArgs<int>(1));
                });
            }
        });

        public DelegateCommand<Batch> CompareCommand => new((Batch batch) =>
        {

        });

        public DelegateCommand<object> ExportCommand => new((object o) =>
        {
            if (!IsTemplate)//批次信息
            {
                var csv = new StringBuilder();
                var headers = new List<string>()
                {
                    Language.GetValue("ID").ToString(), Language.GetValue("Name").ToString(),
                    Language.GetValue("Reactor").ToString(),Language.GetValue("Status").ToString(),
                    Language.GetValue("Start Time").ToString(), Language.GetValue("Close Time").ToString(),
                    Language.GetValue("Project").ToString(),Language.GetValue("Description").ToString()
                };
                csv.AppendLine(string.Join(",", headers));

                foreach (var batch in Batches)
                {
                    var rowValues = new List<object>()
                    {
                        batch.Id,
                        batch.Name,
                        batch.Reactor,
                        batch.Status,
                        batch.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        batch.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        batch.Project,
                        batch.Description
                    };
                    csv.AppendLine(string.Join(",", rowValues));
                }
                string fileName = Language.GetValue("Batch").ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    FileName = fileName,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, csv.ToString());
                }
            }
            else//批次参数模板导出
            {
                ProjectTemplate template = o as ProjectTemplate;
                string fileName = "";
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Temp Files (*.template)|*.template",
                    FileName = fileName,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string json = JsonConvert.SerializeObject(template);
                    json = AESEncryption.Encrypt(json);
                    File.WriteAllText(saveFileDialog.FileName, json);
                }
            }
        });

        public DelegateCommand<object> EditCommand => new((object o) =>
        {
            if (o == null) return;
            if (o.GetType().Name == nameof(Batch))
            {
                Batch batch = (Batch)o;
                DialogParameters pairs = new DialogParameters
            {
                { "Batch", batch },
                {"Mode", OpenMode.Edit  }
            };
                dialogService?.ShowDialog("EditBatchView", pairs, callback =>
                {
                    if (callback.Result != ButtonResult.OK)
                    {
                        return;
                    }
                    BatchManager.GetInstance().Save(Batches);
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
            if (o.GetType().Name == nameof(Batch))
            {
                Batch batch = (Batch)o;
                DialogParameters pairs = new DialogParameters
            {
                { "Batch", batch },
                {"Mode", OpenMode.View }
            };
                dialogService?.ShowDialog("EditBatchView", pairs, callback =>
                {
                    if (callback.Result != ButtonResult.OK)
                    {
                        return;
                    }
                });
            }
            else if (o.GetType().Name == nameof(ProjectTemplate))
            {
                ProjectTemplate template = (ProjectTemplate)o;
                DialogParameters pairs = new DialogParameters
                {
                    { "Template", template },
                    { "Mode", OpenMode.View }
                };
                dialogService?.ShowDialog(nameof(EditTemplateView), pairs, callback =>
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
            if (o.GetType().Name == nameof(Batch))
            {
                Batch batch = (Batch)o;
                BatchCol.Remove(batch);
                Batches.Remove(batch);
                BatchManager.GetInstance().Save(Batches);
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
                    Batches = new ObservableCollection<Batch>(BatchManager.GetInstance().Batches);
                }
                else
                {
                    var collection = Batches.Where(t => t.Id.Contains(key) || t.Name.Contains(key) || t.Reactor.Contains(key)
                    || t.Status.Contains(key) || t.Project.Contains(key) || t.Description.Contains(key));
                    Batches = new ObservableCollection<Batch>(collection);
                }
                PageCount = Batches.Count / DataCountPerPage + (Batches.Count % DataCountPerPage != 0 ? 1 : 0);
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

        public BatchViewModel(IContainerProvider containerProvider, IDialogService dialog) : base(containerProvider)
        {
            PageIndex = 1;
            dialogService = dialog;
            Batches = new ObservableCollection<Batch>(BatchManager.GetInstance().Batches);
            ProjectTemplates = new ObservableCollection<ProjectTemplate>(ProjectTemplateManager.GetInstance().Templates);
            PageCount = Batches.Count / DataCountPerPage + (Batches.Count % DataCountPerPage != 0 ? 1 : 0);
            var data = Batches.Take(DataCountPerPage);
            BatchCol = new ObservableCollection<Batch>(data);
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
                PageCount = Batches.Count / DataCountPerPage + (Batches.Count % DataCountPerPage != 0 ? 1 : 0);
                var data = Batches.Skip((info.Info - 1) * DataCountPerPage).Take(DataCountPerPage);
                BatchCol = new ObservableCollection<Batch>(data);
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
