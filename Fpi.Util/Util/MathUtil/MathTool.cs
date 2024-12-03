using System.Collections;
using System;
using System.Collections.Generic;

namespace Fpi.Util.MathUtil
{
    /// <summary>
    /// MathTool ��ժҪ˵����
    /// </summary>
    public class MathTool
    {
        private MathTool()
        {

        }

        /// <summary>
        /// ��Ԫ���Իع�(MLR)
        /// ��������:ͬʱ��X�����Y��������Ԫ���Իع�ģ��
        /// �ο�����:����ѧ����ѧ������ ��» ��ѧ�� ���� ��ѧ������  P170
        /// For Exmaple:
        /// X = [1,2,3,4;
        ///      1,1,1,1]
        /// Y = [3,5,7,9]; //���е���ʽ����
        /// --> b = [2 ,1];
        /// --> Y = bX
        /// </summary>
        /// <param name="order">��ϴ���</param>
        /// <param name="calX">X����</param>
        /// <param name="calY">Y����</param>
        /// <returns>b���ع鷽�̵�ϵ�� Y=b[0]*X+b[1]��					
        /// </returns>
        public static double[] MLR(int order, double[] calX, double[] calY)
        {
            if ((calX == null) || (calY == null) || calX.Length != calY.Length)
            {
                return null;
            }
            int cols = calX.Length;

            //����X����
            double[,] x = new double[order + 1,cols];
            for (int i = 0; i < order + 1; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    x[i, j] = System.Math.Pow(calX[j], order - i);
                }
            }
            Matrix X = new Matrix(x);

            //����X����
            Matrix Y = new Matrix(1, cols, calY);

            //��ع�ϵ��b
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
        /// ��˹���
        /// </summary>
        /// <param name="x">X����</param>
        /// <param name="y">Y����</param>
        /// <returns>����"����2��Ԫ�� 1:���λ�� 2:�ֱ���</returns>
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

            // ���߱����ĳ�ʼֵΪy����Сֵ - 0.1
            double bg = minY - 0.1;
            // ���Ϊy�����ֵ - ���߱���
            double amp = System.Math.Abs(maxY - minY);

            //�õ�ÿһ�е�ƽ��ֵ
            Matrix temp = Mean(xMatrix);
            double cen = temp[0, 0];

            //�ҳ��Ȱ��ߴ�ĵ�
            ArrayList ups = new ArrayList();
            for (int i = 0; i < y.Length; i++)
            {
                if (y[i] >= maxY/2)
                {
                    ups.Add(i);
                }
            }

            //����ĳ�ʼֵ
            double width;
            if (ups.Count > 3)
            {
                width = System.Math.Abs(x[maxIndex] - x[(int)ups[0]]);
            }
            else
            {
                width = System.Math.Abs(x[1] - x[0]);
            }

            //��������С�������
            Matrix p = new Matrix(4, 1, new double[4] {amp, cen, width, bg});
            p = NoLineFit(xMatrix, yMatrix, new ModelDelegate(Model), p);

            double[] result = new double[2];
            result[0] = p[1, 0]; //��ֵλ��
            result[1] = System.Math.Abs(p[2, 0]*2.3548); //�ֱ���	

            return result;
        }

        /// <summary>
        /// ��������С��������㷨
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

            //���� LM �㷨��ʼȨ��
            double lambda = 0.01;
            //���õ�������
            double sqrteps = System.Math.Sqrt(System.Math.Pow(2, -52));
            int n = p.RowsCount;

            Matrix yfit = modelfunc(x, p);

            Matrix r = y - yfit;
            double sse = ((Matrix) (r.Transpose()*r))[0, 0];

            //���õ�����ֹ�ĳ�ʼֵ
            Matrix zerosp = new Matrix(n, 1);
            zerosp.SetAllElementsValue(0); //��matlab����������� 0 ����

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
                //����������
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
                        p = beta_old + step; //matlab�и����Ϊ p=beta_old+step

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
        /// Ŀ��:������ݾ�����ÿһ�е�ƽ��ֵ��
        /// </summary>
        /// <param name="x">���ݾ���:[�� * ��]��</param>
        /// <returns>�������:[1 * ��]��</returns>
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
        /// Model����ί�� x:������Ա��� p:��Ҫ�Ż��Ĳ�����ʼֵ
        /// </summary>
        private delegate Matrix ModelDelegate(Matrix x, Matrix p);

        /// <summary>
        /// �㷨���ʽ�����滻��������ʽ
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
        /// ��ȡ�ſ˱�ֵ
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
                    //�ô�Ӧ��Ϊ  System.Math.Abs(p[i,0]) <= System.Math.Pow(2,-52))   
                {
                    double nb = System.Math.Sqrt(temp);

                    if (System.Math.Abs(nb) <= System.Math.Pow(2, -52))
                        //�ô�Ӧ��Ϊ  System.Math.Abs(nb) <= System.Math.Pow(2,-
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
        /// ��Ͼ���
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="isCombinateRow">true:��һ��,�����  false:��һ��,�����</param>
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
        /// �õ�����ĳ���
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
        /// ��˹���
        /// </summary>
        /// <param name="data">��ά����</param>
        /// <returns>1:���λ�� 2:�ֱ���</returns>

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
        /// ��׼ƫ��
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
        ///  Savitzky-Golay���ƽ����������
        /// Ŀ��:ʵ�ֶ��źŵ�ƽ�����󵼡�
        /// ԭ��:����Savitsky-Golay����ʽ��������ƽ������
        /// </summary>
        /// <param name="X">������� n ������ m ��ʾ�����㣬[�������� * ���׸���]</param>
        /// <param name="window">������ϵĴ��ڵĴ�С��ֻ��ȡ����</param>
        /// <param name="order">����ʽ��ϵ���ߴ���</param>
        /// <param name="deriv">�󵼵Ľ״Σ�0��ʾƽ����</param>
        /// <returns>Y  �󵼻�ƽ����õ��ľ�������</returns>
        /// �ο�����:
        /// "Generalized digital smoothing filters made easy by matrix calculations".
        /// Stephen E. Bialkowski, Analytical Chemistry, Vol.61, No.11, 1989
        public static Matrix Savgol(Matrix X, int window, int order, int deriv)
        {
            if (X == null)
                return null;
            //��һ��:���� matrix X�� int window,poly,order��һ������windowĬ��ֵ=7,polyĬ��ֵ=3,orderĬ��ֵ=0
            //�ڶ���:�������window�����жϣ���ֵֻ��Ϊ����ֵ�������׳��쳣
            //������:��poly �� order ��ֵ�����жϣ�max(poly)<15��min(poly)>3��
            //min(order)>0, max(order)<ploy-1 ,�����׳��쳣������ʾ��Ϣ
            //���Ĳ�:�Ծ�����о�����㣬���̼�����
            // ע�ⴰ�ڵĴ�С�������3�����ұ���������������������󣬶�����е�����
            window = Math.Max(3, 1 + 2 * ((window - 1) / 2));
            //ע����ϴ�������<=���ڿ��-1����С�� 5 ������������󣬶�����е�����
            order = Math.Min(order, 5);
            order = Math.Min(order, window - 1);
            //ע���󵼴�������<=��ϴ��� ������������󣬶�����е�����
            deriv = Math.Min(deriv, order);
            //������
            //p��ʾ�������ߵĿ��
            int p = (window - 1) / 2;
            //���һ�������������
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
                //����
                Matrix temp = x.Transpose() * x;
                if (!temp.InvertGaussJordan())
                {
                    throw new MatrixException("�����������");
                }
                Matrix weights = temp * x.Transpose();
                //�����ݽ���ƽ������
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
                //��β�ͽ����󵼺�ƽ������
                //����ʽģ��
                Matrix weightesTemp = new Matrix(window, 2);
                for (int i = 0; i < window; i++)
                {
                    weightesTemp[i, 0] = y[k, i];
                    weightesTemp[i, 1] = y[k, y.ColumnsCount - window + i];
                }
                weights = weights * weightesTemp;
                //deriv���󵼵Ľ��
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
                //������ǰ����β�ͽ������
                Matrix xTemp1 = x.GetSubMatrix(p + 1, 1 + order - deriv);
                Matrix yhatTemp1 = xTemp1 * weights.ReceiveColVector(0);
                //�����ݺ󼸵�β�ͽ������
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
            //���ؽ��
            Matrix result = new Matrix();
            result = yhat.Transpose();
            return result;
        }

        /// <summary>
        ///  Savitzky-Golay���ƽ����������
        /// Ŀ��:ʵ�ֶ��źŵ�ƽ�����󵼡�
        /// ԭ��:����Savitsky-Golay����ʽ��������ƽ������
        /// </summary>
        /// <param name="data">���� n ������ m ��ʾ�����㣬[�������� * ���׸���]</param>
        /// <param name="window">������ϵĴ��ڵĴ�С��ֻ��ȡ����</param>
        /// <param name="order">����ʽ��ϵ���ߴ���</param>
        /// <param name="deriv">�󵼵Ľ״Σ�0��ʾƽ����</param>
        /// <returns>Y  �󵼻�ƽ����õ�������</returns>
        /// �ο�����:
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
        /// Ѱ���㷨(����),��ȡ��ϸ�ķ�����
        /// </summary>
        /// <param name="data">Y��������</param>
        /// <param name="pointAmount">/���ݴ��ڴ�С</param>
        /// <param name="limen">��ֵ���������ڸ�ֵ��</param>
        /// <returns>��ϸ�ķ�����</returns>
        public static List<PeakInfo> SeekPeakDetail(double[] data, int pointAmount, float limen)
        {
            List<PeakInfo> list=new List<PeakInfo>();
            int searchFrom = 0; //�������������λ��
            //����б��
            double[] slopes = new double[data.Length];
            for (int i = 0; i < slopes.Length; i++)
            {
                slopes[i] = double.NaN;
            }

            //����ÿ�����ݵ��б��
            for (int i = searchFrom; i < slopes.Length; i++)
            {
                slopes[i] = ComputeDataWindowSlope(data, i);
            }  

            while (searchFrom < data.Length)
            {
                PeakInfo peakInfo=new PeakInfo();
                int startPoint,leftPoint, rightPoint, topPoint, endPoint;
                double peakArea = double.NaN;   //�����
                double peakHeight = double.NaN; //���
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
                            //�޸�
                            info.StartPoint = startPoint;
                            info.LeftPoint = leftPoint;
                            info.RightPoint = rightPoint;
                            info.EndPoint = endPoint;
                           
                        }
                    }
                    if (!hasData)
                    {
                        //����
                        list.Add(peakInfo);
                    }
                    
                }
                //����Ѱ����һ����
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
        /// Ѱ���㷨(����)
        /// </summary>
        /// <param name="data">Y��������</param>
        /// <param name="slopes">б��</param>
        /// <param name="peakLimit">��ֵ���������ڸ�ֵ��</param>
        /// <param name="dataWindowSize">���ݴ��ڴ�С</param>
        /// <param name="searchFrom">��Ѱ���</param>
        /// <param name="pointA">�����</param>
        /// <param name="pointB">��յ�</param>
        /// <param name="pointC">�ҹյ�</param>
        /// <param name="pointD">�嶥��</param>
        /// <param name="pointE">���յ�</param>
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
                //�������
                pointA = SearchStartPointA(slopes, peakLimit, dataWindowSize, searchFrom);
                if (pointA == -1)
                    return;
                //�����յ�
                pointB = SearchLeftSummitPointB(slopes, peakLimit, dataWindowSize, pointA + 1);
                if (pointB == -1)
                    return;
                //����ҹյ�
                pointC = SearchRightSummitPointC(slopes, peakLimit, dataWindowSize, pointB + 1);
                if (pointC == -1)
                    return;
                //�����յ�
                pointE = SearchEndPointE(slopes, peakLimit, dataWindowSize, pointC + 1);
                if (pointE == -1)
                    return;
                //����嶥��
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
                //����б�ʼ����10����
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

                //б��
                double slope;

                //��ֵ
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
            //�����ж�
            //0 < d0 < d1 < ... <dn-1
            //���ٴ���2��������di>pt

            double[] windowSlopes = new double[dataWindowSize];
            for (int i = searchFrom; i < slopes.Length; i++)
            {
                //����һ������
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
                    //�Ƿ�������������
                    if (windowSlopes[j] < 0)
                    {
                        hits = 0;
                        break;
                    }
                    //�Ƿ������������
                    if (j < windowSlopes.Length - 1 && windowSlopes[j] > windowSlopes[j + 1])
                    {
                        hits = 0;
                        break;
                    }
                    //�Ƿ���������2������ֵ������ֵ
                    if (j < windowSlopes.Length - 1 && windowSlopes[j] > peakLimit && windowSlopes[j + 1] > peakLimit)
                    {
                        hits++;
                    }
                }

                if (hits > 0)
                {
                    return i;  //�ҵ����
                }

                //���������ж�
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

                    //�Ƿ���������2������ֵ������ֵ
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

            return -1;  //û�ҵ�
        }

        private static int SearchLeftSummitPointB(double[] slopes, double peakLimit, int dataWindowSize, int searchFrom)
        {
            double[] windowSlopes = new double[dataWindowSize];
            for (int i = searchFrom; i < slopes.Length; i++)
            {
                //����һ������
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

                //�����ж�                 

                //�Ƿ��������εݼ�����
                bool flag = true;
                for (int j = 0; j < windowSlopes.Length - 1; j++)
                {
                    if (j < windowSlopes.Length - 1 && windowSlopes[j] < windowSlopes[j + 1])
                    {
                        flag = false;
                        break;
                    }
                }
                //����������εݼ�
                if (flag)
                {
                    if (peakLimit > windowSlopes[0])
                    {
                        return i;
                    }
                }

                //���������ж�
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

            return -1;  //û�ҵ�
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

            return -1;  //û�ҵ�
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
                //����һ������
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

                //�����ж�
                int hits = 0;
                for (int j = 0; j < windowSlopes.Length; j++)
                {
                    if (double.IsNaN(windowSlopes[j]))
                    {
                        hits = 0;
                        break;
                    }
                    //�Ƿ�������ڸ���ֵ����
                    if (windowSlopes[j] < (-1 * peakLimit))
                    {
                        hits = 0;
                        break;
                    }
                    //�Ƿ�����С������ֵ����
                    if (windowSlopes[j] > peakLimit)
                    {
                        hits = 0;
                        break;
                    }

                    //�Ƿ������������
                    if (j < windowSlopes.Length - 1 && windowSlopes[j] > windowSlopes[j + 1])
                    {
                        hits = 0;
                        break;
                    }

                    hits++;

                }

                if (hits == dataWindowSize)
                {
                    return (i + dataWindowSize - 1); //�ҵ�
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
                    return (i + dataWindowSize - 1); //�ҵ�
                }
            }

            return -1;  //û�ҵ�
        }
        


        /// <summary>
        /// ���μ�Ѱ���㷨
        /// </summary>
        /// <param name="y">Y��������</param>
        /// <param name="pointAmount">�������ҵĵ���,���û�ﵽ��������Ϊ�Ƿ�</param>
        /// <param name="limen">��ֵ���������ڸ�ֵ��</param>
        /// <returns>�嶥��������б�</returns>
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
        /// ���μ�Ѱ���㷨
        /// </summary>
        /// <param name="y">Y��������</param>
        /// <param name="pointAmount">�������ҵĵ���,���û�ﵽ��������Ϊ�Ƿ�</param>
        /// <returns>�嶥��������б�</returns>
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
        /// ���μ�Ѱ���ֵ�㷨
        /// </summary>
        /// <param name="y">Y��������</param>
        /// <param name="pointAmount">��ȵ����ҵĵ���,���û�ﵽ��������Ϊ�Ƿ��</param>
        /// <param name="limen">��ֵ</param>
        /// <returns>��ȶ���������б�</returns>
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
                //��ӷ�ֵ
                if (up >= pointAmount && down >= pointAmount && y[apexIndex] >= limen)
                {
                    up = 0;
                    down = 0;
                    peakIndexs.Add(apexIndex);
                }
                //��ӹ�ֵ
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
        /// ���μ�Ѱ���ֵ�㷨
        /// </summary>
        /// <param name="y">Y��������</param>
        /// <param name="pointAmount">��ȵ����ҵĵ���,���û�ﵽ��������Ϊ�Ƿ��</param>
        /// <returns>��ȶ���������б�</returns>
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
                //��ӷ�ֵ
                if (up >= pointAmount && down >= pointAmount )
                {
                    up = 0;
                    down = 0;
                    peakIndexs.Add(apexIndex);
                }
                //��ӹ�ֵ
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
        ///  ���Savitzky-Golay���ƽ����������
        /// </summary>
        /// <param name="data">���� n ������ m ��ʾ�����㣬[�������� * ���׸���]</param>
        /// <param name="window">������ϵĴ��ڵĴ�С��ֻ��ȡ����</param>
        /// <param name="order">����ʽ��ϵ���ߴ���</param>
        /// <param name="deriv">�󵼵Ľ״Σ�0��ʾƽ����</param>
        /// <param name="n">ƽ������</n>
        /// <returns>Y  �󵼻�ƽ����õ�������</returns>
        /// �ο�����:
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