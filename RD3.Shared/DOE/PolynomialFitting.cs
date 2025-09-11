using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

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
}
