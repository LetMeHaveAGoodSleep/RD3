using HandyControl.Tools.Extension;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Extensions;
using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.ViewModels
{
    public class DOEDesignViewModel : BaseViewModel, IDialogAware
    {
       public string[] Columns
        {
            get;
            private set;
        }

        private DataTable _dataSource = new DataTable();
        public DataTable DataSource
        {
            get { return _dataSource; }
            private set { SetProperty(ref _dataSource, value); }
        }

        private int _lowCenterPoint = 2;
        public int LowCenterPoint
        {
            get => _lowCenterPoint;
            set { SetProperty(ref _lowCenterPoint, value); }
        }

        private int _highCenterPoint = 2;
        public int HighCenterPoint
        {
            get => _highCenterPoint;
            set { SetProperty(ref _highCenterPoint, value); }
        }

        private DOEAlpha _alpha = DOEAlpha.Orthogonal;
        public DOEAlpha SelectedAlpha
        {
            get => _alpha;
            set { SetProperty(ref _alpha, value); }
        }

        private DOEFace _face = DOEFace.Circumscribed;
        public DOEFace SelectedFace
        {
            get => _face;
            set { SetProperty(ref _face, value); }
        }

        private DOEDesignType _selectedDesignType = DOEDesignType.CentralComposite;
        public DOEDesignType SelectedDesignType
        {
            get => _selectedDesignType;
            set { SetProperty(ref _selectedDesignType, value); }
        }

        //private int _doeIndex = 2;
        //public int DoeIndex
        //{
        //    get => _doeIndex;
        //    set { SetProperty(ref _doeIndex, value); }
        //}

        private List<Factor> _selectedFactors = [];


        private ObservableCollection<OrthogonalParam> _designCol = [];
        public ObservableCollection<OrthogonalParam> DesignCol
        {
            get=> _designCol;
            set => SetProperty(ref _designCol, value);
        }


        public DelegateCommand CloseCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public  DelegateCommand GenerateCommand => new(async () =>
        {
            //if (SelectedDesignType)
            //{
            //    await DialogExtensions.Info("温馨提示", "请选择设计类型!");
            //    return;
            //}
            foreach (var item in _designCol)
            {
                if ((item.Low == 0 && item.High == 0) || item.High < item.Low)
                {
                    await DialogExtensions.Info("温馨提示", "数据填写错误!");
                    return;
                }
            }

            GenerateDOEResult();
        });

        public DelegateCommand OKCommand => new(async () =>
        {
            if (DataSource.Rows.Count == 0)
            {
                await DialogExtensions.Info("温馨提示", "未生成结果数据！");
                return;
            }
            DialogParameters keyValuePairs = new DialogParameters();
            keyValuePairs.Add("DesignResult", DataSource);
            DialogResult dialogResult = new DialogResult(ButtonResult.OK, keyValuePairs);
            RequestClose?.Invoke(dialogResult);
        });

        public DOEDesignViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {

        }

        public string Title => "新建设计";

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
            DesignCol.Clear();
            _selectedFactors = parameters.GetValue<List<Factor>>("Factors");
            if (_selectedFactors == null) return;
            Columns = new string[_selectedFactors.Count + 1];
            Columns[0] = "";
            DataColumn column = new DataColumn("Index");
            column.ReadOnly = true;
            DataSource.Columns.Add(column);
            for (int i = 0; i < _selectedFactors.Count; i++)
            {
                Factor factor = _selectedFactors[i];
                OrthogonalParam orthogonalParam = new OrthogonalParam()
                {
                    Name = factor.ToString()
                };
                DesignCol.Add(orthogonalParam);
                string colName = factor.ToString();
                Columns[i + 1] = (colName);
                DataColumn dataColumn = new DataColumn(colName);
                dataColumn.ReadOnly = true;
                DataSource.Columns.Add(dataColumn);
            }
        }

        private void GenerateDOEResult()
        {
            switch (SelectedDesignType)
            {
                case DOEDesignType.FullFactorial:
                    DataSource = DOEUtil.GenerateCombinationsAsDataTable(DesignCol);
                    aggregator.SendMessage("", nameof(DOEDesignViewModel), DataSource);
                    break;
                case DOEDesignType.TwoLevelFractionalFactorial:
                    DataSource = DOEUtil.GenerateCombinationsAsDataTable(DesignCol);
                    aggregator.SendMessage("", nameof(DOEDesignViewModel), DataSource);
                    break;
                case DOEDesignType.Plackett_Burman:
                    break;
                case DOEDesignType.Box_Behnken:
                    break;
                case DOEDesignType.CentralComposite:
                    var res = DOEUtil.BuildCCDDesign(DesignCol, (LowCenterPoint, HighCenterPoint), SelectedAlpha, SelectedFace);

                    // 设置alpha值
                    double alpha = DOEUtil.CalculateAlpha(DesignCol.Count, SelectedAlpha);
                    // 步骤一：确定因素数量和水平范围（已在上述代码完成，主要是定义变量存储相关信息）
                    // 步骤二：构建析因点（基于二水平全因子设计算法构建）
                    double[,] factorialPoints = DOEUtil.BuildFactorialPoints(DesignCol);
                    // 步骤三：计算星点（根据传入的alpha值、设计选项以及因素上下限计算星点位置）
                    double[,] axialPoints = DOEUtil.CalculateAxialPoints(DesignCol, alpha, SelectedFace, LowCenterPoint, HighCenterPoint);
                    // 步骤四：添加中心点（计算各因素的中心值并构建中心点坐标，考虑多个中心点情况）
                    double[] centerPoint = DOEUtil.CalculateCenterPoint(DesignCol);
                    double[,] designMatrix = DOEUtil.GetResult(factorialPoints, axialPoints, centerPoint, HighCenterPoint);
                    int count = designMatrix.GetLength(0);
                    int length = designMatrix.GetLength(1);
                    DataSource.Rows.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        DataRow row = DataSource.NewRow();
                        row[0] = (i + 1).ToString();
                        for (int j = 0; j < length; j++)
                        {
                            row[j + 1] = Math.Round(designMatrix[i, j], 3);
                        }
                        DataSource.Rows.Add(row);
                    }
                    break;
            }
        }
    }
}
