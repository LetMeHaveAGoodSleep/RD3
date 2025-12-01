using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NotDBColumnAttribute : Attribute
    {
        // 可选：添加特性的描述信息
        public string Reason { get; }

        // 构造函数：支持传入标记原因
        public NotDBColumnAttribute(string reason = "该属性不作为数据库字段")
        {
            Reason = reason;
        }
    }
}
