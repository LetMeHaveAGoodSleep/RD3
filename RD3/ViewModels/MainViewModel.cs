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
using LayUI.Wpf.Enum;
using System.Windows.Controls;

namespace RD3.ViewModels
{
    [RegionMemberLifetime(KeepAlive = true)]
    public class MainViewModel : BindableBase, IConfigureService
    {
        private string userName;

        public string UserName
        {
            get { return userName; }
            set { userName = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<MenuItemModel> _MenuItemList;
        public ObservableCollection<MenuItemModel> MenuItemList
        {
            get { return _MenuItemList; }
            set { _MenuItemList = value; RaisePropertyChanged(); }
        }

        private AnimationType _AnimationType = AnimationType.RotateOut;
        public AnimationType AnimationType
        {
            get { return _AnimationType; }
            set { SetProperty(ref _AnimationType, value); }
        }

        private MenuItemModel _MenuItemModel;
        public MenuItemModel MenuItemModel
        {
            get { return _MenuItemModel; }
            set
            {
                SetProperty(ref _MenuItemModel, value);
                new Action(async () =>
                {
                    AnimationType = AnimationType.SlideOutToLeft;
                    await Task.Delay(300);
                    //Region.RequestNavigate(SystemResource.Nav_HomeContent, MenuItemModel.PageKey);
                    AnimationType = AnimationType.SlideInToRight;
                }).Invoke();
            }
        }

        private IRegionNavigationService navigationService;

        public DelegateCommand<MenuBar> NavigateCommand { get; private set; }
        public DelegateCommand GoBackCommand { get; private set; }
        public DelegateCommand GoForwardCommand { get; private set; }
        public DelegateCommand LoginOutCommand { get; private set; }
        public DelegateCommand<MenuItemModel> GoPageCommand { get; private set; }
        private ObservableCollection<MenuBar> menuBars;
        private readonly IContainerProvider containerProvider;
        private readonly IRegionManager regionManager;
        private IRegionNavigationJournal journal;

        public ObservableCollection<MenuBar> MenuBars
        {
            get { return menuBars; }
            set { menuBars = value; RaisePropertyChanged(); }
        }

        public MainViewModel(IContainerProvider containerProvider,
            IRegionManager regionManager)
        { 
            MenuBars = new ObservableCollection<MenuBar>();
            NavigateCommand = new DelegateCommand<MenuBar>(Navigate);
            GoBackCommand = new DelegateCommand(() =>
            {
                if (journal != null && journal.CanGoBack)
                    journal.GoBack();
            });
            GoForwardCommand = new DelegateCommand(() =>
            {
                if (journal != null && journal.CanGoForward)
                    journal.GoForward();
            });
            LoginOutCommand = new DelegateCommand(() =>
              {
                  //注销当前用户
                  App.LoginOut(containerProvider);
              });

           GoPageCommand = new DelegateCommand<MenuItemModel>((MenuItemModel item) => 
           {
               if (item == null) return;
               MenuItemModel = item;
           });
            this.containerProvider = containerProvider;
            this.regionManager = regionManager;
        }

        private void Navigate(MenuBar obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.NameSpace))
                return;

            regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(obj.NameSpace, back =>
             {
                 journal = back.Context.NavigationService.Journal;
             });
        }

        void CreateMenuBar()
        {
            MenuBars.Add(new MenuBar() { Icon = "Home", Title = "首页", NameSpace = "IndexView" });
            MenuBars.Add(new MenuBar() { Icon = "NotebookOutline", Title = "待办事项", NameSpace = "ToDoView" });
            MenuBars.Add(new MenuBar() { Icon = "NotebookPlus", Title = "备忘录", NameSpace = "MemoView" });
            MenuBars.Add(new MenuBar() { Icon = "Cog", Title = "设置", NameSpace = "SettingsView" });
        }

        /// <summary>
        /// 配置首页初始化参数
        /// </summary>
        public void Configure()
        {
            UserName = AppSession.UserName;
            CreateMenuBar();
            MenuItemList = GetData();
            MenuItemModel = MenuItemList.First().Data.First();
            MenuItemList.First().IsSelected = true;
            MenuItemModel.IsSelected = true;
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

        private ObservableCollection<MenuItemModel> GetData()
        {

            return
                new ObservableCollection<MenuItemModel>()
                {
                new MenuItemModel()
                {
                    ItemTitle = "BasicElements",
                    Data = new ObservableCollection<MenuItemModel>()
                        {
                            new MenuItemModel()
                            {
                                ItemTitle="Icon", PageKey="",IsNew=true
                            },new MenuItemModel()
                            {
                                ItemTitle="Skeleton", PageKey="",IsNew=true
                            },
                            new MenuItemModel()
                            {
                                ItemTitle="Button", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Form", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Menu", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="TabControl", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="ProgressBar", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Panel", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Tag", PageKey="",IsNew=true
                            },new MenuItemModel()
                            {
                                ItemTitle="Expander", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Transition", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Loading", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="GIF", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="ScaleImage", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Timeline", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="AuxiliaryElement", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="FlowItemsControl", PageKey=""
                            }
                        }
                },new MenuItemModel()
                {
                    ItemTitle = "ComponentExamples",
                    Data = new ObservableCollection<MenuItemModel>()
                        {

                            new MenuItemModel()
                            {
                                ItemTitle="AnimationCommand", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="ToolTip",PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Badge", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Ripple", PageKey="",IsNew=true
                            },new MenuItemModel()
                            {
                                ItemTitle="PopupBox", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Dialog", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Drawer", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="DateTime", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="DataGrid", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Pagination", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="DropDownMenu", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Upload", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="ShuttleGrid",PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="TreeView", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Cascader", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="TreeSelect", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Slider",PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Score", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Carousel", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Message", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="Notification",PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="NoticeBar", PageKey="",IsNew=true
                            },new MenuItemModel()
                            {
                                ItemTitle="VRImage", PageKey=""
                            },new MenuItemModel()
                            {
                                ItemTitle="PropertyGrid", PageKey="",IsNew=true
                            },new MenuItemModel()
                            {
                                ItemTitle="Keyboard", PageKey=""
                            }
                        }
                },new MenuItemModel()
                {
                    ItemTitle = "递归菜单",
                    Data = new ObservableCollection<MenuItemModel>()
                    {
                        new MenuItemModel()
                        {
                            ItemTitle="二级菜单",
                            Data = new ObservableCollection<MenuItemModel>()
                            {
                                new MenuItemModel()
                                {
                                    ItemTitle="三级菜单",
                                    Data = new ObservableCollection<MenuItemModel>()
                                    {
                                        new MenuItemModel()
                                        {
                                            ItemTitle="四级菜单",
                                            Data = new ObservableCollection<MenuItemModel>()
                                            {
                                                new MenuItemModel()
                                                {
                                                    ItemTitle="五级菜单",
                                                    Data = new ObservableCollection<MenuItemModel>()
                                                    {
                                                        new MenuItemModel()
                                                        {
                                                            ItemTitle="六级菜单"
                                                        },
                                                    }
                                                },
                                            }
                                        },
                                    }
                                },
                            }
                        },
                    }
                }
                };
        }
    }
}
