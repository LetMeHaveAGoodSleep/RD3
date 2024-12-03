using System;
using System.Collections.Generic;
using System.Text;

namespace Fpi.Util.MathUtil
{
    public static class MatlabFunc {
        /// <summary>
        /// 求向量的长度
        /// </summary>
        public static int Matlab_length(FpiVector fv) {
            return fv.Length;
        }

        /// <summary>
        /// 求 绝对值
        /// </summary>
        /// <param name="fm">目标矩阵</param>
        /// <returns>求 绝对值后的矩阵</returns>
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
        /// 按列取和各个元素的和
        /// </summary>
        /// <param name="fm">目标矩阵</param>
        /// <returns>取和后的 向量</returns>
        public static FpiVector Matlab_sum(FpiMatrix fm) {
            return Matlab_sum(fm, 1);
        }

        /// <summary>
        /// 取和各个元素的和
        /// </summary>
        /// <param name="fm">目标矩阵</param>
        /// <param name="i">i=1 按列取和,i=2 按行取和</param>
        /// <returns>取和后的 向量</returns>
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
                throw new Exception("输入的 i 有错误, i 应该等于1或2！");
            }
        }

        /// <summary>
        /// 取向量各元素的和
        /// </summary>
        /// <param name="fv">目标向量</param>
        /// <returns>各元素的和</returns>
        public static double Matlab_sum(FpiVector fv) {
            double ret = 0;
            for (int t = 1; t <= fv.Length; t++) {
                ret = ret + fv[t];
            }
            return ret;
        }

        /// <summary>
        /// 查找最后 num 个不是 0 的位置
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
        /// 对矩阵进行排列 默认按列进行升序排序
        /// </summary>
        /// <param name="fm">目标矩阵</param>
        /// <returns></returns>
        public static FpiMatrix Matlab_sort(FpiMatrix fm) {
            return MatlabFunc.Matlab_sort(fm, 1, FuncSortType.ascend);
        }

        /// <summary>
        /// 对矩阵进行排列 默认按升序排序
        /// </summary>
        /// <param name="fm">目标矩阵</param>
        /// <param name="i">i=1 按列排序,i=2 按行排序</param>
        /// <returns></returns>
        public static FpiMatrix Matlab_sort(FpiMatrix fm, int i) {
            return MatlabFunc.Matlab_sort(fm, i, FuncSortType.ascend);
        }

        /// <summary>
        /// 对矩阵进行排列
        /// </summary>
        /// <param name="fm">目标矩阵</param>
        /// <param name="i">i=1 按列排序,i=2 按行排序</param>
        /// <param name="st">排序类型 ascend:升序 descend:降序 </param>
        /// <returns></returns>
        public static FpiMatrix Matlab_sort(FpiMatrix fm, int i, FuncSortType st) {
            FpiMatrix ret = new FpiMatrix(fm.RowsCount, fm.ColumnsCount);
            if (i == 1) {//按列排序
                for (int c = 1; c <= fm.ColumnsCount; c++) {
                    FpiVector fv = (FpiVector)fm[FpiMatrix.FpiMatrixColon, c];
                    FpiVector fvsort = MatlabFunc.Matlab_sort(fv, st);
                    ret[FpiMatrix.FpiMatrixColon, c] = fvsort;
                }
                return ret;
            }
            else if (i == 2) {//按行排序
                for (int r = 1; r <= fm.RowsCount; r++) {
                    FpiVector fv = (FpiVector)fm[r, FpiMatrix.FpiMatrixColon];
                    FpiVector fvsort = MatlabFunc.Matlab_sort(fv, st);
                    ret[r, FpiMatrix.FpiMatrixColon] = fvsort;
                }
                return ret;
            }
            else {
                throw new Exception("输入的 i 有错误, i 应该等于1或2！");
            }
        }

        /// <summary>
        /// 对向量进行排序
        /// </summary>
        /// <param name="fv">目标向量</param>
        /// <param name="st">排序类型 ascend:升序 descend:降序 </param>
        /// <returns>排序后的向量</returns>
        public static FpiVector Matlab_sort(FpiVector fv, FuncSortType st) {
            double[] d = (double[])fv;
            double[] sortd = MatlabFunc.sort(d, st);
            return new FpiVector(fv.VType, sortd);
        }

        /// <summary>
        /// 对向量进行排序 默认按升序排序
        /// </summary>
        /// <param name="fv">目标向量</param>
        /// <returns>排序后的向量</returns>
        public static FpiVector Matlab_sort(FpiVector fv) {
            return Matlab_sort(fv, FuncSortType.ascend);
        }

        /// <summary>
        /// 对向量进行排序
        /// </summary>
        /// <param name="fv">目标向量</param>
        /// <param name="st">排序类型 ascend:升序 descend:降序</param>
        /// <returns>返回一个向量的数组，FpiVector[0] 排列后的向量 FpiVector[1] 排列后向量元素所对应的位子(从 1 开始)</returns>
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
        /// 对向量进行排序 默认按升序排序
        /// </summary>
        /// <param name="fv">目标向量</param>
        /// <returns>返回一个向量的数组，FpiVector[0] 排列后的向量 FpiVector[1] 排列后向量元素所对应的位子(从 1 开始)</returns>
        public static FpiVector[] Matlab_sort2(FpiVector fv) {
            return Matlab_sort2(fv, FuncSortType.ascend);
        }

        /// <summary>
        /// 对 double[] 进行排序
        /// </summary>
        /// <param name="d">double 数组</param>
        /// <param name="f">排序类型</param>
        /// <returns>排序完成后的数据</returns>
        private static double[] sort(double[] d, FuncSortType f) {
            if (f == FuncSortType.ascend) {//升序
                Array.Sort(d);
                return d;
            }
            else { //降序
                Array.Sort(d);
                double[] ret = new double[d.Length];
                for (int l = 0; l < d.Length; l++) {
                    ret[l] = d[d.Length - 1 - l];
                }
                return ret;
            }
        }

        /// <summary>
        /// 合并两个向量 v1+v2
        /// </summary>
        public static FpiVector Matlab_uniteFpiVector(FpiVector v1, FpiVector v2) {
            if (v1.VType != v2.VType) {
                throw new Exception("两个向量类型不一致,无法合并！");
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
        /// 合并一个数据和一个向量 d+fv
        /// </summary>
        public static FpiVector Matlab_uniteFpiVector(double d, FpiVector fv) {
            FpiVector f = new FpiVector(fv.VType, new double[] { d });
            return MatlabFunc.Matlab_uniteFpiVector(f, fv);
        }

        /// <summary>
        /// 合并一个数据和一个向量 fv+d
        /// </summary>
        public static FpiVector Matlab_uniteFpiVector(FpiVector fv, double d) {
            FpiVector f = new FpiVector(fv.VType, new double[] { d });
            return MatlabFunc.Matlab_uniteFpiVector(fv, f);
        }

        /// <summary>
        /// 对 矩阵 进行 四舍五入的操作
        /// </summary>
        /// <param name="fm">目标矩阵</param>
        /// <returns>四舍五入的操作后的矩阵</returns>
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
        ///  对 double 数据进行 四舍五入的操作
        /// </summary>
        /// <param name="d">目标数</param>
        /// <returns>四舍五入后的数</returns>
        public static double Matlab_round(double d) {
            return System.Math.Round(d, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 返回 向量中最大的元素
        /// </summary>
        /// <param name="fv">目标向量</param>
        /// <returns>最大的元素</returns>
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
        /// sort 排序的类型
        /// </summary>
        public enum FuncSortType : int {
            /// <summary>
            /// 升序 Ascending order (default)
            /// </summary>
            ascend = 1,
            /// <summary>
            /// 降序 Descending order
            /// </summary>
            descend = 2,
        }
    }
}