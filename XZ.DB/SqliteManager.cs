using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XZ.DB
{
    public class SqliteManager
    {

        #region 长连接核心配置与实例
        /// <summary>
        /// 全局长连接实例（静态单例）
        /// </summary>
        private static SQLiteConnection? connection;

        /// <summary>
        /// 线程安全锁
        /// </summary>
        private static readonly object _connectionLock = new object();

        public static void CreateConnection(string dbAddress = "hisDatas/xzrd3.db")
        {
            // 创建SQLite连接字符串构建器，并设置数据源和版本
            var connectionStringBuilder = new SQLiteConnectionStringBuilder
            {
                DataSource = dbAddress,
                Version = 3
            };

            // 通过连接字符串构建器创建SQLite连接对象
            connection = new SQLiteConnection(connectionStringBuilder.ConnectionString);
            // 打开数据库连接
            connection.Open();
        }

        /// <summary>
        /// 默认连接字符串（可全局配置）
        /// </summary>
        public static string DefaultConnectionString { get; set; } =
            $"Data Source=hisDatas/xzrd3.db;Version=3;Pooling=False;";
        // 注意：长连接需禁用连接池（Pooling=False），避免连接被池管理回收

        /// <summary>
        /// 连接是否已打开
        /// </summary>
        public static bool IsConnectionOpen => connection != null && connection.State == System.Data.ConnectionState.Open;
        #endregion

        #region 长连接生命周期管理
        /// <summary>
        /// 打开长连接（首次调用创建连接，后续调用直接复用）
        /// </summary>
        public static void Open()
        {
            Open(DefaultConnectionString);
        }

        /// <summary>
        /// 打开长连接（指定连接字符串）
        /// </summary>
        public static void Open(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "连接字符串不能为空");

            lock (_connectionLock) // 线程安全
            {
                if (IsConnectionOpen)
                {
                    return;
                }

                // 释放旧连接（若存在）
                if (connection != null)
                {
                    connection.Dispose();
                    connection = null;
                }

                // 创建新连接并打开
                connection = new SQLiteConnection(connectionString);
                connection.Open();
            }
        }

        /// <summary>
        /// 关闭长连接（释放资源）
        /// </summary>
        public static void Close()
        {
            lock (_connectionLock)
            {
                if (connection == null)
                    return;

                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
                connection.Dispose();
                connection = null;
            }
        }

        /// <summary>
        /// 确保长连接已打开（内部使用，自动重连）
        /// </summary>
        private static void EnsureConnectionOpen()
        {
            if (!IsConnectionOpen)
            {
                Open(); // 自动使用默认连接字符串重连
            }
        }
        #endregion

        /// <summary>
        /// 执行查询SQL命令（如SELECT），返回单个结果。
        /// </summary>
        /// <param name="sql">SQL命令字符串。</param>
        /// <returns>查询结果的单个值。</returns>
        private static object ExecuteScalar(string sql)
        {
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 执行查询并返回单个结果
        /// </summary>
        private static T ExecuteScalar<T>(string sql)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                object result = command.ExecuteScalar();
                return result == DBNull.Value ? default : (T)Convert.ChangeType(result, typeof(T));
            }
        }

        /// <summary>
        /// 执行非查询SQL命令（如INSERT, UPDATE, DELETE）。
        /// </summary>
        /// <param name="sql">SQL命令字符串。</param>
        /// <param name="parameters">SQL命令参数数组。</param>
        /// <returns>命令执行影响的行数。</returns>
        public static int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            try
            {
                try
                {
                    // 使用SQLiteCommand对象执行SQL命令
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        int num = command.ExecuteNonQuery();
                        return num;
                    }
                }
                catch
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        public static bool TableExists(string tableName)
        {
            string sql = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'";
            return Convert.ToInt32(ExecuteScalar(sql)) > 0;
        }

        /// <summary>
        /// 获取表的现有字段名列表
        /// </summary>
        private static List<string> GetExistingColumns(string tableName)
        {
            List<string> columns = new List<string>();
            string sql = $"PRAGMA table_info({tableName});"; // SQLite 内置语句，查询表结构

            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // 第 2 列是字段名（PRAGMA table_info 返回结果：cid, name, type, notnull, dflt_value, pk）
                        string columnName = reader.GetString(1);
                        columns.Add(columnName);
                    }
                }
            }
            return columns;
        }

        /// <summary>
        /// 生成 CREATE TABLE 语句（复用之前的逻辑）
        /// </summary>
        private static string GenerateCreateTableSql<T>() where T : class
        {
            Type entityType = typeof(T);
            string tableName = GetTableName<T>();
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");

            // 筛选：仅保留「公共属性 + 无 [NotMapped] 特性」的属性
            PropertyInfo[] properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !prop.IsDefined(typeof(NotMappedAttribute), inherit: false)) // 过滤排除字段
                .ToArray();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo prop = properties[i];
                string columnDef = GetColumnDefinition(prop, isNewTable: true); // 新增 isNewTable 参数，区分建表和加字段
                sqlBuilder.AppendLine($"  {columnDef}");
                if (i < properties.Length - 1)
                    sqlBuilder.Append(',');
            }

            sqlBuilder.AppendLine(");");
            return sqlBuilder.ToString();
        }

        /// <summary>
        /// 生成单个字段定义（改造：支持建表和加字段两种场景）
        /// </summary>
        private static string GetColumnDefinition(PropertyInfo prop, bool isNewTable)
        {
            StringBuilder columnBuilder = new StringBuilder();
            string columnName = GetColumnName(prop);
            string sqliteType = GetSqliteType(prop.PropertyType);

            // 字段名 + 类型
            columnBuilder.Append($"{columnName} {sqliteType}");

            // 1. 主键约束：仅建表时生效（SQLite 不允许 ALTER TABLE 添加主键）
            if (isNewTable && prop.IsDefined(typeof(KeyAttribute)))
            {
                columnBuilder.Append(" PRIMARY KEY");
                DatabaseGeneratedAttribute dbGenAttr = prop.GetCustomAttribute<DatabaseGeneratedAttribute>();
                if (dbGenAttr != null && dbGenAttr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
                {
                    columnBuilder.Append(" AUTOINCREMENT");
                }
            }

            // 2. 非空约束：建表时直接加；加字段时需指定默认值（否则现有数据会有 NULL 冲突）
            if (prop.IsDefined(typeof(RequiredAttribute)) && !IsNullableType(prop.PropertyType))
            {
                if (isNewTable)
                {
                    columnBuilder.Append(" NOT NULL");
                }
                else
                {
                    // 现有表添加非空字段 → 必须指定默认值
                    string defaultValue = GetDefaultValue(prop.PropertyType);
                    columnBuilder.Append($" NOT NULL DEFAULT {defaultValue}");
                }
            }

            // 3. 字符串长度约束：仅建表时生效（SQLite ALTER TABLE 不支持添加 CHECK 约束，需单独执行）
            if (isNewTable)
            {
                MaxLengthAttribute maxLenAttr = prop.GetCustomAttribute<MaxLengthAttribute>();
                if (maxLenAttr != null && sqliteType.Equals("TEXT", StringComparison.OrdinalIgnoreCase))
                {
                    columnBuilder.Append($" CHECK(LENGTH({columnName}) <= {maxLenAttr.Length})");
                }
            }

            return columnBuilder.ToString();
        }

        /// <summary>
        /// 生成 ADD COLUMN 语句（新增方法）
        /// </summary>
        private static string GenerateAddColumnSql(string tableName, PropertyInfo prop)
        {
            // SQLite ALTER TABLE 限制：不能添加主键、不能添加自增字段、不能添加 WITHOUT ROWID 等
            if (prop.IsDefined(typeof(KeyAttribute)))
            {
                return null;
            }

            string columnDef = GetColumnDefinition(prop, isNewTable: false);
            // 用 IF NOT EXISTS 避免重复添加（SQLite 3.33.0+ 支持，若版本较低可移除，通过字段比对避免）
            return $"ALTER TABLE {tableName} ADD COLUMN {columnDef};";
        }

        /// <summary>
        /// 获取字段默认值（用于现有表添加非空字段）
        /// </summary>
        private static string GetDefaultValue(Type type)
        {
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return underlyingType.Name switch
            {
                nameof(Int32) => "0",
                nameof(Int64) => "0",
                nameof(Byte) => "0",
                nameof(Boolean) => "0", // false
                nameof(Single) => "0.00",
                nameof(Double) => "0.00",
                nameof(Decimal) => "0.00",
                nameof(String) => "''", // 空字符串
                nameof(DateTime) => $"'{DateTime.MinValue:yyyy-MM-dd HH:mm:ss}'", // 当前时间
                nameof(Guid) => $"'{Guid.Empty}'",
                _ => "NULL" // 其他类型默认 NULL（需确保字段允许空）
            };
        }

        /// <summary>
        /// C# 类型 → SQLite 类型映射（复用之前的逻辑）
        /// </summary>
        private static string GetSqliteType(Type clrType)
        {
            // 处理可空类型（如 int?、Enum?）
            Type underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;

            // 新增：判断是否为枚举类型（Enum）
            if (underlyingType.IsEnum)
            {
                return "INTEGER"; // 枚举映射为 SQLite 的 INTEGER 类型（存储枚举的 int 值）
            }

            return underlyingType.Name switch
            {
                nameof(Int32) => "INTEGER",
                nameof(Int64) => "INTEGER",
                nameof(Byte) => "INTEGER",
                nameof(Boolean) => "INTEGER",
                nameof(Single) => "REAL",
                nameof(Double) => "REAL",
                nameof(Decimal) => "REAL",
                nameof(String) => "TEXT",
                nameof(DateTime) => "TEXT",
                nameof(Guid) => "TEXT",
                _ => "BLOB"
            };
        }

        /// <summary>
        /// 判断是否为可空类型（复用之前的逻辑）
        /// </summary>
        private static bool IsNullableType(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// 获取表名（复用之前的逻辑）
        /// </summary>
        private static string GetTableName<T>() where T : class
        {
            Type entityType = typeof(T);
            TableAttribute tableAttr = entityType.GetCustomAttribute<TableAttribute>();
            return tableAttr?.Name ?? entityType.Name;
        }

        private static string GetColumnName(PropertyInfo prop)
        {
            var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
            string columnName = columnAttr?.Name ?? prop.Name;
            // 包含特殊字符时添加反引号
            if (columnName.Contains("-") || columnName.Contains(" "))
            {
                columnName = $"`{columnName}`";
            }
            return columnName;
        }

        /// <summary>
        /// 判断属性是否为自增主键（[Key] + [DatabaseGenerated(DatabaseGeneratedOption.Identity)]）
        /// </summary>
        private static bool IsAutoIncrementPrimaryKey(PropertyInfo prop)
        {
            bool isKey = prop.IsDefined(typeof(KeyAttribute), inherit: false);
            if (!isKey) return false;

            var dbGenAttr = prop.GetCustomAttribute<DatabaseGeneratedAttribute>();
            return dbGenAttr != null && dbGenAttr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
        }

        /// <summary>
        /// 判断数据库字段是否为主键（适配自定义字段名）
        /// </summary>
        private static bool IsPrimaryKeyColumn(Type entityType, string dbColumnName)
        {
            // 查找类中标记 [Key] 的属性，对比其对应的数据库字段名
            var keyProp = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(prop => prop.IsDefined(typeof(KeyAttribute), inherit: false));

            if (keyProp == null) return false;
            string keyColumnName = GetColumnName(keyProp); // 主键的自定义字段名
            return string.Equals(keyColumnName, dbColumnName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断对象是否为默认值（值类型为默认值，引用类型为 null）
        /// </summary>
        private static bool IsDefaultValue(object value, Type type)
        {
            if (value == null)
                return true; // 引用类型 null 视为默认值

            if (type.IsValueType)
            {
                object defaultValue = Activator.CreateInstance(type);
                return value.Equals(defaultValue); // 值类型与默认值比较
            }

            return false;
        }

        /// <summary>
        /// 判断属性是否为枚举类型（含可空枚举，如 Enum?）
        /// </summary>
        private static bool IsEnumProperty(PropertyInfo prop)
        {
            Type propType = prop.PropertyType;
            if (propType.IsEnum)
                return true;
            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                return propType.GetGenericArguments()[0].IsEnum;
            return false;
        }

        private static object ConvertEnumToDbValue(object enumValue)
        {
            if (enumValue == null)
                return DBNull.Value;

            Type enumType = enumValue.GetType();
            if (!enumType.IsEnum)
                throw new ArgumentException($"参数 {nameof(enumValue)} 不是枚举类型");

            // 方案1：存储枚举的 int 值（推荐，占用空间小、查询效率高）
            return Convert.ToInt32(enumValue);
        }

        /// <summary>
        /// 将数据库值转换为枚举类型（与 ConvertEnumToDbValue 存储方案对应）
        /// </summary>
        private static object ConvertDbValueToEnum(Type enumType, object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value)
                return null;

            if (!enumType.IsEnum)
                throw new ArgumentException($"目标类型 {enumType.Name} 不是枚举类型");

            // 与存储方案对应：方案1（int值）→ 转换为枚举
            if (dbValue is int intValue)
            {
                if (Enum.IsDefined(enumType, intValue))
                    return Enum.ToObject(enumType, intValue);
                else
                    throw new InvalidOperationException($"数据库值 {intValue} 不是枚举 {enumType.Name} 的有效成员");
            }

            if (dbValue is long longValue)
            {
                int intValue1 = (int)longValue;
                if (Enum.IsDefined(enumType, intValue1))
                    return Enum.ToObject(enumType, intValue1);
                else
                    throw new InvalidOperationException($"数据库值 {intValue1} 不是枚举 {enumType.Name} 的有效成员");
            }

            // 与存储方案对应：方案2（名称字符串）→ 转换为枚举
            if (dbValue is string stringValue)
            {
                return Enum.Parse(enumType, stringValue, ignoreCase: true);
            }

            throw new NotSupportedException($"数据库值类型 {dbValue.GetType().Name} 无法转换为枚举 {enumType.Name}");
        }

        /// <summary>
        /// 兼容 SQLite 3 低版本：删除表中多余字段（通过临时表重建）
        /// </summary>
        /// <param name="originalTableName">原表名</param>
        /// <param name="tempTableName">临时表名</param>
        /// <param name="keepColumnNames">需要保留的字段名（类中存在的字段）</param>
        private static void CompatDropColumns(string originalTableName, string tempTableName, List<string> keepColumnNames)
        {
            // 步骤1：获取原表的完整结构（字段定义+主键+约束）
            string originalTableSql = GetOriginalTableCreateSql(originalTableName);
            if (string.IsNullOrEmpty(originalTableSql))
            {
                throw new InvalidOperationException($"无法获取表 [{originalTableName}] 的创建语句，无法删除多余字段");
            }

            // 步骤2：生成临时表的创建语句（仅保留需要的字段）
            string tempTableSql = GenerateTempTableCreateSql(originalTableSql, tempTableName, keepColumnNames);
            ExecuteNonQuery(tempTableSql);

            // 步骤3：复制原表中需要保留的字段数据到临时表
            string copyDataSql = $"INSERT INTO {tempTableName} ({string.Join(", ", keepColumnNames)}) " +
                                $"SELECT {string.Join(", ", keepColumnNames)} FROM {originalTableName};";
            ExecuteNonQuery(copyDataSql);

            // 步骤4：删除原表
            string dropOriginalSql = $"DROP TABLE {originalTableName};";
            ExecuteNonQuery(dropOriginalSql);

            // 步骤5：将临时表重命名为原表名
            string renameSql = $"ALTER TABLE {tempTableName} RENAME TO {originalTableName};";
            ExecuteNonQuery(renameSql);
        }

        /// <summary>
        /// 获取原表的创建语句（从 sqlite_master 中读取）
        /// </summary>
        private static string GetOriginalTableCreateSql(string tableName)
        {
            string sql = $"SELECT sql FROM sqlite_master WHERE type='table' AND name='{tableName}';";
            string createSql = ExecuteScalar<string>(sql);
            // 移除原表名后的分号和多余字符（确保语法正确）
            return createSql?.TrimEnd(';', ' ', '\n', '\r') ?? string.Empty;
        }

        /// <summary>
        /// 获取枚举属性的实际枚举类型（处理可空枚举）
        /// </summary>
        private static Type GetEnumType(PropertyInfo prop)
        {
            Type propType = prop.PropertyType;
            if (propType.IsEnum)
                return propType;
            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                return propType.GetGenericArguments()[0];
            throw new ArgumentException($"属性 {prop.Name} 不是枚举类型");
        }

        /// <summary>
        /// 生成临时表的创建语句（仅保留需要的字段）
        /// </summary>
        private static string GenerateTempTableCreateSql(string originalTableSql, string tempTableName, List<string> keepColumnNames)
        {
            // 提取原表的字段定义部分（去掉 CREATE TABLE 表名 ( ... ) 外层结构）
            int startIdx = originalTableSql.IndexOf('(') + 1;
            int endIdx = originalTableSql.LastIndexOf(')');
            if (startIdx < 0 || endIdx < startIdx)
            {
                throw new FormatException($"原表创建语句格式异常：{originalTableSql}");
            }

            string originalColumnsPart = originalTableSql.Substring(startIdx, endIdx - startIdx);
            StringBuilder tempColumnsPart = new StringBuilder();

            // 按逗号分割字段定义（处理字段中可能包含的逗号，如 CHECK 约束中的逗号）
            var columnDefinitions = SplitColumnDefinitions(originalColumnsPart);
            foreach (var colDef in columnDefinitions)
            {
                string colDefTrimmed = colDef.Trim();
                if (string.IsNullOrEmpty(colDefTrimmed)) continue;

                // 提取字段名（第一个空格前的内容）
                string colName = colDefTrimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                // 只保留需要的字段
                if (keepColumnNames.Contains(colName, StringComparer.OrdinalIgnoreCase))
                {
                    if (tempColumnsPart.Length > 0)
                        tempColumnsPart.AppendLine(",");
                    tempColumnsPart.Append(colDefTrimmed);
                }
            }

            // 生成临时表创建语句
            return $"CREATE TABLE {tempTableName} (\n{tempColumnsPart.ToString()}\n);";
        }

        /// <summary>
        /// 分割字段定义（处理字段中包含的逗号，如 CHECK 约束）
        /// </summary>
        private static List<string> SplitColumnDefinitions(string columnsPart)
        {
            List<string> result = new List<string>();
            int bracketCount = 0;
            int startIdx = 0;

            for (int i = 0; i < columnsPart.Length; i++)
            {
                char c = columnsPart[i];
                if (c == '(') bracketCount++;
                else if (c == ')') bracketCount--;
                else if (c == ',' && bracketCount == 0)
                {
                    // 仅分割字段之间的逗号（括号内的逗号不分割）
                    result.Add(columnsPart.Substring(startIdx, i - startIdx));
                    startIdx = i + 1;
                }
            }

            // 添加最后一个字段
            result.Add(columnsPart.Substring(startIdx));
            return result;
        }

        private static void AddColumnLengthConstraint(string tableName, PropertyInfo prop)
        {
            var maxLenAttr = prop.GetCustomAttribute<MaxLengthAttribute>();
            if (maxLenAttr == null) return;

            string columnName = GetColumnName(prop); // 用自定义字段名
            string sqliteType = GetSqliteType(prop.PropertyType);
            if (sqliteType.Equals("TEXT", StringComparison.OrdinalIgnoreCase))
            {
                string constraintName = $"CK_{tableName}_{columnName}_Length"; // 约束名用字段名
                string sql = $"ALTER TABLE {tableName} ADD CONSTRAINT {constraintName} CHECK(LENGTH({columnName}) <= {maxLenAttr.Length});";
                try
                {
                    ExecuteNonQuery(sql);
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode == 19) // 约束已存在
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 给 SQLiteCommand 添加参数（支持匿名对象传参）
        /// </summary>
        private static void AddParametersToCommand(SQLiteCommand command, object parameters)
        {
            if (parameters == null)
                return;

            Type paramType = parameters.GetType();
            // 遍历匿名对象的属性，添加为 SQL 参数
            foreach (var prop in paramType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                string paramName = $"@{prop.Name}";
                object value = prop.GetValue(parameters) ?? DBNull.Value;
                command.Parameters.AddWithValue(paramName, value);
            }
        }

        /// <summary>
        /// 创建或更新表（支持完全对齐表结构）
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="alignFullStructure">是否完全对齐表结构（true=删除类中不存在的字段，false=只添加缺失字段，默认false）</param>
        public static void CreateOrAlterTable<T>(bool alignFullStructure = false) where T : class
        {
            EnsureConnectionOpen();

            Type entityType = typeof(T);
            string tableName = GetTableName<T>();
            string tempTableName = $"temp_{tableName}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";

            bool tableExists = TableExists(tableName);

            if (!tableExists)
            {
                string createTableSql = GenerateCreateTableSql<T>();
                ExecuteNonQuery(createTableSql);
            }
            else
            {
                List<string> existingColumns = GetExistingColumns(tableName); // 数据库中实际字段名（可能是自定义的）
                var entityProperties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(prop => !prop.IsDefined(typeof(NotMappedAttribute), inherit: false))
                    .ToArray();

                // 1. 添加缺失字段（用自定义字段名比对）
                foreach (var prop in entityProperties)
                {
                    string columnName = GetColumnName(prop); // 用自定义字段名
                    if (!existingColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
                    {
                        string addColumnSql = GenerateAddColumnSql(tableName, prop);
                        if (!string.IsNullOrEmpty(addColumnSql))
                        {
                            ExecuteNonQuery(addColumnSql);
                            AddColumnLengthConstraint(tableName, prop);
                        }
                    }
                }

                // 2. 完全对齐：删除多余字段（用自定义字段名比对）
                if (alignFullStructure)
                {
                    // 类中需要保留的字段名（自定义字段名）
                    List<string> entityColumnNames = entityProperties.Select(prop => GetColumnName(prop)).ToList();
                    // 筛选表中存在但类中没有的字段（排除主键）
                    var columnsToDelete = existingColumns
                        .Where(col => !entityColumnNames.Contains(col, StringComparer.OrdinalIgnoreCase))
                        .Where(col => !IsPrimaryKeyColumn(entityType, col)) // 主键保护
                        .ToList();

                    if (columnsToDelete.Any())
                    {
                        CompatDropColumns(tableName, tempTableName, entityColumnNames); // 传入自定义字段名列表
                    }
                }
            }
        }

        /// <summary>
        /// 插入单条实体数据（自动处理枚举字段：默认存 int 值，支持可空枚举）
        /// </summary>
        /// <typeparam name="T">实体类（支持含枚举/可空枚举字段）</typeparam>
        /// <param name="entity">要插入的实体对象</param>
        /// <returns>是否插入成功</returns>
        public static bool Insert<T>(T entity) where T : class, new()
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "插入的实体不能为空");

            EnsureConnectionOpen();
            Type entityType = typeof(T);
            string tableName = GetTableName<T>();

            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !prop.IsDefined(typeof(NotMappedAttribute), inherit: false))
                .Where(prop => !IsAutoIncrementPrimaryKey(prop))
                .ToArray();

            if (!properties.Any())
                throw new InvalidOperationException("实体没有可映射的字段");

            string columnNames = string.Join(", ", properties.Select(prop => GetColumnName(prop)));
            string paramNames = string.Join(", ", properties.Select(prop => $"@{GetColumnName(prop)}"));

            string sql = $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramNames});";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                foreach (var prop in properties)
                {
                    string paramName = $"@{GetColumnName(prop)}";
                    object propValue = prop.GetValue(entity) ?? DBNull.Value;

                    if (IsEnumProperty(prop))
                    {
                        if (propValue != DBNull.Value)
                            propValue = ConvertEnumToDbValue(propValue);
                    }

                    command.Parameters.AddWithValue(paramName, propValue);
                }

                try
                {
                    int affectedRows = command.ExecuteNonQuery();
                    return affectedRows > 0;
                }
                catch (SQLiteException)
                {
                    return false;
                }
            }
        }

        public static int InsertReturnID<T>(T entity) where T : class, new()
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "插入的实体不能为空");

            EnsureConnectionOpen();
            Type entityType = typeof(T);
            string tableName = GetTableName<T>();

            // 获取所有可映射的属性（排除NotMapped和自增主键）
            var allProperties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !prop.IsDefined(typeof(NotMappedAttribute), inherit: false))
                .ToArray();

            // 分离自增主键和普通属性
            var autoIncrementPk = allProperties.FirstOrDefault(prop => IsAutoIncrementPrimaryKey(prop));
            var normalProperties = allProperties
                .Where(prop => !IsAutoIncrementPrimaryKey(prop))
                .ToArray();

            if (!normalProperties.Any() && autoIncrementPk == null)
                throw new InvalidOperationException("实体没有可映射的字段");

            // 构建SQL语句
            string columnNames = string.Join(", ", normalProperties.Select(prop => GetColumnName(prop)));
            string paramNames = string.Join(", ", normalProperties.Select(prop => $"@{GetColumnName(prop)}"));

            // SQLite获取自增主键需要添加 SELECT last_insert_rowid()
            string sql = $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramNames}); SELECT last_insert_rowid();";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;

                // 添加参数
                foreach (var prop in normalProperties)
                {
                    string paramName = $"@{GetColumnName(prop)}";
                    object propValue = prop.GetValue(entity) ?? DBNull.Value;

                    if (IsEnumProperty(prop) && propValue != DBNull.Value)
                    {
                        propValue = ConvertEnumToDbValue(propValue);
                    }

                    command.Parameters.AddWithValue(paramName, propValue);
                }

                try
                {
                    // 执行并获取自增主键值
                    object result = command.ExecuteScalar();

                    // 处理主键返回值
                    if (result != null && result != DBNull.Value && int.TryParse(result.ToString(), out int primaryKey))
                    {
                        // 如果实体有自增主键属性，将值设置回实体
                        if (autoIncrementPk != null)
                        {
                            autoIncrementPk.SetValue(entity, primaryKey);
                        }
                        return primaryKey;
                    }

                    // 如果没有返回自增主键（例如主键不是自增的）
                    throw new InvalidOperationException("无法获取插入后的主键值，可能主键不是自增类型");
                }
                catch (SQLiteException ex)
                {
                    // 记录异常信息并重新抛出或返回特定值
                    throw new InvalidOperationException($"插入数据时发生SQLite错误: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"插入数据失败: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 批量插入实体（带事务，原子性操作：要么全成功，要么全失败）
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="entities">待插入的实体集合</param>
        /// <returns>是否插入成功（true=全部插入，false=插入失败）</returns>
        /// <exception cref="ArgumentNullException">实体集合为空时抛出</exception>
        /// <exception cref="InvalidOperationException">实体无映射字段时抛出</exception>
        public static bool BulkInsert<T>(IEnumerable<T> entities) where T : class, new()
        {
            // 1. 空值校验
            if (entities == null)
                throw new ArgumentNullException(nameof(entities), "批量插入的实体集合不能为空");
            var entityList = entities.ToList();
            if (entityList.Count == 0)
                return false;

            EnsureConnectionOpen();
            Type entityType = typeof(T);
            string tableName = GetTableName<T>();

            // 2. 获取可映射字段（过滤自增主键、NotMapped特性）
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !prop.IsDefined(typeof(NotMappedAttribute), inherit: false))
                .Where(prop => !IsAutoIncrementPrimaryKey(prop))
                .ToArray();

            if (!properties.Any())
                throw new InvalidOperationException("实体没有可映射的字段（已过滤自增主键/NotMapped字段）");

            // 3. 构建列名和参数化VALUES语句
            string columnNames = string.Join(", ", properties.Select(prop => GetColumnName(prop)));
            var valueClauses = new List<string>();
            var parameters = new List<SQLiteParameter>();

            // 遍历实体生成参数和VALUES子句（参数名加索引避免冲突）
            for (int i = 0; i < entityList.Count; i++)
            {
                T entity = entityList[i];
                var paramNames = new List<string>();

                foreach (var prop in properties)
                {
                    string paramName = $"@{GetColumnName(prop)}_{i}";
                    paramNames.Add(paramName);

                    // 获取属性值并处理空值/枚举
                    object propValue = prop.GetValue(entity) ?? DBNull.Value;
                    if (IsEnumProperty(prop) && propValue != DBNull.Value)
                    {
                        propValue = ConvertEnumToDbValue(propValue);
                    }

                    parameters.Add(new SQLiteParameter(paramName, propValue));
                }

                valueClauses.Add($"({string.Join(", ", paramNames)})");
            }

            // 4. 构建最终SQL
            string valuesClause = string.Join(", ", valueClauses);
            string sql = $"INSERT INTO {tableName} ({columnNames}) VALUES {valuesClause};";

            // 5. 开启事务执行批量插入（原子性保障）
            SQLiteTransaction transaction = null;
            try
            {
                transaction = connection.BeginTransaction(); // 启动事务

                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction; // 绑定事务
                    command.CommandText = sql;
                    command.Parameters.AddRange(parameters.ToArray());

                    int affectedRows = command.ExecuteNonQuery();
                    // 验证插入行数是否匹配（确保所有实体都插入成功）
                    if (affectedRows == entityList.Count)
                    {
                        transaction.Commit(); // 提交事务
                        return true;
                    }
                    else
                    {
                        transaction.Rollback(); // 行数不匹配，回滚
                        return false;
                    }
                }
            }
            catch (SQLiteException)
            {
                transaction?.Rollback(); // 异常时回滚事务
                return false;
            }
            finally
            {
                transaction?.Dispose(); // 释放事务资源
            }
        }

        /// <summary>
        /// 按条件查询单条数据（自动将数据库值转换为枚举类型，支持可空枚举）
        /// </summary>
        /// <typeparam name="T">实体类（支持含枚举/可空枚举字段）</typeparam>
        /// <param name="whereClause">查询条件（枚举条件需传 int 值或名称字符串，与存储方案对应）</param>
        /// <param name="parameters">条件参数（枚举参数会自动转换为数据库存储值）</param>
        /// <returns>查询到的实体对象（枚举字段已正确转换）</returns>
        public static T QuerySingle<T>(string whereClause = null, object parameters = null) where T : class, new()
        {
            EnsureConnectionOpen();
            Type entityType = typeof(T);
            string tableName = GetTableName<T>();

            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !prop.IsDefined(typeof(NotMappedAttribute), inherit: false))
                .ToArray();
            string columnNames = string.Join(", ", properties.Select(prop => GetColumnName(prop)));

            string sql = string.IsNullOrEmpty(whereClause)
                ? $"SELECT {columnNames} FROM {tableName} LIMIT 1;"
                : $"SELECT {columnNames} FROM {tableName} WHERE {whereClause} LIMIT 1;";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                AddParametersToCommand(command, parameters);

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            T entity = new T();
                            foreach (var prop in properties)
                            {
                                string columnName = GetColumnName(prop);
                                int ordinal = reader.GetOrdinal(columnName);
                                if (reader.IsDBNull(ordinal))
                                {
                                    prop.SetValue(entity, prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null);
                                    continue;
                                }

                                object dbValue = reader.GetValue(ordinal);
                                if (IsEnumProperty(prop))
                                {
                                    Type enumType = GetEnumType(prop);
                                    dbValue = ConvertDbValueToEnum(enumType, dbValue);
                                }
                                else
                                {
                                    dbValue = Convert.ChangeType(dbValue, prop.PropertyType);
                                }

                                prop.SetValue(entity, dbValue);
                            }
                            return entity;
                        }
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 按条件查询多条数据（自动处理枚举字段的数据库值与枚举类型互转）
        /// </summary>
        /// <typeparam name="T">实体类（支持含枚举/可空枚举字段）</typeparam>
        /// <param name="whereClause">查询条件（枚举条件示例："Status = @Status"，参数传枚举值即可）</param>
        /// <param name="parameters">条件参数（枚举参数会自动转换为数据库存储值）</param>
        /// <param name="orderBy">排序条件</param>
        /// <returns>查询到的实体列表（枚举字段已正确转换）</returns>
        public static List<T> QueryList<T>(string whereClause = null, object parameters = null, string orderBy = null) where T : class, new()
        {
            EnsureConnectionOpen();
            Type entityType = typeof(T);
            string tableName = GetTableName<T>();
            var resultList = new List<T>();

            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !prop.IsDefined(typeof(NotMappedAttribute), inherit: false))
                .ToArray();
            string columnNames = string.Join(", ", properties.Select(prop => GetColumnName(prop)));

            var sqlBuilder = new System.Text.StringBuilder($"SELECT {columnNames} FROM {tableName}");
            if (!string.IsNullOrEmpty(whereClause))
                sqlBuilder.Append($" WHERE {whereClause}");
            if (!string.IsNullOrEmpty(orderBy))
                sqlBuilder.Append($" ORDER BY {orderBy}");
            string sql = sqlBuilder.ToString();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                AddParametersToCommand(command, parameters);

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            T entity = new T();
                            foreach (var prop in properties)
                            {
                                string columnName = GetColumnName(prop);
                                int ordinal = reader.GetOrdinal(columnName);
                                object dbValue = reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);

                                if (IsEnumProperty(prop))
                                {
                                    if (dbValue != null)
                                    {
                                        Type enumType = GetEnumType(prop);
                                        dbValue = ConvertDbValueToEnum(enumType, dbValue);
                                    }
                                }
                                else if (dbValue != null)
                                {
                                    dbValue = Convert.ChangeType(dbValue, prop.PropertyType);
                                }

                                if (dbValue == null)
                                {
                                    prop.SetValue(entity, prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null);
                                }
                                else
                                {
                                    prop.SetValue(entity, dbValue);
                                }
                            }
                            resultList.Add(entity);
                        }
                    }
                    return resultList;
                }
                catch
                {
                    return resultList;
                }
            }
        }

        /// <summary>
        /// 按主键更新单条数据（自动处理枚举字段：数据库值与枚举类型互转）
        /// </summary>
        /// <typeparam name="T">实体类（支持含枚举/可空枚举字段）</typeparam>
        /// <param name="entity">要更新的实体对象（主键必须赋值）</param>
        /// <returns>是否更新成功</returns>
        public static bool Update<T>(T entity) where T : class, new()
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "更新的实体不能为空");

            EnsureConnectionOpen();
            Type entityType = typeof(T);
            string tableName = GetTableName<T>();

            var keyProp = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(prop => prop.IsDefined(typeof(KeyAttribute), inherit: false));
            if (keyProp == null)
                throw new InvalidOperationException("实体类必须包含 [Key] 特性标记的主键字段");

            var updateProperties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => !prop.IsDefined(typeof(NotMappedAttribute), inherit: false))
                .Where(prop => prop != keyProp)
                //.Where(prop => !IsDefaultValue(prop.GetValue(entity), prop.PropertyType))
                .ToArray();

            if (!updateProperties.Any())
            {
                return false;
            }

            string setClause = string.Join(", ", updateProperties.Select(prop => $"{GetColumnName(prop)} = @{GetColumnName(prop)}"));
            string keyColumnName = GetColumnName(keyProp);
            string sql = $"UPDATE {tableName} SET {setClause} WHERE {keyColumnName} = @{keyColumnName};";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                foreach (var prop in updateProperties)
                {
                    string paramName = $"@{GetColumnName(prop)}";
                    object propValue = prop.GetValue(entity) ?? DBNull.Value;

                    if (IsEnumProperty(prop))
                    {
                        if (propValue != DBNull.Value)
                            propValue = ConvertEnumToDbValue(propValue);
                    }

                    command.Parameters.AddWithValue(paramName, propValue);
                }

                object keyValue = keyProp.GetValue(entity) ?? throw new InvalidOperationException("主键字段不能为空");
                if (IsEnumProperty(keyProp))
                {
                    keyValue = ConvertEnumToDbValue(keyValue);
                }
                command.Parameters.AddWithValue($"@{keyColumnName}", keyValue);

                try
                {
                    int affectedRows = command.ExecuteNonQuery();
                    return affectedRows > 0;
                }
                catch (SQLiteException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 按主键删除单条数据（支持主键为枚举类型）
        /// </summary>
        /// <typeparam name="T">实体类（主键可为枚举/可空枚举）</typeparam>
        /// <param name="keyValue">主键值（枚举类型会自动转换为数据库存储值）</param>
        /// <returns>是否删除成功</returns>
        public static bool DeleteByKey<T>(object keyValue) where T : class, new()
        {
            if (keyValue == null)
                throw new ArgumentNullException(nameof(keyValue), "主键值不能为空");

            EnsureConnectionOpen();
            Type entityType = typeof(T);
            string tableName = GetTableName<T>();

            var keyProp = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(prop => prop.IsDefined(typeof(KeyAttribute), inherit: false));
            if (keyProp == null)
                throw new InvalidOperationException("实体类必须包含 [Key] 特性标记的主键字段");

            string keyColumnName = GetColumnName(keyProp);
            string sql = $"DELETE FROM {tableName} WHERE {keyColumnName} = @{keyColumnName};";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                if (IsEnumProperty(keyProp))
                {
                    keyValue = ConvertEnumToDbValue(keyValue);
                }
                command.Parameters.AddWithValue($"@{keyColumnName}", keyValue);

                try
                {
                    int affectedRows = command.ExecuteNonQuery();
                    return affectedRows > 0;
                }
                catch (SQLiteException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 按条件删除多条数据（支持枚举类型条件参数）
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="whereClause">删除条件（枚举条件示例："Status = @Status"）</param>
        /// <param name="parameters">条件参数（枚举参数会自动转换为数据库存储值）</param>
        /// <returns>删除的行数</returns>
        public static int DeleteByCondition<T>(string whereClause, object parameters) where T : class, new()
        {
            if (string.IsNullOrEmpty(whereClause))
                throw new ArgumentException("删除条件不能为空，避免误删全表", nameof(whereClause));

            EnsureConnectionOpen();
            Type entityType = typeof(T);
            string tableName = GetTableName<T>();

            string sql = $"DELETE FROM {tableName} WHERE {whereClause};";

            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                AddParametersToCommand(command, parameters);

                try
                {
                    return command.ExecuteNonQuery();
                }
                catch (SQLiteException)
                {
                    return 0;
                }
            }
        }
    }
}