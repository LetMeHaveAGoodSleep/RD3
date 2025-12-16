using DryIoc;
using log4net.Core;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Shared;
using RD3.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static RD3.Shared.QuadraticSurfaceLM;

namespace RD3.ViewModels
{
    public class DOEAnalyse2DViewModel : BaseViewModel, IDialogAware
    {
        public int Level;

        private DataTable _dataResult;

        private ObservableCollection<Factor2DParam> _factor2DParams = [];
        public ObservableCollection<Factor2DParam> Factor2DParams
        {
            get { return _factor2DParams; }
            set { SetProperty(ref _factor2DParams, value); }
        }

        private ObservableCollection<OrthogonalParam> _designCol = [];
        public ObservableCollection<OrthogonalParam> DesignCol
        {
            get => _designCol;
            set => SetProperty(ref _designCol, value);
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
            for (int i = 0; i < Factor2DParams.Count; i++)
            {
                Factor2DParams[i].CurrentValue = Factor2DParams[i].Maximum;
            }
            //Prediction = maxValue;
        });

        public DelegateCommand PredictMinCommand => new(() =>
        {
            for (int i = 0; i < Factor2DParams.Count; i++)
            {
                Factor2DParams[i].CurrentValue = Factor2DParams[i].Minimum;
            }
            //Prediction = minValue;
        });

        public DOEAnalyse2DViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
        }

        public string Title => "全因素交互";

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
            _dataResult = parameters.GetValue<DataTable>("Result").Copy();
            Level = parameters.GetValue<int>(nameof(Level));
            DesignCol = parameters.GetValue<ObservableCollection<OrthogonalParam>>(nameof(OrthogonalParam));
            if (_dataResult == null || _dataResult.Rows.Count == 0)
            {
                return;
            }

            for (int i = 0; i < DesignCol.Count; i++)
            {
                var levelValues = new List<double>();
                if (Level >= 1 && double.TryParse(DesignCol[i].Level1.ToString(), out double l1))
                    levelValues.Add(l1);
                if (Level >= 2 && double.TryParse(DesignCol[i].Level2.ToString(), out double l2))
                    levelValues.Add(l2);
                if (Level >= 3 && double.TryParse(DesignCol[i].Level3.ToString(), out double l3))
                    levelValues.Add(l3);
                if (Level >= 4 && double.TryParse(DesignCol[i].Level4.ToString(), out double l4))
                    levelValues.Add(l4);
                if (Level >= 5 && double.TryParse(DesignCol[i].Level5.ToString(), out double l5))
                    levelValues.Add(l5);
                double min = levelValues.Min();
                double max = levelValues.Max();

                Factor2DParam param = new Factor2DParam()
                {
                    FactorName = DesignCol[i].Name,
                    Minimum = min,
                    Maximum= max,
                    CurrentValue = min,
                    Frequency = (max - min) / 100
                };
                Factor2DParams.Add(param);
            }

            AnalysisSolution.GetInstance().EventPublisher.PublishDOEResult(_dataResult);
        }
    }

    public class Factor2DParam : BindableBase
    {
        private string _factorName;
        public string FactorName
        {
            get => _factorName;
            set => SetProperty(ref _factorName, value);
        }

        private double _minimum = 0d;
        public double Minimum
        {
            get => _minimum;
            set => SetProperty(ref _minimum, value);
        }

        private double _maximum = 100d;
        public double Maximum
        {
            get => _maximum;
            set => SetProperty(ref _maximum, value);
        }

        //private double _low = 0d;
        //public double Low
        //{
        //    get => _low;
        //    set => SetProperty(ref _low, value);
        //}

        //private double _high = 100d;
        //public double High
        //{
        //    get => _high;
        //    set => SetProperty(ref _high, value);
        //}

        private double _currentValue = 100d;
        public double CurrentValue
        {
            get => _currentValue;
            set => SetProperty(ref _currentValue, value);
        }

        private double _frequency = 0d;
        public double Frequency
        {
            get => _frequency;
            set => SetProperty(ref _frequency, value);
        }
    }
}
