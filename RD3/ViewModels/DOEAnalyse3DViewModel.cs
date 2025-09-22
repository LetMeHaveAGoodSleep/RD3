using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Distributions;
using ScottPlot;
using RD3.Shared;

namespace RD3.ViewModels
{
    public class DOEAnalyse3DViewModel : BaseViewModel, IDialogAware
    {
        private int _order = 2;
        public int Order
        {
            get { return _order; }
            set
            {
                SetProperty(ref _order, value);
            }
        }

        private double _mse = 0d;
        public double MSE
        {
            get { return _mse; }
            set
            {
                SetProperty(ref _mse, value);
            }
        }

        private Factor2DParam _xFactor2DParam;
        public Factor2DParam XFactor2DParam
        {
            get { return _xFactor2DParam; }
            set
            {
                SetProperty(ref _xFactor2DParam, value);
            }
        }

        private Factor2DParam _yFactor2DParam;
        public Factor2DParam YFactor2DParam
        {
            get { return _yFactor2DParam; }
            set
            {
                SetProperty(ref _yFactor2DParam, value);
            }
        }

        private double _prediction;
        public double Prediction
        {
            get { return _prediction; }
            set
            {
                SetProperty(ref _prediction, value);
            }
        }

        public List<string> Terms = [];

        public Vector<double> FitCoefficient = Vector<double>.Build.Dense(1, 0.0);

        public DelegateCommand PredictMaxCommand => new(() => 
        {
            var ranges = DenseMatrix.OfArray(new double[,] { { XFactor2DParam.Minimum, XFactor2DParam.Maximum }, { YFactor2DParam.Minimum, YFactor2DParam.Maximum } });
            var (maxValue, maxX) = PolynomialExtrema.FindMaximum(FitCoefficient, Terms, ranges);
            XFactor2DParam.CurrentValue = maxX[0];
            YFactor2DParam.CurrentValue = maxX[1];
            Prediction = maxValue;
        });

        public DelegateCommand PredictMinCommand => new(() => 
        {
            var ranges = DenseMatrix.OfArray(new double[2, 2] { { XFactor2DParam.Minimum, XFactor2DParam.Maximum }, { YFactor2DParam.Minimum, YFactor2DParam.Maximum } });
            var (minValue, minX) = PolynomialExtrema.FindMinimum(FitCoefficient, Terms, ranges);
            XFactor2DParam.CurrentValue = minX[0];
            YFactor2DParam.CurrentValue = minX[1];
            Prediction = minValue;
        });

        public DOEAnalyse3DViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => "3D分析";

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
            var _dataResult = parameters.GetValue<DataTable>("Result").Copy();
            var temp = parameters.GetValue<Factor2DParam[]>(nameof(Factor2DParam));
            XFactor2DParam = temp[0];
            YFactor2DParam = temp[1];
            for (int i = _dataResult.Columns.Count - 3; i > 0; i--)
            {
                if (!temp.Select(t => t.FactorName).Contains(_dataResult.Columns[i].ColumnName))
                {
                    _dataResult.Columns.RemoveAt(i);
                }
            }
            _dataResult.Columns[XFactor2DParam.FactorName].SetOrdinal(0);
            _dataResult.Columns[YFactor2DParam.FactorName].SetOrdinal(1);
            AnalysisSolution.GetInstance().EventPublisher.PublishDOEResult(_dataResult);
        }
    }
}
