using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace RD3.Shared
{
    public static class DataTableConverter
    {
        private static readonly Dictionary<Type, Delegate> Converters = new Dictionary<Type, Delegate>();

        public static List<T> ConvertTo<T>(DataTable table) where T : new()
        {
            if (table == null || table.Rows.Count == 0)
                return new List<T>();

            if (!Converters.TryGetValue(typeof(T), out var converter))
            {
                converter = CreateConverter<T>(table);
                Converters[typeof(T)] = converter;
            }

            return ((Func<DataTable, List<T>>)converter)(table);
        }

        private static Func<DataTable, List<T>> CreateConverter<T>(DataTable table) where T : new()
        {
            Type type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            // 参数表达式
            var tableParam = Expression.Parameter(typeof(DataTable), "table");
            var rowsProp = Expression.Property(tableParam, "Rows");
            var countProp = Expression.Property(rowsProp, "Count");

            // 局部变量
            var listVar = Expression.Variable(typeof(List<T>), "list");
            var rowVar = Expression.Variable(typeof(DataRow), "row");
            var objVar = Expression.Variable(type, "obj");
            var indexVar = Expression.Variable(typeof(int), "i");

            // 定义循环的break标签
            var breakLabel = Expression.Label("LoopBreak");

            // 构建循环体
            var loopBody = Expression.Block(
                // 获取当前行: row = table.Rows[i]
                Expression.Assign(
                    rowVar,
                    Expression.Property(rowsProp, "Item", indexVar)
                ),
                // 创建新对象: obj = new T()
                Expression.Assign(objVar, Expression.New(type)),
                // 处理属性和字段赋值
                ProcessMembers(objVar, rowVar, properties, fields),
                // 添加到列表: list.Add(obj)
                Expression.Call(listVar, "Add", null, objVar),
                // i++
                Expression.PostIncrementAssign(indexVar)
            );

            // 构建完整表达式
            var block = Expression.Block(
                // 局部变量声明
                new[] { listVar, rowVar, objVar, indexVar },
                // list = new List<T>()
                Expression.Assign(listVar, Expression.New(typeof(List<T>))),
                // i = 0
                Expression.Assign(indexVar, Expression.Constant(0)),
                // while循环
                Expression.Loop(
                    Expression.IfThenElse(
                        // 条件: i < table.Rows.Count
                        Expression.LessThan(indexVar, countProp),
                        // 循环体
                        loopBody,
                        // 否则跳出循环
                        Expression.Break(breakLabel)
                    ),
                    breakLabel
                ),
                // 返回list
                listVar
            );

            return Expression.Lambda<Func<DataTable, List<T>>>(block, tableParam).Compile();
        }

        private static Expression ProcessMembers(Expression obj, Expression row, PropertyInfo[] properties, FieldInfo[] fields)
        {
            var statements = new List<Expression>();

            // 处理属性
            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    statements.Add(ProcessMember(obj, row, property));
                }
            }

            // 处理字段
            foreach (var field in fields)
            {
                statements.Add(ProcessMember(obj, row, field));
            }

            return Expression.Block(statements);
        }

        private static Expression ProcessMember(Expression obj, Expression row, MemberInfo member)
        {
            // row[member.Name]
            var columnAccess = Expression.Property(
                row,
                "Item",
                Expression.Constant(member.Name)
            );

            // row[member.Name] != DBNull.Value
            var dbNullCheck = Expression.NotEqual(
                columnAccess,
                Expression.Constant(DBNull.Value)
            );

            // 获取成员类型
            Type memberType = member is PropertyInfo prop ?
                prop.PropertyType :
                ((FieldInfo)member).FieldType;

            // Convert.ChangeType(row[member.Name], memberType)
            var convert = Expression.Convert(
                Expression.Call(
                    typeof(Convert),
                    "ChangeType",
                    null,
                    columnAccess,
                    Expression.Constant(memberType)
                ),
                memberType
            );

            // obj.Member = convertedValue
            var assign = Expression.Assign(
                Expression.MakeMemberAccess(obj, member),
                convert
            );

            // if (row[member.Name] != DBNull.Value) { assign }
            return Expression.IfThen(dbNullCheck, assign);
        }
    }
}

