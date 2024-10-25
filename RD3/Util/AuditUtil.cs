using RD3.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
   public class AuditUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj1">old value</param>
        /// <param name="obj2">new value</param>
        public static void AddAuditRecord(object obj1, object obj2)
        {
            if (obj1.GetType() != obj2.GetType())
            {
                LogHelper.Error("不是同一类型：Audit");
                return;
            }

            Type type = obj1.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties.Where(t=>(t.GetValue(obj1)).GetType().IsClass))
            {
                object value1 = property.GetValue(obj1);
                object value2 = property.GetValue(obj2);
                if ((value1 == null && value2 != null) || (value1 != null && value2 == null) || (value1 != null && !value1.Equals(value2)))
                {
                    PropertyInfo[] properties1 = value1.GetType().GetProperties();
                    foreach (PropertyInfo property1 in properties1)
                    {
                        object value3 = property.GetValue(obj1);
                        object value4 = property.GetValue(obj2);
                        if ((value3 == null && value4 != null) || (value3 != null && value4 == null) || (value3 != null && !value3.Equals(value4)))
                        {
                            Operation operation = new Operation();
                            operation.OccurrenceTime = DateTime.Now;
                            operation.Batch = AppSession.CurrentBatch?.Name;
                            operation.Reactor = AppSession.CurrentBatch?.Reactor;
                            operation.Description = string.Format("Changed from {0} to {1}", value1, value2);
                            operation.OperationStatement = "Modify control mode";
                        }
                    }
                }
            }
        }
    }
}
