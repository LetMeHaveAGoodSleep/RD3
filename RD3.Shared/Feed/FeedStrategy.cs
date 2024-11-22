using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class FeedStrategy : BindableBase,ICloneable
    {
        private FeedMethod _feedMethod;
        public FeedMethod FeedMethod
        {
            get => _feedMethod;
            set { SetProperty(ref _feedMethod, value); }
        }

        private float _constantVal;
        public float ConstantVal
        {
            get => _constantVal;
            set { SetProperty(ref _constantVal, value); }
        }

        private AssociateStrategy _associateStrategy = new();
        public AssociateStrategy AssociateStrategy
        {
            get => _associateStrategy;
            set { SetProperty(ref _associateStrategy, value); }
        }

        private GradientStrategy _gradientStrategy = new();
        public GradientStrategy GradientStrategy
        {
            get => _gradientStrategy;
            set { SetProperty(ref _gradientStrategy, value); }
        }

        public object Clone()
        {
            var clonedObject = ObjectCloner.DeepCopy(this);
            return clonedObject;
        }
    }

    public class AssociateStrategy : BindableBase
    {
        private float _associateSpeed;
        public float AssociateSpeed
        {
            get => _associateSpeed;
            set { SetProperty(ref _associateSpeed, value); }
        }

        private float _associateVal;
        public float AssociateVal
        {
            get => _associateVal;
            set { SetProperty(ref _associateVal, value); }
        }

        private float _associateAmount;
        public float AssociateAmount
        {
            get => _associateAmount;
            set { SetProperty(ref _associateAmount, value); }
        }

        private FeedAssociateModule _associateModule;
        public FeedAssociateModule AssociateModule
        {
            get => _associateModule;
            set { SetProperty(ref _associateModule, value); }
        }

        private FeedAssociatePattern _associatePattern;
        public FeedAssociatePattern AssociatePattern
        {
            get => _associatePattern;
            set { SetProperty(ref _associatePattern, value); }
        }

        private FeedAssociateThreshold _associateThreshold;
        public FeedAssociateThreshold AssociateThreshold
        {
            get => _associateThreshold;
            set { SetProperty(ref _associateThreshold, value); }
        }
    }

    public class GradientStrategy : BindableBase
    {
        private FeedTimer _feedTimer = FeedTimer.Day;
        public FeedTimer FeedTimer
        {
            get => _feedTimer;
            set
            {
                SetProperty(ref _feedTimer, value);
            }
        }

        #region 20个时序操作
        private float _time1;
        public float Time1 
        {
            get => _time1;
            set
            {
                SetProperty(ref _time1, value);
            }
        }

        private float _speed1;
        public float Speed1
        {
            get => _speed1;
            set
            {
                SetProperty(ref _speed1, value);
            }
        }

        private float _dose1;
        public float Dose1
        {
            get => _dose1;
            set
            {
                SetProperty(ref _dose1, value);
            }
        }

        private float _time2;
        public float Time2
        {
            get => _time2;
            set
            {
                SetProperty(ref _time2, value);
            }
        }

        private float _speed2;
        public float Speed2
        {
            get => _speed2;
            set
            {
                SetProperty(ref _speed2, value);
            }
        }

        private float _dose2;
        public float Dose2
        {
            get => _dose2;
            set
            {
                SetProperty(ref _dose2, value);
            }
        }

        private float _time3;
        public float Time3
        {
            get => _time3;
            set
            {
                SetProperty(ref _time3, value);
            }
        }

        private float _speed3;
        public float Speed3
        {
            get => _speed3;
            set
            {
                SetProperty(ref _speed3, value);
            }
        }

        private float _dose3;
        public float Dose3
        {
            get => _dose3;
            set
            {
                SetProperty(ref _dose3, value);
            }
        }

        private float _time4;
        public float Time4
        {
            get => _time4;
            set
            {
                SetProperty(ref _time4, value);
            }
        }

        private float _speed4;
        public float Speed4
        {
            get => _speed4;
            set
            {
                SetProperty(ref _speed4, value);
            }
        }

        private float _dose4;
        public float Dose4
        {
            get => _dose4;
            set
            {
                SetProperty(ref _dose4, value);
            }
        }

        private float _time5;
        public float Time5
        {
            get => _time5;
            set
            {
                SetProperty(ref _time5, value);
            }
        }

        private float _speed5;
        public float Speed5
        {
            get => _speed5;
            set
            {
                SetProperty(ref _speed5, value);
            }
        }

        private float _dose5;
        public float Dose5
        {
            get => _dose5;
            set
            {
                SetProperty(ref _dose5, value);
            }
        }

        private float _time6;
        public float Time6
        {
            get => _time6;
            set
            {
                SetProperty(ref _time6, value);
            }
        }

        private float _speed6;
        public float Speed6
        {
            get => _speed6;
            set
            {
                SetProperty(ref _speed6, value);
            }
        }

        private float _dose6;
        public float Dose6
        {
            get => _dose6;
            set
            {
                SetProperty(ref _dose6, value);
            }
        }

        private float _time7;
        public float Time7
        {
            get => _time7;
            set
            {
                SetProperty(ref _time7, value);
            }
        }

        private float _speed7;
        public float Speed7
        {
            get => _speed7;
            set
            {
                SetProperty(ref _speed7, value);
            }
        }

        private float _dose7;
        public float Dose7
        {
            get => _dose7;
            set
            {
                SetProperty(ref _dose7, value);
            }
        }

        private float _time8;
        public float Time8
        {
            get => _time8;
            set
            {
                SetProperty(ref _time8, value);
            }
        }

        private float _speed8;
        public float Speed8
        {
            get => _speed8;
            set
            {
                SetProperty(ref _speed8, value);
            }
        }

        private float _dose8;
        public float Dose8
        {
            get => _dose8;
            set
            {
                SetProperty(ref _dose8, value);
            }
        }

        private float _time9;
        public float Time9
        {
            get => _time9;
            set
            {
                SetProperty(ref _time9, value);
            }
        }

        private float _speed9;
        public float Speed9
        {
            get => _speed9;
            set
            {
                SetProperty(ref _speed9, value);
            }
        }

        private float _dose9;
        public float Dose9
        {
            get => _dose9;
            set
            {
                SetProperty(ref _dose9, value);
            }
        }

        private float _time10;
        public float Time10
        {
            get => _time10;
            set
            {
                SetProperty(ref _time10, value);
            }
        }

        private float _speed10;
        public float Speed10
        {
            get => _speed10;
            set
            {
                SetProperty(ref _speed10, value);
            }
        }

        private float _dose10;
        public float Dose10
        {
            get => _dose10;
            set
            {
                SetProperty(ref _dose10, value);
            }
        }

        private float _time11;
        public float Time11
        {
            get => _time11;
            set
            {
                SetProperty(ref _time11, value);
            }
        }

        private float _speed11;
        public float Speed11
        {
            get => _speed11;
            set
            {
                SetProperty(ref _speed11, value);
            }
        }

        private float _dose11;
        public float Dose11
        {
            get => _dose11;
            set
            {
                SetProperty(ref _dose11, value);
            }
        }

        private float _time12;
        public float Time12
        {
            get => _time12;
            set
            {
                SetProperty(ref _time12, value);
            }
        }

        private float _speed12;
        public float Speed12
        {
            get => _speed12;
            set
            {
                SetProperty(ref _speed12, value);
            }
        }

        private float _dose12;
        public float Dose12
        {
            get => _dose12;
            set
            {
                SetProperty(ref _dose12, value);
            }
        }

        private float _time13;
        public float Time13
        {
            get => _time13;
            set
            {
                SetProperty(ref _time13, value);
            }
        }

        private float _speed13;
        public float Speed13
        {
            get => _speed13;
            set
            {
                SetProperty(ref _speed13, value);
            }
        }

        private float _dose13;
        public float Dose13
        {
            get => _dose13;
            set
            {
                SetProperty(ref _dose13, value);
            }
        }

        private float _time14;
        public float Time14
        {
            get => _time14;
            set
            {
                SetProperty(ref _time14, value);
            }
        }

        private float _speed14;
        public float Speed14
        {
            get => _speed14;
            set
            {
                SetProperty(ref _speed14, value);
            }
        }

        private float _dose14;
        public float Dose14
        {
            get => _dose14;
            set
            {
                SetProperty(ref _dose14, value);
            }
        }

        private float _time15;
        public float Time15
        {
            get => _time15;
            set
            {
                SetProperty(ref _time15, value);
            }
        }

        private float _speed15;
        public float Speed15
        {
            get => _speed15;
            set
            {
                SetProperty(ref _speed15, value);
            }
        }

        private float _dose15;
        public float Dose15
        {
            get => _dose15;
            set
            {
                SetProperty(ref _dose15, value);
            }
        }

        private float _time16;
        public float Time16
        {
            get => _time16;
            set
            {
                SetProperty(ref _time16, value);
            }
        }

        private float _speed16;
        public float Speed16
        {
            get => _speed16;
            set
            {
                SetProperty(ref _speed16, value);
            }
        }

        private float _dose16;
        public float Dose16
        {
            get => _dose16;
            set
            {
                SetProperty(ref _dose16, value);
            }
        }

        private float _time17;
        public float Time17
        {
            get => _time17;
            set
            {
                SetProperty(ref _time17, value);
            }
        }

        private float _speed17;
        public float Speed17
        {
            get => _speed17;
            set
            {
                SetProperty(ref _speed17, value);
            }
        }

        private float _dose17;
        public float Dose17
        {
            get => _dose17;
            set
            {
                SetProperty(ref _dose17, value);
            }
        }

        private float _time18;
        public float Time18
        {
            get => _time18;
            set
            {
                SetProperty(ref _time18, value);
            }
        }

        private float _speed18;
        public float Speed18
        {
            get => _speed18;
            set
            {
                SetProperty(ref _speed18, value);
            }
        }

        private float _dose18;
        public float Dose18
        {
            get => _dose18;
            set
            {
                SetProperty(ref _dose18, value);
            }
        }

        private float _time19;
        public float Time19
        {
            get => _time19;
            set
            {
                SetProperty(ref _time19, value);
            }
        }

        private float _speed19;
        public float Speed19
        {
            get => _speed19;
            set
            {
                SetProperty(ref _speed19, value);
            }
        }

        private float _dose19;
        public float Dose19
        {
            get => _dose19;
            set
            {
                SetProperty(ref _dose19, value);
            }
        }

        private float _time20;
        public float Time20
        {
            get => _time20;
            set
            {
                SetProperty(ref _time20, value);
            }
        }

        private float _speed20;
        public float Speed20
        {
            get => _speed20;
            set
            {
                SetProperty(ref _speed20, value);
            }
        }

        private float _dose20;
        public float Dose20
        {
            get => _dose20;
            set
            {
                SetProperty(ref _dose20, value);
            }
        }
        #endregion

        private float _amount;
        public float Amount
        {
            get => _amount;
            set
            {
                SetProperty(ref _amount, value);
            }
        }

        public List<Tuple<float, float, float>> GradientTuples
        {
            get
            {
                List<Tuple<float, float, float>> list = [];

                Type type = this.GetType();
                for (int i = 1; i <= 20; i++)
                {
                    string timePropertyName = $"Time{i}";
                    string speedPropertyName = $"Speed{i}";
                    string dosePropertyName = $"Dose{i}";

                    float timeValue = (float)type.GetProperty(timePropertyName).GetValue(this) * (int)this.FeedTimer;
                    float speedValue = (float)type.GetProperty(speedPropertyName).GetValue(this);
                    float doseValue = (float)type.GetProperty(dosePropertyName).GetValue(this);

                    list.Add(Tuple.Create(timeValue, speedValue, doseValue));
                }
                return list;
            }
        }
    }

    public class FormulaStrategy : BindableBase
    {
        private string _formula;
        public string Formula
        {
            get => _formula;
            set { SetProperty(ref _formula, value); }
        }
    }
}
