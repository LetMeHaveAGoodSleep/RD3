using System;

namespace Fpi.Util.MathUtil
{
    /// <summary>
    /// 类，矩阵异常类
    /// </summary>
    public class MatrixException : Exception
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public MatrixException()
            : base()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MatrixException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MatrixException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}