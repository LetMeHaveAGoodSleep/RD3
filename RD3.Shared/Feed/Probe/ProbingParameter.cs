using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class ProbingParameter : BindableBase
    {
        private float _tmax;
        private float _oreac;
        private float _ops;
        private float _osp;
        private int _nmax1;
        private int _nmax2;
        private float _k;
        private float _ki;
        private float _f;
        private string _deviceID;
        private float _allowDiff;
        private float _m;
        private float _fMax = 200;
        private float _fMin = 1;

        public float Tmax
        {
            get => _tmax;
            set => SetProperty(ref _tmax, value);
        }

        /// <summary>
        /// Do设定值的1-3%
        /// </summary>
        public float Oreac
        {
            get => _oreac;
            set => SetProperty(ref _oreac, value);
        }

        public float Ops
        {
            get => _ops;
            set => SetProperty(ref _ops, value);
        }

        /// <summary>
        /// /Do的setpoint
        /// </summary>
        public float Osp
        {
            get => _osp;
            set => SetProperty(ref _osp, value);
        }

        /// <summary>
        /// 转速限1 Nmax-
        /// </summary>
        public int Nmax1
        {
            get => _nmax1;
            set => SetProperty(ref _nmax1, value);
        }

        /// <summary>
        /// 转速限2 Nmax+
        /// </summary>
        public int Nmax2
        {
            get => _nmax2;
            set => SetProperty(ref _nmax2, value);
        }

        /// <summary>
        /// 减小比例系数
        /// </summary>
        public float M
        {
            get => _m;
            set => SetProperty(ref _m, value);
        }

        /// <summary>
        /// 增大比例系数
        /// </summary>
        public float k
        {
            get => _k;
            set => SetProperty(ref _k, value);
        }

        /// <summary>
        /// [0.5-1] 控制脉冲
        /// </summary>
        public float ki
        {
            get => _ki;
            set => SetProperty(ref _ki, value);
        }

        /// <summary>
        /// 初始补料速度
        /// </summary>
        public float F
        {
            get => _f;
            set => SetProperty(ref _f, value);
        }

        /// <summary>
        /// 最大补料速度
        /// </summary>
        public float FMax
        {
            get => _fMax;
            set => SetProperty(ref _fMax, value);
        }

        /// <summary>
        /// 最小补料速度
        /// </summary>
        public float FMin
        {
            get => _fMin;
            set => SetProperty(ref _fMin, value);
        }

        public string DeviceID
        {
            get => _deviceID;
            set => SetProperty(ref _deviceID, value);
        }

        public float AllowDiff
        {
            get => _allowDiff;
            set => SetProperty(ref _allowDiff, value);
        }
    }
}
