using HandyControl.Tools.Extension;
using HelixToolkit.Wpf;
using ImTools;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
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
using System.Windows.Media.Media3D;
using static SkiaSharp.HarfBuzz.SKShaper;

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

        private ObservableCollection<int> _levelCol = [2, 3, 4, 5];
        public ObservableCollection<int> LevelCol
        {
            get => _levelCol;
            set { SetProperty(ref _levelCol, value); }
        }

        private int _level = 3;
        public int Level
        {
            get => _level;
            set { SetProperty(ref _level, value); }
        }

        private bool _isLevelEnabled = false;
        public bool IsLevelEnabled
        {
            get => _isLevelEnabled;
            set { SetProperty(ref _isLevelEnabled, value); }
        }

        private string _generators = string.Empty;
        public string Generators
        {
            get => _generators;
            set { SetProperty(ref _generators, value); }
        }

        private int _sampleNumber = 0;
        public int SampleNumber
        {
            get => _sampleNumber;
            set { SetProperty(ref _sampleNumber, value); }
        }

        private int _iterations = 5;
        public int Iterations
        {
            get => _iterations;
            set { SetProperty(ref _iterations, value); }
        }

        private int _lowCenterPoint = 3;
        public int LowCenterPoint
        {
            get => _lowCenterPoint;
            set { SetProperty(ref _lowCenterPoint, value); }
        }

        private int _highCenterPoint = 3;
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

        private DOEFace _face = DOEFace.Faced;
        public DOEFace SelectedFace
        {
            get => _face;
            set { SetProperty(ref _face, value); }
        }

        private ProbDistribution _selectedDistribution = ProbDistribution.None;
        public ProbDistribution SelectedDistribution
        {
            get => _selectedDistribution;
            set { SetProperty(ref _selectedDistribution, value); }
        }

        private Criterion _selectedCriterion = Criterion.None;
        public Criterion SelectedCriterion
        {
            get => _selectedCriterion;
            set { SetProperty(ref _selectedCriterion, value); }
        }

        private DOEDesignType _selectedDesignType = DOEDesignType.CentralComposite;
        public DOEDesignType SelectedDesignType
        {
            get => _selectedDesignType;
            set 
            { 
                SetProperty(ref _selectedDesignType, value);
                if (value == DOEDesignType.TwoLevelFractionalFactorial || value == DOEDesignType.Plackett_Burman ||
                    value == DOEDesignType.Box_Behnken || value == DOEDesignType.CentralComposite)
                {
                    IsLevelEnabled = false;
                }
                else
                {
                    IsLevelEnabled = true;
                }

                switch (value)
                {
                    case DOEDesignType.TwoLevelFractionalFactorial:
                    case DOEDesignType.Plackett_Burman:
                        Level = 2;
                        break;
                    
                    case DOEDesignType.Box_Behnken:
                    case DOEDesignType.CentralComposite:
                        Level = 3;
                        break;
                }
            }
        }

        private ObservableCollection<ParameterNode> _selectedFactors = [];


        private ObservableCollection<OrthogonalParam> _designCol = [];
        public ObservableCollection<OrthogonalParam> DesignCol
        {
            get=> _designCol;
            set => SetProperty(ref _designCol, value);
        }

        public DelegateCommand CloseCommand => new(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));

        public  DelegateCommand GenerateCommand => new(async () =>
        {
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
            keyValuePairs.Add(nameof(DesignCol), DesignCol);
            keyValuePairs.Add(nameof(Level), Level);
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
            _selectedFactors = parameters.GetValue<ObservableCollection<ParameterNode>>(nameof(Factor));
            if (_selectedFactors == null) return;
            Columns = new string[_selectedFactors.Count + 1];
            Columns[0] = "";
            DataColumn column = new DataColumn("索引");
            column.ReadOnly = true;
            DataSource.Columns.Add(column);
            for (int i = 0; i < _selectedFactors.Count; i++)
            {
                ParameterNode factor = _selectedFactors[i];
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
            Matrix<double> result = null;
            switch (SelectedDesignType)
            {
                case DOEDesignType.FullFactorial:
                    try 
                    {
                        result = DesignOfExperiments.BuildFullFactDesign(DesignCol);
                        MatrixConvertToDataTable(result);
                    } 
                    catch (Exception ex) 
                    { 
                        HandyControl.Controls.MessageBox.Warning(ex.Message, "温馨提示"); 
                    }
                    break;
                case DOEDesignType.TwoLevelFractionalFactorial:
                    try 
                    {
                        result = DesignOfExperiments.BuildFracFactDesign(DesignCol, Generators);
                        MatrixConvertToDataTable(result);
                    } 
                    catch (Exception ex) 
                    { 
                        HandyControl.Controls.MessageBox.Warning(ex.Message, "温馨提示"); 
                    }
                    break;
                case DOEDesignType.Plackett_Burman:
                    try 
                    {
                        result = DesignOfExperiments.BuildPlackettBurmanDesign(DesignCol);
                        MatrixConvertToDataTable(result);
                    } 
                    catch (Exception ex) 
                    { 
                        HandyControl.Controls.MessageBox.Warning(ex.Message, "温馨提示");
                    }
                    break;
                case DOEDesignType.Box_Behnken:
                    try 
                    {
                        result = DesignOfExperiments.BuildBoxBehnkenDesign(DesignCol, LowCenterPoint);
                        MatrixConvertToDataTable(result);
                    } 
                    catch (Exception ex) 
                    { 
                        HandyControl.Controls.MessageBox.Warning(ex.Message, "温馨提示"); 
                    }
                    break;
                case DOEDesignType.CentralComposite:
                    try 
                    {
                        result = DesignOfExperiments.BuildCCDDesign(DesignCol, (LowCenterPoint, HighCenterPoint), SelectedAlpha, SelectedFace);
                        MatrixConvertToDataTable(result);
                    } 
                    catch (Exception ex) 
                    { 
                        HandyControl.Controls.MessageBox.Warning(ex.Message, "温馨提示"); 
                    }
                    break;
                case DOEDesignType.LatinHypercube:
                    try 
                    {
                        result = DesignOfExperiments.BuildLhsDesign(DesignCol, SampleNumber, SelectedDistribution, SelectedCriterion);
                        MatrixConvertToDataTable(result);
                    } 
                    catch (Exception ex) 
                    { 
                        HandyControl.Controls.MessageBox.Warning(ex.Message, "温馨提示"); 
                    }
                    break;

            }
        }

        private void MatrixConvertToDataTable(Matrix<double> matrix)
        {
            DataSource.Rows.Clear();
            for (int i = 0; i < matrix.RowCount; i++)
            {
                DataRow row = DataSource.NewRow();
                row[0] = (i + 1).ToString();
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    row[j + 1] = Math.Round(matrix[i, j], 3);
                }
                DataSource.Rows.Add(row);
            }
        }
    }
}
