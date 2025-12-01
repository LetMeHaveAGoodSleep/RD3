using RD3.Shared;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace RD3
{
    public static class DataGridColumnsAttach
    {
        // 定义附加属性：ColumnsSource（绑定列配置集合）
        public static readonly DependencyProperty ColumnsSourceProperty =
            DependencyProperty.RegisterAttached(
                "ColumnsSource",
                typeof(IEnumerable),
                typeof(DataGridColumnsAttach),
                new PropertyMetadata(null, OnColumnsSourceChanged));

        // 定义私有附加属性：用于存储集合变更的监听对象（避免内存泄漏）
        private static readonly DependencyProperty CollectionListenerProperty =
            DependencyProperty.RegisterAttached(
                "CollectionListener",
                typeof(INotifyCollectionChanged),
                typeof(DataGridColumnsAttach),
                new PropertyMetadata(null));

        // 设置附加属性的方法
        public static void SetColumnsSource(DependencyObject obj, IEnumerable value)
        {
            obj.SetValue(ColumnsSourceProperty, value);
        }

        // 获取附加属性的方法
        public static IEnumerable GetColumnsSource(DependencyObject obj)
        {
            return (IEnumerable)obj.GetValue(ColumnsSourceProperty);
        }

        #region 核心：列数据源变更事件
        // 当列数据源变化时，重新绑定监听并生成列
        private static void OnColumnsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dataGrid) return;

            // ========== 步骤1：取消旧数据源的所有监听 ==========
            // 取消旧集合的变更监听
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= OnCollectionChanged;
                // 取消旧集合中所有ReportNode的属性变更监听
                UnsubscribeReportNodeProperties((IEnumerable)oldCollection);
            }
            // 清空旧的监听对象存储
            dataGrid.SetValue(CollectionListenerProperty, null);

            // ========== 步骤2：绑定新数据源的监听并生成列 ==========
            if (e.NewValue is not IEnumerable newColumnsSource)
            {
                dataGrid.Columns.Clear();
                return;
            }

            // 存储新集合的监听对象，方便后续反订阅
            if (newColumnsSource is INotifyCollectionChanged newCollection)
            {
                dataGrid.SetValue(CollectionListenerProperty, newCollection);
                newCollection.CollectionChanged += OnCollectionChanged;
            }

            // 为新集合中的ReportNode订阅属性变更事件
            SubscribeReportNodeProperties(newColumnsSource);

            // 生成列
            GenerateDataGridColumns(dataGrid, newColumnsSource);
        }
        #endregion

        #region 集合变更监听：增/删/改集合元素时刷新列
        private static void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 找到绑定的DataGrid
            if (sender is not IEnumerable collection ||
                !TryFindDataGridByCollection(collection, out var dataGrid))
            {
                return;
            }

            // 取消旧元素的属性监听（如移除/替换元素时）
            if (e.OldItems != null) UnsubscribeReportNodeProperties(e.OldItems);
            // 订阅新元素的属性监听（如添加/替换元素时）
            if (e.NewItems != null) SubscribeReportNodeProperties(e.NewItems);

            // 重新生成列
            GenerateDataGridColumns(dataGrid, collection);
        }
        #endregion

        #region ReportNode属性变更监听：单个属性变动时刷新列
        private static void OnReportNodePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ReportNode node ||
                !TryFindDataGridByReportNode(node, out var dataGrid) ||
                dataGrid.GetValue(ColumnsSourceProperty) is not IEnumerable columnsSource)
            {
                return;
            }

            // 仅当影响列显示的属性变更时刷新（可根据需求扩展）
            if (e.PropertyName is nameof(node.Used) or nameof(node.Width) or nameof(node.ShowName)
                or nameof(node.FieldName) or nameof(node.Unit) or nameof(node.TypeName))
            {
                GenerateDataGridColumns(dataGrid, columnsSource);
            }
        }
        #endregion

        #region 工具方法：订阅/取消订阅ReportNode的属性事件
        /// <summary>
        /// 为集合中的所有ReportNode订阅属性变更事件
        /// </summary>
        private static void SubscribeReportNodeProperties(IEnumerable items)
        {
            foreach (var item in items)
            {
                if (item is INotifyPropertyChanged node)
                {
                    node.PropertyChanged += OnReportNodePropertyChanged;
                }
            }
        }

        /// <summary>
        /// 取消集合中所有ReportNode的属性变更事件订阅
        /// </summary>
        private static void UnsubscribeReportNodeProperties(IEnumerable items)
        {
            foreach (var item in items)
            {
                if (item is INotifyPropertyChanged node)
                {
                    node.PropertyChanged -= OnReportNodePropertyChanged;
                }
            }
        }
        #endregion

        #region 工具方法：根据集合/ReportNode查找绑定的DataGrid
        /// <summary>
        /// 根据列配置集合查找绑定的DataGrid
        /// </summary>
        private static bool TryFindDataGridByCollection(IEnumerable collection, out DataGrid dataGrid)
        {
            dataGrid = null;
            // 遍历所有可视化树中的DataGrid，匹配ColumnsSource
            foreach (var window in Application.Current.Windows)
            {
                if (window is Window w)
                {
                    FindDataGridByCollection(w.Content as DependencyObject, collection, ref dataGrid);
                    if (dataGrid != null) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据ReportNode查找绑定的DataGrid
        /// </summary>
        private static bool TryFindDataGridByReportNode(ReportNode node, out DataGrid dataGrid)
        {
            dataGrid = null;
            foreach (var window in Application.Current.Windows)
            {
                if (window is Window w)
                {
                    FindDataGridByReportNode(w.Content as DependencyObject, node, ref dataGrid);
                    if (dataGrid != null) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 递归遍历可视化树，根据集合查找DataGrid
        /// </summary>
        private static void FindDataGridByCollection(DependencyObject obj, IEnumerable collection, ref DataGrid dataGrid)
        {
            if (obj == null || dataGrid != null) return;

            if (obj is DataGrid dg && GetColumnsSource(dg) == collection)
            {
                dataGrid = dg;
                return;
            }

            // 递归子元素
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                FindDataGridByCollection(VisualTreeHelper.GetChild(obj, i), collection, ref dataGrid);
            }
        }

        /// <summary>
        /// 递归遍历可视化树，根据ReportNode查找DataGrid
        /// </summary>
        private static void FindDataGridByReportNode(DependencyObject obj, ReportNode node, ref DataGrid dataGrid)
        {
            if (obj == null || dataGrid != null) return;

            if (obj is DataGrid dg && GetColumnsSource(dg) is IEnumerable columns && ContainsReportNode(columns, node))
            {
                dataGrid = dg;
                return;
            }

            // 递归子元素
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                FindDataGridByReportNode(VisualTreeHelper.GetChild(obj, i), node, ref dataGrid);
            }
        }

        /// <summary>
        /// 检查列配置集合是否包含指定的ReportNode
        /// </summary>
        private static bool ContainsReportNode(IEnumerable columns, ReportNode node)
        {
            foreach (var item in columns)
            {
                if (item == node) return true;
            }
            return false;
        }
        #endregion

        #region 核心方法：生成DataGrid列
        /// <summary>
        /// 统一生成DataGrid列的逻辑
        /// </summary>
        private static void GenerateDataGridColumns(DataGrid dataGrid, IEnumerable columnsSource)
        {
            if (dataGrid == null || columnsSource == null) return;

            // 清空原有列
            dataGrid.Columns.Clear();

            // 遍历列配置集合，创建对应的DataGridTextColumn
            foreach (var item in columnsSource)
            {
                if (item is ReportNode columnConfig && columnConfig.Used)
                {
                    string stringFormat = null;
                    // 根据类型设置格式化字符串
                    if (columnConfig.TypeName == typeof(DateTime).FullName)
                    {
                        stringFormat = "yyyy-MM-dd HH:mm:ss";
                    }
                    else if (columnConfig.TypeName == typeof(Single).FullName)
                    {
                        stringFormat = "F2";
                    }

                    // 创建文本列
                    var column = new DataGridTextColumn
                    {
                        Width = columnConfig.Width,
                        Header = string.IsNullOrWhiteSpace(columnConfig.Unit)? columnConfig.ShowName: $"{columnConfig.ShowName}({columnConfig.Unit})",
                        Binding = new Binding(columnConfig.FieldName) { StringFormat = stringFormat },
                        HeaderStyle = App.Current.Resources["DataGridCenterColumnHeaderStyle"] as Style,
                        CellStyle = App.Current.Resources["DataGridTextCenterCellStyle"] as Style,
                        IsReadOnly = true,
                        CanUserSort = false
                    };

                    dataGrid.Columns.Add(column);
                }
            }
        }
        #endregion
    }
}