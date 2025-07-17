using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RD3
{
    public static class DOEUtil
    {
       public static double[,] BuildFactorialPoints(IList<OrthogonalParam> factors)
        {
            int factorCount = factors.Count;
            double[] lowerLimits = new double[factorCount];
            double[] upperLimits = new double[factorCount];
            for (int i = 0; i < factorCount; i++)
            {
                lowerLimits[i] = factors[i].Low;
                upperLimits[i] = factors[i].High;
            }
            int factorialPointsCount = (int)Math.Pow(2, factorCount);
            double[,] factorialPoints = new double[factorialPointsCount, factorCount];
            for (int i = 0; i < factorialPointsCount; i++)
            {
                int binaryRepresentation = i;
                for (int j = 0; j < factorCount; j++)
                {
                    // 通过二进制表示来确定每个因素取下限还是上限
                    int bit = binaryRepresentation % 2;
                    binaryRepresentation /= 2;
                    factorialPoints[i, j] = bit == 0 ? lowerLimits[j] : upperLimits[j];
                }
            }
            return factorialPoints;
        }

       public static DataTable GenerateCombinationsAsDataTable(IList<OrthogonalParam> factors)
        {
            DataTable dataTable = new DataTable();
            // 添加列，列名可以根据因素的含义自定义，这里简单以Factor开头加上序号命名
            for (int i = 0; i < factors.Count; i++)
            {
                string colName = factors[i].Name + "_SP";
                dataTable.Columns.Add(colName);
            }
            int factorCount = factors.Count;
            double[] lowerLimits = new double[factorCount];
            double[] upperLimits = new double[factorCount];
            for (int i = 0; i < factorCount; i++)
            {
                lowerLimits[i] = factors[i].Low;
                upperLimits[i] = factors[i].High;
            }
            int factorialPointsCount = (int)Math.Pow(2, factorCount);

            for (int i = 0; i < factorialPointsCount; i++)
            {
                DataRow row = dataTable.NewRow();
                for (int j = 0; j < factorCount; j++)
                {
                    // 根据二进制表示来确定每个因素取下限还是上限值
                    bool useLowerLimit = ((i >> j) & 1) == 0;
                    double value = useLowerLimit ? lowerLimits[j] : upperLimits[j];
                    row[j] = value;
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }

        public static double CalculateAlpha(int factorCount, DOEAlpha alpha)
        {
            switch (alpha)
            {
                case DOEAlpha.Orthogonal:
                    return Math.Sqrt((Math.Sqrt(factorCount + 2) + Math.Sqrt(factorCount)) /
            (2 * Math.Sqrt(factorCount + 2) - 2 * Math.Sqrt(factorCount)));
                case DOEAlpha.Rotatable:
                    return Math.Pow(2, factorCount / 4.0);
                //case DOEAlpha.Spherical:
                //    return Math.Sqrt(factorCount);
                default:
                    return Math.Sqrt((Math.Sqrt(factorCount + 2) + Math.Sqrt(factorCount)) /
            (2 * Math.Sqrt(factorCount + 2) - 2 * Math.Sqrt(factorCount)));
            }
        }


        /// <summary>
        /// 构建中心复合设计表
        /// </summary>
        public static Dictionary<string, List<double>> BuildCCDDesign(IList<OrthogonalParam> paramCol,(int center1, int center2)? centerPoints,DOEAlpha alpha, DOEFace face)
        {
            Dictionary<string, List<double>> factorRanges = new Dictionary<string, List<double>>();
            foreach (OrthogonalParam param in paramCol)
            {
                factorRanges.Add(param.Name, [param.Low, param.High]);
            }

            // 设置默认中心点配置
            var centers = centerPoints ?? (2, 2);

            // 清理水平范围数据
            foreach (var factor in factorRanges.Keys.ToList())
            {
                var levels = factorRanges[factor];
                if (levels.Count > 2)
                {
                    Console.WriteLine($"警告：{factor}的水平数超过2个，已自动截取两端值");
                    factorRanges[factor] = new List<double> { levels[0], levels[levels.Count - 1] };
                }
            }

            // 计算并添加中点值
            foreach (var factor in factorRanges.Keys.ToList())
            {
                var levels = factorRanges[factor];
                if (levels.Count == 2)
                {
                    double midpoint = (levels[0] + levels[1]) / 2;
                    var newLevels = new List<double> { levels[0], midpoint, levels[1] };
                    factorRanges[factor] = newLevels;
                }
            }

            // 生成编码设计矩阵
            int factorCount = factorRanges.Count;
            var codedMatrix = CorrectCcDesign(factorCount, centers, alpha, face);

            // 转换为实际值数据表
            var designTable = new Dictionary<string, List<double>>();
            var levelValues = factorRanges.Values.ToList();

            // 初始化每列数据
            foreach (var factor in factorRanges.Keys)
            {
                designTable[factor] = new List<double>();
            }

            // 填充实际值
            for (int row = 0; row < codedMatrix.GetLength(0); row++)
            {
                for (int col = 0; col < factorCount; col++)
                {
                    string factorName = factorRanges.Keys.ElementAt(col);
                    double codedValue = codedMatrix[row, col];
                    double actualValue = ConvertToActualValue(codedValue, levelValues[col]);
                    designTable[factorName].Add(actualValue);
                }
            }
            return designTable;
        }

        /// <summary>
        /// 将编码值转换为实际水平值
        /// </summary>
        private static double ConvertToActualValue(double coded, List<double> levels)
        {
            if (levels.Count == 3) // 有中点的情况
            {
                return coded switch
                {
                    -1 => levels[0], // 低水平
                    0 => levels[1],   // 中点
                    1 => levels[2],   // 高水平
                    _ => levels[1] + (levels[2] - levels[1]) * coded // 星点线性插值
                };
            }
            else // 只有高低水平的情况
            {
                return levels[0] + (coded + 1) * (levels[1] - levels[0]) / 2;
            }
        }

        /// <summary>
        /// 创建中心复合设计矩阵
        /// </summary>
        private static double[,] CorrectCcDesign(int factorCount, (int center1, int center2) centerPoints, DOEAlpha alpha, DOEFace face)
        {
            double[,] factorialPoints = null;
            double[,] starPoints = null;
            double alphaValue = 1;

            // 正交设计
            if (alpha == DOEAlpha.Orthogonal)
            {
                var result = Star(factorCount, centerPoints, alpha, face);
                starPoints = result.Item1;
                alphaValue = result.Item2;
            }
            // 可旋转设计
            else if (alpha == DOEAlpha.Rotatable)
            {
                var result = Star(factorCount, centerPoints, alpha, face);
                starPoints = result.Item1;
                alphaValue = result.Item2;
            }

            // 内接设计
            if (face == DOEFace.Inscribed)
            {
                factorialPoints = Ff2nCorrected(factorCount);
                factorialPoints = MatrixDivide(factorialPoints, alphaValue);

                var result = Star(factorCount, centerPoints, alpha, face);
                starPoints = result.Item1;
                alphaValue = result.Item2;
            }
            // 面心设计
            else if (face == DOEFace.Faced)
            {
                var result = Star(factorCount, centerPoints, alpha, face);
                starPoints = result.Item1;
                alphaValue = result.Item2;

                factorialPoints = Ff2nCorrected(factorCount);
            }
            // 外接设计（默认）
            else if (face == DOEFace.Circumscribed)
            {
                factorialPoints = Ff2nCorrected(factorCount);
            }

            // 生成中心点
            var centerPoints1 = RepeatCenter(factorCount, centerPoints.center1);
            var centerPoints2 = RepeatCenter(factorCount, centerPoints.center2);

            factorialPoints = DOEUtil.Union(factorialPoints, centerPoints1);
            starPoints = DOEUtil.Union(starPoints, centerPoints2);
            var designMatrix = DOEUtil.Union(factorialPoints, starPoints);

            return designMatrix;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factorCount"></param>
        /// <param name="alpha"></param>
        /// <param name="lowCenterPoint"></param>
        /// <param name="HighCenterPoint"></param>
        /// <returns></returns>
        public static (double[,], double) Star(int n, (int, int) center, DOEAlpha alpha, DOEFace face)
        {
            double a = default;
            if (face == DOEFace.Faced)
            {
                a = 1d;
            }
            else
            {
                switch (alpha)
                {
                    case DOEAlpha.Orthogonal:
                        int nc = (int)Math.Pow(2, n);  // 因子设计点数
                        int nco = center.Item1;        // 因子设计的中心点数
                        int na = 2 * n;                // 轴向点数
                        int nao = center.Item2;        // 轴向设计的中心点数                         
                        a = Math.Sqrt(n * (1 + nao / (double)na) / (1 + nco / (double)nc));// 正交设计中的 alpha 值
                        break;
                    case DOEAlpha.Rotatable:
                        int nc1 = (int)Math.Pow(2, n);  // 因子设计点数
                        a = Math.Pow(nc1, 0.25);       // 可旋转设计中的 alpha 值
                        break;
                }
            }

            // 创建矩阵
            double[,] H = new double[2 * n, n];
            for (int i = 0; i < n; i++)
            {
                H[2 * i, i] = -1;
                H[2 * i + 1, i] = 1;
            }

            // 用 alpha 值缩放矩阵
            for (int i = 0; i < H.GetLength(0); i++)
            {
                for (int j = 0; j < H.GetLength(1); j++)
                {
                    H[i, j] *= a;
                }
            }

            return (H, a);
        }

        /// <summary>
        /// 矩阵除法运算（辅助方法）
        /// </summary>
        public static double[,] MatrixDivide(double[,] matrix, double divisor)
        {
            var result = new double[matrix.GetLength(0), matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    result[i, j] = matrix[i, j] / divisor;
                }
            }
            return result;
        }

        /// <summary>
        /// 生成中心点（辅助方法）
        /// </summary>
        public static double[,] RepeatCenter(int n, int repeats)
        {
            var matrix = new double[repeats, n];
            // 所有值保持为0（中心点）
            return matrix;
        }

        /// <summary>
        /// 合并两个矩阵（辅助方法）
        /// </summary>
        public static double[,] Union(double[,] m1, double[,] m2)
        {
            var rows1 = m1.GetLength(0);
            var rows2 = m2.GetLength(0);
            var cols = m1.GetLength(1);

            var result = new double[rows1 + rows2, cols];

            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = m1[i, j];
                }
            }

            for (int i = 0; i < rows2; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[rows1 + i, j] = m2[i, j];
                }
            }

            return result;
        }

        /// <summary>
        /// 创建通用全因子设计矩阵
        /// </summary>
        /// <param name="levels">每个因子的水平数数组</param>
        /// <returns>编码为0到k-1的设计矩阵</returns>
        public static double[,] FullFactCorrected(int[] levels)
        {
            int n = levels.Length;  // 因子数量
            int nbLines = 1;
            foreach (int level in levels)  // 计算总试验次数
            {
                nbLines *= level;
            }

            double[,] H = new double[nbLines, n];  // 初始化设计矩阵

            int levelRepeat = 1;    // 水平重复次数
            int rangeRepeat = nbLines;  // 范围重复次数

            for (int i = 0; i < n; i++)  // 遍历每个因子
            {
                rangeRepeat /= levels[i];  // 更新范围重复次数

                // 生成当前因子的水平序列（带重复）
                int[] lvl = new int[levels[i] * levelRepeat];
                for (int j = 0; j < levels[i]; j++)  // 遍历每个水平
                {
                    for (int k = 0; k < levelRepeat; k++)
                    {
                        lvl[j * levelRepeat + k] = j;  // 填充水平值
                    }
                }

                // 重复水平序列生成完整列
                int[] rng = new int[lvl.Length * rangeRepeat];
                for (int m = 0; m < rangeRepeat; m++)
                {
                    Array.Copy(lvl, 0, rng, m * lvl.Length, lvl.Length);
                }

                // 填充到设计矩阵
                for (int p = 0; p < nbLines; p++)
                {
                    H[p, i] = rng[p];
                }

                levelRepeat *= levels[i];  // 更新水平重复次数
            }

            return H;
        }

        /// <summary>
        /// 创建2水平全因子设计矩阵
        /// </summary>
        /// <param name="n">因子数量</param>
        /// <returns>编码为-1和1的设计矩阵</returns>
        public static double[,] Ff2nCorrected(int n)
        {
            // 生成全2数组
            int[] levels = new int[n];
            for (int i = 0; i < n; i++)
            {
                levels[i] = 2;
            }

            // 生成0/1编码矩阵
            double[,] zeroOneMatrix = FullFactCorrected(levels);

            // 转换为-1/1编码
            for (int i = 0; i < zeroOneMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < zeroOneMatrix.GetLength(1); j++)
                {
                    zeroOneMatrix[i, j] = 2 * zeroOneMatrix[i, j] - 1;
                }
            }

            return zeroOneMatrix;
        }

        public static double[,] CalculateAxialPoints(IList<OrthogonalParam> factors, double alpha, DOEFace face,int centerPoints,int repeat)
        {
            double[,] axialPoints = new double[2 * factors.Count, factors.Count];
            double[] lowerLimits = new double[factors.Count];
            double[] upperLimits = new double[factors.Count];
            for (int i = 0; i < factors.Count; i++)
            {
                lowerLimits[i] = factors[i].Low;
                upperLimits[i] = factors[i].High;
            }
            switch (face)
            {
                case DOEFace.Faced:
                    // 如果Faced选项，按照面心设计方式计算星点坐标
                    for (int i = 0; i < factors.Count; i++)
                    {
                        // 计算正轴方向星点
                        double[] positiveAxialPoint = new double[factors.Count];
                        for (int j = 0; j < factors.Count; j++)
                        {
                            positiveAxialPoint[j] = j == i ? upperLimits[i] : (lowerLimits[j] + upperLimits[j]) / 2;
                        }
                        for (int j = 0; j < factors.Count; j++)
                        {
                            axialPoints[i, j] = positiveAxialPoint[j];
                        }

                        // 计算负轴方向星点
                        double[] negativeAxialPoint = new double[factors.Count];
                        for (int j = 0; j < factors.Count; j++)
                        {
                            negativeAxialPoint[j] = j == i ? lowerLimits[i] : (lowerLimits[j] + upperLimits[j]) / 2;
                        }
                        for (int j = 0; j < factors.Count; j++)
                        {
                            axialPoints[factors.Count + i, j] = negativeAxialPoint[j];
                        }
                    }
                    break;
                case DOEFace.Inscribed:
                    // 如果是Inscribed选项，按照内接设计方式计算星点坐标（这里假设存在CalculateInscribedRadius方法来计算半径radius）
                    double radius = CalculateInscribedRadius(factors.Count, repeat);
                    for (int i = 0; i < factors.Count; i++)
                    {
                        double[] positiveAxialPoint = new double[factors.Count];
                        for (int j = 0; j < factors.Count; j++)
                        {
                            positiveAxialPoint[j] = j == i ? (lowerLimits[i] + upperLimits[i]) / 2 + radius : (lowerLimits[j] + upperLimits[j]) / 2;
                        }
                        for (int j = 0; j < factors.Count; j++)
                        {
                            axialPoints[i, j] = positiveAxialPoint[j];
                        }

                        // 计算负轴方向星点（与正轴类似逻辑）
                        double[] negativeAxialPoint = new double[factors.Count];
                        for (int j = 0; j < factors.Count; j++)
                        {
                            negativeAxialPoint[j] = j == i ? (lowerLimits[i] + upperLimits[i]) / 2 - radius : (lowerLimits[j] + upperLimits[j]) / 2;
                        }
                        for (int j = 0; j < factors.Count; j++)
                        {
                            axialPoints[factors.Count + i, j] = negativeAxialPoint[j];
                        }
                    }
                    break;
                case DOEFace.Circumscribed:
                    double radius1 = CalculateCircumscribedRadius(factors.Count, centerPoints, lowerLimits, upperLimits);
                    for (int i = 0; i < factors.Count; i++)
                    {
                        double[] positiveAxialPoint = new double[factors.Count];
                        for (int j = 0; j < factors.Count; j++)
                        {
                            positiveAxialPoint[j] = j == i ? (lowerLimits[i] + upperLimits[i]) / 2 + radius1 : (lowerLimits[j] + upperLimits[j]) / 2;
                        }
                        for (int j = 0; j < factors.Count; j++)
                        {
                            axialPoints[i, j] = positiveAxialPoint[j];
                        }

                        // 计算负轴方向星点（与正轴类似逻辑）
                        double[] negativeAxialPoint = new double[factors.Count];
                        for (int j = 0; j < factors.Count; j++)
                        {
                            negativeAxialPoint[j] = j == i ? (lowerLimits[i] + upperLimits[i]) / 2 - radius1 : (lowerLimits[j] + upperLimits[j]) / 2;
                        }
                        for (int j = 0; j < factors.Count; j++)
                        {
                            axialPoints[factors.Count + i, j] = negativeAxialPoint[j];
                        }
                    }
                    break;
            }
            return axialPoints;
        }

        public static double CalculateInscribedRadius(int factorCount, int centerPointRepeatCount = 0)
        {
            double alpha = Math.Pow(2, (factorCount - centerPointRepeatCount) / 4.0);
            return alpha;
        }

        // 计算中心复合 - 中心外接设计中的外切圆（球）半径
        public static double CalculateCircumscribedRadius(int k, int n_c, double[] xMin, double[] xMax)
        {
            int n_f = (int)Math.Pow(2, k);
            double F = k * (n_f - 1) + n_c;
            double alpha = Math.Pow(F, 0.25);
            double[] xCenter = new double[k];
            for (int i = 0; i < k; i++)
            {
                xCenter[i] = (xMin[i] + xMax[i]) / 2;
            }
            // 以一个顶点为例计算外切球半径（假设是高维情况，这里以二维思路类比）
            double vertexMaxX = xMax[0];
            double vertexMaxY = xMax[1];
            double R = Math.Sqrt((vertexMaxX - xCenter[0]) * (vertexMaxX - xCenter[0]) + (vertexMaxY - xCenter[1]) * (vertexMaxY - xCenter[1]));
            return R;
        }

        public static double[] CalculateCenterPoint(IList<OrthogonalParam> factors)
        {
            int factorCount = factors.Count;
            double[] centerPoint = new double[factorCount];
            for (int i = 0; i < factorCount; i++)
            {
                centerPoint[i] = (factors[i].Low + factors[i].High) / 2;
            }
            return centerPoint;
        }

        public static double[,] GetResult(double[,] factorialPoints, double[,] axialPoints, double[] centerPoints, int repeat)
        {
            int factorCount = factorialPoints.GetLength(0);
            int levelCount = factorialPoints.GetLength(1);
            int starPointsCount = axialPoints.GetLength(0);
            int centerPointsTotal = centerPoints.Length * repeat;

            int totalPoints = factorCount + starPointsCount + repeat;
            double[,] designMatrix = new double[totalPoints, levelCount];

            // 填充析因点到设计矩阵
            for (int i = 0; i < factorCount; i++)
            {
                for (int j = 0; j < levelCount; j++)
                {
                    designMatrix[i, j] = factorialPoints[i, j];
                }
            }

            // 填充星点到设计矩阵
            for (int i = 0; i < starPointsCount; i++)
            {
                for (int j = 0; j < levelCount; j++)
                {
                    designMatrix[factorCount + i, j] = axialPoints[i, j];
                }
            }

            for (int cpIndex = 0; cpIndex < repeat; cpIndex++)
            {
                for (int j = 0; j < levelCount; j++)
                {
                    designMatrix[factorCount + starPointsCount + cpIndex, j] = centerPoints[j];
                }
            }

            return designMatrix;
        }

        public static double[,] RemoveDuplicateRows(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            // 用于存储去重后的行索引
            List<int> uniqueRowIndexes = new List<int>();

            // 使用字典来辅助判断行是否重复，以行数据转为字符串作为键（也可考虑其他合适的方式来唯一标识行）
            Dictionary<string, bool> rowChecker = new Dictionary<string, bool>();

            for (int i = 0; i < rows; i++)
            {
                string rowData = "";
                for (int j = 0; j < cols; j++)
                {
                    rowData += matrix[i, j].ToString() + ",";
                }

                // 如果字典中不存在该键（即该行未出现过），则添加到去重行索引列表，并在字典中记录该键已存在
                if (!rowChecker.ContainsKey(rowData))
                {
                    uniqueRowIndexes.Add(i);
                    rowChecker.Add(rowData, true);
                }
            }

            // 根据去重后的行索引构建新的矩阵
            double[,] resultMatrix = new double[uniqueRowIndexes.Count, cols];
            for (int i = 0; i < uniqueRowIndexes.Count; i++)
            {
                int sourceRowIndex = uniqueRowIndexes[i];
                for (int j = 0; j < cols; j++)
                {
                    resultMatrix[i, j] = matrix[sourceRowIndex, j];
                }
            }

            return resultMatrix;
        }
    }
}
