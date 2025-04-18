using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fpi.Util.MathUtil
{
    public class PeakInfo
    {
        /// <summary>
        /// 峰起点
        /// </summary>
        public int StartPoint { set; get; }

        /// <summary>
        /// 左拐点
        /// </summary>
        public int LeftPoint { set; get; }

        /// <summary>
        /// 右拐点
        /// </summary>
        public int RightPoint { set; get; }

        /// <summary>
        /// 峰顶点
        /// </summary>
        public int TopPoint { set; get; }

        /// <summary>
        /// 峰终点
        /// </summary>
        public int EndPoint { set; get; }
    }
}
