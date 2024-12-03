using System;

using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Fpi.Util.MathUtil
{
    public class MathRange 
    {
        public double minValue;
        public double maxValue;
        public double a;
        public double b;
        public double c;

        private ArrayList tempZeroList = new ArrayList(); //存放待平均的零点
        private ArrayList tempRangeParaList = new ArrayList(); //存放待平均的标定系数

        private double zeroPoint = 0; //零点
        private double rangePara = 1; //标定系数

        private int smoothTimes = 1; //平滑次数

        public MathRange(double zero,double range)
        {
            this.zeroPoint = zero;
            this.rangePara = range;
        }

        /// <summary>
        /// 得到向量计算平滑后的原始浓度
        /// </summary>
        /// <param name="valdata"></param>
        /// <returns></returns>
        private double ComputeVectorConc(double valdata)
        {
            double tempconc = Math.Pow(valdata, 2) * a + b * valdata + c;
            tempconc = ComputeSmoothAverConc(tempconc);
            return tempconc;
        }
        /// <summary>
        /// 得到扣除零点后的原始浓度
        /// </summary>
        /// <param name="valdata"></param>
        /// <returns></returns>
        private double ComputeOriginalConc(double valdata, float press)
        {
            double tempconc = ComputeVectorConc(valdata);
            tempconc = PressCompensate(tempconc, press);
            tempconc -= this.zeroPoint;
            return tempconc;
        }
        /// <summary>
        /// 计算压力补偿后的浓度
        /// </summary>
        /// <param name="val"></param>
        /// <param name="press">压力</param>
        /// <returns></returns>
        private double PressCompensate(double val, float press)
        {
            return val * 14.69f / press;
        }
        public double ComputeRealConc(double val, float press)
        {
            double realConc = ComputeOriginalConc(val,press) * this.rangePara;
            return realConc;
        }
        #region 滑动平均计算

        private int computeTimes = 0;


        private double lastConc = double.NaN;
        //private DataCash concCash = new DataCash(2);
        //private DataCash concSmoothCash = new DataCash(2);
        private DataSmoothCash concSmoothCash = new DataSmoothCash();
        /// <summary>
        /// 计算滑动平均浓度
        /// </summary>
        /// <param name="conc">当次浓度</param>
        /// <param name="computetimes"></param>
        /// <returns></returns>
        private double ComputeSmoothAverConc(double conc)
        {
            
            double result = conc;

            if (double.IsNaN(lastConc))
            {
                lastConc = 0;
            }
            if (computeTimes++ <= smoothTimes)
            {
                result = ((computeTimes - 1) * lastConc + conc) / computeTimes;
            }
            else
            {
                computeTimes = smoothTimes;
                result = ((smoothTimes - 1) * lastConc + conc) / smoothTimes;
            }
            lastConc = result;
            return result;
        }


        private bool ChgSmall(DataCash concCash, DataCash smoothConcCash, double limit)
        {
            int count = concCash.Count;
            if (double.IsNaN(smoothConcCash.Datas[0]))
            {
                return false;
            }
            for (int i = 0; i < count - 1; i++)
            {
                if (double.IsNaN(smoothConcCash.Datas[i]))
                    continue;
                double tmp = Math.Abs(concCash.Datas[i] - smoothConcCash.Datas[i]);
                if (tmp > limit)
                {
                    return false;
                }

            }
            return true;

        }

        private bool ChgBig(DataCash concCash, DataCash smoothConcCash, double limit)
        {
            int count = concCash.Count;
            if (double.IsNaN(smoothConcCash.Datas[0]))
            {
                return false;
            }
            for (int i = 0; i < count - 1; i++)
            {
                if (double.IsNaN(smoothConcCash.Datas[i]))
                    continue;
                double tmp = Math.Abs(concCash.Datas[i] - smoothConcCash.Datas[i]);
                if (tmp < limit)
                {
                    return false;
                }

            }
            return true;

        }


        /// <summary>
        /// 重新开始滑动平均计算浓度
        /// </summary>
        public void RestSmoothValue()
        {
            preConc = 0f;
            computeTimes = 0;
        }

        #endregion

        #region 计算零点

        /// <summary>
        /// 计算零点
        /// </summary>
        /// <param name="specdata"></param>
        public void ComputeZeroPoint(double val, float press)
        {
            double zeropoint = ComputeVectorConc(val);
            zeropoint = PressCompensate(zeropoint, press);
            tempZeroList.Add(zeropoint);
        }
        /// <summary>
        /// 计算平均后的零点
        /// </summary>
        public void ComputeAverZeroPoint()
        {
            double aver = 0;
            for (int i = 0; i < tempZeroList.Count; i++)
            {
                aver += (double)tempZeroList[i];
            }
            if (tempZeroList.Count == 0)
            {
                this.zeroPoint = 0;
            }
            else
            {
                if (IOManager.GetInstance().ZeroAirGas && this.paranode.id == "H2O")
                {
                    //
                }
                else
                {
                    this.zeroPoint = aver / tempZeroList.Count;
                }
            }
            this.tempZeroList.Clear();
        }
        #endregion

        #region 计算标定系数

        /// <summary>
        /// 计算标定系数
        /// </summary>
        /// <param name="val"></param>
        /// <param name="press"></param>
        public void ComputeRangePara(double val, double calConc, float press)
        {
            double realConc = ComputeOriginalConc(val,press);
            //realConc = PressCompensate(realConc, press);
            this.tempRangeParaList.Add(calConc / realConc);
        }
        /// <summary>
        /// 计算平均后的标定系数
        /// </summary>
        public void ComputeAverRangePara()
        {
            double aver = 0;
            for (int i = 0; i < tempRangeParaList.Count; i++)
            {
                aver += (double)tempRangeParaList[i];
            }
            if (tempRangeParaList.Count == 0)
            {
                this.rangePara = 1;
            }
            else
            {
                double k = aver / tempRangeParaList.Count;
                if (this.paranode.id == "30")
                {
                    k = k / 100;
                }
                if (k > 1.5d || k < 0.8d)
                {
                    throw new Exception("标定系数超限");
                }
                this.rangePara = k;
            }
            this.tempRangeParaList.Clear();
        }
        #endregion
    }
}
