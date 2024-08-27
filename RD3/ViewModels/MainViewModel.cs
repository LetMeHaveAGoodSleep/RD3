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

namespace RD3.ViewModels
{
    public class MainViewModel : NavigationViewModel, IConfigureService
    {
        private string userName;

        public string UserName
        {
            get { return userName; }
            set { userName = value; RaisePropertyChanged(); }
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
            }
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
            if (LanguageName == Const.CHNLanguage)
            {
                LanguageName = Const.ENGLanguage;
            }
            else
            {
                LanguageName = Const.CHNLanguage;
            }
        });

        public DelegateCommand<string> SelectCmd => new(SwitchMenuItem);

        private ObservableCollection<MenuBar> menuBars = new ObservableCollection<MenuBar>();
        private readonly IRegionManager regionManager;
        private IRegionNavigationJournal journal;
        private readonly IDialogService dialogService;

        public ObservableCollection<MenuBar> MenuBars
        {
            get { return menuBars; }
            set { menuBars = value; RaisePropertyChanged(); }
        }

        public MainViewModel(IContainerProvider containerProvider,
            IRegionManager regionManager) : base(containerProvider)
        {

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
            MenuBars.Add(new MenuBar() { Icon = "Home", Title = Language.GetValue("Home").ToString(), NameSpace = "IndexView" });
            MenuBars.Add(new MenuBar() { Icon = "Project", Title = Language.GetValue("Project").ToString(), NameSpace = "ProjectView" });
            MenuBars.Add(new MenuBar() { Icon = "Data", Title = Language.GetValue("Data").ToString(), NameSpace = "BatchView" });
            MenuBars.Add(new MenuBar() { Icon = "Calibrate", Title = Language.GetValue("Calibrate").ToString(), NameSpace = "CalibrateView" });
            MenuBars.Add(new MenuBar() { Icon = "Control", Title = Language.GetValue("Control").ToString(), NameSpace = "CommunicationView" });
            MenuBars.Add(new MenuBar() { Icon = "Settings", Title = Language.GetValue("Settings").ToString(), NameSpace = "AlarmView" });
        }

        /// <summary>
        /// 配置首页初始化参数
        /// </summary>
        public void Configure()
        {
            UserName = AppSession.CurrentUser.UserName;
            LanguageName = Const.CHNLanguage;
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
