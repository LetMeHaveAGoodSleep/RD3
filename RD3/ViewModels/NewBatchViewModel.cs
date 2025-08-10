using CustomApp;
using ImTools;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Extensions;
using RD3.Shared;
using RD3.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using XZ.SQLite;

namespace RD3.ViewModels
{
    public class NewBatchViewModel : BaseViewModel, IDialogAware
    {
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

        private List<RD3Project> _projectList = new List<RD3Project>();

        public List<RD3Project> ProjectList
        {
            get => _projectList;
            set => SetProperty(ref _projectList, value);
        }

        public string Title => "批次管理";

        public event Action<IDialogResult> RequestClose;


        ObservableCollection<RD3Batch> _rD3Batch = new ObservableCollection<RD3Batch>();
        public ObservableCollection<RD3Batch> RD3Batch
        {
            get { return _rD3Batch; }
            set { SetProperty(ref _rD3Batch, value); }
        }

        public DelegateCommand AddCommand => new(() =>
        {
        });

        public DelegateCommand<object> EditCommand => new((object o) =>
        {
            
        });

        public DelegateCommand<object> CompareCommand => new((object o) =>
        {
            DialogParameters pairs = new DialogParameters
                {
                    { "Batches", o },
                };
            DialogHostService.ShowOnce(nameof(CompareBatchView), pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                //ProjectTemplateManager.GetInstance().Save(ProjectTemplates);
            });
        });

        /// <summary>
        /// 离线数据命令
        /// </summary>
        public DelegateCommand<object> OffLineDataCommand => new((object o) =>
        {
            DialogParameters pairs = new DialogParameters
                {
                    { "Batch", o },
                };
            DialogHostService.ShowOnce(nameof(OffLineDatasView), pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                //ProjectTemplateManager.GetInstance().Save(ProjectTemplates);
            });
        });

        /// <summary>
        /// 导出数据命令
        /// </summary>
        public DelegateCommand<object> OutputDataCommand => new((object o) =>
        {
            DialogParameters pairs = new DialogParameters
                {
                    { "Batch", o },
                };
            DialogHostService.ShowOnce(nameof(OutputBatchDataView), pairs, callback =>
            {
                if (callback.Result != ButtonResult.OK)
                {
                    return;
                }
                //ProjectTemplateManager.GetInstance().Save(ProjectTemplates);
            });
        });


        public DelegateCommand OKCommand => new(() =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        });
        public DelegateCommand CancelCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public DelegateCommand<object> DeleteCommand => new((object o) => 
        {
            var list = o as List<RD3Batch>;
            foreach (var item in list)
            {
                try
                {
                    RD3Batch.Remove(item);
                    RD3SQLHelper.DeleteBatch(item);
                }
                catch (Exception ex)
                { }

            }
        });

        public DelegateCommand ReloadDataCommand => new(() => InitBatchInfos());

        string hisDataDir = AppDomain.CurrentDomain.BaseDirectory + "HistoryData";
        public NewBatchViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            
        }

        /// <summary>
        /// 初始化批次数据 -读文件
        /// --后期改为读数据库
        /// </summary>
        private void InitBatchInfos()
        {
            try
            {
                List<RD3Batch> batches = RD3SQLHelper.QueryBatch();
                RD3Batch = new ObservableCollection<RD3Batch>(batches);
            }
            catch(Exception ex)
            {

            }
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
            InitBatchInfos();
        }
    }
}
