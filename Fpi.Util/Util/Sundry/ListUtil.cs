using System.Collections;

namespace Fpi.Util.Sundry
{
    /// <summary>
    /// 集合操作方法集
    /// </summary>
    public class ListUtil
    {
        /// <summary>
        /// 在集合内向上移动
        /// </summary>
        /// <param name="items">集合</param>
        /// <param name="currentIndex">当前项的索引</param>
        /// <returns></returns>
        public static bool UpMove(IList items, int currentIndex)
        {
            if (currentIndex == 0)
            {
                return false;
            }

            object obj = items[currentIndex];
            items.RemoveAt(currentIndex);
            items.Insert(currentIndex - 1, obj);
            return true;
        }


        /// <summary>
        /// 在集合内向下移动
        /// </summary>
        /// <param name="items">集合</param>
        /// <param name="currentIndex">当前项的索引</param>
        /// <returns></returns>
        public static bool DownMove(IList items, int currentIndex)
        {
            if (currentIndex == items.Count - 1)
            {
                return false;
            }
            object obj = items[currentIndex];
            items.RemoveAt(currentIndex);
            items.Insert(currentIndex + 1, obj);
            return true;
        }

        /// <summary>
        /// 移除集合内所有成员
        /// </summary>
        /// <param name="items">集合</param>
        public static void RemoveAll(IList items)
        {
            items.Clear();
        }

        /// <summary>
        /// 移除集合内指定索引处成员
        /// </summary>
        /// <param name="items">集合</param>
        /// <param name="index">指定索引</param>
        /// <returns>若索引不在集合范围内返回false ,否则执行移除动作并返回true</returns>
        public static bool RemoveAt(IList items, int index)
        {
            if (index < 0 || index >= items.Count)
            {
                return false;
            }
            items.RemoveAt(index);
            return true;
        }
    }
}