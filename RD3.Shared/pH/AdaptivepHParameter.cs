using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class AdaptivepHParameter : BindableBase
    {
        // ========== 基础控制参数 ==========
        private double _targetPH = 7.0;
        /// <summary>
        /// 目标pH设定值
        /// 范围建议：0-14 pH
        /// 精度：0.01 pH
        /// 修改此值会自动重置控制器状态
        /// </summary>
        public double TargetPH
        {
            get => _targetPH;
            set => SetProperty(ref _targetPH, value);
        }

        private double _deadZone = 0.05;
        /// <summary>
        /// 死区范围（pH单位）
        /// 当|当前pH-目标pH| < DeadZone时停止调节
        /// 范围建议：0.01-0.5 pH
        /// </summary>
        public double DeadZone
        {
            get => _deadZone;
            set => SetProperty(ref _deadZone, value);
        }

        private double _maxDosingVolume = 10.0;
        /// <summary>
        /// 单次最大添加体积（mL）
        /// 防止初始阶段添加过量
        /// 应根据反应容器大小设置
        /// </summary>
        public double MaxDosingVolume
        {
            get => _maxDosingVolume;
            set => SetProperty(ref _maxDosingVolume, value);
        }

        private double _minDosingVolume = 0.1;
        /// <summary>
        /// 单次最小添加体积（mL）
        /// 确保微调精度
        /// 不得小于泵的最小流量
        /// </summary>
        public double MinDosingVolume
        {
            get => _minDosingVolume;
            set => SetProperty(ref _minDosingVolume, value);
        }

        private double _baseAdjustmentFactor = 1.0;
        /// <summary>
        /// 基础调节系数（mL/pH）
        /// 表示每偏离目标pH 1.0时需要添加的试剂体积
        /// 需根据实际化学反应标定
        /// </summary>
        public double BaseAdjustmentFactor
        {
            get => _baseAdjustmentFactor;
            set => SetProperty(ref _baseAdjustmentFactor, value);
        }

        // ========== 自适应参数 ==========
        private double _initialDecayFactor = 0.3;
        /// <summary>
        /// 初始衰减系数（0.1-1.0）
        /// 值越小调节越保守，防过冲能力越强
        /// 建议启动时设为0.3-0.5
        /// </summary>
        public double InitialDecayFactor
        {
            get => _initialDecayFactor;
            set => SetProperty(ref _initialDecayFactor, value);
        }

        private double _learningRate = 0.01;
        /// <summary>
        /// 参数学习速率（0.01-0.1）
        /// 影响自适应参数的调整速度
        /// 值越大响应越快但可能不稳定
        /// </summary>
        public double LearningRate
        {
            get => _learningRate;
            set => SetProperty(ref _learningRate, value);
        }

        private int _historyWindowSize = 5;
        /// <summary>
        /// 历史数据窗口大小（3-20）
        /// 用于计算误差趋势
        /// 值越大系统响应越平缓
        /// </summary>
        public int HistoryWindowSize
        {
            get => _historyWindowSize;
            set => SetProperty(ref _historyWindowSize, value);
        }

        // ========== 混合效率参数 ==========
        private double _stirringCoeff = 0.0005;
        /// <summary>
        /// 搅拌转速影响系数
        /// 公式：混合效率 += StirringCoeff * RPM
        /// 需通过实验标定
        /// 典型值：0.0003-0.001
        /// </summary>
        public double StirringCoeff
        {
            get => _stirringCoeff;
            set => SetProperty(ref _stirringCoeff, value);
        }

        private double _volumeCoeff = 0.1;
        /// <summary>
        /// 溶液体积影响系数
        /// 公式：混合效率 += VolumeCoeff / 体积(L)
        /// 需通过实验标定
        /// 典型值：0.05-0.3
        /// </summary>
        public double VolumeCoeff
        {
            get => _volumeCoeff;
            set => SetProperty(ref _volumeCoeff, value);
        }
    }
}
