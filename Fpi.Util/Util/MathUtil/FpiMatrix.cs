using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;


namespace Fpi.Util.MathUtil
{
    /// <summary>
    /// 矩阵类 继承Matlab
    /// </summary>
    public class FpiMatrix : Matrix {

        public static string FpiMatrixColon = ":";

        #region 构造函数
        public FpiMatrix()
            : base() { }

        /// <summary>
        /// 指定行列构造函数
        /// </summary>
        /// <param name="nRows">指定的矩阵行数</param>
        /// <param name="nCols">指定的矩阵列数</param>
        public FpiMatrix(int nRows, int nCols)
            : base(nRows, nCols) { }

        /// <summary>
        /// 指定值构造函数
        /// </summary>
        /// <param name="value">二维数组，存储矩阵各元素的值</param>
        public FpiMatrix(double[,] value)
            : base(value) { }

        /// <summary>
        /// 指定值构造函数
        /// </summary>
        /// <param name="nRows">指定的矩阵行数</param>
        /// <param name="nCols">指定的矩阵列数</param>
        /// <param name="value">一维数组，长度为nRows*nCols，存储矩阵各元素的值</param>
        public FpiMatrix(int nRows, int nCols, double[] value)
            : base(nRows, nCols, value) { }


        /// <summary>
        /// 方阵构造函数
        /// </summary>
        /// <param name="nSize">方阵行列数</param>
        public FpiMatrix(int nSize)
            : base(nSize) { }

        /// <summary>
        /// 方阵构造函数
        /// </summary>
        /// <param name="nSize">方阵行列数</param>
        /// <param name="value">一维数组，长度为nRows*nRows，存储方阵各元素的值</param>
        public FpiMatrix(int nSize, double[] value)
            : base(nSize, value) { }

        /// <summary>
        /// 拷贝构造函数
        /// </summary>
        /// <param name="other">源矩阵</param>
        public FpiMatrix(Matrix other)
            : base(other) {
        }

        public FpiMatrix(FpiMatrix other)
            : base((Matrix)other) { }
        #endregion

        #region []
        /// <summary>
        /// 行列编号 均从 1 开始 和Matlab一致
        /// </summary>
        public new double this[int row, int col] {
            get {
                if (row > 0 && row <= RowsCount && col > 0 && col <= ColumnsCount) {
                    return base[row - 1, col - 1];
                }
                else {
                    throw new Exception("上下限超出！");
                }
            }
            set {
                if (row > RowsCount || col > ColumnsCount) {
                    int rr = RowsCount;
                    int cc = ColumnsCount;
                    if (row > RowsCount) {
                        rr = row;
                    }
                    if (col > ColumnsCount) {
                        cc = col;
                    }

                    FpiMatrix fm = new FpiMatrix(rr, cc);
                    for (int r = 0; r < RowsCount; r++) {
                        for (int c = 0; c < ColumnsCount; c++) {
                            fm.SetElement(r, c, base[r, c]);
                        }
                    }
                    base.Init(rr, cc);
                    elements = (double[])fm.elements.Clone();
                }
                base[row - 1, col - 1] = value;
            }
        }

        /// <summary>
        /// 编号从 1 开始 和Matlab一致
        /// </summary>
        public FpiMatrix this[string colon, int col] {
            get {
                if (colon == FpiMatrix.FpiMatrixColon) {
                    double[] data = new double[RowsCount];
                    for (int r = 0; r < RowsCount; r++) {
                        data[r] = base[r, col - 1];
                    }
                    return new FpiMatrix(RowsCount, 1, data);
                }
                else {
                    throw new Exception("输入出错！");
                }
            }
            set {
                if (colon == FpiMatrix.FpiMatrixColon) {
                    if (value.ColumnsCount != 1 || (value.RowsCount != this.RowsCount)) {
                        throw new Exception("输入出错！");
                    }
                    for (int r = 1; r <= value.RowsCount; r++) {
                        this[r, col] = value[r, 1];
                    }
                }
                else {
                    throw new Exception("输入出错！");
                }
            }
        }

        /// <summary>
        /// 编号从 1 开始 和Matlab一致
        /// </summary>
        public FpiMatrix this[string colon, int colStart, int colEnd] {
            get {
                if (colon == FpiMatrix.FpiMatrixColon) {
                    double[,] data = new double[RowsCount, colEnd - colStart + 1];
                    for (int r = 0; r < RowsCount; r++) {
                        for (int c = colStart; c <= colEnd; c++) {
                            data[r, c - colStart] = base[r, c - 1];
                        }
                    }
                    return new FpiMatrix(data);
                }
                else {
                    throw new Exception("输入出错！");
                }
            }
            set {
                if (colon == FpiMatrix.FpiMatrixColon) {
                    if (value.ColumnsCount != (colEnd - colStart + 1) || (value.RowsCount != this.RowsCount)) {
                        throw new Exception("输入出错！");
                    }
                    for (int r = 1; r <= value.RowsCount; r++) {
                        for (int c = colStart; c <= colEnd; c++) {
                            this[r, c] = value[r, c - colStart + 1];
                        }
                    }
                }
                else {
                    throw new Exception("输入出错！");
                }
            }
        }

        /// <summary>
        /// 编号从 1 开始 和Matlab一致
        /// </summary>
        public FpiMatrix this[int row, string colon] {
            get {
                if (colon == FpiMatrix.FpiMatrixColon) {
                    double[] data = new double[ColumnsCount];
                    for (int c = 0; c < ColumnsCount; c++) {
                        data[c] = base[row - 1, c];
                    }
                    return new FpiMatrix(1, ColumnsCount, data);
                }
                else {
                    throw new Exception("输入出错！");
                }
            }
            set {
                if (colon == FpiMatrix.FpiMatrixColon) {
                    if (value.RowsCount != 1 || (value.ColumnsCount != this.ColumnsCount)) {
                        throw new Exception("输入出错！");
                    }
                    for (int c = 1; c <= this.ColumnsCount; c++) {
                        this[row, c] = value[1, c];
                    }
                }
                else {
                    throw new Exception("输入出错！");
                }
            }
        }

        /// <summary>
        /// 编号从 1 开始 和Matlab一致
        /// </summary>
        public FpiMatrix this[int rowStart, int rowEnd, string colon] {
            get {
                if (colon == FpiMatrix.FpiMatrixColon) {
                    double[,] data = new double[rowEnd - rowStart + 1, ColumnsCount];
                    for (int r = rowStart; r <= rowEnd; r++) {
                        for (int c = 0; c < ColumnsCount; c++) {
                            data[r - rowStart, c] = base[r - 1, c];
                        }
                    }
                    return new FpiMatrix(data);
                }
                else {
                    throw new Exception("输入出错！");
                }
            }
            set {
                if (colon == FpiMatrix.FpiMatrixColon) {
                    if (value.RowsCount != (rowEnd - rowStart + 1) || (value.ColumnsCount != this.ColumnsCount)) {
                        throw new Exception("输入出错！");
                    }
                    for (int r = rowStart; r <= rowEnd; r++) {
                        for (int c = 1; c <= this.ColumnsCount; c++) {
                            this[r, c] = value[r - rowStart + 1, c];
                        }
                    }
                }
                else {
                    throw new Exception("输入出错！");
                }
            }
        }

        /// <summary>
        /// 编号从 1 开始 和Matlab一致
        /// </summary>
        public FpiMatrix this[int rowStart, int rowEnd, int colStart, int colEnd] {
            get {
                double[,] data = new double[rowEnd - rowStart + 1, colEnd - colStart + 1];
                for (int r = rowStart; r <= rowEnd; r++) {
                    for (int c = colStart; c <= colEnd; c++) {
                        data[r - rowStart, c - colStart] = base[r - 1, c - 1];
                    }
                }
                return new FpiMatrix(data);
            }
            set {
                if (value.RowsCount != (rowEnd - rowStart + 1) || value.ColumnsCount != (colEnd - colStart + 1)) {
                    throw new Exception("输入出错！");
                }
                for (int r = rowStart; r <= rowEnd; r++) {
                    for (int c = colStart; c <= colEnd; c++) {
                        this[r, c] = value[r - rowStart + 1, c - colStart + 1];
                    }
                }
            }
        }

        public FpiVector this[LogicFpiMatrix lfm] {
            get {
                List<double> dd = new List<double>();
                if (lfm.RowsCount > this.RowsCount || lfm.ColumnsCount > this.ColumnsCount) {
                    throw new Exception("逻辑矩阵的行数或列数大于目标的行数或列数,无法取值！");
                }
                for (int r = 1; r <= lfm.RowsCount; r++) {
                    for (int c = 1; c <= lfm.ColumnsCount; c++) {
                        if (lfm[r, c] == 1) {
                            dd.Add(this[r, c]);
                        }
                    }
                }
                return new FpiVector(VectorType.ColVector, dd.ToArray());
            }
        }
        #endregion

        #region 类型转换
        /// <summary>
        /// 类型转换
        /// </summary>
        public static explicit operator FpiVector(FpiMatrix fm) {//强制类型转换
            if (fm.RowsCount == 1) { //行向量
                return new FpiVector(VectorType.RowVector, fm.elements);
            }
            else if (fm.ColumnsCount == 1) {//列向量
                return new FpiVector(VectorType.ColVector, fm.elements);
            }
            else {
                throw new Exception("无法转换,原距阵的行或列不为 1 ！");
            }
        }

        ///// <summary>
        ///// 强制类型转换 查看是否有含0的元素 有则返回 False 没有则返回 True
        ///// </summary>
        //public static explicit operator bool(FpiMatrix fm) {
        //    for (int r = 1; r <= fm.RowsCount; r++) {
        //        for (int c = 1; c <= fm.ColumnsCount; c++) {
        //            if (fm[r, c] == 0) {
        //                return false;
        //            }
        //        }
        //    }
        //    return true;
        //}

        #endregion

        #region 操作符 重载
        public static LogicFpiMatrix operator >(FpiMatrix fm, double d) {
            LogicFpiMatrix ret = new LogicFpiMatrix(fm.RowsCount, fm.ColumnsCount);
            for (int r = 1; r <= fm.RowsCount; r++) {
                for (int c = 1; c <= fm.ColumnsCount; c++) {
                    if (fm[r, c] > d)
                        ret[r, c] = 1;
                    else
                        ret[r, c] = 0;
                }
            }
            return ret;
        }
        public static LogicFpiMatrix operator >(double d, FpiMatrix fm) {
            return (fm < d);
        }
        public static LogicFpiMatrix operator >=(FpiMatrix fm, double d) {
            LogicFpiMatrix ret = new LogicFpiMatrix(fm.RowsCount, fm.ColumnsCount);
            for (int r = 1; r <= fm.RowsCount; r++) {
                for (int c = 1; c <= fm.ColumnsCount; c++) {
                    if (fm[r, c] >= d)
                        ret[r, c] = 1;
                    else
                        ret[r, c] = 0;
                }
            }
            return ret;
        }
        public static LogicFpiMatrix operator >=(double d, FpiMatrix fm) {
            return (fm <= d);
        }
        public static LogicFpiMatrix operator ==(FpiMatrix fm, double d) {
            LogicFpiMatrix ret = new LogicFpiMatrix(fm.RowsCount, fm.ColumnsCount);
            for (int r = 1; r <= fm.RowsCount; r++) {
                for (int c = 1; c <= fm.ColumnsCount; c++) {
                    if (fm[r, c] == d)
                        ret[r, c] = 1;
                    else
                        ret[r, c] = 0;
                }
            }
            return ret;
        }
        public static LogicFpiMatrix operator ==(double d, FpiMatrix fm) {
            return (fm == d);
        }
        public static LogicFpiMatrix operator !=(FpiMatrix fm, double d) {
            LogicFpiMatrix ret = new LogicFpiMatrix(fm.RowsCount, fm.ColumnsCount);
            for (int r = 1; r <= fm.RowsCount; r++) {
                for (int c = 1; c <= fm.ColumnsCount; c++) {
                    if (fm[r, c] != d)
                        ret[r, c] = 1;
                    else
                        ret[r, c] = 0;
                }
            }
            return ret;
        }
        public static LogicFpiMatrix operator !=(double d, FpiMatrix fm) {
            return (fm != d);
        }
        public static LogicFpiMatrix operator <(FpiMatrix fm, double d) {
            LogicFpiMatrix ret = new LogicFpiMatrix(fm.RowsCount, fm.ColumnsCount);
            for (int r = 1; r <= fm.RowsCount; r++) {
                for (int c = 1; c <= fm.ColumnsCount; c++) {
                    if (fm[r, c] < d)
                        ret[r, c] = 1;
                    else
                        ret[r, c] = 0;
                }
            }
            return ret;
        }
        public static LogicFpiMatrix operator <(double d, FpiMatrix fm) {
            return (fm > d);
        }
        public static LogicFpiMatrix operator <=(FpiMatrix fm, double d) {
            LogicFpiMatrix ret = new LogicFpiMatrix(fm.RowsCount, fm.ColumnsCount);
            for (int r = 1; r <= fm.RowsCount; r++) {
                for (int c = 1; c <= fm.ColumnsCount; c++) {
                    if (fm[r, c] <= d)
                        ret[r, c] = 1;
                    else
                        ret[r, c] = 0;
                }
            }
            return ret;
        }
        public static LogicFpiMatrix operator <=(double d, FpiMatrix fm) {
            return (fm >= d);
        }
        public static LogicFpiMatrix operator &(FpiMatrix fm1, FpiMatrix fm2) {
            if (fm1.RowsCount != fm2.RowsCount || fm2.ColumnsCount != fm1.ColumnsCount) {
                throw new Exception("操作符左右两边矩阵的行列必须一致！");
            }
            LogicFpiMatrix ret = new LogicFpiMatrix(fm1.RowsCount, fm1.ColumnsCount);
            for (int r = 1; r <= fm1.RowsCount; r++) {
                for (int c = 1; c <= fm1.ColumnsCount; c++) {
                    if (fm1[r, c] == 0 || fm2[r, c] == 0)
                        ret[r, c] = 0;
                    else
                        ret[r, c] = 1;
                }
            }
            return ret;
        }
        #endregion

        /// <summary>
        /// 删除对应的行
        /// </summary>
        /// <param name="fvDel">包含所要删除的行的数</param>
        public void RemoveRow(FpiVector fvDel) {
            
        }

        /// <summary>
        /// 删除对应列
        /// </summary>
        /// <param name="fvDel">包含所要删除的列的数</param>
        public void RemoveCol(FpiVector fvDel) {
            
        }

        /// <summary>
        /// 删除对应的行
        /// </summary>
        /// <param name="RowNum">行号</param>
        public void RemoveRow(int RowNum) {

        }

        /// <summary>
        /// 删除对应列
        /// </summary>
        /// <param name="ColNum">列号</param>
        public void RemoveCol(int ColNum) {
            
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object other) {
            return base.Equals(other);
        }
    }

    /// <summary>
    /// 向量类 
    /// </summary>
    public class FpiVector {
        protected double[] elements = new double[] { };
        protected VectorType vType;
        /// <summary>
        /// 向量类型
        /// </summary>
        public VectorType VType {
            get { return vType; }
            set { vType = value; }
        }

        #region 构造函数
        /// <summary>
        /// 无数据的行向量
        /// </summary>
        public FpiVector() {
            vType = VectorType.RowVector;
            elements = new double[] { };
        }
        public FpiVector(VectorType vType) {
            this.vType = vType;
            elements = new double[] { };
        }
        public FpiVector(VectorType vType, double[] elements) {
            this.vType = vType;
            this.elements = elements;
        }
        public FpiVector(VectorType vType, int size) {
            this.vType = vType;
            this.elements = new double[size];
        }
        #endregion

        #region 类型转换
        /// <summary>
        /// 类型转换
        /// </summary>
        public static implicit operator FpiMatrix(FpiVector fv) {
            if (fv.vType == VectorType.RowVector) //行向量
                return new FpiMatrix(1, fv.elements.Length, fv.elements);
            else//列向量
                return new FpiMatrix(fv.elements.Length, 1, fv.elements);
        }

        public static implicit operator double[](FpiVector fv) {
            double[] ret = new double[fv.elements.Length];
            for (int t = 0; t < fv.elements.Length; t++) {
                ret[t] = fv.elements[t];
            }
            return ret;
        }

        //public static explicit operator bool(FpiVector fv) {
        //    //return (bool)((FpiMatrix)fv);
        //}
        #endregion

        /// <summary>
        /// 编号从 1 开始
        /// </summary>
        public double this[int num] {
            get {
                if (num > elements.Length) {
                    throw new Exception("上限超出!");
                }
                else if (num < 1) {
                    throw new Exception("下限超出!");
                }
                return elements[num - 1];
            }
            set {
                if (num < 1) {
                    throw new Exception("下限超出!");
                }
                if (num > elements.Length) {
                    double[] e = new double[num];
                    for (int t = 0; t < elements.Length; t++)
                        e[t] = elements[t];
                    elements = e;
                }
                elements[num - 1] = value;
            }
        }

        /// <summary>
        /// 分别存放数据位子的向量
        /// </summary>
        public FpiVector this[FpiVector StationNums] {
            get {
                FpiVector ret = new FpiVector(this.VType, StationNums.Length);
                for (int t = 1; t <= StationNums.Length; t++) {
                    if (StationNums[t] <= this.Length) {
                        ret[t] = this[(int)StationNums[t]];
                    }
                    else {
                        throw new Exception("输入位置有错误！");
                    }
                }
                return ret;
            }
        }

        public FpiVector this[LogicFpiVector lfv] {
            get {
                if (lfv.Length > this.Length) {
                    throw new Exception("逻辑向量的长度大于向量的长度,无法取值！");
                }
                List<double> d = new List<double>();
                for (int t = 1; t <= lfv.Length; t++) {
                    if (lfv[t] == 1) {
                        d.Add(this[t]);
                    }
                }
                return new FpiVector(this.vType, d.ToArray());
            }
        }

        /// <summary>
        /// 编号从 1 开始
        /// </summary>
        public FpiVector this[int numStr, int numEnd] {
            get {
                if (numEnd >= numStr && numEnd <= this.Length) {
                    FpiVector ret = new FpiVector(this.vType, numEnd - numStr + 1);
                    for (int t = 0; t < numEnd - numStr + 1; t++) {
                        ret.elements[t] = this.elements[t + numStr - 1];
                    }
                    return ret;
                }
                else {
                    throw new Exception("上下限超出！");
                }
            }
        }

        public int Length {
            get { return elements.Length; }
        }

        #region 操作符 重载
        public static LogicFpiVector operator >(FpiVector fv, double d) {
            List<double> ds = new List<double>();
            for (int t = 1; t <= fv.Length; t++) {
                if (fv[t] > d)
                    ds.Add(1);
                else
                    ds.Add(0);
            }
            return new LogicFpiVector(fv.vType, ds.ToArray());
        }
        public static LogicFpiVector operator >(double d, FpiVector fv) {
            return (fv < d);
        }
        public static LogicFpiVector operator >=(FpiVector fv, double d) {
            List<double> ds = new List<double>();
            for (int t = 1; t <= fv.Length; t++) {
                if (fv[t] >= d)
                    ds.Add(1);
                else
                    ds.Add(0);
            }
            return new LogicFpiVector(fv.vType, ds.ToArray());
        }
        public static LogicFpiVector operator >=(double d, FpiVector fv) {
            return (fv <= d);
        }
        public static LogicFpiVector operator ==(FpiVector fv, double d) {
            List<double> ds = new List<double>();
            for (int t = 1; t <= fv.Length; t++) {
                if (fv[t] == d)
                    ds.Add(1);
                else
                    ds.Add(0);
            }
            return new LogicFpiVector(fv.vType, ds.ToArray());
        }
        public static LogicFpiVector operator ==(double d, FpiVector fv) {
            return (fv == d);
        }
        public static LogicFpiVector operator !=(FpiVector fv, double d) {
            List<double> ds = new List<double>();
            for (int t = 1; t <= fv.Length; t++) {
                if (fv[t] != d)
                    ds.Add(1);
                else
                    ds.Add(0);
            }
            return new LogicFpiVector(fv.vType, ds.ToArray());
        }
        public static LogicFpiVector operator !=(double d, FpiVector fv) {
            return (fv != d);
        }
        public static LogicFpiVector operator <(FpiVector fv, double d) {
            List<double> ds = new List<double>();
            for (int t = 1; t <= fv.Length; t++) {
                if (fv[t] < d)
                    ds.Add(1);
                else
                    ds.Add(0);
            }
            return new LogicFpiVector(fv.vType, ds.ToArray());
        }
        public static LogicFpiVector operator <(double d, FpiVector fv) {
            return (fv > d);
        }
        public static LogicFpiVector operator <=(FpiVector fv, double d) {
            List<double> ds = new List<double>();
            for (int t = 1; t <= fv.Length; t++) {
                if (fv[t] <= d)
                    ds.Add(1);
                else
                    ds.Add(0);
            }
            return new LogicFpiVector(fv.vType, ds.ToArray());
        }
        public static LogicFpiVector operator <=(double d, FpiVector fv) {
            return (fv >= d);
        }
        public static FpiVector operator &(FpiVector fv1, FpiVector fv2) {
            return (FpiVector)(((FpiMatrix)fv1) & ((FpiMatrix)fv2));
        }
        #endregion

        /// <summary>
        /// 删除 所在位子的数据点
        /// </summary>
        /// <param name="fvDel"></param>
        public void Remove(FpiVector fvDel) {
            List<double> d = new List<double>();
            for (int t = 1; t <= this.Length; t++) {
                bool isdel = false;
                for (int dt = 1; dt <= fvDel.Length; dt++) {
                    if (t == (int)fvDel[dt]) {
                        //需要删除的数据删除
                        isdel = true;
                    }
                }
                if (!isdel) {//不需要删除的数据
                    d.Add(this[t]);
                }
            }
            this.elements = d.ToArray();
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }
    }

    /// <summary>
    /// 逻辑矩阵类 继承FpiMatrix
    /// </summary>
    public class LogicFpiMatrix : FpiMatrix {

        #region 构造函数
        public LogicFpiMatrix()
            : base() {
            Init();
        }

        /// <summary>
        /// 指定行列构造函数
        /// </summary>
        /// <param name="nRows">指定的矩阵行数</param>
        /// <param name="nCols">指定的矩阵列数</param>
        public LogicFpiMatrix(int nRows, int nCols)
            : base(nRows, nCols) {
            Init();
        }

        /// <summary>
        /// 指定值构造函数
        /// </summary>
        /// <param name="value">二维数组，存储矩阵各元素的值</param>
        public LogicFpiMatrix(double[,] value)
            : base(value) {
            Init();
        }

        /// <summary>
        /// 指定值构造函数
        /// </summary>
        /// <param name="nRows">指定的矩阵行数</param>
        /// <param name="nCols">指定的矩阵列数</param>
        /// <param name="value">一维数组，长度为nRows*nCols，存储矩阵各元素的值</param>
        public LogicFpiMatrix(int nRows, int nCols, double[] value)
            : base(nRows, nCols, value) {
            Init();
        }


        /// <summary>
        /// 方阵构造函数
        /// </summary>
        /// <param name="nSize">方阵行列数</param>
        public LogicFpiMatrix(int nSize)
            : base(nSize) {
            Init();
        }

        /// <summary>
        /// 方阵构造函数
        /// </summary>
        /// <param name="nSize">方阵行列数</param>
        /// <param name="value">一维数组，长度为nRows*nRows，存储方阵各元素的值</param>
        public LogicFpiMatrix(int nSize, double[] value)
            : base(nSize, value) {
            Init();
        }

        private void Init() {
            for (int r = 1; r <= this.RowsCount; r++) {
                for (int c = 1; c <= this.ColumnsCount; c++) {
                    if (this[r, c] != 0)
                        SetElement(r - 1, c - 1, 1);
                    else
                        SetElement(r - 1, c - 1, 0);
                }
            }
        }
        #endregion

        #region 类型转换
        public static implicit operator bool(LogicFpiMatrix lfm) {
            for (int r = 1; r <= lfm.RowsCount; r++) {
                for (int c = 1; c <= lfm.ColumnsCount; c++) {
                    if (lfm[r, c] == 0) {
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion
    }

    /// <summary>
    /// 逻辑向量类 继承FpiVector
    /// </summary>
    public class LogicFpiVector : FpiVector {
        #region 构造函数
        /// <summary>
        /// 无数据的行向量
        /// </summary>
        public LogicFpiVector() {
            vType = VectorType.RowVector;
            elements = new double[] { };
        }
        public LogicFpiVector(VectorType vType) {
            this.vType = vType;
            elements = new double[] { };
        }
        public LogicFpiVector(VectorType vType, double[] elements) {
            this.vType = vType;
            this.elements = elements;
        }
        public LogicFpiVector(VectorType vType, int size) {
            this.vType = vType;
            this.elements = new double[size];
        }
        #endregion

        #region 类型转换
        public static implicit operator bool(LogicFpiVector lfm) {
            for (int t = 0; t < lfm.elements.Length; t++) {
                if (lfm.elements[t] == 0) {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }

    public enum VectorType : int {
        /// <summary>
        /// 行向量
        /// </summary>
        RowVector = 1,
        /// <summary>
        /// 列向量
        /// </summary>
        ColVector = 2,
    }
}
