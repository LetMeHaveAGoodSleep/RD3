using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace XZ.SQLite
{
    /// <summary>
    /// js序列化
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ObjectToString<T>(T obj)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = int.MaxValue;
            return javaScriptSerializer.Serialize(obj);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        public static T StringToObject<T>(string content)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = int.MaxValue;
            return javaScriptSerializer.Deserialize<T>(content);
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object ConvertToType(object obj, Type targetType)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            return javaScriptSerializer.ConvertToType(obj, targetType);
        }
    }

    /// <summary>
    /// SQLite 帮助类
    /// </summary>
    public class SQLiteHelper
    {
        // 用于与SQLite数据库交互的连接对象
        private static SQLiteConnection connection;
        //// 操作的表名
        //private string tableName;
        //// 表的列名，以逗号分隔的字符串
        //private string columnNameStr;
        ////表的列名
        //private string[] columnNames;

        /// <summary>
        /// 通过指定的数据库文件路径初始化SQLiteHelper类的实例。
        /// </summary>
        /// <param name="dbAddress">数据库文件的路径。</param>
        public SQLiteHelper(string dbAddress)
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
        /// 创建连接
        /// </summary>
        /// <param name="dbAddress"></param>
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
        /// 关闭数据库连接。
        /// </summary>
        public static void Close()
        {
            // 如果连接不为空且状态为打开，则关闭连接
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// 创建表，包括指定的列和类型。
        /// </summary>
        /// <param name="tableName">要创建的表名。</param>
        /// <param name="hasAutoIncrementId">是否自动添加自增ID。</param>
        /// <param name="columns">列名数组。</param>
        /// <param name="columnTypes">列类型数组。</param>
        public static void CreateTable(string tableName, bool hasAutoIncrementId, string[] columns, Type[] columnTypes)
        {
            //// 设置当前操作的表名
 
            // 创建列定义列表
            var columnDefinitions = new List<string>();
            // 如果需要自动添加ID列
            if (hasAutoIncrementId)
            {
                columnDefinitions.Add("ID INTEGER PRIMARY KEY AUTOINCREMENT");
            }
            // 遍历列类型数组，添加列定义
            for (int i = 0; i < columns.Length; i++)
            {
                var columnName = columns[i];
                var columnTypeStr = GetColumnType(columnTypes[i]);
                columnDefinitions.Add($"{columnName} {columnTypeStr}");
            }

            // 构建列定义字符串
            string columnDefinitionsStr = string.Join(", ", columnDefinitions);
            // 构建创建表的SQL语句
            string sqlStr = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnDefinitionsStr});";
            // 执行非查询SQL命令创建表
            ExecuteNonQuery(sqlStr);
        }

        /// <summary>
        /// 按类的属性创建表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="hasAutoIncrementId"></param>
        /// <param name="fieldInfo"></param>
        public static void CreateTableByFields(string tableName, bool hasAutoIncrementId, PropertyInfo[] fieldInfo)
        {
            // 创建列定义列表
            var columnDefinitions = new List<string>();
            // 如果需要自动添加ID列
            if (hasAutoIncrementId)
            {
                columnDefinitions.Add("ID INTEGER PRIMARY KEY AUTOINCREMENT");
            }
            // 遍历列类型数组，添加列定义
            for (int i = 0; i < fieldInfo.Length; i++)
            {
                var columnName = fieldInfo[i].Name;
                var columnTypeStr = GetColumnType(fieldInfo[i].PropertyType);
                columnDefinitions.Add($"{columnName} {columnTypeStr}");
            }

            // 构建列定义字符串
            string columnDefinitionsStr = string.Join(", ", columnDefinitions);
            // 构建创建表的SQL语句
            string sqlStr = $"CREATE TABLE IF NOT EXISTS {tableName} ({columnDefinitionsStr});";
            // 执行非查询SQL命令创建表
            ExecuteNonQuery(sqlStr);
        }

        /// <summary>
        /// 删除当前的表
        /// </summary>
        public static void DeleteTable(string tableName)
        {
            string sql = $"DROP TABLE IF EXISTS {tableName};";
            ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 创建索引以提高查询效率，在创建表之后使用
        /// 索引名不能重复，增加表名
        /// </summary>
        /// <param name="columnName">要创建索引的列名。</param>
        public static void CreateIndex(string tableName,string columnName)
        {
            string sql = $"CREATE INDEX IF NOT EXISTS {columnName}_{tableName} ON {tableName} ({columnName});";
            ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 销毁指定的索引。
        /// </summary>
        /// <param name="indexName">要删除的索引的名称。</param>
        public static void DeleteIndex(string columnName)
        {
            string sql = $"DROP INDEX IF EXISTS {columnName};";
            ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 获取C#类型对应的SQLite类型字符串。
        /// </summary>
        /// <param name="type">C#中的类型。</param>
        /// <returns>对应的SQLite类型字符串。</returns>
        private static string GetColumnType(Type type)
        {
            // 根据C#类型返回对应的SQLite类型字符串
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return "INTEGER";
                case TypeCode.Double:
                    return "REAL";
                case TypeCode.Single:
                    return "FLOAT";
                case TypeCode.DateTime:
                    return "DATETIME";
                case TypeCode.Boolean:
                    return "BOOLEAN";

                default:
                    return "TEXT";
            }
        }

        /// <summary>
        /// 向表中插入记录。
        /// </summary>
        /// <param name="values">要插入的值的数组。</param>
        /// <returns>插入操作影响的行数。</returns>
        public static int Insert(string tableName, string columnNameStr, bool returnID,params object[] values)
        {
            // 创建参数列表并初始化
            var parameters = values.Select((value, index) => new SQLiteParameter($"@{index}", value)).ToArray();
            // 构建参数化SQL语句
            var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));
            // 构建插入数据的SQL语句
            if (!returnID)
            {
                string sql = $"INSERT INTO {tableName} ({columnNameStr}) VALUES ({parameterNames});";
                // 执行非查询SQL命令并返回影响的行数
                return ExecuteNonQuery(sql, parameters);
            }
            else
            {
                int id = -1;
                string sql = $"INSERT INTO {tableName} ({columnNameStr}) VALUES ({parameterNames}) RETURNING ID;";
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddRange(parameters);
                    using (var reader = command.ExecuteReader())
                    {
                        if(reader.Read())
                            id = reader.GetInt32(0);
                    }
                }
                return id;
            }
        }

        /// <summary>
        /// 获取多条件的字符串组合
        /// </summary>
        /// <param name="bAnd">True为And逻辑，False 为 OR 逻辑</param>
        /// <param name="condition1"></param>
        /// <param name="condition2"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public static string GetMultiContidion(string tableName, bool bAnd, string condition1, string condition2, params string[] conditions)
        {
            if (bAnd)
            {
                if (conditions != null && conditions.Length > 0)
                {
                    string str1 = string.Join(" And ", conditions);
                    return string.Join(" And ", condition1, condition2, str1);
                }
                else
                {
                    return string.Join(" And ", condition1, condition2);
                }
            }
            else
            {
                if (conditions != null && conditions.Length > 0)
                {
                    string str1 = string.Join(" OR ", conditions);
                    return string.Join(" OR ", condition1, condition2, str1);
                }
                else
                {
                    return string.Join(" OR ", condition1, condition2);
                }
            }
        }

        /// <summary>
        /// 根据条件删除记录。
        /// </summary>
        /// <param name="condition">删除条件。</param>
        /// <returns>删除操作影响的行数。</returns>
        public static int Delete(string tableName, string condition)
        {
            // 构建删除数据的SQL语句
            string sql = $"DELETE FROM {tableName} WHERE {condition};";

            // 执行非查询SQL命令并返回影响的行数
            return ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 更新表中的记录。
        /// </summary>
        /// <param name="columnName">要更新的列名。</param>
        /// <param name="value">新的值。</param>
        /// <param name="condition">更新条件。</param>
        /// <returns>更新操作影响的行数。</returns>
        public static int Update(string tableName, string columnName, object value, string condition)
        {
            // 构建更新数据的SQL语句
            string query = $"UPDATE {tableName} SET {columnName} = @{value} WHERE {condition};";
            // 创建参数对象并添加到SQL命令中
            var parameter = new SQLiteParameter(value.ToString(), value);
            // 执行非查询SQL命令并返回影响的行数
            return ExecuteNonQuery(query, parameter);
        }

        /// <summary>
        /// 更新表中的记录。
        /// </summary>
        /// <param name="columnName">要更新的列名。</param>
        /// <param name="value">新的值。</param>
        /// <param name="condition">更新条件。</param>
        /// <returns>更新操作影响的行数。</returns>
        public static int Updates(string tableName,string[] columnNames, object[] values, string condition)
        {
            // 构建更新数据的SQL语句
            string update = string.Empty;
            for(int i = 0;i< columnNames.Length;i++)
            {
                update += $"{columnNames[i]} = @{values[i]},";
            }
            update = update.TrimEnd(',');

            string query = $"UPDATE {tableName} SET {update} WHERE {condition};";
            // 创建参数对象并添加到SQL命令中
            var parameter = values.Select((value, index) => new SQLiteParameter($"@{index}", value)).ToArray();
            // 执行非查询SQL命令并返回影响的行数
            return ExecuteNonQuery(query, parameter);
        }
        /// <summary>
        /// 根据条件查询列的值。
        /// </summary>
        /// <param name="columnName">要查询的列名。</param>
        /// <param name="condition">查询条件。</param>
        /// <returns>查询结果的值。</returns>
        public static object GetValue(string tableName, string columnName, string condition)
        {
            // 构建查询数据的SQL语句
            string selectQuery = $"SELECT {columnName} FROM {tableName} WHERE {condition};";
            // 执行查询SQL命令并返回查询结果
            return ExecuteScalar(selectQuery);
        }

        /// <summary>
        /// 根据条件查询列的值。
        /// </summary>
        /// <param name="columnName">要查询的列名。</param>
        /// <param name="condition">查询条件。</param>
        /// <returns>查询结果的值。</returns>
        public List<object> GetValues(string tableName, string columnName, string condition)
        {
            List<object> values = new List<object>();

            string selectQuery = "";

            if (string.IsNullOrWhiteSpace(condition))
            {
                selectQuery = $"SELECT {columnName} FROM {tableName};";
            }
            else
            {
                selectQuery = $"SELECT {columnName} FROM {tableName} WHERE {condition};";
            }

            try
            {
                using (var reader = ExecuteQuery(selectQuery))
                {
                    while (reader.Read())
                    {
                        values.Add(reader[columnName]);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return values;
        }

        /// <summary>
        /// 读取数据库数据
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataTable GetDatasToDataTable(string sql)
        {
            // 创建DataTable并填充数据
            DataTable dataTable = new DataTable();
            // 创建SQLite命令对象
            //string sql = "SELECT * FROM your_table_name"; // 替换your_table_name为你的表名
            using (var command = connection.CreateCommand())
            {
                try
                {
                    command.CommandText = sql;
                    SQLiteDataReader reader = command.ExecuteReader(); // 创建SQLite数据阅读器对象
                    dataTable.Load(reader); // 将数据读取器中的数据加载到DataTable中

                    //// 输出DataTable中的数据作为示例
                    //foreach (DataRow row in dataTable.Rows)
                    //{
                    //    Console.WriteLine(string.Join(", ", row.ItemArray)); // 打印每行数据
                    //}
                }
                catch { }
            }

            return dataTable;
        }

        /// <summary>
        /// 根据条件获取所有行的数据
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public List<Dictionary<string, object>> GetRowDatas(string tableName, string[] columnNames, string condition)
        {
            List<Dictionary<string, object>> values = new List<Dictionary<string, object>>();

            string selectQuery = "";
            string columnNameStr = string.Join(",", columnNames);
            if (string.IsNullOrWhiteSpace(condition))
            {
                selectQuery = $"SELECT {columnNameStr} FROM {tableName};";
            }
            else
            {
                selectQuery = $"SELECT {columnNameStr} FROM {tableName} WHERE {condition};";
            }

            try
            {
                using (var reader = ExecuteQuery(selectQuery))
                {
                    while (reader.Read())
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        foreach (var columnName in columnNames)
                        {
                            dict.Add(columnName, reader[columnName]);
                        }
                        values.Add(dict);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return values;
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
                // 使用SQLiteCommand对象执行SQL命令
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                // 记录异常信息到日志文件
                LogException(ex);
                return 0;
            }
        }

        /// <summary>
        /// 执行查询SQL命令（如SELECT），返回SQLiteDataReader对象。
        /// </summary>
        /// <param name="sql">SQL命令字符串。</param>
        /// <returns>SQLiteDataReader对象。</returns>
        public static SQLiteDataReader ExecuteQuery(string sql)
        {
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    return command.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                return null;
            }
        }

        public static DataTable GetDatatable(string sql)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
                {
                    // 使用 DataAdapter 填充 DataTable
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                return null;
            }
            return dataTable;
        }

        public static void CommitTransaction(List<Tuple<string, SQLiteParameter[]>> source)
        {
            try
            {
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var row in source) 
                    {
                        ExecuteNonQuery(row.Item1, row.Item2);
                    }
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                return ;
            }
        }

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
                LogException(ex);
                return null;
            }
        }

        /// <summary>
        /// 记录异常信息到日志文件。
        /// </summary>
        /// <param name="ex">要记录的异常对象。</param>
        private static void LogException(Exception ex)
        {
            // 将异常信息追加到日志文件中
            string errorMessage = $"发生错误：{ex.Message}{Environment.NewLine}{ex.StackTrace}";
            File.AppendAllText("error.log", errorMessage);
        }
    }

    /// <summary>
    /// 业务逻辑
    /// </summary>
    public class RD3SQLHelper
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public static void InitDB()
        {
            SQLiteHelper.CreateConnection();//创建数据库链接
            CreateProjectTable();
            CreateBatchTable();
            //CreateRealTimeParamTable();

            CreateAuditTable();//创建操作日志表
            CreateAlarmRecordTable();
        }

        public static string DataFormat = "yyyy-MM-dd";
        public static string DateTimeFormat = "yyyy-MM-dd mm:ss";
        #region 公用方法

        public static int GetNewID(string tableName)
        {
            string sql = $"SELECT MAX(id) FROM {tableName};";
            int id = 0;

            using (var reader = SQLiteHelper.ExecuteQuery(sql))
            {
                if (reader.Read())
                    try
                    {
                        id = reader.GetInt32(0);
                    }
                    catch(Exception ex)
                    {

                    }
            }
            return id;
        }
        /// <summary>
        /// 初始化字段
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fieldInfos"></param>
        /// <param name="fieldNames"></param>
        private static PropertyInfo[] InitFields(Type type, out string fieldNames)
        {
            fieldNames = string.Empty;
            PropertyInfo[] fieldInfos = type.GetProperties().Where(c => c.CanWrite && c.CanRead && c.Name != "ID").ToArray();//ID 为自增
            foreach (var field in fieldInfos)
            {
                fieldNames += $"{field.Name},";
            }
            fieldNames = fieldNames.TrimEnd(',');
            return fieldInfos;
        }

        /// <summary>
        /// 插入值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fields"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static int InsetValue(object obj,string fieldNames, PropertyInfo[] fields,string tableName,bool returnID = false)
        {
            object[] values = new object[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].Name == "ID")
                    continue;
                values[i] = fields[i].GetValue(obj);
            }
            int id = SQLiteHelper.Insert(tableName, fieldNames, returnID, values);
            return id;
        }
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="propertys"></param>
        /// <param name="reader"></param>
        /// <param name="obj"></param>
        private static void SetPropertyInfoValue(PropertyInfo[] propertys, SQLiteDataReader reader,object obj)
        {
            foreach (var item in propertys)
            {
                object value = reader[item.Name];
                if (value != DBNull.Value)
                {
                    item.SetValue(obj, reader[item.Name]);
                }
                else
                {
                    if(item.PropertyType == typeof(string))
                        item.SetValue(obj, "");
                }
            }
        }


        #endregion

        #region 在线数据
        /// <summary>
        /// 设备编号-数据节点编号-数据节点
        /// </summary>
        public static Dictionary<string, Dictionary<string, dataValue>> pNodeDic = new Dictionary<string, Dictionary<string, dataValue>>();
        private static string pNodeDicPath = AppDomain.CurrentDomain.BaseDirectory + "Data\\pNode.json";

        public static void SaveDataValue()
        {
            string result = JsonHelper.ObjectToString(pNodeDic);
            File.WriteAllText(pNodeDicPath, result);
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public static void InitDataValueNode()
        {
            string result = File.ReadAllText(pNodeDicPath);
            pNodeDic = JsonHelper.StringToObject<Dictionary<string, Dictionary<string, dataValue>>>(result);
        }
        #endregion

        #region 离线数据
        public static Dictionary<string, dataValue> offLineNodeDic = new Dictionary<string, dataValue>();
        #endregion

        #region 在线数据
        public static string realTimeParamTable = "realTimeParamTable";
        /// <summary>
        /// 实时数据数据表
        /// </summary>
        /// <param name="type"></param>
        public static void CreateRealTimeParamTable()
        {
            SQLiteHelper.CreateTable(realTimeParamTable, hasAutoIncrementId: true,
                                    new string[] { "deviceID", "batchID", "dateTime", "realTimeParam",  },
                                    new Type[] { typeof(string), typeof(string), typeof(string), typeof(string)});

            // 创建索引以提高查询效率
            SQLiteHelper.CreateIndex(realTimeParamTable, "deviceID");
            SQLiteHelper.CreateIndex(realTimeParamTable, "batchID");
            SQLiteHelper.CreateIndex(realTimeParamTable, "dateTime");
        }


        /// <summary>
        /// 方成
        /// </summary>
        public static void CreateAuditTable()
        {
            SQLiteHelper.CreateTable("Audit", hasAutoIncrementId: true,
                                    new string[] { "DeviceID", "BatchID", "DateTime", "Remark", },
                                    new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) });

            // 创建索引以提高查询效率
            SQLiteHelper.CreateIndex("Audit", "DeviceID");
            SQLiteHelper.CreateIndex("Audit", "BatchID");
            SQLiteHelper.CreateIndex("Audit", "DateTime");
        }

        /// <summary>
        /// 保存操作数据
        /// </summary>
        /// <param name="v"></param>
        public static void AddAuditRecord(string deviceID, string batchID, string dateTime, string remark)
        {
            int row = SQLiteHelper.Insert("Audit",
               "DeviceID,BatchID,DateTime,Remark",
               true, new object[] { deviceID, batchID, dateTime, remark });
        }


        /// <summary>
        /// 方成
        /// </summary>
        public static void CreateAlarmRecordTable()
        {
            SQLiteHelper.CreateTable("AlarmRecord", hasAutoIncrementId: true,
                                    new string[] { "Reactor","Code", "Source", "Module", "Grade", "Remark", "AlarmTime", "RemoveTime" },
                                    new Type[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string) });

            // 创建索引以提高查询效率
            SQLiteHelper.CreateIndex("AlarmRecord", "Reactor");
            SQLiteHelper.CreateIndex("AlarmRecord", "Code");
            SQLiteHelper.CreateIndex("AlarmRecord", "Source");
            SQLiteHelper.CreateIndex("AlarmRecord", "Module");
            SQLiteHelper.CreateIndex("AlarmRecord", "Grade"); 
        }

        /// <summary>
        /// 保存报警数据
        /// </summary>
        public static int AddAlarmRecord(string reactor,string code,string source,string module,string grade,string remark, string alarmTime)
        {
            int row = SQLiteHelper.Insert("AlarmRecord", "Reactor,Code, Source, Module,Grade, Remark, AlarmTime,RemoveTime",
               true, new object[] { reactor,code, source, module, grade, remark,alarmTime,"" });
            return row;
        }

        /// <summary>
        /// 更新报警信息
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public static bool UpdateAlarmRecord(long id,DateTime removeTime)
        {
            string sql = $"UPDATE AlarmRecord set RemoveTime = '{removeTime}' where ID = {id}";
            int row = SQLiteHelper.ExecuteNonQuery(sql);
            return row > 0;
        }

        public static DataTable QueryAlarmRecord()
        {
            string querySql = $"select * from AlarmRecord order by AlarmTime desc";
            DataTable dataTable = SQLiteHelper.GetDatatable(querySql);
            return dataTable;
        }

        /// <summary>
        /// 保存实时数据1
        /// </summary>
        /// <param name="v"></param>
        public static void AddrealTimeParamDatas(string deviceID, string batchID, string dateTime,string realTimeParam)
        {
            int row = SQLiteHelper.Insert(realTimeParamTable,
               "deviceID,batchID,dateTime,realTimeParam",
               true,new object[] { deviceID, batchID, dateTime, realTimeParam });
        }

        public static string realTimeParamTable1 = "realTimeParamTable1";
        private static string realTimeParamTable1Columns = "";
        /// <summary>
        /// 实时数据数据表
        /// </summary>
        /// <param name="type"></param>
        public static void CreateRealTimeParamTable1(PropertyInfo[] propertyInfos)
        {
            string[] columns = new string[propertyInfos.Length + 3];
            columns[0] = "deviceID";
            columns[1] = "batchID";
            columns[2] = "dateTime";
            Type[] columnTypes = new Type[propertyInfos.Length + 3];
            columnTypes[0] = typeof(string);
            columnTypes[1] = typeof(string);
            columnTypes[2] = typeof(string);
            for (int i = 0;i< propertyInfos.Length;i++)
            {
                columns[i + 3] = propertyInfos[i].Name;
                columnTypes[i + 3] = propertyInfos[i].PropertyType;
            }
            SQLiteHelper.CreateTable(realTimeParamTable1, true,columns, columnTypes);

            foreach (var col in columns)
            {
                realTimeParamTable1Columns += string.Format("{0},", col);
            }
            realTimeParamTable1Columns = realTimeParamTable1Columns.TrimEnd(',');

            // 创建索引以提高查询效率
            SQLiteHelper.CreateIndex(realTimeParamTable1, columns[0]);
            SQLiteHelper.CreateIndex(realTimeParamTable1, columns[1]);
            SQLiteHelper.CreateIndex(realTimeParamTable1, columns[2]);
        }

        /// <summary>
        /// 保存实时数据1
        /// </summary>
        /// <param name="v"></param>
        public static void AddrealTimeParamDatas1(object[] values)
        {
            int row = SQLiteHelper.Insert(realTimeParamTable1, realTimeParamTable1Columns, true, values);
        }


        public static void BulkInsertRealTimeParam(List<object[]> list)
        {
            List<Tuple<string, SQLiteParameter[]>> list1 = new List<Tuple<string, SQLiteParameter[]>>();
            foreach (object[] row in list) 
            {
                var parameters = row.Select((value, index) => new SQLiteParameter($"@{index}", value)).ToArray();
                // 构建参数化SQL语句
                var parameterNames = string.Join(", ", parameters.Select(p => p.ParameterName));

                string sql = $"INSERT INTO {realTimeParamTable1} ({realTimeParamTable1Columns}) VALUES ({parameterNames}) RETURNING ID;";
                list1.Add(Tuple.Create(sql, parameters));
            }
            SQLiteHelper.CommitTransaction(list1);
        }
        #endregion

        #region 实验
        public static string ProjectTable = "rd3project";
        static PropertyInfo[] projectFields;
        static string projectFieldNames = "";
        /// <summary>
        /// 创建表
        /// </summary>
        public static bool CreateProjectTable()
        {
            projectFields = InitFields(typeof(RD3Project), out projectFieldNames);
            SQLiteHelper.CreateTableByFields(ProjectTable, hasAutoIncrementId: true, projectFields);

            //创建索引以提高查询效率
            SQLiteHelper.CreateIndex(ProjectTable, "ownerID");
            SQLiteHelper.CreateIndex(ProjectTable, "startDate"); 
            SQLiteHelper.CreateIndex(ProjectTable, "endDate");
            return true;
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <returns></returns>
        public static bool InsertProject(RD3Project project)
        {
            project.ID = InsetValue(project, projectFieldNames, projectFields, ProjectTable,true);
            return project.ID > 0;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        public static bool UpdataProject(RD3Project project)
        {
            string updateSql = "";
            foreach (var item in projectFields)
            {
                updateSql += $"{item.Name} = '{item.GetValue(project)}',";
            }
            updateSql = updateSql.TrimEnd(',');
            string sql = $"UPDATE {ProjectTable} set {updateSql} where ID = {project.ID}";
            int row = SQLiteHelper.ExecuteNonQuery(sql);

            return row > 0;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        public static bool DeleteProject(RD3Project project)
        {
            string sql = $"delete from {ProjectTable} where ID = {project.ID}";
            int row = SQLiteHelper.ExecuteNonQuery(sql);
            return row>0;
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public static List<RD3Project> QueryProject()
        {
            string querySql = $"select * from {ProjectTable} order by ID";
            List<RD3Project> list = new List<RD3Project>();

            using (var reader = SQLiteHelper.ExecuteQuery(querySql))
            {
                while (reader.Read())
                {
                    var project = new RD3Project() { ID = Convert.ToInt32(reader["ID"].ToString()) };
                    //foreach (var item in projectFields)
                    //{
                    //    item.SetValue(project, reader[item.Name]);
                    //}
                    SetPropertyInfoValue(projectFields,reader,project);
                    list.Add(project);
                }
            }
            return list;
        }
        #endregion

        #region 批次
        public static string BatchTable = "rd3Batch";
        static PropertyInfo[] batchFields;
        static string batchFieldNames = "";
        /// <summary>
        /// 创建表
        /// </summary>
        public static bool CreateBatchTable()
        {
            batchFields = InitFields(typeof(RD3Batch), out batchFieldNames);
            SQLiteHelper.CreateTableByFields(BatchTable, hasAutoIncrementId: true, batchFields);

            //创建索引以提高查询效率
            SQLiteHelper.CreateIndex(BatchTable, "ownerID");
            SQLiteHelper.CreateIndex(BatchTable, "startDate");
            SQLiteHelper.CreateIndex(BatchTable, "endDate");
            return true;
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <returns></returns>
        public static int InsertBatch(RD3Batch batch)
        {
            batch.ID = InsetValue(batch, batchFieldNames, batchFields, BatchTable, true);
            return batch.ID ;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        public static bool UpdataBatch(RD3Batch batch)
        {
            string updateSql = "";
            foreach (var item in batchFields)
            {
                updateSql += $"{item.Name} = '{item.GetValue(batch)}',";
            }
            updateSql = updateSql.TrimEnd(',');
            string sql = $"UPDATE {BatchTable} set {updateSql} where ID = {batch.ID}";
            int row = SQLiteHelper.ExecuteNonQuery(sql);

            return row > 0;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        public static bool DeleteBatch(RD3Batch batch)
        {
            string sql = $"delete from {BatchTable} where ID = {batch.ID}";
            int row = SQLiteHelper.ExecuteNonQuery(sql);
            return row > 0;
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public static List<RD3Batch> QueryBatch()
        {
            string querySql = $"select * from {BatchTable} order by ID";
            List<RD3Batch> list = new List<RD3Batch>();

            using (var reader = SQLiteHelper.ExecuteQuery(querySql))
            {
                while (reader.Read())
                {
                    var batch = new RD3Batch() { ID = Convert.ToInt32(reader["ID"].ToString()) };
                    SetPropertyInfoValue(batchFields, reader, batch);
                    list.Add(batch);
                }
            }
            return list;
        }

        public static RD3Batch QueryBatchByID(int id)
        {
            string querySql = $"select * from {BatchTable} where ID = {id}";
            RD3Batch batch = new RD3Batch(); 

            using (var reader = SQLiteHelper.ExecuteQuery(querySql))
            {
                while (reader.Read())
                {
                    batch = new RD3Batch() { ID = Convert.ToInt32(reader["ID"].ToString()) };
                    SetPropertyInfoValue(batchFields, reader, batch);
                }
            }
            return batch;
        }
        #endregion
    }

    #region 数据库映射类
    /// <summary>
    /// 实验
    /// </summary>
    public class RD3Project
    {
        public int ID;//实验ID 唯一编号
        public string Name { set; get; }//实验名称
        public string createTime  { set; get; }//创建时间
        public string description { set; get; }//实验描述
        public string ownerID { set; get; }//实验创建者ID
        public string ownerName { set; get; }//实验创建者
        public string customer { set; get; }//用户？？
        public string startDate { set; get; }//开始日期
        public string endDate { set; get; }//结束日期
        public long deleteFlag { set; get; }//删除标志
    }

    /// <summary>
    /// 批次
    /// </summary>
    public class RD3Batch
    {
        public int ID { set; get; }//唯一编号
        //public int id { set; get; }//唯一编号
        public string createTime { set; get; }//创建时间
        public string createUser {  set; get; }//创建者
        public string idRemark { set; get; }//用户自定义id描述
        public string description { set; get; }//备注
        public long projectID { set; get; }//实验ID
        public string devieceID { set; get; }//设备ID
        public string labTarget { set; get; }//实验目的
        public long statue { set; get; }//批次运行状态
        public string startDateTime { set; get; }//开始时间
        public string endDateTime { set; get; }//结束时间
        public string Strain
        {
            get;set;
        }
        public string Tester
        {
            get; set;
        }
    }

    public enum RD3BatchStatue
    {
        Availble = 0,//空闲
        Running =1,//运行
        Disrupt‌ = 2,//中断
        Complete = 3//完成
    }

    /// <summary>
    /// 样品 同一批次可以多个样品
    /// </summary>
    public class RD3Sample
    {
        public int ID;//唯一编号
        public long batchID { set; get; }//批次号
        //public string devieceID;//设备ID
        public string type { set; get; }//样品类别
        public string description { set; get; }//样品描述
        public string remark { set; get; }//备注
        public string collectDateTime { set; get; }//采集时间
        public string userID { set; get; }//采集人
    }

    /// <summary>
    /// 离线数据 同一样品可以多个离线数据
    /// </summary>
    public class offLineData
    {
        public int ID;
        //public string collectDateTime { set; get; }//采集时间
        //public string userID { set; get; }//采集人
        public long sampleID { set; get; }//样品编号
        public long paramterID { set; get; }//数据绑定ID
        public double value { set; get; }//值
        //public string unit;
        //public string description;
    }

    /// <summary>
    /// 数据
    /// </summary>
    public class paramterNode
    {
        public int ID;//编号
        public string name { set; get; }//名称
        public string ename { set; get; }//英文名称
        public long maxValue { set; get; }//最大值
        public long minValue { set; get; }//最小值
        public string unit { set; get; }//单位
        public int axisType { set; get; }//坐标类型 0-左侧坐标，1-左侧新加坐标，2-右侧坐标，3右侧新加坐标
        public string description { set; get; }//描述

        public string deviceID { set; get; }//设备编号

        public string colorStr  { set; get; }//颜色

        public string fieldName { set; get; }//反射字段
    }

    /// <summary>
    /// 数据值
    /// </summary>
    public class dataValue: paramterNode
    {
        public double value { set; get; }
        public int batchID {  set; get; }//批次ID
    }
    #endregion
}
