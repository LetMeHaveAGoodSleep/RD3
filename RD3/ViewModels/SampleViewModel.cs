using HandyControl.Controls;
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
    public class SampleViewModel : NavigationViewModel, IDialogAware
    {
        private readonly IDialogService dialogService;

        private ObservableCollection<Sample> _sampleCol = new ObservableCollection<Sample>();
        public ObservableCollection<Sample> SampleCol { get { return _sampleCol; } set { SetProperty(ref _sampleCol, value); } }

        private ObservableCollection<Sample> _samples = new ObservableCollection<Sample>();
        public ObservableCollection<Sample> Samples { get { return _samples; } set { SetProperty(ref _samples, value); } }

        private int _pageCount;
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

        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand => new(PageUpdated);
        private void PageUpdated(FunctionEventArgs<int> info)
        {
            var data = Samples.Skip((info.Info - 1) * DataCountPerPage).Take(DataCountPerPage);
            SampleCol = new ObservableCollection<Sample>(data);
        }
        public DelegateCommand<FunctionEventArgs<string>> SearchCommand => new((FunctionEventArgs<string> e) =>
        {
            string key = e.Info;
            if (string.IsNullOrEmpty(key))
            {
                Samples = new ObservableCollection<Sample>(Samples);
            }
            else
            {
                var collection = Samples.Where(t => t.Batch.Contains(key) || t.Remark.Contains(key) || t.Reactor.Contains(key)
                || t.Id.Contains(key) || t.Value.ToString().Contains(key) || t.Type.ToString().Contains(key) || t.MainParam.ToString().Contains(key));
                Samples = new ObservableCollection<Sample>(collection);
            }
            PageUpdated(new FunctionEventArgs<int>(PageIndex));
        });

        public DelegateCommand AddSampleCommand => new(() =>
        {
            string Id = DateTime.Now.ToString("yyyyMMdd");
            int index = 1;
            var result = SampleManager.GetInstance().Samples.ToList().FindLast(t => t.Id.StartsWith(Id));
            try
            {
                index = Convert.ToInt32(result?.Id.Substring(8, 2)) + 1;
            }
            catch (Exception ex)
            {

            }
            Id += index.ToString().PadLeft(2, '0');
            Sample sample = new Sample() { Id = Id, Creator = AppSession.CurrentUser.UserName, SampleTime = DateTime.Now };
            DialogParameters pairs = new DialogParameters
            {
                { "Sample", sample },
                {"Mode", "Add"  }
            };
            dialogService?.ShowDialog("EditSampleView", pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                Samples.Add(sample);
                SampleManager.GetInstance().Save(Samples);
                PageUpdated(new FunctionEventArgs<int>(PageIndex));
            });
        });

        public DelegateCommand<Sample> EditCommand => new((Sample sample) =>
        {
            if (sample == null) return;
            DialogParameters pairs = new DialogParameters
            {
                { "Sample", sample },
                {"Mode", "Edit"  }
            };
            dialogService?.ShowDialog("EditSampleView", pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                SampleManager.GetInstance().Save(Samples);
            });
        });
        public DelegateCommand<Sample> CompareCommand => new((Sample sample) =>
        {
            SampleCol.Remove(sample);
            Samples.Remove(sample);
            SampleManager.GetInstance().Save(Samples);
        });

        public DelegateCommand<Sample> ExportCommand => new((Sample sample) =>
        {

        });

        public DelegateCommand<Sample> DeleteCommand => new((Sample sample) =>
        {
            SampleCol.Remove(sample);
            Samples.Remove(sample);
            SampleManager.GetInstance().Save(Samples);
        });

        public SampleViewModel(IContainerProvider containerProvider, IDialogService dialog, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            dialogService = dialog;
            PageIndex = 1;
            Samples = new ObservableCollection<Sample>(SampleManager.GetInstance().Samples);
            PageCount = Samples.Count / DataCountPerPage + (Samples.Count % DataCountPerPage != 0 ? 1 : 0);
            var data = Samples.Take(DataCountPerPage);
            SampleCol = new ObservableCollection<Sample>(data);
        }

        public string Title => AppSession.CompanyName;

        public event Action<IDialogResult> RequestClose;

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
    }
}
