using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RD3
{
    public class ListBoxExtensions
    {
        #region 附加属性：SelectedItems（绑定ViewModel中的选中项集合）
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(ListBoxExtensions),
                new PropertyMetadata(null, OnSelectedItemsPropertyChanged));

        // 获取附加属性值
        public static IList GetSelectedItems(DependencyObject obj)
        {
            return (IList)obj.GetValue(SelectedItemsProperty);
        }

        // 设置附加属性值
        public static void SetSelectedItems(DependencyObject obj, IList value)
        {
            obj.SetValue(SelectedItemsProperty, value);
        }
        #endregion

        #region 内部逻辑：监听SelectionChanged并同步数据
        // 附加属性值变化时的处理
        private static void OnSelectedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBox listBox) return;

            // 移除旧事件监听，避免重复触发
            listBox.SelectionChanged -= ListBox_SelectionChanged;

            // 新值不为空时，绑定事件并初始化选中项
            if (e.NewValue is IList newSelectedItems)
            {
                listBox.SelectionChanged += ListBox_SelectionChanged;
                // 初始化：将ViewModel中的选中项同步到ListBox
                SyncListBoxSelectedItems(listBox, newSelectedItems);
            }
        }

        // ListBox选中项变化时，同步到ViewModel的集合
        private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox) return;

            var targetSelectedItems = GetSelectedItems(listBox);
            if (targetSelectedItems == null) return;

            // 加锁避免并发修改异常
            lock (targetSelectedItems)
            {
                // 移除取消选中的项
                foreach (var removedItem in e.RemovedItems)
                {
                    if (targetSelectedItems.Contains(removedItem))
                    {
                        targetSelectedItems.Remove(removedItem);
                    }
                }

                // 添加新选中的项
                foreach (var addedItem in e.AddedItems)
                {
                    if (!targetSelectedItems.Contains(addedItem))
                    {
                        targetSelectedItems.Add(addedItem);
                    }
                }
            }
        }

        // 初始化：将ViewModel的选中项同步到ListBox
        private static void SyncListBoxSelectedItems(ListBox listBox, IList viewModelSelectedItems)
        {
            if (listBox.ItemsSource == null || viewModelSelectedItems.Count == 0) return;

            // 清空ListBox原有选中项
            listBox.SelectedItems.Clear();

            // 批量添加ViewModel中的选中项
            foreach (var item in viewModelSelectedItems)
            {
                if (listBox.Items.Contains(item))
                {
                    listBox.SelectedItems.Add(item);
                }
            }
        }
        #endregion
    }
}
