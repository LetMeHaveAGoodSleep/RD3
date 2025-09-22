using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public static class QuadraticSurfaceLM
    {
        // 生成所有 a=(a1..ak)，满足 ai>=0 且 Σai ≤ degree（包含常数项）
        private static List<int[]> GenerateExponents(int k, int degree)
        {
            var result = new List<int[]>();
            int[] cur = new int[k];

            void Dfs(int idx, int remain)
            {
                if (idx == k - 1)
                {
                    cur[idx] = remain;                // 把剩余都放在最后一个上，得到 Σai == degree
                    result.Add((int[])cur.Clone());
                    // 同时还要包含 Σai < degree 的项：通过减小最后一位把剩余“落空”
                    for (int s = remain - 1; s >= 0; s--)
                    {
                        cur[idx] = s;
                        result.Add((int[])cur.Clone());
                    }
                    return;
                }
                for (int a = 0; a <= remain; a++)
                {
                    cur[idx] = a;
                    Dfs(idx + 1, remain - a);
                }
            }

            // 生成 Σai == d (d=0..degree) 的所有组合，合并即可得到 Σai ≤ degree
            for (int d = 0; d <= degree; d++)
            {
                Dfs(0, d);
            }
            // 去重（上面的构造已含重复保护，这里稳妥再唯一化）
            var uniq = new List<int[]>();
            var seen = new HashSet<string>();
            foreach (var a in result)
            {
                var key = string.Join(",", a);
                if (seen.Add(key)) uniq.Add(a);
            }
            // 约定列顺序：按总阶升序，再按字典序
            uniq.Sort((u, v) =>
            {
                int su = 0, sv = 0; foreach (var x in u) su += x; foreach (var x in v) sv += x;
                int cmp = su.CompareTo(sv);
                if (cmp != 0) return cmp;
                for (int i = 0; i < u.Length; i++)
                {
                    cmp = u[i].CompareTo(v[i]);
                    if (cmp != 0) return cmp;
                }
                return 0;
            });
            return uniq;
        }

        // 计算 ϕ_a(x) = ∏ x_i^{a_i}
        private static double Basis(Vector<double> x, int[] a)
        {
            double v = 1.0;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == 0) continue;
                v *= Math.Pow(x[i], a[i]);
            }
            return v;
        }

        // 生成多项式项名，如 1, x1, x2^2, x1x2, x1^2x3 等
        private static string BuildTermName(int[] a)
        {
            var parts = new List<string>();
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == 0) continue;
                if (a[i] == 1) parts.Add($"x{i + 1}");
                else parts.Add($"x{i + 1}^{a[i]}");
            }
            return parts.Count == 0 ? "1" : string.Join("", parts);
        }

        // SSE 辅助：使用多指数集合
        private static double SSE(Matrix<double> X, Vector<double> y, Vector<double> b, List<int[]> exps)
        {
            double totalSSE = 0.0;
            object lockObject = new object(); // 用于同步对 totalSSE 的访问

            Parallel.For(0, X.RowCount, r =>
            {
                var xr = X.Row(r);
                double z = 0.0;
                for (int j = 0; j < exps.Count; j++)
                {
                    z += b[j] * Basis(xr, exps[j]);
                }
                double e = y[r] - z;
                double localSSE = e * e;

                lock (lockObject) // 安全地累加到总和
                {
                    totalSSE += localSSE;
                }
            });

            return totalSSE;
        }

        // 通用版（支持 k>=1）
        public static (Vector<double> Parameters, string Formula, double mse, List<string> Terms) FitQuadraticSurfaceLM(
            Matrix<double> factorMatrix,
            Vector<double> responseVector,
            int degree = 2,
            double initialLambda = 0.01,
            int maxIterations = 100,
            double tolerance = 1e-8)
        {
            if (degree < 1) throw new ArgumentException("degree 必须 ≥ 1");
            int n = factorMatrix.RowCount;
            int k = factorMatrix.ColumnCount;
            if (n < 2 || k < 1) throw new ArgumentException("样本数或因子数不合法。");

            // 1) 多指数集合与术语
            var exps = GenerateExponents(k, degree);          // 列个数 p
            int p = exps.Count;
            var terms = new List<string>(p);
            foreach (var a in exps) terms.Add(BuildTermName(a));

            // 2) LM+LU
            var beta = Vector<double>.Build.Dense(p, 0.0);
            double lambda = initialLambda;
            const double lambdaFactor = 10.0;

            for (int iter = 0; iter < maxIterations; iter++)
            {
                // J(n×p)、r(n)
                var J = Matrix<double>.Build.Dense(n, p);
                var r = Vector<double>.Build.Dense(n);

                for (int i = 0; i < n; i++)
                {
                    var row = factorMatrix.Row(i);
                    // ϕ(x)
                    for (int j = 0; j < p; j++) J[i, j] = Basis(row, exps[j]);
                    // 预测与残差
                    double zhat = 0.0;
                    for (int j = 0; j < p; j++) zhat += beta[j] * J[i, j];
                    r[i] = responseVector[i] - zhat;
                }

                var JT = J.Transpose();
                var H = JT * J;
                var g = JT * r;

                if (g.L2Norm() < tolerance) break;
                for (int d = 0; d < p; d++) H[d, d] += lambda;

                Vector<double> delta;
                try { delta = H.LU().Solve(g); }
                catch
                {
                    lambda *= lambdaFactor;
                    if (lambda > 1e12) break;
                    continue;
                }

                if (delta.L2Norm() < tolerance) { beta += delta; break; }

                var betaNew = beta + delta;
                double errOld = r.DotProduct(r);
                double errNew = SSE(factorMatrix, responseVector, betaNew, exps);

                if (errNew < errOld) { beta = betaNew; lambda /= lambdaFactor; }
                else
                {
                    lambda *= lambdaFactor;
                    if (lambda > 1e12) break;
                }
            }

            // 3) 公式与 MSE
            string formula = "y = " + string.Join(" + ",
                beta.Select((v, j) => (terms[j] == "1") ? v.ToString("0.##E+0") : $"{v.ToString("0.##E+0")}*{terms[j]}")
                    ).Replace("+ -", "- ");
            double sse = SSE(factorMatrix, responseVector, beta, exps);
            double mse = sse / Math.Max(1, n);

            return (beta, formula, mse, terms);
        }

        /// <summary>
        /// 用 LM + LU 拟合二维多项式表面 z = Σ β_ij x^i y^j，i+j ≤ degree（含交互）
        /// degree=1 为平面；degree=2 为二次；更高阶自动扩展。
        /// </summary>
        //public static (Vector<double> Parameters, string Formula, double mse, List<string> Terms) FitQuadraticSurfaceLM(
        //    Matrix<double> factorMatrix,
        //    Vector<double> responseVector,
        //    int degree = 2,
        //    double initialLambda = 0.01,
        //    int maxIterations = 100,
        //    double tolerance = 1e-8)
        //{

        //    int n = factorMatrix.RowCount;

        //    // 1) 生成所有 (i,j) 指数对：i>=0, j>=0, i+j<=degree
        //    var expo = new List<(int i, int j)>();
        //    for (int i = 0; i <= degree; i++)
        //        for (int j = 0; j <= degree - i; j++)
        //            expo.Add((i, j)); // 按 i 再 j 的顺序

        //    int p = expo.Count;

        //    // 术语与初值
        //    var terms = expo.Select(e => BuildTermName(e.i, e.j)).ToList();
        //    var beta = Vector<double>.Build.Dense(p, 0.0);

        //    double lambda = initialLambda;
        //    const double lambdaFactor = 10.0;

        //    for (int iter = 0; iter < maxIterations; iter++)
        //    {
        //        // 2) 构建 J(n×p)、r(n)（线性于参数：J_ik = ϕ_k(x_i,y_i)）
        //        var J = Matrix<double>.Build.Dense(n, p);
        //        var r = Vector<double>.Build.Dense(n);

        //        for (int k = 0; k < n; k++)
        //        {
        //            double x = factorMatrix[k, 0], y = factorMatrix[k, 1];

        //            // ϕ(x,y)
        //            for (int t = 0; t < p; t++)
        //                J[k, t] = Basis(x, y, expo[t].i, expo[t].j);

        //            // 预测与残差
        //            double zhat = 0.0;
        //            for (int t = 0; t < p; t++) zhat += beta[t] * J[k, t];
        //            r[k] = responseVector[k] - zhat;
        //        }

        //        // 3) H=JᵀJ, g=Jᵀr
        //        var JT = J.Transpose();
        //        var H = JT * J;
        //        var g = JT * r;

        //        // 梯度收敛
        //        if (g.L2Norm() < tolerance) break;

        //        // 阻尼
        //        for (int d = 0; d < p; d++) H[d, d] += lambda;

        //        // 4) LM+LU 解 (H+λI)Δ = g
        //        Vector<double> delta;
        //        try
        //        {
        //            delta = H.LU().Solve(g);
        //        }
        //        catch
        //        {
        //            lambda *= lambdaFactor;
        //            if (lambda > 1e12) break;
        //            continue;
        //        }

        //        // 步长收敛
        //        if (delta.L2Norm() < tolerance) { beta += delta; break; }

        //        // 5) 接受/拒绝（比较 SSE）
        //        var betaNew = beta + delta;
        //        double errOld = SSE(factorMatrix, responseVector, beta, expo);
        //        double errNew = SSE(factorMatrix, responseVector, betaNew, expo);

        //        if (errNew < errOld)
        //        {
        //            beta = betaNew;
        //            lambda /= lambdaFactor;
        //        }
        //        else
        //        {
        //            lambda *= lambdaFactor;
        //            if (lambda > 1e12) break;
        //        }
        //    }

        //    // 6) 公式与 MSE
        //    string formula = BuildFormula(beta, expo);
        //    double sse = SSE(factorMatrix, responseVector, beta, expo);
        //    double mse = sse / Math.Max(1, n - p);

        //    return (beta, formula, mse, terms);
        //}

        // --- Predict（保持与 RobustPolynomialFitting 风格一致） ---
        // 批量预测：factorMatrix 为 n×k，parameters 长度应等于 terms 的项数
        public static Vector<double> Predict(
            Matrix<double> factorMatrix,
            Vector<double> parameters,
            List<string> terms)
        {
            if (factorMatrix == null || parameters == null || terms == null)
                throw new ArgumentNullException("factorMatrix/parameters/terms 不能为空。");

            int n = factorMatrix.RowCount;
            int k = factorMatrix.ColumnCount;

            // 由 terms 解析得到每一列对应的指数向量 a=(a1..ak)
            var exps = ParseTermsToExponents(k, terms);
            if (parameters.Count != exps.Count)
                throw new ArgumentException("parameters 长度与 terms 解析出的项数不一致。");

            var y = Vector<double>.Build.Dense(n, 0.0);
            for (int r = 0; r < n; r++)
            {
                var xr = factorMatrix.Row(r);
                double z = 0.0;
                for (int j = 0; j < exps.Count; j++)
                    z += parameters[j] * Basis(xr, exps[j]);
                y[r] = z;
            }
            return y;
        }

        // 单点预测：x 为 k 维向量
        public static double Predict(
            Vector<double> x,
            Vector<double> parameters,
            List<string> terms)
        {
            if (x == null || parameters == null || terms == null)
                throw new ArgumentNullException("x/parameters/terms 不能为空。");

            var exps = ParseTermsToExponents(x.Count, terms);
            if (parameters.Count != exps.Count)
                throw new ArgumentException("parameters 长度与 terms 解析出的项数不一致。");

            double z = 0.0;
            for (int j = 0; j < exps.Count; j++)
                z += parameters[j] * Basis(x, exps[j]);
            return z;
        }

        // ===== 工具：由 terms 解析指数向量（支持 1、x1、x2^3、x1*x3、x1^2*x3、x1x2 等） =====
        private static List<int[]> ParseTermsToExponents(int k, List<string> terms)
        {
            var list = new List<int[]>(terms.Count);

            foreach (var raw in terms)
            {
                var term = raw.Replace(" ", "");
                var a = new int[k]; // 全 0

                if (term == "1" || string.IsNullOrEmpty(term))
                {
                    list.Add(a);
                    continue;
                }

                // 支持 * 分隔，也支持连写（x1x2 等）。先按 * 切分；若无 *，整体作为一个段处理。
                var segments = term.Contains("*") ? term.Split('*') : new[] { term };

                foreach (var seg in segments)
                {
                    // 解析一段里可能的多次出现（允许连写，如 "x1^2x3"）
                    int i = 0;
                    while (i < seg.Length)
                    {
                        if (seg[i] != 'x' && seg[i] != 'X') { i++; continue; }

                        i++; // 跳过 'x'
                             // 读取索引
                        int idx = 0;
                        while (i < seg.Length && char.IsDigit(seg[i]))
                        {
                            idx = idx * 10 + (seg[i] - '0');
                            i++;
                        }
                        if (idx <= 0 || idx > k)
                            throw new ArgumentException($"项 \"{raw}\" 中的因子索引超界：x{idx} (1..{k})");

                        // 可选幂次 ^p
                        int power = 1;
                        if (i < seg.Length && seg[i] == '^')
                        {
                            i++; // 跳过 ^
                            int p = 0;
                            if (i >= seg.Length || !char.IsDigit(seg[i]))
                                throw new ArgumentException($"项 \"{raw}\" 的幂次缺失：x{idx}^?");
                            while (i < seg.Length && char.IsDigit(seg[i]))
                            {
                                p = p * 10 + (seg[i] - '0');
                                i++;
                            }
                            power = Math.Max(1, p);
                        }

                        a[idx - 1] += power;
                    }
                }

                list.Add(a);
            }

            return list;
        }

        // ===== 工具：基函数 ϕ_a(x) = ∏ x_i^{a_i}（已在你类里存在则复用）=====

        // --- 内部工具 ---
        private static double Basis(double x, double y, int i, int j)
        {
            double xi = (i == 0) ? 1.0 : Math.Pow(x, i);
            double yj = (j == 0) ? 1.0 : Math.Pow(y, j);
            return xi * yj;
        }

        private static string BuildTermName(int i, int j)
        {
            if (i == 0 && j == 0) return "1";
            string xi = i == 0 ? "" : (i == 1 ? "x1" : $"x1^{i}");
            string yj = j == 0 ? "" : (j == 1 ? "x2" : $"x2^{j}");
            if (xi == "") return yj;
            if (yj == "") return xi;
            return xi + yj;
        }

        private static string BuildFormula(Vector<double> b, List<(int i, int j)> expo)
        {
            var parts = new List<string>();
            for (int t = 0; t < b.Count; t++)
            {
                string name = BuildTermName(expo[t].i, expo[t].j);
                string coeff = b[t].ToString("0.##E+0");
                parts.Add($"{coeff}*{name}");
            }
            // 把常数项的 "*1" 去掉
            string f = string.Join(" + ", parts);
            f = f.Replace("*1", "");
            return "z = " + f;
        }

        private static double SSE(Matrix<double> X, Vector<double> y, Vector<double> b, List<(int i, int j)> expo)
        {
            double s = 0.0;
            for (int k = 0; k < X.RowCount; k++)
            {
                double x = X[k, 0], yy = X[k, 1];
                double z = 0.0;
                for (int t = 0; t < expo.Count; t++)
                    z += b[t] * Basis(x, yy, expo[t].i, expo[t].j);
                double rk = y[k] - z;
                s += rk * rk;
            }
            return s;
        }
    }

    public static class PolynomialExtrema
    {
        /// <summary>
        /// 求多项式的真正极值点（稳健方法）
        /// </summary>
        public static (double value, Vector<double> x) FindTrueExtremum(
            Vector<double> parameters,
            List<string> terms,
            Matrix<double> ranges,
            bool findMaximum = true)
        {
            int k = ranges.RowCount;
            var exps = ParseTermsToExponents(k, terms);

            // 使用多种方法求极值，取最好的结果
            var candidates = new List<(double value, Vector<double> x)>();

            // 方法1：解析解（仅适用于二次多项式）
            if (IsQuadraticPolynomial(exps))
            {
                try
                {
                    var analytical = FindQuadraticExtremum(parameters, exps, ranges, findMaximum);
                    candidates.Add(analytical);
                }
                catch { }
            }

            // 方法2：多起点梯度方法
            for (int start = 0; start < 10; start++)
            {
                try
                {
                    var gradient = FindGradientExtremum(parameters, exps, ranges, findMaximum, start);
                    candidates.Add(gradient);
                }
                catch { }
            }

            // 方法3：边界搜索
            var boundary = FindBoundaryExtremum(parameters, exps, ranges, findMaximum);
            candidates.Add(boundary);

            // 方法4：网格搜索（作为备选）
            var grid = FindGridExtremum(parameters, exps, ranges, findMaximum, 50);
            candidates.Add(grid);

            // 选择最佳结果
            if (candidates.Count == 0)
                throw new InvalidOperationException("无法找到极值点");

            return findMaximum
                ? candidates.OrderByDescending(c => c.value).First()
                : candidates.OrderBy(c => c.value).First();
        }

        /// <summary>
        /// 求多项式的最大值
        /// </summary>
        public static (double maxValue, Vector<double> x) FindMaximum(
            Vector<double> parameters,
            List<string> terms,
            Matrix<double> ranges)
        {
            return FindTrueExtremum(parameters, terms, ranges, true);
        }

        /// <summary>
        /// 求多项式的最小值
        /// </summary>
        public static (double minValue, Vector<double> x) FindMinimum(
            Vector<double> parameters,
            List<string> terms,
            Matrix<double> ranges)
        {
            return FindTrueExtremum(parameters, terms, ranges, false);
        }

        // ===== 核心方法 =====

        /// <summary>
        /// 计算多项式在给定点的值
        /// </summary>
        private static double EvaluatePolynomial(Vector<double> x, Vector<double> parameters, List<int[]> exps)
        {
            double result = 0.0;
            for (int i = 0; i < exps.Count; i++)
            {
                double termValue = 1.0;
                for (int j = 0; j < x.Count; j++)
                {
                    if (exps[i][j] > 0)
                        termValue *= Math.Pow(x[j], exps[i][j]);
                }
                result += parameters[i] * termValue;
            }
            return result;
        }

        /// <summary>
        /// 判断是否为二次多项式
        /// </summary>
        private static bool IsQuadraticPolynomial(List<int[]> exps)
        {
            foreach (var exp in exps)
            {
                int totalDegree = exp.Sum();
                if (totalDegree > 2) return false;
            }
            return true;
        }

        /// <summary>
        /// 求二次多项式的极值点（解析解）
        /// </summary>
        private static (double value, Vector<double> x) FindQuadraticExtremum(
            Vector<double> parameters,
            List<int[]> exps,
            Matrix<double> ranges,
            bool findMaximum)
        {
            int k = ranges.RowCount;

            // 构建二次型矩阵 Q 和线性项向量 c
            var Q = Matrix<double>.Build.Dense(k, k);
            var c = Vector<double>.Build.Dense(k);

            // 从多项式项中提取系数
            for (int i = 0; i < exps.Count; i++)
            {
                var exp = exps[i];
                var param = parameters[i];

                // 常数项
                if (exp.All(x => x == 0))
                {
                    // 常数项不参与极值计算
                    continue;
                }

                // 线性项
                for (int j = 0; j < k; j++)
                {
                    if (exp[j] == 1 && exp.Count(x => x > 0) == 1)
                    {
                        c[j] += param;
                    }
                }

                // 二次项
                for (int j = 0; j < k; j++)
                {
                    for (int l = 0; l < k; l++)
                    {
                        if (j == l && exp[j] == 2)
                        {
                            Q[j, l] += 2 * param;
                        }
                        else if (j != l && exp[j] == 1 && exp[l] == 1)
                        {
                            Q[j, l] += param;
                            Q[l, j] += param;
                        }
                    }
                }
            }

            // 求解 Qx + c = 0，即 x = -Q^(-1)c
            Vector<double> x;
            try
            {
                x = -Q.Inverse() * c;
            }
            catch
            {
                throw new InvalidOperationException("无法求解二次多项式的极值点");
            }

            // 检查极值点是否在范围内
            bool inRange = true;
            for (int i = 0; i < k; i++)
            {
                if (x[i] < ranges[i, 0] || x[i] > ranges[i, 1])
                {
                    inRange = false;
                    break;
                }
            }

            if (inRange)
            {
                double value = EvaluatePolynomial(x, parameters, exps);
                return (value, x);
            }
            else
            {
                throw new InvalidOperationException("极值点超出范围");
            }
        }

        /// <summary>
        /// 梯度方法求极值
        /// </summary>
        private static (double value, Vector<double> x) FindGradientExtremum(
            Vector<double> parameters,
            List<int[]> exps,
            Matrix<double> ranges,
            bool findMaximum,
            int startSeed)
        {
            int k = ranges.RowCount;
            var x = Vector<double>.Build.Dense(k, 0.0);

            // 随机初始化起点
            var random = new Random(startSeed);
            for (int i = 0; i < k; i++)
            {
                x[i] = ranges[i, 0] + random.NextDouble() * (ranges[i, 1] - ranges[i, 0]);
            }

            double learningRate = 0.01;
            int maxIterations = 1000;
            double tolerance = 1e-8;

            for (int iter = 0; iter < maxIterations; iter++)
            {
                var gradient = ComputeGradient(x, parameters, exps);

                if (gradient.L2Norm() < tolerance) break;

                // 梯度上升（最大值）或梯度下降（最小值）
                var direction = findMaximum ? gradient : -gradient;
                x += learningRate * direction;

                // 确保在范围内
                for (int i = 0; i < k; i++)
                {
                    x[i] = Math.Max(ranges[i, 0], Math.Min(ranges[i, 1], x[i]));
                }
            }

            double value = EvaluatePolynomial(x, parameters, exps);
            return (value, x);
        }

        /// <summary>
        /// 边界搜索
        /// </summary>
        private static (double value, Vector<double> x) FindBoundaryExtremum(
            Vector<double> parameters,
            List<int[]> exps,
            Matrix<double> ranges,
            bool findMaximum)
        {
            int k = ranges.RowCount;
            double bestValue = findMaximum ? double.NegativeInfinity : double.PositiveInfinity;
            var bestX = Vector<double>.Build.Dense(k, 0.0);

            // 检查所有边界点
            for (int i = 0; i < k; i++)
            {
                // 检查 x_i 的下界
                var x = Vector<double>.Build.Dense(k, 0.0);
                for (int j = 0; j < k; j++)
                {
                    if (j == i) x[j] = ranges[j, 0];
                    else x[j] = 0.5 * (ranges[j, 0] + ranges[j, 1]); // 中点
                }

                double value = EvaluatePolynomial(x, parameters, exps);
                if (findMaximum ? (value > bestValue) : (value < bestValue))
                {
                    bestValue = value;
                    bestX = x;
                }

                // 检查 x_i 的上界
                x = Vector<double>.Build.Dense(k, 0.0);
                for (int j = 0; j < k; j++)
                {
                    if (j == i) x[j] = ranges[j, 1];
                    else x[j] = 0.5 * (ranges[j, 0] + ranges[j, 1]); // 中点
                }

                value = EvaluatePolynomial(x, parameters, exps);
                if (findMaximum ? (value > bestValue) : (value < bestValue))
                {
                    bestValue = value;
                    bestX = x;
                }
            }

            return (bestValue, bestX);
        }

        /// <summary>
        /// 网格搜索（备选方法）
        /// </summary>
        private static (double value, Vector<double> x) FindGridExtremum(
            Vector<double> parameters,
            List<int[]> exps,
            Matrix<double> ranges,
            bool findMaximum,
            int samples)
        {
            int k = ranges.RowCount;
            double bestValue = findMaximum ? double.NegativeInfinity : double.PositiveInfinity;
            var bestX = Vector<double>.Build.Dense(k, 0.0);

            // 网格搜索
            for (int i = 0; i <= samples; i++)
            {
                var x = Vector<double>.Build.Dense(k, 0.0);
                x[0] = ranges[0, 0] + (ranges[0, 1] - ranges[0, 0]) * i / samples;

                for (int j = 0; j <= samples; j++)
                {
                    x[1] = ranges[1, 0] + (ranges[1, 1] - ranges[1, 0]) * j / samples;

                    if (k > 2)
                    {
                        for (int l = 0; l <= samples; l++)
                        {
                            x[2] = ranges[2, 0] + (ranges[2, 1] - ranges[2, 0]) * l / samples;

                            if (k > 3)
                            {
                                for (int m = 0; m <= samples; m++)
                                {
                                    x[3] = ranges[3, 0] + (ranges[3, 1] - ranges[3, 0]) * m / samples;

                                    double value = EvaluatePolynomial(x, parameters, exps);
                                    if (findMaximum ? (value > bestValue) : (value < bestValue))
                                    {
                                        bestValue = value;
                                        bestX = x;
                                    }
                                }
                            }
                            else
                            {
                                double value = EvaluatePolynomial(x, parameters, exps);
                                if (findMaximum ? (value > bestValue) : (value < bestValue))
                                {
                                    bestValue = value;
                                    bestX = x;
                                }
                            }
                        }
                    }
                    else
                    {
                        double value = EvaluatePolynomial(x, parameters, exps);
                        if (findMaximum ? (value > bestValue) : (value < bestValue))
                        {
                            bestValue = value;
                            bestX = x;
                        }
                    }
                }
            }

            return (bestValue, bestX);
        }

        /// <summary>
        /// 计算梯度
        /// </summary>
        private static Vector<double> ComputeGradient(Vector<double> x, Vector<double> parameters, List<int[]> exps)
        {
            int k = x.Count;
            var gradient = Vector<double>.Build.Dense(k, 0.0);

            for (int i = 0; i < exps.Count; i++)
            {
                var exp = exps[i];
                var param = parameters[i];

                for (int j = 0; j < k; j++)
                {
                    if (exp[j] > 0)
                    {
                        double term = param * exp[j];
                        for (int l = 0; l < k; l++)
                        {
                            if (l == j)
                            {
                                if (exp[l] > 1) term *= Math.Pow(x[l], exp[l] - 1);
                            }
                            else if (exp[l] > 0)
                            {
                                term *= Math.Pow(x[l], exp[l]);
                            }
                        }
                        gradient[j] += term;
                    }
                }
            }

            return gradient;
        }

        // ===== 辅助方法 =====

        /// <summary>
        /// 解析项字符串到指数向量
        /// </summary>
        private static List<int[]> ParseTermsToExponents(int k, List<string> terms)
        {
            var list = new List<int[]>(terms.Count);

            foreach (var raw in terms)
            {
                var term = raw.Replace(" ", "");
                var a = new int[k];

                if (term == "1" || string.IsNullOrEmpty(term))
                {
                    list.Add(a);
                    continue;
                }

                // 处理项，例如 "x1x2", "x3^2", "x2x3" 等
                int i = 0;
                while (i < term.Length)
                {
                    if (term[i] == 'x' || term[i] == 'X')
                    {
                        i++; // 跳过 'x'

                        // 读取因子索引
                        int idx = 0;
                        while (i < term.Length && char.IsDigit(term[i]))
                        {
                            idx = idx * 10 + (term[i] - '0');
                            i++;
                        }

                        if (idx <= 0 || idx > k)
                            throw new ArgumentException($"项 \"{raw}\" 中索引越界 x{idx} (1..{k})");

                        // 读取幂次
                        int power = 1;
                        if (i < term.Length && term[i] == '^')
                        {
                            i++;
                            int p = 0;
                            if (i >= term.Length || !char.IsDigit(term[i]))
                                throw new ArgumentException($"项 \"{raw}\" 幂缺失：x{idx}^?");
                            while (i < term.Length && char.IsDigit(term[i]))
                            {
                                p = p * 10 + (term[i] - '0');
                                i++;
                            }
                            power = Math.Max(1, p);
                        }

                        a[idx - 1] += power; // 索引从0开始，所以减1
                    }
                    else
                    {
                        i++; // 跳过其他字符
                    }
                }
                list.Add(a);
            }
            return list;
        }
    }
}
