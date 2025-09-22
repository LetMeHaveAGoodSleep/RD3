using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.Optimization.ObjectiveFunctions; // 这个命名空间可能包含目标函数相关的类

namespace RD3.Shared
{
    public static class PolynomialFitting
    {
        /// <summary>
        /// 执行多因子多项式拟合
        /// </summary>
        /// <param name="factors">因子矩阵，每行是一个观测点，每列是一个因子</param>
        /// <param name="responses">响应值向量</param>
        /// <param name="degree">多项式次数</param>
        /// <param name="includeInteractions">是否包含交互项（仅当degree>=2时有效）</param>
        /// <returns>拟合系数向量，系数顺序与构建设计矩阵的列顺序一致</returns>
        public static (Vector<double> coefficients, string formula) FitPolynomial(Matrix<double> factors, Vector<double> responses, int degree = 2, bool includeInteractions = true)
        {
            int numObservations = factors.RowCount;
            int numFactors = factors.ColumnCount;

            // 计算设计矩阵的列数（即参数个数）
            //int numParams = CalculateParameterCount(numFactors, degree, includeInteractions);

            //if (numObservations <= numParams)
            //{
            //    throw new ArgumentException($"观测点数量({numObservations})必须大于参数个数({numParams})");
            //}

            // 构建设计矩阵
            Matrix<double> designMatrix = BuildDesignMatrix(factors, degree, includeInteractions);

            // 使用最小二乘法求解: β = (X'X)^(-1)X'y
            // MathNet 提供了更稳定、更专业的线性代数方法，例如 QR 分解法求解最小二乘问题[3,6](@ref)
            // 以下是最直接的方法：
            Matrix<double> xtx = designMatrix.Transpose() * designMatrix;
            Matrix<double> xtxInverse = xtx.Inverse();
            Vector<double> coefficients = xtxInverse * (designMatrix.Transpose() * responses);

            // 在返回系数之前，生成公式字符串
            string formula = GeneratePolynomialFormula(coefficients, factors.ColumnCount, degree, includeInteractions);

            return (coefficients, formula);
        }
        /// <summary>
        /// 生成多项式公式字符串 (使用 * 表示乘法)
        /// </summary>
        private static string GeneratePolynomialFormula(Vector<double> coefficients, int numFactors, int degree, bool includeInteractions)
        {
            var sb = new StringBuilder();
            sb.Append("y = ");

            // 获取因子名称（默认使用 x1, x2, x3...）
            string[] factorNames = new string[numFactors];
            for (int i = 0; i < numFactors; i++)
            {
                factorNames[i] = $"x{i + 1}";
            }

            int coefficientIndex = 0;
            bool isFirstTerm = true; // 用于标记是否为公式中的第一项，以处理正负号显示

            // 常数项
            if (coefficients[coefficientIndex] != 0)
            {
                sb.Append(FormatCoefficient(coefficients[coefficientIndex], isFirstTerm));
                isFirstTerm = false;
                coefficientIndex++;
            }

            // 一次项 (格式: β1*x1)
            for (int i = 0; i < numFactors; i++)
            {
                if (coefficients[coefficientIndex] != 0)
                {
                    sb.Append(FormatTerm(coefficients[coefficientIndex], factorNames[i], isFirstTerm));
                    isFirstTerm = false;
                }
                coefficientIndex++;
            }

            // 二次项和交互项（当degree>=2时）
            if (degree >= 2)
            {
                // 平方项 (格式: β11*x1^2)
                for (int i = 0; i < numFactors; i++)
                {
                    if (coefficients[coefficientIndex] != 0)
                    {
                        sb.Append(FormatTerm(coefficients[coefficientIndex], $"{factorNames[i]}^2", isFirstTerm));
                        isFirstTerm = false;
                    }
                    coefficientIndex++;
                }

                // 交互项 (格式: β12*x1*x2)
                if (includeInteractions)
                {
                    for (int i = 0; i < numFactors; i++)
                    {
                        for (int j = i + 1; j < numFactors; j++)
                        {
                            if (coefficients[coefficientIndex] != 0)
                            {
                                sb.Append(FormatTerm(coefficients[coefficientIndex], $"{factorNames[i]}*{factorNames[j]}", isFirstTerm));
                                isFirstTerm = false;
                            }
                            coefficientIndex++;
                        }
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 格式化项（系数 + 变量），使用 * 表示乘法
        /// </summary>
        private static string FormatTerm(double coefficient, string variable, bool isFirstTerm)
        {
            if (coefficient == 0) return "";

            string sign;
            double absValue = Math.Abs(coefficient);

            // 根据是否是第一项和系数的正负来决定符号的显示
            if (isFirstTerm)
            {
                sign = coefficient >= 0 ? "" : "-";
            }
            else
            {
                sign = coefficient >= 0 ? " + " : " - ";
            }

            // 根据系数绝对值是否为1来决定是否显示系数数值
            if (Math.Abs(absValue - 1.0) < 1e-10) // 考虑浮点数精度
            {
                return $"{sign}{variable}";
            }
            else
            {
                return $"{sign}{absValue:F4}*{variable}";
            }
        }

        /// <summary>
        /// 格式化常数项系数
        /// </summary>
        private static string FormatCoefficient(double coefficient, bool isFirstTerm)
        {
            if (coefficient == 0) return "";

            string sign;
            double absValue = Math.Abs(coefficient);

            if (isFirstTerm)
            {
                sign = coefficient >= 0 ? "" : "-";
            }
            else
            {
                sign = coefficient >= 0 ? " + " : " - ";
            }

            return $"{sign}{absValue:F4}";
        }

        /// <summary>
        /// 使用拟合的系数进行预测
        /// </summary>
        public static double Predict(Vector<double> coefficients, Vector<double> factorValues, int degree = 2, bool includeInteractions = true)
        {
            Vector<double> xVector = BuildFeatureVector(factorValues, degree, includeInteractions);
            return xVector.DotProduct(coefficients);
        }

        /// <summary>
        /// 构建设计矩阵 (核心改动方法)
        /// </summary>
        private static Matrix<double> BuildDesignMatrix(Matrix<double> factors, int degree, bool includeInteractions)
        {
            int numObservations = factors.RowCount;
            int numFactors = factors.ColumnCount;
            int numParams = CalculateParameterCount(numFactors, degree, includeInteractions);

            // 使用 DenseMatrix 构建一个明确的行列数矩阵[7](@ref)
            var designMatrix = Matrix<double>.Build.Dense(numObservations, numParams);

            for (int i = 0; i < numObservations; i++)
            {
                // 直接使用 Matrix.Row(i) 获取当前观测点的因子值向量，更简洁[3](@ref)
                Vector<double> currentFactorValues = factors.Row(i);

                // 构建特征向量并赋值到设计矩阵的当前行
                Vector<double> featureVector = BuildFeatureVector(currentFactorValues, degree, includeInteractions);
                designMatrix.SetRow(i, featureVector); // 使用 SetRow 方法批量赋值[3](@ref)
            }

            return designMatrix;
        }

        /// <summary>
        /// 构建特征向量（对应设计矩阵的一行）
        /// </summary>
        private static Vector<double> BuildFeatureVector(Vector<double> factorValues, int degree, bool includeInteractions)
        {
            int numFactors = factorValues.Count;
            var features = new List<double>();

            // 1. 添加常数项
            features.Add(1.0);

            // 2. 添加所有一次项 (即因子值本身)
            features.AddRange(factorValues);

            // 3. 添加高次项和交互项（当degree>=2时）
            if (degree >= 2)
            {
                // 添加所有平方项
                for (int i = 0; i < numFactors; i++)
                {
                    features.Add(Math.Pow(factorValues[i], 2));
                }

                // 添加所有两两交互项
                if (includeInteractions)
                {
                    for (int i = 0; i < numFactors; i++)
                    {
                        for (int j = i + 1; j < numFactors; j++)
                        {
                            features.Add(factorValues[i] * factorValues[j]);
                        }
                    }
                }
            }

            // 4. 可根据需要扩展更高阶项（如三次项 degree>=3）
            // ...

            // 将列表转换为 MathNet 向量[3](@ref)
            return Vector<double>.Build.Dense(features.ToArray());
        }

        /// <summary>
        /// 计算参数个数 (无需改动)
        /// </summary>
        private static int CalculateParameterCount(int numFactors, int degree, bool includeInteractions)
        {
            int count = 1; // 常数项

            // 一次项
            count += numFactors;

            if (degree >= 2)
            {
                // 平方项
                count += numFactors;

                // 交互项
                if (includeInteractions)
                {
                    count += numFactors * (numFactors - 1) / 2;
                }
            }

            // 可在此添加更高阶项的参数计数
            return count;
        }
    }

    public static class LMFitter
    {
        /// <summary>
        /// 使用Levenberg-Marquardt算法进行多项式拟合
        /// </summary>
        /// <param name="factorMatrix">自变量矩阵（每行一个样本，每列一个自变量）</param>
        /// <param name="responseVector">响应变量向量</param>
        /// <param name="degree">多项式最高阶数</param>
        /// <param name="includeInteractions">是否包含交互项</param>
        /// <param name="initialLambda">初始阻尼系数</param>
        /// <param name="maxIterations">最大迭代次数</param>
        /// <param name="tolerance">收敛容差</param>
        /// <returns>拟合参数和公式字符串、均方误差</returns>
        public static (Vector<double> Parameters, string Formula,double mse, List<string> Terms) LMFit(
            Matrix<double> factorMatrix,
            Vector<double> responseVector,
            int degree = 2,
            bool includeInteractions = true,
            double initialLambda = 0.01,
            int maxIterations = 100,
            double tolerance = 1e-8)
        {
            // 输入验证
            ValidateInputs(factorMatrix, responseVector, degree);

            int variableCount = factorMatrix.ColumnCount;
            int sampleCount = factorMatrix.RowCount;

            // 生成多项式项和表达式
            var (termExpressions, termEvaluators) = GeneratePolynomialTerms(variableCount, degree, includeInteractions);
            int parameterCount = termExpressions.Count;

            // 初始化参数
            Vector<double> parameters = DenseVector.OfArray(Enumerable.Repeat(0.1, parameterCount).ToArray());

            // 计算初始残差和误差
            Vector<double> residuals = CalculateResiduals(factorMatrix, responseVector, parameters, termEvaluators);
            double error = residuals.DotProduct(residuals);

            // LM算法参数
            double lambda = initialLambda;
            const double lambdaFactor = 10.0;
            bool converged = false;

            for (int iter = 0; iter < maxIterations; iter++)
            {
                // 计算雅可比矩阵
                Matrix<double> jacobian = CalculateJacobian(factorMatrix, termEvaluators);

                // 计算J^T*J和J^T*r
                Matrix<double> jTj = jacobian.Transpose() * jacobian;
                Vector<double> jTr = jacobian.Transpose() * residuals;

                // 添加阻尼项 - 修正单位矩阵创建方式
                // 使用CreateDiagonal创建单位矩阵，对角线元素为1.0
                Matrix<double> identity = DenseMatrix.CreateDiagonal(parameterCount, parameterCount, 1.0);
                Matrix<double> jTjDamped = jTj + identity * lambda;

                // 求解线性方程组获取参数更新量
                Vector<double> delta = jTjDamped.Solve(jTr);

                // 尝试更新参数
                Vector<double> newParameters = parameters + delta;
                Vector<double> newResiduals = CalculateResiduals(factorMatrix, responseVector, newParameters, termEvaluators);
                double newError = newResiduals.DotProduct(newResiduals);

                // 检查是否改善
                if (newError < error)
                {
                    parameters = newParameters;
                    residuals = newResiduals;
                    error = newError;
                    lambda /= lambdaFactor;

                    // 检查收敛条件
                    if (delta.Norm(2) < tolerance)
                    {
                        converged = true;
                        break;
                    }
                }
                else
                {
                    lambda *= lambdaFactor;
                }
            }

            if (!converged)
            {
                Console.WriteLine("警告：达到最大迭代次数，可能未完全收敛");
            }

            // 生成拟合公式
            string formula = GenerateFormula(parameters, termExpressions);

            // 计算MSE（均方误差）
            double mse = CalculateMSE(residuals, sampleCount, parameterCount);

            return (parameters, formula, mse, termExpressions);
        }

        /// <summary>
        /// 根据拟合参数和自变量向量预测响应值
        /// </summary>
        /// <param name="parameters">拟合得到的参数向量</param>
        /// <param name="terms">多项式项表达式列表（来自LMFit的返回结果）</param>
        /// <param name="factorValues">自变量向量</param>
        /// <param name="degree">多项式阶数（需与拟合时一致）</param>
        /// <param name="includeInteractions">是否包含交互项（需与拟合时一致）</param>
        /// <returns>预测的响应值</returns>
        public static double Predict(
            Vector<double> parameters,
            List<string> terms,
            Vector<double> factorValues,
            int degree,
            bool includeInteractions)
        {
            // 验证输入
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            if (terms == null) throw new ArgumentNullException(nameof(terms));
            if (factorValues == null) throw new ArgumentNullException(nameof(factorValues));
            if (parameters.Count != terms.Count)
                throw new ArgumentException("参数数量与多项式项数量不匹配");

            // 生成与拟合时一致的评估器
            var evaluators = GenerateTermEvaluators(terms, factorValues.Count, degree, includeInteractions);

            // 计算预测值
            double prediction = 0.0;
            for (int i = 0; i < parameters.Count; i++)
            {
                prediction += parameters[i] * evaluators[i](factorValues);
            }

            return prediction;
        }


        /// <summary>
        /// 生成多项式项评估器
        /// </summary>
        private static List<Func<Vector<double>, double>> GenerateTermEvaluators(
            List<string> terms, int variableCount, int degree, bool includeInteractions)
        {
            // 先验证terms是否与参数匹配
            var (validTerms, evaluators) = GeneratePolynomialTerms(variableCount, degree, includeInteractions);

            if (!terms.SequenceEqual(validTerms))
            {
                throw new ArgumentException("多项式项与给定的阶数和交互项设置不匹配，请使用LMFit返回的terms参数");
            }

            return evaluators;
        }

        /// <summary>
        /// 计算均方误差(MSE)
        /// </summary>
        /// <param name="residuals">残差向量</param>
        /// <param name="sampleCount">样本数量</param>
        /// <param name="parameterCount">参数数量</param>
        /// <returns>均方误差值</returns>
        private static double CalculateMSE(Vector<double> residuals, int sampleCount, int parameterCount)
        {
            // 普通MSE：残差平方和 / 样本数
            double mse = residuals.DotProduct(residuals) / sampleCount;

            // 可选：调整后的MSE（考虑自由度）：残差平方和 / (样本数 - 参数数)
            // 适用于评估模型泛化能力，当样本量远大于参数数时，与普通MSE差异很小
            if (sampleCount > parameterCount)
            {
                mse = residuals.DotProduct(residuals) / (sampleCount - parameterCount);
            }

            return mse;
        }

        /// <summary>
        /// 验证输入有效性
        /// </summary>
        private static void ValidateInputs(Matrix<double> x, Vector<double> y, int degree)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.RowCount != y.Count) throw new ArgumentException("自变量矩阵行数与响应向量长度必须相等");
            if (x.ColumnCount < 1) throw new ArgumentException("自变量数量必须至少为1");
            if (degree < 0) throw new ArgumentException("多项式阶数不能为负数");
            if (degree == 0 && x.ColumnCount > 0) throw new ArgumentException("零阶多项式不能有自变量");
        }

        /// <summary>
        /// 生成多项式项表达式和评估器
        /// </summary>
        private static (List<string> Expressions, List<Func<Vector<double>, double>> Evaluators) GeneratePolynomialTerms(
            int variableCount, int degree, bool includeInteractions)
        {
            var expressions = new List<string>();
            var evaluators = new List<Func<Vector<double>, double>>();

            // 添加常数项
            expressions.Add("1");
            evaluators.Add(v => 1.0);

            // 生成所有可能的多项式项
            GenerateTermsRecursive(
                new int[variableCount],
                0,
                variableCount,
                degree,
                includeInteractions,
                expressions,
                evaluators);

            return (expressions, evaluators);
        }

        /// <summary>
        /// 递归生成多项式项
        /// </summary>
        private static void GenerateTermsRecursive(
            int[] exponents,
            int startIndex,
            int variableCount,
            int maxDegree,
            bool includeInteractions,
            List<string> expressions,
            List<Func<Vector<double>, double>> evaluators)
        {
            for (int i = startIndex; i < variableCount; i++)
            {
                int[] newExponents = (int[])exponents.Clone();
                newExponents[i]++;

                int currentDegree = newExponents.Sum();
                if (currentDegree > maxDegree)
                    continue;

                // 检查是否为纯交互项（所有指数都是1且变量数>1）
                bool isInteraction = newExponents.All(e => e <= 1) && newExponents.Sum() > 1;
                if (isInteraction && !includeInteractions)
                    continue;

                // 生成项表达式
                string term = string.Join("*", newExponents
                    .Select((exp, idx) => exp > 0 ? $"x{idx + 1}" + (exp > 1 ? $"^{exp}" : "") : "")
                    .Where(s => !string.IsNullOrEmpty(s)));

                expressions.Add(term);

                // 创建项评估器
                int[] capturedExponents = newExponents; // 捕获当前指数数组
                evaluators.Add(v =>
                {
                    double result = 1.0;
                    for (int j = 0; j < variableCount; j++)
                    {
                        if (capturedExponents[j] > 0)
                        {
                            result *= Math.Pow(v[j], capturedExponents[j]);
                        }
                    }
                    return result;
                });

                // 递归生成更高阶项
                GenerateTermsRecursive(newExponents, includeInteractions ? i : i + 1, variableCount, maxDegree, includeInteractions, expressions, evaluators);
            }
        }

        /// <summary>
        /// 计算残差向量
        /// </summary>
        private static Vector<double> CalculateResiduals(
            Matrix<double> x,
            Vector<double> y,
            Vector<double> parameters,
            List<Func<Vector<double>, double>> termEvaluators)
        {
            int n = x.RowCount;
            Vector<double> residuals = DenseVector.OfArray(new double[n]);

            for (int i = 0; i < n; i++)
            {
                double prediction = 0.0;
                Vector<double> row = x.Row(i);

                for (int j = 0; j < parameters.Count; j++)
                {
                    prediction += parameters[j] * termEvaluators[j](row);
                }

                residuals[i] = y[i] - prediction;
            }

            return residuals;
        }

        /// <summary>
        /// 计算雅可比矩阵
        /// </summary>
        private static Matrix<double> CalculateJacobian(
            Matrix<double> x,
            List<Func<Vector<double>, double>> termEvaluators)
        {
            int n = x.RowCount;
            int p = termEvaluators.Count;
            Matrix<double> jacobian = DenseMatrix.OfArray(new double[n, p]);

            for (int i = 0; i < n; i++)
            {
                Vector<double> row = x.Row(i);
                for (int j = 0; j < p; j++)
                {
                    // 雅可比矩阵元素是残差对参数的偏导数的负值
                    jacobian[i, j] = -termEvaluators[j](row);
                }
            }

            return jacobian;
        }

        /// <summary>
        /// 生成拟合公式字符串
        /// </summary>
        private static string GenerateFormula(Vector<double> parameters, List<string> termExpressions)
        {
            List<string> terms = new List<string>();

            for (int i = 0; i < parameters.Count; i++)
            {
                double param = parameters[i];
                if (Math.Abs(param) < 1e-10) // 忽略接近零的参数
                    continue;

                string term = $"{param:F4}";
                if (i > 0) // 不是常数项
                {
                    term += "*" + termExpressions[i];
                }

                terms.Add(term);
            }

            if (terms.Count == 0)
                return "y = 0";

            // 构建公式，处理正负号
            string formula = "z = " + terms[0];
            for (int i = 1; i < terms.Count; i++)
            {
                if (terms[i].StartsWith("-"))
                {
                    formula += " - " + terms[i].Substring(1);
                }
                else
                {
                    formula += " + " + terms[i];
                }
            }

            return formula;
        }
    }

}
