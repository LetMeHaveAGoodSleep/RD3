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
using System.Windows.Data;

namespace RD3.ViewModels
{
    public class FeedGradientViewModel : BaseViewModel, IDialogAware
    {
        string deviceID = "";
        Dictionary<string, ObservableCollection<FeedGradientInfo>> allDeviceInfos = new Dictionary<string, ObservableCollection<FeedGradientInfo>>();

        ObservableCollection<FeedGradientInfo> _feedGradientInfos = [];
        public ObservableCollection<FeedGradientInfo> FeedGradientInfos
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

        FeedGradientInfo _selectedFeedGradientInfo;
        public FeedGradientInfo SelectedFeedGradientInfo
        {
            get { return _selectedFeedGradientInfo; }
            set { SetProperty(ref _selectedFeedGradientInfo, value); }
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

        public DelegateCommand AddCommand => new(() =>
        {
            if (FeedGradientInfos.Count < 1)
            {
                FeedGradientInfos.Add(new FeedGradientInfo() { BeginTime = 0, EndTime = 60, InfoType = "Constant" });
            }
            else 
            {
                var clone = FeedGradientInfos[FeedGradientInfos.Count - 1].Clone() as FeedGradientInfo;
                clone.BeginTime = clone.EndTime;
                clone.EndTime = clone.EndTime + 60;
                FeedGradientInfos.Add(clone);
            }  
        });

        public DelegateCommand DeleteCommand => new(() =>
        {
            if (SelectedFeedGradientInfo != null)
                FeedGradientInfos.Remove(SelectedFeedGradientInfo);
        });

        public DelegateCommand SaveCommand => new(() =>
        {
            for (int i = FeedGradientInfos.Count - 1; i >= 0; i--)
            {
                var item = FeedGradientInfos[i];
                if (string.IsNullOrWhiteSpace(item.InfoType))
                {
                    FeedGradientInfos.Remove(item);
                }
            }

            if (CollectionViewSource.GetDefaultView(FeedGradientInfos) is ListCollectionView view1)
            {
                if (view1.IsAddingNew) view1.CommitNew();
                if (view1.IsEditingItem) view1.CommitEdit();
            }

            allDeviceInfos[deviceID] = FeedGradientInfos;
            FeedGradientManager.GetInstance().Save(allDeviceInfos);
        });

        public string Title => "补料时间序列";

        private readonly IRegionManager regionManager;
        private IRegionNavigationJournal journal;
        private readonly IDialogService dialogService;

        public event Action<IDialogResult> RequestClose;

        public FeedGradientViewModel(IContainerProvider containerProvider,
            IRegionManager regionManager, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            this.regionManager = regionManager;
            this.dialogService = containerProvider.Resolve<IDialogService>();

            FeedGradientTypes = new ObservableCollection<FeedGradientType>();
            FeedGradientTypes.Add(new FeedGradientType() { Name= "Polynomial", Description= "多项式" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "Exponential", Description = "指数" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "DO_Feedback", Description = "DO_stat(流速)" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "PH_Feedback", Description = "pH_stat(流速)" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "DO_Feedback_Total", Description = "DO_stat(体积)" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "PH_Feedback_Total", Description = "pH_stat(体积)" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "Constant", Description = "恒速" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "Quantitative", Description = "定量" });
            FeedGradientTypes.Add(new FeedGradientType() { Name = "Cycle", Description = "周期" });
        }//

        public void Configure()
        {
        }

        public bool CanCloseDialog()
        {
            bool flag = true;
            for (int i = FeedGradientInfos.Count - 1; i >= 0; i--)
            {
                var item = FeedGradientInfos[i];
                if (item.BeginTime == default && item.EndTime == default)
                {
                    MessageBox.Show("温馨提示", $"第{i + 1}行的起始时间未填写");
                    flag = false;
                    break;
                }
                if (item.BeginTime == item.EndTime)
                {
                    MessageBox.Show("温馨提示", $"第{i + 1}行的起始时间相等");
                    flag = false;
                    break;
                }
                if (string.IsNullOrWhiteSpace(item.InfoType))
                {
                    MessageBox.Show("温馨提示", $"第{i + 1}行的数据未填写");
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        public void OnDialogClosed()
        {
            SaveCommand.Execute();
        }
        public void OnDialogOpened(IDialogParameters parameters)
        {
            deviceID = parameters.GetValue<string>("deviceID");

            deviceID = parameters.GetValue<string>("deviceID");
            allDeviceInfos = FeedGradientManager.GetInstance().FeedGradientCol;
            var feedPump = parameters.GetValue<string>(nameof(PeristalticPump));
            Enum.TryParse(typeof(PeristalticPump), feedPump, out var peristalticPump);
            //如果没有补料策略，复制第一个仪器的策略过来
            if (!allDeviceInfos.ContainsKey(deviceID))
            {
                foreach (var key in allDeviceInfos.Keys)
                {
                    ObservableCollection<FeedGradientInfo> temp = [];
                    foreach (var info in allDeviceInfos[key])
                    {
                        temp.Add(info.Clone() as FeedGradientInfo);
                    }
                    allDeviceInfos.Add(deviceID, temp);
                    break;
                }
            }
            var collection = allDeviceInfos[deviceID];
            ObservableCollection<FeedGradientInfo> collection1 = [];
            foreach (var info in collection)
            {
                if (info.Pump == (PeristalticPump)peristalticPump)
                {
                    FeedGradientInfos.Add(info);
                }
                else
                {
                    collection1.Add(info);
                }
            }
            if (FeedGradientInfos.Count < 1)
            {
                foreach (var item in collection1)
                {
                    FeedGradientInfo info = item.Clone() as FeedGradientInfo;
                    info.Pump = (PeristalticPump)peristalticPump;
                    FeedGradientInfos.Add(info);

                    if (!allDeviceInfos[deviceID].Contains(info))
                    {
                        allDeviceInfos[deviceID].Add(info);
                    }
                }
            }
            FeedGradientInfos = allDeviceInfos[deviceID];
            SelectedFeedGradientInfo = FeedGradientInfos.Count > 0 ? FeedGradientInfos[0] : null;
        }
    }
}
