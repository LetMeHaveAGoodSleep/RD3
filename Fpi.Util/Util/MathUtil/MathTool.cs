using System.Collections;
using System;
using System.Collections.Generic;

namespace Fpi.Util.MathUtil
{
    /// <summary>
    /// MathTool 的摘要说明。
    /// </summary>
    public class MathTool
    {
        private MathTool()
        {

        }

        /// <summary>
        /// 多元线性回归(MLR)
        /// 方法介绍:同时对X矩阵和Y矩阵建立多元线性回归模型
        /// 参考文献:＜化学计量学方法＞ 许禄 邵学广 主编 科学出版社  P170
        /// For Exmaple:
        /// X = [1,2,3,4;
        ///      1,1,1,1]
        /// Y = [3,5,7,9]; //以行的形式输入
        /// --> b = [2 ,1];
        /// --> Y = bX
        /// </summary>
        /// <param name="order">拟合次数</param>
        /// <param name="calX">X数据</param>
        /// <param name="calY">Y数据</param>
        /// <returns>b：回归方程的系数 Y=b[0]*X+b[1]；					
        /// </returns>
        public static double[] MLR(int order, double[] calX, double[] calY)
        {
            if ((calX == null) || (calY == null) || calX.Length != calY.Length)
            {
                return null;
            }
            int cols = calX.Length;

            //构造X矩阵
            double[,] x = new double[order + 1,cols];
            for (int i = 0; i < order + 1; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    x[i, j] = System.Math.Pow(calX[j], order - i);
                }
            }
            Matrix X = new Matrix(x);

            //构造X矩阵
            Matrix Y = new Matrix(1, cols, calY);

            //求回归系数b
            Matrix temp = X*(X.Transpose());
            temp.InvertGaussJordan();
            Matrix b = Y*(X.Transpose())*temp;

            double[] result = new double[b.ColumnsCount];
            for (int i = 0; i < b.ColumnsCount; i++)
            {
                result[i] = b[0, i];
            }
            return result;
        }


        /// <summary>
        /// 高斯拟合
        /// </summary>
        /// <param name="x">X数组</param>
        /// <param name="y">Y数组</param>
        /// <returns>数据"包含2个元素 1:峰的位置 2:分辨率</returns>
        public static double[] GaussianFit(double[] x, double[] y)
        {
            Matrix xMatrix = new Matrix(x.Length, 1, x);
            Matrix yMatrix = new Matrix(y.Length, 1, y);

            double minY = y[0];
            int minIndex = 0;
            double maxY = y[0];
            int maxIndex = 0;
            for (int i = 1; i < y.Length; i++)
            {
                if (y[i] < minY)
                {
                    minY = y[i];
                    minIndex = i;
                }
                if (y[i] > maxY)
                {
                    maxY = y[i];
                    maxIndex = i;
                }
            }

            // 基线背景的初始值为y的最小值 - 0.1
            double bg = minY - 0.1;
            // 峰高为y的最大值 - 基线背景
            double amp = System.Math.Abs(maxY - minY);

            //得到每一列的平均值
            Matrix temp = Mean(xMatrix);
            double cen = temp[0, 0];

            //找出比半峰高大的点
            ArrayList ups = new ArrayList();
            for (int i = 0; i < y.Length; i++)
            {
                if (y[i] >= maxY/2)
                {
                    ups.Add(i);
                }
            }

            //半峰宽的初始值
            double width;
            if (ups.Count > 3)
            {
                width = System.Math.Abs(x[maxIndex] - x[(int)ups[0]]);
            }
            else
            {
                width = System.Math.Abs(x[1] - x[0]);
            }

            //非线性最小二乘拟合
            Matrix p = new Matrix(4, 1, new double[4] {amp, cen, width, bg});
            p = NoLineFit(xMatrix, yMatrix, new ModelDelegate(Model), p);

            double[] result = new double[2];
            result[0] = p[1, 0]; //峰值位置
            result[1] = System.Math.Abs(p[2, 0]*2.3548); //分辨率	

            return result;
        }

        /// <summary>
        /// 非线性最小二乘拟合算法
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="amp"></param>
        /// <param name="cen"></param>
        /// <param name="width"></param>
        /// <param name="bg"></param>
        /// <returns></returns>
        private static Matrix NoLineFit(Matrix x, Matrix y, ModelDelegate modelfunc, Matrix p)
            //ref double amp,ref double cen,ref double width,ref double bg)
        {
            int maxIter = 1000;
            double tolFun = 1e-8;
            double tolX = 1e-8;
            double derivStep = System.Math.Pow(2, -52/3);

            //设置 LM 算法初始权重
            double lambda = 0.01;
            //设置迭代步长
            double sqrteps = System.Math.Sqrt(System.Math.Pow(2, -52));
            int n = p.RowsCount;

            Matrix yfit = modelfunc(x, p);

            Matrix r = y - yfit;
            double sse = ((Matrix) (r.Transpose()*r))[0, 0];

            //设置迭代中止的初始值
            Matrix zerosp = new Matrix(n, 1);
            zerosp.SetAllElementsValue(0); //在matlab程序中这个是 0 矩阵

            bool breakOut = false;
            //string cause = "";
            int iter = 0;
            while (iter < maxIter)
            {
                iter ++;
                Matrix beta_old = (Matrix) p.Clone();
                double sse_old = sse;
                Matrix J = GetJacobian(p, derivStep, modelfunc, x, yfit);
                Matrix diagJ = new Matrix(1, J.ColumnsCount);
                for (int col = 0; col < J.ColumnsCount; col++)
                {
                    double sum = 0;
                    for (int row = 0; row < J.RowsCount; row++)
                    {
                        sum += J[row, col]*J[row, col];
                    }
                    diagJ[0, col] = sum;
                }

                Matrix JPlusTemp = new Matrix(diagJ.ColumnsCount, diagJ.ColumnsCount);
                JPlusTemp.SetAllElementsValue(0);
                for (int col = 0; col < diagJ.ColumnsCount; col++)
                {
                    JPlusTemp[col, col] = System.Math.Sqrt(lambda*diagJ[0, col]);
                }


                Matrix JPlus = CombinateMatrix(J, JPlusTemp, true);
                Matrix RPlus = CombinateMatrix(r, zerosp, true);
                //求广义逆矩阵
                Matrix JPlusInvert = new Matrix();
                Matrix temp1 = new Matrix(), temp2 = new Matrix();
                JPlus.InvertUV(JPlusInvert, temp1, temp2, System.Math.Pow(2, -52));

                Matrix step = JPlusInvert*RPlus;
                p = p + step;

                yfit = modelfunc(x, p);
                r = y - yfit;
                sse = ((Matrix) (r.Transpose()*r))[0, 0];

                if (sse < sse_old)
                {
                    lambda = 0.1*lambda;
                }
                else
                {
                    while (sse > sse_old)
                    {
                        lambda = 10*lambda;
                        if (lambda > 1e+16)
                        {
                            breakOut = true;
                            break;
                        }
                        diagJ = new Matrix(1, J.ColumnsCount);
                        for (int col = 0; col < J.ColumnsCount; col++)
                        {
                            double sum = 0;
                            for (int row = 0; row < J.RowsCount; row++)
                            {
                                sum += J[row, col]*J[row, col];
                            }
                            diagJ[0, col] = sum;
                        }
                        JPlusTemp = new Matrix(diagJ.ColumnsCount, diagJ.ColumnsCount);
                        JPlusTemp.SetAllElementsValue(0);
                        for (int col = 0; col < diagJ.ColumnsCount; col++)
                        {
                            JPlusTemp[col, col] = System.Math.Sqrt(lambda*diagJ[0, col]);
                        }
                        JPlus = CombinateMatrix(J, JPlusTemp, true);
                        JPlus.InvertUV(JPlusInvert, temp1, temp2, System.Math.Pow(2, -52));
                        step = JPlusInvert*RPlus;
                        p = beta_old + step; //matlab中该语句为 p=beta_old+step

                        yfit = modelfunc(x, p);
                        r = y - yfit;
                        sse = ((Matrix) (r.Transpose()*r))[0, 0];
                    }
                }

                if (Norm(step) < tolX*(sqrteps + Norm(p)))
                {
                    //cause = "tolx";
                    break;
                }
                else if (System.Math.Abs(sse - sse_old) <= tolFun*sse)
                {
                    //cause = "tolfun";
                    break;
                }
                else if (breakOut)
                {
                    //cause = "stall";
                    break;
                }
            }
//			if(iter >= maxIter)
//			{
//				cause = "maxiter";
//			}
            return p;
        }


        /// <summary>
        /// 目标:求出数据矩阵中每一列的平均值。
        /// </summary>
        /// <param name="x">数据矩阵:[行 * 列]。</param>
        /// <returns>结果矩阵:[1 * 列]。</returns>
        private static Matrix Mean(Matrix x)
        {
            Matrix result = new Matrix(1, x.ColumnsCount);

            double tempSum = 0;

            for (int col = 0; col < x.ColumnsCount; col++)
            {
                tempSum = 0;
                for (int row = 0; row < x.RowsCount; row++)
                {
                    tempSum += x[row, col];
                }
                result[0, col] = tempSum/x.RowsCount;
            }
            return result;
        }

        /// <summary>
        /// Model函数委托 x:输入的自变量 p:需要优化的参数初始值
        /// </summary>
        private delegate Matrix ModelDelegate(Matrix x, Matrix p);

        /// <summary>
        /// 算法表达式，可替换成其它形式
        /// </summary>
        /// <param name="x"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private static Matrix Model(Matrix x, Matrix p)
        {
            Matrix yhat = new Matrix(x.RowsCount, 1);
            for (int i = 0; i < x.RowsCount; i++)
            {
                yhat[i, 0] = p[3, 0] + p[0, 0]*System.Math.Exp(-0.5*System.Math.Pow((x[i, 0] - p[1, 0])/p[2, 0], 2));
            }
            return yhat;
        }


        /// <summary>
        /// 获取雅克比值
        /// </summary>
        /// <param name="p"></param>
        /// <param name="fdiffstep"></param>
        /// <param name="X"></param>
        /// <param name="yfit"></param>
        /// <returns></returns>
        private static Matrix GetJacobian(Matrix p, double fdiffstep, ModelDelegate modelfunc, Matrix X, Matrix yfit)
        {
            int num = p.RowsCount;
            Matrix delta = new Matrix(num, 1);
            delta.SetAllElementsValue(0);

            Matrix J = new Matrix(X.RowsCount, num);
            double temp = 0;
            for (int i = 0; i < num; i++)
            {
                temp += p[i, 0]*p[i, 0];
            }
            temp = System.Math.Sqrt(temp);
            for (int i = 0; i < num; i++)
            {
                if (System.Math.Abs(p[i, 0]) <= System.Math.Pow(2, -52))
                    //该处应该为  System.Math.Abs(p[i,0]) <= System.Math.Pow(2,-52))   
                {
                    double nb = System.Math.Sqrt(temp);

                    if (System.Math.Abs(nb) <= System.Math.Pow(2, -52))
                        //该处应该为  System.Math.Abs(nb) <= System.Math.Pow(2,-
                    {
                        delta[i, 0] = fdiffstep*(nb + 1);
                    }
                    else
                    {
                        delta[i, 0] = fdiffstep*(nb + 0);
                    }
                }
                else
                {
                    delta[i, 0] = fdiffstep*p[i, 0];
                }
                Matrix yplus = modelfunc(X, delta + p);
                Matrix dy = yplus - yfit;
                Matrix j = dy*(1/delta[i, 0]);

                for (int k = 0; k < X.RowsCount; k++)
                {
                    J[k, i] = j[k, 0];
                }

                delta[i, 0] = 0;
            }

            return J;
        }

        /// <summary>
        /// 组合矩阵
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="isCombinateRow">true:列一致,组合行  false:行一致,组合列</param>
        /// <returns></returns>
        private static Matrix CombinateMatrix(Matrix a, Matrix b, bool isCombinateRow)
        {
            if (isCombinateRow)
            {
                if (a.ColumnsCount != b.ColumnsCount)
                {
                    return null;
                }
                Matrix result = new Matrix(a.RowsCount + b.RowsCount, a.ColumnsCount);
                for (int row = 0; row < result.RowsCount; row++)
                {
                    for (int col = 0; col < result.ColumnsCount; col++)
                    {
                        if (row < a.RowsCount)
                        {
                            result[row, col] = a[row, col];
                        }
                        else
                        {
                            result[row, col] = b[row - a.RowsCount, col];
                        }
                    }
                }
                return result;
            }
            else
            {
                if (a.RowsCount != b.RowsCount)
                {
                    return null;
                }
                Matrix result = new Matrix(a.RowsCount, a.ColumnsCount + b.ColumnsCount);
                for (int col = 0; col < result.ColumnsCount; col++)
                {
                    for (int row = 0; row < result.ColumnsCount; row++)
                    {
                        if (col < a.ColumnsCount)
                        {
                            result[row, col] = a[row, col];
                        }
                        else
                        {
                            result[row, col] = b[row, col - a.ColumnsCount];
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 得到矩阵的长度
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double Norm(Matrix x)
        {
            double result = 0;
            for (int row = 0; row < x.RowsCount; row++)
            {
                for (int col = 0; col < x.ColumnsCount; col++)
                {
                    result += x[row, col]*x[row, col];
                }
            }
            result = System.Math.Sqrt(result);
            return result;
        }
        /// <summary>
        /// 高斯拟合
        /// </summary>
        /// <param name="data">二维数据</param>
        /// <returns>1:峰的位置 2:分辨率</returns>

        public static double[] GaussianFit(double[,] data)
        {
            int count = data.GetLength(0);
            double[] x = new double[count];
            double[] y = new double[count];
            for (int i = 0; i < count; i++)
            {
                x[i] = data[i, 0];
                y[i] = data[i, 1];
            }
            return GaussianFit(x, y);
        }

        /// <summary>
        /// 标准偏差
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static double Warp(double[] arr)
        {
            if (arr.Length == 1)
                return arr[0];
            double avg = Avg(arr);
            double sum2 = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                sum2 += System.Math.Pow(arr[i] - avg, 2);
            }
            return System.Math.Sqrt(sum2/(arr.Length - 1));
        }

        public static double Sum(double[] arr)
        {
            double sum = 0d;
            for (int i = 0; i < arr.Length; i++)
            {
                sum += arr[i];
            }
            return sum;
        }

        public static double Avg(double[] arr)
        {
            return Sum(arr)/arr.Length;
        }

        public static double Min(double[] arr)
        {
            double min = double.MaxValue;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] < min)
                    min = arr[i];
            }
            return min;
        }

        public static double Max(double[] arr)
        {
            double max = double.MinValue;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > max)
                    max = arr[i];
            }
            return max;
        }

        /// <summary>
        ///  Savitzky-Golay卷积平滑与求导运算
        /// 目标:实现对信号的平滑和求导。
        /// 原理:利用Savitsky-Golay多项式方法进行平滑和求导
        /// </summary>
        /// <param name="X">输入矩阵 n 个样本 m 表示波长点，[波长点数 * 波谱个数]</param>
        /// <param name="window">用于拟合的窗口的大小，只能取奇数</param>
        /// <param name="order">多项式拟合的最高次数</param>
        /// <param name="deriv">求导的阶次（0表示平滑）</param>
        /// <returns>Y  求导或平滑后得到的矩阵数据</returns>
        /// 参考文献:
        /// "Generalized digital smoothing filters made easy by matrix calculations".
        /// Stephen E. Bialkowski, Analytical Chemistry, Vol.61, No.11, 1989
        public static Matrix Savgol(Matrix X, int window, int order, int deriv)
        {
            if (X == null)
                return null;
            //第一步:输入 matrix X； int window,poly,order，一般设置window默认值=7,poly默认值=3,order默认值=0
            //第二步:对输入的window进行判断，其值只能为奇数值，否则抛出异常
            //第三步:对poly 和 order 数值进行判断，max(poly)<15，min(poly)>3；
            //min(order)>0, max(order)<ploy-1 ,否则抛出异常给出提示信息
            //第四步:对矩阵进行具体计算，过程见程序
            // 注意窗口的大小必须大于3，而且必须是奇数，如果输入有误，对其进行调整。
            window = Math.Max(3, 1 + 2 * ((window - 1) / 2));
            //注意拟合次数必须<=窗口宽度-1，并小于 5 ，如果输入有误，对其进行调整。
            order = Math.Min(order, 5);
            order = Math.Min(order, window - 1);
            //注意求导次数必须<=拟合次数 ，如果输入有误，对其进行调整。
            deriv = Math.Min(deriv, order);
            //计算结果
            //p表示左右两边的宽度
            int p = (window - 1) / 2;
            //设计一个矩阵并求广义逆
            Matrix x = new Matrix(window, 1 + order);
            int pTemp = -p;
            for (int i = 0; i < x.RowsCount; i++)
            {
                for (int j = 0; j < x.ColumnsCount; j++)
                {
                    x[i, j] = Math.Pow(pTemp, j);
                }
                pTemp++;
            }
            Matrix y = X.Transpose();
            Matrix yhat = new Matrix(X.ColumnsCount, X.RowsCount);
            for (int k = 0; k < y.RowsCount; k++)
            {
                //求逆
                Matrix temp = x.Transpose() * x;
                if (!temp.InvertGaussJordan())
                {
                    throw new MatrixException("矩阵求逆出错！");
                }
                Matrix weights = temp * x.Transpose();
                //对数据进行平滑和求导
                for (int i = p + 1; i < y.ColumnsCount - p - 1; i++)
                {
                    Matrix yTemp = new Matrix(2 * p + 1, 1);
                    for (int j = 0; j < yTemp.RowsCount; j++)
                    {
                        yTemp[j, 0] = y[k, j + i - p];
                    }
                    Matrix yhatTemp = weights.ReceiveRowVector(deriv) * yTemp;
                    yhat[k, i] = yhatTemp[0, 0];
                }
                //对尾巴进行求导和平滑运算
                //多项式模型
                Matrix weightesTemp = new Matrix(window, 2);
                for (int i = 0; i < window; i++)
                {
                    weightesTemp[i, 0] = y[k, i];
                    weightesTemp[i, 1] = y[k, y.ColumnsCount - window + i];
                }
                weights = weights * weightesTemp;
                //deriv次求导的结果
                for (int i = 0; i < deriv; i++)
                {
                    Matrix diag = new Matrix(order - i, order - i);
                    for (int j = 0; j < diag.RowsCount; j++)
                    {
                        diag[j, j] = j + 1;
                    }
                    Matrix weightTemp2 = new Matrix(order - i, weights.ColumnsCount);
                    for (int j = 0; j < weightTemp2.RowsCount; j++)
                    {
                        for (int l = 0; l < weightTemp2.ColumnsCount; l++)
                        {
                            weightTemp2[j, l] = weights[1 + j, l];
                        }
                    }
                    weights = diag * weightTemp2;
                }
                //对数据前几点尾巴进行拟合
                Matrix xTemp1 = x.GetSubMatrix(p + 1, 1 + order - deriv);
                Matrix yhatTemp1 = xTemp1 * weights.ReceiveColVector(0);
                //对数据后几点尾巴进行拟合
                Matrix xTemp2 = new Matrix(window - p, 1 + order - deriv);
                for (int i = 0; i < xTemp2.RowsCount; i++)
                {
                    for (int j = 0; j < xTemp2.ColumnsCount; j++)
                    {
                        xTemp2[i, j] = x[p + i, j];
                    }
                }
                Matrix yhatTemp2 = xTemp2 * weights.ReceiveColVector(1);
                for (int i = 0; i < p + 1; i++)
                {
                    yhat[k, i] = yhatTemp1[i, 0];
                    yhat[k, y.ColumnsCount - p + i - 1] = yhatTemp2[i, 0];
                }
            }
            //返回结果
            Matrix result = new Matrix();
            result = yhat.Transpose();
            return result;
        }

        /// <summary>
        ///  Savitzky-Golay卷积平滑与求导运算
        /// 目标:实现对信号的平滑和求导。
        /// 原理:利用Savitsky-Golay多项式方法进行平滑和求导
        /// </summary>
        /// <param name="data">输入 n 个样本 m 表示波长点，[波长点数 * 波谱个数]</param>
        /// <param name="window">用于拟合的窗口的大小，只能取奇数</param>
        /// <param name="order">多项式拟合的最高次数</param>
        /// <param name="deriv">求导的阶次（0表示平滑）</param>
        /// <returns>Y  求导或平滑后得到的数据</returns>
        /// 参考文献:
        /// "Generalized digital smoothing filters made easy by matrix calculations".
        /// Stephen E. Bialkowski, Analytical Chemistry, Vol.61, No.11, 1989
        public static double[] Savgol(double[] data, int window, int order, int deriv)
        {
            if (data == null)
                return null;
            Matrix X = new Matrix(data.Length, 1, data);
            Matrix result = Savgol(X, window, order, deriv);
            return result.GetData();
        }

        /// <summary>
        /// 寻峰算法(烟气),获取详细的峰数据
        /// </summary>
        /// <param name="data">Y坐标数组</param>
        /// <param name="pointAmount">/数据窗口大小</param>
        /// <param name="limen">阀值（峰必须大于该值）</param>
        /// <returns>详细的峰数据</returns>
        public static List<PeakInfo> SeekPeakDetail(double[] data, int pointAmount, float limen)
        {
            List<PeakInfo> list=new List<PeakInfo>();
            int searchFrom = 0; //特征点搜索起点位置
            //计算斜率
            double[] slopes = new double[data.Length];
            for (int i = 0; i < slopes.Length; i++)
            {
                slopes[i] = double.NaN;
            }

            //计算每个数据点的斜率
            for (int i = searchFrom; i < slopes.Length; i++)
            {
                slopes[i] = ComputeDataWindowSlope(data, i);
            }  

            while (searchFrom < data.Length)
            {
                PeakInfo peakInfo=new PeakInfo();
                int startPoint,leftPoint, rightPoint, topPoint, endPoint;
                double peakArea = double.NaN;   //峰面积
                double peakHeight = double.NaN; //峰高
                SearchFeaturePoints(data, slopes, limen, pointAmount, searchFrom, out startPoint, out leftPoint,
                   out rightPoint, out topPoint, out endPoint);
                peakInfo.StartPoint=startPoint;
                peakInfo.LeftPoint=leftPoint;
                peakInfo.RightPoint = rightPoint;
                peakInfo.TopPoint = topPoint;
                peakInfo.EndPoint = endPoint;

                if (startPoint != -1 && leftPoint != -1 && rightPoint != -1 && topPoint != -1 && endPoint != -1)
                {
                    bool hasData = false;
                    foreach (PeakInfo info in list)
                    {
                        if ( info.TopPoint == topPoint)
                        {
                            hasData = true;
                            //修改
                            info.StartPoint = startPoint;
                            info.LeftPoint = leftPoint;
                            info.RightPoint = rightPoint;
                            info.EndPoint = endPoint;
                           
                        }
                    }
                    if (!hasData)
                    {
                        //新增
                        list.Add(peakInfo);
                    }
                    
                }
                //继续寻找下一个峰
                if (endPoint != -1)
                {
                    searchFrom = searchFrom + 1;
                }
                else
                {
                    searchFrom++;
                }
            }
            return list;
        }


      

        /// <summary>
        /// 寻峰算法(烟气)
        /// </summary>
        /// <param name="data">Y坐标数组</param>
        /// <param name="slopes">斜率</param>
        /// <param name="peakLimit">阀值（峰必须大于该值）</param>
        /// <param name="dataWindowSize">数据窗口大小</param>
        /// <param name="searchFrom">搜寻起点</param>
        /// <param name="pointA">峰起点</param>
        /// <param name="pointB">左拐点</param>
        /// <param name="pointC">右拐点</param>
        /// <param name="pointD">峰顶点</param>
        /// <param name="pointE">峰终点</param>
        /// <returns></returns>
        private static void SearchFeaturePoints(double[] data, double[] slopes, double peakLimit, int dataWindowSize, int searchFrom, out int pointA, out int pointB, out int pointC, out int pointD, out int pointE)
        {
            pointA = -1;
            pointB = -1;
            pointC = -1;
            pointD = -1;
            pointE = -1;

            try
            {
                //检测峰起点
                pointA = SearchStartPointA(slopes, peakLimit, dataWindowSize, searchFrom);
                if (pointA == -1)
                    return;
                //检测左拐点
                pointB = SearchLeftSummitPointB(slopes, peakLimit, dataWindowSize, pointA + 1);
                if (pointB == -1)
                    return;
                //检测右拐点
                pointC = SearchRightSummitPointC(slopes, peakLimit, dataWindowSize, pointB + 1);
                if (pointC == -1)
                    return;
                //检测峰终点
                pointE = SearchEndPointE(slopes, peakLimit, dataWindowSize, pointC + 1);
                if (pointE == -1)
                    return;
                //计算峰顶点
                pointD = SearchSummitPointD(data, pointB, pointC);
            }
            catch
            {
            }
        }


        private  static double ComputeDataWindowSlope(double[] data, int index)
        {
            try
            {
                //参与斜率计算的10个数
                double x1;
                double x2;
                double x3;
                double x4;
                double x5;
                double x6;
                double x7;
                double x8;
                double x9;
                double x10;

                //斜率
                double slope;

                //赋值
                x1 = data[index + 4];
                x2 = data[index + 3];
                x3 = data[index + 2];
                x4 = data[index + 1];
                x5 = data[index];
                x6 = data[index - 1];
                x7 = data[index - 2];
                x8 = data[index - 3];
                x9 = data[index - 4];
                x10 = data[index - 5];

                slope = 9 * x1 + 7 * x2 + 5 * x3 + 3 * x4 + x5 - x6 - 3 * x7 - 5 * x8 - 7 * x9 - 9 * x10;

                return slope;
            }
            catch
            {
                return double.NaN;
            }
        }

        private static int SearchStartPointA(double[] slopes, double peakLimit, int dataWindowSize, int searchFrom)
        {
            //特征判断
            //0 < d0 < d1 < ... <dn-1
            //至少存在2个连续的di>pt

            double[] windowSlopes = new double[dataWindowSize];
            for (int i = searchFrom; i < slopes.Length; i++)
            {
                //填满一个窗口
                for (int j = 0; j < windowSlopes.Length; j++)
                {
                    try
                    {
                        windowSlopes[j] = slopes[i + j];
                    }
                    catch
                    {
                        windowSlopes[j] = double.NaN;
                    }
                }

                int hits = 0;
                for (int j = 0; j < windowSlopes.Length; j++)
                {
                    if (double.IsNaN(windowSlopes[j]))
                    {
                        hits = 0;
                        break;
                    }
                    //是否满足正数条件
                    if (windowSlopes[j] < 0)
                    {
                        hits = 0;
                        break;
                    }
                    //是否满足递增条件
                    if (j < windowSlopes.Length - 1 && windowSlopes[j] > windowSlopes[j + 1])
                    {
                        hits = 0;
                        break;
                    }
                    //是否满足至少2个连续值超过阀值
                    if (j < windowSlopes.Length - 1 && windowSlopes[j] > peakLimit && windowSlopes[j + 1] > peakLimit)
                    {
                        hits++;
                    }
                }

                if (hits > 0)
                {
                    return i;  //找到起点
                }

                //总体趋势判断
                double P = 0; double dmax = 0; double B = 0;
                hits = 0;
                for (int k = 0; k < windowSlopes.Length; k++)
                {
                    if (double.IsNaN(windowSlopes[k]))
                        break;
                    double d = windowSlopes[k];
                    if (d > dmax)
                    {
                        B++;
                        P = P + B;
                        dmax = d;
                    }
                    else
                    {
                        B = 0;
                    }

                    //是否满足至少2个连续值超过阀值
                    if (k < windowSlopes.Length - 1 && windowSlopes[k] > peakLimit && windowSlopes[k + 1] > peakLimit)
                    {
                        hits++;
                    }
                }
                if (P >= peakLimit && hits > 0)
                {
                    return i;
                }
            }

            return -1;  //没找到
        }

        private static int SearchLeftSummitPointB(double[] slopes, double peakLimit, int dataWindowSize, int searchFrom)
        {
            double[] windowSlopes = new double[dataWindowSize];
            for (int i = searchFrom; i < slopes.Length; i++)
            {
                //填满一个窗口
                for (int j = 0; j < windowSlopes.Length; j++)
                {
                    try
                    {
                        windowSlopes[j] = slopes[i + j];
                    }
                    catch
                    {
                        windowSlopes[j] = double.NaN;
                    }
                }

                //特征判断                 

                //是否满足依次递减条件
                bool flag = true;
                for (int j = 0; j < windowSlopes.Length - 1; j++)
                {
                    if (j < windowSlopes.Length - 1 && windowSlopes[j] < windowSlopes[j + 1])
                    {
                        flag = false;
                        break;
                    }
                }
                //如果满足依次递减
                if (flag)
                {
                    if (peakLimit > windowSlopes[0])
                    {
                        return i;
                    }
                }

                //总体趋势判断
                double P = 0; double dmax = peakLimit; double B = 0;
                for (int j = 0; j < windowSlopes.Length; j++)
                {
                    if (double.IsNaN(windowSlopes[j]))
                        break;
                    double d = windowSlopes[j];
                    if (d < dmax)
                    {
                        B++;
                        P = P + B;
                        dmax = d;
                    }
                    else
                    {
                        B = 0;
                    }
                }
                if (P >= peakLimit && peakLimit > windowSlopes[0])
                {
                    return i;
                }

            }

            return -1;  //没找到
        }

        private static int SearchRightSummitPointC(double[] slopes, double peakLimit, int dataWindowSize, int searchFrom)
        {
            if (slopes[searchFrom] > slopes[searchFrom - 1])
                return -1;
            double[] windowSlopes = new double[dataWindowSize];
            for (int i = searchFrom; i < slopes.Length; i++)
            {
                if (double.IsNaN(slopes[i]) == false && slopes[i] < (-1 * peakLimit))
                    return i;
            }

            return -1;  //没找到
        }

        private static int SearchSummitPointD(double[] data, int startPosition, int endPosition)
        {
            double maxvalue = data[startPosition];
            int maxindex = startPosition;
            for (int i = startPosition + 1; i < endPosition; i++)
            {
                if (data[i] > maxvalue)
                {
                    maxvalue = data[i];
                    maxindex = i;
                }
            }

            return maxindex;
        }

        public static int SearchEndPointE(double[] slopes, double peakLimit, int dataWindowSize, int searchFrom)
        {
            double[] windowSlopes = new double[dataWindowSize];
            for (int i = searchFrom; i < slopes.Length; i++)
            {
                if (slopes[i] < -1.0)
                {
                    continue;
                }
                //填满一个窗口
                for (int j = 0; j < windowSlopes.Length; j++)
                {
                    try
                    {
                        windowSlopes[j] = slopes[i + j];
                    }
                    catch
                    {
                        windowSlopes[j] = double.NaN;
                    }
                }

                //特征判断
                int hits = 0;
                for (int j = 0; j < windowSlopes.Length; j++)
                {
                    if (double.IsNaN(windowSlopes[j]))
                    {
                        hits = 0;
                        break;
                    }
                    //是否满足大于负阀值条件
                    if (windowSlopes[j] < (-1 * peakLimit))
                    {
                        hits = 0;
                        break;
                    }
                    //是否满足小于正阀值条件
                    if (windowSlopes[j] > peakLimit)
                    {
                        hits = 0;
                        break;
                    }

                    //是否满足递增条件
                    if (j < windowSlopes.Length - 1 && windowSlopes[j] > windowSlopes[j + 1])
                    {
                        hits = 0;
                        break;
                    }

                    hits++;

                }

                if (hits == dataWindowSize)
                {
                    return (i + dataWindowSize - 1); //找到
                }

                double P = 0; double dmax1 = (-1 * peakLimit); double dmax2 = peakLimit; double B = 0;

                for (int k = 0; k < windowSlopes.Length; k++)
                {
                    if (double.IsNaN(windowSlopes[k]))
                        break;
                    double d = windowSlopes[k];
                    if (d > dmax1 && d < dmax2)
                    {
                        B++;
                        P = P + B;
                        dmax1 = d;
                    }
                    else
                    {
                        B = 0;
                    }
                }

                if (P >= peakLimit)
                {
                    return (i + dataWindowSize - 1); //找到
                }
            }

            return -1;  //没找到
        }
        


        /// <summary>
        /// 单次简单寻峰算法
        /// </summary>
        /// <param name="y">Y坐标数组</param>
        /// <param name="pointAmount">顶点左右的点数,如果没达到点数不认为是峰</param>
        /// <param name="limen">阀值（峰必须大于该值）</param>
        /// <returns>峰顶点的索引列表</returns>
        public static int[] SeekPeak(double[] y, int pointAmount, float limen)
        {
            if (y == null || y.Length < 3)
            {
                return null;
            }
            List<int> peakIndexs = new List<int>();
            int apexIndex = 0;
            int up = 0;
            int down = 0;
            int len = y.Length;
            for (int i = 1; i < len; i++)
            {
                if (y[i] > y[i - 1])
                {
                    if (down > 0)
                    {
                        up = 0;
                        down = 0;
                    }
                    up++;
                    apexIndex = i;
                }
                else if (y[i] < y[i - 1])
                {
                    if (up > 0)
                    {
                        down++;
                    }
                }
                if (up >= pointAmount && down >= pointAmount && y[apexIndex] >= limen)
                {
                    up = 0;
                    down = 0;
                    peakIndexs.Add(apexIndex);
                }
            }
            return peakIndexs.ToArray();
        }

        /// <summary>
        /// 单次简单寻峰算法
        /// </summary>
        /// <param name="y">Y坐标数组</param>
        /// <param name="pointAmount">顶点左右的点数,如果没达到点数不认为是峰</param>
        /// <returns>峰顶点的索引列表</returns>
        public static int[] SeekPeak(double[] y, int pointAmount)
        {
            if (y == null || y.Length < 3)
            {
                return null;
            }
            List<int> peakIndexs = new List<int>();
            int apexIndex = 0;
            int up = 0;
            int down = 0;
            int len = y.Length;
            for (int i = 1; i < len; i++)
            {
                if (y[i] > y[i - 1])
                {
                    if (down > 0)
                    {
                        up = 0;
                        down = 0;
                    }
                    up++;
                    apexIndex = i;
                }
                else if (y[i] < y[i - 1])
                {
                    if (up > 0)
                    {
                        down++;
                    }
                }
                if (up >= pointAmount && down >= pointAmount )
                {
                    up = 0;
                    down = 0;
                    peakIndexs.Add(apexIndex);
                }
            }
            return peakIndexs.ToArray();
        }



        /// <summary>
        /// 单次简单寻峰谷值算法
        /// </summary>
        /// <param name="y">Y坐标数组</param>
        /// <param name="pointAmount">峰谷点左右的点数,如果没达到点数不认为是峰谷</param>
        /// <param name="limen">阀值</param>
        /// <returns>峰谷顶点的索引列表</returns>
        public static int[] SeekPeakValley(double[] y, int pointAmount, float limen)
        {
            if (y == null || y.Length < 3)
            {
                return null;
            }
            List<int> peakIndexs = new List<int>();
            int apexIndex = 0;
            int apexIndey = 0;
            int up2 = 0;
            int down2 = 0;
            int up = 0;
            int down = 0;
            int len = y.Length;
            for (int i = 1; i < len; i++)
            {
                if (y[i] > y[i - 1])
                {
                    if (down > 0)
                    {
                        up = 0;
                        down = 0;
                    }
                    if (down2 > 0)
                    {
                        up2++;
                    }
                    up++;
                    apexIndex = i;
                }
                else if (y[i] < y[i - 1])
                {
                    if (up > 0)
                    {
                        down++;
                    }
                    if (up2 > 0)
                    {
                        up2 = 0;
                        down2 = 0;
                    }
                    down2++;
                    apexIndey = i;

                }
                //添加峰值
                if (up >= pointAmount && down >= pointAmount && y[apexIndex] >= limen)
                {
                    up = 0;
                    down = 0;
                    peakIndexs.Add(apexIndex);
                }
                //添加谷值
                if (up2 >= pointAmount && down2 >= pointAmount && y[apexIndey] >= limen)
                {
                    up2 = 0;
                    down2 = 0;
                    peakIndexs.Add(apexIndey);
                }
            }
            return peakIndexs.ToArray();
        }

        /// <summary>
        /// 单次简单寻峰谷值算法
        /// </summary>
        /// <param name="y">Y坐标数组</param>
        /// <param name="pointAmount">峰谷点左右的点数,如果没达到点数不认为是峰谷</param>
        /// <returns>峰谷顶点的索引列表</returns>
        public static int[] SeekPeakValley(double[] y, int pointAmount)
        {
            if (y == null || y.Length < 3)
            {
                return null;
            }
            List<int> peakIndexs = new List<int>();
            int apexIndex = 0;
            int apexIndey = 0;
            int up2 = 0;
            int down2 = 0;
            int up = 0;
            int down = 0;
            int len = y.Length;
            for (int i = 1; i < len; i++)
            {
                if (y[i] > y[i - 1])
                {
                    if (down > 0)
                    {
                        up = 0;
                        down = 0;
                    }
                    if (down2 > 0)
                    {
                        up2++;
                    }
                    up++;
                    apexIndex = i;
                }
                else if (y[i] < y[i - 1])
                {
                    if (up > 0)
                    {
                        down++;
                    }
                    if (up2 > 0)
                    {
                        up2 = 0;
                        down2 = 0;
                    }
                    down2++;
                    apexIndey = i;

                }
                //添加峰值
                if (up >= pointAmount && down >= pointAmount )
                {
                    up = 0;
                    down = 0;
                    peakIndexs.Add(apexIndex);
                }
                //添加谷值
                if (up2 >= pointAmount && down2 >= pointAmount)
                {
                    up2 = 0;
                    down2 = 0;
                    peakIndexs.Add(apexIndey);
                }
            }
            return peakIndexs.ToArray();
        }

        /// <summary>
        ///  多次Savitzky-Golay卷积平滑与求导运算
        /// </summary>
        /// <param name="data">输入 n 个样本 m 表示波长点，[波长点数 * 波谱个数]</param>
        /// <param name="window">用于拟合的窗口的大小，只能取奇数</param>
        /// <param name="order">多项式拟合的最高次数</param>
        /// <param name="deriv">求导的阶次（0表示平滑）</param>
        /// <param name="n">平滑次数</n>
        /// <returns>Y  求导或平滑后得到的数据</returns>
        /// 参考文献:
        /// "Generalized digital smoothing filters made easy by matrix calculations".
        /// Stephen E. Bialkowski, Analytical Chemistry, Vol.61, No.11, 1989
        public static double[] Savgol(double[] data, int window, int order, int deriv, int n)
        {
            double[] result = null;
            if (n < 1)
            {
                if (data == null)
                    return null;
                result = MathTool.Savgol(data, window, order, deriv);
            }
            else
            {
                if (data == null)
                    return null;
                result = data;
                for (int i = 0; i < n; i++)
                {
                    result = MathTool.Savgol(result, window, order, deriv);
                }
            }
            return result;
        }


    }
}