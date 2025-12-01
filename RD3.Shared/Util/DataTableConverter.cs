using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RD3.Shared
{
    public static class DataTableConverter
    {
        // 缓存：实体类型 → 对应的转换委托（避免重复编译表达式树）
        private static readonly Dictionary<Type, Delegate> Converters = new Dictionary<Type, Delegate>();

        /// <summary>
        /// 将 DataTable 转换为指定实体类型的列表
        /// </summary>
        /// <typeparam name="T">目标实体类型（需有无参构造函数）</typeparam>
        /// <param name="table">待转换的 DataTable</param>
        /// <returns>实体列表（DataTable 为空时返回空列表）</returns>
        public static List<T> ConvertTo<T>(DataTable table) where T : new()
        {
            // 空校验：DataTable 为 null 或无行，直接返回空列表
            if (table == null || table.Rows.Count == 0)
                return new List<T>();

            Type targetType = typeof(T);
            // 检查缓存：如果已有该类型的转换委托，直接使用
            if (!Converters.TryGetValue(targetType, out var converter))
            {
                // 无缓存：创建转换委托并缓存
                converter = CreateConverter<T>(table);
                Converters[targetType] = converter;
            }

            // 执行转换并返回结果
            return ((Func<DataTable, List<T>>)converter)(table);
        }

        /// <summary>
        /// 为指定实体类型创建 DataTable 转换委托（表达式树构建）
        /// </summary>
        /// <typeparam name="T">目标实体类型</typeparam>
        /// <param name="table">参考 DataTable（用于获取列名集合）</param>
        /// <returns>转换委托（DataTable → List<T>）</returns>
        private static Func<DataTable, List<T>> CreateConverter<T>(DataTable table) where T : new()
        {
            Type targetType = typeof(T);
            // 获取实体的所有公开实例属性（可写）和公开实例字段
            PropertyInfo[] properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite) // 只处理可写属性
                .ToArray();
            FieldInfo[] fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            // 1. 构建表达式树参数：DataTable 入参
            ParameterExpression tableParam = Expression.Parameter(typeof(DataTable), "table");
            // 2. 获取 DataTable 的 Rows 集合和行数
            MemberExpression rowsProp = Expression.Property(tableParam, "Rows"); // table.Rows
            MemberExpression rowCountProp = Expression.Property(rowsProp, "Count"); // table.Rows.Count

            // 3. 定义局部变量：使用 ParameterExpression（兼容 C# 5.0，替代 VariableExpression）
            ParameterExpression listVar = Expression.Variable(typeof(List<T>), "resultList"); // 存储转换结果
            ParameterExpression rowVar = Expression.Variable(typeof(DataRow), "currentRow"); // 遍历 DataTable 的行
            ParameterExpression objVar = Expression.Variable(targetType, "targetObj"); // 待赋值的实体对象
            ParameterExpression indexVar = Expression.Variable(typeof(int), "loopIndex"); // 循环索引
                                                                                          // 缓存 DataTable 列名集合（避免每行都查询列集合，提升性能）
            ParameterExpression columnNamesVar = Expression.Variable(typeof(List<string>), "columnNames");

            // 4. 定义循环跳出标签
            LabelTarget breakLabel = Expression.Label("LoopBreak");

            // 5. 构建循环体：处理单行数据转换为实体
            BlockExpression loopBody = Expression.Block(
                // 5.1 给当前行赋值：currentRow = table.Rows[loopIndex]
                Expression.Assign(rowVar, Expression.Property(rowsProp, "Item", indexVar)),
                // 5.2 创建新实体：targetObj = new T()
                Expression.Assign(objVar, Expression.New(targetType)),
                // 5.3 处理实体的属性和字段赋值（核心逻辑：判断列是否存在，不存在则赋默认值）
                ProcessMembers(objVar, rowVar, columnNamesVar, properties, fields),
                // 5.4 将赋值后的实体添加到结果列表
                Expression.Call(listVar, "Add", null, objVar),
                // 5.5 循环索引自增：loopIndex++
                Expression.PostIncrementAssign(indexVar)
            );

            // 6. 构建完整的表达式树块
            BlockExpression completeBlock = Expression.Block(
                // 声明局部变量（顺序需与变量定义一致）
                new[] { listVar, columnNamesVar, rowVar, objVar, indexVar },
                // 6.1 初始化结果列表：resultList = new List<T>()
                Expression.Assign(listVar, Expression.New(typeof(List<T>))),
                // 6.2 缓存 DataTable 列名（只查询一次，提升性能）：columnNames = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList()
                Expression.Assign(
                    columnNamesVar,
                    Expression.Call(
                        typeof(Enumerable),
                        "ToList",
                        new[] { typeof(string) },
                        Expression.Call(
                            typeof(Enumerable),
                            "Select",
                            new[] { typeof(DataColumn), typeof(string) },
                            // 第一步：table.Columns.Cast<DataColumn>()
                            Expression.Call(
                                typeof(Enumerable),
                                "Cast",
                                new[] { typeof(DataColumn) },
                                Expression.Property(tableParam, "Columns") // table.Columns
                            ),
                            // 第二步：Select(c => c.ColumnName) —— 核心修正：参数表达式只定义一次
                            BuildColumnSelectLambda() // 单独构建 Select 的 lambda 表达式（避免重复定义参数）
                        )
                    )
                ),
                // 6.3 初始化循环索引：loopIndex = 0
                Expression.Assign(indexVar, Expression.Constant(0)),
                // 6.4 循环：while (loopIndex < table.Rows.Count) { loopBody }
                Expression.Loop(
                    Expression.IfThenElse(
                        // 循环条件：loopIndex < table.Rows.Count
                        Expression.LessThan(indexVar, rowCountProp),
                        // 满足条件：执行循环体
                        loopBody,
                        // 不满足条件：跳出循环
                        Expression.Break(breakLabel)
                    ),
                    breakLabel
                ),
                // 6.5 表达式树返回结果：resultList
                listVar
            );

            // 7. 编译表达式树为委托并返回
            return Expression.Lambda<Func<DataTable, List<T>>>(completeBlock, tableParam).Compile();
        }

        /// <summary>
        /// 单独构建 Select 委托的 lambda 表达式（c => c.ColumnName）
        /// 解决“变量未定义”问题：参数表达式只定义一次
        /// </summary>
        private static Expression<Func<DataColumn, string>> BuildColumnSelectLambda()
        {
            // 定义参数表达式（只定义一次！）
            ParameterExpression colParam = Expression.Parameter(typeof(DataColumn), "c");
            // 构建表达式体：c.ColumnName
            MemberExpression columnNameExpr = Expression.Property(colParam, "ColumnName");
            // 构建 lambda 表达式：c => c.ColumnName
            return Expression.Lambda<Func<DataColumn, string>>(columnNameExpr, colParam);
        }

        /// <summary>
        /// 构建实体属性/字段的赋值表达式（核心：列存在性判断 + 赋值逻辑）
        /// </summary>
        /// <param name="objExpr">实体对象表达式</param>
        /// <param name="rowExpr">DataRow 表达式</param>
        /// <param name="columnNamesExpr">DataTable 列名集合表达式（缓存）</param>
        /// <param name="properties">实体属性集合</param>
        /// <param name="fields">实体字段集合</param>
        /// <returns>赋值表达式块</returns>
        private static Expression ProcessMembers(
            Expression objExpr,
            Expression rowExpr,
            Expression columnNamesExpr,
            PropertyInfo[] properties,
            FieldInfo[] fields)
        {
            List<Expression> assignExpressions = new List<Expression>();

            // 处理所有可写属性
            foreach (PropertyInfo property in properties)
            {
                assignExpressions.Add(ProcessSingleMember(objExpr, rowExpr, columnNamesExpr, property));
            }

            // 处理所有公开字段
            foreach (FieldInfo field in fields)
            {
                assignExpressions.Add(ProcessSingleMember(objExpr, rowExpr, columnNamesExpr, field));
            }

            // 返回所有赋值表达式的块
            return Expression.Block(assignExpressions);
        }

        /// <summary>
        /// 构建单个成员（属性/字段）的赋值表达式
        /// </summary>
        /// <param name="objExpr">实体对象表达式</param>
        /// <param name="rowExpr">DataRow 表达式</param>
        /// <param name="columnNamesExpr">列名集合表达式</param>
        /// <param name="member">实体成员（属性/字段）</param>
        /// <returns>单个成员的赋值表达式</returns>
        private static Expression ProcessSingleMember(
            Expression objExpr,
            Expression rowExpr,
            Expression columnNamesExpr,
            MemberInfo member)
        {
            // 成员名称（用于匹配 DataTable 列名）
            string memberName = member.Name;
            // 成员类型（属性/字段的类型）
            Type memberType = member is PropertyInfo prop ? prop.PropertyType : ((FieldInfo)member).FieldType;
            // 实体成员的访问表达式（如：targetObj.Name）
            MemberExpression memberAccess = Expression.MakeMemberAccess(objExpr, member);

            // 核心：用 List.Exists + Predicate<string> 匹配参数类型（兼容 .NET Framework 4.0+）
            ParameterExpression colParam = Expression.Parameter(typeof(string), "col"); // lambda 参数：col（只定义一次）
                                                                                        // 构建 lambda 体：string.Equals(col, memberName, StringComparison.OrdinalIgnoreCase)
            Expression equalsExpr = Expression.Call(
                typeof(string),
                "Equals",
                null,
                colParam, // 复用上面定义的参数表达式
                Expression.Constant(memberName),
                Expression.Constant(StringComparison.OrdinalIgnoreCase)
            );
            // lambda 编译为 Predicate<string>（匹配 Exists 方法参数）
            LambdaExpression existsLambda = Expression.Lambda<Predicate<string>>(equalsExpr, colParam);

            // 构建 Exists 方法调用：columnNames.Exists(Predicate<string>)
            MethodCallExpression columnExistsCheck = Expression.Call(
                columnNamesExpr,
                "Exists",
                null,
                existsLambda
            );

            // 2. 列存在时的赋值逻辑：从 DataRow 取值并转换类型
            Expression columnExistsAssign = BuildColumnExistsAssignExpression(rowExpr, memberAccess, memberName, memberType);

            // 3. 列不存在时的赋值逻辑：给成员赋类型默认值
            Expression columnNotExistsAssign = BuildColumnNotExistsAssignExpression(memberAccess, memberType);

            // 4. 条件表达式：列存在则执行取值赋值，否则执行默认值赋值
            return Expression.IfThenElse(
                test: columnExistsCheck,
                ifTrue: columnExistsAssign,
                ifFalse: columnNotExistsAssign
            );
        }

        /// <summary>
        /// 构建「列存在时」的赋值表达式（从 DataRow 取值并转换）
        /// </summary>
        /// <param name="rowExpr">DataRow 表达式</param>
        /// <param name="memberAccess">实体成员访问表达式</param>
        /// <param name="memberName">成员名称</param>
        /// <param name="memberType">成员类型</param>
        /// <returns>列存在时的赋值表达式</returns>
        private static Expression BuildColumnExistsAssignExpression(
            Expression rowExpr,
            MemberExpression memberAccess,
            string memberName,
            Type memberType)
        {
            // 1. 从 DataRow 取值：row[memberName]
            IndexExpression rowValueAccess = Expression.Property(rowExpr, "Item", Expression.Constant(memberName));

            // 2. 判断取值是否为 DBNull：row[memberName] != DBNull.Value
            BinaryExpression notDBNullCheck = Expression.NotEqual(rowValueAccess, Expression.Constant(DBNull.Value));

            // 3. 类型转换：将 DataRow 取值转换为成员类型（使用 Convert.ChangeType）
            MethodCallExpression convertMethod = Expression.Call(
                typeof(Convert),
                "ChangeType",
                null,
                rowValueAccess, // 要转换的值
                Expression.Constant(memberType) // 目标类型
            );
            // 强制转换为成员类型（Convert.ChangeType 返回 object，需显式转换）
            UnaryExpression typeConvert = Expression.Convert(convertMethod, memberType);

            // 4. 赋值表达式：member = 转换后的值
            BinaryExpression assign = Expression.Assign(memberAccess, typeConvert);

            // 5. 条件：只有当取值不是 DBNull 时才赋值（否则留默认值）
            return Expression.IfThen(notDBNullCheck, assign);
        }

        /// <summary>
        /// 构建「列不存在时」的赋值表达式（赋类型默认值）
        /// </summary>
        /// <param name="memberAccess">实体成员访问表达式</param>
        /// <param name="memberType">成员类型</param>
        /// <returns>列不存在时的赋值表达式</returns>
        private static Expression BuildColumnNotExistsAssignExpression(MemberExpression memberAccess, Type memberType)
        {
            // 获取成员类型的默认值（如：int→0，string→null，DateTime→MinValue）
            ConstantExpression defaultValue = Expression.Constant(GetDefaultValue(memberType), memberType);

            // 赋值表达式：member = 默认值
            return Expression.Assign(memberAccess, defaultValue);
        }

        /// <summary>
        /// 获取指定类型的默认值（模拟 C# 中的 default(T)）
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>该类型的默认值</returns>
        private static object GetDefaultValue(Type type)
        {
            // 引用类型默认值为 null；值类型默认值为 new T()（如 int→0，bool→false）
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}