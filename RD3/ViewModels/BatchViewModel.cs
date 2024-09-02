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
    public class BatchViewModel : NavigationViewModel
    {
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

        public DelegateCommand AddBatchCommand => new(() =>
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
            Batch batch = new Batch() { Id = Id, EndTime = DateTime.Now, StartTime = DateTime.Now };
            DialogParameters pairs = new DialogParameters
            {
                { "Batch", batch },
                {"Mode", "Add"  }
            };
            dialogService?.ShowDialog("EditBatchView", pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                Batches.Add(batch);
                BatchManager.GetInstance().Save(Batches);
                PageUpdated(new FunctionEventArgs<int>(PageIndex));
            });
        });

        public DelegateCommand<Batch> EditCommand => new((Batch batch) =>
        {
            DialogParameters pairs = new DialogParameters
            {
                { "Batch", batch },
                {"Mode", "Edit"  }
            };
            dialogService?.ShowDialog("EditBatchView", pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                BatchManager.GetInstance().Save(Batches);
            });
        });

        public DelegateCommand<Batch> ViewCommand => new((Batch batch) =>
        {
            DialogParameters pairs = new DialogParameters
            {
                { "Batch", batch },
                {"Mode", "View"  }
            };
            dialogService?.ShowDialog("EditBatchView", pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
            });
        });

        public DelegateCommand<Batch> DeleteCommand => new((Batch batch) =>
        {
            BatchCol.Remove(batch);
            Batches.Remove(batch);
            BatchManager.GetInstance().Save(Batches);
        });

        public DelegateCommand<FunctionEventArgs<string>> SearchCommand => new((FunctionEventArgs<string> e) =>
        {
            string key = e.Info;
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
            if (PageIndex != 1)
            {
                PageIndex = 1;
            }
            else
            {
                var data = Batches.Take(DataCountPerPage);
                BatchCol = new ObservableCollection<Batch>(data);
            }
        });

        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand => new(PageUpdated);

        public BatchViewModel(IContainerProvider containerProvider, IDialogService dialog) : base(containerProvider)
        {
            PageIndex = 1;
            dialogService = dialog;
            Batches = new ObservableCollection<Batch>(BatchManager.GetInstance().Batches);
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
            var data = Batches.Skip((info.Info - 1) * DataCountPerPage).Take(DataCountPerPage);
            BatchCol = new ObservableCollection<Batch>(data);
        }
    }
}
