using MathNet.Symbolics;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using RD3.Common;
using RD3.Common.Events;
using RD3.Extensions;
using RD3.Shared;
using ScottPlot.TickGenerators.TimeUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RD3.ViewModels
{
    public class FeedViewModel : BaseViewModel, IDialogAware
    {
        private FeedFormulaParam _x1;
        public FeedFormulaParam X1 
        {
            get=> _x1;
            set { SetProperty(ref _x1, value); }
        }

        private FeedFormulaParam _x2;
        public FeedFormulaParam X2
        {
            get => _x2;
            set { SetProperty(ref _x2, value); }
        }

        private FeedFormulaParam _x3;
        public FeedFormulaParam X3
        {
            get => _x3;
            set { SetProperty(ref _x3, value); }
        }

        private FeedFormulaParam _x4;
        public FeedFormulaParam X4
        {
            get => _x4;
            set { SetProperty(ref _x4, value); }
        }

        private FeedFormulaParam _x5;
        public FeedFormulaParam X5
        {
            get => _x5;
            set { SetProperty(ref _x5, value); }
        }

        private FeedFormulaParam _x6;
        public FeedFormulaParam X6
        {
            get => _x6;
            set { SetProperty(ref _x6, value); }
        }

        private FloatingPoint _functionSpeed;

        public Dictionary<string, FloatingPoint> SymbolDic
        {
            get 
            {
                Dictionary<string, FloatingPoint> pairs = new Dictionary<string, FloatingPoint>();

                #region 六个参数
                switch (X1)
                {
                    case FeedFormulaParam.N2_output:
                        pairs.Add("x1", 0);
                        break;
                    case FeedFormulaParam.Air_output:
                        pairs.Add("x1", 0);
                        break;
                    case FeedFormulaParam.O2_output:
                        pairs.Add("x1", 0);
                        break;
                    case FeedFormulaParam.Agit_output:
                        pairs.Add("x1", 0);
                        break;
                    case FeedFormulaParam.CO2_output:
                        pairs.Add("x1", 0);
                        break;
                }

                switch (X2)
                {
                    case FeedFormulaParam.N2_output:
                        pairs.Add("x2", 0);
                        break;
                    case FeedFormulaParam.Air_output:
                        pairs.Add("x2", 0);
                        break;
                    case FeedFormulaParam.O2_output:
                        pairs.Add("x2", 0);
                        break;
                    case FeedFormulaParam.Agit_output:
                        pairs.Add("x2", 0);
                        break;
                    case FeedFormulaParam.CO2_output:
                        pairs.Add("x2", 0);
                        break;
                }

                switch (X3)
                {
                    case FeedFormulaParam.N2_output:
                        pairs.Add("x3", 0);
                        break;
                    case FeedFormulaParam.Air_output:
                        pairs.Add("x3", 0);
                        break;
                    case FeedFormulaParam.O2_output:
                        pairs.Add("x3", 0);
                        break;
                    case FeedFormulaParam.Agit_output:
                        pairs.Add("x3", 0);
                        break;
                    case FeedFormulaParam.CO2_output:
                        pairs.Add("x3", 0);
                        break;
                }

                switch (X4)
                {
                    case FeedFormulaParam.N2_output:
                        pairs.Add("x4", 0);
                        break;
                    case FeedFormulaParam.Air_output:
                        pairs.Add("x4", 0);
                        break;
                    case FeedFormulaParam.O2_output:
                        pairs.Add("x4", 0);
                        break;
                    case FeedFormulaParam.Agit_output:
                        pairs.Add("x4", 0);
                        break;
                    case FeedFormulaParam.CO2_output:
                        pairs.Add("x4", 0);
                        break;
                }

                switch (X5)
                {
                    case FeedFormulaParam.N2_output:
                        pairs.Add("x5", 0);
                        break;
                    case FeedFormulaParam.Air_output:
                        pairs.Add("x5", 0);
                        break;
                    case FeedFormulaParam.O2_output:
                        pairs.Add("x5", 0);
                        break;
                    case FeedFormulaParam.Agit_output:
                        pairs.Add("x5", 0);
                        break;
                    case FeedFormulaParam.CO2_output:
                        pairs.Add("x5", 0);
                        break;
                }

                switch (X6)
                {
                    case FeedFormulaParam.N2_output:
                        pairs.Add("x6", 0);
                        break;
                    case FeedFormulaParam.Air_output:
                        pairs.Add("x6", 0);
                        break;
                    case FeedFormulaParam.O2_output:
                        pairs.Add("x6", 0);
                        break;
                    case FeedFormulaParam.Agit_output:
                        pairs.Add("x6", 0);
                        break;
                    case FeedFormulaParam.CO2_output:
                        pairs.Add("x6", 0);
                        break;
                }
                #endregion

                return pairs;
            }
        }

        private DispatcherTimer _functionTimer;

        private int _gradientTimeIndex = 0;
        public int GradientTimeIndex
        {
            get => _gradientTimeIndex;
            set { SetProperty(ref _gradientTimeIndex, value); }
        }
        private DispatcherTimer _gradientTimer;

        private TimeSpan _elapsedTime;

        private string _formattedTime = "00:00:00";

        public string FormattedTime
        {
            get => _formattedTime;
            set
            {
                SetProperty(ref _formattedTime, value);
            }
        }

        /// <summary>
        /// 补料策略是否处理
        /// </summary>
        bool handled = false;

        DispatcherTimer _feedTimer;

        public DOParam DOParam { get; set; }

        public PHParam PHParam { get; set; }

        private FeedStrategy _backupFeedStrategy;

        FeedStrategy _feedStrategy;
        public FeedStrategy FeedStrategy
        {
            get => _feedStrategy;
            set { SetProperty(ref _feedStrategy, value); }
        }

        private string _expression;
        public string Expression
        {
            get => _expression;
            set { SetProperty(ref _expression, value); }
        }

        private SymbolicExpression _bkExpression;

        public DelegateCommand<object> ExcuteCommand => new((object o) => 
        {
            if (!Enum.TryParse(FeedStrategy.FeedMethod.GetType(), o?.ToString(), true, out var targetValue))
            {
                return;
            }
            if (FeedStrategy.FeedMethod == (FeedMethod)targetValue)
            {
                CommandWrapper.SetFeedSpeed(0);//停止补料
                FeedStrategy.FeedMethod = FeedMethod.Unknown;

                StopAllTimer();
                //aggregator.SendMessage("已取消当前补料策略", nameof(FeedViewModel));
                return;
            }
            FeedStrategy.FeedMethod = (FeedMethod)targetValue;
            switch (FeedStrategy.FeedMethod)
            {
                case FeedMethod.Constant:
                    CommandWrapper.SetFeedSpeed(FeedStrategy.ConstantVal);

                    StopAllTimer();
                    break;
                case FeedMethod.Associate://放在实时信息中处理
                    break;
                case FeedMethod.Gradient:
                    StopAllTimer();

                    _gradientTimer = new DispatcherTimer();
                    _gradientTimer.Interval = TimeSpan.FromSeconds(1);
                    _gradientTimer.Tick += ((s, e) => 
                    {
                        _elapsedTime += TimeSpan.FromSeconds(1);
                        if (GradientTimeIndex == 0)
                        {
                            FormattedTime = _elapsedTime.ToString(@"dd\:hh\:mm\:ss");
                        }
                        else
                        {
                            FormattedTime = _elapsedTime.ToString(@"hh\:mm\:ss");
                        }

                        Task.Run(new Action(() => { GradientAction(); }));
                    });
                    _gradientTimer.Start();
                    break;
                case FeedMethod.Function:
                    StopAllTimer();

                    var speed = _bkExpression.Evaluate(SymbolDic);
                    CommandWrapper.SetFeedSpeed((float)speed.RealValue);
                    _functionSpeed = speed;

                    _functionTimer = new DispatcherTimer();
                    _functionTimer.Interval = TimeSpan.FromSeconds(5);//每5s检查一次，速度是否变化
                    _functionTimer.Tick += ((s, e) =>
                    {
                        var speed = _bkExpression.Evaluate(SymbolDic);
                        if (speed.RealValue != _functionSpeed.RealValue)
                        {
                            CommandWrapper.SetFeedSpeed((float)speed.RealValue);
                        }
                        _functionSpeed = speed;
                    });
                    _functionTimer.Start();

                    break;
                case FeedMethod.Loop:
                    break;
                case FeedMethod.MutilLinear:
                    break;
            }
        });

        public DelegateCommand<string> CheckExpressionCommand => new((string strExpression) => 
        {
            try
            {
                var expression = SymbolicExpression.Parse(strExpression);
                var result = expression.Evaluate(SymbolDic);

                _bkExpression = expression;
                Expression = expression?.ToString();
            }
            catch (Exception ex)
            {
                Expression = _bkExpression?.ToString();
                LogHelper.Error(ex);
            }
            
        });
        public DelegateCommand CloseCommand => new(() =>
        {
            DialogParameters keyValuePairs = new DialogParameters();
            keyValuePairs.Add(nameof(FeedStrategy), FeedStrategy);
            DialogResult dialogResult = new DialogResult(ButtonResult.Cancel, keyValuePairs);
            RequestClose?.Invoke(dialogResult);
        });

        public DelegateCommand OKCommand => new(() =>
        {
            DialogParameters keyValuePairs = new DialogParameters();
            keyValuePairs.Add(nameof(FeedStrategy), FeedStrategy);
            DialogResult dialogResult = new DialogResult(ButtonResult.OK, keyValuePairs);
            RequestClose?.Invoke(dialogResult);
        });

        public FeedViewModel(IContainerProvider containerProvider, IDialogHostService dialogHostService) : base(containerProvider, dialogHostService)
        {
            aggregator.ResgiterMessage((MessageModel model) =>
            {
                var realTimeParam = model.Model as RealTimeParam;
                if (!CompareUtil.CompareProperties(_backupFeedStrategy, FeedStrategy, _backupFeedStrategy.GetType()))
                {
                    handled = false;
                    _backupFeedStrategy = FeedStrategy.Clone() as FeedStrategy;
                }
                switch (FeedStrategy.FeedMethod)
                {
                    case FeedMethod.Associate:
                        FeedStrategy.AssociateStrategy.AssociateAmount = realTimeParam.Feed;

                        var caliValue = FeedStrategy.AssociateStrategy.AssociateModule == FeedAssociateModule.DO ? realTimeParam.DO : realTimeParam.PH;
                        var presetValue = FeedStrategy.AssociateStrategy.AssociateModule == FeedAssociateModule.DO
                        ? DOParam.DO_PV : PHParam.PH_PV;
                        var lowerLimit = FeedStrategy.AssociateStrategy.AssociateModule == FeedAssociateModule.DO
                        ? DOParam.LowerLimit : PHParam.LowerLimit;
                        var upperLimit = FeedStrategy.AssociateStrategy.AssociateModule == FeedAssociateModule.DO
                        ? DOParam.UpperLimit : PHParam.UpperLimit;

                        if ((FeedStrategy.AssociateStrategy.AssociateThreshold == FeedAssociateThreshold.Lowerlimit
                        && caliValue < presetValue - lowerLimit)
                        || (FeedStrategy.AssociateStrategy.AssociateThreshold == FeedAssociateThreshold.Upperlimit
                        && caliValue > presetValue + upperLimit))
                        {
                            if (handled) return;
                            if (FeedStrategy.AssociateStrategy.AssociatePattern == FeedAssociatePattern.Speed)
                            {
                                CommandWrapper.SetFeedSpeed(FeedStrategy.AssociateStrategy.AssociateSpeed);
                                handled = true;
                            }
                            else if (FeedStrategy.AssociateStrategy.AssociatePattern == FeedAssociatePattern.Dose)
                            {
                                CommandWrapper.SetFeedSpeed(FeedStrategy.AssociateStrategy.AssociateSpeed);
                                var hour = FeedStrategy.AssociateStrategy.AssociateVal / FeedStrategy.AssociateStrategy.AssociateSpeed;
                                _feedTimer = new DispatcherTimer();
                                _feedTimer.Interval = TimeSpan.FromHours(hour);
                                _feedTimer.Tick += ((s, e) =>
                                {
                                    LogHelper.Info(string.Format("补料时长：{0}h，补料速度：{1}ml/h,补料剂量：{3}", hour
                                        , FeedStrategy.AssociateStrategy.AssociateSpeed, FeedStrategy.AssociateStrategy.AssociateVal));
                                    CommandWrapper.SetFeedSpeed(0);
                                    _feedTimer?.Stop();
                                });
                                handled = true;
                            }
                        }
                        else
                        {
                            handled = false;
                            CommandWrapper.SetFeedSpeed(0);
                        }
                        break;
                    default:
                        handled = false;
                        CommandWrapper.SetFeedSpeed(0);

                        _gradientTimer?.Stop();
                        _gradientTimer = null;
                        break;
                }
            }, nameof(ClockSupervisor));
        }

        public string Title => AppSession.CompanyName;

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
            _backupFeedStrategy = FeedStrategy = parameters.GetValue<FeedStrategy>(nameof(FeedStrategy)).Clone() as FeedStrategy;
            DOParam = parameters.GetValue<DOParam>(nameof(DOParam)) as DOParam;
            PHParam = parameters.GetValue<PHParam>(nameof(PHParam)) as PHParam;
        }

        private void StopAllTimer()
        {
            _gradientTimer?.Stop();
            _gradientTimer = null;

            _functionTimer?.Stop();
            _functionTimer = null;
        }

        private void GradientAction()
        {
            var result = FeedStrategy.GradientStrategy.GradientTuples.Find(t => t.Item1 == _elapsedTime.Seconds);
            if (result == null)
            {
                return;
            }
            CommandWrapper.SetFeedSpeed(result.Item2);
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            double needHour = result.Item3 / result.Item2;
            dispatcherTimer.Interval = TimeSpan.FromHours(needHour);
            dispatcherTimer.Tick += ((s, e) => 
            {
                LogHelper.Info(string.Format("补料时长：{0}h，补料速度：{1}ml/h,补料剂量：{3}", needHour
                                        , result.Item2, result.Item3));
                CommandWrapper.SetFeedSpeed(0);

                dispatcherTimer?.Stop();
                dispatcherTimer = null;
            });
        }
    }
}
