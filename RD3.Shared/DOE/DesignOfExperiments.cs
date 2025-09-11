using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using RD3;
using RD3.Shared;

namespace RD3.Shared
{
    /// <summary>
    /// 实验设计工具类，提供各种实验设计方法
    /// </summary>
    public static class DesignOfExperiments
    {
        #region 公共方法
        /// <summary>
        /// 生成全因子试验设计矩阵（Full Factorial Design Matrix）
        /// 全因子设计会包含所有因子水平的可能组合，适用于探索因子间的主效应和交互效应
        /// </summary>
        /// <param name="factorLevelRanges">因子水平范围字典
        /// 键：因子名称（如"Pressure"）
        /// 值：该因子的所有水平列表（如[50, 60, 70]）
        /// </param>
        /// <returns>全因子设计矩阵（Matrix<double>），每行代表一个试验组合，每列对应一个因子的水平值</returns>
        /// <exception cref="ArgumentException">当输入的因子水平列表为空时抛出</exception>
        public static Matrix<double> BuildFullFactDesign(IList<OrthogonalParam> paramCol)
        {
            int factorCount = paramCol.Count;
            int[] levels = new int[factorCount];
            for (int i = 0; i < factorCount; i++)
            {
                levels[i] = 2;
            }
            List<double[]> list = paramCol.Select(item => new double[] { item.Low, item.High }).ToList();

            //生成全因子设计的基础索引矩阵（元素为水平索引，如0,1,2）
            var matrix = FullFact(levels);

            //将索引矩阵映射到实际因子水平值
            Matrix<double> result = DenseMatrix.Create(matrix.RowCount, matrix.ColumnCount, 0.0);
            for (int row = 0; row < matrix.RowCount; row++)
            {
                for (int col = 0; col < matrix.ColumnCount; col++)
                {
                    // 获取当前位置的水平索引（转换为整数）
                    int levelIndex = (int)matrix[row, col];
                    // 映射到实际水平值
                    result[row, col] = list[col][levelIndex];
                }
            }
            return result;
        }

        /// <summary>
        /// 构建2水平部分因子设计矩阵（Fractional Factorial Design）
        /// 部分因子设计通过牺牲交互效应的可识别性，减少试验次数，适用于因子较多但资源有限的场景
        /// </summary>
        /// <param name="factorLevelRanges">因子水平范围字典
        /// 键：因子名称（如"Pressure"）
        /// 值：该因子的水平列表（仅需最小和最大值，如[50, 70]）
        /// </param>
        /// <param name="genString">生成器字符串，定义因子间的混淆结构
        /// 例如："a b ab"表示第三个因子是前两个因子的交互效应
        /// 支持正负号调整交互效应方向，如"a b -ab"
        /// </param>
        /// <returns>部分因子设计矩阵（Matrix<double>），每行代表一个试验组合，每列对应一个因子的水平值</returns>
        /// <exception cref="ArgumentException">当输入参数不满足要求时抛出（如因子水平数不为2、生成器字符串长度不匹配等）</exception>
        public static Matrix<double> BuildFracFactDesign(IList<OrthogonalParam> paramCol, string genString)
        {
            //验证生成器字符串与因子数量匹配
            int factorCount = paramCol.Count;
            string[] genParts = genString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (genParts.Length != factorCount)
            {
                throw new ArgumentException(
                    "生成器字符串的元素数量与因子数量不匹配",
                    nameof(genString)
                );
            }

            //提取因子水平列表（按字典键顺序排列）
            List<double[]> list = paramCol.Select(item => new double[] { item.Low, item.High }).ToList();

            // 生成部分因子设计的基础矩阵（元素为-1和1，代表高低水平编码）
            Matrix<double> matrix = FracFactDesign(genString);

            //将编码矩阵（-1/1）转换为实际水平值（映射到因子的高低水平）
            Matrix<double> result = DenseMatrix.Create(matrix.RowCount, matrix.ColumnCount, 0.0);
            for (int row = 0; row < matrix.RowCount; row++)
            {
                for (int col = 0; col < matrix.ColumnCount; col++)
                {
                    // 获取当前位置的水平索引（转换为整数）
                    int levelIndex = (int)matrix[row, col];
                    // 映射到实际水平值
                    result[row, col] = list[col][levelIndex];
                }
            }
            return result;
        }

        /// <summary>
        /// 生成 Box-Behnken 设计矩阵
        /// </summary>
        /// <param name="factorCount">因子数量（必须 ≥3）</param>
        /// <param name="centerPoints">中心点重复次数（若为 null 则使用默认值）</param>
        /// <returns>Box-Behnken 设计矩阵，类型为 Matrix<double></returns>
        public static Matrix<double> BuildBoxBehnkenDesign(IList<OrthogonalParam> paramCol, int? centerPoints = null)
        {
            int factorCount = paramCol.Count;
            // 验证因子数量至少为3
            if (factorCount < 3)
                throw new ArgumentException("因子数量必须至少为3", nameof(factorCount));

            List<double[]> list = paramCol.Select(item => new double[] { item.Low, (item.Low + item.High) / 2, item.High }).ToList();

            var matrix = BBDesignCorrected(factorCount, centerPoints);
            return MapToFactorLevels(matrix, list);
        }

        /// <summary>
        /// 构建 Plackett-Burman 设计矩阵（基于因子范围字典）
        /// Plackett-Burman 设计适用于筛选大量因子，通过少量实验确定主要影响因素，假设因子间交互作用可忽略
        /// </summary>
        /// <param name="factorLevelRanges">因子范围字典，键为因子名称，值为包含最小和最大值的列表（若超过2个值则自动取首尾）</param>
        /// <returns>包含实验设计的 DataTable，列名为因子名，行为实验组合</returns>
        public static Matrix<double> BuildPlackettBurmanDesign(IList<OrthogonalParam> paramCol)
        {

            int factorCount = paramCol.Count;
            List<double[]> factorLists = paramCol.Select(item => new double[] { item.Low, item.High }).ToList();

            // 生成 Plackett-Burman 设计矩阵（元素为 -1 和 1）
            Matrix<double> designMatrix = PBDesign(factorCount);

            int rowCount = designMatrix.RowCount;
            int colCount = designMatrix.ColumnCount;
            Matrix<double> result = DenseMatrix.OfArray(new double[rowCount, colCount]);

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    double code = designMatrix[row, col];
                    // 根据编码（-1,1）映射到对应的因子水平
                    int index = code switch
                    {
                        -1 => 0,
                        1 => 1,
                        _ => throw new InvalidOperationException("无效的编码值，必须为-1或1")
                    };
                    result[row, col] = factorLists[col][index];
                }
            }
            return result;
        }

        /// <summary>
        /// 生成中心复合设计（Central Composite Design）矩阵
        /// 中心复合设计是响应面设计的一种，结合了析因设计、星点设计和中心点，用于拟合二次响应面模型
        /// </summary>
        /// <param name="n">因子数量（必须大于1的整数）</param>
        /// <param name="center">长度为2的数组，分别指定析因块和星点块的中心点数量</param>
        /// <param name="alpha"> alpha值的计算方式：
        /// - "orthogonal" 或 "o"：正交设计（默认）
        /// - "rotatable" 或 "r"：旋转设计（各点预测方差仅与到中心的距离有关）
        /// </param>
        /// <param name="face">星点与析因点的关系：
        /// - "circumscribed" 或 "ccc"：外切设计（星点在析因点外，默认）
        /// - "inscribed" 或 "cci"：内切设计（星点在因子极限处，析因点按alpha缩放）
        /// - "faced" 或 "ccf"：面心设计（星点在析因面中心，alpha=1）
        /// </param>
        /// <returns>编码水平为-1和1的设计矩阵（Matrix<double>）</returns>
        public static Matrix<double> BuildCCDDesign(IList<OrthogonalParam> paramCol, (int center1, int center2) centers, DOEAlpha alpha, DOEFace face)
        {
            if (paramCol.Count <= 1)
                throw new ArgumentException("因子数量必须大于1");

            if (centers.Item1 < 0 || centers.Item2 < 0)
                throw new ArgumentException("中心点数量不能为负数", nameof(centers));

            List<double[]> list = paramCol.Select(item => new double[] { item.Low, (item.Low + item.High) / 2, item.High }).ToList();

            // 初始化析因矩阵(H1)和星点矩阵(H2)
            Matrix<double> H1 = null;
            Matrix<double> H2 = null;
            double alphaValue = 0;

            switch (alpha)
            {
                case DOEAlpha.Orthogonal:
                    (H2, alphaValue) = Star(paramCol.Count, centers, DOEAlpha.Orthogonal);
                    break;
                case DOEAlpha.Rotatable:
                    (H2, alphaValue) = Star(paramCol.Count, centers, DOEAlpha.Rotatable);
                    break;
            }

            // 根据face类型处理析因矩阵和星点矩阵
            switch (face)
            {
                case DOEFace.Inscribed:
                    // 内切设计：析因点按alpha缩放，星点使用默认生成
                    H1 = Ff2n(paramCol.Count);
                    H1 = H1.Divide(alphaValue); // 缩放析因点到星点范围内
                    (H2, alphaValue) = Star(paramCol.Count, centers); // 重新生成星点确保一致性
                    break;

                case DOEFace.Faced:
                    // 面心设计：星点在面中心(alpha=1)，析因点使用全因子
                    (H2, alphaValue) = Star(paramCol.Count, centers);
                    H1 = Ff2n(paramCol.Count);
                    break;

                case DOEFace.Circumscribed:
                    // 外切设计：析因点使用全因子，星点在析因点外部
                    H1 = Ff2n(paramCol.Count);
                    break;
            }

            // 生成析因块和星点块的中心点矩阵
            Matrix<double> centerPoints1 = RepeatCenter(paramCol.Count, centers.center1);
            Matrix<double> centerPoints2 = RepeatCenter(paramCol.Count, centers.center2);

            // 合并析因点与析因块中心点
            Matrix<double> factorialBlock = H1.Stack(centerPoints1);
            // 合并星点与星点块中心点
            Matrix<double> starBlock = H2.Stack(centerPoints2);
            // 合并所有点得到最终设计矩阵
            Matrix<double> designMatrix = factorialBlock.Stack(starBlock);

            var result = ConstructDfFromMatrix(designMatrix, list);
            return result;
        }

        /// <summary>
        /// 生成拉丁超立方抽样(Latin Hypercube Sampling)设计矩阵
        /// </summary>
        /// <param name="factorLevelRanges">因子水平范围字典，键为因子名称，值为包含最小和最大值的数组</param>
        /// <param name="numSamples">抽样数量，若为null则默认等于因子数量</param>
        /// <param name="probDistribution">概率分布类型，支持"Normal"、"Poisson"等，null表示均匀分布</param>
        /// <returns>拉丁超立方设计矩阵，每行代表一个样本，每列对应一个因子</returns>
        public static Matrix<double> BuildLhsDesign(IList<OrthogonalParam> paramCol, int numSamples, ProbDistribution probDistribution, Criterion criterion, int iterations = 5)
        {
            List<double[]> list = paramCol.Select(item => new double[] { item.Low, item.High }).ToList();
            int factorCount = paramCol.Count;
            Matrix<double> lhsMatrix = null;
            if (criterion == Criterion.None)
            {
                lhsMatrix = LhsClassic(factorCount, numSamples);
            }
            if (lhsMatrix == null)
            {
                switch (criterion)
                {
                    case Criterion.Center:
                        lhsMatrix = LhsCentered(factorCount, numSamples);
                        break;
                    case Criterion.Maximin:
                        lhsMatrix = LhsMaximin(factorCount, numSamples, iterations, Criterion.Maximin);
                        break;
                    case Criterion.CenterMaximin:
                        lhsMatrix = LhsMaximin(factorCount, numSamples, iterations, Criterion.CenterMaximin);
                        break;
                    case Criterion.Correlation:
                        lhsMatrix = LhsCorrelate(factorCount, numSamples, iterations);
                        break;
                }
            }
            var result = ConstructDfFromRandomMatrix(lhsMatrix, list);
            return result;
        }
        #endregion

        #region 辅助方法

        /// <summary>
        /// 生成经典拉丁超立方抽样设计矩阵
        /// （将每个因子的取值范围[0,1]均匀划分为samples个子区间，在每个子区间内随机采样，
        /// 并对每个因子的采样结果进行随机置换，确保每个区间在每行中仅出现一次）
        /// </summary>
        /// <param name="n">因子数量（设计矩阵的列数）</param>
        /// <param name="samples">每个因子的采样数量（设计矩阵的行数）</param>
        /// <returns>归一化的拉丁超立方设计矩阵（元素值在[0,1]范围内）</returns>
        public static Matrix<double> LhsClassic(int n, int samples)
        {
            // 1. 生成均匀分布的区间分割点
            // 将[0,1]区间等分为samples个子区间，得到samples+1个分割点
            // 例如：samples=3时，分割点为[0, 1/3, 2/3, 1]
            double[] cutPoints = new double[samples + 1];
            for (int i = 0; i <= samples; i++)
            {
                cutPoints[i] = (double)i / samples;
            }

            // 2. 在每个区间内生成随机点
            // rdPoints用于存储每个区间内的随机采样值，维度为samples×n
            double[,] rdPoints = new double[samples, n];
            Random random = new Random();

            for (int factor = 0; factor < n; factor++)  // 遍历每个因子
            {
                for (int sample = 0; sample < samples; sample++)  // 遍历每个采样点
                {
                    // 当前区间的左边界（第sample个区间的起点）
                    double intervalStart = cutPoints[sample];
                    // 当前区间的右边界（第sample个区间的终点）
                    double intervalEnd = cutPoints[sample + 1];
                    // 区间长度
                    double intervalLength = intervalEnd - intervalStart;
                    // 在[0,1)内生成随机数，缩放至当前区间内
                    double randomValue = intervalStart + random.NextDouble() * intervalLength;
                    rdPoints[sample, factor] = randomValue;
                }
            }

            // 3. 对每个因子的采样结果进行随机置换
            // 确保每个区间在设计矩阵的每行中仅出现一次（拉丁超立方的核心特性）
            double[,] designMatrix = new double[samples, n];
            for (int factor = 0; factor < n; factor++)
            {
                // 生成0到samples-1的随机置换索引（打乱顺序）
                int[] permutation = GetRandomPermutation(samples, random);

                // 根据置换索引重新排列当前因子的采样值
                for (int sample = 0; sample < samples; sample++)
                {
                    designMatrix[sample, factor] = rdPoints[permutation[sample], factor];
                }
            }

            // 将二维数组转换为MathNet的Matrix<double>返回
            return DenseMatrix.OfArray(designMatrix);
        }

        /// <summary>
        /// 生成中心拉丁超立方抽样设计矩阵（每个区间内的样本点固定在区间中心）
        /// 核心逻辑：将每个因子的[0,1]区间均匀划分为samples个子区间，取每个区间的中点作为候选点，
        /// 然后对每个因子的中点集合进行随机置换，确保拉丁超立方的"每行每列仅出现一个区间"特性
        /// </summary>
        /// <param name="n">因子数量（设计矩阵的列数）</param>
        /// <param name="samples">每个因子的采样数量（设计矩阵的行数）</param>
        /// <returns>归一化的中心拉丁超立方设计矩阵（元素值在[0,1]范围内）</returns>
        public static Matrix<double> LhsCentered(int n, int samples)
        {
            // 1. 生成均匀分布的区间分割点
            // 将[0,1]区间等分为samples个子区间，得到samples+1个分割点
            // 例如：samples=3时，分割点为[0, 1/3, 2/3, 1]
            double[] cutPoints = new double[samples + 1];
            for (int i = 0; i <= samples; i++)
            {
                cutPoints[i] = (double)i / samples;
            }

            // 2. 计算每个区间的中点
            // 每个区间[cutPoints[i], cutPoints[i+1]]的中点为(cutPoints[i] + cutPoints[i+1])/2
            double[] centers = new double[samples];
            for (int i = 0; i < samples; i++)
            {
                centers[i] = (cutPoints[i] + cutPoints[i + 1]) / 2.0;
            }

            // 3. 初始化设计矩阵（行数为samples，列数为n）
            double[,] designMatrix = new double[samples, n];
            Random random = new Random();

            // 4. 对每个因子的中点集合进行随机置换
            // 确保每个区间的中点在设计矩阵的每行中仅出现一次
            for (int factor = 0; factor < n; factor++)
            {
                // 生成0到samples-1的随机置换索引（打乱中点顺序）
                int[] permutation = GetRandomPermutation(samples, random);

                // 根据置换索引重新排列当前因子的中点，填充设计矩阵
                for (int sample = 0; sample < samples; sample++)
                {
                    designMatrix[sample, factor] = centers[permutation[sample]];
                }
            }

            // 将二维数组转换为MathNet的Matrix<double>返回
            return DenseMatrix.OfArray(designMatrix);
        }

        /// <summary>
        /// 生成最大化最小距离的拉丁超立方抽样设计矩阵
        /// 核心逻辑：通过多次迭代生成候选设计，选择样本点间最小距离最大的设计，
        /// 支持两种基础抽样方式（经典随机抽样或中心抽样）
        /// </summary>
        /// <param name="n">因子数量（设计矩阵的列数）</param>
        /// <param name="samples">每个因子的采样数量（设计矩阵的行数）</param>
        /// <param name="iterations">迭代次数，每次迭代生成一个候选设计并评估</param>
        /// <param name="lhstype">基础抽样类型："maximin" 表示经典随机抽样，"centermaximin" 表示中心抽样</param>
        /// <returns>优化后的拉丁超立方设计矩阵（元素值在[0,1]范围内）</returns>
        public static Matrix<double> LhsMaximin(int n, int samples, int iterations, Criterion criterion)
        {
            // 初始化最大最小距离为0（记录最优设计的最小距离）
            double maxMinDistance = 0;
            // 初始化最优设计矩阵
            Matrix<double> bestDesign = null;

            // 迭代生成候选设计并筛选最优解
            for (int i = 0; i < iterations; i++)
            {
                // 根据基础抽样类型生成候选设计
                Matrix<double> candidateDesign;
                if (criterion == Criterion.Maximin)
                {
                    // 生成经典随机抽样的拉丁超立方设计
                    candidateDesign = LhsClassic(n, samples);
                }
                else if (criterion == Criterion.CenterMaximin)
                {
                    // 生成中心抽样的拉丁超立方设计
                    candidateDesign = LhsCentered(n, samples);
                }
                else
                {
                    throw new ArgumentException($"无效的抽样类型: {criterion}，仅支持 'maximin' 或 'centermaximin'");
                }

                // 计算当前候选设计中所有样本点对的距离
                double[] pairwiseDistances = CalculatePairwiseDistances(candidateDesign);

                // 忽略空设计的情况（样本量小于2时无距离可计算）
                if (pairwiseDistances.Length == 0)
                    continue;

                // 找到当前候选设计中最小的点对距离
                double currentMinDistance = pairwiseDistances.Min();

                // 如果当前设计的最小距离大于历史最优，更新最优设计
                if (currentMinDistance > maxMinDistance)
                {
                    maxMinDistance = currentMinDistance;
                    bestDesign = candidateDesign.Clone(); // 深拷贝避免引用问题
                }
            }

            // 如果未生成有效设计（如迭代次数为0或样本量不足），返回空矩阵
            return bestDesign ?? DenseMatrix.OfArray(new double[0, 0]);
        }

        /// <summary>
        /// 生成最小化最大相关系数的拉丁超立方抽样设计矩阵
        /// 核心逻辑：通过多次迭代生成候选设计，选择因子间最大相关系数最小的设计，
        /// 确保样本点在各因子维度上的相关性尽可能低
        /// </summary>
        /// <param name="n">因子数量（设计矩阵的列数）</param>
        /// <param name="samples">每个因子的采样数量（设计矩阵的行数）</param>
        /// <param name="iterations">迭代次数，每次迭代生成一个候选设计并评估相关性</param>
        /// <returns>优化后的拉丁超立方设计矩阵（元素值在[0,1]范围内）</returns>
        public static Matrix<double> LhsCorrelate(int n, int samples, int iterations)
        {
            // 初始化最小最大相关系数为正无穷（记录最优设计的最大相关系数）
            double minMaxCorrelation = double.PositiveInfinity;
            // 初始化最优设计矩阵
            Matrix<double> bestDesign = null;

            // 迭代生成候选设计并筛选最优解
            for (int i = 0; i < iterations; i++)
            {
                // 生成经典随机抽样的拉丁超立方设计作为候选
                Matrix<double> candidateDesign = LhsClassic(n, samples);

                // 计算候选设计的相关系数矩阵
                Matrix<double> correlationMatrix = CalculateCorrelationMatrix(candidateDesign);

                // 找到当前候选设计中最大的非对角线相关系数（排除自身相关的1）
                double currentMaxCorrelation = FindMaxOffDiagonalCorrelation(correlationMatrix);

                // 如果当前设计的最大相关系数小于历史最优，更新最优设计
                if (currentMaxCorrelation < minMaxCorrelation)
                {
                    minMaxCorrelation = currentMaxCorrelation;
                    bestDesign = candidateDesign.Clone(); // 深拷贝避免引用问题
                                                          // 可在此处添加日志输出，跟踪优化过程
                                                          // Console.WriteLine($"新候选解：最大相关系数 = {minMaxCorrelation}");
                }
            }

            // 如果未生成有效设计（如迭代次数为0或样本量不足），返回空矩阵
            return bestDesign ?? DenseMatrix.OfArray(new double[0, 0]);
        }

        /// <summary>
        /// 计算设计矩阵中所有样本点对的欧氏距离
        /// </summary>
        /// <param name="design">拉丁超立方设计矩阵（行数为样本数，列数为因子数）</param>
        /// <returns>所有点对的距离数组（长度为 m*(m-1)/2，其中 m 为样本数）</returns>
        private static double[] CalculatePairwiseDistances(Matrix<double> design)
        {
            int sampleCount = design.RowCount;
            if (sampleCount < 2)
                return Array.Empty<double>(); // 样本量小于2时无距离可计算

            // 计算点对数量：m个样本的点对数量为 m*(m-1)/2
            int distanceCount = sampleCount * (sampleCount - 1) / 2;
            double[] distances = new double[distanceCount];
            int index = 0;

            // 遍历所有点对（i < j 避免重复计算）
            for (int i = 0; i < sampleCount - 1; i++)
            {
                for (int j = i + 1; j < sampleCount; j++)
                {
                    // 计算样本i和样本j的欧氏距离
                    double squaredDistance = 0;
                    for (int k = 0; k < design.ColumnCount; k++)
                    {
                        double diff = design[i, k] - design[j, k];
                        squaredDistance += diff * diff;
                    }
                    distances[index] = Math.Sqrt(squaredDistance);
                    index++;
                }
            }
            return distances;
        }

        /// <summary>
        /// 计算设计矩阵的相关系数矩阵
        /// 相关系数矩阵R中，R[i,j]表示第i个因子与第j个因子的 Pearson 相关系数
        /// </summary>
        /// <param name="design">拉丁超立方设计矩阵（行数为样本数，列数为因子数）</param>
        /// <returns>相关系数矩阵（方阵，大小为因子数×因子数）</returns>
        private static Matrix<double> CalculateCorrelationMatrix(Matrix<double> design)
        {
            int factorCount = design.ColumnCount;
            Matrix<double> correlationMatrix = DenseMatrix.Create(factorCount, factorCount, 0);

            for (int i = 0; i < factorCount; i++)
            {
                // 对角线元素为1（自身相关系数）
                correlationMatrix[i, i] = 1.0;

                for (int j = i + 1; j < factorCount; j++)
                {
                    // 计算因子i和因子j的相关系数
                    double corr = CalculatePearsonCorrelation(design.Column(i), design.Column(j));
                    correlationMatrix[i, j] = corr;
                    correlationMatrix[j, i] = corr; // 相关矩阵对称
                }
            }

            return correlationMatrix;
        }

        /// <summary>
        /// 计算两个因子（向量）之间的 Pearson 相关系数
        /// </summary>
        /// <param name="x">第一个因子的样本向量</param>
        /// <param name="y">第二个因子的样本向量</param>
        /// <returns>Pearson 相关系数（范围[-1,1]）</returns>
        private static double CalculatePearsonCorrelation(Vector<double> x, Vector<double> y)
        {
            int n = x.Count;
            if (n != y.Count)
                throw new ArgumentException("两个因子的样本数量必须相同");

            // 计算均值
            double meanX = x.Average();
            double meanY = y.Average();

            // 计算分子（协方差×n）和分母（标准差乘积×n）
            double numerator = 0;
            double sumSqX = 0;
            double sumSqY = 0;

            for (int i = 0; i < n; i++)
            {
                double diffX = x[i] - meanX;
                double diffY = y[i] - meanY;
                numerator += diffX * diffY;
                sumSqX += diffX * diffX;
                sumSqY += diffY * diffY;
            }

            // 避免除以零（若某个因子无波动）
            if (sumSqX == 0 || sumSqY == 0)
                return 0;

            return numerator / Math.Sqrt(sumSqX * sumSqY);
        }

        /// <summary>
        /// 查找相关系数矩阵中最大的非对角线元素（即最大的因子间相关系数）
        /// </summary>
        /// <param name="correlationMatrix">相关系数矩阵</param>
        /// <returns>最大非对角线相关系数</returns>
        private static double FindMaxOffDiagonalCorrelation(Matrix<double> correlationMatrix)
        {
            int size = correlationMatrix.RowCount;
            double maxCorr = 0;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (i != j) // 排除对角线元素
                    {
                        double absCorr = Math.Abs(correlationMatrix[i, j]);
                        if (absCorr > maxCorr)
                        {
                            maxCorr = absCorr;
                        }
                    }
                }
            }

            return maxCorr;
        }

        /// <summary>
        /// 生成0到length-1的随机置换索引
        /// </summary>
        /// <param name="length">置换的长度</param>
        /// <param name="random">随机数生成器实例</param>
        /// <returns>随机排序的索引数组</returns>
        private static int[] GetRandomPermutation(int length, Random random)
        {
            int[] permutation = new int[length];
            // 初始化索引：0,1,2,...,length-1
            for (int i = 0; i < length; i++)
            {
                permutation[i] = i;
            }

            //  Fisher-Yates洗牌算法：随机交换元素，生成置换
            for (int i = length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1); // 生成[0,i]之间的随机索引
                                            // 交换元素
                (permutation[i], permutation[j]) = (permutation[j], permutation[i]);
            }

            return permutation;
        }

        /// <summary>
        /// 生成星点设计矩阵（轴向点）
        /// </summary>
        /// <param name="n">因子数量</param>
        /// <param name="alphaType">alpha计算方式（orthogonal/rotatable/faced）</param>
        /// <param name="center">中心点数量（用于正交设计计算）</param>
        /// <returns>星点矩阵和对应的alpha值</returns>
        private static (Matrix<double> starMatrix, double alpha) Star(int n, (int center1, int center2) centers, DOEAlpha alphaType = DOEAlpha.Faced)
        {
            double alpha = 0;

            switch (alphaType)
            {
                case DOEAlpha.Faced:
                    // 面心设计固定alpha=1
                    alpha = 1.0;
                    break;

                case DOEAlpha.Orthogonal:
                    // 正交设计：alpha确保析因块和星点块正交
                    int factorialPoints = (int)Math.Pow(2, n);
                    int factorialCenter = centers.Item1;
                    int starPoints = 2 * n;
                    int starCenter = centers.Item2;
                    alpha = Math.Sqrt(
                        n * (1 + (double)starCenter / starPoints) /
                        (1 + (double)factorialCenter / factorialPoints)
                    );
                    break;

                case DOEAlpha.Rotatable:
                    // 旋转设计：alpha = (2^n)^0.25（确保旋转性）
                    alpha = Math.Pow(Math.Pow(2, n), 0.25);
                    break;
            }

            // 生成星点矩阵：每个因子对应两个轴向点（-alpha和+alpha）
            int rowCount = 2 * n;
            var starMatrix = DenseMatrix.Create(rowCount, n, 0);

            for (int i = 0; i < n; i++)
            {
                starMatrix[2 * i, i] = -alpha;   // 负向星点
                starMatrix[2 * i + 1, i] = alpha; // 正向星点
            }

            return (starMatrix, alpha);
        }

        /// <summary>
        /// 创建2水平部分因子设计矩阵（Fractional Factorial Design）
        /// 该设计通过生成器字符串定义因子间的混淆结构，以减少试验次数
        /// </summary>
        /// <param name="gen">生成器字符串，包含小写/大写字母及"+"/"-"运算符
        /// 例如："a b ab"表示第三个因子是前两个因子的交互效应；"a b -ab"表示第三个因子是前两个因子交互效应的相反数</param>
        /// <returns>部分因子设计矩阵（Matrix<double>），元素为-1和1表示高低水平编码</returns>
        /// <exception cref="ArgumentException">当生成器字符串格式无效时抛出</exception>
        public static Matrix<double> FracFactDesign(string gen)
        {
            // 1. 解析生成器字符串，提取因子项（去除空字符串）
            // 使用正则分割处理包含+-符号的项，例如将"a b -ab"分割为["a", "b", "ab"]
            var splitPattern = new Regex(@"\-?\s?\+?");
            var terms = splitPattern.Split(gen)
                                   .Where(item => !string.IsNullOrWhiteSpace(item))
                                   .Select(item => item.Trim())
                                   .ToList();

            if (terms.Count == 0)
            {
                throw new ArgumentException("生成器字符串格式无效，未找到有效因子项", nameof(gen));
            }

            // 2. 确定主因子（单个字母）和交互项（多个字母组合）的索引
            var termLengths = terms.Select(t => t.Length).ToList();
            var mainFactorIndices = termLengths.Select((len, idx) => new { len, idx })
                                               .Where(x => x.len == 1)
                                               .Select(x => x.idx)
                                               .ToList();
            var interactionIndices = termLengths.Select((len, idx) => new { len, idx })
                                                .Where(x => x.len != 1)
                                                .Select(x => x.idx)
                                                .ToList();

            // 3. 解析运算符（+/-）的位置
            var splitBySpace = gen.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var positiveIndices = Grep(splitBySpace, "+"); // 包含"+"的项索引
            var negativeIndices = Grep(splitBySpace, "-"); // 包含"-"的项索引

            // 4. 生成主因子的2水平全因子设计矩阵（元素为-1和1）
            int mainFactorCount = mainFactorIndices.Count;
            Matrix<double> mainFactors = Ff2n(mainFactorCount);

            // 5. 构建完整的部分因子设计矩阵
            int rowCount = mainFactors.RowCount;
            int colCount = terms.Count;
            Matrix<double> designMatrix = DenseMatrix.Create(rowCount, colCount, 0);

            // 填充主因子列
            for (int i = 0; i < mainFactorIndices.Count; i++)
            {
                int col = mainFactorIndices[i];
                designMatrix.SetColumn(col, mainFactors.Column(i));
            }

            // 6. 填充交互项列（通过主因子列的乘积计算）
            foreach (int col in interactionIndices)
            {
                string term = terms[col];
                int[] factorOffsets;

                // 处理小写字母（a-z）
                factorOffsets = term.Select(c => c - 'a').ToArray();
                if (factorOffsets.Any(offset => offset < 0 || offset >= mainFactorCount))
                {
                    // 处理大写字母（A-Z）
                    factorOffsets = term.Select(c => c - 'A').ToArray();
                    if (factorOffsets.Any(offset => offset < 0 || offset >= mainFactorCount))
                    {
                        throw new ArgumentException($"生成器字符串中的交互项'{term}'包含无效因子", nameof(gen));
                    }
                }

                // 计算交互项：对应主因子列的逐元素乘积
                Vector<double> interaction = DenseVector.Create(rowCount, i => 1.0);
                foreach (int offset in factorOffsets)
                {
                    interaction = interaction.PointwiseMultiply(mainFactors.Column(offset));
                }

                designMatrix.SetColumn(col, interaction);
            }

            // 7. 应用负号运算符（对包含"-"的列取反）
            foreach (int idx in negativeIndices)
            {
                designMatrix.SetColumn(idx, designMatrix.Column(idx) * -1);
            }

            return designMatrix;
        }

        /// <summary>
        /// 查找包含指定符号的项的索引（模拟Python的grep功能）
        /// </summary>
        /// <param name="haystack">待搜索的字符串数组</param>
        /// <param name="needle">要查找的符号（"+"或"-"）</param>
        /// <returns>包含该符号的项的索引列表</returns>
        private static List<int> Grep(string[] haystack, string needle)
        {
            var indices = new List<int>();
            for (int i = 0; i < haystack.Length; i++)
            {
                if (haystack[i].Contains(needle))
                {
                    indices.Add(i);
                }
            }
            return indices;
        }

        /// <summary>
        /// 创建 Box-Behnken 设计矩阵
        /// </summary>
        /// <param name="n">因素数量（必须至少为3）</param>
        /// <param name="center">中心点数量（为null时使用默认值）</param>
        /// <returns>Box-Behnken 设计矩阵</returns>
        public static Matrix<double> BBDesignCorrected(int n, int? center = null)
        {
            // 生成2因素的2水平全因子设计矩阵 (4x2)
            Matrix<double> hFact = Ff2n(2);

            // 计算因子组合部分的行数
            int nbLines = (int)(0.5 * n * (n - 1) * hFact.RowCount);

            // 初始化因子组合矩阵（全部填充0）
            Matrix<double> h = DenseMatrix.OfArray(new double[nbLines, n]);

            int index = 0;
            // 填充因子组合部分
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    index++;
                    int startRow = (index - 1) * hFact.RowCount;
                    int endRow = index * hFact.RowCount;

                    // 填充第i列
                    for (int row = startRow; row < endRow; row++)
                    {
                        h[row, i] = hFact[row - startRow, 0];
                    }

                    // 填充第j列
                    for (int row = startRow; row < endRow; row++)
                    {
                        h[row, j] = hFact[row - startRow, 1];
                    }
                }
            }

            // 确定中心点数量
            int centerPoints = center ?? GetDefaultCenterPoints(n);


            // 生成中心点矩阵
            Matrix<double> centerMatrix = RepeatCenter(n, centerPoints);

            // 合并因子组合矩阵和中心点矩阵
            return h.Stack(centerMatrix);
        }

        private static int GetDefaultCenterPoints(int n)
        {
            if (n <= 16)
            {
                int[] points = { 0, 0, 0, 3, 3, 6, 6, 6, 8, 9, 10, 12, 12, 13, 14, 15, 16 };
                return points[n];
            }
            else
            {
                return n;
            }
        }

        public static Matrix<double> PBDesign(int n)
        {
            if (n <= 0)
                throw new ArgumentException("Number of factors must be a positive integer", nameof(n));

            int keep = n;
            n = 4 * (n / 4 + 1); // 确保行数是4的倍数

            // 计算n的分解（对应Python的np.frexp）
            var frexpResults = new (double f, int e)[3];
            frexpResults[0] = Frexp(n);
            frexpResults[1] = Frexp(n / 12.0);
            frexpResults[2] = Frexp(n / 20.0);

            var validKIndices = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(frexpResults[i].f - 0.5) < 1e-10 && frexpResults[i].e > 0)
                    validKIndices.Add(i);
            }

            if (validKIndices.Count == 0)
                throw new ArgumentException("n must be a valid multiple of 4 (4, 8, 12, 16, 20, etc.)", nameof(n));

            int k = validKIndices[0];
            int exp = frexpResults[k].e - 1;

            Matrix<double> H;

            switch (k)
            {
                case 0: // N = 1*2^e
                    H = DenseMatrix.OfArray(new double[,] { { 1 } });
                    break;
                case 1: // N = 12*2^e
                    double[] toeplitzFirstRow = { -1, -1, 1, -1, -1, -1, 1, 1, 1, -1, 1 };
                    double[] toeplitzFirstCol = { -1, 1, -1, 1, 1, 1, -1, -1, -1, 1, -1 };
                    Matrix<double> toeplitzMat = Toeplitz(toeplitzFirstRow, toeplitzFirstCol);

                    // 创建全为1的列向量（11行，1列）并与Toeplitz矩阵水平拼接
                    Vector<double> onesCol11 = DenseVector.Create(11, _ => 1.0);
                    Matrix<double> onesColMatrix11 = onesCol11.ToColumnMatrix();
                    Matrix<double> lowerPart = onesColMatrix11.Append(toeplitzMat);

                    // 创建全为1的行向量（1行，12列）并与lowerPart垂直拼接
                    Vector<double> onesRow12 = DenseVector.Create(12, _ => 1.0);
                    Matrix<double> topRow = onesRow12.ToRowMatrix();
                    H = topRow.Stack(lowerPart);
                    break;
                case 2: // N = 20*2^e
                    double[] hankelFirstRow = { -1, -1, 1, 1, -1, -1, -1, -1, 1, -1, 1, -1, 1, 1, 1, 1, -1, -1, 1 };
                    double[] hankelLastRow = { 1, -1, -1, 1, 1, -1, -1, -1, -1, 1, -1, 1, -1, 1, 1, 1, 1, -1, -1 };
                    Matrix<double> hankelMat = Hankel(hankelFirstRow, hankelLastRow);

                    // 创建全为1的列向量（19行，1列）并与Hankel矩阵水平拼接
                    Vector<double> onesCol19 = DenseVector.Create(19, _ => 1.0);
                    Matrix<double> onesColMatrix19 = onesCol19.ToColumnMatrix();
                    Matrix<double> lowerPart20 = onesColMatrix19.Append(hankelMat);

                    // 创建全为1的行向量（1行，20列）并与lowerPart20垂直拼接
                    Vector<double> onesRow20 = DenseVector.Create(20, _ => 1.0);
                    Matrix<double> topRow20 = onesRow20.ToRowMatrix();
                    H = topRow20.Stack(lowerPart20);
                    break;
                default:
                    throw new InvalidOperationException("Unexpected k value");
            }

            // Kronecker乘积扩展（修正矩阵拼接逻辑）
            for (int i = 0; i < exp; i++)
            {
                Matrix<double> h1 = H.Append(H);
                Matrix<double> h2 = H.Append(-H);
                H = h1.Stack(h2);
            }

            // 裁剪到所需的因子数量并翻转行
            H = H.SubMatrix(0, H.RowCount, 1, keep);
            H = FlipUpDown(H);

            return H;
        }

        private static (double f, int e) Frexp(double x)
        {
            if (x == 0)
                return (0, 0);

            int exp = 0;
            while (Math.Abs(x) >= 1)
            {
                x /= 2;
                exp++;
            }
            while (Math.Abs(x) < 0.5 && x != 0)
            {
                x *= 2;
                exp--;
            }
            return (x, exp);
        }

        private static Matrix<double> Toeplitz(double[] firstRow, double[] firstCol)
        {
            int n = firstRow.Length;
            int m = firstCol.Length;
            Matrix<double> mat = new DenseMatrix(m, n);

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int idx = j - i;
                    if (idx >= 0 && idx < firstRow.Length)
                        mat[i, j] = firstRow[idx];
                    else if (-idx >= 0 && -idx < firstCol.Length)
                        mat[i, j] = firstCol[-idx];
                }
            }
            return mat;
        }

        private static Matrix<double> Hankel(double[] firstRow, double[] lastRow)
        {
            int n = firstRow.Length;
            int m = lastRow.Length;
            Matrix<double> mat = new DenseMatrix(m, n);

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int idx = i + j;
                    if (idx < firstRow.Length)
                        mat[i, j] = firstRow[idx];
                    else
                        mat[i, j] = lastRow[idx - firstRow.Length + 1];
                }
            }
            return mat;
        }

        private static Matrix<double> FlipUpDown(Matrix<double> mat)
        {
            int rows = mat.RowCount;
            Matrix<double> flipped = new DenseMatrix(rows, mat.ColumnCount);
            for (int i = 0; i < rows; i++)
            {
                flipped.SetRow(i, mat.Row(rows - 1 - i));
            }
            return flipped;
        }

        /// <summary>
        /// 将编码矩阵（-1,0,1）映射到实际因子水平
        /// </summary>
        private static Matrix<double> MapToFactorLevels(Matrix<double> codedMatrix, List<double[]> factorLevels)
        {
            int rowCount = codedMatrix.RowCount;
            int colCount = codedMatrix.ColumnCount;
            Matrix<double> result = DenseMatrix.OfArray(new double[rowCount, colCount]);

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    double code = codedMatrix[row, col];
                    // 根据编码（-1,0,1）映射到对应的因子水平
                    int index = code switch
                    {
                        -1 => 0,
                        0 => 1,
                        1 => 2,
                        _ => throw new InvalidOperationException("无效的编码值，必须为-1、0或1")
                    };
                    result[row, col] = factorLevels[col][index];
                }
            }
            return result;
        }

        /// <summary>
        /// 创建全因子设计矩阵（对应 Python 的 fullfact 函数）
        /// </summary>
        /// <param name="levels">每个因子的水平数量数组</param>
        /// <returns>设计矩阵，元素为 0 到 k-1（k 为对应因子的水平数）</returns>
        public static Matrix<double> FullFact(int[] levels)
        {
            int n = levels.Length; // 因子数量
            int nbLines = 1;

            // 计算总试验次数（所有水平数的乘积）
            foreach (int level in levels)
            {
                if (level < 1)
                    throw new ArgumentException("每个因子的水平数必须至少为 1", nameof(levels));
                nbLines *= level;
            }

            Matrix<double> design = DenseMatrix.OfArray(new double[nbLines, n]);
            int levelRepeat = 1;
            int rangeRepeat = nbLines;

            for (int i = 0; i < n; i++)
            {
                rangeRepeat /= levels[i];
                int[] lvl = new int[levels[i] * levelRepeat];

                // 生成基础水平序列（如 [0,0,1,1] 对应 level=2, levelRepeat=2）
                for (int j = 0; j < levels[i]; j++)
                {
                    for (int k = 0; k < levelRepeat; k++)
                    {
                        lvl[j * levelRepeat + k] = j;
                    }
                }

                // 扩展序列到总长度（重复 rangeRepeat 次）
                int[] rng = new int[nbLines];
                for (int r = 0; r < rangeRepeat; r++)
                {
                    Array.Copy(lvl, 0, rng, r * lvl.Length, lvl.Length);
                }

                // 填充当前因子列
                for (int row = 0; row < nbLines; row++)
                {
                    design[row, i] = rng[row];
                }

                levelRepeat *= levels[i];
            }

            return design;
        }

        /// <summary>
        /// 创建 2 水平全因子设计矩阵（对应 Python 的 ff2n 函数）
        /// </summary>
        /// <param name="n">因子数量</param>
        /// <returns>设计矩阵，元素为 -1 和 1</returns>
        public static Matrix<double> Ff2n(int n)
        {
            if (n < 1)
                throw new ArgumentException("因子数量必须至少为 1", nameof(n));

            // 生成 [2, 2, ..., 2]（长度为 n）的水平数组
            int[] levels = new int[n];
            for (int i = 0; i < n; i++)
            {
                levels[i] = 2;
            }

            // 调用 FullFact 后转换为 -1 和 1（原 Python 逻辑：2*fullfact([2]*n) - 1）
            Matrix<double> factMatrix = FullFact(levels);
            return factMatrix.Multiply(2.0).Subtract(1.0);
        }

        /// <summary>
        /// 创建中心点
        /// </summary>
        /// <param name="n">因子数量</param>
        /// <param name="repeat">重复次数</param>
        /// <returns>中心点矩阵</returns>
        public static Matrix<double> RepeatCenter(int n, int repeat)
        {
            return Matrix<double>.Build.Dense(repeat, n);
        }

        /// <summary>
        /// 将设计矩阵（值范围 [-1,1] 或其他编码范围）转换为对应因子实际取值的矩阵
        /// 适用于 Box-Behnken、中心复合设计等需要映射多水平因子的实验设计
        /// </summary>
        /// <param name="x">设计矩阵，元素值通常为编码值（如 -1, 0, 1 等），每行代表一个样本，每列对应一个因子</param>
        /// <param name="factorLists">因子水平列表，每个子数组包含该因子的所有可能水平值（至少 3 个，如 [min, mid, max]）</param>
        /// <returns>转换后的矩阵，元素值为因子的实际水平值</returns>
        public static Matrix<double> ConstructDfFromMatrix(Matrix<double> x, List<double[]> factorLists)
        {
            // 验证输入矩阵列数与因子数量是否匹配
            if (x.ColumnCount != factorLists.Count)
            {
                throw new ArgumentException(
                    "矩阵列数与因子数量不匹配",
                    nameof(factorLists)
                );
            }

            // 验证每个因子至少包含 3 个水平值（适用于 Box-Behnken 等设计）
            foreach (var factorLevels in factorLists)
            {
                if (factorLevels.Length < 3)
                {
                    throw new ArgumentException(
                        "每个因子必须包含至少 3 个水平值（如最小值、中间值、最大值）",
                        nameof(factorLists)
                    );
                }
            }

            // 获取矩阵的行数（样本数量）和列数（因子数量）
            int rowCount = x.RowCount;
            int colCount = x.ColumnCount;

            // 初始化结果矩阵，维度与输入矩阵相同
            Matrix<double> result = DenseMatrix.Create(rowCount, colCount, 0.0);

            // 遍历每个样本（行）
            for (int row = 0; row < rowCount; row++)
            {
                // 遍历每个因子（列）
                for (int col = 0; col < colCount; col++)
                {
                    // 获取当前位置的编码值（如 Box-Behnken 设计中可能为 -1, 0, 1）
                    double codedValue = x[row, col];

                    // 获取当前因子的所有水平值并排序（确保顺序为 [min, mid, max, ...]）
                    double[] levels = factorLists[col];
                    Array.Sort(levels);

                    // 根据编码值映射到实际水平值
                    // 对于常见的三水平设计（如 -1, 0, 1），分别对应最小、中间、最大值
                    double actualValue;
                    if (Math.Abs(codedValue + 1.0) < 1e-9) // 编码值为 -1（允许微小浮点误差）
                    {
                        actualValue = levels[0]; // 最小值
                    }
                    else if (Math.Abs(codedValue) < 1e-9) // 编码值为 0
                    {
                        actualValue = levels[1]; // 中间值
                    }
                    else if (Math.Abs(codedValue - 1.0) < 1e-9) // 编码值为 1
                    {
                        actualValue = levels[2]; // 最大值
                    }
                    else
                    {
                        // 处理其他可能的编码值（如中心复合设计中的扩展点）
                        // 此处采用线性插值以适应更多水平场景
                        double minLevel = levels[0];
                        double maxLevel = levels[levels.Length - 1];
                        double range = maxLevel - minLevel;
                        // 将编码值从 [-1,1] 映射到 [minLevel, maxLevel]
                        actualValue = minLevel + (codedValue + 1.0) / 2.0 * range;
                    }

                    result[row, col] = actualValue;
                }
            }

            return result;
        }

        /// <summary>
        /// 将随机矩阵（值范围 [0,1)）转换为对应因子范围的矩阵
        /// 该方法用于将设计矩阵从单位超立方空间映射到实际因子的取值范围
        /// </summary>
        /// <param name="x">随机矩阵，元素值在 [0,1) 范围内，每行代表一个样本，每列对应一个因子</param>
        /// <param name="factorArray">因子范围列表，每个元素是包含两个值的数组 [最小值, 最大值]</param>
        /// <returns>转换后的矩阵，元素值落在对应因子的取值范围内</returns>
        public static Matrix<double> ConstructDfFromRandomMatrix(Matrix<double> x, List<double[]> factorArray)
        {
            // 获取矩阵的行数（样本数量）和列数（因子数量）
            int rowCount = x.RowCount;
            int colCount = x.ColumnCount;

            // 初始化结果矩阵，维度与输入矩阵相同
            Matrix<double> result = DenseMatrix.Create(rowCount, colCount, 0.0);

            // 遍历每个样本（行）
            for (int row = 0; row < rowCount; row++)
            {
                // 遍历每个因子（列）
                for (int col = 0; col < colCount; col++)
                {
                    // 获取当前位置的随机值（[0,1) 范围内）
                    double randomValue = x[row, col];

                    // 获取当前因子的最小值和最大值
                    double min = factorArray[col][0];
                    double max = factorArray[col][1];

                    // 计算因子取值范围的长度
                    double range = max - min;

                    // 将随机值从 [0,1) 映射到 [min, max) 范围
                    // 映射公式：实际值 = 最小值 + 随机值 × 范围长度
                    result[row, col] = min + randomValue * range;
                }
            }

            return result;
        }
        #endregion
    }
}