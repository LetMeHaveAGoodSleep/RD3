using System;
using System.Collections.Generic;

namespace RD3.Shared
{
    /// <summary>
    /// 智能pH调节控制器（多因素自适应）
    /// </summary>
    public class IntelligentPHController
    {
        // ========== 用户可配置参数 ==========

        // 基础控制参数
        private double _targetPH = 0;
        /// <summary>
        /// 目标pH值（设定值）
        /// 当设定值变化超过0.01时自动重置控制器状态
        /// </summary>
        public double TargetPH
        {
            get => _targetPH;
            set
            {
                if (Math.Abs(_targetPH - value) > 0.01)
                {
                    _targetPH = value;
                    ResetControllerState();
                }
            }
        }

        /// <summary>
        /// 死区范围（单位：pH）
        /// 当|当前pH-目标pH| < DeadZone 时，不进行调节
        /// </summary>
        public double DeadZone { get; set; } = 0.05;

        /// <summary>
        /// 单次最大添加量（单位：mL）
        /// 防止初始阶段添加过量
        /// </summary>
        public double MaxDosingVolume { get; set; } = 10.0;

        /// <summary>
        /// 单次最小添加量（单位：mL）
        /// 确保微调精度
        /// </summary>
        public double MinDosingVolume { get; set; } = 0.1;

        /// <summary>
        /// 基础调节系数（单位：mL/pH）
        /// 表示每偏离目标pH 1.0时需要添加的试剂体积
        /// </summary>
        public double BaseAdjustmentFactor { get; set; } = 2.0;

        // ========== 自适应学习参数 ==========

        /// <summary>
        /// 初始衰减系数（0.1~1.0）
        /// 值越小调节越保守，防过冲能力越强
        /// </summary>
        public double InitialDecayFactor { get; set; } = 0.6;

        /// <summary>
        /// 参数学习速率（0.01~0.1）
        /// 影响自适应参数的调整速度
        /// </summary>
        public double LearningRate { get; set; } = 0.03;

        /// <summary>
        /// 历史数据窗口大小
        /// 用于计算误差趋势（建议3~10）
        /// </summary>
        public int HistoryWindowSize { get; set; } = 5;

        // ========== 混合效率参数 ==========

        /// <summary>
        /// 搅拌转速影响系数（需实验标定）
        /// 公式：效率 += StirringCoeff * currentRPM
        /// </summary>
        public double StirringCoeff { get; set; } = 0.0005;

        /// <summary>
        /// 溶液体积影响系数（需实验标定）
        /// 公式：效率 += VolumeCoeff / currentVolume_L
        /// </summary>
        public double VolumeCoeff { get; set; } = 0.1;

        // ========== 运行时状态 ==========
        private double _currentDecayFactor;
        private readonly Queue<double> _errorHistory = new();
        private bool _isFirstMeasurement = true;
        private double _lastDosingVolume;
        private bool _lastWasAlkali;

        // ========== 核心控制方法 ==========

        /// <summary>
        /// 计算需要添加的试剂体积
        /// </summary>
        /// <param name="currentPH">当前pH传感器读数</param>
        /// <param name="currentRPM">当前搅拌转速（RPM）</param>
        /// <param name="currentVolume_L">当前溶液体积（升）</param>
        /// <returns>
        /// Tuple: 
        ///   bool - 是否加碱（true:碱, false:酸）
        ///   double - 添加体积（mL）
        /// </returns>
        public (bool isAlkali, double volume) CalculateDosing(
            double currentPH,
            int currentRPM,
            double currentVolume_L)
        {
            // 1. 计算当前误差
            double error = TargetPH - currentPH;

            // 2. 检查死区范围
            if (Math.Abs(error) < DeadZone)
            {
                ResetControllerState();
                return (false, 0);
            }

        // 3. 计算混合效率（0~1范围）
        double mixingEfficiency = CalculateMixingEfficiency(currentRPM, currentVolume_L);

            // 4. 计算基础添加量（考虑混合效率）
            double baseVolume = Math.Abs(error) * BaseAdjustmentFactor * (1.2 - mixingEfficiency);

            // 5. 应用自适应控制逻辑
            double adjustedVolume = ApplyAdaptiveControl(baseVolume, error);

            // 6. 更新系统状态
            UpdateSystemState(error, adjustedVolume);

            return (
                isAlkali: error > 0,
                volume: Math.Clamp(adjustedVolume, MinDosingVolume, MaxDosingVolume)
            );
        }

        // ========== 私有方法 ==========

        /// <summary>
        /// 计算混合效率（0~1范围）
        /// 效率越高表示混合越快，所需添加量可以越少
        /// </summary>
        private double CalculateMixingEfficiency(int rpm, double volume_L)
        {
            // 基础效率（0.5表示中等混合速度）
            double efficiency = 0.5;

            // 搅拌转速影响（转速越高效率越高）
            efficiency += StirringCoeff * rpm;

            // 溶液体积影响（体积越小效率越高）
            if (volume_L > 0.1) // 防止除以0
                efficiency += VolumeCoeff / volume_L;

            // 限制在合理范围
            return Math.Clamp(efficiency, 0.1, 0.95);
        }

        /// <summary>
        /// 应用自适应控制逻辑
        /// </summary>
        private double ApplyAdaptiveControl(double baseVolume, double error)
        {
            // 首次测量不应用衰减
            if (_isFirstMeasurement)
                return baseVolume;

            // 1. 应用当前衰减系数
            double adjustedVolume = baseVolume * _currentDecayFactor;

            // 2. 反向趋势检测（防过冲）
            if (IsOvershootTrend(error))
            {
                adjustedVolume *= 0.6; // 大幅减少本次添加量
            }

            // 3. 误差持续减小时适当增加步长
            if (IsConsistentImprovement())
            {
                adjustedVolume *= 1.2; // 适当增加添加量
            }

            return adjustedVolume;
        }

        /// <summary>
        /// 检测是否出现过冲趋势
        /// </summary>
        private bool IsOvershootTrend(double currentError)
        {
            if (_errorHistory.Count < 2) return false;

            // 检查误差符号是否反转
            double lastError = _errorHistory.Peek();
            return (lastError > 0 && currentError < 0) ||
                   (lastError < 0 && currentError > 0);
        }

        /// <summary>
        /// 检测误差是否持续减小
        /// </summary>
        private bool IsConsistentImprovement()
        {
            if (_errorHistory.Count < 3) return false;

            // 检查最近3次误差是否持续减小
            double[] lastErrors = _errorHistory.ToArray();
            return Math.Abs(lastErrors[0]) < Math.Abs(lastErrors[1]) &&
                   Math.Abs(lastErrors[1]) < Math.Abs(lastErrors[2]);
        }

        /// <summary>
        /// 更新系统状态和自适应参数
        /// </summary>
        private void UpdateSystemState(double error, double adjustedVolume)
        {
            // 记录历史误差（滑动窗口）
            _errorHistory.Enqueue(error);
            if (_errorHistory.Count > HistoryWindowSize)
                _errorHistory.Dequeue();

            // 更新衰减系数
            UpdateDecayFactor();

            _isFirstMeasurement = false;
        }

        /// <summary>
        /// 动态调整衰减系数
        /// </summary>
        private void UpdateDecayFactor()
        {
            if (_errorHistory.Count < 3) return;

            // 计算平均绝对误差
            double avgError = 0;
            foreach (var err in _errorHistory) avgError += Math.Abs(err);
            avgError /= _errorHistory.Count;

            // 根据误差水平调整衰减系数
            _currentDecayFactor = avgError switch
            {
                < 0.3 => Math.Min(0.9, _currentDecayFactor + LearningRate), // 误差小时激进
                > 0.7 => Math.Max(0.3, _currentDecayFactor - LearningRate), // 误差大时保守
                _ => _currentDecayFactor
            };
        }

        /// <summary>
        /// 重置控制器状态（目标pH变化时调用）
        /// </summary>
        private void ResetControllerState()
        {
            _errorHistory.Clear();
            _currentDecayFactor = InitialDecayFactor;
            _isFirstMeasurement = true;
        }
    }
}
