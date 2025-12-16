using Fpi.Instruments;
using log4net.Util;
using MathNet.Symbolics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Common.Events;
using RD3.Extensions;
using RD3.Shared;
using RD3.Shared.Util;
using RD3.Views;
using ScottPlot.Colormaps;
using ScottPlot.TickGenerators.TimeUnits;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using XZ.DB;
using XZ.SQLite;
using static MaterialDesignThemes.Wpf.Theme;

namespace RD3.ViewModels
{
    public class ParameterNodeViewModel : BaseViewModel, IDialogAware
    {
       private ParameterNode _selectedNode;
        public ParameterNode SelectedNode
        {
            get => _selectedNode;
            set { SetProperty(ref _selectedNode, value); }
        }

        private ObservableCollection<ParameterNode> _parameterNodeCol = [];
        public ObservableCollection<ParameterNode> ParameterNodeCol
        {
            get => _parameterNodeCol;
            set { SetProperty(ref _parameterNodeCol, value); }
        }

        private bool _curveParamDisplay = true;
        public bool CurveParamDisplay
        {
            get=> _curveParamDisplay;
            set { SetProperty(ref _curveParamDisplay, value); }
        }

        private ObservableCollection<string> _fieldCol = [];
        public ObservableCollection<string> FieldCol
        {
            get => _fieldCol;
            set { SetProperty(ref _fieldCol, value); }
        }

        private ObservableCollection<string> _parameterUnitCol = [];
        public ObservableCollection<string> ParameterUnitCol
        {
            get => _parameterUnitCol;
            set { SetProperty(ref _parameterUnitCol, value); }
        }

        public DelegateCommand SaveCommand => new(() =>
        {
            ParameterNodeManager.GetInstance().Save();
            HandyControl.Controls.MessageBox.Info("保存成功");
            RequestClose?.Invoke(new Prism.Services.Dialogs.DialogResult(ButtonResult.OK));
        });

        public DelegateCommand<object> DeleteCommand => new((object o) =>
        {
            ParameterNode node = o as ParameterNode;
            ParameterNodeCol.Remove(node);
            if (SqliteManager.DeleteByKey<ParameterNode>(node.ID))
            {
                HandyControl.Controls.MessageBox.Info("删除成功");
            }
        });

        public DelegateCommand SelectColorCommand => new(() =>
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                var currentColor = ColorUtil.FromHexCode(SelectedNode.ColorHex);
                colorDialog.Color = currentColor;
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SelectedNode.ColorStr = $"{colorDialog.Color.R},{colorDialog.Color.G},{colorDialog.Color.B}";
                    SelectedNode.ColorHex = $"#{colorDialog.Color.A.ToString("X2")}{colorDialog.Color.R.ToString("X2")}{colorDialog.Color.G.ToString("X2")}{colorDialog.Color.B.ToString("X2")}";
                }
            }
        });

        public ParameterNodeViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
           
        }

        public string Title => "因子设置";


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
            if (parameters.ContainsKey("Flag"))
            {
                CurveParamDisplay = parameters.GetValue<bool>("Flag");
            }

            var propertyNames = typeof(RealTimeParam).GetProperties().Where(c => c.CanWrite && c.CanRead && c.GetCustomAttribute<NotDBColumnAttribute>() == null).Select(t=>t.Name);
            FieldCol = [.. propertyNames];
            ParameterNodeCol = ParameterNodeManager.GetInstance().ParameterNodes;
            ParameterUnitCol = [.. SqliteManager.QueryList<ParamUnit>().Select(t=>t.Unit)];
        }
    }
}
