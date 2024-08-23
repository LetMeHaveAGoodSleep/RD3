﻿using RD3.Common;
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

namespace RD3.ViewModels
{
    public class MainViewModel : BindableBase, IConfigureService
    {
        private string userName;

        public string UserName
        {
            get { return userName; }
            set { userName = value; RaisePropertyChanged(); }
        }

        private string languageName;
        public string LanguageName
        {
            get { return languageName; }
            set { languageName = value; RaisePropertyChanged(); }
        }

        private IRegionNavigationService navigationService;

        public DelegateCommand<MenuBar> NavigateCommand => new(Navigate);
        public DelegateCommand GoBackCommand => new(() =>
        {
            if (journal != null && journal.CanGoBack)
                journal.GoBack();
        });
        public DelegateCommand GoForwardCommand => new(() =>
        {
            if (journal != null && journal.CanGoForward)
                journal.GoForward();
        });
        public DelegateCommand LoginOutCommand => new(() => { App.LoginOut(containerProvider); });
        public DelegateCommand ManageUserCommand => new(() =>
        {
            dialogService.ShowDialog("UserView", callback =>
            {
            });
        });
        public DelegateCommand ChangeLanguageCommand => new(() => 
        {
            dialogService.ShowDialog("CommunicationView", callback =>
            {
            }); 
        });

        public DelegateCommand<string> SelectCmd => new(SwitchMenuItem);

        private ObservableCollection<MenuBar> menuBars = new ObservableCollection<MenuBar>();
        private readonly IContainerProvider containerProvider;
        private readonly IRegionManager regionManager;
        private IRegionNavigationJournal journal;
        private readonly IEventAggregator aggregator;
        private readonly IDialogService dialogService;

        public ObservableCollection<MenuBar> MenuBars
        {
            get { return menuBars; }
            set { menuBars = value; RaisePropertyChanged(); }
        }

        public MainViewModel(IContainerProvider containerProvider,
            IRegionManager regionManager, IEventAggregator aggregator)
        {
            this.aggregator = aggregator;
            this.containerProvider = containerProvider;
            this.regionManager = regionManager;
            this.dialogService = containerProvider.Resolve<IDialogService>();
        }

        private void SwitchMenuItem(string header)
        {
            if (string.IsNullOrWhiteSpace(header)) return;

            MenuBar menuBar = MenuBars.FindFirst(t => t.Title.Trim().Equals(header));
            Navigate(menuBar);
        } 

        private void Navigate(MenuBar obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.NameSpace))
            {
                regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("ErrorView");
                return;
            }
                

            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(obj.NameSpace, back =>
             {
                 if (!(bool)back.Result)
                 {
                     regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("ErrorView");
                     return;
                 }
                 journal = back.Context.NavigationService.Journal;
             });
        }

        void CreateMenuBar()
        {
            MenuBars.Add(new MenuBar() { Icon = "Home", Title = "Home", NameSpace = "IndexView" });
            MenuBars.Add(new MenuBar() { Icon = "Project", Title = "Projects", NameSpace = "ProjectView" });
            MenuBars.Add(new MenuBar() { Icon = "Data", Title = "Data", NameSpace = "BatchView" });
            MenuBars.Add(new MenuBar() { Icon = "Calibrate", Title = "Calibrate", NameSpace = "CalibrateView" });
            MenuBars.Add(new MenuBar() { Icon = "Control", Title = "Control", NameSpace = "CommunicationView" });
            MenuBars.Add(new MenuBar() { Icon = "Settings", Title = "Settings", NameSpace = "AlarmView" });
        }

        /// <summary>
        /// 配置首页初始化参数
        /// </summary>
        public void Configure()
        {
            UserName = AppSession.CurrentUser.UserName;
            LanguageName = "中文";
            CreateMenuBar();
            var navigationParameters = new NavigationParameters();
            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(
        new Uri(nameof(IndexView) + navigationParameters.ToString(), UriKind.Relative), navigationCallback =>
            {
                if ((bool)navigationCallback.Result)
                {
                    journal = navigationCallback.Context.NavigationService.Journal;
                }
            });
        }
    }
}
