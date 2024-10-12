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

namespace RD3.ViewModels
{
    public class MainViewModel : NavigationViewModel, IConfigureService
    {
        private string userName;

        private User _user;
        public User User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
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

        public DelegateCommand<FunctionEventArgs<object>> SelectCmd => new(SwitchMenuItem);

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

        private void SwitchMenuItem(FunctionEventArgs<object> info)
        {
            if (string.IsNullOrWhiteSpace((info.Info as SideMenuItem)?.Header.ToString())) return;

            MenuBar menuBar = MenuBars.FindFirst(t => t.Title.Trim().Equals((info.Info as SideMenuItem)?.Header.ToString()));
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
            MenuBars.Add(new MenuBar() { Icon = "Home", Title = Language.GetValue("Main").ToString(), NameSpace = "IndexView" });
            MenuBars.Add(new MenuBar() { Icon = "Project", Title = Language.GetValue("Project").ToString(), NameSpace = "ProjectView" });
            MenuBars.Add(new MenuBar() { Icon = "Data", Title = Language.GetValue("Data").ToString(), NameSpace = "BatchView" });
            MenuBars.Add(new MenuBar() { Icon = "Calibrate", Title = Language.GetValue("Calibrate").ToString(), NameSpace = "CalibrationView" });
            MenuBars.Add(new MenuBar() { Icon = "Control", Title = Language.GetValue("Control").ToString(), NameSpace = "ControlView" });
            MenuBars.Add(new MenuBar() { Icon = "Audit", Title = Language.GetValue("Audit").ToString(), NameSpace = "AuditView" });
            MenuBars.Add(new MenuBar() { Icon = "Setting", Title = Language.GetValue("Setting").ToString(), NameSpace = "SettingView" });
            MenuBars.Add(new MenuBar() { Icon = "Debug", Title = Language.GetValue("Debug").ToString(), NameSpace = "MCUDebugView" }); 
        }

        /// <summary>
        /// 配置首页初始化参数
        /// </summary>
        public void Configure()
        {
            LanguageName = Const.CHNLanguage;
            CreateMenuBar();
            User = AppSession.CurrentUser;
            aggregator.SendMessage("", nameof(MainViewModel));
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
