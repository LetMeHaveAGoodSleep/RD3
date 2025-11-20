using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;

    public static class AuditLogUtil
    {
        /// <summary>
        /// 仅提取DeviceParameter自身属性及直接集合中包含的AuditParam实例
        /// </summary>
        public static HashSet<AuditParam> ExtractDeviceParamAuditLogs(DeviceParameter deviceParam)
        {
            var auditInstances = new HashSet<AuditParam>();
            // 只遍历DeviceParameter自身的属性（不深入嵌套对象的内部属性）
            TraverseDeviceParamProperties(deviceParam, auditInstances);
            return auditInstances;
        }

        /// <summary>
        /// 仅遍历DeviceParameter的直接属性和其包含的集合元素（不递归嵌套对象的属性）
        /// </summary>
        private static void TraverseDeviceParamProperties(DeviceParameter deviceParam, HashSet<AuditParam> auditInstances)
        {
            if (deviceParam == null)
                return;

            // 2. 反射获取DeviceParameter的所有公共属性
            var properties = typeof(DeviceParameter).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                // 跳过索引器和不可读属性
                if (prop.GetIndexParameters().Length > 0 || !prop.CanRead)
                    continue;

                object propValue;
                try
                {
                    propValue = prop.GetValue(deviceParam);
                }
                catch
                {
                    continue; // 忽略访问失败的属性
                }

                // 3. 处理属性值：若为AuditParam实例，直接添加
                if (propValue is AuditParam auditObj && !auditInstances.Contains(auditObj))
                {
                    auditInstances.Add(auditObj);
                }
                // 4. 处理集合类型（仅遍历集合元素，不深入元素的内部属性）
                else if (propValue is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        // 集合元素若为AuditParam，添加到集合
                        if (item is AuditParam itemAudit && !auditInstances.Contains(itemAudit))
                        {
                            auditInstances.Add(itemAudit);
                        }
                        // 不递归遍历集合元素的内部属性（避免深层嵌套）
                    }
                }
                // 5. 非集合、非AuditParam的属性：不处理（避免深入嵌套对象）
            }
        }
    }
}
