using System;

namespace Fpi.Util.MathUtil
{
    /// <summary>
    /// �࣬�����쳣��
    /// </summary>
    public class MatrixException : Exception
    {
        /// <summary>
        /// ���캯��
        /// </summary>
        public MatrixException()
            : base()
        {
        }

        /// <summary>
        /// ���캯��
        /// </summary>
        public MatrixException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// ���캯��
        /// </summary>
        public MatrixException(String message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}