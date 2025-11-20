using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    /// <summary>
    /// 审计追踪信息基类
    /// </summary>
    public class AuditParam : BindableBase, ICloneable
    {
        // 审计日志集合（存储所有属性变更记录）
        private readonly List<AuditLog> _auditLogs = new List<AuditLog>();
        /// <summary>
        /// 公开审计日志（只读，避免外部修改）
        /// </summary>
        public IReadOnlyList<AuditLog> AuditLogs => _auditLogs.AsReadOnly();

        private string _moduleName;
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName 
        { 
            get { return _moduleName; }
            set { SetProperty(ref _moduleName, value); }
        }

        private bool _isAuditing = false;
        /// <summary>
        /// 启用审计追踪与否
        /// </summary>
        [JsonIgnore]
        public bool IsAuditing
        {
            get { return _isAuditing; }
            set { SetProperty(ref _isAuditing, value); }
        }

        /// <summary>
        /// 从属性的DescriptionAttribute中获取描述文本
        /// </summary>
        private string GetDescriptionFromAttribute(PropertyInfo property)
        {
            // 获取属性上的DescriptionAttribute
            var descriptionAttr = property.GetCustomAttribute<DescriptionAttribute>();
            // 若存在特性则返回描述文本，否则返回属性名
            return descriptionAttr?.Description ?? property.Name;
        }

        /// <summary>
        /// 通过堆栈跟踪获取触发变更的属性（与之前相同）
        /// </summary>
        private PropertyInfo GetCallingProperty()
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame.GetMethod();
                if (method.IsSpecialName && method.Name.StartsWith("set_"))
                {
                    string propertyName = method.Name.Substring(4);
                    PropertyInfo property = null;
                    Type currentType = GetType();

                    // 1. 逐级搜索当前类及所有基类
                    while (currentType != null && property == null)
                    {
                        // 搜索当前类型中"自身声明的公共实例属性"（避免跨类同名冲突）
                        property = currentType.GetProperty(
                            propertyName,
                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
                        );
                        // 若当前类型未找到，继续搜索基类
                        currentType = currentType.BaseType;
                    }

                    return property;
                }
            }
            return null;
        }

        protected bool SetPropertyWithAudit<T>(ref T storage, T value)
        {
            // 先判断值是否真的发生变化（复用BindableBase的逻辑）
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false; // 未变更，不记录日志
            }

            // 1. 获取调用者属性信息
            var property = GetCallingProperty();
            if (property == null)
                throw new InvalidOperationException("无法获取属性信息");

            // 2. 读取Description特性（核心修改：使用内置特性）
            string propertyName = GetDescriptionFromAttribute(property);

            // 记录旧值
            T oldValue = storage;

            // 调用基类的SetProperty更新值（触发PropertyChanged事件）
            bool isChanged = SetProperty(ref storage, value, property.Name);

            // 若值已变更，且当前启用审计（IsAuditing为true），则记录日志
            if (isChanged && IsAuditing)
            {
                object logOldValue = oldValue switch
                {
                    Enum enumValue => EnumUtil.GetEnumDescription(enumValue),
                    bool boolValue => boolValue ? "是" : "否",
                    DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss"), // 日期格式化
                    float f => f.ToString("F2"), // 浮点数保留2位小数
                    double d => d.ToString("F2"),
                    _ => oldValue // 其他类型默认原值
                };

                object logNewValue = value switch
                {
                    Enum enumValue => EnumUtil.GetEnumDescription(enumValue),
                    bool boolValue => boolValue ? "是" : "否",
                    DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss"), // 日期格式化
                    float f => f.ToString("F2"), // 浮点数保留2位小数
                    double d => d.ToString("F2"),
                    _ => value // 其他类型默认原值
                };

                _auditLogs.Add(new AuditLog
                {
                    PropertyName = propertyName,
                    OldValue = logOldValue,
                    NewValue = logNewValue,
                    ModuleName = this.ModuleName // 关联所属模块
                });
            }

            return isChanged;
        }

        /// <summary>
        /// 移除所有已标记为“已使用”的日志
        /// </summary>
        /// <returns>移除的日志数量</returns>
        public int RemoveUsedLogs()
        {
            // 筛选出已使用的日志
            var usedLogs = _auditLogs.Where(log => log.IsUsed).ToList();
            int removedCount = usedLogs.Count;

            // 从列表中移除
            foreach (var log in usedLogs)
            {
                _auditLogs.Remove(log);
            }
            return removedCount;
        }

        public object Clone()
        {
            var clonedObject = ObjectCloner.DeepCopy(this);
            return clonedObject;
        }
    }

    /// <summary>
    /// 审计日志实体（记录单次属性变更）
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// 变更时间
        /// </summary>
        public DateTime ChangeTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 变更的属性名
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 变更前的值
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// 变更后的值
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// 所属模块（取自AuditParam的ModuleName）
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// 标记是否已使用（被处理过）
        /// </summary>
        public bool IsUsed { get; set; } = false;
    }
}
