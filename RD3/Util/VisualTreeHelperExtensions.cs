using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RD3.Shared
{
    public static class VisualTreeHelperExtensions
    {
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    yield return typedChild;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }

        // 辅助方法：在可视化树中向上查找特定类型的父容器（如ListBoxItem）
        public static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }

        public static ListBoxItem FindTouchedListBoxItem(ListBox listBox, Point touchPoint)
        {
            foreach (var item in listBox.Items)
            {
                ListBoxItem container = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(item);
                if (container != null && container.InputHitTest(touchPoint) != null)
                {
                    return container;
                }
            }
            return null;
        }

        public static int FindNearestIndex(ListBox listBox, Point point)
        {
            int nearestIndex = -1;
            double minDistance = double.MaxValue;

            for (int i = 0; i < listBox.Items.Count; i++)
            {
                ListBoxItem item = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (item == null) continue;

                // 获取项的相对位置和渲染边界
                Point itemPosition = item.TranslatePoint(new Point(0, 0), listBox);
                Rect itemRect = new Rect(itemPosition, item.RenderSize);

                // 计算鼠标点与当前项中心的垂直距离（主要考虑垂直列表）
                double centerY = itemRect.Top + itemRect.Height / 2;
                double distance = Math.Abs(point.Y - centerY);

                // 找到中心点距离鼠标最近的那个项
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }

            // 如果拖拽位置在最后一项之后，则返回最后一项的索引+1
            if (nearestIndex >= 0 && point.Y > ((ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(nearestIndex)).TranslatePoint(new Point(0, 0), listBox).Y + ((ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(nearestIndex)).RenderSize.Height)
            {
                nearestIndex++;
            }

            return nearestIndex;
        }
    }
}
