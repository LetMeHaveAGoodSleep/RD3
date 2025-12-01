using Newtonsoft.Json;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class ChooseRTParamViewModel : BaseViewModel, IDialogAware
    {
        public ObservableCollection<ReportNode> ReportNodes
        {
            get => AnalysisSolution.GetInstance().ReportNodeCol;
        }

        public DelegateCommand AddCommand => new(() => 
        {
            ReportNodes.Add(new ReportNode() { Width = 100, ShowName = "未命名", Used = true });
        });

        public DelegateCommand<ReportNode> DeleteCommand => new((ReportNode node) =>
        {
            ReportNodes.Remove(node);
        });

        public ChooseRTParamViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => "参数选择";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;        
        }

        public void OnDialogClosed()
        {
            string json = JsonConvert.SerializeObject(ReportNodes);
            File.WriteAllText(FileConst.ReportNodesPath, json);
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}
