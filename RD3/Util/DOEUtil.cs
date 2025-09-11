using RD3.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XZ.SQLite;

namespace RD3
{
    public static class DOEResultUtil
    {
        public static string GetDOEResultSql(List<KeyValuePair<string, double>> conditions, string deviceId, string responseColumn)
        {
            Type type = typeof(RealTimeParam);
            List<PropertyInfo> fields = type.GetProperties().ToList().FindAll(c => c.CanRead && c.CanWrite && c.CanRead && c.PropertyType.IsValueType);
            List<string> strings = new List<string>() { $"where deviceID='{deviceId}'" };
            List<string> selectColList = new List<string>();
            foreach (var item in conditions)
            {
                PropertyInfo property = fields.Find(t => string.Equals(t.Name, item.Key, StringComparison.OrdinalIgnoreCase));
                if (property == null) continue;
                strings.Add($"and {property.Name} BETWEEN {item.Value - 0.01} AND {item.Value + 0.01} ");
                selectColList.Add(property.Name);
            }
            string strSelect = $"select {responseColumn} from {RD3SQLHelper.realTimeParamTable1}";
            string condition = string.Join(" ", strings);
            string sql = $"{strSelect} {condition} ORDER BY dateTime DESC LIMIT 1";
            return sql;
        }
    }
}
