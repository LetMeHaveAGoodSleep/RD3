/*
 * 操作矩阵的类 Matrix
 * 
 * 周长发编制
 */
using System;
using System.Collections;
using System.IO;

namespace Fpi.Util.MathUtil
{
    /**
    * 操作矩阵的类 Matrix

    * @author 周长发
    * @version 1.0
    */
#if !WINCE
    [Serializable]
#endif
    public class Matrix
    {
        #region 私有变量

        /// <summary>
        /// 矩阵列数
        /// </summary>
        private int numColumns;

        /// <summary>
        /// 矩阵行数
        /// </summary>
        private int numRows; 

        /// <summary>
        /// 矩阵运算缺省精度
        /// </summary>
        private double eps = 0.000001; 

        /// <summary>
        /// 矩阵原始数据存储
        /// Maxtrix[i（行）,j（列）]->elements[i*列数+j]
        /// </summary>
        protected double[] elements;

        public static double[] t_tab = {
                                           12.706, 4.303, 3.182, 2.776, 2.571, 2.447, 2.365, 2.306, 2.262, 2.228, 2.201,
                                           2.179, 2.16, 2.145, 2.131, 2.12,
                                           2.11, 2.101, 2.093, 2.086, 2.08, 2.074, 2.069, 2.064, 2.06, 2.056, 2.052,
                                           2.048, 2.045, 2.042, 2.039, 2.036,
                                           2.034, 2.032, 2.03, 2.028, 2.026, 2.024, 2.022, 2.02, 2.019, 2.018, 2.017,
                                           2.016, 2.015, 2.014, 2.013, 2.012,
                                           2.011, 2.01, 2.0095, 2.009, 2.0085, 2.008, 2.0075, 2.007, 2.0065, 2.006,
                                           2.0055, 2.005, 2.0045, 2.004, 2.0035,
                                           2.003, 2.0025, 2.002, 2.0015, 2.001, 2.0005, 2, 1.9999, 1.9998, 1.9997,
                                           1.9996, 1.9995, 1.9994, 1.9993, 1.9992,
                                           1.9991, 1.999, 1.9989, 1.9988, 1.9987, 1.9986, 1.9985, 1.9984, 1.9983, 1.9982
                                           , 1.9981, 1.998, 1.9979, 1.9978,
                                           1.9977, 1.9976, 1.9975, 1.9974, 1.9973, 1.9972, 1.9971, 1.997, 1.9969, 1.9968
                                           , 1.9967, 1.9966, 1.9965, 1.9964,
                                           1.9963, 1.9962, 1.9961, 1.996, 1.9959, 1.9958, 1.9957, 1.9956, 1.9955, 1.9954
                                           , 1.9953, 1.9952, 1.9951, 1.995,
                                           1.9949, 1.9948, 1.9947, 1.9946, 1.9945, 1.9944, 1.9943, 1.9942, 1.9941, 1.994
                                           , 1.9939, 1.9938, 1.9937, 1.9936,
                                           1.9935, 1.9934, 1.9933, 1.9932, 1.9931, 1.993, 1.9929, 1.9928, 1.9927, 1.9926
                                           , 1.9925, 1.9924, 1.9923, 1.9922,
                                           1.9921, 1.992, 1.9919, 1.9918, 1.9917, 1.9916, 1.9915, 1.9914, 1.9913, 1.9912
                                           , 1.9911, 1.991, 1.9909, 1.9908,
                                           1.9907, 1.9906, 1.9905, 1.9904, 1.9903, 1.9902, 1.9901, 1.99, 1.9899, 1.9898,
                                           1.9897, 1.9896, 1.9895, 1.9894,
                                           1.9893, 1.9892, 1.9891, 1.989, 1.9889, 1.9888, 1.9887, 1.9886, 1.9885, 1.9884
                                           , 1.9883, 1.9882, 1.9881, 1.988,
                                           1.9879, 1.9878, 1.9877, 1.9876, 1.9875, 1.9874, 1.9873, 1.9872, 1.9871, 1.987
                                           , 1.9869, 1.9868, 1.9867, 1.9866,
                                           1.9865, 1.9864, 1.9863, 1.9862, 1.9861, 1.986, 1.9859, 1.9858, 1.9857, 1.9856
                                           , 1.9855, 1.9854, 1.9853, 1.9852,
                                           1.9851, 1.985, 1.9849, 1.9848, 1.9847, 1.9846, 1.9845, 1.9844, 1.9843, 1.9842
                                           , 1.9841, 1.984, 1.9839, 1.9838,
                                           1.9837, 1.9836, 1.9835, 1.9834, 1.9833, 1.9832, 1.9831, 1.983, 1.9829, 1.9828
                                           , 1.9827, 1.9826, 1.9825, 1.9824,
                                           1.9823, 1.9822, 1.9821, 1.982, 1.9819, 1.9818, 1.9817, 1.9816, 1.9815, 1.9814
                                           , 1.9813, 1.9812, 1.9811, 1.981,
                                           1.9809, 1.9808, 1.9807, 1.9806, 1.9805, 1.9804, 1.9803, 1.9802, 1.9801, 1.98,
                                           1.97998, 1.97996, 1.97994, 1.97992,
                                           1.9799, 1.97988, 1.97986, 1.97984, 1.97982, 1.9798, 1.97978, 1.97976, 1.97974
                                           , 1.97972, 1.9797, 1.97968, 1.97966,
                                           1.97964, 1.97962, 1.9796, 1.97958, 1.97956, 1.97954, 1.97952, 1.9795, 1.97948
                                           , 1.97946, 1.97944, 1.97942, 1.9794,
                                           1.97938, 1.97936, 1.97934, 1.97932, 1.9793, 1.97928, 1.97926, 1.97924,
                                           1.97922, 1.9792, 1.97918, 1.97916,
                                           1.97914, 1.97912, 1.9791, 1.97908, 1.97906, 1.97904, 1.97902, 1.979, 1.97898,
                                           1.97896, 1.97894, 1.97892,
                                           1.9789, 1.97888, 1.97886, 1.97884, 1.97882, 1.9788, 1.97878, 1.97876, 1.97874
                                           , 1.97872, 1.9787, 1.97868,
                                           1.97866, 1.97864, 1.97862, 1.9786, 1.97858, 1.97856, 1.97854, 1.97852, 1.9785
                                           , 1.97848, 1.97846,
                                           1.97844, 1.97842, 1.9784, 1.97838, 1.97836, 1.97834, 1.97832, 1.9783, 1.97828
                                           , 1.97826, 1.97824,
                                           1.97822, 1.9782, 1.97818, 1.97816, 1.97814, 1.97812, 1.9781, 1.97808, 1.97806
                                           , 1.97804, 1.97802, 1.978, 1.97798,
                                           1.97796, 1.97794, 1.97792, 1.9779, 1.97788, 1.97786, 1.97784, 1.97782, 1.9778
                                           , 1.97778, 1.97776, 1.97774,
                                           1.97772, 1.9777, 1.97768, 1.97766, 1.97764, 1.97762, 1.9776, 1.97758, 1.97756
                                           , 1.97754, 1.97752,
                                           1.9775, 1.97748, 1.97746, 1.97744, 1.97742, 1.9774, 1.97738, 1.97736,
                                           1.977341, 97732, 1.9773, 1.97728,
                                           1.97726, 1.97724, 1.97722, 1.9772, 1.97718, 1.97716, 1.97714, 1.97712, 1.9771
                                           , 1.97708, 1.97706,
                                           1.97704, 1.97702, 1.977, 1.97698, 1.97696, 1.97694, 1.97692, 1.9769, 1.97688,
                                           1.97686, 1.97684,
                                           1.97682, 1.9768, 1.97678, 1.97676, 1.97674, 1.97672, 1.9767, 1.97668, 1.97666
                                           , 1.97664, 1.97662, 1.9766, 1.97658,
                                           1.97656, 1.97654, 1.97652, 1.9765, 1.97648, 1.97646, 1.97644, 1.97642, 1.9764
                                           , 1.97638, 1.97636, 1.97634,
                                           1.97632, 1.9763, 1.97628, 1.97626, 1.97624, 1.97622, 1.9762, 1.97618, 1.97616
                                           , 1.97614, 1.97612, 1.9761,
                                           1.97608, 1.97606, 1.97604, 1.97602, 1.976, 1.97598, 1.97596, 1.97594, 1.97592
                                           , 1.9759, 1.97588, 1.97586,
                                           1.97584, 1.97582, 1.9758, 1.97578, 1.97576, 1.97574, 1.97572, 1.9757, 1.97568
                                           , 1.97566, 1.97564,
                                           1.97562, 1.9756, 1.97558, 1.97556, 1.97554, 1.97552, 1.9755, 1.97548, 1.97546
                                           , 1.97544, 1.97542, 1.9754,
                                           1.97538, 1.97536, 1.97534, 1.97532, 1.9753, 1.97528, 1.97526, 1.97524,
                                           1.97522, 1.9752, 1.97518,
                                           1.97516, 1.97514, 1.97512, 1.9751, 1.97508, 1.97502, 1.975, 1.97506, 1.97504,
                                           1.97498, 1.97496, 1.97494,
                                           1.97492, 1.9749, 1.97488, 1.97486, 1.97484, 1.97482, 1.9748, 1.97478, 1.97476
                                           , 1.97474, 1.97472, 1.9747,
                                           1.97468, 1.97466, 1.97464, 1.97462, 1.9746, 1.97458, 1.97456, 1.97454,
                                           1.97452, 1.9745, 1.97448, 1.97446,
                                           1.97444, 1.97442, 1.9744, 1.97438, 1.97436, 1.97434, 1.97432, 1.9743, 1.97428
                                           , 1.97426, 1.97424, 1.97422,
                                           1.9742, 1.97418, 1.97416, 1.97414, 1.97412, 1.9741, 1.97408, 1.97406, 1.97404
                                           , 1.97402, 1.974, 1.97398,
                                           1.97396, 1.97394, 1.97392, 1.9739, 1.97388, 1.97386, 1.97384, 1.97382, 1.9738
                                           , 1.97378, 1.97376, 1.97374,
                                           1.97372, 1.9737, 1.97368, 1.97366, 1.97364, 1.97362, 1.9736, 1.97358, 1.97356
                                           , 1.97354, 1.97352, 1.9735,
                                           1.97348, 1.97346, 1.97344, 1.97342, 1.9734, 1.97338, 1.97336, 1.97334,
                                           1.97332, 1.9733, 1.97328, 1.97326,
                                           1.97324, 1.97322, 1.9732, 1.97318, 1.97316, 1.97314, 1.97312, 1.9731, 1.97308
                                           , 1.97306, 1.97304, 1.97302,
                                           1.973, 1.97298, 1.97296, 1.97294, 1.97292, 1.9729, 1.97288, 1.97286, 1.97284,
                                           1.97282, 1.9728, 1.97278,
                                           1.97276, 1.97274, 1.97272, 1.9727, 1.97268, 1.97266, 1.97264, 1.97262, 1.9726
                                           , 1.97258, 1.97256, 1.97254,
                                           1.97252, 1.9725, 1.97248, 1.97246, 1.97244, 1.97242, 1.9724, 1.97238, 1.97236
                                           , 1.97234, 1.97232, 1.9723,
                                           1.97228, 1.97226, 1.97224, 1.97222, 1.9722, 1.97218, 1.97216, 1.97214,
                                           1.97212, 1.9721, 1.97208, 1.97206,
                                           1.97204, 1.97202, 1.972, 1.97198, 1.97196, 1.97194, 1.97192, 1.9719, 1.97188,
                                           1.97186, 1.97184, 1.97182,
                                           1.9718, 1.97178, 1.97176, 1.97174, 1.97172, 1.9717, 1.97168, 1.97166, 1.97164
                                           , 1.97162, 1.9716, 1.97158,
                                           1.97156, 1.97154, 1.97152, 1.9715, 1.97148, 1.97146, 1.97144, 1.97142, 1.9714
                                           , 1.97138, 1.97136,
                                           1.97134, 1.97132, 1.9713, 1.97128, 1.97126, 1.97124, 1.97122, 1.9712, 1.97118
                                           , 1.97116, 1.97114, 1.97112,
                                           1.9711, 1.97108, 1.97106, 1.97104, 1.97102, 1.971, 1.97098, 1.97096, 1.97094,
                                           1.97092, 1.9709, 1.97088,
                                           1.97086, 1.97084, 1.97082, 1.9708, 1.97078, 1.97076, 1.97074, 1.97072, 1.9707
                                           , 1.97068, 1.97066, 1.97064,
                                           1.97062, 1.9706, 1.97058, 1.97056, 1.97054, 1.97052, 1.9705, 1.97048, 1.97046
                                           , 1.97044, 1.97042, 1.9704,
                                           1.97038, 1.97036, 1.97034, 1.97032, 1.9703, 1.97028, 1.97026, 1.97024,
                                           1.97022, 1.9702, 1.97018, 1.97016,
                                           1.97014, 1.97012, 1.9701, 1.97008, 1.97006, 1.97004, 1.97002, 1.97, 1.96998,
                                           1.96996, 1.96994, 1.96992,
                                           1.9699, 1.96988, 1.96986, 1.96984, 1.96982, 1.9698, 1.96978, 1.96976, 1.96974
                                           , 1.96972, 1.9697, 1.96968,
                                           1.96966, 1.96964, 1.96962, 1.9696, 1.96958, 1.96956, 1.96954, 1.96952, 1.9695
                                           , 1.96948, 1.96946, 1.96944,
                                           1.96942, 1.9694, 1.96938, 1.96936, 1.96934, 1.96932, 1.9693, 1.96928, 1.96926
                                           , 1.96924, 1.96922, 1.9692,
                                           1.96918, 1.96916, 1.96914, 1.96912, 1.9691, 1.96908, 1.96906, 1.96904,
                                           1.96902, 1.969, 1.96898, 1.96896,
                                           1.96894, 1.96892, 1.9689, 1.96888, 1.96886, 1.96884, 1.96882, 1.9688, 1.96878
                                           , 1.96876, 1.96874, 1.96872,
                                           1.9687, 1.96868, 1.96866, 1.96864, 1.96862, 1.9686, 1.96858, 1.96856, 1.96854
                                           , 1.96852, 1.9685, 1.96848,
                                           1.96846, 1.96844, 1.96842, 1.9684, 1.96838, 1.96836, 1.96834, 1.96832, 1.9683
                                           , 1.96828, 1.96826, 1.96824,
                                           1.96822, 1.9682, 1.96818, 1.96816, 1.96814, 1.96812, 1.9681, 1.96808, 1.96806
                                           , 1.96804, 1.96802, 1.968,
                                           1.96798, 1.96796, 1.96794, 1.96792, 1.9679, 1.96788, 1.96786, 1.96784,
                                           1.96782, 1.9678, 1.96778, 1.96776,
                                           1.96774, 1.96772, 1.9677, 1.96768, 1.96766, 1.96764, 1.96762, 1.9676, 1.96758
                                           , 1.96756, 1.96754, 1.96752,
                                           1.9675, 1.96748, 1.96746, 1.96744, 1.96742, 1.9674, 1.96738, 1.96736, 1.96734
                                           , 1.96732, 1.9673, 1.96728,
                                           1.96726, 1.96724, 1.96722, 1.9672, 1.96718, 1.96716, 1.96714, 1.96712, 1.9671
                                           , 1.96708, 1.96706, 1.96704,
                                           1.96702, 1.967, 1.96698, 1.96696, 1.96694, 1.96692, 1.9669, 1.96688, 1.96686,
                                           1.96684, 1.96682, 1.9668,
                                           1.96678, 1.96676, 1.96674, 1.96672, 1.9667, 1.96668, 1.96666, 1.96664,
                                           1.96662, 1.9666, 1.96658, 1.96656,
                                           1.96654, 1.96652, 1.9665, 1.96648, 1.96646, 1.96644, 1.96642, 1.9664, 1.96638
                                           , 1.96636, 1.96634, 1.96632,
                                           1.9663, 1.96628, 1.96626, 1.96624, 1.96622, 1.9662, 1.96618, 1.96616, 1.96614
                                           , 1.96612, 1.9661, 1.96608,
                                           1.96606, 1.96604, 1.96602, 1.966, 1.96598, 1.96596, 1.96594, 1.96592, 1.9659,
                                           1.96588, 1.96586, 1.96584,
                                           1.96582, 1.9658, 1.96578, 1.96576, 1.96574, 1.96572, 1.9657, 1.96568, 1.96566
                                           , 1.96564, 1.96562, 1.9656,
                                           1.96558, 1.96556, 1.96554, 1.96552, 1.9655, 1.96548, 1.96546, 1.96544,
                                           1.96542, 1.9654
                                       };

        #endregion

        /**
         * 属性: 矩阵列数
         */

        public int ColumnsCount
        {
            get { return numColumns; }
        }

        /**
         * 属性: 矩阵行数
         */

        public int RowsCount
        {
            get { return numRows; }
        }

        /**
         * 索引器: 访问矩阵元素
         * @param row - 元素的行
         * @param col - 元素的列
         */

        public double this[int row, int col]
        {
            get { return elements[col + row*numColumns]; }
            set { elements[col + row*numColumns] = value; }
        }

        /**
         * 属性: Eps
         */

        public double Eps
        {
            get { return eps; }
            set { eps = value; }
        }

        /**
         * 基本构造函数
         */

        public Matrix()
        {
            numColumns = 1;
            numRows = 1;
            Init(numRows, numColumns);
        }

        /**
         * 指定行列构造函数
         * 
         * @param nRows - 指定的矩阵行数
         * @param nCols - 指定的矩阵列数
         */

        public Matrix(int nRows, int nCols)
        {
            numRows = nRows;
            numColumns = nCols;
            Init(numRows, numColumns);
        }

        /**
         * 指定值构造函数
         * 
         * @param value - 二维数组，存储矩阵各元素的值
         */

        public Matrix(double[,] value)
        {
            numRows = value.GetLength(0);
            numColumns = value.GetLength(1);
            double[] data = new double[numRows*numColumns];
            int k = 0;
            for (int i = 0; i < numRows; ++i)
            {
                for (int j = 0; j < numColumns; ++j)
                {
                    data[k++] = value[i, j];
                }
            }
            Init(numRows, numColumns);
            SetData(data);
        }

        /**
         * 指定值构造函数
         * 
         * @param nRows - 指定的矩阵行数
         * @param nCols - 指定的矩阵列数
         * @param value - 一维数组，长度为nRows*nCols，存储矩阵各元素的值
         */

        public Matrix(int nRows, int nCols, double[] value)
        {
            numRows = nRows;
            numColumns = nCols;
            Init(numRows, numColumns);
            SetData(value);
        }

        /**
         * 方阵构造函数
         * 
         * @param nSize - 方阵行列数
         */

        public Matrix(int nSize)
        {
            numRows = nSize;
            numColumns = nSize;
            Init(nSize, nSize);
        }

        /**
         * 方阵构造函数
         * 
         * @param nSize - 方阵行列数
         * @param value - 一维数组，长度为nRows*nRows，存储方阵各元素的值
         */

        public Matrix(int nSize, double[] value)
        {
            numRows = nSize;
            numColumns = nSize;
            Init(nSize, nSize);
            SetData(value);
        }

        /**
         * 拷贝构造函数
         * 
         * @param other - 源矩阵
         */

        public Matrix(Matrix other)
        {
            numColumns = other.GetNumColumns();
            numRows = other.GetNumRows();
            Init(numRows, numColumns);
            SetData(other.elements);
        }

        /**
         * 初始化函数
         * 
         * @param nRows - 指定的矩阵行数
         * @param nCols - 指定的矩阵列数
         * @return bool, 成功返回true, 否则返回false
         */

        public bool Init(int nRows, int nCols)
        {
            numRows = nRows;
            numColumns = nCols;
            int nSize = nCols*nRows;
            if (nSize < 0)
                return false;

            // 分配内存
            elements = new double[nSize];

            return true;
        }

        /**
         * 设置矩阵运算的精度
         * 
         * @param newEps - 新的精度值
         */

        public void SetEps(double newEps)
        {
            eps = newEps;
        }

        /**
         * 取矩阵的精度值
         * 
         * @return double型，矩阵的精度值
         */

        public double GetEps()
        {
            return eps;
        }

        /**
         * 重载 + 运算符
         * 
         * @return Matrix对象
         */

        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            return m1.Add(m2);
        }

        /**
         * 重载 - 运算符
         * 
         * @return Matrix对象
         */

        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            return m1.Subtract(m2);
        }

        /**
         * 重载 * 运算符
         * 
         * @return Matrix对象
         */

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            return m1.Multiply(m2);
        }

        public static Matrix operator *(Matrix m1, double m2)
        {
            return m1.Multiply(m2);
        }

        /**
         * 重载 double[] 运算符
         * 
         * @return double[]对象
         */

        public static implicit operator double[](Matrix m)
        {
            return m.elements;
        }

        /**
         * 将方阵初始化为单位矩阵
         * 
         * @param nSize - 方阵行列数
         * @return bool 型，初始化是否成功
         */

        public bool MakeUnitMatrix(int nSize)
        {
            if (!Init(nSize, nSize))
                return false;

            for (int i = 0; i < nSize; ++i)
                for (int j = 0; j < nSize; ++j)
                    if (i == j)
                        SetElement(i, j, 1);

            return true;
        }

        /**
         * 将矩阵各元素的值转化为字符串, 元素之间的分隔符为",", 行与行之间有回车换行符
         * @return string 型，转换得到的字符串
         */

        public override string ToString()
        {
            return ToString(",", true);
        }

        /**
         * 将矩阵各元素的值转化为字符串
         * 
         * @param sDelim - 元素之间的分隔符
         * @param bLineBreak - 行与行之间是否有回车换行符
         * @return string 型，转换得到的字符串
         */

        public string ToString(string sDelim, bool bLineBreak)
        {
            string s = "";

            for (int i = 0; i < numRows; ++i)
            {
                for (int j = 0; j < numColumns; ++j)
                {
                    string ss = GetElement(i, j).ToString("F8");
                    s += ss;

                    if (bLineBreak)
                    {
                        if (j != numColumns - 1)
                            s += sDelim;
                    }
                    else
                    {
                        if (i != numRows - 1 || j != numColumns - 1)
                            s += sDelim;
                    }
                }
                if (bLineBreak)
                    if (i != numRows - 1)
                        s += "\r\n";
            }

            return s;
        }

        /**
         * 将矩阵指定行中各元素的值转化为字符串
         * 
         * @param nRow - 指定的矩阵行，nRow = 0表示第一行
         * @param sDelim - 元素之间的分隔符
         * @return string 型，转换得到的字符串
         */

        public string ToStringRow(int nRow, string sDelim)
        {
            string s = "";

            if (nRow >= numRows)
                return s;

            for (int j = 0; j < numColumns; ++j)
            {
                string ss = GetElement(nRow, j).ToString("F");
                s += ss;
                if (j != numColumns - 1)
                    s += sDelim;
            }

            return s;
        }

        /**
         * 将矩阵指定列中各元素的值转化为字符串
         * 
         * @param nCol - 指定的矩阵行，nCol = 0表示第一列
         * @param sDelim - 元素之间的分隔符
         * @return string 型，转换得到的字符串
         */

        public string ToStringCol(int nCol, string sDelim /*= " "*/)
        {
            string s = "";

            if (nCol >= numColumns)
                return s;

            for (int i = 0; i < numRows; ++i)
            {
                string ss = GetElement(i, nCol).ToString("F");
                s += ss;
                if (i != numRows - 1)
                    s += sDelim;
            }

            return s;
        }

        /**
         * 设置矩阵各元素的值
         * 
         * @param value - 一维数组，长度为numColumns*numRows，存储
         *	              矩阵各元素的值
         */

        public void SetData(double[] value)
        {
            elements = (double[]) value.Clone();
        }

        /**
         * 设置指定元素的值
         * 
         * @param nRow - 元素的行
         * @param nCol - 元素的列
         * @param value - 指定元素的值
         * @return bool 型，说明设置是否成功
         */

        public bool SetElement(int nRow, int nCol, double value)
        {
            if (nCol < 0 || nCol >= numColumns || nRow < 0 || nRow >= numRows)
                return false; // array bounds error

            elements[nCol + nRow*numColumns] = value;

            return true;
        }

        /**
         * 获取指定元素的值
         * 
         * @param nRow - 元素的行
         * @param nCol - 元素的列
         * @return double 型，指定元素的值
         */

        public double GetElement(int nRow, int nCol)
        {
            return elements[nCol + nRow*numColumns];
        }

        /**
         * 获取矩阵的列数
         * 
         * @return int 型，矩阵的列数
         */

        public int GetNumColumns()
        {
            return numColumns;
        }

        /**
         * 获取矩阵的行数
         * @return int 型，矩阵的行数
         */

        public int GetNumRows()
        {
            return numRows;
        }

        /**
         * 获取矩阵的数据
         * 
         * @return double型数组，指向矩阵各元素的数据缓冲区
         */

        public double[] GetData()
        {
            return elements;
        }

        public double[,] GetArrayData()
        {
            double[,] result = new double[numRows,numColumns];
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    result[i, j] = elements[j + i*numColumns];
                }
            }
            return result;
        }

        /**
         * 获取指定行的向量
         * 
         * @param nRow - 向量所在的行
         * @param pVector - 指向向量中各元素的缓冲区
         * @return int 型，向量中元素的个数，即矩阵的列数
         */

        public int GetRowVector(int nRow, double[] pVector)
        {
            for (int j = 0; j < numColumns; ++j)
                pVector[j] = GetElement(nRow, j);

            return numColumns;
        }

        /**
         * 获取指定列的向量
         * 
         * @param nCol - 向量所在的列
         * @param pVector - 指向向量中各元素的缓冲区
         * @return int 型，向量中元素的个数，即矩阵的行数
         */

        public int GetColVector(int nCol, double[] pVector)
        {
            for (int i = 0; i < numRows; ++i)
                pVector[i] = GetElement(i, nCol);

            return numRows;
        }

        /**
         * 给矩阵赋值
         * 
         * @param other - 用于给矩阵赋值的源矩阵
         * @return Matrix型，阵与other相等
         */

        public Matrix SetValue(Matrix other)
        {
            if (other != this)
            {
                Init(other.GetNumRows(), other.GetNumColumns());
                SetData(other.elements);
            }

            // finally return a reference to ourselves
            return this;
        }

        /**
         * 判断矩阵否相等
         * 
         * @param other - 用于比较的矩阵
         * @return bool 型，两个矩阵相等则为true，否则为false
         */

        public override bool Equals(object other)
        {
            Matrix matrix = other as Matrix;
            if (matrix == null)
                return false;

            // 首先检查行列数是否相等
            if (numColumns != matrix.GetNumColumns() || numRows != matrix.GetNumRows())
                return false;

            for (int i = 0; i < numRows; ++i)
            {
                for (int j = 0; j < numColumns; ++j)
                {
                    if (System.Math.Abs(GetElement(i, j) - matrix.GetElement(i, j)) > eps)
                        return false;
                }
            }

            return true;
        }

        /**
         * 因为重写了Equals，因此必须重写GetHashCode
         * 
         * @return int型，返回复数对象散列码
         */

        public override int GetHashCode()
        {
            double sum = 0;
            for (int i = 0; i < numRows; ++i)
            {
                for (int j = 0; j < numColumns; ++j)
                {
                    sum += System.Math.Abs(GetElement(i, j));
                }
            }
            return (int) System.Math.Sqrt(sum);
        }

        /**
         * 实现矩阵的加法
         * 
         * @param other - 与指定矩阵相加的矩阵
         * @return Matrix型，指定矩阵与other相加之和
         * @如果矩阵的行/列数不匹配，则会抛出异常
         */

        public Matrix Add(Matrix other)
        {
            // 构造结果矩阵
            Matrix result = new Matrix(this); // 拷贝构造

            if (other == null)
                return result;

            // 首先检查行列数是否相等
            if (numColumns != other.GetNumColumns() ||
                numRows != other.GetNumRows())
                throw new Exception("矩阵的行/列数不匹配。");


            // 矩阵加法
            for (int i = 0; i < numRows; ++i)
            {
                for (int j = 0; j < numColumns; ++j)
                    result.SetElement(i, j, result.GetElement(i, j) + other.GetElement(i, j));
            }

            return result;
        }

        /**
         * 实现矩阵的减法
         * 
         * @param other - 与指定矩阵相减的矩阵
         * @return Matrix型，指定矩阵与other相减之差
         * @如果矩阵的行/列数不匹配，则会抛出异常
         */

        public Matrix Subtract(Matrix other)
        {
            if (numColumns != other.GetNumColumns() ||
                numRows != other.GetNumRows())
                throw new Exception("矩阵的行/列数不匹配。");

            // 构造结果矩阵
            Matrix result = new Matrix(this); // 拷贝构造

            // 进行减法操作
            for (int i = 0; i < numRows; ++i)
            {
                for (int j = 0; j < numColumns; ++j)
                    result.SetElement(i, j, result.GetElement(i, j) - other.GetElement(i, j));
            }

            return result;
        }

        /**
         * 实现矩阵的数乘
         * 
         * @param value - 与指定矩阵相乘的实数
         * @return Matrix型，指定矩阵与value相乘之积
         */

        public Matrix Multiply(double value)
        {
            // 构造目标矩阵
            Matrix result = new Matrix(this); // copy ourselves

            // 进行数乘
            for (int i = 0; i < numRows; ++i)
            {
                for (int j = 0; j < numColumns; ++j)
                    result.SetElement(i, j, result.GetElement(i, j)*value);
            }

            return result;
        }

        /**
         * 实现矩阵的乘法
         * 
         * @param other - 与指定矩阵相乘的矩阵
         * @return Matrix型，指定矩阵与other相乘之积
         * @如果矩阵的行/列数不匹配，则会抛出异常
         */

        public Matrix Multiply(Matrix other)
        {
            // 首先检查行列数是否符合要求
            if (numColumns != other.GetNumRows())
                throw new Exception("矩阵的行/列数不匹配。");

            // ruct the object we are going to return
            Matrix result = new Matrix(numRows, other.GetNumColumns());

            // 矩阵乘法，即
            //
            // [A][B][C]   [G][H]     [A*G + B*I + C*K][A*H + B*J + C*L]
            // [D][E][F] * [I][J] =   [D*G + E*I + F*K][D*H + E*J + F*L]
            //             [K][L]
            //
            double value;
            for (int i = 0; i < result.GetNumRows(); ++i)
            {
                for (int j = 0; j < other.GetNumColumns(); ++j)
                {
                    value = 0.0;
                    for (int k = 0; k < numColumns; ++k)
                    {
                        value += GetElement(i, k)*other.GetElement(k, j);
                    }

                    result.SetElement(i, j, value);
                }
            }

            return result;
        }

        /**
         * 复矩阵的乘法
         * 
         * @param AR - 左边复矩阵的实部矩阵
         * @param AI - 左边复矩阵的虚部矩阵
         * @param BR - 右边复矩阵的实部矩阵
         * @param BI - 右边复矩阵的虚部矩阵
         * @param CR - 乘积复矩阵的实部矩阵
         * @param CI - 乘积复矩阵的虚部矩阵
         * @return bool型，复矩阵乘法是否成功
         */

        public bool Multiply(Matrix AR, Matrix AI, Matrix BR, Matrix BI, Matrix CR, Matrix CI)
        {
            // 首先检查行列数是否符合要求
            if (AR.GetNumColumns() != AI.GetNumColumns() ||
                AR.GetNumRows() != AI.GetNumRows() ||
                BR.GetNumColumns() != BI.GetNumColumns() ||
                BR.GetNumRows() != BI.GetNumRows() ||
                AR.GetNumColumns() != BR.GetNumRows())
                return false;

            // 构造乘积矩阵实部矩阵和虚部矩阵
            Matrix mtxCR = new Matrix(AR.GetNumRows(), BR.GetNumColumns());
            Matrix mtxCI = new Matrix(AR.GetNumRows(), BR.GetNumColumns());
            // 复矩阵相乘
            for (int i = 0; i < AR.GetNumRows(); ++i)
            {
                for (int j = 0; j < BR.GetNumColumns(); ++j)
                {
                    double vr = 0;
                    double vi = 0;
                    for (int k = 0; k < AR.GetNumColumns(); ++k)
                    {
                        double p = AR.GetElement(i, k)*BR.GetElement(k, j);
                        double q = AI.GetElement(i, k)*BI.GetElement(k, j);
                        double s = (AR.GetElement(i, k) + AI.GetElement(i, k))*
                                   (BR.GetElement(k, j) + BI.GetElement(k, j));
                        vr += p - q;
                        vi += s - p - q;
                    }
                    mtxCR.SetElement(i, j, vr);
                    mtxCI.SetElement(i, j, vi);
                }
            }

            CR = mtxCR;
            CI = mtxCI;

            return true;
        }

        /**
         * 矩阵的转置
         * 
         * @return Matrix型，指定矩阵转置矩阵
         */

        public Matrix Transpose()
        {
            // 构造目标矩阵
            Matrix Trans = new Matrix(numColumns, numRows);

            // 转置各元素
            for (int i = 0; i < numRows; ++i)
            {
                for (int j = 0; j < numColumns; ++j)
                    Trans.SetElement(j, i, GetElement(i, j));
            }

            return Trans;
        }

        /// <summary>
        /// 得到矩阵每个列的长度（每个元素的平方和再开根号）
        /// </summary>
        /// <returns></returns>
        public Matrix Norm()
        {
            Matrix result = new Matrix(1, numColumns);

            for (int j = 0; j < numColumns; j++)
            {
                double temp = 0;
                for (int i = 0; i < numRows; i++)
                {
                    temp += System.Math.Pow(GetElement(i, j), 2);
                }
                result.SetElement(0, j, System.Math.Sqrt(temp));
            }
            return result;
        }

        /**
         * 实矩阵求逆的全选主元高斯－约当法
         * 
         * @return bool型，求逆是否成功
         */

        public bool InvertGaussJordan()
        {
            int i, j, k, l, u, v;
            double d = 0, p = 0;

            // 分配内存
            int[] pnRow = new int[numColumns];
            int[] pnCol = new int[numColumns];

            // 消元
            for (k = 0; k <= numColumns - 1; k++)
            {
                d = 0.0;
                for (i = k; i <= numColumns - 1; i++)
                {
                    for (j = k; j <= numColumns - 1; j++)
                    {
                        l = i*numColumns + j;
                        p = System.Math.Abs(elements[l]);
                        if (p > d)
                        {
                            d = p;
                            pnRow[k] = i;
                            pnCol[k] = j;
                        }
                    }
                }

                // 失败
                if (d == 0.0)
                {
                    return false;
                }

                if (pnRow[k] != k)
                {
                    for (j = 0; j <= numColumns - 1; j++)
                    {
                        u = k*numColumns + j;
                        v = pnRow[k]*numColumns + j;
                        p = elements[u];
                        elements[u] = elements[v];
                        elements[v] = p;
                    }
                }

                if (pnCol[k] != k)
                {
                    for (i = 0; i <= numColumns - 1; i++)
                    {
                        u = i*numColumns + k;
                        v = i*numColumns + pnCol[k];
                        p = elements[u];
                        elements[u] = elements[v];
                        elements[v] = p;
                    }
                }

                l = k*numColumns + k;
                elements[l] = 1.0/elements[l];
                for (j = 0; j <= numColumns - 1; j++)
                {
                    if (j != k)
                    {
                        u = k*numColumns + j;
                        elements[u] = elements[u]*elements[l];
                    }
                }

                for (i = 0; i <= numColumns - 1; i++)
                {
                    if (i != k)
                    {
                        for (j = 0; j <= numColumns - 1; j++)
                        {
                            if (j != k)
                            {
                                u = i*numColumns + j;
                                elements[u] = elements[u] - elements[i*numColumns + k]*elements[k*numColumns + j];
                            }
                        }
                    }
                }

                for (i = 0; i <= numColumns - 1; i++)
                {
                    if (i != k)
                    {
                        u = i*numColumns + k;
                        elements[u] = -elements[u]*elements[l];
                    }
                }
            }

            // 调整恢复行列次序
            for (k = numColumns - 1; k >= 0; k--)
            {
                if (pnCol[k] != k)
                {
                    for (j = 0; j <= numColumns - 1; j++)
                    {
                        u = k*numColumns + j;
                        v = pnCol[k]*numColumns + j;
                        p = elements[u];
                        elements[u] = elements[v];
                        elements[v] = p;
                    }
                }

                if (pnRow[k] != k)
                {
                    for (i = 0; i <= numColumns - 1; i++)
                    {
                        u = i*numColumns + k;
                        v = i*numColumns + pnRow[k];
                        p = elements[u];
                        elements[u] = elements[v];
                        elements[v] = p;
                    }
                }
            }

            // 成功返回
            return true;
        }

        /**
         * 复矩阵求逆的全选主元高斯－约当法
         * 
         * @param mtxImag - 复矩阵的虚部矩阵，当前矩阵为复矩阵的实部
         * @return bool型，求逆是否成功
         */

        public bool InvertGaussJordan(Matrix mtxImag)
        {
            int i, j, k, l, u, v, w;
            double p, q, s, t, d, b;

            // 分配内存
            int[] pnRow = new int[numColumns];
            int[] pnCol = new int[numColumns];

            // 消元
            for (k = 0; k <= numColumns - 1; k++)
            {
                d = 0.0;
                for (i = k; i <= numColumns - 1; i++)
                {
                    for (j = k; j <= numColumns - 1; j++)
                    {
                        u = i*numColumns + j;
                        p = elements[u]*elements[u] + mtxImag.elements[u]*mtxImag.elements[u];
                        if (p > d)
                        {
                            d = p;
                            pnRow[k] = i;
                            pnCol[k] = j;
                        }
                    }
                }

                // 失败
                if (d == 0.0)
                {
                    return false;
                }

                if (pnRow[k] != k)
                {
                    for (j = 0; j <= numColumns - 1; j++)
                    {
                        u = k*numColumns + j;
                        v = pnRow[k]*numColumns + j;
                        t = elements[u];
                        elements[u] = elements[v];
                        elements[v] = t;
                        t = mtxImag.elements[u];
                        mtxImag.elements[u] = mtxImag.elements[v];
                        mtxImag.elements[v] = t;
                    }
                }

                if (pnCol[k] != k)
                {
                    for (i = 0; i <= numColumns - 1; i++)
                    {
                        u = i*numColumns + k;
                        v = i*numColumns + pnCol[k];
                        t = elements[u];
                        elements[u] = elements[v];
                        elements[v] = t;
                        t = mtxImag.elements[u];
                        mtxImag.elements[u] = mtxImag.elements[v];
                        mtxImag.elements[v] = t;
                    }
                }

                l = k*numColumns + k;
                elements[l] = elements[l]/d;
                mtxImag.elements[l] = -mtxImag.elements[l]/d;
                for (j = 0; j <= numColumns - 1; j++)
                {
                    if (j != k)
                    {
                        u = k*numColumns + j;
                        p = elements[u]*elements[l];
                        q = mtxImag.elements[u]*mtxImag.elements[l];
                        s = (elements[u] + mtxImag.elements[u])*(elements[l] + mtxImag.elements[l]);
                        elements[u] = p - q;
                        mtxImag.elements[u] = s - p - q;
                    }
                }

                for (i = 0; i <= numColumns - 1; i++)
                {
                    if (i != k)
                    {
                        v = i*numColumns + k;
                        for (j = 0; j <= numColumns - 1; j++)
                        {
                            if (j != k)
                            {
                                u = k*numColumns + j;
                                w = i*numColumns + j;
                                p = elements[u]*elements[v];
                                q = mtxImag.elements[u]*mtxImag.elements[v];
                                s = (elements[u] + mtxImag.elements[u])*(elements[v] + mtxImag.elements[v]);
                                t = p - q;
                                b = s - p - q;
                                elements[w] = elements[w] - t;
                                mtxImag.elements[w] = mtxImag.elements[w] - b;
                            }
                        }
                    }
                }

                for (i = 0; i <= numColumns - 1; i++)
                {
                    if (i != k)
                    {
                        u = i*numColumns + k;
                        p = elements[u]*elements[l];
                        q = mtxImag.elements[u]*mtxImag.elements[l];
                        s = (elements[u] + mtxImag.elements[u])*(elements[l] + mtxImag.elements[l]);
                        elements[u] = q - p;
                        mtxImag.elements[u] = p + q - s;
                    }
                }
            }

            // 调整恢复行列次序
            for (k = numColumns - 1; k >= 0; k--)
            {
                if (pnCol[k] != k)
                {
                    for (j = 0; j <= numColumns - 1; j++)
                    {
                        u = k*numColumns + j;
                        v = pnCol[k]*numColumns + j;
                        t = elements[u];
                        elements[u] = elements[v];
                        elements[v] = t;
                        t = mtxImag.elements[u];
                        mtxImag.elements[u] = mtxImag.elements[v];
                        mtxImag.elements[v] = t;
                    }
                }

                if (pnRow[k] != k)
                {
                    for (i = 0; i <= numColumns - 1; i++)
                    {
                        u = i*numColumns + k;
                        v = i*numColumns + pnRow[k];
                        t = elements[u];
                        elements[u] = elements[v];
                        elements[v] = t;
                        t = mtxImag.elements[u];
                        mtxImag.elements[u] = mtxImag.elements[v];
                        mtxImag.elements[v] = t;
                    }
                }
            }

            // 成功返回
            return true;
        }

        /**
         * 对称正定矩阵的求逆
         * 
         * @return bool型，求逆是否成功
         */

        public bool InvertSsgj()
        {
            int i, j, k, m;
            double w, g;

            // 临时内存
            double[] pTmp = new double[numColumns];

            // 逐列处理
            for (k = 0; k <= numColumns - 1; k++)
            {
                w = elements[0];
                if (w == 0.0)
                {
                    return false;
                }

                m = numColumns - k - 1;
                for (i = 1; i <= numColumns - 1; i++)
                {
                    g = elements[i*numColumns];
                    pTmp[i] = g/w;
                    if (i <= m)
                        pTmp[i] = -pTmp[i];
                    for (j = 1; j <= i; j++)
                        elements[(i - 1)*numColumns + j - 1] = elements[i*numColumns + j] + g*pTmp[j];
                }

                elements[numColumns*numColumns - 1] = 1.0/w;
                for (i = 1; i <= numColumns - 1; i++)
                    elements[(numColumns - 1)*numColumns + i - 1] = pTmp[i];
            }

            // 行列调整
            for (i = 0; i <= numColumns - 2; i++)
                for (j = i + 1; j <= numColumns - 1; j++)
                    elements[i*numColumns + j] = elements[j*numColumns + i];

            return true;
        }

        /**
         * 托伯利兹矩阵求逆的埃兰特方法
         * 
         * @return bool型，求逆是否成功
         */

        public bool InvertTrench()
        {
            int i, j, k;
            double a, s;

            // 上三角元素
            double[] t = new double[numColumns];
            // 下三角元素
            double[] tt = new double[numColumns];

            // 上、下三角元素赋值
            for (i = 0; i < numColumns; ++i)
            {
                t[i] = GetElement(0, i);
                tt[i] = GetElement(i, 0);
            }

            // 临时缓冲区
            double[] c = new double[numColumns];
            double[] r = new double[numColumns];
            double[] p = new double[numColumns];

            // 非Toeplitz矩阵，返回
            if (t[0] == 0.0)
            {
                return false;
            }

            a = t[0];
            c[0] = tt[1]/t[0];
            r[0] = t[1]/t[0];

            for (k = 0; k <= numColumns - 3; k++)
            {
                s = 0.0;
                for (j = 1; j <= k + 1; j++)
                    s = s + c[k + 1 - j]*tt[j];

                s = (s - tt[k + 2])/a;
                for (i = 0; i <= k; i++)
                    p[i] = c[i] + s*r[k - i];

                c[k + 1] = -s;
                s = 0.0;
                for (j = 1; j <= k + 1; j++)
                    s = s + r[k + 1 - j]*t[j];

                s = (s - t[k + 2])/a;
                for (i = 0; i <= k; i++)
                {
                    r[i] = r[i] + s*c[k - i];
                    c[k - i] = p[k - i];
                }

                r[k + 1] = -s;
                a = 0.0;
                for (j = 1; j <= k + 2; j++)
                    a = a + t[j]*c[j - 1];

                a = t[0] - a;

                // 求解失败
                if (a == 0.0)
                {
                    return false;
                }
            }

            elements[0] = 1.0/a;
            for (i = 0; i <= numColumns - 2; i++)
            {
                k = i + 1;
                j = (i + 1)*numColumns;
                elements[k] = -r[i]/a;
                elements[j] = -c[i]/a;
            }

            for (i = 0; i <= numColumns - 2; i++)
            {
                for (j = 0; j <= numColumns - 2; j++)
                {
                    k = (i + 1)*numColumns + j + 1;
                    elements[k] = elements[i*numColumns + j] - c[i]*elements[j + 1];
                    elements[k] = elements[k] + c[numColumns - j - 2]*elements[numColumns - i - 1];
                }
            }

            return true;
        }

        /**
         * 求行列式值的全选主元高斯消去法
         * 
         * @return double型，行列式的值
         */

        public double ComputeDetGauss()
        {
            int i, j, k, nis = 0, js = 0, l, u, v;
            double f, det, q, d;

            // 初值
            f = 1.0;
            det = 1.0;

            // 消元
            for (k = 0; k <= numColumns - 2; k++)
            {
                q = 0.0;
                for (i = k; i <= numColumns - 1; i++)
                {
                    for (j = k; j <= numColumns - 1; j++)
                    {
                        l = i*numColumns + j;
                        d = System.Math.Abs(elements[l]);
                        if (d > q)
                        {
                            q = d;
                            nis = i;
                            js = j;
                        }
                    }
                }

                if (q == 0.0)
                {
                    det = 0.0;
                    return (det);
                }

                if (nis != k)
                {
                    f = -f;
                    for (j = k; j <= numColumns - 1; j++)
                    {
                        u = k*numColumns + j;
                        v = nis*numColumns + j;
                        d = elements[u];
                        elements[u] = elements[v];
                        elements[v] = d;
                    }
                }

                if (js != k)
                {
                    f = -f;
                    for (i = k; i <= numColumns - 1; i++)
                    {
                        u = i*numColumns + js;
                        v = i*numColumns + k;
                        d = elements[u];
                        elements[u] = elements[v];
                        elements[v] = d;
                    }
                }

                l = k*numColumns + k;
                det = det*elements[l];
                for (i = k + 1; i <= numColumns - 1; i++)
                {
                    d = elements[i*numColumns + k]/elements[l];
                    for (j = k + 1; j <= numColumns - 1; j++)
                    {
                        u = i*numColumns + j;
                        elements[u] = elements[u] - d*elements[k*numColumns + j];
                    }
                }
            }

            // 求值
            det = f*det*elements[numColumns*numColumns - 1];

            return (det);
        }

        /**
         * 求矩阵秩的全选主元高斯消去法
         * 
         * @return int型，矩阵的秩
         */

        public int ComputeRankGauss()
        {
            int i, j, k, nn, nis = 0, js = 0, l, ll, u, v;
            double q, d;

            // 秩小于等于行列数
            nn = numRows;
            if (numRows >= numColumns)
                nn = numColumns;

            k = 0;

            // 消元求解
            for (l = 0; l <= nn - 1; l++)
            {
                q = 0.0;
                for (i = l; i <= numRows - 1; i++)
                {
                    for (j = l; j <= numColumns - 1; j++)
                    {
                        ll = i*numColumns + j;
                        d = System.Math.Abs(elements[ll]);
                        if (d > q)
                        {
                            q = d;
                            nis = i;
                            js = j;
                        }
                    }
                }

                if (q == 0.0)
                    return (k);

                k = k + 1;
                if (nis != l)
                {
                    for (j = l; j <= numColumns - 1; j++)
                    {
                        u = l*numColumns + j;
                        v = nis*numColumns + j;
                        d = elements[u];
                        elements[u] = elements[v];
                        elements[v] = d;
                    }
                }
                if (js != l)
                {
                    for (i = l; i <= numRows - 1; i++)
                    {
                        u = i*numColumns + js;
                        v = i*numColumns + l;
                        d = elements[u];
                        elements[u] = elements[v];
                        elements[v] = d;
                    }
                }

                ll = l*numColumns + l;
                for (i = l + 1; i <= numColumns - 1; i++)
                {
                    d = elements[i*numColumns + l]/elements[ll];
                    for (j = l + 1; j <= numColumns - 1; j++)
                    {
                        u = i*numColumns + j;
                        elements[u] = elements[u] - d*elements[l*numColumns + j];
                    }
                }
            }

            return (k);
        }

        /**
         * 对称正定矩阵的乔里斯基分解与行列式的求值
         * 
         * @param realDetValue - 返回行列式的值
         * @return bool型，求解是否成功
         */

        public bool ComputeDetCholesky(ref double realDetValue)
        {
            int i, j, k, u, l;
            double d;

            // 不满足求解要求
            if (elements[0] <= 0.0)
                return false;

            // 乔里斯基分解

            elements[0] = System.Math.Sqrt(elements[0]);
            d = elements[0];

            for (i = 1; i <= numColumns - 1; i++)
            {
                u = i*numColumns;
                elements[u] = elements[u]/elements[0];
            }

            for (j = 1; j <= numColumns - 1; j++)
            {
                l = j*numColumns + j;
                for (k = 0; k <= j - 1; k++)
                {
                    u = j*numColumns + k;
                    elements[l] = elements[l] - elements[u]*elements[u];
                }

                if (elements[l] <= 0.0)
                    return false;

                elements[l] = System.Math.Sqrt(elements[l]);
                d = d*elements[l];

                for (i = j + 1; i <= numColumns - 1; i++)
                {
                    u = i*numColumns + j;
                    for (k = 0; k <= j - 1; k++)
                        elements[u] = elements[u] - elements[i*numColumns + k]*elements[j*numColumns + k];

                    elements[u] = elements[u]/elements[l];
                }
            }

            // 行列式求值
            realDetValue = d*d;

            // 下三角矩阵
            for (i = 0; i <= numColumns - 2; i++)
                for (j = i + 1; j <= numColumns - 1; j++)
                    elements[i*numColumns + j] = 0.0;

            return true;
        }

        /**
         * 矩阵的三角分解，分解成功后，原矩阵将成为Q矩阵
         * 
         * @param mtxL - 返回分解后的L矩阵
         * @param mtxU - 返回分解后的U矩阵
         * @return bool型，求解是否成功
         */

        public bool SplitLU(Matrix mtxL, Matrix mtxU)
        {
            int i, j, k, w, v, ll;

            // 初始化结果矩阵
            if (!mtxL.Init(numColumns, numColumns) ||
                !mtxU.Init(numColumns, numColumns))
                return false;

            for (k = 0; k <= numColumns - 2; k++)
            {
                ll = k*numColumns + k;
                if (elements[ll] == 0.0)
                    return false;

                for (i = k + 1; i <= numColumns - 1; i++)
                {
                    w = i*numColumns + k;
                    elements[w] = elements[w]/elements[ll];
                }

                for (i = k + 1; i <= numColumns - 1; i++)
                {
                    w = i*numColumns + k;
                    for (j = k + 1; j <= numColumns - 1; j++)
                    {
                        v = i*numColumns + j;
                        elements[v] = elements[v] - elements[w]*elements[k*numColumns + j];
                    }
                }
            }

            for (i = 0; i <= numColumns - 1; i++)
            {
                for (j = 0; j < i; j++)
                {
                    w = i*numColumns + j;
                    mtxL.elements[w] = elements[w];
                    mtxU.elements[w] = 0.0;
                }

                w = i*numColumns + i;
                mtxL.elements[w] = 1.0;
                mtxU.elements[w] = elements[w];

                for (j = i + 1; j <= numColumns - 1; j++)
                {
                    w = i*numColumns + j;
                    mtxL.elements[w] = 0.0;
                    mtxU.elements[w] = elements[w];
                }
            }

            return true;
        }

        /**
         * 一般实矩阵的QR分解，分解成功后，原矩阵将成为R矩阵
         * 
         * @param mtxQ - 返回分解后的Q矩阵
         * @return bool型，求解是否成功
         */

        public bool SplitQR(Matrix mtxQ)
        {
            int i, j, k, l, nn, p, jj;
            double u, alpha, w, t;

            if (numRows < numColumns)
                return false;

            // 初始化Q矩阵
            if (!mtxQ.Init(numRows, numRows))
                return false;

            // 对角线元素单位化
            for (i = 0; i <= numRows - 1; i++)
            {
                for (j = 0; j <= numRows - 1; j++)
                {
                    l = i*numRows + j;
                    mtxQ.elements[l] = 0.0;
                    if (i == j)
                        mtxQ.elements[l] = 1.0;
                }
            }

            // 开始分解

            nn = numColumns;
            if (numRows == numColumns)
                nn = numRows - 1;

            for (k = 0; k <= nn - 1; k++)
            {
                u = 0.0;
                l = k*numColumns + k;
                for (i = k; i <= numRows - 1; i++)
                {
                    w = System.Math.Abs(elements[i*numColumns + k]);
                    if (w > u)
                        u = w;
                }

                alpha = 0.0;
                for (i = k; i <= numRows - 1; i++)
                {
                    t = elements[i*numColumns + k]/u;
                    alpha = alpha + t*t;
                }

                if (elements[l] > 0.0)
                    u = -u;

                alpha = u*System.Math.Sqrt(alpha);
                if (alpha == 0.0)
                    return false;

                u = System.Math.Sqrt(2.0*alpha*(alpha - elements[l]));
                if ((u + 1.0) != 1.0)
                {
                    elements[l] = (elements[l] - alpha)/u;
                    for (i = k + 1; i <= numRows - 1; i++)
                    {
                        p = i*numColumns + k;
                        elements[p] = elements[p]/u;
                    }

                    for (j = 0; j <= numRows - 1; j++)
                    {
                        t = 0.0;
                        for (jj = k; jj <= numRows - 1; jj++)
                            t = t + elements[jj*numColumns + k]*mtxQ.elements[jj*numRows + j];

                        for (i = k; i <= numRows - 1; i++)
                        {
                            p = i*numRows + j;
                            mtxQ.elements[p] = mtxQ.elements[p] - 2.0*t*elements[i*numColumns + k];
                        }
                    }

                    for (j = k + 1; j <= numColumns - 1; j++)
                    {
                        t = 0.0;

                        for (jj = k; jj <= numRows - 1; jj++)
                            t = t + elements[jj*numColumns + k]*elements[jj*numColumns + j];

                        for (i = k; i <= numRows - 1; i++)
                        {
                            p = i*numColumns + j;
                            elements[p] = elements[p] - 2.0*t*elements[i*numColumns + k];
                        }
                    }

                    elements[l] = alpha;
                    for (i = k + 1; i <= numRows - 1; i++)
                        elements[i*numColumns + k] = 0.0;
                }
            }

            // 调整元素
            for (i = 0; i <= numRows - 2; i++)
            {
                for (j = i + 1; j <= numRows - 1; j++)
                {
                    p = i*numRows + j;
                    l = j*numRows + i;
                    t = mtxQ.elements[p];
                    mtxQ.elements[p] = mtxQ.elements[l];
                    mtxQ.elements[l] = t;
                }
            }

            return true;
        }

        /**
         * 一般实矩阵的奇异值分解，分解成功后，原矩阵对角线元素就是矩阵的奇异值
         * 
         * @param mtxU - 返回分解后的U矩阵
         * @param mtxV - 返回分解后的V矩阵
         * @param eps - 计算精度
         * @return bool型，求解是否成功
         */

        public bool SplitUV(Matrix mtxU, Matrix mtxV, double eps)
        {
            int i, j, k, l, it, ll, kk, ix, iy, mm, nn, iz, m1, ks;
            double d, dd, t, sm, sm1, em1, sk, ek, b, c, shh;
            double[] fg = new double[2];
            double[] cs = new double[2];

            int m = numRows;
            int n = numColumns;

            // 初始化U, V矩阵
            if (!mtxU.Init(m, m) || !mtxV.Init(n, n))
                return false;

            // 临时缓冲区
            int ka = System.Math.Max(m, n) + 1;
            double[] s = new double[ka];
            double[] e = new double[ka];
            double[] w = new double[ka];

            // 指定迭代次数为60
            it = 60;
            k = n;

            if (m - 1 < n)
                k = m - 1;

            l = m;
            if (n - 2 < m)
                l = n - 2;
            if (l < 0)
                l = 0;

            // 循环迭代计算
            ll = k;
            if (l > k)
                ll = l;
            if (ll >= 1)
            {
                for (kk = 1; kk <= ll; kk++)
                {
                    if (kk <= k)
                    {
                        d = 0.0;
                        for (i = kk; i <= m; i++)
                        {
                            ix = (i - 1)*n + kk - 1;
                            d = d + elements[ix]*elements[ix];
                        }

                        s[kk - 1] = System.Math.Sqrt(d);
                        if (s[kk - 1] != 0.0)
                        {
                            ix = (kk - 1)*n + kk - 1;
                            if (elements[ix] != 0.0)
                            {
                                s[kk - 1] = System.Math.Abs(s[kk - 1]);
                                if (elements[ix] < 0.0)
                                    s[kk - 1] = -s[kk - 1];
                            }

                            for (i = kk; i <= m; i++)
                            {
                                iy = (i - 1)*n + kk - 1;
                                elements[iy] = elements[iy]/s[kk - 1];
                            }

                            elements[ix] = 1.0 + elements[ix];
                        }

                        s[kk - 1] = -s[kk - 1];
                    }

                    if (n >= kk + 1)
                    {
                        for (j = kk + 1; j <= n; j++)
                        {
                            if ((kk <= k) && (s[kk - 1] != 0.0))
                            {
                                d = 0.0;
                                for (i = kk; i <= m; i++)
                                {
                                    ix = (i - 1)*n + kk - 1;
                                    iy = (i - 1)*n + j - 1;
                                    d = d + elements[ix]*elements[iy];
                                }

                                d = -d/elements[(kk - 1)*n + kk - 1];
                                for (i = kk; i <= m; i++)
                                {
                                    ix = (i - 1)*n + j - 1;
                                    iy = (i - 1)*n + kk - 1;
                                    elements[ix] = elements[ix] + d*elements[iy];
                                }
                            }

                            e[j - 1] = elements[(kk - 1)*n + j - 1];
                        }
                    }

                    if (kk <= k)
                    {
                        for (i = kk; i <= m; i++)
                        {
                            ix = (i - 1)*m + kk - 1;
                            iy = (i - 1)*n + kk - 1;
                            mtxU.elements[ix] = elements[iy];
                        }
                    }

                    if (kk <= l)
                    {
                        d = 0.0;
                        for (i = kk + 1; i <= n; i++)
                            d = d + e[i - 1]*e[i - 1];

                        e[kk - 1] = System.Math.Sqrt(d);
                        if (e[kk - 1] != 0.0)
                        {
                            if (e[kk] != 0.0)
                            {
                                e[kk - 1] = System.Math.Abs(e[kk - 1]);
                                if (e[kk] < 0.0)
                                    e[kk - 1] = -e[kk - 1];
                            }

                            for (i = kk + 1; i <= n; i++)
                                e[i - 1] = e[i - 1]/e[kk - 1];

                            e[kk] = 1.0 + e[kk];
                        }

                        e[kk - 1] = -e[kk - 1];
                        if ((kk + 1 <= m) && (e[kk - 1] != 0.0))
                        {
                            for (i = kk + 1; i <= m; i++)
                                w[i - 1] = 0.0;

                            for (j = kk + 1; j <= n; j++)
                                for (i = kk + 1; i <= m; i++)
                                    w[i - 1] = w[i - 1] + e[j - 1]*elements[(i - 1)*n + j - 1];

                            for (j = kk + 1; j <= n; j++)
                            {
                                for (i = kk + 1; i <= m; i++)
                                {
                                    ix = (i - 1)*n + j - 1;
                                    elements[ix] = elements[ix] - w[i - 1]*e[j - 1]/e[kk];
                                }
                            }
                        }

                        for (i = kk + 1; i <= n; i++)
                            mtxV.elements[(i - 1)*n + kk - 1] = e[i - 1];
                    }
                }
            }

            mm = n;
            if (m + 1 < n)
                mm = m + 1;
            if (k < n)
                s[k] = elements[k*n + k];
            if (m < mm)
                s[mm - 1] = 0.0;
            if (l + 1 < mm)
                e[l] = elements[l*n + mm - 1];

            e[mm - 1] = 0.0;
            nn = m;
            if (m > n)
                nn = n;
            if (nn >= k + 1)
            {
                for (j = k + 1; j <= nn; j++)
                {
                    for (i = 1; i <= m; i++)
                        mtxU.elements[(i - 1)*m + j - 1] = 0.0;
                    mtxU.elements[(j - 1)*m + j - 1] = 1.0;
                }
            }

            if (k >= 1)
            {
                for (ll = 1; ll <= k; ll++)
                {
                    kk = k - ll + 1;
                    iz = (kk - 1)*m + kk - 1;
                    if (s[kk - 1] != 0.0)
                    {
                        if (nn >= kk + 1)
                        {
                            for (j = kk + 1; j <= nn; j++)
                            {
                                d = 0.0;
                                for (i = kk; i <= m; i++)
                                {
                                    ix = (i - 1)*m + kk - 1;
                                    iy = (i - 1)*m + j - 1;
                                    d = d + mtxU.elements[ix]*mtxU.elements[iy]/mtxU.elements[iz];
                                }

                                d = -d;
                                for (i = kk; i <= m; i++)
                                {
                                    ix = (i - 1)*m + j - 1;
                                    iy = (i - 1)*m + kk - 1;
                                    mtxU.elements[ix] = mtxU.elements[ix] + d*mtxU.elements[iy];
                                }
                            }
                        }

                        for (i = kk; i <= m; i++)
                        {
                            ix = (i - 1)*m + kk - 1;
                            mtxU.elements[ix] = -mtxU.elements[ix];
                        }

                        mtxU.elements[iz] = 1.0 + mtxU.elements[iz];
                        if (kk - 1 >= 1)
                        {
                            for (i = 1; i <= kk - 1; i++)
                                mtxU.elements[(i - 1)*m + kk - 1] = 0.0;
                        }
                    }
                    else
                    {
                        for (i = 1; i <= m; i++)
                            mtxU.elements[(i - 1)*m + kk - 1] = 0.0;
                        mtxU.elements[(kk - 1)*m + kk - 1] = 1.0;
                    }
                }
            }

            for (ll = 1; ll <= n; ll++)
            {
                kk = n - ll + 1;
                iz = kk*n + kk - 1;

                if ((kk <= l) && (e[kk - 1] != 0.0))
                {
                    for (j = kk + 1; j <= n; j++)
                    {
                        d = 0.0;
                        for (i = kk + 1; i <= n; i++)
                        {
                            ix = (i - 1)*n + kk - 1;
                            iy = (i - 1)*n + j - 1;
                            d = d + mtxV.elements[ix]*mtxV.elements[iy]/mtxV.elements[iz];
                        }

                        d = -d;
                        for (i = kk + 1; i <= n; i++)
                        {
                            ix = (i - 1)*n + j - 1;
                            iy = (i - 1)*n + kk - 1;
                            mtxV.elements[ix] = mtxV.elements[ix] + d*mtxV.elements[iy];
                        }
                    }
                }

                for (i = 1; i <= n; i++)
                    mtxV.elements[(i - 1)*n + kk - 1] = 0.0;

                mtxV.elements[iz - n] = 1.0;
            }

            for (i = 1; i <= m; i++)
                for (j = 1; j <= n; j++)
                    elements[(i - 1)*n + j - 1] = 0.0;

            m1 = mm;
            it = 60;
            while (true)
            {
                if (mm == 0)
                {
                    ppp(elements, e, s, mtxV.elements, m, n);
                    return true;
                }
                if (it == 0)
                {
                    ppp(elements, e, s, mtxV.elements, m, n);
                    return false;
                }

                kk = mm - 1;
                while ((kk != 0) && (System.Math.Abs(e[kk - 1]) != 0.0))
                {
                    d = System.Math.Abs(s[kk - 1]) + System.Math.Abs(s[kk]);
                    dd = System.Math.Abs(e[kk - 1]);
                    if (dd > eps*d)
                        kk = kk - 1;
                    else
                        e[kk - 1] = 0.0;
                }

                if (kk == mm - 1)
                {
                    kk = kk + 1;
                    if (s[kk - 1] < 0.0)
                    {
                        s[kk - 1] = -s[kk - 1];
                        for (i = 1; i <= n; i++)
                        {
                            ix = (i - 1)*n + kk - 1;
                            mtxV.elements[ix] = -mtxV.elements[ix];
                        }
                    }

                    while ((kk != m1) && (s[kk - 1] < s[kk]))
                    {
                        d = s[kk - 1];
                        s[kk - 1] = s[kk];
                        s[kk] = d;
                        if (kk < n)
                        {
                            for (i = 1; i <= n; i++)
                            {
                                ix = (i - 1)*n + kk - 1;
                                iy = (i - 1)*n + kk;
                                d = mtxV.elements[ix];
                                mtxV.elements[ix] = mtxV.elements[iy];
                                mtxV.elements[iy] = d;
                            }
                        }

                        if (kk < m)
                        {
                            for (i = 1; i <= m; i++)
                            {
                                ix = (i - 1)*m + kk - 1;
                                iy = (i - 1)*m + kk;
                                d = mtxU.elements[ix];
                                mtxU.elements[ix] = mtxU.elements[iy];
                                mtxU.elements[iy] = d;
                            }
                        }

                        kk = kk + 1;
                    }

                    it = 60;
                    mm = mm - 1;
                }
                else
                {
                    ks = mm;
                    while ((ks > kk) && (System.Math.Abs(s[ks - 1]) != 0.0))
                    {
                        d = 0.0;
                        if (ks != mm)
                            d = d + System.Math.Abs(e[ks - 1]);
                        if (ks != kk + 1)
                            d = d + System.Math.Abs(e[ks - 2]);

                        dd = System.Math.Abs(s[ks - 1]);
                        if (dd > eps*d)
                            ks = ks - 1;
                        else
                            s[ks - 1] = 0.0;
                    }

                    if (ks == kk)
                    {
                        kk = kk + 1;
                        d = System.Math.Abs(s[mm - 1]);
                        t = System.Math.Abs(s[mm - 2]);
                        if (t > d)
                            d = t;

                        t = System.Math.Abs(e[mm - 2]);
                        if (t > d)
                            d = t;

                        t = System.Math.Abs(s[kk - 1]);
                        if (t > d)
                            d = t;

                        t = System.Math.Abs(e[kk - 1]);
                        if (t > d)
                            d = t;

                        sm = s[mm - 1]/d;
                        sm1 = s[mm - 2]/d;
                        em1 = e[mm - 2]/d;
                        sk = s[kk - 1]/d;
                        ek = e[kk - 1]/d;
                        b = ((sm1 + sm)*(sm1 - sm) + em1*em1)/2.0;
                        c = sm*em1;
                        c = c*c;
                        shh = 0.0;

                        if ((b != 0.0) || (c != 0.0))
                        {
                            shh = System.Math.Sqrt(b*b + c);
                            if (b < 0.0)
                                shh = -shh;

                            shh = c/(b + shh);
                        }

                        fg[0] = (sk + sm)*(sk - sm) - shh;
                        fg[1] = sk*ek;
                        for (i = kk; i <= mm - 1; i++)
                        {
                            sss(fg, cs);
                            if (i != kk)
                                e[i - 2] = fg[0];

                            fg[0] = cs[0]*s[i - 1] + cs[1]*e[i - 1];
                            e[i - 1] = cs[0]*e[i - 1] - cs[1]*s[i - 1];
                            fg[1] = cs[1]*s[i];
                            s[i] = cs[0]*s[i];

                            if ((cs[0] != 1.0) || (cs[1] != 0.0))
                            {
                                for (j = 1; j <= n; j++)
                                {
                                    ix = (j - 1)*n + i - 1;
                                    iy = (j - 1)*n + i;
                                    d = cs[0]*mtxV.elements[ix] + cs[1]*mtxV.elements[iy];
                                    mtxV.elements[iy] = -cs[1]*mtxV.elements[ix] + cs[0]*mtxV.elements[iy];
                                    mtxV.elements[ix] = d;
                                }
                            }

                            sss(fg, cs);
                            s[i - 1] = fg[0];
                            fg[0] = cs[0]*e[i - 1] + cs[1]*s[i];
                            s[i] = -cs[1]*e[i - 1] + cs[0]*s[i];
                            fg[1] = cs[1]*e[i];
                            e[i] = cs[0]*e[i];

                            if (i < m)
                            {
                                if ((cs[0] != 1.0) || (cs[1] != 0.0))
                                {
                                    for (j = 1; j <= m; j++)
                                    {
                                        ix = (j - 1)*m + i - 1;
                                        iy = (j - 1)*m + i;
                                        d = cs[0]*mtxU.elements[ix] + cs[1]*mtxU.elements[iy];
                                        mtxU.elements[iy] = -cs[1]*mtxU.elements[ix] + cs[0]*mtxU.elements[iy];
                                        mtxU.elements[ix] = d;
                                    }
                                }
                            }
                        }

                        e[mm - 2] = fg[0];
                        it = it - 1;
                    }
                    else
                    {
                        if (ks == mm)
                        {
                            kk = kk + 1;
                            fg[1] = e[mm - 2];
                            e[mm - 2] = 0.0;
                            for (ll = kk; ll <= mm - 1; ll++)
                            {
                                i = mm + kk - ll - 1;
                                fg[0] = s[i - 1];
                                sss(fg, cs);
                                s[i - 1] = fg[0];
                                if (i != kk)
                                {
                                    fg[1] = -cs[1]*e[i - 2];
                                    e[i - 2] = cs[0]*e[i - 2];
                                }

                                if ((cs[0] != 1.0) || (cs[1] != 0.0))
                                {
                                    for (j = 1; j <= n; j++)
                                    {
                                        ix = (j - 1)*n + i - 1;
                                        iy = (j - 1)*n + mm - 1;
                                        d = cs[0]*mtxV.elements[ix] + cs[1]*mtxV.elements[iy];
                                        mtxV.elements[iy] = -cs[1]*mtxV.elements[ix] + cs[0]*mtxV.elements[iy];
                                        mtxV.elements[ix] = d;
                                    }
                                }
                            }
                        }
                        else
                        {
                            kk = ks + 1;
                            fg[1] = e[kk - 2];
                            e[kk - 2] = 0.0;
                            for (i = kk; i <= mm; i++)
                            {
                                fg[0] = s[i - 1];
                                sss(fg, cs);
                                s[i - 1] = fg[0];
                                fg[1] = -cs[1]*e[i - 1];
                                e[i - 1] = cs[0]*e[i - 1];
                                if ((cs[0] != 1.0) || (cs[1] != 0.0))
                                {
                                    for (j = 1; j <= m; j++)
                                    {
                                        ix = (j - 1)*m + i - 1;
                                        iy = (j - 1)*m + kk - 2;
                                        d = cs[0]*mtxU.elements[ix] + cs[1]*mtxU.elements[iy];
                                        mtxU.elements[iy] = -cs[1]*mtxU.elements[ix] + cs[0]*mtxU.elements[iy];
                                        mtxU.elements[ix] = d;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /**
         * 内部函数，由SplitUV函数调用
         */

        private void ppp(double[] a, double[] e, double[] s, double[] v, int m, int n)
        {
            int i, j, p, q;
            double d;

            if (m >= n)
                i = n;
            else
                i = m;

            for (j = 1; j <= i - 1; j++)
            {
                a[(j - 1)*n + j - 1] = s[j - 1];
                a[(j - 1)*n + j] = e[j - 1];
            }

            a[(i - 1)*n + i - 1] = s[i - 1];
            if (m < n)
                a[(i - 1)*n + i] = e[i - 1];

            for (i = 1; i <= n - 1; i++)
            {
                for (j = i + 1; j <= n; j++)
                {
                    p = (i - 1)*n + j - 1;
                    q = (j - 1)*n + i - 1;
                    d = v[p];
                    v[p] = v[q];
                    v[q] = d;
                }
            }
        }

        /**
         * 内部函数，由SplitUV函数调用
         */

        private void sss(double[] fg, double[] cs)
        {
            double r, d;

            if ((System.Math.Abs(fg[0]) + System.Math.Abs(fg[1])) == 0.0)
            {
                cs[0] = 1.0;
                cs[1] = 0.0;
                d = 0.0;
            }
            else
            {
                d = System.Math.Sqrt(fg[0]*fg[0] + fg[1]*fg[1]);
                if (System.Math.Abs(fg[0]) > System.Math.Abs(fg[1]))
                {
                    d = System.Math.Abs(d);
                    if (fg[0] < 0.0)
                        d = -d;
                }
                if (System.Math.Abs(fg[1]) >= System.Math.Abs(fg[0]))
                {
                    d = System.Math.Abs(d);
                    if (fg[1] < 0.0)
                        d = -d;
                }

                cs[0] = fg[0]/d;
                cs[1] = fg[1]/d;
            }

            r = 1.0;
            if (System.Math.Abs(fg[0]) > System.Math.Abs(fg[1]))
                r = cs[1];
            else if (cs[0] != 0.0)
                r = 1.0/cs[0];

            fg[0] = d;
            fg[1] = r;
        }

        /**
         * 求广义逆的奇异值分解法，分解成功后，原矩阵对角线元素就是矩阵的奇异值
         * 
         * @param mtxAP - 返回原矩阵的广义逆矩阵
         * @param mtxU - 返回分解后的U矩阵
         * @param mtxV - 返回分解后的V矩阵
         * @param eps - 计算精度
         * @return bool型，求解是否成功
         */

        public bool InvertUV(Matrix mtxAP, Matrix mtxU, Matrix mtxV, double eps)
        {
            int i, j, k, l, t, p, q, f;

            // 调用奇异值分解
            if (!SplitUV(mtxU, mtxV, eps))
                return false;

            int m = numRows;
            int n = numColumns;

            // 初始化广义逆矩阵
            if (!mtxAP.Init(n, m))
                return false;

            // 计算广义逆矩阵

            j = n;
            if (m < n)
                j = m;
            j = j - 1;
            k = 0;
            while ((k <= j) && (elements[k*n + k] != 0.0))
                k = k + 1;

            k = k - 1;
            for (i = 0; i <= n - 1; i++)
            {
                for (j = 0; j <= m - 1; j++)
                {
                    t = i*m + j;
                    mtxAP.elements[t] = 0.0;
                    for (l = 0; l <= k; l++)
                    {
                        f = l*n + i;
                        p = j*m + l;
                        q = l*n + l;
                        mtxAP.elements[t] = mtxAP.elements[t] + mtxV.elements[f]*mtxU.elements[p]/elements[q];
                    }
                }
            }

            return true;
        }

        /**
         * 约化对称矩阵为对称三对角阵的豪斯荷尔德变换法
         * 
         * @param mtxQ - 返回豪斯荷尔德变换的乘积矩阵Q
         * @param mtxT - 返回求得的对称三对角阵
         * @param dblB - 一维数组，长度为矩阵的阶数，返回对称三对角阵的主对角线元素
         * @param dblC - 一维数组，长度为矩阵的阶数，前n-1个元素返回对称三对角阵的
         *               次对角线元素
         * @return bool型，求解是否成功
         */

        public bool MakeSymTri(Matrix mtxQ, Matrix mtxT, double[] dblB, double[] dblC)
        {
            int i, j, k, u;
            double h, f, g, h2;

            // 初始化矩阵Q和T
            if (!mtxQ.Init(numColumns, numColumns) ||
                !mtxT.Init(numColumns, numColumns))
                return false;

            if (dblB == null || dblC == null)
                return false;

            for (i = 0; i <= numColumns - 1; i++)
            {
                for (j = 0; j <= numColumns - 1; j++)
                {
                    u = i*numColumns + j;
                    mtxQ.elements[u] = elements[u];
                }
            }

            for (i = numColumns - 1; i >= 1; i--)
            {
                h = 0.0;
                if (i > 1)
                {
                    for (k = 0; k <= i - 1; k++)
                    {
                        u = i*numColumns + k;
                        h = h + mtxQ.elements[u]*mtxQ.elements[u];
                    }
                }

                if (h == 0.0)
                {
                    dblC[i] = 0.0;
                    if (i == 1)
                        dblC[i] = mtxQ.elements[i*numColumns + i - 1];
                    dblB[i] = 0.0;
                }
                else
                {
                    dblC[i] = System.Math.Sqrt(h);
                    u = i*numColumns + i - 1;
                    if (mtxQ.elements[u] > 0.0)
                        dblC[i] = -dblC[i];

                    h = h - mtxQ.elements[u]*dblC[i];
                    mtxQ.elements[u] = mtxQ.elements[u] - dblC[i];
                    f = 0.0;
                    for (j = 0; j <= i - 1; j++)
                    {
                        mtxQ.elements[j*numColumns + i] = mtxQ.elements[i*numColumns + j]/h;
                        g = 0.0;
                        for (k = 0; k <= j; k++)
                            g = g + mtxQ.elements[j*numColumns + k]*mtxQ.elements[i*numColumns + k];

                        if (j + 1 <= i - 1)
                            for (k = j + 1; k <= i - 1; k++)
                                g = g + mtxQ.elements[k*numColumns + j]*mtxQ.elements[i*numColumns + k];

                        dblC[j] = g/h;
                        f = f + g*mtxQ.elements[j*numColumns + i];
                    }

                    h2 = f/(h + h);
                    for (j = 0; j <= i - 1; j++)
                    {
                        f = mtxQ.elements[i*numColumns + j];
                        g = dblC[j] - h2*f;
                        dblC[j] = g;
                        for (k = 0; k <= j; k++)
                        {
                            u = j*numColumns + k;
                            mtxQ.elements[u] = mtxQ.elements[u] - f*dblC[k] - g*mtxQ.elements[i*numColumns + k];
                        }
                    }

                    dblB[i] = h;
                }
            }

            for (i = 0; i <= numColumns - 2; i++)
                dblC[i] = dblC[i + 1];

            dblC[numColumns - 1] = 0.0;
            dblB[0] = 0.0;
            for (i = 0; i <= numColumns - 1; i++)
            {
                if ((dblB[i] != (double) 0.0) && (i - 1 >= 0))
                {
                    for (j = 0; j <= i - 1; j++)
                    {
                        g = 0.0;
                        for (k = 0; k <= i - 1; k++)
                            g = g + mtxQ.elements[i*numColumns + k]*mtxQ.elements[k*numColumns + j];

                        for (k = 0; k <= i - 1; k++)
                        {
                            u = k*numColumns + j;
                            mtxQ.elements[u] = mtxQ.elements[u] - g*mtxQ.elements[k*numColumns + i];
                        }
                    }
                }

                u = i*numColumns + i;
                dblB[i] = mtxQ.elements[u];
                mtxQ.elements[u] = 1.0;
                if (i - 1 >= 0)
                {
                    for (j = 0; j <= i - 1; j++)
                    {
                        mtxQ.elements[i*numColumns + j] = 0.0;
                        mtxQ.elements[j*numColumns + i] = 0.0;
                    }
                }
            }

            // 构造对称三对角矩阵
            for (i = 0; i < numColumns; ++i)
            {
                for (j = 0; j < numColumns; ++j)
                {
                    mtxT.SetElement(i, j, 0);
                    k = i - j;
                    if (k == 0)
                        mtxT.SetElement(i, j, dblB[j]);
                    else if (k == 1)
                        mtxT.SetElement(i, j, dblC[j]);
                    else if (k == -1)
                        mtxT.SetElement(i, j, dblC[i]);
                }
            }

            return true;
        }

        /**
         * 实对称三对角阵的全部特征值与特征向量的计算
         * 
         * @param dblB - 一维数组，长度为矩阵的阶数，传入对称三对角阵的主对角线元素；
         *			     返回时存放全部特征值。
         * @param dblC - 一维数组，长度为矩阵的阶数，前n-1个元素传入对称三对角阵的
         *               次对角线元素
         * @param mtxQ - 如果传入单位矩阵，则返回实对称三对角阵的特征值向量矩阵；
         *			     如果传入MakeSymTri函数求得的矩阵A的豪斯荷尔德变换的乘积
         *               矩阵Q，则返回矩阵A的特征值向量矩阵。其中第i列为与数组dblB
         *               中第j个特征值对应的特征向量。
         * @param nMaxIt - 迭代次数
         * @param eps - 计算精度
         * @return bool型，求解是否成功
         */

        public bool ComputeEvSymTri(double[] dblB, double[] dblC, Matrix mtxQ, int nMaxIt, double eps)
        {
            int i, j, k, m, it, u, v;
            double d, f, h, g, p, r, e, s;

            // 初值
            int n = mtxQ.GetNumColumns();
            dblC[n - 1] = 0.0;
            d = 0.0;
            f = 0.0;

            // 迭代计算

            for (j = 0; j <= n - 1; j++)
            {
                it = 0;
                h = eps*(System.Math.Abs(dblB[j]) + System.Math.Abs(dblC[j]));
                if (h > d)
                    d = h;

                m = j;
                while ((m <= n - 1) && (System.Math.Abs(dblC[m]) > d))
                    m = m + 1;

                if (m != j)
                {
                    do
                    {
                        if (it == nMaxIt)
                            return false;

                        it = it + 1;
                        g = dblB[j];
                        p = (dblB[j + 1] - g)/(2.0*dblC[j]);
                        r = System.Math.Sqrt(p*p + 1.0);
                        if (p >= 0.0)
                            dblB[j] = dblC[j]/(p + r);
                        else
                            dblB[j] = dblC[j]/(p - r);

                        h = g - dblB[j];
                        for (i = j + 1; i <= n - 1; i++)
                            dblB[i] = dblB[i] - h;

                        f = f + h;
                        p = dblB[m];
                        e = 1.0;
                        s = 0.0;
                        for (i = m - 1; i >= j; i--)
                        {
                            g = e*dblC[i];
                            h = e*p;
                            if (System.Math.Abs(p) >= System.Math.Abs(dblC[i]))
                            {
                                e = dblC[i]/p;
                                r = System.Math.Sqrt(e*e + 1.0);
                                dblC[i + 1] = s*p*r;
                                s = e/r;
                                e = 1.0/r;
                            }
                            else
                            {
                                e = p/dblC[i];
                                r = System.Math.Sqrt(e*e + 1.0);
                                dblC[i + 1] = s*dblC[i]*r;
                                s = 1.0/r;
                                e = e/r;
                            }

                            p = e*dblB[i] - s*g;
                            dblB[i + 1] = h + s*(e*g + s*dblB[i]);
                            for (k = 0; k <= n - 1; k++)
                            {
                                u = k*n + i + 1;
                                v = u - 1;
                                h = mtxQ.elements[u];
                                mtxQ.elements[u] = s*mtxQ.elements[v] + e*h;
                                mtxQ.elements[v] = e*mtxQ.elements[v] - s*h;
                            }
                        }

                        dblC[j] = s*p;
                        dblB[j] = e*p;
                    } while (System.Math.Abs(dblC[j]) > d);
                }

                dblB[j] = dblB[j] + f;
            }

            for (i = 0; i <= n - 1; i++)
            {
                k = i;
                p = dblB[i];
                if (i + 1 <= n - 1)
                {
                    j = i + 1;
                    while ((j <= n - 1) && (dblB[j] <= p))
                    {
                        k = j;
                        p = dblB[j];
                        j = j + 1;
                    }
                }

                if (k != i)
                {
                    dblB[k] = dblB[i];
                    dblB[i] = p;
                    for (j = 0; j <= n - 1; j++)
                    {
                        u = j*n + i;
                        v = j*n + k;
                        p = mtxQ.elements[u];
                        mtxQ.elements[u] = mtxQ.elements[v];
                        mtxQ.elements[v] = p;
                    }
                }
            }

            return true;
        }

        /**
         * 约化一般实矩阵为赫申伯格矩阵的初等相似变换法
         */

        public void MakeHberg()
        {
            int i = 0, j, k, u, v;
            double d, t;

            for (k = 1; k <= numColumns - 2; k++)
            {
                d = 0.0;
                for (j = k; j <= numColumns - 1; j++)
                {
                    u = j*numColumns + k - 1;
                    t = elements[u];
                    if (System.Math.Abs(t) > System.Math.Abs(d))
                    {
                        d = t;
                        i = j;
                    }
                }

                if (d != 0.0)
                {
                    if (i != k)
                    {
                        for (j = k - 1; j <= numColumns - 1; j++)
                        {
                            u = i*numColumns + j;
                            v = k*numColumns + j;
                            t = elements[u];
                            elements[u] = elements[v];
                            elements[v] = t;
                        }

                        for (j = 0; j <= numColumns - 1; j++)
                        {
                            u = j*numColumns + i;
                            v = j*numColumns + k;
                            t = elements[u];
                            elements[u] = elements[v];
                            elements[v] = t;
                        }
                    }

                    for (i = k + 1; i <= numColumns - 1; i++)
                    {
                        u = i*numColumns + k - 1;
                        t = elements[u]/d;
                        elements[u] = 0.0;
                        for (j = k; j <= numColumns - 1; j++)
                        {
                            v = i*numColumns + j;
                            elements[v] = elements[v] - t*elements[k*numColumns + j];
                        }

                        for (j = 0; j <= numColumns - 1; j++)
                        {
                            v = j*numColumns + k;
                            elements[v] = elements[v] + t*elements[j*numColumns + i];
                        }
                    }
                }
            }
        }

        /**
         * 求赫申伯格矩阵全部特征值的QR方法
         * 
         * @param dblU - 一维数组，长度为矩阵的阶数，返回时存放特征值的实部
         * @param dblV - 一维数组，长度为矩阵的阶数，返回时存放特征值的虚部
         * @param nMaxIt - 迭代次数
         * @param eps - 计算精度
         * @return bool型，求解是否成功
         */

        public bool ComputeEvHBerg(double[] dblU, double[] dblV, int nMaxIt, double eps)
        {
            int m, it, i, j, k, l, ii, jj, kk, ll;
            double b, c, w, g, xy, p, q, r, x, s, e, f, z, y;

            int n = numColumns;

            it = 0;
            m = n;
            while (m != 0)
            {
                l = m - 1;
                while ((l > 0) && (System.Math.Abs(elements[l*n + l - 1]) >
                                   eps*
                                   (System.Math.Abs(elements[(l - 1)*n + l - 1]) + System.Math.Abs(elements[l*n + l]))))
                    l = l - 1;

                ii = (m - 1)*n + m - 1;
                jj = (m - 1)*n + m - 2;
                kk = (m - 2)*n + m - 1;
                ll = (m - 2)*n + m - 2;
                if (l == m - 1)
                {
                    dblU[m - 1] = elements[(m - 1)*n + m - 1];
                    dblV[m - 1] = 0.0;
                    m = m - 1;
                    it = 0;
                }
                else if (l == m - 2)
                {
                    b = -(elements[ii] + elements[ll]);
                    c = elements[ii]*elements[ll] - elements[jj]*elements[kk];
                    w = b*b - 4.0*c;
                    y = System.Math.Sqrt(System.Math.Abs(w));
                    if (w > 0.0)
                    {
                        xy = 1.0;
                        if (b < 0.0)
                            xy = -1.0;
                        dblU[m - 1] = (-b - xy*y)/2.0;
                        dblU[m - 2] = c/dblU[m - 1];
                        dblV[m - 1] = 0.0;
                        dblV[m - 2] = 0.0;
                    }
                    else
                    {
                        dblU[m - 1] = -b/2.0;
                        dblU[m - 2] = dblU[m - 1];
                        dblV[m - 1] = y/2.0;
                        dblV[m - 2] = -dblV[m - 1];
                    }

                    m = m - 2;
                    it = 0;
                }
                else
                {
                    if (it >= nMaxIt)
                        return false;

                    it = it + 1;
                    for (j = l + 2; j <= m - 1; j++)
                        elements[j*n + j - 2] = 0.0;
                    for (j = l + 3; j <= m - 1; j++)
                        elements[j*n + j - 3] = 0.0;
                    for (k = l; k <= m - 2; k++)
                    {
                        if (k != l)
                        {
                            p = elements[k*n + k - 1];
                            q = elements[(k + 1)*n + k - 1];
                            r = 0.0;
                            if (k != m - 2)
                                r = elements[(k + 2)*n + k - 1];
                        }
                        else
                        {
                            x = elements[ii] + elements[ll];
                            y = elements[ll]*elements[ii] - elements[kk]*elements[jj];
                            ii = l*n + l;
                            jj = l*n + l + 1;
                            kk = (l + 1)*n + l;
                            ll = (l + 1)*n + l + 1;
                            p = elements[ii]*(elements[ii] - x) + elements[jj]*elements[kk] + y;
                            q = elements[kk]*(elements[ii] + elements[ll] - x);
                            r = elements[kk]*elements[(l + 2)*n + l + 1];
                        }

                        if ((System.Math.Abs(p) + System.Math.Abs(q) + System.Math.Abs(r)) != 0.0)
                        {
                            xy = 1.0;
                            if (p < 0.0)
                                xy = -1.0;
                            s = xy*System.Math.Sqrt(p*p + q*q + r*r);
                            if (k != l)
                                elements[k*n + k - 1] = -s;
                            e = -q/s;
                            f = -r/s;
                            x = -p/s;
                            y = -x - f*r/(p + s);
                            g = e*r/(p + s);
                            z = -x - e*q/(p + s);
                            for (j = k; j <= m - 1; j++)
                            {
                                ii = k*n + j;
                                jj = (k + 1)*n + j;
                                p = x*elements[ii] + e*elements[jj];
                                q = e*elements[ii] + y*elements[jj];
                                r = f*elements[ii] + g*elements[jj];
                                if (k != m - 2)
                                {
                                    kk = (k + 2)*n + j;
                                    p = p + f*elements[kk];
                                    q = q + g*elements[kk];
                                    r = r + z*elements[kk];
                                    elements[kk] = r;
                                }

                                elements[jj] = q;
                                elements[ii] = p;
                            }

                            j = k + 3;
                            if (j >= m - 1)
                                j = m - 1;

                            for (i = l; i <= j; i++)
                            {
                                ii = i*n + k;
                                jj = i*n + k + 1;
                                p = x*elements[ii] + e*elements[jj];
                                q = e*elements[ii] + y*elements[jj];
                                r = f*elements[ii] + g*elements[jj];
                                if (k != m - 2)
                                {
                                    kk = i*n + k + 2;
                                    p = p + f*elements[kk];
                                    q = q + g*elements[kk];
                                    r = r + z*elements[kk];
                                    elements[kk] = r;
                                }

                                elements[jj] = q;
                                elements[ii] = p;
                            }
                        }
                    }
                }
            }

            return true;
        }

        /**
         * 求实对称矩阵特征值与特征向量的雅可比法
         * 
         * @param dblEigenValue - 一维数组，长度为矩阵的阶数，返回时存放特征值
         * @param mtxEigenVector - 返回时存放特征向量矩阵，其中第i列为与数组
         *                         dblEigenValue中第j个特征值对应的特征向量
         * @param nMaxIt - 迭代次数
         * @param eps - 计算精度
         * @return bool型，求解是否成功
         */

        public bool ComputeEvJacobi(double[] dblEigenValue, Matrix mtxEigenVector, int nMaxIt, double eps)
        {
            int i, j, p = 0, q = 0, u, w, t, s, l;
            double fm, cn, sn, omega, x, y, d;

            if (!mtxEigenVector.Init(numColumns, numColumns))
                return false;

            l = 1;
            for (i = 0; i <= numColumns - 1; i++)
            {
                mtxEigenVector.elements[i*numColumns + i] = 1.0;
                for (j = 0; j <= numColumns - 1; j++)
                    if (i != j)
                        mtxEigenVector.elements[i*numColumns + j] = 0.0;
            }

            while (true)
            {
                fm = 0.0;
                for (i = 1; i <= numColumns - 1; i++)
                {
                    for (j = 0; j <= i - 1; j++)
                    {
                        d = System.Math.Abs(elements[i*numColumns + j]);
                        if ((i != j) && (d > fm))
                        {
                            fm = d;
                            p = i;
                            q = j;
                        }
                    }
                }

                if (fm < eps)
                {
                    for (i = 0; i < numColumns; ++i)
                        dblEigenValue[i] = GetElement(i, i);
                    return true;
                }

                if (l > nMaxIt)
                    return false;

                l = l + 1;
                u = p*numColumns + q;
                w = p*numColumns + p;
                t = q*numColumns + p;
                s = q*numColumns + q;
                x = -elements[u];
                y = (elements[s] - elements[w])/2.0;
                omega = x/System.Math.Sqrt(x*x + y*y);

                if (y < 0.0)
                    omega = -omega;

                sn = 1.0 + System.Math.Sqrt(1.0 - omega*omega);
                sn = omega/System.Math.Sqrt(2.0*sn);
                cn = System.Math.Sqrt(1.0 - sn*sn);
                fm = elements[w];
                elements[w] = fm*cn*cn + elements[s]*sn*sn + elements[u]*omega;
                elements[s] = fm*sn*sn + elements[s]*cn*cn - elements[u]*omega;
                elements[u] = 0.0;
                elements[t] = 0.0;
                for (j = 0; j <= numColumns - 1; j++)
                {
                    if ((j != p) && (j != q))
                    {
                        u = p*numColumns + j;
                        w = q*numColumns + j;
                        fm = elements[u];
                        elements[u] = fm*cn + elements[w]*sn;
                        elements[w] = -fm*sn + elements[w]*cn;
                    }
                }

                for (i = 0; i <= numColumns - 1; i++)
                {
                    if ((i != p) && (i != q))
                    {
                        u = i*numColumns + p;
                        w = i*numColumns + q;
                        fm = elements[u];
                        elements[u] = fm*cn + elements[w]*sn;
                        elements[w] = -fm*sn + elements[w]*cn;
                    }
                }

                for (i = 0; i <= numColumns - 1; i++)
                {
                    u = i*numColumns + p;
                    w = i*numColumns + q;
                    fm = mtxEigenVector.elements[u];
                    mtxEigenVector.elements[u] = fm*cn + mtxEigenVector.elements[w]*sn;
                    mtxEigenVector.elements[w] = -fm*sn + mtxEigenVector.elements[w]*cn;
                }
            }
        }

        /**
         * 求实对称矩阵特征值与特征向量的雅可比过关法
         * 
         * @param dblEigenValue - 一维数组，长度为矩阵的阶数，返回时存放特征值
         * @param mtxEigenVector - 返回时存放特征向量矩阵，其中第i列为与数组
         *                         dblEigenValue中第j个特征值对应的特征向量
         * @param eps - 计算精度
         * @return bool型，求解是否成功
         */

        public bool ComputeEvJacobi(double[] dblEigenValue, Matrix mtxEigenVector, double eps)
        {
            int i, j, p, q, u, w, t, s;
            double ff, fm, cn, sn, omega, x, y, d;

            if (!mtxEigenVector.Init(numColumns, numColumns))
                return false;

            for (i = 0; i <= numColumns - 1; i++)
            {
                mtxEigenVector.elements[i*numColumns + i] = 1.0;
                for (j = 0; j <= numColumns - 1; j++)
                    if (i != j)
                        mtxEigenVector.elements[i*numColumns + j] = 0.0;
            }

            ff = 0.0;
            for (i = 1; i <= numColumns - 1; i++)
            {
                for (j = 0; j <= i - 1; j++)
                {
                    d = elements[i*numColumns + j];
                    ff = ff + d*d;
                }
            }

            ff = System.Math.Sqrt(2.0*ff);
            ff = ff/(1.0*numColumns);

            bool nextLoop = false;
            while (true)
            {
                for (i = 1; i <= numColumns - 1; i++)
                {
                    for (j = 0; j <= i - 1; j++)
                    {
                        d = System.Math.Abs(elements[i*numColumns + j]);
                        if (d > ff)
                        {
                            p = i;
                            q = j;

                            u = p*numColumns + q;
                            w = p*numColumns + p;
                            t = q*numColumns + p;
                            s = q*numColumns + q;
                            x = -elements[u];
                            y = (elements[s] - elements[w])/2.0;
                            omega = x/System.Math.Sqrt(x*x + y*y);
                            if (y < 0.0)
                                omega = -omega;

                            sn = 1.0 + System.Math.Sqrt(1.0 - omega*omega);
                            sn = omega/System.Math.Sqrt(2.0*sn);
                            cn = System.Math.Sqrt(1.0 - sn*sn);
                            fm = elements[w];
                            elements[w] = fm*cn*cn + elements[s]*sn*sn + elements[u]*omega;
                            elements[s] = fm*sn*sn + elements[s]*cn*cn - elements[u]*omega;
                            elements[u] = 0.0;
                            elements[t] = 0.0;

                            for (j = 0; j <= numColumns - 1; j++)
                            {
                                if ((j != p) && (j != q))
                                {
                                    u = p*numColumns + j;
                                    w = q*numColumns + j;
                                    fm = elements[u];
                                    elements[u] = fm*cn + elements[w]*sn;
                                    elements[w] = -fm*sn + elements[w]*cn;
                                }
                            }

                            for (i = 0; i <= numColumns - 1; i++)
                            {
                                if ((i != p) && (i != q))
                                {
                                    u = i*numColumns + p;
                                    w = i*numColumns + q;
                                    fm = elements[u];
                                    elements[u] = fm*cn + elements[w]*sn;
                                    elements[w] = -fm*sn + elements[w]*cn;
                                }
                            }

                            for (i = 0; i <= numColumns - 1; i++)
                            {
                                u = i*numColumns + p;
                                w = i*numColumns + q;
                                fm = mtxEigenVector.elements[u];
                                mtxEigenVector.elements[u] = fm*cn + mtxEigenVector.elements[w]*sn;
                                mtxEigenVector.elements[w] = -fm*sn + mtxEigenVector.elements[w]*cn;
                            }

                            nextLoop = true;
                            break;
                        }
                    }

                    if (nextLoop)
                        break;
                }

                if (nextLoop)
                {
                    nextLoop = false;
                    continue;
                }

                nextLoop = false;

                // 如果达到精度要求，退出循环，返回结果
                if (ff < eps)
                {
                    for (i = 0; i < numColumns; ++i)
                        dblEigenValue[i] = GetElement(i, i);
                    return true;
                }

                ff = ff/(1.0*numColumns);
            }
        }

        /// <summary>
        /// 从文件中读取矩阵，根据行数和每一行数据个数构建行列
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>读取失败返回false</returns>
        public static Matrix LoadFromTextFile(string fileName)
        {
            StreamReader sr = new StreamReader(fileName);

            if (sr == null)
            {
                return null;
            }

            ArrayList stringArray = new ArrayList();
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                stringArray.Add(line);
            }
            sr.Close();

            int rows = stringArray.Count;
            int cols = 0;

            if (stringArray.Count > 0)
            {
                String[] numberString = stringArray[0].ToString().Split(',');
                cols = numberString.Length;
            }
            else
            {
                return null;
            }

            Matrix result = new Matrix(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                String[] tempData = stringArray[i].ToString().Split(',');
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = double.Parse(tempData[j]);
                }
            }

            return result;
        }

        /// <summary>
        /// 将矩阵保存到文本文件中
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>读取失败返回false</returns>
        public void SaveToTextFile(string fileName)
        {
            StreamWriter sw = new StreamWriter(fileName);
            sw.Write(this.ToString());
            sw.Close();
        }

        /// <summary>
        /// 方法，获取左上角子矩阵
        /// </summary>
        /// <param name="subRow">子矩阵的行数</param>
        /// <param name="subCol">子矩阵的列数</param>
        /// <returns>返回子矩阵</returns>
        public Matrix GetSubMatrix(int subRow, int subCol)
        {
            if ((subRow > numRows) || (subCol > numColumns))
            {
                throw new MatrixException("子矩阵行列数不能比母矩阵大。");
            }

            Matrix result = new Matrix(subRow, subCol);

            for (int i = 0; i < subRow; i++)
            {
                for (int j = 0; j < subCol; j++)
                {
                    result[i, j] = this[i, j];
                }
            }
            return result;
        }

        /// <summary>
        /// 方法，获取左上角子方阵
        /// </summary>
        /// <param name="subRowCols">子矩阵的行列数</param>
        /// <returns>返回子矩阵</returns>
        public Matrix GetSubMatrix(int subRowCols)
        {
            if ((subRowCols > numRows) || (subRowCols > numColumns))
            {
                throw new MatrixException("子矩阵行列数不能比母矩阵大。");
            }

            Matrix result = new Matrix(subRowCols, subRowCols);

            for (int i = 0; i < subRowCols; i++)
            {
                for (int j = 0; j < subRowCols; j++)
                {
                    result[i, j] = this[i, j];
                }
            }
            return result;
        }

        /// <summary>
        /// 删除指定列的向量，得到新的矩阵
        /// </summary>
        /// <param name="nCol">向量所在的列</param>
        /// <returns>Matrix 型, 新的矩阵</returns>
        public Matrix DeleteColVector(int nCol)
        {
            if (nCol >= ColumnsCount || nCol < 0)
            {
                throw new MatrixException("删除列向量不正确。");
            }
            Matrix result = new Matrix(RowsCount, ColumnsCount - 1);

            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColumnsCount - 1; j++)
                {
                    if (j < nCol)
                    {
                        result[i, j] = this[i, j];
                    }
                    else
                        result[i, j] = this[i, j + 1];
                }
            }
            return result;
        }

        /// <summary>
        /// 删除多列,列索引在矩阵行向量中存储
        /// </summary>
        /// <param name="nCols">1*n形式,且数据从小到大排列.</param>
        /// <returns></returns>
        public Matrix DeleteColVector(Matrix nCols)
        {
            try
            {
                Matrix result = this.Clone() as Matrix;
                for (int i = nCols.ColumnsCount - 1; i >= 0; i--)
                {
                    result = result.DeleteColVector((int) nCols[0, i]);
                }
                return result;
            }
            catch
            {
                throw new MatrixException("删除列向量不正确。");
            }
        }

        /// <summary>
        /// 删除指定行的向量，得到新的矩阵
        /// </summary>
        /// <param name="nRow">向量所在的行</param>
        /// <returns> Matrix 型, 新的矩阵</returns>
        public Matrix DeleteRowVector(int nRow)
        {
            if (nRow >= RowsCount || nRow < 0)
            {
                throw new MatrixException("删除行向量不正确。");
            }

            Matrix result = new Matrix(RowsCount - 1, ColumnsCount);

            for (int i = 0; i < ColumnsCount; i++)
            {
                for (int j = 0; j < RowsCount - 1; j++)
                {
                    if (j < nRow)
                    {
                        result[i, j] = this[i, j];
                    }
                    else
                        result[i, j] = this[i, j + 1];
                }
            }
            return result;
        }

        /// <summary>
        /// 删除多行,行索引在矩阵行向量中存储
        /// </summary>
        /// <param name="nCols">1*n形式,且数据从小到大排列.</param>
        /// <returns></returns>
        public Matrix DeleteRowVector(Matrix nRows)
        {
            try
            {
                Matrix result = this.Clone() as Matrix;
                for (int i = nRows.ColumnsCount - 1; i >= 0; i--)
                {
                    result = result.DeleteRowVector((int) nRows[0, i]);
                }
                return result;
            }
            catch
            {
                throw new MatrixException("删除行向量不正确。");
            }
        }

        /// <summary>
        /// 方法，获取矩阵的第ncol列向量，列数从0开始
        /// </summary>
        /// <param name="nCol">需要的列向量</param>
        /// <returns>返回需要的列向量矩阵</returns>
        public Matrix ReceiveColVector(int nCol)
        {
            if (nCol >= ColumnsCount || nCol < 0)
            {
                throw new MatrixException("获取列向量不正确。");
            }

            Matrix result = new Matrix(RowsCount, 1);

            for (int i = 0; i < RowsCount; i++)
            {
                result[i, 0] = this[i, nCol];
            }
            return result;
        }

        /// <summary>
        /// 获取多列,列索引在矩阵行向量中存储
        /// </summary>
        /// <param name="nCols">1*n形式,且数据从小到大排列.</param>
        /// <returns></returns>
        public Matrix ReceiveColVector(Matrix nCols)
        {
            Matrix result = new Matrix(this.RowsCount, nCols.ColumnsCount);
            for (int i = 0; i < nCols.ColumnsCount; i++)
            {
                Matrix temp = this.ReceiveColVector((int) nCols[0, i]);
                for (int j = 0; j < result.RowsCount; j++)
                {
                    result[j, i] = temp[j, 0];
                }
            }
            return result;
        }

        /// <summary>
        /// 方法，获取矩阵的第nRow行向量，行数从0开始
        /// </summary>
        /// <param name="nRow">需要的行向量</param>
        /// <returns>返回需要的行向量矩阵</returns>
        public Matrix ReceiveRowVector(int nRow)
        {
            if (nRow >= RowsCount || nRow < 0)
            {
                throw new MatrixException("获取行向量不正确。");
            }

            Matrix result = new Matrix(1, ColumnsCount);

            for (int i = 0; i < ColumnsCount; i++)
            {
                result[0, i] = this[nRow, i];
            }
            return result;
        }

        /// <summary>
        /// 获取多行,行索引在矩阵行向量中存储
        /// </summary>
        /// <param name="nCols">1*n形式,且数据从小到大排列.</param>
        /// <returns></returns>
        public Matrix ReceiveRowVector(Matrix nRows)
        {
            Matrix result = new Matrix(nRows.ColumnsCount, this.ColumnsCount);
            for (int i = 0; i < nRows.ColumnsCount; i++)
            {
                Matrix temp = this.ReceiveRowVector((int) nRows[0, i]);
                for (int j = 0; j < result.ColumnsCount; j++)
                {
                    result[i, j] = temp[0, j];
                }
            }
            return result;
        }

        /// <summary>
        /// 方法，获取矩阵的最大值
        /// </summary>
        /// <returns> 返回矩阵的最大值</returns>
        public double MaxValue()
        {
            double max = double.MinValue;
            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColumnsCount; j++)
                {
                    if (max < this[i, j]) max = this[i, j];
                }
            }
            return max;
        }

        /// <summary>
        /// 方法，获取矩阵的最大值的位置
        /// </summary>
        /// <returns> 返回矩阵的最大值的位置</returns>
        public int MaxValuePosition()
        {
            double max = this[0, 0];
            int maxPosition = 0;
            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColumnsCount; j++)
                {
                    if (max < this[i, j])
                    {
                        max = this[i, j];
                        maxPosition = i*ColumnsCount + j;
                    }
                }
            }
            return maxPosition;
        }

        /// </summary>
        /// 方法，获取矩阵的最小值的位置
        /// </summary>
        /// <returns> 返回矩阵的最小值的位置</returns>
        public int MinValuePosition()
        {
            double min = this[0, 0];
            int minPosition = 0;
            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColumnsCount; j++)
                {
                    if (min > this[i, j])
                    {
                        min = this[i, j];
                        minPosition = i*ColumnsCount + j;
                    }
                }
            }
            return minPosition;
        }

        /// <summary>
        /// 方法，获取矩阵的最小值
        /// </summary>
        /// <returns> 返回矩阵的最小值</returns>
        public double MinValue()
        {
            double min = double.MaxValue;
            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColumnsCount; j++)
                {
                    if (min > this[i, j]) min = this[i, j];
                }
            }
            return min;
        }

        /// <summary>
        /// 将矩阵所有元素的值置为value
        /// </summary>
        /// <param name="value">value</param>
        public void SetAllElementsValue(double value)
        {
            for (int i = 0; i < this.RowsCount; i++)
            {
                for (int j = 0; j < this.ColumnsCount; j++)
                {
                    this[i, j] = value;
                }
            }
        }

        /// <summary>
        /// 得到一个单位矩阵
        /// </summary>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static Matrix GetEye(int rows)
        {
            Matrix eye = new Matrix(rows, rows);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (i == j)
                    {
                        eye[i, j] = 1;
                    }
                    else
                    {
                        eye[i, j] = 0;
                    }
                }
            }
            return eye;
        }

        /// <summary>
        /// 得到矩阵所有元素的平方和 
        /// </summary>
        /// <returns></returns>
        public double GetSquareSum()
        {
            if (elements == null)
            {
                return 0;
            }
            double sum = 0;
            for (int i = 0; i < (this.ColumnsCount*this.RowsCount); i++)
            {
                sum += elements[i]*elements[i];
            }
            return sum;
        }

        /// <summary>
        /// 返回矩阵的“列和”行向量
        /// </summary>
        /// <returns></returns>
        public Matrix GetColumnsSum()
        {
            Matrix result = new Matrix(1, this.ColumnsCount);
            for (int i = 0; i < this.ColumnsCount; i++)
            {
                for (int j = 0; j < this.RowsCount; j++)
                {
                    result[0, i] += this[j, i];
                }
            }
            return result;
        }

        /// <summary>
        /// 返回矩阵的“列平方和”行向量
        /// </summary>
        /// <returns></returns>
        public Matrix GetColumnsPowSum()
        {
            Matrix result = new Matrix(1, this.ColumnsCount);
            for (int i = 0; i < this.ColumnsCount; i++)
            {
                for (int j = 0; j < this.RowsCount; j++)
                {
                    result[0, i] += this[j, i]*this[j, i];
                }
            }
            return result;
        }

//		public byte[] GetSingleBytes()
//		{
//			ArrayList byteList = new ArrayList();
//			for(int i=0;i<this.RowsCount;i++)
//			{
//				for(int j=0;j<this.ColumnsCount;j++)
//				{
//					byteList.Add(BitConverter.GetBytes(Convert.ToSingle(this[i,j])));
//				}
//			}
//			return Fpi.FpiSerializable.FpiSerializationUtil.MerageBytes(byteList);
//		}
//
//		public byte[] GetUIntBytes()
//		{
//			ArrayList byteList = new ArrayList();
//			for(int i=0;i<this.RowsCount;i++)
//			{
//				for(int j=0;j<this.ColumnsCount;j++)
//				{
//					byteList.Add(BitConverter.GetBytes(Convert.ToInt16(this[i,j])));
//				}
//			}
//			return Fpi.FpiSerializable.FpiSerializationUtil.MerageBytes(byteList);
//		}

        #region ICloneable 成员

        public object Clone()
        {
            return new Matrix(this);
        }

        #endregion
    }
}