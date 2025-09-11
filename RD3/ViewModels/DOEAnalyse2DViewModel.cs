using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using RD3.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace RD3.ViewModels
{
    public class DOEAnalyse2DViewModel : BaseViewModel, IDialogAware
    {
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
            DesignCol = parameters.GetValue<ObservableCollection<OrthogonalParam>>(nameof(OrthogonalParam));
            if (_dataResult == null || _dataResult.Rows.Count == 0)
            {
                return;
            }

            for (int i = 0; i < DesignCol.Count; i++)
            {
                Factor2DParam param = new Factor2DParam()
                {
                    FactorName = DesignCol[i].Name,
                    Minimum = DesignCol[i].Low,
                    Maximum= DesignCol[i].High,
                    //Low = DesignCol[i].Low,
                    //High = DesignCol[i].High,
                    CurrentValue = DesignCol[i].Low,
                    Frequency = (DesignCol[i].High - DesignCol[i].Low) / 100
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
