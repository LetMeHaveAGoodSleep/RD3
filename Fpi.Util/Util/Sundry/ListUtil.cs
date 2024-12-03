using System.Collections;

namespace Fpi.Util.Sundry
{
    /// <summary>
    /// ���ϲ���������
    /// </summary>
    public class ListUtil
    {
        /// <summary>
        /// �ڼ����������ƶ�
        /// </summary>
        /// <param name="items">����</param>
        /// <param name="currentIndex">��ǰ�������</param>
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
        /// �ڼ����������ƶ�
        /// </summary>
        /// <param name="items">����</param>
        /// <param name="currentIndex">��ǰ�������</param>
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
        /// �Ƴ����������г�Ա
        /// </summary>
        /// <param name="items">����</param>
        public static void RemoveAll(IList items)
        {
            items.Clear();
        }

        /// <summary>
        /// �Ƴ�������ָ����������Ա
        /// </summary>
        /// <param name="items">����</param>
        /// <param name="index">ָ������</param>
        /// <returns>���������ڼ��Ϸ�Χ�ڷ���false ,����ִ���Ƴ�����������true</returns>
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