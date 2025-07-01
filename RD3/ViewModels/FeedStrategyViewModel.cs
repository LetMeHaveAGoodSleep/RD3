using RD3.Common;
using RD3.Common.Models;
using RD3.Extensions;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RD3.Views;
using DryIoc;
using System.Windows.Controls;
using HandyControl.Data;
using HandyControl.Controls;
using ImTools;
using Prism.Services.Dialogs;
using System.Windows.Threading;
using Prism.Events;
using RD3.Shared;
using HandyControl.Tools;
using System.Resources;
using static MaterialDesignThemes.Wpf.Theme;
using System.Diagnostics;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Fpi.Communication.Manager;
using static RD3.Views.NewMainView;
using System.ComponentModel;
using System.Reflection.PortableExecutable;
using OpenTK.Compute.OpenCL;
using System.Data;
using XZ.SQLite;
using OpenTK.Audio.OpenAL;
using System.IO;
using System.CodeDom;
using System.Windows.Documents;
using System.Reflection;
using Fpi.Util.WinApiUtil.CommDataType;
using System.Windows.Data;
using MathNet.Symbolics;
using System.Windows.Forms;
using ScottPlot.Colormaps;

namespace RD3.ViewModels
{
    public class FeedStrategyViewModel : BaseViewModel, IDialogAware
    {
        ObservableCollection<FeedStrategyInfo> _feedGradientInfos = [];
        public ObservableCollection<FeedStrategyInfo> FeedStrategyInfos
        {
            get { return _feedGradientInfos; }
            set 
            {
                if (CollectionViewSource.GetDefaultView(_feedGradientInfos) is ListCollectionView view)
                {
                    if (view.IsAddingNew) view.CommitNew();
                    if (view.IsEditingItem) view.CommitEdit();
                }

                if (CollectionViewSource.GetDefaultView(value) is ListCollectionView view1)
                {
                    if (view1.IsAddingNew) view1.CommitNew();
                    if (view1.IsEditingItem) view1.CommitEdit();
                }

                SetProperty(ref _feedGradientInfos, value); 
            }
        }


        ObservableCollection<FeedGradientType> _feedGradientTypes = new ObservableCollection<FeedGradientType>();
        public ObservableCollection<FeedGradientType> FeedGradientTypes
        {
            get { return _feedGradientTypes; }
            set { SetProperty(ref _feedGradientTypes, value); }
        }


        FeedStrategyInfo _selectedFeedStrategy;
        public FeedStrategyInfo SelectedFeedStrategy
        {
            get { return _selectedFeedStrategy; }
            set { SetProperty(ref _selectedFeedStrategy, value); }
        }

        private string _languageName;
        public string LanguageName
        {
            get { return _languageName; }
            set
            {
                
                if (value != _languageName)
                {
                    if (value == Const.CHNLanguage)
                    {
                        Language.LoadResourceKey("zh_CN");
                    }
                    else
                    {
                        Language.LoadResourceKey("en_US");
                    }
                }
                SetProperty(ref _languageName, value);
                AppSession.LanguageName = value;
            }
        }

        string deviceID = "";
        Dictionary<string, ObservableCollection<FeedStrategyInfo>> allDeviceInfos = new Dictionary<string, ObservableCollection<FeedStrategyInfo>>();

        public DelegateCommand AddCommand => new(() =>
        {
            FeedStrategyInfos.Add(new FeedStrategyInfo()
            {
                InfoType = "Constant"
            });
        });

        public DelegateCommand DeleteCommand => new(() =>
        {
            if (SelectedFeedStrategy != null)
                FeedStrategyInfos.Remove(SelectedFeedStrategy);
        });

        public DelegateCommand SaveCommand => new(() =>
        {
            if (CollectionViewSource.GetDefaultView(FeedStrategyInfos) is ListCollectionView view1)
            {
                if (view1.IsAddingNew) view1.CommitNew();
                if (view1.IsEditingItem) view1.CommitEdit();

                FeedStrategyInfos = new ObservableCollection<FeedStrategyInfo>(view1.Cast<FeedStrategyInfo>());
            }
            allDeviceInfos[deviceID] = FeedStrategyInfos;
            FeedStrategyManager.GetInstance().Save(allDeviceInfos);
        });

        public string Title => "补料策略";

        private readonly IRegionManager regionManager;
        private IRegionNavigationJournal journal;
        private readonly IDialogService dialogService;

        public event Action<IDialogResult> RequestClose;

        public FeedStrategyViewModel(IContainerProvider containerProvider,
            IRegionManager regionManager, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            this.regionManager = regionManager;
            this.dialogService = containerProvider.Resolve<IDialogService>();

            FeedGradientTypes = new ObservableCollection<FeedGradientType>();
            FeedGradientTypes.Add(new FeedGradientType() { Name = "Polynomial", Description = "多项式" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "Exponential", Description = "指数" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "DO_Feedback", Description = "DO_stat(流速)" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "PH_Feedback", Description = "pH_stat(流速)" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "DO_Feedback_Total", Description = "DO_stat(体积)" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "PH_Feedback_Total", Description = "pH_stat(体积)" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "Constant", Description = "恒速" });
            //FeedGradientTypes.Add(new FeedGradientType() { Name = "Quantitative", Description = "定量" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "Cycle", Description = "周期" });
        }//

        public void Configure()
        {
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            SaveCommand.Execute();
        }
        public void OnDialogOpened(IDialogParameters parameters)
        {
            deviceID = parameters.GetValue<string>("deviceID");
            allDeviceInfos = FeedStrategyManager.GetInstance().FeedStrategyCol;
            var feedPump = parameters.GetValue<string>(nameof(PeristalticPump));
            Enum.TryParse(typeof(PeristalticPump), feedPump, out var peristalticPump);
            //如果没有补料策略，复制第一个仪器的策略过来
            if (!allDeviceInfos.ContainsKey(deviceID))
            {
                foreach (var key in allDeviceInfos.Keys) 
                {
                    ObservableCollection<FeedStrategyInfo> temp = [];
                    foreach (var info in allDeviceInfos[key])
                    {
                        temp.Add(info.Clone() as FeedStrategyInfo);
                    }
                    allDeviceInfos.Add(deviceID, temp);
                    break;
                }
            }
            var collection = allDeviceInfos[deviceID];
            ObservableCollection<FeedStrategyInfo> collection1 = [];
            foreach (var info in collection)
            {
                if (info.Pump == (PeristalticPump)peristalticPump)
                {
                    FeedStrategyInfos.Add(info);
                }
                else
                {
                    collection1.Add(info);
                }
            }
            if (FeedStrategyInfos.Count < 1)
            {
                foreach (var item in collection1)
                {
                    FeedStrategyInfo info = item.Clone() as FeedStrategyInfo;
                    info.Pump = (PeristalticPump)peristalticPump;
                    FeedStrategyInfos.Add(info);
                    if (!allDeviceInfos[deviceID].Contains(info))
                    {
                        allDeviceInfos[deviceID].Add(info);
                    }
                }
            }
            
            SelectedFeedStrategy = FeedStrategyInfos.Count > 0 ? FeedStrategyInfos[0] : null;
        }
    }
}
