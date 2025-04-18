using System;
using System.Collections.Generic;
using System.Text;

namespace Fpi.Util.Sundry
{
    public class DateTimeUtil
    {
        /// <summary>
        /// 利用System.Environment.TickCount来获取从oldTick到现在所经过的时间(ms)
        /// </summary>
        /// <param name="oldTick">初始时间</param>
        /// <returns></returns>
        public static int GetTickCount(int oldTick)
        {
            int tick = 0;
            //int curTick = Environment.TickCount;
            int curTick = GetTickCount();
            if (curTick >= oldTick)
            {
                tick = curTick - oldTick;
            }
            else
            {
                tick = int.MaxValue - oldTick + curTick;
            }
            return tick;
        }

        /// <summary>
        /// 获取当前Environment.TickCount值(pan_xu 2012.9.28)
        /// </summary>
        public static int GetTickCount()
        {
            // TickCount cycles between Int32.MinValue, which is a negative 
            // number, and Int32.MaxValue once every 49.8 days. This sample
            // removes the sign bit to yield a nonnegative number that cycles 
            // between zero and Int32.MaxValue once every 24.9 days.

            return Environment.TickCount & Int32.MaxValue;
        }
    }
}
