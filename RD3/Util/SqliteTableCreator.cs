using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XZ.DB;

namespace RD3
{
    public static class SqliteTableCreator
    {
        /// <summary>
        /// 扫描所有程序集，自动为带 [Table] 特性的类建表/更新表（原生特性）
        /// <param name="alignFullStructure">是否完全对齐表结构（true=删除类中不存在的字段，false=只添加缺失字段，默认false）</param>
        /// </summary>
        public static void CreateAllTablesByNativeAttribute(bool alignFullStructure=false)
        {
            // 1. 扫描所有程序集，获取带 [Table] 特性的类（核心修改）
            List<Type> tableTypes = ScanAllNativeTableTypes();
            if (tableTypes.Count == 0)
            {
                return;
            }
            // 2. 逐个类执行建表/更新表（复用反射调用逻辑）
            foreach (var tableType in tableTypes)
            {
                try
                {
                    MethodInfo createMethod = typeof(SqliteManager)
                            .GetMethod(
                                nameof(SqliteManager.CreateOrAlterTable),
                                BindingFlags.Public | BindingFlags.Static,
                                Type.DefaultBinder,
                                 new Type[] { typeof(bool) },// Type.EmptyTypes,
                                null
                            );
                    if (createMethod == null)
                    {
                        continue;
                    }

                    // 构造泛型方法（传入实体类类型 T）
                    MethodInfo genericMethod = createMethod.MakeGenericMethod(tableType);
                    genericMethod.Invoke(null, new object[] { alignFullStructure });
                }
                catch (Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// 过滤系统/第三方程序集（完全复用）
        /// </summary>
        private static bool IsSystemOrThirdPartyAssembly(Assembly assembly)
        {
            string assemblyName = assembly.GetName().Name;
            var includedPrefixes = new[] { "Fpi.", "RD3.", "RD3", "XZ." };

            // 存在任意前缀匹配 → 不是第三方（返回 false）；否则是第三方（返回 true）
            return !includedPrefixes.Any(prefix =>
                assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// 扫描所有程序集，筛选带 [Table] 特性的类（原生特性）
        /// </summary>
        private static List<Type> ScanAllNativeTableTypes()
        {
            List<Type> tableTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    // 过滤系统/第三方程序集（复用原有规则）
                    if (IsSystemOrThirdPartyAssembly(assembly))
                        continue;

                    // 筛选：非抽象类 + 带 [Table] 特性（核心修改）
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.IsClass && !type.IsAbstract && type.IsDefined(typeof(TableAttribute), inherit: false))
                        {
                            tableTypes.Add(type);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            return tableTypes;
        }
    }
}
