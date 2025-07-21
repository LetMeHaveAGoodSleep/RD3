using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace RD3.Shared
{
    /// <summary>
    /// 实验设计工具类，提供各种实验设计方法
    /// </summary>
    public static class DesignOfExperiments
    {
        #region 公共方法

        /// <summary>
        /// 创建Box-Behnken设计
        /// </summary>
        /// <param name="n">因子数量(至少3个)</param>
        /// <param name="center">中心点数量(默认为1)</param>
        /// <returns>设计矩阵</returns>
        public static Matrix<double> Bbdesign(int n, int? center = null)
        {
            if (n < 3)
                throw new ArgumentException("因子数量必须至少为3");

            // 首先计算2个参数的因子设计
            var H_fact = Ff2n(2);

            // 初始化设计矩阵
            int nbLines = (n * (n - 1) / 2) * H_fact.RowCount;
            var H = RepeatCenter(n, nbLines);

            // 为每对维度创建因子设计
            int index = 0;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    index++;
                    int start = Math.Max(0, (index - 1) * H_fact.RowCount);
                    int end = index * H_fact.RowCount;

                    // 设置因子值
                    for (int k = 0; k < H_fact.RowCount; k++)
                    {
                        H[start + k, i] = H_fact[k, 0];
                        H[start + k, j] = H_fact[k, 1];
                    }
                }
            }

            // 设置中心点数量
            int centerPoints = center ?? (n <= 16 ? new[] { 0, 0, 0, 3, 3, 6, 6, 6, 8, 9, 10, 12, 12, 13, 14, 15, 16 }[n] : n);

            // 添加中心点
            var H_center = RepeatCenter(n, centerPoints);
            return Matrix<double>.Build.DenseOfMatrix(H).Stack(H_center);
        }

        /// <summary>
        /// 创建中心复合设计
        /// </summary>
        /// <param name="n">因子数量</param>
        /// <param name="center">中心点数量数组(默认为[4,4])</param>
        /// <param name="alpha">alpha类型('orthogonal'或'rotatable')</param>
        /// <param name="face">设计类型('circumscribed','inscribed'或'faced')</param>
        /// <returns>设计矩阵</returns>
        public static Matrix<double> Ccdesign(int n, int[] center = null, string alpha = "orthogonal", string face = "circumscribed")
        {
            if (n <= 1)
                throw new ArgumentException("因子数量必须大于1");

            if (center == null)
                center = new[] { 4, 4 };
            if (center.Length != 2)
                throw new ArgumentException("中心点参数必须是长度为2的数组");

            Matrix<double> H1, H2;
            double a = 1;

            // 正交设计
            if (alpha.ToLower() == "orthogonal" || alpha.ToLower() == "o")
            {
                var result = Star(n, "orthogonal", center);
                H2 = result.Item1;
                a = result.Item2;
            }
            // 可旋转设计
            else if (alpha.ToLower() == "rotatable" || alpha.ToLower() == "r")
            {
                var result = Star(n, "rotatable");
                H2 = result.Item1;
                a = result.Item2;
            }
            else
            {
                throw new ArgumentException("无效的alpha值");
            }

            // 内切CCD
            if (face.ToLower() == "inscribed" || face.ToLower() == "cci")
            {
                H1 = Ff2n(n) / a; // 缩放因子点
                var result = Star(n);
                H2 = result.Item1;
                a = result.Item2;
            }
            // 面心CCD
            else if (face.ToLower() == "faced" || face.ToLower() == "ccf")
            {
                var result = Star(n); // alpha总是1
                H2 = result.Item1;
                a = result.Item2;
                H1 = Ff2n(n);
            }
            // 外切CCD
            else if (face.ToLower() == "circumscribed" || face.ToLower() == "ccc")
            {
                H1 = Ff2n(n);
            }
            else
            {
                throw new ArgumentException("无效的face值");
            }

            // 添加中心点
            var C1 = RepeatCenter(n, center[0]);
            var C2 = RepeatCenter(n, center[1]);

            // 合并所有部分
            H1 = Union(H1, C1);
            H2 = Union(H2, C2);
            return Union(H1, H2);
        }

        /// <summary>
        /// 创建拉丁超立方设计
        /// </summary>
        /// <param name="n">因子数量</param>
        /// <param name="samples">每个因子的样本数(默认为n)</param>
        /// <param name="criterion">设计准则('center','maximin','centermaximin'或'correlation')</param>
        /// <param name="iterations">迭代次数(默认为5)</param>
        /// <returns>设计矩阵</returns>
        public static Matrix<double> Lhs(int n, int? samples = null, string criterion = null, int? iterations = null)
        {
            int actualSamples = samples ?? n;
            int actualIterations = iterations ?? 5;

            if (criterion == null)
                return LhsClassic(n, actualSamples);

            switch (criterion.ToLower())
            {
                case "center":
                case "c":
                    return LhsCentered(n, actualSamples);
                case "maximin":
                case "m":
                    return LhsMaximin(n, actualSamples, actualIterations, "maximin");
                case "centermaximin":
                case "cm":
                    return LhsMaximin(n, actualSamples, actualIterations, "centermaximin");
                case "correlation":
                case "corr":
                    return LhsCorrelate(n, actualSamples, actualIterations);
                default:
                    throw new ArgumentException("无效的criterion值");
            }
        }

        /// <summary>
        /// 创建Plackett-Burman设计
        /// </summary>
        /// <param name="n">因子数量</param>
        /// <returns>正交设计矩阵</returns>
        public static Matrix<double> Pbdesign(int n)
        {
            if (n <= 0)
                throw new ArgumentException("因子数量必须是正整数");

            int keep = n;
            n = 4 * (n / 4 + 1); // 计算正确的行数(4的倍数)

            // 检查n是否有效
            if (n != Math.Pow(2, Math.Log(n, 2)) && n != 12 * Math.Pow(2, Math.Log(n / 12, 2)) && n != 20 * Math.Pow(2, Math.Log(n / 20, 2)))
                throw new ArgumentException("无效的输入，n必须是4、12或20的倍数");

            Matrix<double> H;
            int e;

            // 根据n的大小选择基础矩阵
            if (n == Math.Pow(2, Math.Log(n, 2)))
            {
                e = (int)Math.Log(n, 2) - 1;
                H = Matrix<double>.Build.Dense(1, 1, 1);
            }
            else if (n == 12 * Math.Pow(2, Math.Log(n / 12, 2)))
            {
                e = (int)Math.Log(n / 12, 2);
                // 创建12x12的基础矩阵
                var ones = Matrix<double>.Build.Dense(1, 12, 1);
                var toeplitzMat = Toeplitz(new[] { -1.0, -1, 1, -1, -1, -1, 1, 1, 1, -1, 1 },
                                          new[] { -1.0, 1, -1, 1, 1, 1, -1, -1, -1, 1, -1 });
                var hStack = Matrix<double>.Build.Dense(11, 1, 1).Append(toeplitzMat);
                H = ones.Stack(hStack);
            }
            else // n == 20 * Math.Pow(2, Math.Log(n / 20, 2))
            {
                e = (int)Math.Log(n / 20, 2);
                // 创建20x20的基础矩阵
                var ones = Matrix<double>.Build.Dense(1, 20, 1);
                var hankelMat = Hankel(
                    new[] { -1.0, -1, 1, 1, -1, -1, -1, -1, 1, -1, 1, -1, 1, 1, 1, 1, -1, -1, 1 },
                    new[] { 1.0, -1, -1, 1, 1, -1, -1, -1, -1, 1, -1, 1, -1, 1, 1, 1, 1, -1, -1 });
                var hStack = Matrix<double>.Build.Dense(19, 1, 1).Append(hankelMat);
                H = ones.Stack(hStack);
            }

            // Kronecker积构造
            for (int i = 0; i < e; i++)
            {
                var h1 = H.KroneckerProduct(Matrix<double>.Build.Dense(1, 1, 1));
                var h2 = H.KroneckerProduct(Matrix<double>.Build.Dense(1, 1, -1));
                H = h1.Stack(h2);
            }

            // 裁剪矩阵到所需大小
            return H.SubMatrix(0, H.RowCount, 1, keep);
        }

        /// <summary>
        /// 创建全因子设计
        /// </summary>
        /// <param name="levels">每个因子的水平数数组</param>
        /// <returns>设计矩阵</returns>
        public static Matrix<double> Fullfact(int[] levels)
        {
            int numFactors = levels.Length;
            int numLines = levels.Aggregate(1, (a, b) => a * b);
            var H = Matrix<double>.Build.Dense(numLines, numFactors);

            int levelRepeat = 1;
            int rangeRepeat = numLines;

            for (int i = 0; i < numFactors; i++)
            {
                rangeRepeat /= levels[i];
                var lvl = new List<double>();

                for (int j = 0; j < levels[i]; j++)
                {
                    lvl.AddRange(Enumerable.Repeat((double)j, levelRepeat));
                }

                var rng = new List<double>();
                for (int k = 0; k < rangeRepeat; k++)
                {
                    rng.AddRange(lvl);
                }

                for (int m = 0; m < numLines; m++)
                {
                    H[m, i] = rng[m];
                }

                levelRepeat *= levels[i];
            }

            return H;
        }

        /// <summary>
        /// 创建2水平全因子设计
        /// </summary>
        /// <param name="n">因子数量</param>
        /// <returns>设计矩阵(-1和1编码)</returns>
        public static Matrix<double> Ff2n(int n)
        {
            var levels = Enumerable.Repeat(2, n).ToArray();
            return 2 * Fullfact(levels) - 1;
        }

        /// <summary>
        /// 创建2水平部分因子设计
        /// </summary>
        /// <param name="gen">生成器字符串(如"a b ab")</param>
        /// <returns>设计矩阵</returns>
        public static Matrix<double> Fracfact(string gen)
        {
            // 识别字母和组合
            var tokens = gen.Split(new[] { ' ', '+', '-' }, StringSplitOptions.RemoveEmptyEntries);
            var tokenLengths = tokens.Select(t => t.Length).ToArray();

            // 单字母(主因子)的索引
            var mainIndices = tokenLengths.Select((len, idx) => len == 1 ? idx : -1).Where(i => i != -1).ToArray();

            // 字母组合的索引
            var interactionIndices = tokenLengths.Select((len, idx) => len != 1 ? idx : -1).Where(i => i != -1).ToArray();

            // 检查生成器字符串中的"-"或"+"运算符
            var elements = gen.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var minusIndices = elements.Select((elem, idx) => elem.StartsWith("-") ? idx : -1).Where(i => i != -1).ToArray();

            // 用2水平全因子设计填充
            var H1 = Ff2n(mainIndices.Length);
            var H = Matrix<double>.Build.Dense(H1.RowCount, tokens.Length);

            // 填充主因子
            for (int i = 0; i < mainIndices.Length; i++)
            {
                H.SetColumn(mainIndices[i], H1.Column(i));
            }

            // 识别组合并填充交互项
            foreach (var k in interactionIndices)
            {
                var factors = tokens[k].ToLower().ToCharArray();
                var indices = factors.Select(c => (int)(c - 'a')).ToArray();

                // 计算交互项
                var interaction = Matrix<double>.Build.Dense(H1.RowCount, 1, 1);
                foreach (var idx in indices)
                {
                    interaction = interaction.PointwiseMultiply(H1.Column(idx).ToColumnMatrix());
                }

                H.SetColumn(k, interaction.Column(0));
            }

            // 处理"-"运算符
            foreach (var idx in minusIndices)
            {
                H.SetColumn(idx, -H.Column(idx));
            }

            return H;
        }

        /// <summary>
        /// 折叠设计以减少混杂效应
        /// </summary>
        /// <param name="H">设计矩阵</param>
        /// <param name="columns">要折叠的列索引(默认为全部)</param>
        /// <returns>折叠后的设计矩阵</returns>
        public static Matrix<double> Fold(Matrix<double> H, int[] columns = null)
        {
            if (H.ColumnCount == 0 || H.RowCount == 0)
                throw new ArgumentException("输入设计矩阵必须为2维");

            columns = columns ?? Enumerable.Range(0, H.ColumnCount).ToArray();
            var Hf = H.Clone();

            foreach (var col in columns)
            {
                var uniqueValues = H.Column(col).ToArray().Distinct().ToArray();
                if (uniqueValues.Length != 2)
                    throw new ArgumentException("输入设计矩阵必须仅为2水平因子");

                for (int i = 0; i < H.RowCount; i++)
                {
                    Hf[i, col] = (Math.Abs(H[i, col] - uniqueValues[0]) < 1e-10) ? uniqueValues[1] : uniqueValues[0];
                }
            }

            return H.Stack(Hf);
        }

        #endregion

        #region 辅助方法

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
        /// 创建星点
        /// </summary>
        /// <param name="n">因子数量</param>
        /// <param name="alpha">alpha类型('faced','orthogonal'或'rotatable')</param>
        /// <param name="center">中心点数量数组</param>
        /// <returns>元组(星点矩阵, alpha值)</returns>
        public static Tuple<Matrix<double>, double> Star(int n, string alpha = "faced", int[] center = null)
        {
            center = center ?? new[] { 1, 1 };
            double a = 1;

            // 面心设计的星点
            if (alpha.ToLower() == "faced")
            {
                a = 1;
            }
            // 正交设计
            else if (alpha.ToLower() == "orthogonal")
            {
                int nc = (int)Math.Pow(2, n); // 因子点数量
                int nco = center[0]; // 因子设计的中心点
                int na = 2 * n; // 轴向点数量
                int nao = center[1]; // 轴向设计的中心点

                // 正交设计中的alpha值
                a = Math.Sqrt(n * (1 + nao / (double)na) / (1 + nco / (double)nc));
            }
            // 可旋转设计
            else if (alpha.ToLower() == "rotatable")
            {
                int nc = (int)Math.Pow(2, n); // 因子点数量
                a = Math.Pow(nc, 0.25); // 可旋转设计中的alpha值
            }
            else
            {
                throw new ArgumentException("无效的alpha值");
            }

            // 创建星点矩阵
            var H = Matrix<double>.Build.Dense(2 * n, n);
            for (int i = 0; i < n; i++)
            {
                H[2 * i, i] = -1;
                H[2 * i + 1, i] = 1;
            }

            H *= a;
            return Tuple.Create(H, a);
        }

        /// <summary>
        /// 合并两个矩阵(垂直堆叠)
        /// </summary>
        /// <param name="H1">上部矩阵</param>
        /// <param name="H2">下部矩阵</param>
        /// <returns>合并后的矩阵</returns>
        public static Matrix<double> Union(Matrix<double> H1, Matrix<double> H2)
        {
            return H1.Stack(H2);
        }

        /// <summary>
        /// 计算回归误差的方差
        /// </summary>
        /// <param name="H">回归矩阵</param>
        /// <param name="x">计算位置的坐标</param>
        /// <param name="model">回归模型字符串</param>
        /// <param name="sigma">方差估计(默认为1)</param>
        /// <returns>回归误差的方差</returns>
        public static double VarRegressionMatrix(Matrix<double> H, Matrix<double> x, string model, double sigma = 1)
        {
            if (x.RowCount == 1)
                x = x.Transpose();

            var xMod = BuildRegressionMatrix(x, model);
            var HtH = H.Transpose().Multiply(H);

            if (HtH.Rank() < HtH.ColumnCount)
                throw new ArgumentException("模型和DOE不匹配");

            var invHtH = HtH.Inverse();
            var result = sigma * sigma * xMod.Transpose().Multiply(invHtH).Multiply(xMod);
            return result[0, 0];
        }

        /// <summary>
        /// 构建回归矩阵
        /// </summary>
        /// <param name="H">DOE矩阵</param>
        /// <param name="model">模型字符串</param>
        /// <param name="build">布尔数组</param>
        /// <returns>回归矩阵</returns>
        private static Matrix<double> BuildRegressionMatrix(Matrix<double> H, string model, bool[] build = null)
        {
            var tokens = model.Split(' ');
            int sizeIndex = H.ColumnCount == 1 ? H.RowCount.ToString().Length : H.ColumnCount.ToString().Length;

            build = build ?? Enumerable.Repeat(true, tokens.Length).ToArray();

            // 测试向量方向是否正确
            if (H.RowCount == 1)
                H = H.Transpose();

            // 收集单项式索引
            var monomIndices = new List<int>();
            for (int i = 0; i < tokens.Length; i++)
            {
                if (build[i])
                {
                    var indices = Grep(tokens[i], "x" + new string('0', sizeIndex - i.ToString().Length) + i);
                    monomIndices.AddRange(indices);
                }
            }

            monomIndices = monomIndices.Distinct().OrderByDescending(x => x).ToList();

            // 替换变量名
            int numVars = H.ColumnCount == 1 ? H.RowCount : H.ColumnCount;
            bool vectorMode = H.ColumnCount == 1;

            for (int i = 0; i < numVars; i++)
            {
                string pattern = "x" + new string('0', sizeIndex - i.ToString().Length) + i;
                string replacement = vectorMode ? $"H[{i}]" : $"H[row, {i}]";

                for (int j = 0; j < tokens.Length; j++)
                {
                    tokens[j] = tokens[j].Replace(pattern, replacement);
                }
            }

            // 构建回归矩阵
            if (vectorMode)
            {
                var R = Matrix<double>.Build.Dense(tokens.Length, 1);
                for (int j = 0; j < tokens.Length; j++)
                {
                    // 注意：这里简化处理，实际应用中需要更复杂的表达式解析
                    R[j, 0] = EvaluateExpression(tokens[j], H);
                }
                return R;
            }
            else
            {
                var R = Matrix<double>.Build.Dense(H.RowCount, tokens.Length);
                for (int i = 0; i < H.RowCount; i++)
                {
                    for (int j = 0; j < tokens.Length; j++)
                    {
                        // 注意：这里简化处理，实际应用中需要更复杂的表达式解析
                        R[i, j] = EvaluateExpression(tokens[j], H, i);
                    }
                }
                return R;
            }
        }

        /// <summary>
        /// 经典拉丁超立方设计
        /// </summary>
        private static Matrix<double> LhsClassic(int n, int samples)
        {
            // 生成区间
            var cut = LinSpace(0, 1, samples + 1);

            // 在每个区间内均匀填充点
            var rand = new Random();
            var u = Matrix<double>.Build.Dense(samples, n);
            for (int i = 0; i < samples; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    u[i, j] = rand.NextDouble();
                }
            }

            var a = cut.Take(samples).ToArray();
            var b = cut.Skip(1).Take(samples).ToArray();
            var rdpoints = Matrix<double>.Build.Dense(samples, n);

            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < samples; i++)
                {
                    rdpoints[i, j] = u[i, j] * (b[i] - a[i]) + a[i];
                }
            }

            // 创建随机配对
            var H = Matrix<double>.Build.Dense(samples, n);
            for (int j = 0; j < n; j++)
            {
                var order = Enumerable.Range(0, samples).OrderBy(x => rand.Next()).ToArray();
                for (int i = 0; i < samples; i++)
                {
                    H[i, j] = rdpoints[order[i], j];
                }
            }

            return H;
        }

        /// <summary>
        /// 中心拉丁超立方设计
        /// </summary>
        private static Matrix<double> LhsCentered(int n, int samples)
        {
            // 生成区间
            var cut = LinSpace(0, 1, samples + 1);

            // 在每个区间中心填充点
            var a = cut.Take(samples).ToArray();
            var b = cut.Skip(1).Take(samples).ToArray();
            var centers = a.Zip(b, (x, y) => (x + y) / 2).ToArray();

            // 创建随机配对
            var rand = new Random();
            var H = Matrix<double>.Build.Dense(samples, n);
            for (int j = 0; j < n; j++)
            {
                var shuffled = centers.OrderBy(x => rand.Next()).ToArray();
                for (int i = 0; i < samples; i++)
                {
                    H[i, j] = shuffled[i];
                }
            }

            return H;
        }

        /// <summary>
        /// 最大化最小距离的拉丁超立方设计
        /// </summary>
        private static Matrix<double> LhsMaximin(int n, int samples, int iterations, string lhsType)
        {
            double maxDist = 0;
            Matrix<double> bestH = null;
            var rand = new Random();

            for (int i = 0; i < iterations; i++)
            {
                Matrix<double> candidate;
                if (lhsType == "maximin")
                    candidate = LhsClassic(n, samples);
                else
                    candidate = LhsCentered(n, samples);

                var d = Pdist(candidate);
                double minD = d.Min();

                if (minD > maxDist)
                {
                    maxDist = minD;
                    bestH = candidate.Clone();
                }
            }

            return bestH;
        }

        /// <summary>
        /// 最小化相关性的拉丁超立方设计
        /// </summary>
        private static Matrix<double> LhsCorrelate(int n, int samples, int iterations)
        {
            double minCorr = double.MaxValue;
            Matrix<double> bestH = null;
            var rand = new Random();

            for (int i = 0; i < iterations; i++)
            {
                var candidate = LhsClassic(n, samples);
                var corr = CorrelationMatrix(candidate);
                double maxAbsCorr = 0;

                // 计算非对角线元素的最大绝对值
                for (int row = 0; row < corr.RowCount; row++)
                {
                    for (int col = 0; col < corr.ColumnCount; col++)
                    {
                        if (row != col)
                        {
                            maxAbsCorr = Math.Max(maxAbsCorr, Math.Abs(corr[row, col]));
                        }
                    }
                }

                if (maxAbsCorr < minCorr)
                {
                    minCorr = maxAbsCorr;
                    bestH = candidate.Clone();
                }
            }

            return bestH;
        }

        /// <summary>
        /// 计算点间距离
        /// </summary>
        private static double[] Pdist(Matrix<double> x)
        {
            var distances = new List<double>();
            int m = x.RowCount;

            for (int i = 0; i < m - 1; i++)
            {
                for (int j = i + 1; j < m; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < x.ColumnCount; k++)
                    {
                        sum += Math.Pow(x[j, k] - x[i, k], 2);
                    }
                    distances.Add(Math.Sqrt(sum));
                }
            }

            return distances.ToArray();
        }

        /// <summary>
        /// 创建Toeplitz矩阵
        /// </summary>
        private static Matrix<double> Toeplitz(double[] firstCol, double[] firstRow)
        {
            int n = firstCol.Length;
            int m = firstRow.Length;
            var mat = Matrix<double>.Build.Dense(n, m);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (i >= j)
                        mat[i, j] = firstCol[i - j];
                    else
                        mat[i, j] = firstRow[j - i];
                }
            }

            return mat;
        }

        /// <summary>
        /// 创建Hankel矩阵
        /// </summary>
        private static Matrix<double> Hankel(double[] firstCol, double[] lastRow)
        {
            int n = firstCol.Length;
            int m = lastRow.Length;
            var mat = Matrix<double>.Build.Dense(n, m);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    if (i + j < n)
                        mat[i, j] = firstCol[i + j];
                    else
                        mat[i, j] = lastRow[i + j - n + 1];
                }
            }

            return mat;
        }

        /// <summary>
        /// 在字符串中查找所有匹配位置
        /// </summary>
        private static IEnumerable<int> Grep(string haystack, string needle)
        {
            int start = 0;
            while (true)
            {
                int pos = haystack.IndexOf(needle, start);
                if (pos == -1) yield break;
                yield return pos;
                start = pos + needle.Length;
            }
        }

        /// <summary>
        /// 线性空间生成
        /// </summary>
        private static double[] LinSpace(double start, double stop, int num)
        {
            var result = new double[num];
            double step = (stop - start) / (num - 1);

            for (int i = 0; i < num; i++)
            {
                result[i] = start + i * step;
            }

            return result;
        }

        /// <summary>
        /// 计算相关系数矩阵
        /// </summary>
        private static Matrix<double> CorrelationMatrix(Matrix<double> x)
        {
            int n = x.ColumnCount;
            var corr = Matrix<double>.Build.Dense(n, n);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        corr[i, j] = 1.0;
                    }
                    else
                    {
                        corr[i, j] = PearsonCorrelation(x.Column(i), x.Column(j));
                    }
                }
            }

            return corr;
        }

        /// <summary>
        /// 计算皮尔逊相关系数
        /// </summary>
        private static double PearsonCorrelation(Vector<double> x, Vector<double> y)
        {
            double sumX = x.Sum();
            double sumY = y.Sum();
            double sumXY = x.DotProduct(y);
            double sumX2 = x.DotProduct(x);
            double sumY2 = y.DotProduct(y);
            int n = x.Count;

            double numerator = sumXY - (sumX * sumY / n);
            double denominator = Math.Sqrt((sumX2 - sumX * sumX / n) * (sumY2 - sumY * sumY / n));

            return denominator == 0 ? 0 : numerator / denominator;
        }

        /// <summary>
        /// 简单表达式求值(简化版)
        /// </summary>
        private static double EvaluateExpression(string expr, Matrix<double> H, int row = -1)
        {
            // 注意：这是一个简化版的表达式求值，仅用于演示
            // 实际应用中应该使用更完整的表达式解析器

            if (expr.Contains("H["))
            {
                if (row == -1) // 向量模式
                {
                    // 提取索引
                    int index = int.Parse(expr.Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                    return H[index, 0];
                }
                else // 矩阵模式
                {
                    // 提取行列
                    var parts = expr.Split(new[] { '[', ',', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    int col = int.Parse(parts[2]);
                    return H[row, col];
                }
            }

            // 简单乘法处理
            if (expr.Contains("*"))
            {
                var parts = expr.Split('*');
                double result = 1;
                foreach (var part in parts)
                {
                    result *= EvaluateExpression(part.Trim(), H, row);
                }
                return result;
            }

            // 默认返回0
            return 0;
        }

        #endregion
    }
}