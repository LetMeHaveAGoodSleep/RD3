using System;
using System.Collections.Generic;
using System.Text;

namespace Fpi.Util.MathUtil
{
    public static class MatlabFunc {
        /// <summary>
        /// �������ĳ���
        /// </summary>
        public static int Matlab_length(FpiVector fv) {
            return fv.Length;
        }

        /// <summary>
        /// �� ����ֵ
        /// </summary>
        /// <param name="fm">Ŀ�����</param>
        /// <returns>�� ����ֵ��ľ���</returns>
        public static FpiMatrix Matlab_abs(FpiMatrix fm) {
            FpiMatrix ret = new FpiMatrix(fm.RowsCount, fm.ColumnsCount);
            for (int r = 0; r < fm.RowsCount; r++) {
                for (int c = 0; c < fm.ColumnsCount; c++) {
                    ret.SetElement(r, c, System.Math.Abs(fm.GetElement(r, c)));
                }
            }
            return ret;
        }

        /// <summary>
        /// ����ȡ�͸���Ԫ�صĺ�
        /// </summary>
        /// <param name="fm">Ŀ�����</param>
        /// <returns>ȡ�ͺ�� ����</returns>
        public static FpiVector Matlab_sum(FpiMatrix fm) {
            return Matlab_sum(fm, 1);
        }

        /// <summary>
        /// ȡ�͸���Ԫ�صĺ�
        /// </summary>
        /// <param name="fm">Ŀ�����</param>
        /// <param name="i">i=1 ����ȡ��,i=2 ����ȡ��</param>
        /// <returns>ȡ�ͺ�� ����</returns>
        public static FpiVector Matlab_sum(FpiMatrix fm, int i) {
            if (i == 1) {
                FpiVector ret = new FpiVector(VectorType.RowVector, fm.ColumnsCount);
                for (int c = 1; c <= fm.ColumnsCount; c++) {
                    double rowvalue = 0;
                    for (int r = 1; r <= fm.RowsCount; r++) {
                        rowvalue = rowvalue + fm[r, c];
                    }
                    ret[c] = rowvalue;
                }
                return ret;
            }
            else if (i == 2) {
                FpiVector ret = new FpiVector(VectorType.ColVector, fm.RowsCount);
                for (int r = 1; r <= fm.RowsCount; r++) {
                    double colvalue = 0;
                    for (int c = 1; c <= fm.ColumnsCount; c++) {
                        colvalue = colvalue + fm[r, c];
                    }
                    ret[r] = colvalue;
                }
                return ret;
            }
            else {
                throw new Exception("����� i �д���, i Ӧ�õ���1��2��");
            }
        }

        /// <summary>
        /// ȡ������Ԫ�صĺ�
        /// </summary>
        /// <param name="fv">Ŀ������</param>
        /// <returns>��Ԫ�صĺ�</returns>
        public static double Matlab_sum(FpiVector fv) {
            double ret = 0;
            for (int t = 1; t <= fv.Length; t++) {
                ret = ret + fv[t];
            }
            return ret;
        }

        /// <summary>
        /// ������� num ������ 0 ��λ��
        /// </summary>
        public static FpiVector Matlab_findlast(FpiVector fv, int num) {
            FpiVector ret = new FpiVector(VectorType.RowVector, num);
            int findNum = 0;
            for (int t = fv.Length; t > 0; t--) {
                if ((bool)(fv[t] != 0) && findNum < num) {
                    findNum++;
                    ret[findNum] = t;
                }
            }
            return ret;
        }

        /// <summary>
        /// �Ծ���������� Ĭ�ϰ��н�����������
        /// </summary>
        /// <param name="fm">Ŀ�����</param>
        /// <returns></returns>
        public static FpiMatrix Matlab_sort(FpiMatrix fm) {
            return MatlabFunc.Matlab_sort(fm, 1, FuncSortType.ascend);
        }

        /// <summary>
        /// �Ծ���������� Ĭ�ϰ���������
        /// </summary>
        /// <param name="fm">Ŀ�����</param>
        /// <param name="i">i=1 ��������,i=2 ��������</param>
        /// <returns></returns>
        public static FpiMatrix Matlab_sort(FpiMatrix fm, int i) {
            return MatlabFunc.Matlab_sort(fm, i, FuncSortType.ascend);
        }

        /// <summary>
        /// �Ծ����������
        /// </summary>
        /// <param name="fm">Ŀ�����</param>
        /// <param name="i">i=1 ��������,i=2 ��������</param>
        /// <param name="st">�������� ascend:���� descend:���� </param>
        /// <returns></returns>
        public static FpiMatrix Matlab_sort(FpiMatrix fm, int i, FuncSortType st) {
            FpiMatrix ret = new FpiMatrix(fm.RowsCount, fm.ColumnsCount);
            if (i == 1) {//��������
                for (int c = 1; c <= fm.ColumnsCount; c++) {
                    FpiVector fv = (FpiVector)fm[FpiMatrix.FpiMatrixColon, c];
                    FpiVector fvsort = MatlabFunc.Matlab_sort(fv, st);
                    ret[FpiMatrix.FpiMatrixColon, c] = fvsort;
                }
                return ret;
            }
            else if (i == 2) {//��������
                for (int r = 1; r <= fm.RowsCount; r++) {
                    FpiVector fv = (FpiVector)fm[r, FpiMatrix.FpiMatrixColon];
                    FpiVector fvsort = MatlabFunc.Matlab_sort(fv, st);
                    ret[r, FpiMatrix.FpiMatrixColon] = fvsort;
                }
                return ret;
            }
            else {
                throw new Exception("����� i �д���, i Ӧ�õ���1��2��");
            }
        }

        /// <summary>
        /// ��������������
        /// </summary>
        /// <param name="fv">Ŀ������</param>
        /// <param name="st">�������� ascend:���� descend:���� </param>
        /// <returns>����������</returns>
        public static FpiVector Matlab_sort(FpiVector fv, FuncSortType st) {
            double[] d = (double[])fv;
            double[] sortd = MatlabFunc.sort(d, st);
            return new FpiVector(fv.VType, sortd);
        }

        /// <summary>
        /// �������������� Ĭ�ϰ���������
        /// </summary>
        /// <param name="fv">Ŀ������</param>
        /// <returns>����������</returns>
        public static FpiVector Matlab_sort(FpiVector fv) {
            return Matlab_sort(fv, FuncSortType.ascend);
        }

        /// <summary>
        /// ��������������
        /// </summary>
        /// <param name="fv">Ŀ������</param>
        /// <param name="st">�������� ascend:���� descend:����</param>
        /// <returns>����һ�����������飬FpiVector[0] ���к������ FpiVector[1] ���к�����Ԫ������Ӧ��λ��(�� 1 ��ʼ)</returns>
        public static FpiVector[] Matlab_sort2(FpiVector fv, FuncSortType st) {
            double[] d = (double[])fv;
            List<double> f1 = new List<double>();
            List<double> f2 = new List<double>();
            if (st == FuncSortType.ascend) {
                f1.Add(double.MinValue);
                f1.Add(double.MaxValue);
            }
            else {
                f1.Add(double.MaxValue);
                f1.Add(double.MinValue);
            }
            f2.Add(0);
            f2.Add(0);

            for (int t = 0; t < d.Length; t++) {
                for (int tt = 0; tt < f1.Count - 1; tt++) {
                    if (st == FuncSortType.ascend) {
                        if (f1[tt] <= d[t] && f1[tt + 1] >= d[t]) {
                            f1.Insert(tt + 1, d[t]);
                            f2.Insert(tt + 1, t + 1);
                            break;
                        }
                    }
                    else {
                        if (f1[tt] >= d[t] && f1[tt + 1] <= d[t]) {
                            f1.Insert(tt + 1, d[t]);
                            f2.Insert(tt + 1, t + 1);
                            break;
                        }
                    }
                }
            }
            f1.RemoveAt(0);
            f1.RemoveAt(f1.Count - 1);
            f2.RemoveAt(0);
            f2.RemoveAt(f2.Count - 1);
            FpiVector[] ret = new FpiVector[2];
            ret[0] = new FpiVector(fv.VType, f1.ToArray());
            ret[1] = new FpiVector(fv.VType, f2.ToArray());
            return ret;
        }

        /// <summary>
        /// �������������� Ĭ�ϰ���������
        /// </summary>
        /// <param name="fv">Ŀ������</param>
        /// <returns>����һ�����������飬FpiVector[0] ���к������ FpiVector[1] ���к�����Ԫ������Ӧ��λ��(�� 1 ��ʼ)</returns>
        public static FpiVector[] Matlab_sort2(FpiVector fv) {
            return Matlab_sort2(fv, FuncSortType.ascend);
        }

        /// <summary>
        /// �� double[] ��������
        /// </summary>
        /// <param name="d">double ����</param>
        /// <param name="f">��������</param>
        /// <returns>������ɺ������</returns>
        private static double[] sort(double[] d, FuncSortType f) {
            if (f == FuncSortType.ascend) {//����
                Array.Sort(d);
                return d;
            }
            else { //����
                Array.Sort(d);
                double[] ret = new double[d.Length];
                for (int l = 0; l < d.Length; l++) {
                    ret[l] = d[d.Length - 1 - l];
                }
                return ret;
            }
        }

        /// <summary>
        /// �ϲ��������� v1+v2
        /// </summary>
        public static FpiVector Matlab_uniteFpiVector(FpiVector v1, FpiVector v2) {
            if (v1.VType != v2.VType) {
                throw new Exception("�����������Ͳ�һ��,�޷��ϲ���");
            }
            FpiVector ret = new FpiVector(v1.VType, v1.Length + v2.Length);
            for (int t = 1; t <= v1.Length;t++ ) {
                ret[t] = v1[t];
            }
            for (int t = 1; t <= v2.Length; t++) {
                ret[t + v1.Length] = v2[t];
            }
            return ret;
        }

        /// <summary>
        /// �ϲ�һ�����ݺ�һ������ d+fv
        /// </summary>
        public static FpiVector Matlab_uniteFpiVector(double d, FpiVector fv) {
            FpiVector f = new FpiVector(fv.VType, new double[] { d });
            return MatlabFunc.Matlab_uniteFpiVector(f, fv);
        }

        /// <summary>
        /// �ϲ�һ�����ݺ�һ������ fv+d
        /// </summary>
        public static FpiVector Matlab_uniteFpiVector(FpiVector fv, double d) {
            FpiVector f = new FpiVector(fv.VType, new double[] { d });
            return MatlabFunc.Matlab_uniteFpiVector(fv, f);
        }

        /// <summary>
        /// �� ���� ���� ��������Ĳ���
        /// </summary>
        /// <param name="fm">Ŀ�����</param>
        /// <returns>��������Ĳ�����ľ���</returns>
        public static FpiMatrix Matlab_round(FpiMatrix fm) {
            FpiMatrix ret = new FpiMatrix(fm.RowsCount, fm.ColumnsCount);
            for (int r = 1; r <= fm.RowsCount; r++) {
                for (int c = 1; c <= fm.ColumnsCount; c++) {
                    ret[r, c] = System.Math.Round(fm[r, c], MidpointRounding.AwayFromZero);
                }
            }
            return ret;
        }

        /// <summary>
        ///  �� double ���ݽ��� ��������Ĳ���
        /// </summary>
        /// <param name="d">Ŀ����</param>
        /// <returns>������������</returns>
        public static double Matlab_round(double d) {
            return System.Math.Round(d, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// ���� ����������Ԫ��
        /// </summary>
        /// <param name="fv">Ŀ������</param>
        /// <returns>����Ԫ��</returns>
        public static double Matlab_max(FpiVector fv) {
            double ret = double.MinValue;
            for (int t = 1; t <= fv.Length; t++) {
                if (fv[t] > ret) {
                    ret = fv[t];
                }
            }
            return ret;
        }

        /// <summary>
        /// sort ���������
        /// </summary>
        public enum FuncSortType : int {
            /// <summary>
            /// ���� Ascending order (default)
            /// </summary>
            ascend = 1,
            /// <summary>
            /// ���� Descending order
            /// </summary>
            descend = 2,
        }
    }
}